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
using Accord.Imaging;
using Accord.Imaging.Filters;
using System.Windows.Forms;
using System.ComponentModel;
using System.Drawing;

namespace Cell_Tool_3 
{
    class MyWatershed
    {
        private ImageAnalyser IA;
        public Form Dialog = new Form();
        private ComboBox ThresholdCB = new ComboBox();
        public TextBox ToleranceTB = new TextBox();
        public DistanceTransformMethod distance = DistanceTransformMethod.Euclidean;
        public float tolerance = 0.5f;
        public int threshold = 1;
        public CheckBox FillHolesCB = new CheckBox();
        public MyWatershed(ImageAnalyser IA)
        {
            this.IA = IA;
            createDialog();
        }
        private void createDialog()
        {
            Dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
            Dialog.Text = "Watershed";
            Dialog.StartPosition = FormStartPosition.CenterScreen;
            Dialog.WindowState = FormWindowState.Normal;
            Dialog.MinimizeBox = false;
            Dialog.MaximizeBox = false;
            Dialog.BackColor = IA.FileBrowser.BackGround2Color1;
            Dialog.ForeColor = IA.FileBrowser.ShriftColor1;

            Dialog.FormClosing += new FormClosingEventHandler(delegate(object o, FormClosingEventArgs a)
            {
                Dialog.Visible = false;
                a.Cancel = true;
            });
            
            Dialog.Width = 240;
            Dialog.Height = 190;

            Dialog.SuspendLayout();
            /*
            Label ThresholdLab = new Label();
            ThresholdLab.Text = "Select threshold:";
            ThresholdLab.Width = 100;
            ThresholdLab.Location = new System.Drawing.Point(10,15);
            Dialog.Controls.Add(ThresholdLab);
                        
            ThresholdCB.Width = 30;
            ThresholdCB.Location = new System.Drawing.Point(110, 13);
            Dialog.Controls.Add(ThresholdCB);
            ThresholdCB.SelectedIndexChanged += new EventHandler(delegate (object o, EventArgs a)
            {
                threshold = ThresholdCB.SelectedIndex + 1;
            });
            */

            Label ToleranceLab = new Label();
            ToleranceLab.Text = "Tolerance:";
            ToleranceLab.Width = 100;
            ToleranceLab.Location = new System.Drawing.Point(10, 15);
            Dialog.Controls.Add(ToleranceLab);
            
            ToleranceTB.Text = tolerance.ToString();
            ToleranceTB.Width = 30;
            ToleranceTB.Location = new System.Drawing.Point(110, 13);
            Dialog.Controls.Add(ToleranceTB);

            Label DistanceLab = new Label();
            DistanceLab.Text = "Distance Transform:";
            DistanceLab.Width = 100;
            DistanceLab.Location = new System.Drawing.Point(10, 45);
            Dialog.Controls.Add(DistanceLab);
            
            ComboBox DistanceCB = new ComboBox();
            DistanceCB.Width =110;
            DistanceCB.Items.AddRange(new string[] { "Euclidean", "SquaredEuclidean", "Chessboard", "Manhattan" });
            DistanceCB.SelectedIndex = 0;
            DistanceCB.SelectedIndexChanged+=new EventHandler(delegate(object o, EventArgs a)
            {
                switch (DistanceCB.SelectedIndex)
                {
                    case 0:
                        distance = DistanceTransformMethod.Euclidean;
                        break;
                    case 1:
                        distance = DistanceTransformMethod.SquaredEuclidean;
                        break;
                    case 2:
                        distance = DistanceTransformMethod.Chessboard;
                        break;
                    case 3:
                        distance = DistanceTransformMethod.Manhattan;
                        break;
                    default:
                        distance = DistanceTransformMethod.Euclidean;
                        break;
                }
            });
            DistanceCB.Location = new System.Drawing.Point(110, 43);
            Dialog.Controls.Add(DistanceCB);
            
            FillHolesCB.Text = "Fill holes";
            FillHolesCB.Location = new System.Drawing.Point(15, 75);
            Dialog.Controls.Add(FillHolesCB);

            //buttons
            Panel okBox = new Panel();
            okBox.Height = 40;
            okBox.Dock = DockStyle.Bottom;
            Dialog.Controls.Add(okBox);

            Button okBtn = new Button();
            okBtn.Text = "Process";
            okBtn.BackColor = System.Drawing.SystemColors.ButtonFace;
            okBtn.ForeColor = System.Drawing.Color.Black;
            okBtn.Location = new System.Drawing.Point(20, 10);
            okBtn.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            okBox.Controls.Add(okBtn);

            Button cancelBtn = new Button();
            cancelBtn.Text = "Cancel";
            cancelBtn.BackColor = System.Drawing.SystemColors.ButtonFace;
            cancelBtn.Location = new System.Drawing.Point(Dialog.Width - cancelBtn.Width - 60, 10);
            cancelBtn.ForeColor = System.Drawing.Color.Black;
            cancelBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            okBox.Controls.Add(cancelBtn);

            okBtn.Click += new EventHandler(delegate (object sender, EventArgs e)
            {
                try
                {
                    tolerance = float.Parse(ToleranceTB.Text);
                }
                catch
                {
                    MessageBox.Show("Tolerance must be numeric!");
                    return;
                }
                Dialog.Visible = false;
                //event
                TifFileInfo fi = null;
                try
                {
                    fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
                }
                catch { return; }

                if (fi == null) return;
                if (!fi.available)
                {
                    MessageBox.Show("Image is not ready yet! \nTry again later.");
                    return;
                }
                int C = fi.cValue;
                //background worker
                var bgw = new BackgroundWorker();
                bgw.WorkerReportsProgress = true;
                fi.available = false;
                fi.fillHoles = FillHolesCB.Checked;
                //Add event for projection here
                bgw.DoWork += new DoWorkEventHandler(delegate (Object o, DoWorkEventArgs a)
                {
                    ProcessFilter(fi,C,this.tolerance, 1,this.distance);
                    ((BackgroundWorker)o).ReportProgress(0);
                });

                bgw.ProgressChanged += new ProgressChangedEventHandler(delegate (Object o, ProgressChangedEventArgs a)
                {
                    if (a.ProgressPercentage == 0)
                    {
                        IA.Segmentation.MyFilters.addToHistoryOldInfo(C, fi);

                        fi.newFilterHistory[C].Add(
                            ToString(fi, C, this.tolerance, 1, this.distance));

                        IA.Segmentation.MyFilters.addToHistoryNewInfo(C, fi);

                        IA.MarkAsNotSaved();
                        IA.ReloadImages();
                    }
                    fi.available = true;
                    IA.FileBrowser.StatusLabel.Text = "Ready";
                });
                //Start background worker
                IA.FileBrowser.StatusLabel.Text = "Watershed...";
                //start bgw
                bgw.RunWorkerAsync();
            });

            cancelBtn.Click += new EventHandler(delegate (object sender, EventArgs e)
            {
                Dialog.Visible = false;
            });

            Dialog.KeyPreview = true;
            Dialog.KeyDown += new KeyEventHandler(delegate (object sender, KeyEventArgs e)
            {
                switch (e.KeyCode)
                {
                    case Keys.Escape:
                        Dialog.Visible = false;
                        break;
                    case Keys.Enter:
                        okBtn.PerformClick();
                        break;
                    default:
                        break;
                }
            });

            Dialog.ResumeLayout();
        }
        public void StartDialog(object sender,EventArgs e)
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
                MessageBox.Show("Image is not ready yet! \nTry again later.");
                return;
            }

            if (checkChanel(fi)) return;

            //Update Threshold panel
            int oldIndex = ThresholdCB.SelectedIndex+1;
            if (oldIndex < 1) oldIndex = 1;
            ThresholdCB.Items.Clear();
            for (int i = 1; i <= fi.thresholds[fi.cValue]; i++)
                ThresholdCB.Items.Add(i.ToString());
            if(oldIndex <= ThresholdCB.Items.Count)
            {
                threshold = oldIndex;
                ThresholdCB.SelectedIndex = oldIndex - 1;
            }

            if (ThresholdCB.Items.Count == 0 || IA.Segmentation.SegmentationCBox.SelectedIndex == 0)
            {
                MessageBox.Show("Image must be segmented!");
                return;
            }

            ToleranceTB.Text = tolerance.ToString();

            FillHolesCB.Checked = fi.fillHoles;

            // Linux change
            IA.FileBrowser.StatusLabel.Text = "Dialog open";
            Dialog.ShowDialog();
            IA.FileBrowser.StatusLabel.Text = "Ready";
        }
        private void DuplicateImage(TifFileInfo fi)
        {
            if (fi.image8bitFilter == null) fi.image8bitFilter = fi.image8bit;
            if (fi.image16bitFilter == null) fi.image16bitFilter = fi.image16bit;

            if (fi.FilterHistory.Count == 0)
                switch (fi.bitsPerPixel)
                {
                    case 8:
                        byte[][][] newImage8 = new byte[fi.imageCount][][];
                        Parallel.For(0, fi.imageCount, (ind) =>
                        {
                            byte[][] frame = new byte[fi.sizeY][];
                            for (int y = 0; y < fi.sizeY; y++)
                            {
                                frame[y] = new byte[fi.sizeX];
                                Array.Copy(fi.image8bitFilter[ind][y], frame[y], fi.sizeX);
                            }
                            newImage8[ind] = frame;
                        });
                        fi.image8bitFilter = newImage8;
                        break;
                    case 16:
                        ushort[][][] newImage16 = new ushort[fi.imageCount][][];
                        Parallel.For(0, fi.imageCount, (ind) =>
                        {
                            ushort[][] frame = new ushort[fi.sizeY][];
                            for (int y = 0; y < fi.sizeY; y++)
                            {
                                frame[y] = new ushort[fi.sizeX];
                                Array.Copy(fi.image16bitFilter[ind][y], frame[y], fi.sizeX);
                            }
                            newImage16[ind] = frame;
                        });
                        fi.image16bitFilter = newImage16;
                        break;
                }
        }
        public void ProcessFilter(TifFileInfo fi, int channel, float tolerance, int thresholdValue, DistanceTransformMethod distance)
        {
            //Apply in the history
            if (fi.newFilterHistory == null || fi.newFilterHistory.Length != fi.sizeC)
            {
                fi.isBinary = new bool[fi.sizeC];
                fi.newFilterHistory = new List<string>[fi.sizeC];
                for (int i = 0; i < fi.sizeC; i++)
                {
                    fi.newFilterHistory[i] = new List<string>();
                    fi.isBinary[i] = false;
                }
            }
            fi.isBinary[fi.cValue] = true;
            Filters.MyConvolution.CheckIsImagePrepared(fi);

            int[] indexes = new int[fi.imageCount / fi.sizeC];

            for (int i = channel, position = 0; i < fi.imageCount; i += fi.sizeC, position++)
                indexes[position] = i;
            
            foreach (int frame in indexes)
            {
                UnmanagedImage input = CreateImage(frame, fi, thresholdValue);

                BinaryWatershed bw = new BinaryWatershed(tolerance, distance);
                input = bw.Apply(input);

                ReturnCTImage(frame, fi, input);

            }
        }
        
        public string ToString(TifFileInfo fi, int channel, float tol, int thresholdValue, DistanceTransformMethod dist)
        {
            int distanceInd = 0;
            switch (dist)
            {
                //"Euclidean", "SquaredEuclidean", "Chessboard", "Manhattan"
                case DistanceTransformMethod.Euclidean:
                    distanceInd = 0;
                    break;
                case DistanceTransformMethod.SquaredEuclidean:
                    distanceInd = 1;
                    break;
                case DistanceTransformMethod.Chessboard:
                    distanceInd = 2;
                    break;
                case DistanceTransformMethod.Manhattan:
                    distanceInd = 3;
                    break;
            }
            
            return "wshed\t" + 
                channel + "\t" + 
                tol + "\t" + 
                thresholdValue + "\t" + 
                distanceInd + "\t" + 
                fi.fillHoles.ToString();
        }
        private bool checkChanel(TifFileInfo fi)
        {
            foreach (string str in fi.watershedList)
                if (int.Parse(str.Split(new string[] { "\t" }, StringSplitOptions.None)[0]) == fi.cValue)
                {
                    MessageBox.Show(
                        "Watershed is already applied to the current channel.\nPlease reset the filters!");
                    return true;
                }
            return false;
        }
        public void LoadFromString(TifFileInfo fi,string str)
        {
            string[] vals = str.Split(new string[] { "\t" }, StringSplitOptions.None);
            int channel = int.Parse(vals[0]);
            float tol = float.Parse(vals[1]);
            int thresholdValue = int.Parse(vals[2]);
            int distanceInd = int.Parse(vals[3]);

            //fill holes part
            bool fillHoles = false;
            if(vals.Length == 5)
                fillHoles = bool.Parse(vals[4]);
            fi.fillHoles = fillHoles;

            //distance type
            DistanceTransformMethod dist = DistanceTransformMethod.Euclidean;
            switch (distanceInd)
            {
                //"Euclidean", "SquaredEuclidean", "Chessboard", "Manhattan"
                case 0:
                    dist = DistanceTransformMethod.Euclidean;
                    break;
                case 1:
                    dist = DistanceTransformMethod.SquaredEuclidean;
                    break;
                case 2:
                    dist = DistanceTransformMethod.Chessboard;
                    break;
                case 3:
                    dist = DistanceTransformMethod.Manhattan;
                    break;
            }

            ProcessFilter(fi, channel, tol, thresholdValue, dist);
            fi.isBinary[channel] = true;
        }
        private UnmanagedImage CreateImage(int frame, TifFileInfo fi, int threshold)
        {
            byte[] image = new byte[fi.sizeX * fi.sizeY];
            int position = 0;

            byte zeroInd = 0;
            if (fi.fillHoles)
                zeroInd = 1;
            
            if (fi.bitsPerPixel == 8)
            {
                byte[][] oldImage = fi.image8bitFilter[frame];
                for (int y = 0; y < fi.sizeY; y++)
                {
                    image[position] = zeroInd;
                    position++;

                    for (int x = 1; x < fi.sizeX-1; x++, position++)
                    {
                        if (oldImage[y][x] < threshold)
                            image[position] = 0;
                        else
                            image[position] = 255;
                    }

                    image[position] = zeroInd;
                    position++;
                }
            }
            else if(fi.bitsPerPixel == 16)
            {
                ushort[][] oldImage = fi.image16bitFilter[frame];
                for (int y = 0; y < fi.sizeY; y++)
                {
                    image[position] = zeroInd;
                    position++;

                    for (int x = 1; x < fi.sizeX - 1; x++, position++)
                    {
                        if (oldImage[y][x] < threshold)
                            image[position] = 0;
                        else
                            image[position] = 255;
                    }

                    image[position] = zeroInd;
                    position++;
                }
            }
            //start fill holes
            if (zeroInd == 1)
                FillHoles(image, fi.sizeX);
            //return image
            return UnmanagedImage.FromByteArray(image,fi.sizeX,fi.sizeY,System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
        }
        private byte[] FillHoles(byte[] image, int w)
        {
            
            List<int> temp = new List<int>();
            //Check image size and return if it is less then 2 rows
            if (image.Length <= w + w) return image;
            //fill first row with zeroInd
            for (int i = 0; i < w; i++)
                image[i] = 1;
            //fill last row with zeroInd
            for (int i = image.Length - w; i < image.Length; i++)
                image[i] = 1;

            //up->down && left->right
            for (int i = w; i < image.Length - w; i++)
                if (image[i] == 0 &&
                    (image[i - 1] == 1 || image[i - w] == 1 ||
                    image[i + 1] == 1 || image[i + w] == 1))
                    image[i] = 1;
            //up<-down && left<-right
            for (int i = image.Length - w; i > w; i--)
                if (image[i] == 0 &&
                    (image[i + 1] == 1 || image[i + w] == 1 ||
                    image[i - 1] == 1 || image[i - w] == 1))
                    image[i] = 1;
                else if (image[i] == 0)
                    temp.Add(i);

            int count = temp.Count;
            while (count != 0)
            {
                count = 0;
                foreach (int i in temp)
                {
                    if (image[i] == 0 &&
                    (image[i - 1] == 1 || image[i - w] == 1 ||
                    image[i + 1] == 1 || image[i + w] == 1))
                    {
                        image[i] = 1;
                        count++;
                    }
                }
                for (int i = temp.Count - 1; i >= 0; i--)
                {
                    int val = temp[i];
                    if (image[val] == 0 &&
                    (image[val - 1] == 1 || image[val - w] == 1 ||
                    image[val + 1] == 1 || image[val + w] == 1))
                    {
                        image[val] = 1;
                        count++;
                        temp.RemoveAt(i);
                    }
                    else if (image[val] == 1)
                    {
                        temp.RemoveAt(i);
                    }
                }
            }

            //fill the holes and restore the image background
            for (int i = 0; i < image.Length; i++)
                if (image[i] == 1)
                    image[i] = 0;
                else if (image[i] == 0)
                    image[i] = 255;
            //return the image
            return image;
        }
        private void ReturnCTImage(int frame, TifFileInfo fi,UnmanagedImage image)
        {
            byte[] imageArr = image.ToByteArray();
            int position = 0;
            if (fi.bitsPerPixel == 8)
            {
                byte[][] oldImage = fi.image8bitFilter[frame];
                for (int y = 0; y < fi.sizeY; y++, position += fi.sizeX)
                {
                    Array.Copy(imageArr, position, oldImage[y], 0, fi.sizeX);
                }
            }
            else if (fi.bitsPerPixel == 16)
            {
                ushort[][] oldImage = fi.image16bitFilter[frame];
                for (int y = 0; y < fi.sizeY; y++)
                    for (int x = 0; x < fi.sizeX; x++,position++)
                        if (imageArr[position] == 0)
                            oldImage[y][x] = 0;
                        else
                            oldImage[y][x] = 16000;
            }
        }
        
    }
}
