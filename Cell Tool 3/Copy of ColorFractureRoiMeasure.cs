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
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Drawing;
namespace Cell_Tool_3
{
    class ColorFractureRoiMeasure
    {
        ImageAnalyser IA;
        Color selectedCol = Color.Empty;
        Color lastCol = Color.Empty;
        public ColorFractureRoiMeasure(ImageAnalyser IA)
        {
            this.IA = IA;
        }

        public void ExportAllResults(object sender, EventArgs e)
        {
            
            TifFileInfo fi = null;
            try
            {
                fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
            }
            catch { return; }
            if (fi == null) return;

            if (!fi.available)
            {
                MessageBox.Show("Image is not avaliable!\nTry again later.");
                return;
            }
            int c = fi.cValue;
            #region Choose color
            lastCol = fi.thresholdColors[c][fi.thresholds[c]];
            List<Color> colorL = new List<Color>();
            for (int i = 0; i <= fi.thresholds[c]; i++)
                if (fi.thresholdColors[c][i] != Color.Transparent &&
                    colorL.IndexOf(fi.thresholdColors[c][i]) == -1)
                    colorL.Add(fi.thresholdColors[c][i]);
            if (fi.SelectedSpotThresh[c] != 0 && colorL.IndexOf(fi.SpotColor[c]) == -1)
                colorL.Add(fi.SpotColor[c]);

            if (colorL.Count == 0)
            {
                MessageBox.Show("The image must be segmented!");
                return;
            }

            Form binaryForm = BinaryDialog(colorL);

            // Linux change
            IA.FileBrowser.StatusLabel.Text = "Dialog open";
            binaryForm.ShowDialog();
            IA.FileBrowser.StatusLabel.Text = "Ready";

            selectedCol = (Color)binaryForm.Tag;

            binaryForm.Dispose();

            if (selectedCol == Color.Empty)
            {
                MessageBox.Show("Foreground is not selected!");
                return;
            }

            #endregion Chose color

            TreeNode node = IA.TabPages.Collections[IA.TabPages.SelectedIndex][0].Tag as TreeNode;

            string dir = node.Tag.ToString().Replace(".tif", "");

            //background worker

            //var bgw = new BackgroundWorker();
            //bgw.WorkerReportsProgress = true;
            fi.available = false;
            //Add event for projection here
            //bgw.DoWork += new DoWorkEventHandler(delegate (Object o, DoWorkEventArgs a)
            {
                //for (int c = 0; c < fi.sizeC; c++)
                if (fi.roiList[c] != null && fi.roiList[c].Count != 0)
                {
                    
                    string dir1 = dir + "_Ch" + c + "_" +
                    fi.LutList[c].ToString().Replace("Color [", "").Replace("]", "") +
                    ".txt";
                    string dir2 = dir + "_Ch" + c + "_" +
                    fi.LutList[c].ToString().Replace("Color [", "").Replace("]", "") +
                    "_Results.txt";
                    //calculate the size of the result row
                    int resultSize = 0;
                    foreach (ROI roi in fi.roiList[c])
                        if (roi.Checked == true)
                        {
                           
                            Measure(roi, fi, c, IA);

                            if (roi.Shape == 0 || roi.Shape == 1)
                                resultSize += roi.Results[c].Length;
                            else if (roi.Shape == 2 || roi.Shape == 3 || roi.Shape == 4 || roi.Shape == 5)
                                resultSize += 4 + roi.Stack * 4;

                        }
                    //chart result
                    
                    string val = IA.chart.GetResults(fi, c);
                    if (val != "")
                    {
                        try
                        {
                            File.WriteAllText(dir1, val);
                        }
                        catch
                        {
                            //((BackgroundWorker)o).ReportProgress(1);
                            MessageBox.Show("File is used by other program!");
                        }
                    }
                    
                    //standart results
                    if (resultSize == 0)
                    {
                        //((BackgroundWorker)o).ReportProgress(0);
                        foreach (ROI roi in fi.roiList[c])
                            if (roi.Checked == true)
                            {
                                RoiMeasure.Measure(roi, fi, c, IA);
                            }

                        IA.FileBrowser.Refresh_AfterSave();
                        fi.available = true;
                        IA.FileBrowser.StatusLabel.Text = "Ready";
                        IA.ReloadImages();
                        return;
                    }

                    {
                        //create result matrix
                        double[] result;

                        int t = 1;
                        int z = 1;
                        int position;
                        string str;

                        double time = 0;
                        int timeIndex = 0;
                        double timeT = fi.TimeSteps[timeIndex];
                        try
                        {
                            if (File.Exists(dir2))
                                File.Delete(dir2);
                        }
                        catch
                        {
                            //((BackgroundWorker)o).ReportProgress(1);
                            MessageBox.Show("File is used by other program!");
                            return;
                        }

                        using (StreamWriter write = new StreamWriter(dir2))
                        {
                            //titles part
                            List<string> titles = new List<string>();
                            titles.Add("ImageN");
                            if (fi.sizeT > 1) titles.Add("T");
                            if (fi.sizeT > 1) titles.Add("T(sec.)");
                            if (fi.sizeZ > 1) titles.Add("Z");

                            int roiN = 1;
                            foreach (ROI roi in fi.roiList[c])
                            {
                                if (roi.Checked == true && roi.Results[c] != null)
                                {
                                    string com = "";
                                    if (roi.Comment != "") com = ": " + roi.Comment;

                                    titles.Add("Area" + roiN.ToString() + com);
                                    titles.Add("Mean" + roiN.ToString() + com);
                                    titles.Add("Min" + roiN.ToString() + com);
                                    titles.Add("Max" + roiN.ToString() + com);
                                    if (roi.Stack > 0)
                                        if (roi.Shape == 0 || roi.Shape == 1)
                                            for (int n = 1; n <= roi.Stack; n++)
                                            {
                                                titles.Add("Area" + roiN.ToString() + "." + n.ToString() + ".LeftUp" + com);
                                                titles.Add("Mean" + roiN.ToString() + "." + n.ToString() + ".LeftUp" + com);
                                                titles.Add("Min" + roiN.ToString() + "." + n.ToString() + ".LeftUp" + com);
                                                titles.Add("Max" + roiN.ToString() + "." + n.ToString() + ".LeftUp" + com);

                                                titles.Add("Area" + roiN.ToString() + "." + n.ToString() + ".RightUp" + com);
                                                titles.Add("Mean" + roiN.ToString() + "." + n.ToString() + ".RightUp" + com);
                                                titles.Add("Min" + roiN.ToString() + "." + n.ToString() + ".RightUp" + com);
                                                titles.Add("Max" + roiN.ToString() + "." + n.ToString() + ".RightUp" + com);

                                                titles.Add("Area" + roiN.ToString() + "." + n.ToString() + ".LeftDown" + com);
                                                titles.Add("Mean" + roiN.ToString() + "." + n.ToString() + ".LeftDown" + com);
                                                titles.Add("Min" + roiN.ToString() + "." + n.ToString() + ".LeftDown" + com);
                                                titles.Add("Max" + roiN.ToString() + "." + n.ToString() + ".LeftDown" + com);

                                                titles.Add("Area" + roiN.ToString() + "." + n.ToString() + ".RightDown" + com);
                                                titles.Add("Mean" + roiN.ToString() + "." + n.ToString() + ".RightDown" + com);
                                                titles.Add("Min" + roiN.ToString() + "." + n.ToString() + ".RightDown" + com);
                                                titles.Add("Max" + roiN.ToString() + "." + n.ToString() + ".RightDown" + com);

                                            }
                                        else if (roi.Shape == 2 || roi.Shape == 3 || roi.Shape == 4 || roi.Shape == 5)
                                            for (int n = 1; n <= roi.Stack; n++)
                                            {
                                                titles.Add("Area" + roiN.ToString() + "." + n.ToString() + com);
                                                titles.Add("Mean" + roiN.ToString() + "." + n.ToString() + com);
                                                titles.Add("Min" + roiN.ToString() + "." + n.ToString() + com);
                                                titles.Add("Max" + roiN.ToString() + "." + n.ToString() + com);
                                            }
                                }
                                roiN++;
                            }
                            write.WriteLine(string.Join("\t", titles));
                            //calculations
                            for (int i = c; i < fi.imageCount; i += fi.sizeC)
                            {
                                //extract row from rois
                                position = 0;
                                result = new double[resultSize];
                                foreach (ROI roi in fi.roiList[c])
                                {
                                    if (roi.Checked == true)
                                    {
                                        if (roi.Shape == 0 || roi.Shape == 1)
                                        {
                                            if (roi.Results[i] != null
                                        && roi.FromT <= t && roi.ToT >= t
                                        && roi.FromZ <= z && roi.ToZ >= z)
                                            {
                                                Array.Copy(roi.Results[i], 0, result, position, roi.Results[i].Length);
                                            }

                                            position += roi.Results[c].Length;
                                        }
                                        else if (roi.Shape == 2 || roi.Shape == 3 || roi.Shape == 4 || roi.Shape == 5)
                                        {
                                            if (roi.Results[i] != null
                                        && roi.FromT <= t && roi.ToT >= t
                                        && roi.FromZ <= z && roi.ToZ >= z)
                                            {
                                                //main roi
                                                Array.Copy(roi.Results[i], 0, result, position, 4);
                                                position += 4;
                                                //layers
                                                for (int p = 4; p < roi.Results[i].Length; p += 16)
                                                {
                                                    Array.Copy(roi.Results[i], p, result, position, 4);
                                                    position += 4;
                                                }
                                            }
                                            else
                                            {
                                                position += 4;
                                                //layers
                                                for (int p = 4; p < roi.Results[i].Length; p += 16)
                                                {
                                                    position += 4;
                                                }
                                            }
                                        }
                                    }
                                }
                                //write the line
                                //if (CheckArrayForValues(result))
                                {
                                    str = string.Join("\t", result);

                                    if (fi.sizeZ > 1) str = z.ToString() + "\t" + str;
                                    if (fi.sizeT > 1)
                                    {
                                        str = t.ToString() + "\t" + time.ToString() + "\t" + str;
                                    }
                                    str = i.ToString() + "\t" + str;
                                    write.WriteLine(str);
                                }
                                //recalculate z and t

                                z += 1;
                                if (z > fi.sizeZ)
                                {
                                    z = 1;
                                    t += 1;

                                    if (t > fi.sizeT)
                                    {
                                        t = 1;
                                    }

                                    if (t <= timeT)
                                    {
                                        time += fi.TimeSteps[timeIndex + 1];
                                    }
                                    else
                                    {
                                        timeIndex += 2;

                                        if (timeIndex < fi.TimeSteps.Count)
                                            timeT += fi.TimeSteps[timeIndex];
                                        else
                                        {
                                            timeIndex -= 2;
                                            timeT += fi.imageCount;
                                        }

                                        time += fi.TimeSteps[timeIndex + 1];
                                    }

                                }
                            }
                        }
                    }

                   
                }

                //((BackgroundWorker)o).ReportProgress(0);
                }//);
            
            //bgw.ProgressChanged += new ProgressChangedEventHandler(delegate (Object o, ProgressChangedEventArgs a)
            {
                //if (a.ProgressPercentage == 0)
                {
                    foreach (ROI roi in fi.roiList[c])
                        if (roi.Checked == true)
                        {
                            RoiMeasure.Measure(roi, fi, c, IA);
                        }

                    IA.FileBrowser.Refresh_AfterSave();
                    fi.available = true;
                    IA.FileBrowser.StatusLabel.Text = "Ready";
                    IA.ReloadImages();
                }
                //else
                {
                   // MessageBox.Show("File is used by other program!");
                }
            }//);
            //Start background worker
           // IA.FileBrowser.StatusLabel.Text = "Saving results...";
            //start bgw
           // bgw.RunWorkerAsync();

            fi.available = true;
            IA.FileBrowser.StatusLabel.Text = "Ready";
        }
        private bool CheckColor(TifFileInfo fi, byte val, int[] SpotDiapason
               , int[] thresholdValues, Color[] thresholdColors, int thresholds, Color SpotColor)
        {
            Color col = Color.Transparent;

            switch (thresholds)
            {
                case 0:
                    if (val > SpotDiapason[0] & val < SpotDiapason[1])
                        col = SpotColor;
                    break;
                default:
                    if (val > SpotDiapason[0] & val < SpotDiapason[1])
                        col = SpotColor;
                    else if (val < thresholdValues[1])
                        col = thresholdColors[0];
                    else if (val < thresholdValues[2])
                        col = thresholdColors[1];
                    else if (val < thresholdValues[3])
                        col = thresholdColors[2];
                    else if (val < thresholdValues[4])
                        col = thresholdColors[3];
                    else
                        col =lastCol;
                    break;
            }
            if (col == selectedCol)
                return true;
            else
                return false;
        }
        
        private bool CheckColor(TifFileInfo fi, ushort val, int[] SpotDiapason
               , int[] thresholdValues, Color[] thresholdColors, int thresholds, Color SpotColor)
        {
            Color col = Color.Transparent;

            switch (thresholds)
            {
                case 0:
                    if (val > SpotDiapason[0] & val < SpotDiapason[1])
                        col = SpotColor;
                    break;
                default:
                    if (val > SpotDiapason[0] & val < SpotDiapason[1])
                        col = SpotColor;
                    else if (val < thresholdValues[1])
                        col = thresholdColors[0];
                    else if (val < thresholdValues[2])
                        col = thresholdColors[1];
                    else if (val < thresholdValues[3])
                        col = thresholdColors[2];
                    else if (val < thresholdValues[4])
                        col = thresholdColors[3];
                    else
                        col = lastCol;
                    break;
            }
            if (col == selectedCol)
                return true;
            else
                return false;
        }
        private Form BinaryDialog(List<Color> colorL)
        {
            Form Dialog = new Form();
            Dialog.Tag = Color.Empty;
            Dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
            Dialog.Text = "Export Color";
            Dialog.StartPosition = FormStartPosition.CenterScreen;
            Dialog.WindowState = FormWindowState.Normal;
            Dialog.MinimizeBox = false;
            Dialog.MaximizeBox = false;
            Dialog.BackColor = IA.FileBrowser.BackGround2Color1;
            Dialog.ForeColor = IA.FileBrowser.ShriftColor1;

            Dialog.FormClosing += new FormClosingEventHandler(delegate (object o, FormClosingEventArgs a)
            {
                Dialog.Visible = false;
                a.Cancel = true;
            });

            Dialog.SuspendLayout();

            Label ThresholdLab = new Label();
            ThresholdLab.Text = "Chose Foreground:";
            ThresholdLab.Width = 100;
            ThresholdLab.Location = new System.Drawing.Point(10, 15);
            Dialog.Controls.Add(ThresholdLab);

            //buttons
            int W = 10;
            foreach (Color col in colorL)
            {
                Button okBtn = new Button();
                okBtn.Width = 30;
                okBtn.Height = 30;
                okBtn.FlatStyle = FlatStyle.Flat;
                okBtn.FlatAppearance.BorderSize = 0;
                okBtn.Text = "";
                okBtn.BackColor = col;
                okBtn.Location = new System.Drawing.Point(W, 40);
                Dialog.Controls.Add(okBtn);

                okBtn.Click += new EventHandler(delegate (object sender, EventArgs e)
                {
                    Dialog.Tag = okBtn.BackColor;
                    Dialog.Visible = false;
                });
                W += 40;
            }

            Dialog.KeyPreview = true;
            Dialog.KeyDown += new KeyEventHandler(delegate (object sender, KeyEventArgs e)
            {
                switch (e.KeyCode)
                {
                    case Keys.Escape:
                        Dialog.Visible = false;
                        break;
                    default:
                        break;
                }
            });

            Dialog.ResumeLayout();

            Dialog.Height = 120;
            Dialog.Width = W + 15;
            if (Dialog.Width < 100)
                Dialog.Width = 100;

            return Dialog;
        }
        private bool CheckArrayForValues(double[] input)
        {
            if (!IA.DeleteEmptyEnabled) return true;

            foreach (double val in input)
                if (val != 0)
                    return true;

            return false;
        }
        #region Measure

        private void Measure(ROI roi, TifFileInfo fi, int cVal, ImageAnalyser IA)
        {
            
                //return;
                if (fi.loaded == false || fi.roiList == null || fi.roiList[cVal] == null ||
                    (!(fi.roiList[cVal].IndexOf(roi) > -1))) return;
                //IA.FileBrowser.StatusLabel.Text = "Measuring ROI...";
                if (roi.Type == 0)
                    switch (roi.Shape)
                    {
                        case 0:
                            GetPointsInRectangleStatic(roi, fi, cVal);
                            break;
                        case 1:
                            GetPointsInOvalStatic(roi, fi, cVal);
                            break;
                        case 2:
                            GetPointsInPolygonStatic(roi, fi, cVal);
                            break;
                        case 3:
                            GetPointsInPolygonStatic(roi, fi, cVal);
                            break;
                        case 4:
                            GetPointsInPolygonStatic(roi, fi, cVal);
                            break;
                        case 5:
                            GetPointsInPolygonStatic(roi, fi, cVal);
                            break;
                    }
                else if (roi.Type == 1)
                    switch (roi.Shape)
                    {
                        case 0:
                            GetPointsInRectangleTracking(roi, fi, cVal);
                            break;
                        case 1:
                            GetPointsInOvalTracking(roi, fi, cVal);
                            break;
                        case 2:
                            GetPointsInPolygonTracking(roi, fi, cVal);
                            break;
                        case 3:
                            GetPointsInPolygonTracking(roi, fi, cVal);
                            break;
                        case 4:
                            GetPointsInPolygonTracking(roi, fi, cVal);
                            break;
                        case 5:
                            GetPointsInPolygonTracking(roi, fi, cVal);
                            break;
                    }
                
           

            //IA.FileBrowser.StatusLabel.Text = "Ready";
        }
        #region rectangles
        private void GetPointsInRectangleStatic(ROI roi, TifFileInfo fi, int cVal)
        {
            //get the location of the first value
            Point p = roi.GetLocation(cVal)[0];
            int X = p.X;
            int Y = p.Y;
            int W = roi.Width;
            int H = roi.Height;
            //create shablon for preventing retacking the same value
            bool[,] shablon = new bool[fi.sizeY, fi.sizeX];
            //take the points for the main roi
            List<Point> mainRoi;
            //calculates the stack
            List<Point>[] stackRoi = new List<Point>[roi.Stack];
            int rowSize = 0;

            if (roi.D >= 0)
            {
                mainRoi = CalculateRectangle(shablon, X, Y, W, H);
                rowSize = 4;

                for (int i = 0; i < roi.Stack; i++)
                {
                    X -= roi.D;
                    Y -= roi.D;
                    W += roi.D + roi.D;
                    H += roi.D + roi.D;

                    stackRoi[i] = CalculateRectangle(shablon, X, Y, W, H);
                    rowSize += 16;
                }
            }
            else
            {
                X -= roi.D * roi.Stack;
                Y -= roi.D * roi.Stack;
                W += (roi.D + roi.D) * roi.Stack;
                H += (roi.D + roi.D) * roi.Stack;

                mainRoi = CalculateRectangle(shablon, X, Y, W, H);
                rowSize = 4;

                for (int i = 0; i < roi.Stack; i++)
                {
                    X += roi.D;
                    Y += roi.D;
                    W -= roi.D + roi.D;
                    H -= roi.D + roi.D;

                    stackRoi[i] = CalculateRectangle(shablon, X, Y, W, H);
                    rowSize += 16;
                }

            }

            shablon = null;
            //measure frames
            roi.Results = new double[fi.imageCount][];

            Parallel.ForEach(SteppedRange(cVal, fi.imageCount, fi.sizeC), imageN =>
            {
                
                    double[] res = new double[rowSize];
                    int position = 0;

                    double[] tempRes = CalculateMainResults(mainRoi, fi, imageN);

                    Array.Copy(tempRes, 0, res, position, tempRes.Length);
                    position += tempRes.Length;

                    Point midP = new Point(p.X + roi.Width / 2, p.Y + roi.Height / 2);

                    for (int i = 0; i < roi.Stack; i++)
                    {
                        tempRes = CalculateStackResults(stackRoi[i], midP, fi, imageN);
                        Array.Copy(tempRes, 0, res, position, tempRes.Length);
                        position += tempRes.Length;
                    }

                    roi.Results[imageN] = res;
                
            });
        }
        private void GetPointsInRectangleTracking(ROI roi, TifFileInfo fi, int cVal)
        {
            //get the location of the first value
            Point p = roi.GetLocation(cVal)[0];
            int X = p.X;
            int Y = p.Y;
            int W = roi.Width;
            int H = roi.Height;
            //create shablon for preventing retacking the same value
            bool[,] shablon = new bool[fi.sizeY, fi.sizeX];
            //take the points for the main roi
            List<Point> mainRoi;
            //calculates the stack
            List<Point>[] stackRoi = new List<Point>[roi.Stack];
            int rowSize = 0;

            if (roi.D >= 0)
            {
                mainRoi = CalculateRectangle(shablon, X, Y, W, H);
                rowSize = 4;

                for (int i = 0; i < roi.Stack; i++)
                {
                    X -= roi.D;
                    Y -= roi.D;
                    W += roi.D + roi.D;
                    H += roi.D + roi.D;

                    stackRoi[i] = CalculateRectangle(shablon, X, Y, W, H);
                    rowSize += 16;
                }
            }
            else
            {
                X -= roi.D * roi.Stack;
                Y -= roi.D * roi.Stack;
                W += (roi.D + roi.D) * roi.Stack;
                H += (roi.D + roi.D) * roi.Stack;

                mainRoi = CalculateRectangle(shablon, X, Y, W, H);
                rowSize = 4;

                for (int i = 0; i < roi.Stack; i++)
                {
                    X += roi.D;
                    Y += roi.D;
                    W -= roi.D + roi.D;
                    H -= roi.D + roi.D;

                    stackRoi[i] = CalculateRectangle(shablon, X, Y, W, H);
                    rowSize += 16;
                }

            }

            shablon = null;
            //measure frames
            roi.Results = new double[fi.imageCount][];

            Parallel.ForEach(SteppedRange(cVal, fi.imageCount, fi.sizeC), imageN =>
            {
                
                    Point p1 = roi.GetLocation(imageN)[0];
                    double[] res = new double[rowSize];
                    int dX = p.X - p1.X;
                    int dY = p.Y - p1.Y;
                    int position = 0;

                    double[] tempRes = CalculateMainResults(mainRoi, fi, imageN, dX, dY);

                    Array.Copy(tempRes, 0, res, position, tempRes.Length);
                    position += tempRes.Length;

                    Point midP = new Point(p1.X + roi.Width / 2, p1.Y + roi.Height / 2);

                    for (int i = 0; i < roi.Stack; i++)
                    {
                        tempRes = CalculateStackResults(stackRoi[i], midP, fi, imageN, dX, dY);
                        Array.Copy(tempRes, 0, res, position, tempRes.Length);
                        position += tempRes.Length;
                    }

                    roi.Results[imageN] = res;
                
            });
        }
        private double[] CalculateStackResults(List<Point> pList, Point midP, TifFileInfo fi, int imageN)
        {
            //left - 0
            //right - 1
            // down left = 2
            //down right = 3
            
            double area0 = 0;
            double mean0 = 0;
            double max0 = 0;
            double min0 = 0;

            double area1 = 0;
            double mean1 = 0;
            double max1 = 0;
            double min1 = 0;

            double area2 = 0;
            double mean2 = 0;
            double max2 = 0;
            double min2 = 0;

            double area3 = 0;
            double mean3 = 0;
            double max3 = 0;
            double min3 = 0;

            int c = fi.cValue;

            int[] SpotDiapason = CalculateBorders(fi, c, imageN,
                fi.SelectedSpotThresh[c], fi.thresholds[c], fi.thresholdValues[c]
                , fi.SpotTailType[c], fi.spotSensitivity[c], fi.SpotThresh[c],
                fi.typeSpotThresh[c]);

            
            switch (fi.bitsPerPixel)
            {
                case 8:
                    byte[][] image = fi.image8bit[imageN];
                    min0 = byte.MaxValue;
                    min1 = byte.MaxValue;
                    min2 = byte.MaxValue;
                    min3 = byte.MaxValue;

                    foreach (Point p in pList)
                    {

                        byte val = image[p.Y][p.X];
                        if (CheckColor(fi, fi.image8bitFilter[imageN][p.Y][p.X], SpotDiapason,
                             fi.thresholdValues[c], fi.thresholdColors[c],
                             fi.thresholds[c], fi.SpotColor[c]))
                            if (p.Y < midP.Y && p.X <= midP.X)
                            {
                                //image[p.Y][p.X] = 50;
                                area0++;
                                mean0 += val;
                                if (max0 < val) max0 = val;
                                if (min0 > val) min0 = val;
                            }
                            else if (p.Y <= midP.Y && p.X > midP.X)
                            {
                                //image[p.Y][p.X] = 100;
                                area1++;
                                mean1 += val;
                                if (max1 < val) max1 = val;
                                if (min1 > val) min1 = val;
                            }
                            else if (p.Y >= midP.Y && p.X < midP.X)
                            {
                                //image[p.Y][p.X] = 150;
                                area2++;
                                mean2 += val;
                                if (max2 < val) max2 = val;
                                if (min2 > val) min2 = val;
                            }
                            else if (p.Y > midP.Y && p.X >= midP.X)
                            {
                                //image[p.Y][p.X] = 200;
                                area3++;
                                mean3 += val;
                                if (max3 < val) max3 = val;
                                if (min3 > val) min3 = val;
                            }
                    }
                    break;
                case 16:
                    ushort[][] image16 = fi.image16bit[imageN];
                    min0 = ushort.MaxValue;
                    min1 = ushort.MaxValue;
                    min2 = ushort.MaxValue;
                    min3 = ushort.MaxValue;

                    foreach (Point p in pList)
                    {
                        ushort val = image16[p.Y][p.X];

                        if (CheckColor(fi, fi.image16bitFilter[imageN][p.Y][p.X], SpotDiapason,
                             fi.thresholdValues[c], fi.thresholdColors[c],
                             fi.thresholds[c], fi.SpotColor[c]))
                            if (p.Y < midP.Y && p.X <= midP.X)
                            {
                                //image16[p.Y][p.X] = 50;
                                area0++;
                                mean0 += val;
                                if (max0 < val) max0 = val;
                                if (min0 > val) min0 = val;
                            }
                            else if (p.Y <= midP.Y && p.X > midP.X)
                            {
                                //image16[p.Y][p.X] = 100;
                                area1++;
                                mean1 += val;
                                if (max1 < val) max1 = val;
                                if (min1 > val) min1 = val;
                            }
                            else if (p.Y >= midP.Y && p.X < midP.X)
                            {
                                //image16[p.Y][p.X] = 150;
                                area2++;
                                mean2 += val;
                                if (max2 < val) max2 = val;
                                if (min2 > val) min2 = val;
                            }
                            else if (p.Y > midP.Y && p.X >= midP.X)
                            {
                                //image16[p.Y][p.X] = 200;
                                area3++;
                                mean3 += val;
                                if (max3 < val) max3 = val;
                                if (min3 > val) min3 = val;
                            }
                    }
                    break;
            }

            if (area0 > 0) mean0 /= area0; else min0 = 0;
            if (area1 > 0) mean1 /= area1; else min1 = 0;
            if (area2 > 0) mean2 /= area2; else min2 = 0;
            if (area3 > 0) mean3 /= area3; else min3 = 0;

            double[] res = new double[] {
            area0, mean0, min0, max0 ,
            area1, mean1, min1, max1 ,
            area2, mean2, min2, max2,
            area3, mean3, min3, max3 };

            return res;

        }
        private double[] CalculateStackResults(List<Point> pList, Point midP, TifFileInfo fi, int imageN, int dX, int dY)
        {
            //left - 0
            //right - 1
            // down left = 2
            //down right = 3

            double area0 = 0;
            double mean0 = 0;
            double max0 = 0;
            double min0 = 0;

            double area1 = 0;
            double mean1 = 0;
            double max1 = 0;
            double min1 = 0;

            double area2 = 0;
            double mean2 = 0;
            double max2 = 0;
            double min2 = 0;

            double area3 = 0;
            double mean3 = 0;
            double max3 = 0;
            double min3 = 0;

            int c = fi.cValue;

            int[] SpotDiapason = CalculateBorders(fi, c, imageN,
                fi.SelectedSpotThresh[c], fi.thresholds[c], fi.thresholdValues[c]
                , fi.SpotTailType[c], fi.spotSensitivity[c], fi.SpotThresh[c],
                fi.typeSpotThresh[c]);

            switch (fi.bitsPerPixel)
            {
                case 8:
                    byte[][] image = fi.image8bit[imageN];
                    min0 = byte.MaxValue;
                    min1 = byte.MaxValue;
                    min2 = byte.MaxValue;
                    min3 = byte.MaxValue;

                    foreach (Point p in pList)
                    {
                        Point p1 = new Point(p.X - dX, p.Y - dY);
                        byte val = image[p1.Y][p1.X];
                        if (CheckColor(fi, fi.image8bitFilter[imageN][p.Y][p.X], SpotDiapason,
                             fi.thresholdValues[c], fi.thresholdColors[c],
                             fi.thresholds[c], fi.SpotColor[c]))
                            if (p1.Y < midP.Y && p1.X <= midP.X)
                            {
                                //image[p1.Y][p1.X] = 50;
                                area0++;
                                mean0 += val;
                                if (max0 < val) max0 = val;
                                if (min0 > val) min0 = val;
                            }
                            else if (p1.Y <= midP.Y && p1.X > midP.X)
                            {
                                //image[p1.Y][p1.X] = 100;
                                area1++;
                                mean1 += val;
                                if (max1 < val) max1 = val;
                                if (min1 > val) min1 = val;
                            }
                            else if (p1.Y >= midP.Y && p1.X < midP.X)
                            {
                                //image[p1.Y][p1.X] = 150;
                                area2++;
                                mean2 += val;
                                if (max2 < val) max2 = val;
                                if (min2 > val) min2 = val;
                            }
                            else if (p1.Y > midP.Y && p1.X >= midP.X)
                            {
                                //image[p1.Y][p1.X] = 200;
                                area3++;
                                mean3 += val;
                                if (max3 < val) max3 = val;
                                if (min3 > val) min3 = val;
                            }
                    }
                    break;
                case 16:
                    ushort[][] image16 = fi.image16bit[imageN];
                    min0 = ushort.MaxValue;
                    min1 = ushort.MaxValue;
                    min2 = ushort.MaxValue;
                    min3 = ushort.MaxValue;

                    foreach (Point p in pList)
                    {
                        Point p1 = new Point(p.X - dX, p.Y - dY);
                        ushort val = image16[p1.Y][p1.X];
                        if (CheckColor(fi, fi.image16bitFilter[imageN][p.Y][p.X], SpotDiapason,
                             fi.thresholdValues[c], fi.thresholdColors[c],
                             fi.thresholds[c], fi.SpotColor[c]))
                            if (p1.Y < midP.Y && p1.X <= midP.X)
                            {
                                //image16[p1.Y][p1.X] = 50;
                                area0++;
                                mean0 += val;
                                if (max0 < val) max0 = val;
                                if (min0 > val) min0 = val;
                            }
                            else if (p1.Y <= midP.Y && p1.X > midP.X)
                            {
                                //image16[p1.Y][p1.X] = 100;
                                area1++;
                                mean1 += val;
                                if (max1 < val) max1 = val;
                                if (min1 > val) min1 = val;
                            }
                            else if (p1.Y >= midP.Y && p1.X < midP.X)
                            {
                                //image16[p1.Y][p1.X] = 150;
                                area2++;
                                mean2 += val;
                                if (max2 < val) max2 = val;
                                if (min2 > val) min2 = val;
                            }
                            else if (p1.Y > midP.Y && p1.X >= midP.X)
                            {
                                //image16[p1.Y][p1.X] = 200;
                                area3++;
                                mean3 += val;
                                if (max3 < val) max3 = val;
                                if (min3 > val) min3 = val;
                            }
                    }
                    break;
            }

            if (area0 > 0) mean0 /= area0; else min0 = 0;
            if (area1 > 0) mean1 /= area1; else min1 = 0;
            if (area2 > 0) mean2 /= area2; else min2 = 0;
            if (area3 > 0) mean3 /= area3; else min3 = 0;

            double[] res = new double[] {
            area0, mean0, min0, max0 ,
            area1, mean1, min1, max1 ,
            area2, mean2, min2, max2,
            area3, mean3, min3, max3 };

            return res;

        }
        private double[] CalculateMainResults(List<Point> pList, TifFileInfo fi, int imageN)
        {
            double area = 0;
            double mean = 0;
            double max = 0;
            double min = 0;

            int c = fi.cValue;
            int[] SpotDiapason = CalculateBorders(fi,c, imageN, 
                fi.SelectedSpotThresh[c], fi.thresholds[c], fi.thresholdValues[c]
                , fi.SpotTailType[c], fi.spotSensitivity[c], fi.SpotThresh[c], 
                fi.typeSpotThresh[c]);
            
            switch (fi.bitsPerPixel)
            {
                case 8:
                    byte[][] image = fi.image8bit[imageN];
                    
                    min = byte.MaxValue;

                    foreach (Point p in pList)
                    {
                        //image[p.Y][p.X] = 0;
                        byte val = image[p.Y][p.X];

                        if (CheckColor(fi, fi.image8bitFilter[imageN][p.Y][p.X], SpotDiapason,
                             fi.thresholdValues[c], fi.thresholdColors[c],
                             fi.thresholds[c], fi.SpotColor[c]))
                        {
                            area++;
                            mean += val;
                            if (max < val) max = val;
                            if (min > val) min = val;
                        }
                    }
                    break;
                case 16:
                    ushort[][] image16 = fi.image16bit[imageN];
                    min = ushort.MaxValue;

                    foreach (Point p in pList)
                    {
                        //image16[p.Y ][p.X ] = 0;
                        ushort val = image16[p.Y][p.X];
                        if (CheckColor(fi, fi.image16bitFilter[imageN][p.Y][p.X], SpotDiapason,
                             fi.thresholdValues[c], fi.thresholdColors[c],
                             fi.thresholds[c], fi.SpotColor[c]))
                        {
                            area++;
                            mean += val;
                            if (max < val) max = val;
                            if (min > val) min = val;
                        }
                    }
                    break;
            }

            if (area > 0) mean /= area; else min = 0;
            double[] res = new double[] { area, mean, min, max };

            return res;
        }
        public int[] CalculateBorders(TifFileInfo fi, int C, int frame,
               int SelectedSpotThresh, int thresholds, int[] thresholdValues, string SpotTailType,
               int spotSensitivity, int SpotThresh, int typeSpotThresh)
        {
            
                if (SelectedSpotThresh == 0) return new int[] { 0, 0 };
                //find borders
                List<int> l = new List<int>();
                l.Add(0);
                l.Add(0);

                for (int i = 1; i <= thresholds; i++)
                    l.Add(thresholdValues[i]);

                //calculate histogram
                int[] h = null;
                switch (fi.bitsPerPixel)
                {
                    case 8:
                        h = calculate8bitHisogram(fi, C, frame);
                        l.Add(byte.MaxValue);
                        l.Add(byte.MaxValue);
                        break;
                    case 16:
                        h = calculate16bitHisogram(fi, C, frame);
                        l.Add(16385);
                        l.Add(16385);
                        break;
                }

                //calculate large diapason
                int minVal = l[SelectedSpotThresh - 1];
                int maxVal = l[SelectedSpotThresh];
                if (SpotTailType == ">")
                {
                    minVal = l[SelectedSpotThresh];
                    maxVal = l[SelectedSpotThresh + 1];
                }
                int step = 101 - spotSensitivity;
                minVal = Convert.ToInt32(minVal / step);
                maxVal = Convert.ToInt32(maxVal / step);

                //calculate redused diapason
                int Max = 0;
                int MaxVal = 0;
                for (int i = minVal; i < maxVal; i++)
                    if (MaxVal <= h[i])
                    {
                        Max = i;
                        MaxVal = h[i];
                    }

                if (MaxVal == 0) return new int[] { 0, 0 };

                //find thresh
                int[] res = new int[] { minVal, maxVal };

                int val = SpotThresh;
                if (typeSpotThresh == 1)
                    val = (int)((MaxVal / 100) * val);

                bool changed = false;
                switch (SpotTailType)
                {
                    case "<":

                        for (int i = Max; i <= maxVal; i++)
                        {
                            if (h[i] <= val)
                            {
                                res[0] = i;
                                changed = true;
                                break;
                            }
                        }
                        break;
                    case ">":
                        for (int i = Max; i >= minVal; i--)
                        {
                            if (h[i] <= val)
                            {
                                res[1] = i;
                                changed = true;
                                break;
                            }
                        }
                        break;
                }
                //report
                if (changed == false) return new int[] { 0, 0 };

                res[0] *= step;
                res[1] *= step;

                return res;
            
            
        }
        private int[] calculate8bitHisogram(TifFileInfo fi, int C, int frame)
        {
            int step = 101 - fi.spotSensitivity[C];
            int[] res = new int[(int)(byte.MaxValue / step) + 1];

            //calculate histogram
            foreach (byte[] row in fi.image8bitFilter[frame])
                foreach (byte val in row)
                    res[(int)(val / step)]++;

            return res;
        }
        private int[] calculate16bitHisogram(TifFileInfo fi, int C, int frame)
        {
            int step = 101 - fi.spotSensitivity[C];
            int[] res = new int[(int)(16384 / step) + 1];

            //calculate histogram
            foreach (ushort[] row in fi.image16bitFilter[frame])
                foreach (ushort val in row)
                    res[(int)(val / step)]++;

            return res;
        }
        private double[] CalculateMainResults(List<Point> pList, TifFileInfo fi, int imageN, int dX, int dY)
        {
                double area = 0;
                double mean = 0;
                double max = 0;
                double min = 0;

                int c = fi.cValue;
                int[] SpotDiapason = CalculateBorders(fi, c, imageN,
                    fi.SelectedSpotThresh[c], fi.thresholds[c], fi.thresholdValues[c]
                    , fi.SpotTailType[c], fi.spotSensitivity[c], fi.SpotThresh[c],
                    fi.typeSpotThresh[c]);

            switch (fi.bitsPerPixel)
                {
                    case 8:
                        byte[][] image = fi.image8bit[imageN];
                        min = byte.MaxValue;

                        foreach (Point p in pList)
                        {
                            //image[p.Y - dY][p.X - dX] = 0;
                            byte val = image[p.Y - dY][p.X - dX];

                            if (CheckColor(fi, fi.image8bitFilter[imageN][p.Y][p.X], SpotDiapason,
                             fi.thresholdValues[c], fi.thresholdColors[c],
                             fi.thresholds[c], fi.SpotColor[c]))
                            {
                                area++;
                                mean += val;
                                if (max < val) max = val;
                                if (min > val) min = val;
                            }
                        }
                        break;
                    case 16:
                        ushort[][] image16 = fi.image16bit[imageN];

                        min = ushort.MaxValue;

                        foreach (Point p in pList)
                        {
                            //image16[p.Y - dY][p.X - dX] = 0;
                            ushort val = image16[p.Y - dY][p.X - dX];

                            if (CheckColor(fi, fi.image16bitFilter[imageN][p.Y][p.X], SpotDiapason,
                             fi.thresholdValues[c], fi.thresholdColors[c],
                             fi.thresholds[c], fi.SpotColor[c]))
                            {
                                area++;
                                mean += val;
                                if (max < val) max = val;
                                if (min > val) min = val;
                            }
                        }
                        break;
                }

                if (area > 0) mean /= area; else min = 0;
                double[] res = new double[] { area, mean, min, max };

                return res;
            
        }
        private IEnumerable<int> SteppedRange(int fromInclusive, int toExclusive, int step)
        {
            for (var i = fromInclusive; i < toExclusive; i += step)
            {
                yield return i;
            }
        }
        private List<Point> CalculateRectangle(bool[,] shablon, int X, int Y, int W, int H)
        {
            List<Point> pList = new List<Point>();

            int X1 = X + W;
            int Y1 = Y + H;

            if (X < 0) X = 0;
            if (Y < 0) Y = 0;
            if (X1 >= shablon.GetLength(1)) X1 = shablon.GetLength(1) - 1;
            if (Y1 >= shablon.GetLength(0)) Y1 = shablon.GetLength(0) - 1;

            for (int curY = Y; curY <= Y1; curY++)
                for (int curX = X; curX <= X1; curX++)
                    if (shablon[curY, curX] == false)
                    {
                        shablon[curY, curX] = true;
                        pList.Add(new Point(curX, curY));
                    }

            return pList;
        }

        #endregion rectangles

        #region oval
        private void GetPointsInOvalStatic(ROI roi, TifFileInfo fi, int cVal)
        {
            //FillEllipse(roi, fi, 0);
            //get the location of the first value
            Point p = roi.GetLocation(cVal)[0];
            int X = p.X;
            int Y = p.Y;
            int W = roi.Width;
            int H = roi.Height;
            //create shablon for preventing retacking the same value
            bool[,] shablon = new bool[fi.sizeY, fi.sizeX];
            //take the points for the main roi
            List<Point> mainRoi;
            //calculates the stack
            List<Point>[] stackRoi = new List<Point>[roi.Stack];
            int rowSize = 0;

            if (roi.D >= 0)
            {
                mainRoi = CalculateEllipse(shablon, X, Y, W, H);
                rowSize = 4;

                for (int i = 0; i < roi.Stack; i++)
                {
                    X -= roi.D;
                    Y -= roi.D;
                    W += roi.D + roi.D;
                    H += roi.D + roi.D;

                    stackRoi[i] = CalculateEllipse(shablon, X, Y, W, H);
                    rowSize += 16;
                }
            }
            else
            {
                X -= roi.D * roi.Stack;
                Y -= roi.D * roi.Stack;
                W += (roi.D + roi.D) * roi.Stack;
                H += (roi.D + roi.D) * roi.Stack;

                mainRoi = CalculateEllipse(shablon, X, Y, W, H);
                rowSize = 4;

                for (int i = 0; i < roi.Stack; i++)
                {
                    X += roi.D;
                    Y += roi.D;
                    W -= roi.D + roi.D;
                    H -= roi.D + roi.D;

                    stackRoi[i] = CalculateEllipse(shablon, X, Y, W, H);
                    rowSize += 16;
                }

            }

            shablon = null;
            //measure frames
            roi.Results = new double[fi.imageCount][];

            Parallel.ForEach(SteppedRange(cVal, fi.imageCount, fi.sizeC), imageN =>
            {
                
                    double[] res = new double[rowSize];
                    int position = 0;

                    double[] tempRes = CalculateMainResults(mainRoi, fi, imageN);

                    Array.Copy(tempRes, 0, res, position, tempRes.Length);
                    position += tempRes.Length;

                    Point midP = new Point(p.X + roi.Width / 2, p.Y + roi.Height / 2);

                    for (int i = 0; i < roi.Stack; i++)
                    {
                        tempRes = CalculateStackResults(stackRoi[i], midP, fi, imageN);
                        Array.Copy(tempRes, 0, res, position, tempRes.Length);
                        position += tempRes.Length;
                    }

                    roi.Results[imageN] = res;
                
            });
        }

        private void GetPointsInOvalTracking(ROI roi, TifFileInfo fi, int cVal)
        {

            //FillEllipse(roi, fi, 0);
            //get the location of the first value
            Point p = roi.GetLocation(cVal)[0];
            int X = p.X;
            int Y = p.Y;
            int W = roi.Width;
            int H = roi.Height;
            //create shablon for preventing retacking the same value
            bool[,] shablon = new bool[fi.sizeY, fi.sizeX];
            //take the points for the main roi
            List<Point> mainRoi;
            //calculates the stack
            List<Point>[] stackRoi = new List<Point>[roi.Stack];
            int rowSize = 0;

            if (roi.D >= 0)
            {
                mainRoi = CalculateEllipse(shablon, X, Y, W, H);
                rowSize = 4;

                for (int i = 0; i < roi.Stack; i++)
                {
                    X -= roi.D;
                    Y -= roi.D;
                    W += roi.D + roi.D;
                    H += roi.D + roi.D;

                    stackRoi[i] = CalculateEllipse(shablon, X, Y, W, H);
                    rowSize += 16;
                }
            }
            else
            {
                X -= roi.D * roi.Stack;
                Y -= roi.D * roi.Stack;
                W += (roi.D + roi.D) * roi.Stack;
                H += (roi.D + roi.D) * roi.Stack;

                mainRoi = CalculateEllipse(shablon, X, Y, W, H);
                rowSize = 4;

                for (int i = 0; i < roi.Stack; i++)
                {
                    X += roi.D;
                    Y += roi.D;
                    W -= roi.D + roi.D;
                    H -= roi.D + roi.D;

                    stackRoi[i] = CalculateEllipse(shablon, X, Y, W, H);
                    rowSize += 16;
                }

            }

            shablon = null;
            //measure frames
            roi.Results = new double[fi.imageCount][];

            Parallel.ForEach(SteppedRange(cVal, fi.imageCount, fi.sizeC), imageN =>
            {
               
                    Point p1 = roi.GetLocation(imageN)[0];
                    double[] res = new double[rowSize];
                    int dX = p.X - p1.X;
                    int dY = p.Y - p1.Y;
                    int position = 0;

                    double[] tempRes = CalculateMainResults(mainRoi, fi, imageN, dX, dY);
                    Array.Copy(tempRes, 0, res, position, tempRes.Length);
                    position += tempRes.Length;

                    //System.Windows.Forms.MessageBox.Show(string.Join("\t", tempRes));

                    Point midP = new Point(p1.X + roi.Width / 2, p1.Y + roi.Height / 2);

                    for (int i = 0; i < roi.Stack; i++)
                    {
                        tempRes = CalculateStackResults(stackRoi[i], midP, fi, imageN, dX, dY);
                        Array.Copy(tempRes, 0, res, position, tempRes.Length);
                        position += tempRes.Length;
                    }
                    roi.Results[imageN] = res;
                
            });
        }

        private List<Point> CalculateEllipse(bool[,] shablon, int Xn, int Yn, int Wn, int Hn)
        {
            List<Point> pList = new List<Point>();

            int left = Xn;
            int top = Yn;
            int right = Xn + Wn;
            int bottom = Yn + Hn;

            int a, b, x, y, temp;
            int old_y;
            int d1, d2;
            int a2, b2, a2b2, a2sqr, b2sqr, a4sqr, b4sqr;
            int a8sqr, b8sqr, a4sqr_b4sqr;
            int fn, fnw, fw;
            int fnn, fnnw, fnwn, fnwnw, fnww, fww, fwnw;

            if (right < left)
            {
                temp = left;
                left = right;
                right = temp;
            }
            if (bottom < top)
            {
                temp = top;
                top = bottom;
                bottom = temp;
            }

            a = (right - left) / 2;
            b = (bottom - top) / 2;

            x = 0;
            y = b;

            a2 = a * a;
            b2 = b * b;
            a2b2 = a2 + b2;
            a2sqr = a2 + a2;
            b2sqr = b2 + b2;
            a4sqr = a2sqr + a2sqr;
            b4sqr = b2sqr + b2sqr;
            a8sqr = a4sqr + a4sqr;
            b8sqr = b4sqr + b4sqr;
            a4sqr_b4sqr = a4sqr + b4sqr;

            fn = a8sqr + a4sqr;
            fnn = a8sqr;
            fnnw = a8sqr;
            fnw = a8sqr + a4sqr - b8sqr * a + b8sqr;
            fnwn = a8sqr;
            fnwnw = a8sqr + b8sqr;
            fnww = b8sqr;
            fwnw = b8sqr;
            fww = b8sqr;
            d1 = b2 - b4sqr * a + a4sqr;

            while ((fnw < a2b2) || (d1 < 0) || ((fnw - fn > b2) && (y > 0)))
            {
                DrawHorizontalOvalLine(left + x, right - x, top + y, shablon, pList); // Replace with your own span filling function. The hard-coded numbers were color values for testing purposes and can be ignored.
                DrawHorizontalOvalLine(left + x, right - x, bottom - y, shablon, pList);

                y--;
                if ((d1 < 0) || (fnw - fn > b2))
                {
                    d1 += fn;
                    fn += fnn;
                    fnw += fnwn;
                }
                else
                {
                    x++;
                    d1 += fnw;
                    fn += fnnw;
                    fnw += fnwnw;
                }
            }

            fw = fnw - fn + b4sqr;
            d2 = d1 + (fw + fw - fn - fn + a4sqr_b4sqr + a8sqr) / 4;
            fnw += b4sqr - a4sqr;

            old_y = y + 1;

            while (x <= a)
            {
                if (y != old_y) // prevent overdraw
                {
                    DrawHorizontalOvalLine(left + x, right - x, top + y, shablon, pList);
                    DrawHorizontalOvalLine(left + x, right - x, bottom - y, shablon, pList);
                }

                old_y = y;
                x++;
                if (d2 < 0)
                {
                    y--;
                    d2 += fnw;
                    fw += fwnw;
                    fnw += fnwnw;
                }
                else
                {
                    d2 += fw;
                    fw += fww;
                    fnw += fnww;
                }
            }

            return pList;
        }
        private void DrawHorizontalOvalLine(int left, int right, int y, bool[,] shablon, List<Point> pList)
        {
            if (left < 0) left = 0;
            if (y < 0) y = 0;
            if (left >= shablon.GetLength(1)) left = shablon.GetLength(1) - 1;
            if (y >= shablon.GetLength(0)) y = shablon.GetLength(0) - 1;

            for (int x = left; x <= right; x++)
                if (shablon[y, x] == false)
                {
                    shablon[y, x] = true;
                    pList.Add(new Point(x, y));
                }
        }

        #endregion oval

        #region polygon
        public List<Point> Polygon_Layers(int imageN, int D, ROI roi, Rectangle rect)
        {
            Point[] points = roi.GetLocation(imageN);
            List<Point> res = new List<Point>();

            double Cx = 0;
            double Cy = 0;
            foreach (Point p in points)
            {
                Cx += p.X;
                Cy += p.Y;
            }

            Cx /= points.Length;
            Cy /= points.Length;
            Cx = (int)Cx;
            Cy = (int)Cy;

            double dY, dX, C, C1, sinA;

            foreach (Point p in points)
            {
                C1 = Math.Sqrt((p.X - Cx) * (p.X - Cx) + (p.Y - Cy) * (p.Y - Cy));
                C = C1 + D;

                sinA = (p.Y - Cy) / C1;
                dY = sinA * C;

                dX = Math.Sqrt(C * C - dY * dY);

                dY += Cy;
                if (p.Y > Cy)
                    dY += 1;

                if (p.X > Cx)
                {
                    dX += Cx + 1;
                }
                else
                {
                    dX = Cx - dX;
                }

                res.Add(new Point((int)dX, (int)dY));
            }

            return res;
        }

        private void GetPointsInPolygonStatic(ROI roi, TifFileInfo fi, int cVal)
        {
            //Point[][] points = roi.GetLocationAll();
            //GetPolygonPoints(points[0], fi, 0);
            Point[] points = roi.GetLocation(cVal);

            //take the points for the main roi
            List<Point> mainRoi = null;
            List<Point>[] stackRoi = null;
            int rowSize = 4;

            if (roi.Stack < 1)
            {
                mainRoi = GetPolygonPoints(points, fi);
            }
            else
            {
                //create shablon for preventing retacking the same value
                bool[,] shablon = new bool[fi.sizeY, fi.sizeX];
                stackRoi = new List<Point>[roi.Stack];

                if (roi.D >= 0)
                {
                    mainRoi = GetPolygonPoints(points.ToList(), fi, shablon);
                    rowSize = 4;

                    int D = roi.D;
                    for (int i = 0; i < roi.Stack; i++)
                    {
                        stackRoi[i] = GetPolygonPoints(
                            Polygon_Layers(cVal, D, roi, new Rectangle(0, 0, fi.sizeX, fi.sizeY)), fi, shablon);

                        rowSize += 16;
                        D += roi.D;
                    }
                }
                else
                {
                    {

                        int D = roi.D * roi.Stack;
                        //inner layer
                        mainRoi = GetPolygonPoints(
                                Polygon_Layers(cVal, D, roi, new Rectangle(0, 0, fi.sizeX, fi.sizeY)), fi, shablon);

                        rowSize = 4;
                        D -= roi.D;
                        //midle layers
                        if (roi.Stack > 1)
                            for (int i = roi.Stack - 1; i > 0; i--)
                            {
                                stackRoi[i] = GetPolygonPoints(
                                    Polygon_Layers(cVal, D, roi, new Rectangle(0, 0, fi.sizeX, fi.sizeY)), fi, shablon);

                                rowSize += 16;
                                D -= roi.D;
                            }
                        //outer layer
                        stackRoi[0] = GetPolygonPoints(points.ToList(), fi, shablon);
                        rowSize += 16;
                    }
                }

                shablon = null;
            }

            //measure frames
            roi.Results = new double[fi.imageCount][];

            Parallel.ForEach(SteppedRange(cVal, fi.imageCount, fi.sizeC), imageN =>
            {
                
                    double[] res = new double[rowSize];
                    int position = 0;

                    double[] tempRes = CalculateMainResults(mainRoi, fi, imageN);

                    Array.Copy(tempRes, 0, res, position, tempRes.Length);
                    position += tempRes.Length;

                    for (int i = 0; i < roi.Stack; i++)
                    {
                        tempRes = CalculateMainResults(stackRoi[i], fi, imageN);
                        Array.Copy(tempRes, 0, res, position, tempRes.Length);
                        position += 16;
                    }

                    roi.Results[imageN] = res;
               
            });
        }
        private void GetPointsInPolygonTracking(ROI roi, TifFileInfo fi, int cVal)
        {
            Point[][] points = roi.GetLocationAll();

            int rowSize = 4;

            if (roi.Stack > 0)
                rowSize += roi.Stack * 16;

            //measure frames
            roi.Results = new double[fi.imageCount][];

            Parallel.ForEach(SteppedRange(cVal, fi.imageCount, fi.sizeC), imageN =>
            {
               
                    double[] res = new double[rowSize];
                    int position = 0;
                    double[] tempRes = null;

                    if (roi.Stack == 0)
                    {
                        tempRes = GetPolygonPoints(points[imageN], fi, imageN);
                        Array.Copy(tempRes, 0, res, position, tempRes.Length);
                    }
                    else
                    {
                        bool[,] shablon = new bool[fi.sizeY, fi.sizeX];

                        if (roi.D >= 0)
                        {
                            tempRes = GetPolygonPoints(points[imageN], fi, imageN, shablon);
                            Array.Copy(tempRes, 0, res, position, tempRes.Length);
                            position += tempRes.Length;

                            int D = roi.D;
                            for (int i = 0; i < roi.Stack; i++)
                            {
                                tempRes = GetPolygonPoints(
                                    Polygon_Layers(imageN, D, roi, new Rectangle(0, 0, fi.sizeX, fi.sizeY)).ToArray(), fi, imageN, shablon);
                                Array.Copy(tempRes, 0, res, position, tempRes.Length);
                                position += 16;
                                D += roi.D;
                            }

                        }
                        else
                        {
                            int D = roi.D * roi.Stack;
                            //inner layer
                            tempRes = GetPolygonPoints(
                                    Polygon_Layers(imageN, D, roi, new Rectangle(0, 0, fi.sizeX, fi.sizeY)).ToArray(), fi, imageN, shablon);
                            Array.Copy(tempRes, 0, res, position, tempRes.Length);
                            position += tempRes.Length;

                            D -= roi.D;
                            //midle layers
                            if (roi.Stack > 1)
                                for (int i = roi.Stack - 1; i > 0; i--)
                                {
                                    tempRes = GetPolygonPoints(
                                    Polygon_Layers(imageN, D, roi, new Rectangle(0, 0, fi.sizeX, fi.sizeY)).ToArray(), fi, imageN, shablon);
                                    Array.Copy(tempRes, 0, res, position, tempRes.Length);
                                    position += 16;
                                    D -= roi.D;
                                }
                            //outer layer
                            tempRes = GetPolygonPoints(points[imageN], fi, imageN, shablon);
                            Array.Copy(tempRes, 0, res, position, tempRes.Length);
                        }

                        shablon = null;
                    }
                    roi.Results[imageN] = res;
               
            });
            
        }
        private List<Point> GetPolygonPoints(Point[] points, TifFileInfo fi)
        {
            List<Point> pList = new List<Point>();
            List<int> xList = new List<int>();
            int x, y, swap, i, j, maxY = 0, minY = fi.sizeY - 1;
            //check the size of the polygon
            foreach (Point p in points)
            {
                if (p.Y > maxY && p.Y < fi.sizeY) maxY = p.Y;
                if (p.Y < minY && p.Y >= 0) minY = p.Y;
            }
            //scan lines for Y coords
            for (y = minY; y <= maxY; y++)
            {
                //prepare list for X coords
                xList.Clear();
                j = points.Length - 1;
                //calculate X points via tgA function
                for (i = 0; i < points.Length; i++)
                {
                    if ((points[i].Y < y && points[j].Y >= y) ||
                        (points[j].Y < y && points[i].Y >= y))
                    {
                        // tgA = (y2-y1)/(x2-x1)
                        x = (int)((((y - points[i].Y) * (points[j].X - points[i].X)) /
                            (points[j].Y - points[i].Y)) + points[i].X);

                        xList.Add(x);
                    }

                    j = i;
                }
                //break if there is no points in the line
                if (xList.Count == 0) continue;
                //sort by value via bubble loop
                i = 0;
                while (i < xList.Count - 1)
                {
                    j = i + 1;
                    if (xList[i] > xList[j])
                    {
                        swap = xList[i];
                        xList[i] = xList[j];
                        xList[j] = swap;

                        if (i > 0) i--;
                    }
                    else
                    {
                        i++;
                    }
                }
                //find all points inside the bounds 2 by 2
                for (i = 0; i < xList.Count; i += 2)
                {
                    j = i + 1;
                    if (xList[i] >= fi.sizeX) break;
                    if (xList[j] > 0)
                    {
                        if (xList[i] < 0) xList[i] = 0;
                        if (xList[j] >= fi.sizeX) xList[j] = fi.sizeX - 1;

                        for (x = xList[i]; x <= xList[j]; x++)
                            pList.Add(new Point(x, y));
                    }
                }
            }

            return pList;
        }

        private List<Point> GetPolygonPoints(List<Point> points, TifFileInfo fi, bool[,] shablon)
        {
            List<Point> pList = new List<Point>();
            List<int> xList = new List<int>();
            int x, y, swap, i, j, maxY = 0, minY = fi.sizeY - 1;
            //check the size of the polygon
            foreach (Point p in points)
            {
                if (p.Y > maxY && p.Y < fi.sizeY) maxY = p.Y;
                if (p.Y < minY && p.Y >= 0) minY = p.Y;
            }
            //scan lines for Y coords
            for (y = minY; y <= maxY; y++)
            {
                //prepare list for X coords
                xList.Clear();
                j = points.Count - 1;
                //calculate X points via tgA function
                for (i = 0; i < points.Count; i++)
                {
                    if ((points[i].Y < y && points[j].Y >= y) ||
                        (points[j].Y < y && points[i].Y >= y))
                    {
                        // tgA = (y2-y1)/(x2-x1)
                        x = (int)((((y - points[i].Y) * (points[j].X - points[i].X)) /
                            (points[j].Y - points[i].Y)) + points[i].X);

                        xList.Add(x);
                    }

                    j = i;
                }
                //break if there is no points in the line
                if (xList.Count == 0) continue;
                //sort by value via bubble loop
                i = 0;
                while (i < xList.Count - 1)
                {
                    j = i + 1;
                    if (xList[i] > xList[j])
                    {
                        swap = xList[i];
                        xList[i] = xList[j];
                        xList[j] = swap;

                        if (i > 0) i--;
                    }
                    else
                    {
                        i++;
                    }
                }
                //find all points inside the bounds 2 by 2
                for (i = 0; i < xList.Count; i += 2)
                {
                    j = i + 1;
                    if (xList[i] >= fi.sizeX) break;
                    if (xList[j] > 0)
                    {
                        if (xList[i] < 0) xList[i] = 0;
                        if (xList[j] >= fi.sizeX) xList[j] = fi.sizeX - 1;

                        for (x = xList[i]; x <= xList[j]; x++)
                            if (shablon[y, x] == false)
                            {
                                pList.Add(new Point(x, y));
                                shablon[y, x] = true;
                            }
                    }
                }
            }

            return pList;
        }
        private double[] GetPolygonPoints(Point[] points, TifFileInfo fi, int imageN, bool[,] shablon)
        {

            int c = fi.cValue;
            int[] SpotDiapason = CalculateBorders(fi, c, imageN,
            fi.SelectedSpotThresh[c], fi.thresholds[c], fi.thresholdValues[c]
            , fi.SpotTailType[c], fi.spotSensitivity[c], fi.SpotThresh[c],
            fi.typeSpotThresh[c]);

            double area = 0;
            double mean = 0;
            double max = 0;
            double min = double.MaxValue;

            List<int> xList = new List<int>();
            int x, y, swap, i, j, maxY = 0, minY = fi.sizeY - 1;
            //check the size of the polygon
            foreach (Point p in points)
            {
                if (p.Y > maxY && p.Y < fi.sizeY) maxY = p.Y;
                if (p.Y < minY && p.Y >= 0) minY = p.Y;
            }
            //scan lines for Y coords
            for (y = minY; y <= maxY; y++)
            {
                //prepare list for X coords
                xList.Clear();
                j = points.Length - 1;
                //calculate X points via tgA function
                for (i = 0; i < points.Length; i++)
                {
                    if ((points[i].Y < y && points[j].Y >= y) ||
                        (points[j].Y < y && points[i].Y >= y))
                    {
                        // tgA = (y2-y1)/(x2-x1)
                        x = (int)((((y - points[i].Y) * (points[j].X - points[i].X)) /
                            (points[j].Y - points[i].Y)) + points[i].X);

                        xList.Add(x);
                    }

                    j = i;
                }
                //break if there is no points in the line
                if (xList.Count == 0) continue;
                //sort by value via bubble loop
                i = 0;
                while (i < xList.Count - 1)
                {
                    j = i + 1;
                    if (xList[i] > xList[j])
                    {
                        swap = xList[i];
                        xList[i] = xList[j];
                        xList[j] = swap;

                        if (i > 0) i--;
                    }
                    else
                    {
                        i++;
                    }
                }


                switch (fi.bitsPerPixel)
                {
                    case 8:
                        byte[][] image = fi.image8bit[imageN];
                       
                        //find all points inside the bounds 2 by 2
                        for (i = 0; i < xList.Count; i += 2)
                        {
                            j = i + 1;
                            if (xList[i] >= fi.sizeX) break;
                            if (xList[j] > 0)
                            {
                                if (xList[i] < 0) xList[i] = 0;
                                if (xList[j] >= fi.sizeX) xList[j] = fi.sizeX - 1;

                                for (x = xList[i]; x <= xList[j]; x++)
                                    if (shablon[y, x] == false)
                                    {
                                        //image[y][x] = 0;
                                        byte val = image[y][x];
                                        if (CheckColor(fi, fi.image8bitFilter[imageN][y][x], SpotDiapason,
                             fi.thresholdValues[c], fi.thresholdColors[c],
                             fi.thresholds[c], fi.SpotColor[c]))
                                        {
                                            area++;
                                            mean += val;
                                            if (max < val) max = val;
                                            if (min > val) min = val;
                                        }
                                        shablon[y, x] = true;

                                    }
                            }
                        }
                        break;
                    case 16:
                        ushort[][] image16 = fi.image16bit[imageN];
                        
                        //find all points inside the bounds 2 by 2
                        for (i = 0; i < xList.Count; i += 2)
                        {
                            j = i + 1;
                            if (xList[i] >= fi.sizeX) break;
                            if (xList[j] > 0)
                            {
                                if (xList[i] < 0) xList[i] = 0;
                                if (xList[j] >= fi.sizeX) xList[j] = fi.sizeX - 1;

                                for (x = xList[i]; x <= xList[j]; x++)
                                    if (shablon[y, x] == false)
                                    {
                                        //image16[y][x] = 0;
                                        ushort val = image16[y][x];
                                        if (CheckColor(fi, fi.image16bitFilter[imageN][y][x], SpotDiapason,
                             fi.thresholdValues[c], fi.thresholdColors[c],
                             fi.thresholds[c], fi.SpotColor[c]))
                                        {
                                            area++;
                                            mean += val;
                                            if (max < val) max = val;
                                            if (min > val) min = val;
                                        }
                                        shablon[y, x] = true;
                                    }
                            }
                        }
                        break;
                }

            }

            if (area > 0) mean /= area; else min = 0;
            double[] res = new double[] { area, mean, min, max };

            return res;
        }
        private double[] GetPolygonPoints(Point[] points, TifFileInfo fi, int imageN)
        {

            int c = fi.cValue;
            int[] SpotDiapason = CalculateBorders(fi, c, imageN,
                            fi.SelectedSpotThresh[c], fi.thresholds[c], fi.thresholdValues[c]
                            , fi.SpotTailType[c], fi.spotSensitivity[c], fi.SpotThresh[c],
                            fi.typeSpotThresh[c]);

            double area = 0;
            double mean = 0;
            double max = 0;
            double min = double.MaxValue;

            List<int> xList = new List<int>();
            int x, y, swap, i, j, maxY = 0, minY = fi.sizeY - 1;
            //check the size of the polygon
            foreach (Point p in points)
            {
                if (p.Y > maxY && p.Y < fi.sizeY) maxY = p.Y;
                if (p.Y < minY && p.Y >= 0) minY = p.Y;
            }
            //scan lines for Y coords
            for (y = minY; y <= maxY; y++)
            {
                //prepare list for X coords
                xList.Clear();
                j = points.Length - 1;
                //calculate X points via tgA function
                for (i = 0; i < points.Length; i++)
                {
                    if ((points[i].Y < y && points[j].Y >= y) ||
                        (points[j].Y < y && points[i].Y >= y))
                    {
                        // tgA = (y2-y1)/(x2-x1)
                        x = (int)((((y - points[i].Y) * (points[j].X - points[i].X)) /
                            (points[j].Y - points[i].Y)) + points[i].X);

                        xList.Add(x);
                    }

                    j = i;
                }
                //break if there is no points in the line
                if (xList.Count == 0) continue;
                //sort by value via bubble loop
                i = 0;
                while (i < xList.Count - 1)
                {
                    j = i + 1;
                    if (xList[i] > xList[j])
                    {
                        swap = xList[i];
                        xList[i] = xList[j];
                        xList[j] = swap;

                        if (i > 0) i--;
                    }
                    else
                    {
                        i++;
                    }
                }


                switch (fi.bitsPerPixel)
                {
                    case 8:
                        byte[][] image = fi.image8bit[imageN];
                        
                        //find all points inside the bounds 2 by 2
                        for (i = 0; i < xList.Count; i += 2)
                        {
                            j = i + 1;
                            if (xList[i] >= fi.sizeX) break;
                            if (xList[j] > 0)
                            {
                                if (xList[i] < 0) xList[i] = 0;
                                if (xList[j] >= fi.sizeX) xList[j] = fi.sizeX - 1;

                                for (x = xList[i]; x <= xList[j]; x++)
                                {
                                    //image[y][x] = 0;
                                    byte val = image[y][x];

                                    if (CheckColor(fi, fi.image8bitFilter[imageN][y][x], SpotDiapason,
                             fi.thresholdValues[c], fi.thresholdColors[c],
                             fi.thresholds[c], fi.SpotColor[c]))
                                    {
                                        area++;
                                        mean += val;
                                        if (max < val) max = val;
                                        if (min > val) min = val;
                                    }
                                }
                            }
                        }
                        break;
                    case 16:
                        ushort[][] image16 = fi.image16bit[imageN];
                        
                        //find all points inside the bounds 2 by 2
                        for (i = 0; i < xList.Count; i += 2)
                        {
                            j = i + 1;
                            if (xList[i] >= fi.sizeX) break;
                            if (xList[j] > 0)
                            {
                                if (xList[i] < 0) xList[i] = 0;
                                if (xList[j] >= fi.sizeX) xList[j] = fi.sizeX - 1;

                                for (x = xList[i]; x <= xList[j]; x++)
                                {
                                    //image16[y][x] = 0;
                                    ushort val = image16[y][x];
                                    if (CheckColor(fi, fi.image16bitFilter[imageN][y][x], SpotDiapason,
                             fi.thresholdValues[c], fi.thresholdColors[c],
                             fi.thresholds[c], fi.SpotColor[c]))
                                    {
                                        area++;
                                        mean += val;
                                        if (max < val) max = val;
                                        if (min > val) min = val;
                                    }
                                }
                            }
                        }
                        break;
                }

            }

            if (area > 0) mean /= area; else min = 0;
            double[] res = new double[] { area, mean, min, max };

            return res;
        }
        #endregion Polygon

        #endregion Measure
    }
}

