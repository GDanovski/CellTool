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
    class Filters:Panel
    {
        private ImageAnalyser IA;
        private MenuStrip ConvolutionMenu = new MenuStrip();
        private MenuStrip BinaryMenu = new MenuStrip();
        private MenuStrip BinaryMenu1 = new MenuStrip();

        public Button ToBinaryBtn = new Button();
        public Button ResetBtn = new Button();
        public ToolStripMenuItem Box3 = new ToolStripMenuItem();
        public ToolStripMenuItem Box5 = new ToolStripMenuItem();
        public ToolStripMenuItem Gaus3 = new ToolStripMenuItem();
        public ToolStripMenuItem Gaus5 = new ToolStripMenuItem();
        public ToolStripMenuItem Med3 = new ToolStripMenuItem();
        public ToolStripMenuItem Med5 = new ToolStripMenuItem();
        public ToolStripMenuItem Sharp3 = new ToolStripMenuItem();
        public ToolStripMenuItem Sharp5 = new ToolStripMenuItem();
        public ToolStripMenuItem Unsharp5 = new ToolStripMenuItem();
        public ToolStripMenuItem Edge3 = new ToolStripMenuItem();
        public ToolStripMenuItem Grad3 = new ToolStripMenuItem();
        public ToolStripMenuItem Sobel3 = new ToolStripMenuItem();
        public ToolStripMenuItem Embos3 = new ToolStripMenuItem();
        public ToolStripMenuItem Embos5 = new ToolStripMenuItem();
        public ToolStripMenuItem Erode = new ToolStripMenuItem();
        public ToolStripMenuItem Dilate = new ToolStripMenuItem();
        public ToolStripMenuItem Open = new ToolStripMenuItem();
        public ToolStripMenuItem Close = new ToolStripMenuItem();
        public ToolStripMenuItem FillHoles = new ToolStripMenuItem();
        public ToolStripMenuItem Watershed = new ToolStripMenuItem();
        #region History
        public void addToHistoryOldInfo(int C, TifFileInfo fi)
        {
            string val = "newFilter(" + C + "\n;" + "Reset)";
            if (fi.newFilterHistory[C].Count != 0)
                val = "newFilter(" + C + "\n;" +
                string.Join("\n;", fi.newFilterHistory[C]) + ")";

            //prepare array and settings
            fi.delHist = true;
            IA.delHist = true;
            IA.UnDoBtn.Enabled = true;
            IA.DeleteFromHistory();
            //apply
            fi.History.Add(val);
        }
        public void addToHistoryNewInfo(int C, TifFileInfo fi)
        {
            string val = "newFilter(" + C + "\n;" + "Reset)";
            if (fi.newFilterHistory[C].Count!=0)
                val = "newFilter(" + C + "\n;" + 
                string.Join("\n;",fi.newFilterHistory[C]) + ")";
            //apply
            fi.History.Add(val);
            //finish changes
            IA.UpdateUndoBtns();
            IA.MarkAsNotSaved();
        }
        public void StartFromHistory(string str)
        {
            //apply to fi
            TifFileInfo fi = findFI();
            if (fi == null) return;

            string[] vals = str.Split(new string[] { "\n;" }, StringSplitOptions.None);
            int C = int.Parse(vals[0]);
            ////
            var bgw = new BackgroundWorker();
            bgw.WorkerReportsProgress = true;

            fi.available = false;
            bgw.DoWork += new DoWorkEventHandler(delegate (Object o, DoWorkEventArgs a)
            {
                try
                {
                    #region resetting
                    //if sizeC is changed or image is newly opened
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

                    int[] imageIndexes = MyConvolution.GetFramesArray(C, fi);

                    switch (fi.bitsPerPixel)
                    {
                        case 8:
                            //duplicate
                            Parallel.ForEach(imageIndexes, (ind) =>
                            {
                                byte[][] frame = new byte[fi.sizeY][];
                                for (int y = 0; y < fi.sizeY; y++)
                                {
                                    frame[y] = new byte[fi.sizeX];
                                    Array.Copy(fi.image8bit[ind][y], frame[y], fi.sizeX);
                                }
                                fi.image8bitFilter[ind] = frame;
                            });
                            break;
                        case 16:

                            Parallel.ForEach(imageIndexes, (ind) =>
                            {
                                ushort[][] frame = new ushort[fi.sizeY][];
                                for (int y = 0; y < fi.sizeY; y++)
                                {
                                    frame[y] = new ushort[fi.sizeX];
                                    Array.Copy(fi.image16bit[ind][y], frame[y], fi.sizeX);
                                }
                                fi.image16bitFilter[ind] = frame;
                            });
                            break;
                    }
                    #endregion resetting

                    if (vals[1] != "Reset")
                    {
                        List<string> l = vals.ToList();
                        l.RemoveAt(0);
                        fi.isBinary[C] = false;

                        foreach (string command in l)
                        {
                            IA.Segmentation.MyFilters.FilterFromString(command, fi);
                        }

                        fi.newFilterHistory[C] = l;
                    }

                    //report progress
                   ((BackgroundWorker)o).ReportProgress(0);
                }
                catch
                {
                    ((BackgroundWorker)o).ReportProgress(1);
                }
            });

            bgw.ProgressChanged += new ProgressChangedEventHandler(delegate (Object o, ProgressChangedEventArgs a)
            {
                if (a.ProgressPercentage == 0)
                {
                    //reload images to screen
                    IA.ReloadImages();
                    IA.MarkAsNotSaved();
                    IA.FileBrowser.StatusLabel.Text = "Ready";
                }
                else
                {
                    IA.FileBrowser.StatusLabel.Text = "Ready";
                }
                fi.available = true;
            });
            //Start background worker
            IA.FileBrowser.StatusLabel.Text = "Loading filters...";
            //start bgw
            bgw.RunWorkerAsync();
        }
        #endregion History
        public void LoadImageInfo(TifFileInfo fi)
        {
            //if sizeC is changed or image is newly opened
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
            //check
            if (fi.isBinary[fi.cValue])
            {
                if (ConvolutionMenu.Enabled)
                    ConvolutionMenu.Enabled = false;
                if (!BinaryMenu.Enabled)
                {
                    BinaryMenu.Enabled = true;
                    BinaryMenu1.Enabled = true;
                }
            }
            else
            {
                if (!ConvolutionMenu.Enabled)
                    ConvolutionMenu.Enabled = true;
                if (BinaryMenu.Enabled)
                {
                    BinaryMenu.Enabled = false;
                    BinaryMenu1.Enabled = false;
                }
            }
        }
        public string DecodeOldFilters(int C, int method)
        {
            string kernel = "";
            switch (method)
            {
                case 1:
                    //Sharpen
                   kernel = "0,-1,0," +
                   "-1,5,-1," +
                   "0,-1,0";
                    break;
                case 2:
                    //Box blur 3x3
                    kernel =
                         "1,1,1," +
                     "1,1,1," +
                     "1,1,1";
                    break;
                case 3:
                    //Box blur 5x5
                    kernel = "1,1,1,1,1," +
                   "1,1,1,1,1," +
                   "1,1,1,1,1," +
                   "1,1,1,1,1," +
                   "1,1,1,1,1";
                    break;
                case 4:
                    //Gaussian blur 3x3
                    kernel = "1,2,1," +
                    "2,4,2," +
                    "1,2,1";
                    break;
                case 5:
                    //Gaussian blur 5x5
                    kernel = "1,4,6,4,1," +
                 "4,16,24,16,4," +
                 "6,24,36,24,6," +
                 "4,16,24,16,4," +
                 "1,4,6,4,1";
                    break;
                case 6:
                    //Unsharp masking
                    kernel = "1,4,6,4,1," +
                 "4,16,24,16,4," +
                 "6,24,-476,24,6," +
                 "4,16,24,16,4," +
                 "1,4,6,4,1";
                    break;
                case 7:
                    //Edge detection bottom
                   kernel = "-1,-2,-1," +
                    "0,0,0," +
                    "1,2,1zz" +
                    "-1,0,1," +
                    "-2,0,2," +
                    "-1,0,1";
                    break;
            }

            return ConvolutionToString(C,kernel);
        }
        public Filters(ImageAnalyser IA)
        {
            this.IA = IA;
            this.SuspendLayout();

            this.BackColor = IA.FileBrowser.BackGround2Color1;
            this.ForeColor = IA.FileBrowser.ShriftColor1;
            this.Dock = DockStyle.Fill;

            #region restore panel
            Panel RestorePanel = new Panel();
            RestorePanel.BackColor = IA.FileBrowser.BackGround2Color1;
            RestorePanel.ForeColor = IA.FileBrowser.ShriftColor1;
            RestorePanel.Dock = DockStyle.Top;
            RestorePanel.Height = 35;
            this.Controls.Add(RestorePanel);
            
            {

                Button btn = ResetBtn;
                btn.Text = "Reset";
                btn.Width = 100;
                btn.FlatStyle = FlatStyle.Standard;
                btn.BackColor = SystemColors.ButtonFace;
                btn.ForeColor = Color.Black;
                RestorePanel.Controls.Add(btn);
                btn.Location = new Point(10, 7);
                btn.Click += new EventHandler(delegate (object o, EventArgs a) 
                {
                    //apply to fi
                    TifFileInfo fi = findFI();
                    if (fi == null) return;

                    //if sizeC is changed or image is newly opened
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

                    if (fi.newFilterHistory[fi.cValue].Count == 0) return;

                    int[] imageIndexes = MyConvolution.GetFramesArray(fi.cValue, fi);
                    
                    switch (fi.bitsPerPixel)
                    {
                        case 8:
                            //duplicate
                            Parallel.ForEach(imageIndexes, (ind) =>
                            {
                                byte[][] frame = new byte[fi.sizeY][];
                                for (int y = 0; y < fi.sizeY; y++)
                                {
                                    frame[y] = new byte[fi.sizeX];
                                    Array.Copy(fi.image8bit[ind][y], frame[y], fi.sizeX);
                                }
                                fi.image8bitFilter[ind] = frame;
                            });
                            break;
                        case 16:
                            
                            Parallel.ForEach(imageIndexes, (ind) =>
                            {
                                ushort[][] frame = new ushort[fi.sizeY][];
                                for (int y = 0; y < fi.sizeY; y++)
                                {
                                    frame[y] = new ushort[fi.sizeX];
                                    Array.Copy(fi.image16bit[ind][y], frame[y], fi.sizeX);
                                }
                                fi.image16bitFilter[ind] = frame;
                            });
                            break;
                    }

                    fi.isBinary[fi.cValue] = false;

                    addToHistoryOldInfo(fi.cValue, fi);
                    fi.newFilterHistory[fi.cValue].Clear();
                    addToHistoryNewInfo(fi.cValue, fi);

                    IA.MarkAsNotSaved();
                    IA.ReloadImages();
                });
            }
            {
                Button btn = ToBinaryBtn;
                btn.Text = "To Binary";
                btn.Width = 100;
                btn.FlatStyle = FlatStyle.Standard;
                btn.BackColor = SystemColors.ButtonFace;
                btn.ForeColor = Color.Black;
                RestorePanel.Controls.Add(btn);
                btn.Location = new Point(115, 7);
                btn.Click += ToBinary_Click;
            }
                #endregion restore panel

                #region Convolutions

            Panel ConvolutionTitlePanel = new Panel();
            ConvolutionTitlePanel.BackColor = IA.FileBrowser.BackGroundColor1;
            ConvolutionTitlePanel.ForeColor = IA.FileBrowser.ShriftColor1;
            ConvolutionTitlePanel.Dock = DockStyle.Top;
            ConvolutionTitlePanel.Height = 20;
            this.Controls.Add(ConvolutionTitlePanel);

            Label ConvolutionTitleLabel = new Label();
            ConvolutionTitleLabel.Text = "Convolutions:";
            ConvolutionTitleLabel.Location = new Point(5, 2);
            ConvolutionTitlePanel.Controls.Add(ConvolutionTitleLabel);

            //ConvolutionMenu.BackColor = IA.FileBrowser.BackGround2Color1;
            //ConvolutionMenu.ForeColor = IA.FileBrowser.ShriftColor1;
            ConvolutionMenu.CanOverflow = true;
            this.Controls.Add(ConvolutionMenu);

            ToolStripItem current = null;

            //Blurs
            ToolStripMenuItem BlurMI = new ToolStripMenuItem();
            BlurMI.Text = "Smooth";
            BlurMI.Overflow = ToolStripItemOverflow.AsNeeded;
            BlurMI.DropDownOpened += new EventHandler(menuItem_Opened);
            BlurMI.DropDownClosed += new EventHandler(menuItem_Closed);
            ConvolutionMenu.Items.Add(BlurMI);

            current = Box3;
            {
                current.Text = "3x3 Box blur";
                current.Tag = 
                    "1,1,1," +
                    "1,1,1," +
                    "1,1,1";

                current.Click += ConvolutionBtn_Click;
                BlurMI.DropDownItems.Add(current);                
            }

            current = Box5;
            {
                current.Text = "5x5 Box blur";
                current.Tag =
                   "1,1,1,1,1," +
                   "1,1,1,1,1," +
                   "1,1,1,1,1," +
                   "1,1,1,1,1," +
                   "1,1,1,1,1";

                current.Click += ConvolutionBtn_Click;
                BlurMI.DropDownItems.Add(current);
            }

            current =Gaus3;
            {
                current.Text = "3x3 Gaussian blur";
                current.Tag =
                    "1,2,1," +
                    "2,4,2," +
                    "1,2,1";

                current.Click += ConvolutionBtn_Click;
                BlurMI.DropDownItems.Add(current);
            }

            current = Gaus5;
            {
                current.Text = "5x5 Gaussian blur";
                current.Tag =
                 "1,4,6,4,1," +
                 "4,16,24,16,4," +
                 "6,24,36,24,6," +
                 "4,16,24,16,4," +
                 "1,4,6,4,1";

                current.Click += ConvolutionBtn_Click;
                BlurMI.DropDownItems.Add(current);
            }

            current = Med3;
            {
                current.Text = "3x3 Median";
                current.Tag = 1;
                current.Click += Median_Click;
                BlurMI.DropDownItems.Add(current);
            }
            current = Med5;
            {
                current.Text = "5x5 Median";
                current.Tag = 2;
                current.Click += Median_Click;
                BlurMI.DropDownItems.Add(current);
            }
            //Sharpen
            ToolStripMenuItem SharpenMI = new ToolStripMenuItem();
            SharpenMI.Text = "Sharpen";
            SharpenMI.Overflow = ToolStripItemOverflow.AsNeeded;
            SharpenMI.DropDownOpened += new EventHandler(menuItem_Opened);
            SharpenMI.DropDownClosed += new EventHandler(menuItem_Closed);
            ConvolutionMenu.Items.Add(SharpenMI);

            current = Sharp3;
            {
                current.Text = "3x3 Sharpen";
                current.Tag =
                   "0,-1,0," +
                   "-1,5,-1," +
                   "0,-1,0";

                current.Click += ConvolutionBtn_Click;
                SharpenMI.DropDownItems.Add(current);
            }

            current = Sharp5;
            {
                current.Text = "5x5 Sharpen";
                current.Tag =
                  "-1,-1,-1,-1,-1," +
                  "-1,2,2,2,-1," +
                  "-1,2,8,2,-1," +
                  "-1,2,2,2,-1," +
                  "-1,-1,-1,-1,-1";

                current.Click += ConvolutionBtn_Click;
                SharpenMI.DropDownItems.Add(current);
            }

            current = Unsharp5;
            {
                current.Text = "5x5 Unsharp masking";
                current.Tag =
                 "1,4,6,4,1," +
                 "4,16,24,16,4," +
                 "6,24,-476,24,6," +
                 "4,16,24,16,4," +
                 "1,4,6,4,1";

                current.Click += ConvolutionBtn_Click;
                SharpenMI.DropDownItems.Add(current);
            }

            //Gradient
            ToolStripMenuItem GradientMI = new ToolStripMenuItem();
            GradientMI.Text = "Gradient";
            GradientMI.Overflow = ToolStripItemOverflow.AsNeeded;
            GradientMI.DropDownOpened += new EventHandler(menuItem_Opened);
            GradientMI.DropDownClosed += new EventHandler(menuItem_Closed);
            ConvolutionMenu.Items.Add(GradientMI);

            current = Edge3;
            {
                current.Text = "3x3 Edge detection";
                current.Tag =
                   "-1,-1,-1," +
                   "-1,8,-1," +
                   "-1,-1,-1";

                current.Click += ConvolutionBtn_Click;
                GradientMI.DropDownItems.Add(current);
            }

            current = Grad3;
            {
                current.Text = "3x3 Gradient detection";
                current.Tag =
                    "-1,-1,-1," +
                    "0,0,0," +
                    "1,1,1zz" +
                    "-1,0,1," +
                    "-1,0,1," +
                    "-1,0,1";

                current.Click += ConvolutionBtn_Click;
                GradientMI.DropDownItems.Add(current);
            }

            current = Sobel3;
            {
                current.Text = "3x3 Sobel operator";
                current.Tag =
                    "-1,-2,-1," +
                    "0,0,0," +
                    "1,2,1zz" +
                    "-1,0,1," +
                    "-2,0,2," +
                    "-1,0,1";

                current.Click += ConvolutionBtn_Click;
                GradientMI.DropDownItems.Add(current);
            }

            current = Embos3;
            {
                current.Text = "3x3 Embos";
                current.Tag =
                   "-1,-1,0," +
                   "-1,0,1," +
                   "0,1,1zz"+ 
                   "0,1,1," +
                   "-1,0,1," +
                   "-1,-1,0";

                current.Click += ConvolutionBtn_Click;
                GradientMI.DropDownItems.Add(current);
            }

            current = Embos5;
            {
                current.Text = "5x5 Embos";
                current.Tag =
                   "-1,-1,-1,-1,0," +
                   "-1,-1,-1,0,1," +
                   "-1,-1,0,1,1," +
                   "-1,0,1,1,1," +
                   "0,1,1,1,1zz"+                    
                   "0,-1,-1,-1,-1," +
                   "1,0,-1,-1,-1," +
                   "1,1,0,-1,-1," +
                   "1,1,1,0,-1," +
                   "1,1,1,1,0";


                current.Click += ConvolutionBtn_Click;
                GradientMI.DropDownItems.Add(current);
            }

            //Gradient
            ToolStripMenuItem OthersMI = new ToolStripMenuItem();
            OthersMI.Text = "Others";
            OthersMI.Overflow = ToolStripItemOverflow.AsNeeded;
            OthersMI.DropDownOpened += new EventHandler(menuItem_Opened);
            OthersMI.DropDownClosed += new EventHandler(menuItem_Closed);
            //ConvolutionMenu.Items.Add(OthersMI);
            
            #endregion Convolutions

            #region Binary operations

            Panel BinaryTitlePanel = new Panel();
            BinaryTitlePanel.BackColor = IA.FileBrowser.BackGroundColor1;
            BinaryTitlePanel.ForeColor = IA.FileBrowser.ShriftColor1;
            BinaryTitlePanel.Height = 20;
            BinaryTitlePanel.Dock = DockStyle.Top;
            this.Controls.Add(BinaryTitlePanel);

            Label BinaryTitleLabel = new Label();
            BinaryTitleLabel.Text = "Binary operations:";
            BinaryTitleLabel.Location = new Point(5, 2);
            BinaryTitlePanel.Controls.Add(BinaryTitleLabel);

            BinaryMenu.CanOverflow = true;
            //BinaryMenu.BackColor = IA.FileBrowser.BackGround2Color1;
            //BinaryMenu.ForeColor = IA.FileBrowser.ShriftColor1;
            this.Controls.Add(BinaryMenu);

            BinaryMenu1.CanOverflow = true;
            //BinaryMenu1.BackColor = IA.FileBrowser.BackGround2Color1;
            //BinaryMenu1.ForeColor = IA.FileBrowser.ShriftColor1;
            this.Controls.Add(BinaryMenu1);

            current = Erode;
            {
                current.Overflow = ToolStripItemOverflow.AsNeeded;
                current.Text = "Erode";
                current.Click += Erode_Click;
                BinaryMenu.Items.Add(current);
            }

            current = Dilate;
            {
                current.Overflow = ToolStripItemOverflow.AsNeeded;
                current.Text = "Dilate";
                current.Click += Dilate_Click;
                BinaryMenu.Items.Add(current);
            }

            current = Open;
            {
                current.Overflow = ToolStripItemOverflow.AsNeeded;
                current.Text = "Open";
                current.Click += Open_Click;
                BinaryMenu.Items.Add(current);
            }

            current = Close;
            {
                current.Overflow = ToolStripItemOverflow.AsNeeded;
                current.Text = "Close";
                current.Click += Close_Click;
                BinaryMenu.Items.Add(current);
            }

            current = FillHoles;
            {
                current.Overflow = ToolStripItemOverflow.AsNeeded;
                current.Text = "Fill Holes";
                current.Click += FillHoles_Click;
               BinaryMenu1.Items.Add(current);
            }

            current = Watershed;
            {
                current.Overflow = ToolStripItemOverflow.AsNeeded;
                current.Text = "Watershed";
                current.Click += Watershed_Click;
                BinaryMenu1.Items.Add(current);
            }
            #endregion Binary operations

            #region order
            BinaryMenu1.SendToBack();
            BinaryMenu.SendToBack();
            BinaryTitlePanel.SendToBack();
            ConvolutionMenu.SendToBack();
            ConvolutionTitlePanel.SendToBack();
            RestorePanel.SendToBack();
            #endregion order

            // Automatically resize each panel to fit the components inside
            ConvolutionTitleLabel.AutoSize = true;
            BinaryTitleLabel.AutoSize = true;
            BinaryMenu1.AutoSize = true;
            BinaryMenu.AutoSize = true;
            //BinaryTitlePanel.AutoSize = true;
            ConvolutionMenu.AutoSize = true;
            //ConvolutionTitlePanel.AutoSize = true;
            RestorePanel.AutoSize = true;
           
            foreach (ToolStripMenuItem menu_item in ConvolutionMenu.Items)
            {
                menu_item.AutoSize = true;
                menu_item.Font = new Font(ConvolutionTitleLabel.Font.FontFamily, 9.0f);
            }

            foreach (ToolStripMenuItem menu_item in BinaryMenu.Items) {
                menu_item.AutoSize = true;
                menu_item.Font = new Font(ConvolutionTitleLabel.Font.FontFamily, 9.0f);
            }
            foreach (ToolStripMenuItem menu_item in BinaryMenu1.Items)
            {
                menu_item.AutoSize = true;
                menu_item.Font = new Font(ConvolutionTitleLabel.Font.FontFamily, 9.0f);
            }


            this.ResumeLayout();
        }
        
        #region History
        public string ConvolutionToString(int C, string kernelStr)
        {
            return "convolution\t" + C + "\t" + kernelStr;
        }
        public string MedianToString(int C, int rad)
        {
            return "Median\t" + C + "\t" + rad;
        }
        public string ErodeToString(int C)
        {
            return "Erode\t" + C;
        }
        public string DilateToString(int C)
        {
            return "Dilate\t" + C;
        }
        public string OpenToString(int C)
        {
            return "Open\t" + C;
        }
        public string CloseToString(int C)
        {
            return "Close\t" + C;
        }
        public string FillHolesToString(int C)
        {
            return "FillHoles\t" + C;
        }
        private string ToBinaryToString(Color selectedCol, TifFileInfo fi, int C, ImageAnalyser IA
               , int[] thresholdValues, Color[] thresholdColors, int thresholds, Color SpotColor,
                int SelectedSpotThresh, string SpotTailType, int spotSensitivity, int SpotThresh, int typeSpotThresh)
        {
            List<string> vals = new List<string>();

            vals.Add("ToBinary");
            vals.Add(ColorTranslator.ToHtml(selectedCol));
            vals.Add(C.ToString());
            vals.Add(string.Join(",", thresholdValues));

            List<string> cols = new List<string>();
            foreach (Color col in thresholdColors)
                cols.Add( ColorTranslator.ToHtml(col));

            vals.Add(string.Join(",", cols));
            vals.Add(thresholds.ToString());
            vals.Add(ColorTranslator.ToHtml(SpotColor));
            vals.Add(SelectedSpotThresh.ToString());
            vals.Add(SpotTailType);
            vals.Add(spotSensitivity.ToString());
            vals.Add(SpotThresh.ToString());
            vals.Add(typeSpotThresh.ToString());

            return string.Join("\t",vals);
        }
        public void FilterFromString(string str, TifFileInfo fi)
        {
            try
            {
                //apply to fi
                if (fi == null) return;

                string[] vals = str.Split(new string[] { "\t" }, StringSplitOptions.None);

                switch (vals[0])
                {
                    case "Median":
                        if(vals.Length == 2)
                            MyConvolution.Median(int.Parse(vals[1]),1, fi);
                        else
                            MyConvolution.Median(int.Parse(vals[1]), int.Parse(vals[2]), fi);
                        break;
                    case "Close":
                        MyConvolution.Dilate(int.Parse(vals[1]), fi);
                        MyConvolution.Erode(int.Parse(vals[1]), fi);
                        break;
                    case "Open":
                        MyConvolution.Erode(int.Parse(vals[1]), fi);
                        MyConvolution.Dilate(int.Parse(vals[1]), fi);
                        break;
                    case "Dilate":
                        MyConvolution.Dilate(int.Parse(vals[1]), fi);
                        break;
                    case "Erode":
                        MyConvolution.Erode(int.Parse(vals[1]), fi);
                        break;
                    case "convolution":
                        string[] kernelStrings = vals[2].Split(
                        new string[] { "zz" }, StringSplitOptions.None);

                        switch (kernelStrings.Length)
                        {
                            case 2:
                                MyConvolution.SobelOperation(
                                    int.Parse(vals[1])
                                    , fi, kernelStrings[0], kernelStrings[1]);
                                break;
                            default:
                                MyConvolution.SmoothImage(
                                     int.Parse(vals[1])
                                     , fi, kernelStrings[0]);
                                break;
                        }
                        break;
                    case "FillHoles":
                        int C = int.Parse(vals[1]);
                        MyConvolution.FillHoles(C, fi);
                        break;
                    case "ToBinary":
                        ToBinaryFromHistory(vals, fi);
                        break;
                    case "wshed":
                        IA.Segmentation.Watershed.LoadFromString(fi, str.Replace("wshed\t", ""));
                        
                        break;
                }
            }
            catch { }
        }
        #endregion History
        public void ConvolutionBtn_Click(object sender, EventArgs e)
        {
            //apply to fi
            TifFileInfo fi = findFI();
            if (fi == null) return;

            string[] kernelStrings = ((string)((ToolStripMenuItem)sender).Tag).Split(
                new string[] { "zz" }, StringSplitOptions.None);

            ApplyFilter(fi, kernelStrings);

        }
        public BackgroundWorker ApplyFilter(TifFileInfo fi, string[] kernelStrings)
        {
            int C = fi.cValue;
            var bgw = new BackgroundWorker();
            bgw.WorkerReportsProgress = true;
            
            fi.available = false;
            bgw.DoWork += new DoWorkEventHandler(delegate (Object o, DoWorkEventArgs a)
            {
                try
                {
                    switch (kernelStrings.Length)
                    {
                        case 2:
                            MyConvolution.SobelOperation(
                                C, fi, kernelStrings[0], kernelStrings[1]);
                            break;
                        default:
                            MyConvolution.SmoothImage(
                                C,fi, kernelStrings[0]);
                            break;
                    }
                    //report progress
                   ((BackgroundWorker)o).ReportProgress(0);
                }
                catch
                {
                    ((BackgroundWorker)o).ReportProgress(1);
                }
            });

            bgw.ProgressChanged += new ProgressChangedEventHandler(delegate (Object o, ProgressChangedEventArgs a)
            {
                if (a.ProgressPercentage == 0)
                {
                    //add to history
                    addToHistoryOldInfo(C, fi);
                    fi.newFilterHistory[C].Add(
                        ConvolutionToString(C, string.Join("zz", kernelStrings)));
                    addToHistoryNewInfo(C, fi);
                    //reload images to screen
                    IA.ReloadImages();
                    IA.MarkAsNotSaved();
                    IA.FileBrowser.StatusLabel.Text = "Ready";
                }
                else
                {
                    IA.FileBrowser.StatusLabel.Text = "Ready";
                }
                fi.available = true;
            });
            //Start background worker
            IA.FileBrowser.StatusLabel.Text = "Calculating image...";
            //start bgw
            bgw.RunWorkerAsync();

            return bgw;
        }
        private void Median_Click(object sender, EventArgs e)
        {
            //apply to fi
            TifFileInfo fi = findFI();
            if (fi == null) return;

            int rad = (int)((ToolStripMenuItem)sender).Tag;
            //if sizeC is changed or image is newly opened
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

            if (fi.isBinary[fi.cValue])
            {
                MessageBox.Show("Image is binary!");
                return;
            }

            int C = fi.cValue;
            
            //background worker
            var bgw = new BackgroundWorker();
            bgw.WorkerReportsProgress = true;
            fi.available = false;

            //Add event for projection here
            bgw.DoWork += new DoWorkEventHandler(delegate (Object o, DoWorkEventArgs a)
            {
                MyConvolution.Median(C, rad, fi);
                ((BackgroundWorker)o).ReportProgress(0);
            });

            bgw.ProgressChanged += new ProgressChangedEventHandler(delegate (Object o, ProgressChangedEventArgs a)
            {
                if (a.ProgressPercentage == 0)
                {
                    //add to history
                    addToHistoryOldInfo(C, fi);
                    fi.newFilterHistory[C].Add(
                    MedianToString(C,rad));
                    addToHistoryNewInfo(C, fi);
                    //reload images to screen
                    IA.MarkAsNotSaved();
                }
                IA.ReloadImages();
                fi.available = true;
                IA.FileBrowser.StatusLabel.Text = "Ready";
            });
            //Start background worker
            IA.FileBrowser.StatusLabel.Text = "Convolution...";
            //start bgw
            bgw.RunWorkerAsync();
        }
        private void menuItem_Opened(object sender, EventArgs e)
        {
            ToolStripMenuItem item1 = sender as ToolStripMenuItem;
           // item1.ForeColor = Color.Black;
        }
        private void menuItem_Closed(object sender, EventArgs e)
        {
            ToolStripMenuItem item1 = sender as ToolStripMenuItem;
            //item1.ForeColor = IA.FileBrowser.ShriftColor1;
        }

        public TifFileInfo findFI()
        {
            TifFileInfo fi = null;
            try
            {
                fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
            }
            catch { return null; }
            //Calculate zoom
            if (fi == null) return null;

            if (fi.available == false)
            {
                MessageBox.Show("Image is not avaliable yet!");
                return null;
            }

            return fi;
        }
        #region Binary
        private void ToBinary_Click(object sender, EventArgs e)
        {
            //apply to fi
            TifFileInfo fi = findFI();
            if (fi == null) return;

            //if sizeC is changed or image is newly opened
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

            if (fi.isBinary[fi.cValue])
            {
                MessageBox.Show("Image is binary!");
                return;
            }
            
            if (fi.SegmentationCBoxIndex[fi.cValue] == 0 &&
            fi.SelectedSpotThresh[fi.cValue] == 0)
            {
                MessageBox.Show("The image must be segmented!");
                return;
            }
            #region Create Variables
            int C = fi.cValue;
            int thresholds = fi.thresholds[C];
            if (fi.SegmentationCBoxIndex[C] == 0) thresholds = 0;
            ImageAnalyser IA = this.IA;
            int[] thresholdValues = fi.thresholdValues[C];
            Color[] thresholdColors = fi.thresholdColors[C];
            Color SpotColor = fi.SpotColor[C];
            int SelectedSpotThresh = fi.SelectedSpotThresh[C];
            string SpotTailType = fi.SpotTailType[C];
            int spotSensitivity = fi.spotSensitivity[C];
            int SpotThresh = fi.SpotThresh[C];
            int typeSpotThresh = fi.typeSpotThresh[C];
            #endregion Create Variables

            List<Color> colorL = new List<Color>();
            for(int i = 0; i <= thresholds; i++)
                if(thresholdColors[i] != Color.Transparent &&
                    colorL.IndexOf(thresholdColors[i])==-1)
                    colorL.Add(thresholdColors[i]);
            if (SelectedSpotThresh != 0 && colorL.IndexOf(SpotColor) == -1)
                colorL.Add(SpotColor);

            if(colorL.Count == 0)
            {
                MessageBox.Show("The image must be segmented!");
                return;
            }

            Form binaryForm = BinaryDialog(colorL);

            // Linux change
            IA.FileBrowser.StatusLabel.Text = "Dialog open";
            binaryForm.ShowDialog();
            IA.FileBrowser.StatusLabel.Text = "Ready";

            Color selectedCol = (Color)binaryForm.Tag;

            binaryForm.Dispose();

            if (selectedCol==Color.Empty)
            {
                MessageBox.Show("Foreground is not selected!");
                return;
            }
            
            //background worker
            var bgw = new BackgroundWorker();
            bgw.WorkerReportsProgress = true;
            fi.available = false;
            //Add event for projection here
            bgw.DoWork += new DoWorkEventHandler(delegate (Object o, DoWorkEventArgs a)
            {
                MyConvolution.ToBinary(selectedCol, fi, C, IA
               , thresholdValues, thresholdColors,thresholds,  SpotColor,
                SelectedSpotThresh,SpotTailType,spotSensitivity,  SpotThresh,
                typeSpotThresh);

                ((BackgroundWorker)o).ReportProgress(0);
            });

            bgw.ProgressChanged += new ProgressChangedEventHandler(delegate (Object o, ProgressChangedEventArgs a)
            {
                if (a.ProgressPercentage == 0)
                {
                    //add to history
                    addToHistoryOldInfo(C, fi);

                    fi.newFilterHistory[C].Add(
                        ToBinaryToString(selectedCol, fi, C, IA
               , thresholdValues, thresholdColors, thresholds, SpotColor,
                SelectedSpotThresh, SpotTailType, spotSensitivity, SpotThresh,
                typeSpotThresh));

                    addToHistoryNewInfo(C, fi);
                    //reload images to screen
                    IA.MarkAsNotSaved();
                }
                IA.ReloadImages();
                fi.available = true;
                IA.FileBrowser.StatusLabel.Text = "Ready";
            });
            //Start background worker
            IA.FileBrowser.StatusLabel.Text = "Binary action...";
            //start bgw
            bgw.RunWorkerAsync();
        }
        private void ToBinaryFromHistory(string[] vals, TifFileInfo fi)
        {
            
            #region Create Variables
            Color selectedCol = ColorTranslator.FromHtml(vals[1]);
            int C = int.Parse(vals[2]);
            int thresholds = int.Parse(vals[5]);
            ImageAnalyser IA = this.IA;

            List<int> thresholdValuesList = new List<int>();
            foreach (string str in vals[3].Split(
                new string[] { "," }, StringSplitOptions.None))
                thresholdValuesList.Add(int.Parse(str));

            int[] thresholdValues = thresholdValuesList.ToArray();

            List<Color> thresholdColorsList = new List<Color>();
            foreach (string str in vals[4].Split(
                new string[] { "," }, StringSplitOptions.None))
                thresholdColorsList.Add(ColorTranslator.FromHtml(str));

            Color[] thresholdColors = thresholdColorsList.ToArray();
            Color SpotColor = ColorTranslator.FromHtml(vals[6]);

            int SelectedSpotThresh = int.Parse(vals[7]);
            string SpotTailType = vals[8];
            int spotSensitivity = int.Parse(vals[9]);
            int SpotThresh = int.Parse(vals[10]);
            int typeSpotThresh = int.Parse(vals[11]);
            #endregion Create Variables

            MyConvolution.ToBinary(selectedCol, fi, C, IA
               , thresholdValues, thresholdColors, thresholds, SpotColor,
                SelectedSpotThresh, SpotTailType, spotSensitivity, SpotThresh,
                typeSpotThresh);

            fi.isBinary[C] = true;
        }

        private Form BinaryDialog(List<Color> colorL)
        {
            Form Dialog = new Form();
            Dialog.Tag = Color.Empty;
            Dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
            Dialog.Text = "To Binary";
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
                okBtn.Location = new System.Drawing.Point( W, 40);
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
        private void Dilate_Click(object sender, EventArgs e)
        {
            //apply to fi
            TifFileInfo fi = findFI();
            if (fi == null) return;

            //if sizeC is changed or image is newly opened
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

            if (!fi.isBinary[fi.cValue])
            {
                MessageBox.Show("Image is not binary!");
                return;
            }

            int C = fi.cValue;

            //background worker
            var bgw = new BackgroundWorker();
            bgw.WorkerReportsProgress = true;
            fi.available = false;

            //Add event for projection here
            bgw.DoWork += new DoWorkEventHandler(delegate (Object o, DoWorkEventArgs a)
            {
                MyConvolution.Dilate(C, fi);
                ((BackgroundWorker)o).ReportProgress(0);
            });

            bgw.ProgressChanged += new ProgressChangedEventHandler(delegate (Object o, ProgressChangedEventArgs a)
            {
                if (a.ProgressPercentage == 0)
                {
                    //add to history
                    addToHistoryOldInfo(C, fi);

                    fi.newFilterHistory[C].Add(
                    DilateToString(C));

                    addToHistoryNewInfo(C, fi);
                    //reload images to screen
                    IA.MarkAsNotSaved();
                }
                IA.ReloadImages();
                fi.available = true;
                IA.FileBrowser.StatusLabel.Text = "Ready";
            });
            //Start background worker
            IA.FileBrowser.StatusLabel.Text = "Binary action...";
            //start bgw
            bgw.RunWorkerAsync();
        }
        private void Erode_Click(object sender, EventArgs e)
        {
            //apply to fi
            TifFileInfo fi = findFI();
            if (fi == null) return;

            //if sizeC is changed or image is newly opened
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

            if (!fi.isBinary[fi.cValue])
            {
                MessageBox.Show("Image is not binary!");
                return;
            }

            int C = fi.cValue;
            
            //background worker
            var bgw = new BackgroundWorker();
            bgw.WorkerReportsProgress = true;
            fi.available = false;

            //Add event for projection here
            bgw.DoWork += new DoWorkEventHandler(delegate (Object o, DoWorkEventArgs a)
            {
                MyConvolution.Erode(C, fi);
                ((BackgroundWorker)o).ReportProgress(0);
            });

            bgw.ProgressChanged += new ProgressChangedEventHandler(delegate (Object o, ProgressChangedEventArgs a)
            {
                if (a.ProgressPercentage == 0)
                {
                    //add to history
                    addToHistoryOldInfo(C, fi);

                    fi.newFilterHistory[C].Add(
                        ErodeToString(C));

                    addToHistoryNewInfo(C, fi);
                    //reload images to screen
                    IA.MarkAsNotSaved();
                }
                IA.ReloadImages();
                fi.available = true;
                IA.FileBrowser.StatusLabel.Text = "Ready";
            });
            //Start background worker
            IA.FileBrowser.StatusLabel.Text = "Binary action...";
            //start bgw
            bgw.RunWorkerAsync();
        }
        private void Open_Click(object sender, EventArgs e)
        {
            //apply to fi
            TifFileInfo fi = findFI();
            if (fi == null) return;

            //if sizeC is changed or image is newly opened
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

            if (!fi.isBinary[fi.cValue])
            {
                MessageBox.Show("Image is not binary!");
                return;
            }

            int C = fi.cValue;

            //background worker
            var bgw = new BackgroundWorker();
            bgw.WorkerReportsProgress = true;
            fi.available = false;

            //Add event for projection here
            bgw.DoWork += new DoWorkEventHandler(delegate (Object o, DoWorkEventArgs a)
            {
                MyConvolution.Erode(C, fi);
                MyConvolution.Dilate(C, fi);
                ((BackgroundWorker)o).ReportProgress(0);
            });

            bgw.ProgressChanged += new ProgressChangedEventHandler(delegate (Object o, ProgressChangedEventArgs a)
            {
                if (a.ProgressPercentage == 0)
                {
                    //add to history
                    addToHistoryOldInfo(C, fi);

                    fi.newFilterHistory[C].Add(
                    OpenToString(C));

                    addToHistoryNewInfo(C, fi);
                    //reload images to screen
                    IA.MarkAsNotSaved();
                }
                IA.ReloadImages();
                fi.available = true;
                IA.FileBrowser.StatusLabel.Text = "Ready";
            });
            //Start background worker
            IA.FileBrowser.StatusLabel.Text = "Binary action...";
            //start bgw
            bgw.RunWorkerAsync();
        }
        private void Close_Click(object sender, EventArgs e)
        {
            //apply to fi
            TifFileInfo fi = findFI();
            if (fi == null) return;

            //if sizeC is changed or image is newly opened
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

            if (!fi.isBinary[fi.cValue])
            {
                MessageBox.Show("Image is not binary!");
                return;
            }

            int C = fi.cValue;

            //background worker
            var bgw = new BackgroundWorker();
            bgw.WorkerReportsProgress = true;
            fi.available = false;

            //Add event for projection here
            bgw.DoWork += new DoWorkEventHandler(delegate (Object o, DoWorkEventArgs a)
            {
                MyConvolution.Dilate(C, fi);
                MyConvolution.Erode(C, fi);
                ((BackgroundWorker)o).ReportProgress(0);
            });

            bgw.ProgressChanged += new ProgressChangedEventHandler(delegate (Object o, ProgressChangedEventArgs a)
            {
                if (a.ProgressPercentage == 0)
                {
                    //add to history
                    addToHistoryOldInfo(C, fi);

                    fi.newFilterHistory[C].Add(
                    CloseToString(C));

                    addToHistoryNewInfo(C, fi);
                    //reload images to screen
                    IA.MarkAsNotSaved();
                }
                IA.ReloadImages();
                fi.available = true;
                IA.FileBrowser.StatusLabel.Text = "Ready";
            });
            //Start background worker
            IA.FileBrowser.StatusLabel.Text = "Binary action...";
            //start bgw
            bgw.RunWorkerAsync();
        }
        private void FillHoles_Click(object sender,EventArgs e)
        {
            //apply to fi
            TifFileInfo fi = findFI();
            if (fi == null) return;

            //if sizeC is changed or image is newly opened
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

            if (!fi.isBinary[fi.cValue])
            {
                MessageBox.Show("Image is not binary!");
                return;
            }

            int C = fi.cValue;
            //background worker
            var bgw = new BackgroundWorker();
            bgw.WorkerReportsProgress = true;
            fi.available = false;
            //Add event for projection here
            bgw.DoWork += new DoWorkEventHandler(delegate (Object o, DoWorkEventArgs a)
            {
                MyConvolution.FillHoles(C, fi);
                ((BackgroundWorker)o).ReportProgress(0);
            });

            bgw.ProgressChanged += new ProgressChangedEventHandler(delegate (Object o, ProgressChangedEventArgs a)
            {
                if (a.ProgressPercentage == 0)
                {
                    //add to history
                    addToHistoryOldInfo(C, fi);

                    fi.newFilterHistory[C].Add(
                        FillHolesToString(C));

                    addToHistoryNewInfo(C, fi);
                    //reload images to screen
                    IA.MarkAsNotSaved();
                }
                IA.ReloadImages();
                fi.available = true;
                IA.FileBrowser.StatusLabel.Text = "Ready";
            });
            //Start background worker
            IA.FileBrowser.StatusLabel.Text = "Binary action...";
            //start bgw
            bgw.RunWorkerAsync();
        }
        public void Watershed_Click(object sender, EventArgs e)
        {
            //apply to fi
            TifFileInfo fi = findFI();
            if (fi == null) return;
            //if sizeC is changed or image is newly opened
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

            if (!fi.isBinary[fi.cValue])
            {
                MessageBox.Show("Image is not binary!");
                return;
            }

            //Update Threshold panel
            MyWatershed _MyWatershed = IA.Segmentation.Watershed;
            _MyWatershed.ToleranceTB.Text = _MyWatershed.tolerance.ToString();
            _MyWatershed.FillHolesCB.Checked = fi.fillHoles;

            // Linux change
            IA.FileBrowser.StatusLabel.Text = "Dialog open";
            _MyWatershed.Dialog.ShowDialog();
            IA.FileBrowser.StatusLabel.Text = "Ready";
        }
        #endregion Binary
        public static class MyConvolution
        {
            #region kernels
            private static int[][] KernelMatrix(string input)
            {
                //calculates the kernel from string
                string[] kernelVals = input.Split(new string[] { "," }, StringSplitOptions.None);
                int size = (int)Math.Sqrt(kernelVals.Length);

                int[][] kernel = new int[size][];
                kernel[0] = new int[size];
                for (int position = 0,x=0,y=0; position<kernelVals.Length && y<size; position++,x++)
                {
                    if (x == size)
                    {
                        x = 0;
                        y++;
                        kernel[y] = new int[size];
                    }

                    kernel[y][x] = int.Parse(kernelVals[position]);
                }
                
                return kernel;
            }
            #endregion kernels

            #region global variables
           public static void CheckIsImagePrepared(TifFileInfo fi)
            {
                try
                {
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

                    switch (fi.bitsPerPixel)
                    {
                        case 8:
                            //return if the image is new
                            if (fi.image8bitFilter != null &&
                                fi.image8bitFilter != fi.image8bit)
                                return;
                            //duplicate
                            byte[][][] newImage8 = new byte[fi.imageCount][][];
                            Parallel.For(0, fi.imageCount, (ind) =>
                            {
                                byte[][] frame = new byte[fi.sizeY][];
                                for (int y = 0; y < fi.sizeY; y++)
                                {
                                    frame[y] = new byte[fi.sizeX];
                                    if (fi.image8bitFilter != null)
                                        Array.Copy(fi.image8bitFilter[ind][y], frame[y], fi.sizeX);
                                }
                                newImage8[ind] = frame;
                            });
                            fi.image8bitFilter = newImage8;

                            return;
                        case 16:
                            //return if the image is new
                            if (fi.image16bitFilter != null &&
                                fi.image16bitFilter != fi.image16bit)
                                return;
                            //duplicate
                            ushort[][][] newImage16 = new ushort[fi.imageCount][][];
                            Parallel.For(0, fi.imageCount, (ind) =>
                            {
                                ushort[][] frame = new ushort[fi.sizeY][];
                                for (int y = 0; y < fi.sizeY; y++)
                                {
                                    frame[y] = new ushort[fi.sizeX];
                                    if(fi.image16bitFilter!=null)
                                        Array.Copy(fi.image16bitFilter[ind][y], frame[y], fi.sizeX);
                                }
                                newImage16[ind] = frame;
                            });
                            fi.image16bitFilter = newImage16;
                            return;
                    }
                }
                catch { }
            }
        
            private static int KernelCoeficient(int[][] kernel)
            {
                int sum = 0;
                //calculate coefition = sum(all values in the table)
                foreach (int[] row in kernel)
                    foreach (int val in row)
                        sum += val;

                return sum;
            }
            private static int[] KernelPxlVal(int[][] kernel)
            {
                List<int> val = new List<int>();
                //check the table for non zero values and apply them to array
                for (int y = 0; y < kernel.Length; y++)
                    for (int x = 0; x < kernel[y].Length; x++)
                        if (kernel[y][x] != 0)
                            val.Add(kernel[y][x]);
                
                return val.ToArray();
            }
            private static Point[] KernelPxlCord(int[][] kernel)
            {
                List<Point> pixels = new List<Point>();
                int index = -(kernel.Length - 1) / 2;
                //check the table for non zero values and calculate the coordinates
                for (int y = 0; y < kernel.Length; y++)
                    for (int x = 0; x < kernel[y].Length; x++)
                        if (kernel[y][x] != 0)
                            pixels.Add(new Point(x + index, y + index));

                return pixels.ToArray();
            }
            private static Point[] MedianPoints(int rad)
            {
                List<Point> pixels = new List<Point>();
                int index = rad+rad+1;
                //calculate the coordinates
                for (int y = 0; y < index; y++)
                    for (int x = 0; x < index; x++)
                            pixels.Add(new Point(x -rad, y -rad));

                return pixels.ToArray();
            }
            private static Point[][][] ImagePxlMatrix(Point[] pxlCords, TifFileInfo fi, int[][] kernel)
            {
                //prepare matrix
                Point[][][] ImagePxlMatr = new Point[fi.sizeY][][];
                //fill the matrix with lists of neighbours
                Parallel.For(0, fi.sizeY, y =>
                {
                    Point[][] Row = new Point[fi.sizeX][];
                    for (int x = 0; x < fi.sizeX; x++)
                    {
                        Point[] pList = new Point[pxlCords.Length];
                        for (int i = 0; i < pxlCords.Length; i++)
                        {
                            pList[i].X = pxlCords[i].X + x;
                            pList[i].Y = pxlCords[i].Y + y;
                        }
                        Row[x] = pList;
                    }
                    ImagePxlMatr[y] = Row;
                });
                //fill the corners
                int CornerConst = ((kernel.Length - 1) / 2);
                int w = ImagePxlMatr[0].Length;
                int h = ImagePxlMatr.Length;

                for (int i = 0; i < CornerConst; i++)
                {
                    //up rows
                    foreach (Point[] pList in ImagePxlMatr[i])
                        for (int countP = 0; countP < pList.Length; countP++)
                        {
                            Point p = pList[countP];
                            if (p.X < 0) { p.X = 0; }
                            if (p.Y < 0) { p.Y = 0; }
                            if (p.X >= w) { p.X = w - 1; }
                            if (p.Y >= h) { p.Y = h - 1; }
                            pList[countP] = p;
                        }
                    //down rows
                    foreach (Point[] pList in ImagePxlMatr[(h - 1) - i])
                        for (int countP = 0; countP < pList.Length; countP++)
                        {
                            Point p = pList[countP];
                            if (p.X < 0) { p.X = 0; }
                            if (p.Y < 0) { p.Y = 0; }
                            if (p.X >= w) { p.X = w - 1; }
                            if (p.Y >= h) { p.Y = h - 1; }
                            pList[countP] = p;
                        }
                    //columns
                    for (int y = 0; y < h; y++)
                    {
                        //left column
                        Point[] pList = ImagePxlMatr[y][i];

                        for (int countP = 0; countP < pList.Length; countP++)
                        {
                            Point p = pList[countP];
                            if (p.X < 0) { p.X = 0; }
                            if (p.Y < 0) { p.Y = 0; }
                            if (p.X >= w) { p.X = w - 1; }
                            if (p.Y >= h) { p.Y = h - 1; }
                            pList[countP] = p;
                        }
                        //right column

                        pList = ImagePxlMatr[y][(w - 1) - i];
                        for (int countP = 0; countP < pList.Length; countP++)
                        {
                            Point p = pList[countP];
                            if (p.X < 0) { p.X = 0; }
                            if (p.Y < 0) { p.Y = 0; }
                            if (p.X >= w) { p.X = w - 1; }
                            if (p.Y >= h) { p.Y = h - 1; }
                            pList[countP] = p;
                        }
                    }
                }
                //End
                return ImagePxlMatr;
            }
            public static int[] GetFramesArray(int C, TifFileInfo fi)
            {
                int[] indexes = new int[fi.imageCount / fi.sizeC];

                for (int i = C, position = 0; i < fi.imageCount; i += fi.sizeC, position++)
                    indexes[position] = i;

                return indexes;
            }
            #endregion global variables

            #region Smooth the image
            //source https://en.wikipedia.org/wiki/Kernel_(image_processing)
            public static void SmoothImage(int C, TifFileInfo fi,string inKernel)
            {
                CheckIsImagePrepared(fi);
                //return if image is binary
                if (fi.isBinary[C]) return;
                //choose kernel table
                int[][] kernel = KernelMatrix(inKernel);
                //find the deviding coeficient
                int coeficient = KernelCoeficient(kernel);
                //prepare array with exact coordinates for the kernel table mumbers with val
                Point[] pxlCords = KernelPxlCord(kernel);
                //prepare array with the values != 0 in the kernel table
                int[] pxlVals = KernelPxlVal(kernel);
                //prepare shablon image matrix with all exact coordinates for sum
                Point[][][] ImagePxlMatr = ImagePxlMatrix(pxlCords, fi, kernel);

                switch (fi.bitsPerPixel)
                {
                    case 8:
                        byte[][][] image8bit = SmoothAllStack8bit(C, ImagePxlMatr, fi, pxlVals, coeficient);
                        fi.image8bitFilter = image8bit;
                        break;
                    case 16:
                        ushort[][][] image16bit = SmoothAllStack16bit(C, ImagePxlMatr, fi, pxlVals, coeficient);
                        fi.image16bitFilter = image16bit;
                        break;
                }
            }
            private static ushort[][][] SmoothAllStack16bit(int C,Point[][][] ImagePxlMatr, TifFileInfo fi, int[] pxlVals, double coeficient)
            {
                ushort[][][] image = fi.image16bitFilter;//source image
                ushort maxVal = ushort.MaxValue; //16 bit image max intensity
                double MinValue = ushort.MinValue; //0

                int[] imageIndexes = GetFramesArray(C, fi);
                Parallel.ForEach(imageIndexes, frame =>
                {
                    ushort[][] selectedImage = new ushort[fi.sizeY][];
                    ushort[][] origSelectedImage = image[frame];

                    for (int y = 0; y < fi.sizeY; y++)
                    {
                        ushort[] row = new ushort[fi.sizeX];
                        for (int x = 0; x < fi.sizeX; x++)
                        {
                            double val = 0;
                            Point[] plist = ImagePxlMatr[y][x];
                            //calculate value of the neighbors
                            for (int pInd = 0; pInd < plist.Length; pInd++)
                            {
                                Point p = plist[pInd];
                                val += ((double)origSelectedImage[p.Y][p.X] * (double)pxlVals[pInd]);
                            }

                            //normalize
                            if (coeficient != 1)
                                if (coeficient == 0)
                                    val = Math.Abs(val);
                                else
                                    val /= coeficient;

                            //check the range of the value and apply
                            if (val > maxVal)
                                row[x] = maxVal;
                            else if (val < MinValue)
                                row[x] = ushort.MinValue;
                            else
                                row[x] = (ushort)val;
                        }
                        //apply the new row
                        selectedImage[y] = row;
                    }
                    //apply the new frame
                    image[frame] = selectedImage;
                });
                //return result image
                return image;
            }
            private static byte[][][] SmoothAllStack8bit(int C, Point[][][] ImagePxlMatr, TifFileInfo fi, int[] pxlVals, double coeficient)
            {
                byte[][][] image = fi.image8bitFilter;//source image
                double maxVal = byte.MaxValue - 2;//255
                double MinValue = byte.MinValue;//0

                int[] imageIndexes = GetFramesArray(C, fi);
                Parallel.ForEach(imageIndexes, frame =>
                {
                    byte[][] selectedImage = new byte[fi.sizeY][];
                    byte[][] origSelectedImage = image[frame];
                    for (int y = 0; y < fi.sizeY; y++)
                    {
                        byte[] row = new byte[fi.sizeX];
                        for (int x = 0; x < fi.sizeX; x++)
                        {
                            double val = 0;
                            Point[] plist = ImagePxlMatr[y][x];

                            //calculate value of the neighbors
                            for (int pInd = 0; pInd < plist.Length; pInd++)
                            {
                                Point p = plist[pInd];
                                val += ((double)origSelectedImage[p.Y][p.X] * (double)pxlVals[pInd]);
                            }
                            //normalize
                            if (coeficient != 1)
                                if (coeficient == 0)
                                    val = Math.Abs(val);
                                else
                                    val /= coeficient;
                            //check the range of the value and apply
                            if (val >= maxVal)
                                row[x] = byte.MaxValue - 1;
                            else if (val < MinValue)
                                row[x] = byte.MinValue;
                            else
                                row[x] = (byte)val;
                        }
                        //apply the new row
                        selectedImage[y] = row;
                    }
                    //apply the new frame
                    image[frame] = selectedImage;
                });
                //return result image
                return image;
            }

            public static void Median(int C,int rad, TifFileInfo fi)
            {
                CheckIsImagePrepared(fi);
                //return if image is binary
                if (fi.isBinary[C]) return;

                Point[] points = MedianPoints(rad);

                int[] imageIndexes = GetFramesArray(C, fi);
                switch (fi.bitsPerPixel)
                {
                    case 8:
                        Parallel.ForEach(imageIndexes, frame =>
                        {
                            fi.image8bitFilter[frame] = MedianAlgorithm(fi.image8bitFilter[frame],points, fi);
                        });
                        break;
                    case 16:
                        Parallel.ForEach(imageIndexes, frame =>
                        {
                            fi.image16bitFilter[frame] = MedianAlgorithm(fi.image16bitFilter[frame],points, fi);
                        });
                        break;
                }
            }
            private static byte[][] MedianAlgorithm(byte[][] image, Point[] points, TifFileInfo fi)
            {
                int x, y;
                byte[][] newImage = new byte[fi.sizeY][];
                List<byte> l = new List<byte>();
                for (y = 0; y < fi.sizeY; y++)
                {
                    newImage[y] = new byte[fi.sizeX];
                    Array.Copy(image[y], newImage[y], fi.sizeX);
                    for (x = 0; x < fi.sizeX; x++)
                    {
                        l.Clear();
                        foreach(Point p in points)
                            if(x+p.X>=0 && x + p.X < fi.sizeX &&
                                y + p.Y >= 0 && y + p.Y < fi.sizeY)
                                l.Add(image[y + p.Y][x + p.X]);
                        
                        newImage[y][x] = findMedian(l);
                    }
                }
                return newImage;
            }
            private static byte findMedian(List<byte> myList)
            {
                byte val = 0;
               
                myList.Sort();

                int med = (int)(myList.Count / 2 - 0.2);

                //nechetno
                if (med + med != myList.Count)
                    val = myList[med];
                else
                    val = (byte)((myList[med] + myList[med - 1]) / 2);

                return val;
            }
            private static ushort findMedian(List<ushort> myList)
            {
                ushort val = 0;
                myList.Sort();

                int med = (int)(myList.Count / 2 - 0.2);

                //nechetno
                if (med + med != myList.Count)
                    val = myList[med];
                else
                    val = (ushort)((myList[med] + myList[med - 1]) / 2);

                return val;
            }
            private static ushort[][] MedianAlgorithm(ushort[][] image, Point[] points, TifFileInfo fi)
            {
                int x, y;
                ushort[][] newImage = new ushort[fi.sizeY][];
                List<ushort> l = new List<ushort>();

                for (y = 0; y < fi.sizeY; y++)
                {
                    newImage[y] = new ushort[fi.sizeX];
                    Array.Copy(image[y], newImage[y], fi.sizeX);
                    for (x = 0; x < fi.sizeX; x++)
                    {
                        l.Clear();

                        foreach (Point p in points)
                            if (x + p.X >= 0 && x + p.X < fi.sizeX &&
                                y + p.Y >= 0 && y + p.Y < fi.sizeY)
                                l.Add(image[y + p.Y][x + p.X]);
                        
                        newImage[y][x] = findMedian(l);
                    }
                }
                return newImage;
            }
            #endregion Smooth the image
            #region Sobel Operation
            public static void SobelOperation(int C,TifFileInfo fi,string inKernelTopDown, string inKernelLeftRight)
            {
                CheckIsImagePrepared(fi);
                //return if image is binary
                if (fi.isBinary[C]) return;
                //choose kernel table
                int[][] kernelTopDown = KernelMatrix(inKernelTopDown);
                int[][] kernelLeftRight = KernelMatrix(inKernelLeftRight);

                //prepare array with exact coordinates for the kernel table mumbers with val
                Point[] pxlCordsTopDown = KernelPxlCord(kernelTopDown);
                Point[] pxlCordsLeftRight = KernelPxlCord(kernelLeftRight);
                //prepare array with the values != 0 in the kernel table
                int[] pxlValsTopDown = KernelPxlVal(kernelTopDown);
                int[] pxlValsLeftRight = KernelPxlVal(kernelLeftRight);
                //prepare shablon image matrix with all exact coordinates for sum
                Point[][][] ImagePxlMatrTopDown = ImagePxlMatrix(pxlCordsTopDown, fi, kernelTopDown);
                Point[][][] ImagePxlMatrLeftRight = ImagePxlMatrix(pxlCordsLeftRight, fi, kernelLeftRight);

                switch (fi.bitsPerPixel)
                {
                    case 8:
                        byte[][][] image8bit = detectEdgesAllStack8bit(
                            C, ImagePxlMatrTopDown, ImagePxlMatrLeftRight, fi,
                            pxlValsTopDown, pxlValsLeftRight);
                        fi.image8bitFilter = image8bit;
                        break;
                    case 16:
                        ushort[][][] image16bit = detectEdgesAllStack16bit(
                            C, ImagePxlMatrTopDown, ImagePxlMatrLeftRight, fi,
                            pxlValsTopDown, pxlValsLeftRight);
                        fi.image16bitFilter = image16bit;
                        break;
                }

            }
            private static byte[][][] detectEdgesAllStack8bit(int C, Point[][][] ImagePxlMatrTopDown, Point[][][] ImagePxlMatrLeftRight,
                TifFileInfo fi, int[] pxlValsTopDown, int[] pxlValsLeftRight)
            {
                byte[][][] image = fi.image8bitFilter;
                double maxVal = byte.MaxValue - 2;
                double MinValue = byte.MinValue;

                int[] imageIndexes = GetFramesArray(C, fi);
                Parallel.ForEach(imageIndexes, frame =>
                {
                    byte[][] selectedImage = new byte[fi.sizeY][];
                    byte[][] origSelectedImage = image[frame];
                    for (int y = 0; y < fi.sizeY; y++)
                    {
                        byte[] row = new byte[fi.sizeX];
                        for (int x = 0; x < fi.sizeX; x++)
                        {
                            //Top-Down
                            double valTopDown = 0;
                            Point[] plistTopDown = ImagePxlMatrTopDown[y][x];
                            for (int pInd = 0; pInd < plistTopDown.Length; pInd++)
                            {
                                Point p = plistTopDown[pInd];
                                valTopDown += ((double)origSelectedImage[p.Y][p.X] * (double)pxlValsTopDown[pInd]);
                            }
                            valTopDown = Math.Abs(valTopDown);
                            //Left-Right
                            double valLeftRight = 0;
                            Point[] plistLeftRight = ImagePxlMatrLeftRight[y][x];
                            for (int pInd = 0; pInd < plistLeftRight.Length; pInd++)
                            {
                                Point p = plistLeftRight[pInd];
                                valLeftRight += ((double)origSelectedImage[p.Y][p.X] * (double)pxlValsLeftRight[pInd]);
                            }
                            valLeftRight = Math.Abs(valLeftRight);
                            //sum
                            double val = (valLeftRight + valTopDown) / 2;
                            //check the value range
                            if (val >= maxVal)
                                row[x] = byte.MaxValue - 1;
                            else if (val < MinValue)
                                row[x] = byte.MinValue;
                            else
                                row[x] = (byte)val;
                        }
                        selectedImage[y] = row;
                    }
                    image[frame] = selectedImage;
                });

                return image;
            }
            private static ushort[][][] detectEdgesAllStack16bit(int C, Point[][][] ImagePxlMatrTopDown, Point[][][] ImagePxlMatrLeftRight,
                TifFileInfo fi, int[] pxlValsTopDown, int[] pxlValsLeftRight)
            {
                ushort[][][] image = fi.image16bitFilter;
                ushort maxVal = ushort.MaxValue;
                double MinValue = ushort.MinValue;

                int[] imageIndexes = GetFramesArray(C, fi);
                Parallel.ForEach(imageIndexes, frame =>
                {
                    ushort[][] selectedImage = new ushort[fi.sizeY][];
                    ushort[][] origSelectedImage = image[frame];
                    for (int y = 0; y < fi.sizeY; y++)
                    {
                        ushort[] row = new ushort[fi.sizeX];
                        for (int x = 0; x < fi.sizeX; x++)
                        {
                            //Top-Down
                            double valTopDown = 0;
                            Point[] plistTopDown = ImagePxlMatrTopDown[y][x];
                            for (int pInd = 0; pInd < plistTopDown.Length; pInd++)
                            {
                                Point p = plistTopDown[pInd];
                                valTopDown += ((double)origSelectedImage[p.Y][p.X] * (double)pxlValsTopDown[pInd]);
                            }
                            valTopDown = Math.Abs(valTopDown);
                            //Right-Left
                            double valLeftRight = 0;
                            Point[] plistLeftRight = ImagePxlMatrLeftRight[y][x];
                            for (int pInd = 0; pInd < plistLeftRight.Length; pInd++)
                            {
                                Point p = plistLeftRight[pInd];
                                valLeftRight += ((double)origSelectedImage[p.Y][p.X] * (double)pxlValsLeftRight[pInd]);
                            }
                            valLeftRight = Math.Abs(valLeftRight);
                            //sum
                            double val = (valLeftRight + valTopDown) / 2;
                            if (val > maxVal)
                                row[x] = maxVal;
                            else if (val < MinValue)
                                row[x] = ushort.MinValue;
                            else
                                row[x] = (ushort)val;
                        }
                        selectedImage[y] = row;
                    }
                    image[frame] = selectedImage;
                });
                return image;
            }
            #endregion Sobel Operation detection

           #region Binary
            public static void ToBinary(Color selectedCol, TifFileInfo fi, int C, ImageAnalyser IA
               , int[] thresholdValues, Color[] thresholdColors, int thresholds, Color SpotColor,
                int SelectedSpotThresh, string SpotTailType, int spotSensitivity, int SpotThresh, int typeSpotThresh)
            {
                CheckIsImagePrepared(fi);

                int[] imageIndexes = MyConvolution.GetFramesArray(C, fi);
                
                switch (fi.bitsPerPixel)
                {
                    case 8:
                        Parallel.ForEach(imageIndexes, (ind) =>
                        {
                            Draw8BitFilteredImage(selectedCol, fi, ind, C, IA
                   , thresholdValues, thresholdColors, thresholds, SpotColor,
                    SelectedSpotThresh, SpotTailType, spotSensitivity, SpotThresh,
                    typeSpotThresh);
                        });
                        break;
                    case 16:
                        Parallel.ForEach(imageIndexes, (ind) =>
                        {
                            Draw16BitFilteredImage(selectedCol, fi, ind, C, IA
               , thresholdValues, thresholdColors, thresholds, SpotColor,
                SelectedSpotThresh, SpotTailType, spotSensitivity, SpotThresh,
                typeSpotThresh);
                        });
                        break;
                }

                fi.isBinary[C] = true;
            }

            private static void Draw8BitFilteredImage(Color selectedCol, TifFileInfo fi, int frame, int C, ImageAnalyser IA
               , int[] thresholdValues, Color[] thresholdColors, int thresholds, Color SpotColor,
                int SelectedSpotThresh, string SpotTailType, int spotSensitivity, int SpotThresh, int typeSpotThresh)
            {
                    //image array
                    byte[][] image = fi.image8bitFilter[frame];
                    //calculate spot detector diapasone
                    int[] SpotDiapason = CalculateBorders(fi, C, frame,SelectedSpotThresh,thresholds,thresholdValues,SpotTailType,spotSensitivity,SpotThresh,typeSpotThresh);
                    //Coordinates
                    int val = 0, x,y;
                    Color lastCol = thresholdColors[thresholds], col = Color.Transparent;

                for (y = 0; y < fi.sizeY; y++)
                    for (x = 0; x < fi.sizeX; x++)
                    {
                        val = (int)image[y][x];
                        col = Color.Transparent;
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
                        if (col != selectedCol)
                            image[y][x] = 0;
                        else 
                            image[y][x] = 255;
                    }
            }
            private static void Draw16BitFilteredImage(Color selectedCol, TifFileInfo fi, int frame, int C, ImageAnalyser IA
               , int[] thresholdValues, Color[] thresholdColors, int thresholds, Color SpotColor,
                int SelectedSpotThresh, string SpotTailType, int spotSensitivity, int SpotThresh, int typeSpotThresh)
            {
                //image array
                ushort[][] image = fi.image16bitFilter[frame];
                //calculate spot detector diapasone
                int[] SpotDiapason = CalculateBorders(fi, C, frame, SelectedSpotThresh, thresholds, thresholdValues, SpotTailType, spotSensitivity, SpotThresh, typeSpotThresh);
                //Coordinates
                int val = 0, x, y;
                Color lastCol = thresholdColors[thresholds], col = Color.Transparent;

                for (y = 0; y < fi.sizeY; y++)
                    for (x = 0; x < fi.sizeX; x++)
                    {
                        val = (int)image[y][x];
                        col = Color.Transparent;
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
                        if (col != selectedCol)
                            image[y][x] = 0;
                        else
                            image[y][x] = 16000;
                    }
            }
            public static int[] CalculateBorders(TifFileInfo fi, int C, int frame, 
                int SelectedSpotThresh, int thresholds, int[] thresholdValues, string SpotTailType,
                int spotSensitivity, int SpotThresh, int typeSpotThresh)
            {
                try
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
                catch
                {
                    return new int[] { 0, 0 };
                }
            }

            private static int[] calculate8bitHisogram(TifFileInfo fi,int C, int frame)
            {
                int step = 101 - fi.spotSensitivity[C];
                int[] res = new int[(int)(byte.MaxValue / step) + 1];

                //calculate histogram
                foreach (byte[] row in fi.image8bitFilter[frame])
                    foreach (byte val in row)
                        res[(int)(val / step)]++;

                return res;
            }
            private static int[] calculate16bitHisogram(TifFileInfo fi, int C, int frame)
            {
                int step = 101 - fi.spotSensitivity[C];
                int[] res = new int[(int)(16384 / step) + 1];

                //calculate histogram
                foreach (ushort[] row in fi.image16bitFilter[frame])
                    foreach (ushort val in row)
                        res[(int)(val / step)]++;

                return res;
            }
            
            public static void FillHoles(int C, TifFileInfo fi)
            {
                if (fi.sizeX < 2 || fi.sizeY < 2) return;

                CheckIsImagePrepared(fi);
                //return if image is binary
                if (!fi.isBinary[C]) return;

                int[] imageIndexes = GetFramesArray(C, fi);
                switch (fi.bitsPerPixel)
                {
                    case 8:
                        Parallel.ForEach(imageIndexes, frame =>
                        {
                            FillHolesAlgorithm(fi.image8bitFilter[frame], fi);
                        });
                        break;
                    case 16:
                        Parallel.ForEach(imageIndexes, frame =>
                        {
                            FillHolesAlgorithm(fi.image16bitFilter[frame], fi);
                        });
                        break;
                }
            }
            private static void FillHolesAlgorithm(byte[][] image,TifFileInfo fi)
            {
                int x, y;
                List<Point> temp = new List<Point>();
                //fill first and last row with zeroInd
                for (x = 0,y=fi.sizeY-1; x < fi.sizeX; x++)
                {
                    image[0][x] = 1;
                    image[y][x] = 1;
                }
                //fill first and last column with zeroInd
                for (y = 0, x = fi.sizeX - 1; y < fi.sizeY; y++)
                {
                    image[y][0] = 1;
                    image[y][x] = 1;
                }
                //up->down && left->right
                for (y = 1; y < fi.sizeY - 1; y++)
                    for (x = 1; x < fi.sizeX - 1; x++)
                    {
                        if (image[y][x] == 0 &&
                           (image[y][x - 1] == 1 || image[y-1][x] == 1 ||
                           image[y][x + 1] == 1 || image[y + 1][x] == 1))
                            image[y][x] = 1;
                    }
                //up<-down && left<-right
                for (y = fi.sizeY - 2; y > 0; y--)
                    for (x = fi.sizeX - 2; x > 0; x--)
                    {
                        if (image[y][x] == 0 &&
                           (image[y][x - 1] == 1 || image[y - 1][x] == 1 ||
                           image[y][x + 1] == 1 || image[y + 1][x] == 1))
                            image[y][x] = 1;
                        else if (image[y][x] == 0)
                            temp.Add(new Point(x, y));
                    }

                int count = temp.Count;
                while (count != 0)
                {
                    count = 0;
                    foreach(Point p in temp)
                    {
                        x = p.X;
                        y = p.Y;

                        if (image[y][x] == 0 &&
                           (image[y][x - 1] == 1 || image[y - 1][x] == 1 ||
                           image[y][x + 1] == 1 || image[y + 1][x] == 1))
                        {
                            image[y][x] = 1;
                            count++;
                        }
                    }
                    for(int i = temp.Count-1; i>=0; i--)
                    {
                        x = temp[i].X;
                        y = temp[i].Y;

                        if (image[y][x] == 0 &&
                           (image[y][x - 1] == 1 || image[y - 1][x] == 1 ||
                           image[y][x + 1] == 1 || image[y + 1][x] == 1))
                        {
                            image[y][x] = 1;
                            count++;
                            temp.RemoveAt(i);
                        }
                        else if (image[y][x] == 1)
                        {
                            temp.RemoveAt(i);
                        }
                    }
                }
                //fill the holes and restore the image background
                for (y = 0; y < fi.sizeY; y++)
                    for (x = 0; x < fi.sizeX; x++)
                    {
                        if (image[y][x] == 1)
                            image[y][x] = 0;
                        else if (image[y][x] == 0)
                            image[y][x] = 255;
                    }
            }
            private static void FillHolesAlgorithm(ushort[][] image, TifFileInfo fi)
            {
                int x, y;
                List<Point> temp = new List<Point>();
                //fill first and last row with zeroInd
                for (x = 0, y = fi.sizeY - 1; x < fi.sizeX; x++)
                {
                    image[0][x] = 1;
                    image[y][x] = 1;
                }
                //fill first and last column with zeroInd
                for (y = 0, x = fi.sizeX - 1; y < fi.sizeY; y++)
                {
                    image[y][0] = 1;
                    image[y][x] = 1;
                }
                //up->down && left->right
                for (y = 1; y < fi.sizeY - 1; y++)
                    for (x = 1; x < fi.sizeX - 1; x++)
                    {
                        if (image[y][x] == 0 &&
                           (image[y][x - 1] == 1 || image[y - 1][x] == 1 ||
                           image[y][x + 1] == 1 || image[y + 1][x] == 1))
                            image[y][x] = 1;
                    }
                //up<-down && left<-right
                for (y = fi.sizeY - 2; y > 0; y--)
                    for (x = fi.sizeX - 2; x > 0; x--)
                    {
                        if (image[y][x] == 0 &&
                           (image[y][x - 1] == 1 || image[y - 1][x] == 1 ||
                           image[y][x + 1] == 1 || image[y + 1][x] == 1))
                            image[y][x] = 1;
                        else if (image[y][x] == 0)
                            temp.Add(new Point(x, y));
                    }

                int count = temp.Count;
                while (count != 0)
                {
                    count = 0;
                    foreach (Point p in temp)
                    {
                        x = p.X;
                        y = p.Y;

                        if (image[y][x] == 0 &&
                           (image[y][x - 1] == 1 || image[y - 1][x] == 1 ||
                           image[y][x + 1] == 1 || image[y + 1][x] == 1))
                        {
                            image[y][x] = 1;
                            count++;
                        }
                    }
                    for (int i = temp.Count - 1; i >= 0; i--)
                    {
                        x = temp[i].X;
                        y = temp[i].Y;

                        if (image[y][x] == 0 &&
                           (image[y][x - 1] == 1 || image[y - 1][x] == 1 ||
                           image[y][x + 1] == 1 || image[y + 1][x] == 1))
                        {
                            image[y][x] = 1;
                            count++;
                            temp.RemoveAt(i);
                        }
                        else if (image[y][x] == 1)
                        {
                            temp.RemoveAt(i);
                        }
                    }
                }
                //fill the holes and restore the image background
                for (y = 0; y < fi.sizeY; y++)
                    for (x = 0; x < fi.sizeX; x++)
                    {
                        if (image[y][x] == 1)
                            image[y][x] = 0;
                        else if (image[y][x] == 0)
                            image[y][x] = 16000;
                    }
            }
            public static void Erode(int C, TifFileInfo fi)
            {
                CheckIsImagePrepared(fi);
                //return if image is binary
                if (!fi.isBinary[C]) return;

                int[] imageIndexes = GetFramesArray(C, fi);
                switch (fi.bitsPerPixel)
                {
                    case 8:
                        Parallel.ForEach(imageIndexes, frame =>
                        {
                            fi.image8bitFilter[frame]=ErodeAlgorithm(fi.image8bitFilter[frame], fi);
                        });
                        break;
                    case 16:
                        Parallel.ForEach(imageIndexes, frame =>
                        {
                            fi.image16bitFilter[frame]=ErodeAlgorithm(fi.image16bitFilter[frame], fi);
                        });
                        break;
                }
            }
            private static byte[][] ErodeAlgorithm(byte[][] image, TifFileInfo fi)
            {
                int x, y;
                byte[][] newImage = new byte[fi.sizeY][];

                for (y = 0; y < fi.sizeY; y++)
                {
                    newImage[y] = new byte[fi.sizeX];
                    Array.Copy(image[y], newImage[y], fi.sizeX);
                    for (x = 0; x < fi.sizeX; x++)
                        if (image[y][x] != 0 &&
                               ((x - 1 >= 0 && image[y][x - 1] == 0) ||
                               (y - 1 >= 0 && image[y - 1][x] == 0) ||
                               (x + 1 < fi.sizeX && image[y][x + 1] == 0) ||
                               (y + 1 < fi.sizeY && image[y + 1][x] == 0)))
                            newImage[y][x] = 0;
                }
                return newImage;
            }
            private static ushort[][] ErodeAlgorithm(ushort[][] image, TifFileInfo fi)
            {
                int x, y;
                ushort[][] newImage = new ushort[fi.sizeY][];

                for (y = 0; y < fi.sizeY; y++)
                {
                    newImage[y] = new ushort[fi.sizeX];
                    Array.Copy(image[y], newImage[y], fi.sizeX);
                    for (x = 0; x < fi.sizeX; x++)
                        if (image[y][x] != 0 &&
                               ((x - 1 >= 0 && image[y][x - 1] == 0) ||
                               (y - 1 >= 0 && image[y - 1][x] == 0) ||
                               (x + 1 < fi.sizeX && image[y][x + 1] == 0) ||
                               (y + 1 < fi.sizeY && image[y + 1][x] == 0)))
                            newImage[y][x] = 0;
                }
                return newImage;
            }
            public static void Dilate(int C, TifFileInfo fi)
            {
                CheckIsImagePrepared(fi);
                //return if image is binary
                if (!fi.isBinary[C]) return;

                int[] imageIndexes = GetFramesArray(C, fi);
                switch (fi.bitsPerPixel)
                {
                    case 8:
                        Parallel.ForEach(imageIndexes, frame =>
                        {
                            fi.image8bitFilter[frame] = DilateAlgorithm(fi.image8bitFilter[frame], fi);
                        });
                        break;
                    case 16:
                        Parallel.ForEach(imageIndexes, frame =>
                        {
                            fi.image16bitFilter[frame] = DilateAlgorithm(fi.image16bitFilter[frame], fi);
                        });
                        break;
                }
            }
            private static byte[][] DilateAlgorithm(byte[][] image, TifFileInfo fi)
            {
                int x, y;
                byte[][] newImage = new byte[fi.sizeY][];

                for (y = 0; y < fi.sizeY; y++)
                {
                    newImage[y] = new byte[fi.sizeX];
                    Array.Copy(image[y], newImage[y], fi.sizeX);
                    for (x = 0; x < fi.sizeX; x++)
                        if (image[y][x] == 0 &&
                               ((x - 1 >= 0 && image[y][x - 1] != 0) ||
                               (y - 1 >= 0 && image[y - 1][x] != 0) ||
                               (x + 1 < fi.sizeX && image[y][x + 1] != 0) ||
                               (y + 1 < fi.sizeY && image[y + 1][x] != 0)))
                            newImage[y][x] = 255;
                }
                return newImage;
            }
            private static ushort[][] DilateAlgorithm(ushort[][] image, TifFileInfo fi)
            {
                int x, y;
                ushort[][] newImage = new ushort[fi.sizeY][];

                for (y = 0; y < fi.sizeY; y++)
                {
                    newImage[y] = new ushort[fi.sizeX];
                    Array.Copy(image[y], newImage[y], fi.sizeX);
                    for (x = 0; x < fi.sizeX; x++)
                        if (image[y][x] == 0 &&
                               ((x - 1 >= 0 && image[y][x - 1] != 0) ||
                               (y - 1 >= 0 && image[y - 1][x] != 0) ||
                               (x + 1 < fi.sizeX && image[y][x + 1] != 0) ||
                               (y + 1 < fi.sizeY && image[y + 1][x] != 0)))
                            newImage[y][x] = 16000;
                }
                return newImage;
            }
            
            #endregion Binary
        }
        public static class AccordConvolution
        {
            
            public static void ProcessFilter(TifFileInfo fi, int channel, int[,] kernel)
            {
                DuplicateImage(fi);

                int[] indexes = new int[fi.imageCount / fi.sizeC];

                for (int i = channel, position = 0; i < fi.imageCount; i += fi.sizeC, position++)
                    indexes[position] = i;
                
                int imageSize = fi.sizeX*fi.sizeY;
                // create filter
                Convolution filter = new Convolution(kernel);
                UnmanagedImage img = null;
                int stripSize = fi.sizeX;
                if (fi.bitsPerPixel == 16)
                    stripSize *= 2;
                foreach (int frame in indexes)
                {
                    img = CreateImage(frame, fi, imageSize);
                    // apply the filter
                    filter.ApplyInPlace(img);
                    ReturnCTImage(frame, fi, img, stripSize);
                }
                
            }
            private static void DuplicateImage(TifFileInfo fi)
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
            private static UnmanagedImage CreateImage(int frame, TifFileInfo fi, int imageSize)
            {
                UnmanagedImage img = null;
                Accord.Imaging.Converters.MatrixToImage conv =
                    new Accord.Imaging.Converters.MatrixToImage();

                switch (fi.bitsPerPixel)
                {
                    case 8:
                        conv.Format = System.Drawing.Imaging.PixelFormat.Format8bppIndexed;
                        conv.Convert(fi.image8bitFilter[frame], out img);
                        break;
                    case 16:
                        short[,] srs = new short[ fi.sizeY, fi.sizeX];

                        for (int y = 0; y < fi.sizeY; y++)
                            for (int x = 0; x < fi.sizeX; x++)
                                srs[y, x] = (short)fi.image16bitFilter[frame][y][x];
                        conv.Format = System.Drawing.Imaging.PixelFormat.Format16bppGrayScale;
                        //conv.Max = 255;
                        conv.Convert(srs, out img);
                        break;
                    default:
                        break;
                }
                return img;
            }
           
            private static void ReturnCTImage(int frame, TifFileInfo fi, UnmanagedImage image, int stripSize)
            {
                Accord.Imaging.Converters.ImageToMatrix conv =
                    new Accord.Imaging.Converters.ImageToMatrix();
                
                if (fi.bitsPerPixel == 8)
                {
                    conv.Convert(image,out fi.image8bitFilter[frame]);
                }
                else if (fi.bitsPerPixel == 16)
                {
                    double[,] srs = null;
                    conv.Max = 255;
                    conv.Convert(image, out srs);
                    for (int y = 0; y < fi.sizeY; y++)
                        for (int x = 0; x < fi.sizeX; x++)
                            fi.image16bitFilter[frame][y][x] = (ushort)srs[y, x];
                    
                }
            }
        }
    }
}
