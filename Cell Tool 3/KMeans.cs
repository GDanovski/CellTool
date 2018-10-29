/*
 CellTool - software for bio-image analysis
 Copyright (C) 2018  Georgi Danovski

 This program is free software: you can redistribute it and/or modify
 it under the terms of the GNU General Public License as published by
 the Free Software Foundation, either version 3 of the License, or
 (at your option) any later version.

 This program is distributed in the hope that it will be useful,
 but WITHOUT ANY WARRANTY; without even the implied warranty of
 MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 GNU General Public License for more details.

 You should have received a copy of the GNU General Public License
 along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Drawing;
using System.ComponentModel;

namespace Cell_Tool_3
{
    class KMeansSegmentation
    {
        /*
         * The code was based on the K-means algorithm developed by:
         * Arif_Khan
         * 11 Aug 2009
         * Australia  Australia 
         * https://www.codeproject.com/Articles/38888/Computer-Vision-Applications-with-C-Part-IV
         * My Website: http://www.puresolutions-online.com
         */

        ImageAnalyser IA;
        public KMeansSegmentation(ImageAnalyser IA)
        {
            this.IA = IA;
        }
        public void Start(int NumberOfColors)
        {
            TifFileInfo fi = FindFI(IA);

            if (fi == null)
            {
                System.Windows.Forms.MessageBox.Show("Image not avaliable!");
                return;
            }

            fi.thresholds[fi.cValue] = NumberOfColors - 1;

            if (NumberOfColors < 2)
            {
                IA.ReloadImages();
                return;
            }

            fi.available = false;
            //create arrays in fi
            fi.thresholdValues[fi.cValue] = new int[5];
            Color[] refCol = fi.RefThresholdColors[fi.cValue];
            switch (fi.thresholdsCBoxIndex[fi.cValue])
            {
                case 1:
                    fi.thresholdColors[fi.cValue] = new Color[]
                    {refCol[0],refCol[1],Color.Transparent,Color.Transparent,Color.Transparent};
                    break;
                case 2:
                    fi.thresholdColors[fi.cValue] = new Color[]
                    {refCol[0],refCol[1],refCol[2],Color.Transparent,Color.Transparent};
                    break;
                case 3:
                    fi.thresholdColors[fi.cValue] = new Color[]
                    {refCol[0],refCol[1],refCol[2],refCol[3],Color.Transparent};
                    break;
                case 4:
                    fi.thresholdColors[fi.cValue] = new Color[]
                    {refCol[0],refCol[1],refCol[2],refCol[3],refCol[4]};
                    break;
                default:
                    fi.thresholdColors[fi.cValue] = new Color[]
                    {Color.Transparent,Color.Transparent,Color.Transparent,Color.Transparent,Color.Transparent};
                    break;
            }
            //create variables

            previousCluster = new Dictionary<int, Cluster>();
            currentCluster = new Dictionary<int, Cluster>();
            switch (fi.bitsPerPixel)
            {
                case 8:
                    Kmeans8bit(fi, NumberOfColors);
                    break;
                case 16:
                    Kmeans16bit(fi, NumberOfColors);
                    break;
            }
        }
        private TifFileInfo FindFI(ImageAnalyser IA)
        {
            TifFileInfo fi = null;
            try
            {
                fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
            }
            catch { return null; }

            if (fi == null) { return null; }
            if (fi.available == false) return null;

            return fi;
        }
        private void Kmeans8bit(TifFileInfo fi, int NumberOfColors)
        {
            int count = 0;
            var bgw = new BackgroundWorker();
            bgw.WorkerReportsProgress = true;
            //Add event for projection here
            bgw.DoWork += new DoWorkEventHandler(delegate (Object o, DoWorkEventArgs a)
            {
                //Check is processed image enabled
                if (fi.image8bitFilter == null)
                {
                    fi.image8bitFilter = fi.image8bit;
                }
                //Calculate active frame
                FrameCalculator FC = new FrameCalculator();
                int frame = FC.FrameC(fi, fi.cValue);
                //find active image data
                byte[][] image = fi.image8bitFilter[frame];
                //find first centroids
                findFirstCentroids(image, NumberOfColors);
                for (int i = 0; i < topColours.Length; i++)
                {
                    previousCluster.Add(topColours[i], new Cluster((float)topColours[i]));
                    currentCluster.Add(topColours[i], new Cluster((float)topColours[i]));
                }

                BuildHistogram(fi, frame);
                ((BackgroundWorker)o).ReportProgress(1);
                converged = false;
               
                while (!converged)
                {
                    Iterate();

                    for (int i = 0, j = 1; j < NumberOfColors; i++, j++)
                        fi.thresholdValues[fi.cValue][j] = (int)Thresholds[i];

                    count++;
                    ((BackgroundWorker)o).ReportProgress(1);
                }
                ((BackgroundWorker)o).ReportProgress(0);
            });

            bgw.ProgressChanged += new ProgressChangedEventHandler(delegate (Object o, ProgressChangedEventArgs a)
            {
                if (a.ProgressPercentage == 0)
                {
                    fi.available = true;
                    IA.FileBrowser.StatusLabel.Text = "Ready";
                    ClearData();
                    IA.ReloadImages();
                }
                else
                {
                    IA.FileBrowser.StatusLabel.Text = "K-means: " + count.ToString() + " iterations...";
                }

            });
            //Start background worker
            IA.FileBrowser.StatusLabel.Text = "K-means: Building Histogram...";
            //start bgw
            bgw.RunWorkerAsync();
        }
        private void Kmeans16bit(TifFileInfo fi, int NumberOfColors)
        {
            int count = 0;
            var bgw = new BackgroundWorker();
            bgw.WorkerReportsProgress = true;
            //Add event for projection here
            bgw.DoWork += new DoWorkEventHandler(delegate (Object o, DoWorkEventArgs a)
            {
                //Check is processed image enabled
                if (fi.image16bitFilter == null)
                {
                    fi.image16bitFilter = fi.image16bit;
                }
                //Calculate active frame
                FrameCalculator FC = new FrameCalculator();
                int frame = FC.FrameC(fi, fi.cValue);
                //find active image data
                ushort[][] image = fi.image16bitFilter[frame];
                //find first centroids
                findFirstCentroids(image, NumberOfColors);
                for (int i = 0; i < topColours.Length; i++)
                {
                    previousCluster.Add(topColours[i], new Cluster((float)topColours[i]));
                    currentCluster.Add(topColours[i], new Cluster((float)topColours[i]));
                }

                BuildHistogram(fi, frame);

                ((BackgroundWorker)o).ReportProgress(1);

                converged = false;
                
                while (!converged)
                {
                    Iterate();

                    for (int i = 0, j = 1; j < NumberOfColors; i++, j++)
                        fi.thresholdValues[fi.cValue][j] = (int)Thresholds[i];

                    count++;
                    ((BackgroundWorker)o).ReportProgress(1);
                }
                
                ((BackgroundWorker)o).ReportProgress(0);
            });

            bgw.ProgressChanged += new ProgressChangedEventHandler(delegate (Object o, ProgressChangedEventArgs a)
            {
                if (a.ProgressPercentage == 0)
                {
                    fi.available = true;
                    IA.FileBrowser.StatusLabel.Text = "Ready";
                    ClearData();
                    IA.ReloadImages();
                }
                else
                {
                    IA.FileBrowser.StatusLabel.Text = "K-means: " + count.ToString() + " iterations...";
                }

               
            });
            //Start background worker
            IA.FileBrowser.StatusLabel.Text = "K-means: Building Histogram...";
            //start bgw
            bgw.RunWorkerAsync();
        }
        #region Find Top Colors
        private void findFirstCentroids(ushort[][] image, int NumberOfColors)
        {
            Dictionary<int, long> colours = new Dictionary<int, long>();

            foreach (ushort[] row in image)
                foreach (ushort val in row)
                    if (colours.ContainsKey((int)val))
                    {
                        colours[(int)val] += 1;
                    }
                    else
                        colours.Add((int)val, (long)1);

            findFirstCentroids(colours, NumberOfColors);
        }
        private void findFirstCentroids(byte[][] image, int NumberOfColors)
        {
            Dictionary<int, long> colours = new Dictionary<int, long>();

            foreach (byte[] row in image)
                foreach (byte val in row)
                    if (colours.ContainsKey((int)val))
                    {
                        colours[(int)val] += 1;
                    }
                    else
                        colours.Add((int)val, (long)1);

            findFirstCentroids(colours, NumberOfColors);
        }
        private void findFirstCentroids(Dictionary<int, long> colours, int NumberOfColors)
        {
            if (NumberOfColors > colours.Count) NumberOfColors = colours.Count;

            topColours = new int[NumberOfColors];

            List<KeyValuePair<int, long>> summaryList = new List<KeyValuePair<int, long>>();
            summaryList.AddRange(colours);

            summaryList.Sort(delegate (KeyValuePair<int, long> kvp1, KeyValuePair<int, long> kvp2)
            { return Comparer<long>.Default.Compare(kvp2.Value, kvp1.Value); });

            for (int i = 0; i < topColours.Length; i++)
            {
                topColours[i] = summaryList[i].Key;
            }

            summaryList = null;
            colours = null;
        }
        #endregion Find Top Colors

        #region Calculate Histogram
        private void BuildHistogram(TifFileInfo fi, int frame)
        {
            switch (fi.bitsPerPixel)
            {
                case 8:
                    calculate8bitHistogram(fi,frame);
                    break;
                case 16:
                    calculate16bitHistogram(fi,frame);
                    break;
            }

            RecalculateHistogramValues();
        }
        private void calculate8bitHistogram(TifFileInfo fi, int frame)
        {
            Histogram = new Dictionary<ushort, HistogramUnit>();

            if (fi.sumHistogramChecked[fi.cValue] == false)
            {
                foreach (byte[] row in fi.image8bitFilter[frame])
                    foreach (byte val in row)
                        if (Histogram.ContainsKey((ushort)val))
                        {
                            Histogram[(ushort)val].Counter += 1;
                        }
                        else
                            Histogram.Add((ushort)val, new HistogramUnit());
            }
            else
            {
                //histogram of all images
                int[] input = new int[(int)(fi.imageCount / fi.sizeC)];
                Dictionary<ushort, int>[] hList = new Dictionary<ushort, int>[input.Length];
                //check wich frames are from selected color
                for (int i = 0, val = fi.cValue; i < input.Length; i++, val += fi.sizeC)
                    input[i] = val;

                //calculate histograms for all images
                Parallel.For(0, input.Length, (i) =>
                {
                    Dictionary<ushort,int> dic = new Dictionary<ushort, int>();

                    foreach (byte[] row in fi.image8bitFilter[i])
                        foreach (byte val in row)
                            if (dic.ContainsKey((ushort)val))
                            {
                                dic[(ushort)val] += 1;
                            }
                            else
                               dic.Add((ushort)val, (int)1);

                    hList[i] = dic;
                });

                foreach (Dictionary<ushort, int> dic in hList)
                    foreach(KeyValuePair<ushort, int> node in dic)
                        if (Histogram.ContainsKey(node.Key))
                        {
                            Histogram[node.Key].Counter += node.Value;
                        }
                        else
                            Histogram.Add(node.Key,new HistogramUnit());
                
                hList = null;
            }
        }
        private void calculate16bitHistogram(TifFileInfo fi, int frame)
        {
            Histogram = new Dictionary<ushort, HistogramUnit>();

            if (fi.sumHistogramChecked[fi.cValue] == false)
            {
                foreach (ushort[] row in fi.image16bitFilter[frame])
                    foreach (ushort val in row)
                        if (Histogram.ContainsKey(val))
                        {
                            Histogram[val].Counter += 1;
                        }
                        else
                            Histogram.Add(val, new HistogramUnit());
            }
            else
            {
                //histogram of all images
                int[] input = new int[(int)(fi.imageCount / fi.sizeC)];
                Dictionary<ushort, int>[] hList = new Dictionary<ushort, int>[input.Length];
                //check wich frames are from selected color
                for (int i = 0, val = fi.cValue; i < input.Length; i++, val += fi.sizeC)
                    input[i] = val;

                //calculate histograms for all images
                Parallel.For(0, input.Length, (i) =>
                {
                    Dictionary<ushort, int> dic = new Dictionary<ushort, int>();

                    foreach (ushort[] row in fi.image16bitFilter[i])
                        foreach (ushort val in row)
                            if (dic.ContainsKey(val))
                            {
                                dic[val] += 1;
                            }
                            else
                                dic.Add(val, (int)1);

                    hList[i] = dic;
                });

                foreach (Dictionary<ushort, int> dic in hList)
                    foreach (KeyValuePair<ushort, int> node in dic)
                        if (Histogram.ContainsKey(node.Key))
                        {
                            Histogram[node.Key].Counter += node.Value;
                        }
                        else
                            Histogram.Add(node.Key, new HistogramUnit());

                hList = null;
            }
        }
        private void RecalculateHistogramValues()
        {
            foreach (KeyValuePair<ushort, HistogramUnit> node in Histogram)
            {
                node.Value.Sum = (long)(node.Key * node.Value.Counter);
            }
        }
        #endregion Calculate Histogram

        #region Iterations
        private void Iterate()
        {
            float d, curD;
            KeyValuePair<int, Cluster> closestCl = new KeyValuePair<int, Cluster>();
            //find closest cluster for each histogram unit and apply info
            foreach (KeyValuePair<ushort, HistogramUnit> node in Histogram)
            {
                d = float.MaxValue;
                foreach (KeyValuePair<int, Cluster> c in currentCluster)
                {
                    curD = Math.Abs(c.Value.Centroid - (float)node.Key);
                    if (curD < d)
                    {
                        d = curD;
                        closestCl = c;
                    }
                }

                closestCl.Value.Counter += node.Value.Counter;
                closestCl.Value.Sum += node.Value.Sum;

                if (closestCl.Value.Min > node.Key) closestCl.Value.Min = node.Key;
                if (closestCl.Value.Max < node.Key) closestCl.Value.Max = node.Key;
            }
            //calculate the new centroids
            foreach (KeyValuePair<int, Cluster> c in currentCluster)
            {
                c.Value.Centroid = c.Value.Sum / c.Value.Counter;
            }
            //Calculate new thresholds
            clusterArr.Clear();
            for (int i = 0; i < currentCluster.Count; i++)
            {
                Cluster tempClust = null;
                foreach (KeyValuePair<int, Cluster> cluster in currentCluster)
                    if (clusterArr.IndexOf(cluster.Value) == -1)
                    {
                        if (tempClust == null)
                        {
                            tempClust = cluster.Value;
                        }
                        else if (tempClust.Min > cluster.Value.Min)
                        {
                            tempClust = cluster.Value;
                        }
                    }
                if (tempClust != null) clusterArr.Add(tempClust);
            }

            Thresholds.Clear();

            for (int i = 0, j = 1; j < clusterArr.Count; i++, j++)
            {
                Thresholds.Add((int)(clusterArr[i].Max + ((clusterArr[j].Min - clusterArr[i].Max) / 2)));
            }

            Thresholds.Add(int.MaxValue);

            clusterArr.Clear();

            //Clear;
            foreach (KeyValuePair<int, Cluster> c in currentCluster)
            {
                c.Value.Min = float.MaxValue;
                c.Value.Max = float.MinValue;
                c.Value.Counter = 1;
                c.Value.Sum = (long)c.Value.Centroid;
            }

            CheckConvergence();
            
        }
        private void CheckConvergence()
        {
            //if current and previous cluster centroids are the same then converged
            bool match = true;
            foreach (KeyValuePair<int, Cluster> cluster in currentCluster)
            {
                if ((int)cluster.Value.Centroid != (int)previousCluster[cluster.Key].Centroid)
                {
                    match = false;
                    break;
                }
            }

            if (!match)
            {
                foreach (KeyValuePair<int, Cluster> cluster in currentCluster)
                {
                    previousCluster[cluster.Key].Centroid = cluster.Value.Centroid;
                }
            }
            converged = match;
        }
        #endregion Iterations

        private void ClearData()
        {
            topColours = null;
            previousCluster = null;
            currentCluster = null;
            Histogram = null;
            Thresholds.Clear();
            clusterArr.Clear();
            converged = false;
        }

        private class Cluster
        {
            public Cluster(float Val)
            {
                Centroid = Val;
                Min = float.MaxValue;
                Max = float.MinValue;
                Counter = 1;
                Sum = (long)Centroid;
            }

            public float Centroid;//The value of the centroid intensity
            public float Min;//The element with min intensity
            public float Max;//The element with max intensity
            public int Counter;//the total number of elements in the cluster
            public long Sum;//The sum intensity of all elements in the cluster
        }
        private class HistogramUnit
        {
            public int Counter=1;//the total number of elements in the cluster
            public long Sum=0;//The sum intensity of all elements in the cluster
        }

        private int[] topColours;
        private Dictionary<int, Cluster> previousCluster;
        private Dictionary<int, Cluster> currentCluster;
        Dictionary<ushort, HistogramUnit> Histogram;
        List<Cluster> clusterArr = new List<Cluster>();
        private List<int> Thresholds = new List<int>();
        private bool converged = false;

    }
}
