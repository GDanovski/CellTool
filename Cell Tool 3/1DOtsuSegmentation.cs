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
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;

namespace Cell_Tool_3
{
    class OtsuSegmentation
    {
        //
        // Multi_OtsuThreshold_.java
        //
        // Algorithm: PS.Liao, TS.Chen, and PC. Chung,
        //            Journal of Information Science and Engineering, vol 17, 713-727 (2001)
        // 
        // Coding   : Yasunari Tosa (ytosa@att.net)
        // Date     : Feb. 19th, 2005
        //
        //Coding C# : Georgi Danovski (georgi_danovski@abv.bg)
        //Date      : 27-Jun-2017
        //
        #region Initialize
        //controls
        public ImageAnalyser IA;
        public Panel panel = new Panel();
        public ComboBox thresholdsNumCB = new ComboBox();
        public CheckBox sumHistogramsCheckBox = new CheckBox();
        public Button ProcessBtn = new Button();

        public CTTrackBar[] threshTrackBars = new CTTrackBar[4];
        public Button[] colorBtns = new Button[5];
        public int GlobalStep16Bit = 0;
        //values
        private int NGRAY = 256;
        //tooltip 
        private ToolTip TurnOnToolTip = new ToolTip();
        public OtsuSegmentation(Panel mainPanel, ImageAnalyser IA)
        {

            this.IA = IA;
            //Core panel 
            panel.Dock = DockStyle.Top;
            panel.BackColor = IA.FileBrowser.BackGround2Color1;
            panel.ForeColor = IA.FileBrowser.ShriftColor1;
            panel.Visible = false;
            panel.Height = 150;
            mainPanel.Controls.Add(panel);
            panel.BringToFront();

            #region Options
            GroupBox optionGB = new GroupBox();
            optionGB.Text = "Options:";
            optionGB.BackColor = IA.FileBrowser.BackGround2Color1;
            optionGB.ForeColor = IA.FileBrowser.ShriftColor1;
            optionGB.Dock = DockStyle.Top;
            optionGB.Height = 85;
            panel.Controls.Add(optionGB);
            optionGB.BringToFront();
            
            Label Name = new Label();
            Name.Text = "Thresholds:";
            Name.Width = TextRenderer.MeasureText(Name.Text, Name.Font).Width;
            Name.Location = new Point(5, 18);
            optionGB.Controls.Add(Name);
            Name.BringToFront();

            ComboBox cb = thresholdsNumCB;
            cb.Text = "0";
            cb.Items.AddRange(new string[] { "0","1","2","3","4"});
            cb.Width = 40;
            cb.Location = new Point(80, 15);
            optionGB.Controls.Add(cb);
            cb.BringToFront();
            cb.DropDownStyle = ComboBoxStyle.DropDownList;
            cb.SelectedIndex = 0;
            cb.AutoSize = false;
            cb.SelectedIndexChanged += new EventHandler(delegate(object o, EventArgs e) 
            {
                TifFileInfo fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
                fi.thresholdsCBoxIndex[fi.cValue] = cb.SelectedIndex;
                IA.ReloadImages();
            });

            CheckBox checkB = sumHistogramsCheckBox;
            checkB.Text = "Use SUM histogram";
            checkB.Tag = "Use SUM histogram:\nCalculates histograms for all images\nand merge them into one";
            checkB.Width = 150;
            checkB.Checked = true;
            checkB.MouseHover += Control_MouseOver;
            checkB.Location = new Point(7, 35);
            optionGB.Controls.Add(checkB);
            checkB.BringToFront();
            checkB.CheckedChanged += new EventHandler(delegate (object o, EventArgs e)
            {
                TifFileInfo fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
                fi.sumHistogramChecked[fi.cValue] = checkB.Checked;
                if (((CheckBox)o).Focused == true)
                {
                    IA.ReloadImages();
                }
            });
            {
                Button btn = ProcessBtn;
                btn.Width = 115;
                btn.FlatStyle = FlatStyle.Standard;
                btn.BackColor = SystemColors.ButtonFace;
                btn.ForeColor = Color.Black;
                btn.Text = "Process";
                btn.Location = new Point(5, 58);
                optionGB.Controls.Add(btn);
                btn.BringToFront();
                btn.Click += new EventHandler(delegate (object o, EventArgs a)
                {
                    int MLEVEL = thresholdsNumCB.SelectedIndex + 1;

                    if (IA.Segmentation.SegmentationCBox.SelectedIndex == 2)
                    {
                        IA.Segmentation.Kmeans.Start(MLEVEL);
                        return;
                    }

                    if (IA.Segmentation.SegmentationCBox.SelectedIndex != 1) return;

                    TifFileInfo fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
                    if (fi.available == false)
                    {
                        MessageBox.Show("Image is not ready yet! \nTry again later.");
                    }
                    else
                    {
                        
                        //background worker
                        var bgw = new BackgroundWorker();
                        bgw.WorkerReportsProgress = true;
                        bgw.DoWork += new DoWorkEventHandler(delegate (Object o1, DoWorkEventArgs a1)
                        {
                            try
                            {
                                //Segmentation event
                                run(fi,MLEVEL);
                            }
                            catch { }
                            //report progress
                            ((BackgroundWorker)o1).ReportProgress(0);
                        });

                        bgw.ProgressChanged += new ProgressChangedEventHandler(delegate (Object o1, ProgressChangedEventArgs a1)
                        {
                            fi.available = true;
                            IA.FileBrowser.StatusLabel.Text = "Ready";
                            IA.MarkAsNotSaved();
                            IA.ReloadImages();
                        });
                        //Apply status
                        IA.FileBrowser.StatusLabel.Text = "Segmentation...";
                        fi.available = false;
                        //start bgw
                        bgw.RunWorkerAsync();
                    }
                });
            }
            #endregion Options

            #region Thresholds
            GroupBox threshGB = new GroupBox();
            threshGB.Text = "Thresholds:";
            threshGB.BackColor = IA.FileBrowser.BackGround2Color1;
            threshGB.ForeColor = IA.FileBrowser.ShriftColor1;
            threshGB.Dock = DockStyle.Fill;
            threshGB.Height = 50;
            panel.Controls.Add(threshGB);
            threshGB.BringToFront();

            //Color btns
            Panel colorPanel = new Panel();
            colorPanel.Dock = DockStyle.Left;
            colorPanel.Width = 25;
            threshGB.Controls.Add(colorPanel);

            Panel UpPanel = new Panel();
            UpPanel.Dock = DockStyle.Top;
            UpPanel.Height = 15;
            threshGB.Controls.Add(UpPanel);
            UpPanel.BringToFront();

            for (int i = 0; i< colorBtns.Length; i++)
            {
                Button btn = new Button();
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderSize = 0;
                btn.ForeColor = Color.Black;
                btn.Text = "";
                btn.Tag = i;
                btn.Dock = DockStyle.Top;
                btn.Height = 25;
                colorPanel.Controls.Add(btn);
                colorBtns[i] = btn;
                btn.BringToFront();
                btn.Visible = false;
                btn.MouseDown += new MouseEventHandler(ColorBtn_Click);
                btn.MouseHover += new EventHandler(delegate (object o, EventArgs a) 
                {
                    if(btn.Text == "")
                        TurnOnToolTip.SetToolTip(btn, "Color " + ((int)btn.Tag).ToString()
                        + ":\nLeft click to Disable\nRight click to change color");
                    else
                        TurnOnToolTip.SetToolTip(btn, "Color " + ((int)btn.Tag).ToString()
                        + " - Disabled\nLeft click to Enable");
                });
            }
            //threshold track bars
            for (int i = 0; i < threshTrackBars.Length; i++)
            {
                CTTrackBar tb = new CTTrackBar();
                tb.Initialize();
                tb.Panel.Dock = DockStyle.Top;
                tb.BackColor(IA.FileBrowser.BackGround2Color1);
                tb.ForeColor(IA.FileBrowser.ShriftColor1);
                tb.Panel.Visible = false;
                tb.Refresh(0, 0, 10);
                tb.Name.Text = "T" + (i + 1).ToString();
                tb.NamePanel.Width = 30;
                threshGB.Controls.Add(tb.Panel);
                threshTrackBars[i] = tb;
                tb.Panel.BringToFront();
                tb.Value.Changed += new ChangedValueEventHandler(delegate (Object o, ChangeValueEventArgs a)
                {
                   TrackBar_ValueChange(a, tb);
                });
            }
            #endregion Thresholds
        }
        private void TrackBar_ValueChange(ChangeValueEventArgs e, CTTrackBar tb)
        {
            //variables
            TifFileInfo fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
            int[] vals = fi.thresholdValues[fi.cValue];
            int val = int.Parse(e.Value);
            int valRef = val;
            //find trackbar index
            int index = 0;
            for (int i = 0; i < threshTrackBars.Length; i++)
                if (threshTrackBars[i] == tb)
                { index = i; break; }
            //Check the avaliable min
            if (index > 0)
                if (val <= vals[index])
                    val = vals[index] + 1;
            //Check the avaliable max
            if (index < fi.thresholds[fi.cValue] - 1)
                if (val >= vals[index + 2])
                    val = vals[index + 2] - 1;
            //refresh if value is wrong
            if (val != valRef)
                tb.Refresh(val, tb.TrackBar1.Minimum, tb.TrackBar1.Maximum);
            //apply changes
            if (val != vals[index + 1])
            {
                if (tb.TrackBar1.Focused == true | tb.TextBox1.Focused == true |
                    tb.ApplyBtn.Focused == true)
                {
                    #region apply to history
                    fi.delHist = true;
                    IA.delHist = true;
                    IA.UnDoBtn.Enabled = true;
                    IA.DeleteFromHistory();
                    fi.History.Add("segmentation.SetThreshOld("
                        + fi.cValue + "," + (index + 1).ToString() + ","
                        + vals[index + 1].ToString() + ")");
                    fi.History.Add("segmentation.SetThreshOld("
                      + fi.cValue + "," + (index + 1).ToString() + ","
                      + val.ToString() + ")");
                    IA.UpdateUndoBtns();
                    IA.MarkAsNotSaved();
                    #endregion apply to history
                }
                vals[index + 1] = val;
                IA.ReloadImages();
            }
        }
        private void ColorBtn_Click(object sender, MouseEventArgs e)
        {
            TifFileInfo fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
            Button btn = (Button)sender;

            if(e.Button == MouseButtons.Left)
            {
                //disable/enable color
                if(fi.thresholdColors[fi.cValue][(int)btn.Tag] == Color.Transparent)
                    fi.thresholdColors[fi.cValue][(int)btn.Tag] =
                        fi.RefThresholdColors[fi.cValue][(int)btn.Tag];
                else
                    fi.thresholdColors[fi.cValue][(int)btn.Tag] = Color.Transparent;

                #region apply to history
                Color val = fi.RefThresholdColors[fi.cValue][(int)btn.Tag];
                Color oldVal = Color.Transparent;

                if (fi.thresholdColors[fi.cValue][(int)btn.Tag] == Color.Transparent)
                { val = Color.Transparent; oldVal = fi.RefThresholdColors[fi.cValue][(int)btn.Tag]; }

                fi.delHist = true;
                IA.delHist = true;
                IA.UnDoBtn.Enabled = true;
                IA.DeleteFromHistory(); fi.History.Add("segmentation.SetColor("
                     + fi.cValue.ToString() + "," +
                     ((int)btn.Tag).ToString() + "," +
                     ColorTranslator.ToHtml(oldVal) + ")");
                fi.History.Add("segmentation.SetColor("
                    + fi.cValue.ToString() + "," +
                    ((int)btn.Tag).ToString() + "," +
                    ColorTranslator.ToHtml(val) + ")");
                IA.UpdateUndoBtns();
                IA.MarkAsNotSaved();
                #endregion apply to history

                IA.ReloadImages();
            }
            else if (e.Button == MouseButtons.Right & btn.Text == "")
            {
                //change color
                ColorDialog colorDialog1 = new ColorDialog();
                colorDialog1.AllowFullOpen = true;
                colorDialog1.AnyColor = true;
                colorDialog1.FullOpen = true;
                colorDialog1.Color = fi.RefThresholdColors[fi.cValue][(int)btn.Tag];
                //set Custom Colors
                if (IA.settings.CustomColors[IA.FileBrowser.ActiveAccountIndex] != "@")
                {
                    List<int> colorsList = new List<int>();
                    foreach (string j in IA.settings.CustomColors[IA.FileBrowser.ActiveAccountIndex]
                        .Split(new[] { "\t" }, StringSplitOptions.None))
                    {
                        colorsList.Add(int.Parse(j));
                    }
                    colorDialog1.CustomColors = colorsList.ToArray();
                }
                // Show the color dialog.
                DialogResult result = colorDialog1.ShowDialog();
                //Copy Custom Colors
                int[] colors = (int[])colorDialog1.CustomColors.Clone();
                string txt = "@";
                if (colors.Length > 0)
                {
                    txt = colors[0].ToString();
                    for (int j = 1; j < colors.Length; j++)
                    {
                        txt += "\t" + colors[j].ToString();
                    }
                }
                IA.settings.CustomColors[IA.FileBrowser.ActiveAccountIndex] = txt;
                IA.settings.Save();

                if (result == DialogResult.OK)
                {
                    #region apply to history
                    fi.delHist = true;
                    IA.delHist = true;
                    IA.UnDoBtn.Enabled = true;
                    IA.DeleteFromHistory();fi.History.Add("segmentation.SetColor("
                        + fi.cValue + "," + ((int)btn.Tag).ToString() + ","
                        + ColorTranslator.ToHtml(fi.thresholdColors[fi.cValue][(int)btn.Tag]) + ")");
                    fi.History.Add("segmentation.SetColor("
                       + fi.cValue + "," + ((int)btn.Tag).ToString() + ","
                       + ColorTranslator.ToHtml(colorDialog1.Color) + ")");
                    IA.UpdateUndoBtns();
                    IA.MarkAsNotSaved();
                    #endregion apply to history
                    fi.RefThresholdColors[fi.cValue][(int)btn.Tag] = colorDialog1.Color;
                    fi.thresholdColors[fi.cValue][(int)btn.Tag] = colorDialog1.Color;
                    IA.ReloadImages();
                }
            }
        }
        public void loadThreshAndColorBtns(TifFileInfo fi)
        {
            //panel.SuspendLayout();
            int threshNum = fi.thresholds[fi.cValue];
            int[] vals1 = fi.thresholdValues[fi.cValue];
            int min = 0;
            int max = 0;
            switch (fi.bitsPerPixel)
            {
                case 8:
                    max = 255; // 2^8  - 1
                    break;
                case 16:
                    max = ushort.MaxValue;//16383; //2^14 - 1
                    break;
            }

            for (int i = 0; i < colorBtns.Length; i++)
            {
                //color btns
                if (i <= threshNum & colorBtns[i].Visible == false)
                {
                    colorBtns[i].Visible = true;
                    colorBtns[i].BringToFront();
                }
                else if (i > threshNum)
                {
                    colorBtns[i].Visible = false;
                }
                
                Color col = fi.thresholdColors[fi.cValue][i];
                
                if (col != Color.Transparent)
                {
                    colorBtns[i].BackColor = col;
                    colorBtns[i].Text = "";
                }
                else
                {
                    colorBtns[i].BackColor = Color.White;
                    colorBtns[i].Text = "NaN";
                }
                //trackBars
                if (i < threshNum & i < threshTrackBars.Length)
                {
                    if (threshTrackBars[i].Panel.Visible == false)
                    {
                        threshTrackBars[i].Panel.Visible = true;
                        threshTrackBars[i].Panel.BringToFront();
                    }
                    threshTrackBars[i].Refresh(vals1[i+1], min, max);
                }
                else if (i >= threshNum & i < threshTrackBars.Length)
                {
                    threshTrackBars[i].Panel.Visible = false;
                }
            }
            //Resize Panel
            
            if (threshNum == 0)
                panel.Height = 85;
            else
                panel.Height = 105 + 25 * (threshNum + 1);
           
            panel.ResumeLayout();
            
        }
        private void Control_MouseOver(object sender, EventArgs e)
        {
            var ctr = (Control)sender;
            TurnOnToolTip.SetToolTip(ctr, ctr.Tag.ToString());
        }
        #endregion Initialize

        #region Segmentation
        public void run(TifFileInfo fi, int MLEVEL)
        {
            fi.thresholds[fi.cValue] = MLEVEL-1;
            if (MLEVEL < 2){return;}
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
            int[] threshold = new int[MLEVEL]; // threshold
            int width = fi.sizeX;
            int height = fi.sizeY;
            ////////////////////////////////////////////
            // Build Histogram
            ////////////////////////////////////////////
            float[] h = new float[NGRAY];
            buildHistogram(h, fi, width, height);
            /////////////////////////////////////////////
            // Build lookup tables from h
            ////////////////////////////////////////////
            float[][] P = new float[NGRAY][];
            float[][] S = new float[NGRAY][];
            float[][] H = new float[NGRAY][];
            buildLookupTables(P, S, H, h);
            ////////////////////////////////////////////////////////
            // now M level loop   MLEVEL dependent term
            ////////////////////////////////////////////////////////
            float maxSig = findMaxSigma(MLEVEL, H, threshold);
            //apply to LUT
            
            switch (fi.bitsPerPixel)
            {
                case 8:
                    for (int i = 0; i < MLEVEL; i++)
                        fi.thresholdValues[fi.cValue][i] = threshold[i];
                    break;
                case 16:
                    for (int i = 0; i < MLEVEL; i++)
                        fi.thresholdValues[fi.cValue][i] = threshold[i] * GlobalStep16Bit;
                    break;
            }
        }
        private void buildHistogram(float[] h, TifFileInfo fi, int width, int height)
        {
            switch (fi.bitsPerPixel)
            {
                case 8:
                    calculate8bitHistogram(h, fi, width, height);
                    break;
                case 16:
                    calculate16bitHistogram(h, fi, width, height);
                    break;
            }
        }
        private void calculate8bitHistogram(float[] h, TifFileInfo fi, int width, int height)
        {
            FrameCalculator FC = new FrameCalculator();
            int frame = FC.FrameC(fi, fi.cValue);
            if (fi.sumHistogramChecked[fi.cValue] == false)
            {
                //histogram of current frame
                for (int y = 0; y < height; y++)
                    for (int x = 0; x < width; x++)
                        h[fi.image8bitFilter[frame][y][x]]++;
            }
            else
            { 
                //histogram of all images
                int[] input = new int[(int)(fi.imageCount / fi.sizeC)];
                float[][] hList = new float[input.Length][];
                //check wich frames are from selected color
                for (int i = 0, val = fi.cValue; i < input.Length; i++, val += fi.sizeC)
                    input[i] = val;
               //calculate histograms for all images
                Parallel.For(0,input.Length, (i) =>
                {
                    float[] h1 = new float[NGRAY];

                    for (int y = 0; y < height; y++)
                        for (int x = 0; x < width; x++)
                            h1[fi.image8bitFilter[input[i]][y][x]]++;

                    hList[i] = h1;
                });
                //sum histograms to one
                for (int hInd = 0; hInd < hList.Length; hInd++)
                    for (int i = 0; i < NGRAY; i++)                    
                        h[i] += hList[hInd][i];
            }
        }
        private ushort calculateMaxIntensity(TifFileInfo fi, int frame)
        {
            ushort max = 0;
            for(int y = 0; y < fi.sizeY; y++)
                foreach (ushort val in fi.image16bitFilter[frame][y])
                    if(max < val) { max = val; }
            return max;
        }
        private void calculate16bitHistogram(float[] h, TifFileInfo fi, int width, int height)
        {
            if (fi.sumHistogramChecked[fi.cValue] == false)
            {
                //histogram of current frame
                FrameCalculator FC = new FrameCalculator();
                int frame = FC.FrameC(fi, fi.cValue);

                int maxVal = ushort.MaxValue +1;
                int[] lut = new int[maxVal];
                //convert information to 8 bit
                int step = (int)(calculateMaxIntensity(fi, frame) / 256);
                GlobalStep16Bit = step;
                for (int i = 0, count = 0, val = 0; i < maxVal; i++, count++)
                {
                    if (count >= step)
                    {
                        count = 0;
                        if (val < 255) val++;
                    }
                    lut[i] = val;
                }
                //calculate histogram
                for (int y = 0; y < height; y++)
                    for (int x = 0; x < width; x++)
                        h[lut[fi.image16bitFilter[frame][y][x]]]++;
            }
            else
            {
                //histogram of all images
                int[] input = new int[(int)(fi.imageCount / fi.sizeC)];
                float[][] hList = new float[input.Length][];

                //check wich frames are from selected color
                for (int i = 0, val = fi.cValue; i < input.Length; i++, val += fi.sizeC)
                    input[i] = val;
                //Find absolute max intensity
                ushort[] maxVals = new ushort[input.Length];
                Parallel.For(0, input.Length, (i) =>
                {
                    maxVals[i] = calculateMaxIntensity(fi, input[i]);
                });

                ushort max = 0;
                foreach (ushort val in maxVals)
                    if(val > max) { max = val; }
                
                int maxVal = ushort.MaxValue + 1;
                int[] lut = new int[maxVal];
                //convert information to 8 bit
                int step = (int)(max / 256);
                GlobalStep16Bit = step;
                for (int i = 0, count = 0, val = 0; i < maxVal; i++, count++)
                {
                    if (count >= step)
                    {
                        count = 0;
                        if (val < 255) val++;
                    }
                    lut[i] = val;
                }
                //calculate histograms for all images
                Parallel.For(0, input.Length, (i) =>
                {
                    float[] h1 = new float[NGRAY];

                    for (int y = 0; y < height; y++)
                        for (int x = 0; x < width; x++)
                            h[lut[fi.image16bitFilter[input[i]][y][x]]]++;

                    hList[i] = h1;
                });
                //sum histograms to one
                for (int hInd = 0; hInd < hList.Length; hInd++)
                    for (int i = 0; i < NGRAY; i++)
                        h[i] += hList[hInd][i];
            }
        }
        private void buildLookupTables(float[][] P, float[][] S, float[][] H, float[] h)
        {
            // initialize
            for (int i = 0; i < NGRAY; ++i)
            {
                P[i] = new float[NGRAY];
                S[i] = new float[NGRAY];
                H[i] = new float[NGRAY];
            }
            // diagonal 
            for (int i = 1; i < NGRAY; ++i)
            {
                P[i][i] = h[i];
                S[i][i] = ((float)i) * h[i];
            }
            // calculate first row (row 0 is all zero)
            for (int i = 1; i < NGRAY - 1; ++i)
            {
                P[1][i + 1] = P[1][i] + h[i + 1];
                S[1][i + 1] = S[1][i] + ((float)(i + 1)) * h[i + 1];
            }
            // using row 1 to calculate others
            for (int i = 2; i < NGRAY; i++)
                for (int j = i + 1; j < NGRAY; j++)
                {
                    P[i][j] = P[1][j] - P[1][i - 1];
                    S[i][j] = S[1][j] - S[1][i - 1];
                }
            // now calculate H[i][j]
            for (int i = 1; i < NGRAY; ++i)
                for (int j = i + 1; j < NGRAY; j++)
                {
                    if (P[i][j] != 0)
                        H[i][j] = (S[i][j] * S[i][j]) / P[i][j];
                    else
                        H[i][j] = 0f;
                }
        }
        private float findMaxSigma(int mlevel, float[][] H, int[] t)
        {
            t[0] = 0;
            float maxSig = 0f;
            switch (mlevel)
            {
                case 2:
                    for (int i = 1; i < NGRAY - mlevel; i++) // t1
                    {
                        float Sq = H[1][i] + H[i + 1][255];
                        if (maxSig < Sq)
                        {
                            t[1] = i;
                            maxSig = Sq;
                        }
                    }
                    break;
                case 3:
                    for (int i = 1; i < NGRAY - mlevel; i++) // t1
                        for (int j = i + 1; j < NGRAY - mlevel + 1; j++) // t2
                        {
                            float Sq = H[1][i] + H[i + 1][j] + H[j + 1][255];
                            if (maxSig < Sq)
                            {
                                t[1] = i;
                                t[2] = j;
                                maxSig = Sq;
                            }
                        }
                    break;
                case 4:
                    for (int i = 1; i < NGRAY - mlevel; i++) // t1
                        for (int j = i + 1; j < NGRAY - mlevel + 1; j++) // t2
                            for (int k = j + 1; k < NGRAY - mlevel + 2; k++) // t3
                            {
                                float Sq = H[1][i] + H[i + 1][j] + H[j + 1][k] + H[k + 1][255];
                                if (maxSig < Sq)
                                {
                                    t[1] = i;
                                    t[2] = j;
                                    t[3] = k;
                                    maxSig = Sq;
                                }
                            }
                    break;
                case 5:
                    for (int i = 1; i < NGRAY - mlevel; i++) // t1
                        for (int j = i + 1; j < NGRAY - mlevel + 1; j++) // t2
                            for (int k = j + 1; k < NGRAY - mlevel + 2; k++) // t3
                                for (int m = k + 1; m < NGRAY - mlevel + 3; m++) // t4
                                {
                                    float Sq = H[1][i] + H[i + 1][j] + H[j + 1][k] + H[k + 1][m] + H[m + 1][255];
                                    if (maxSig < Sq)
                                    {
                                        t[1] = i;
                                        t[2] = j;
                                        t[3] = k;
                                        t[4] = m;
                                        maxSig = Sq;
                                    }
                                }
                    break;
            }
            return maxSig;
        }
        #endregion Segmentation
    }
}
