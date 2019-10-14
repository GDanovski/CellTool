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

namespace Cell_Tool_3
{
    class SpotDetector
    {
        #region Initialize
        //controls
        public ImageAnalyser IA;
        //thresh
        CTTrackBar thresh;
        CTTrackBar sensitivity;
        //select thresh
        ComboBox selectT = new ComboBox();
        ComboBox tType = new ComboBox();
        Button ColBtn = new Button();
        Button TailType = new Button();
        //tooltip 
        private ToolTip TurnOnToolTip = new ToolTip();
        
        public SpotDetector(Panel mainPanel, ImageAnalyser IA)
        {
            this.IA = IA;

            Panel p1 = new Panel();
            p1.Height = 5;
            p1.Dock = DockStyle.Top;
            mainPanel.Controls.Add(p1);
            p1.BringToFront();

            #region Threshold
            {
                CTTrackBar tb = new CTTrackBar();
                tb.Initialize();
                tb.Panel.Dock = DockStyle.Top;
                tb.BackColor(IA.FileBrowser.BackGround2Color1);
                tb.ForeColor(IA.FileBrowser.ShriftColor1);
                tb.Panel.Visible = true;
                tb.Refresh(0, 0, 10);
                tb.Name.Text = "Threshold:";
                tb.NamePanel.Width = 62;
                mainPanel.Controls.Add(tb.Panel);
                thresh = tb;
                tb.Panel.BringToFront();
                tb.Value.Changed += new ChangedValueEventHandler(delegate (Object o, ChangeValueEventArgs a)
                {
                    thresh_valueChange(int.Parse(a.Value));
                });
            }
            #endregion Threshold

            #region Options
            {
                CTTrackBar tb = new CTTrackBar();
                tb.Initialize();
                tb.Panel.Dock = DockStyle.Top;
                tb.BackColor(IA.FileBrowser.BackGround2Color1);
                tb.ForeColor(IA.FileBrowser.ShriftColor1);
                tb.Panel.Visible = true;
                tb.Refresh(100, 1, 100);
                tb.Name.Text = "Sensitivity:";
                tb.NamePanel.Width = 62;
                mainPanel.Controls.Add(tb.Panel);
                sensitivity = tb;
                tb.Panel.BringToFront();
                tb.Value.Changed += new ChangedValueEventHandler(delegate (Object o, ChangeValueEventArgs a)
                {
                    sensitivity_valueChange(int.Parse(a.Value));
                });
            }

            Panel p = new Panel();
            p.Height = 30;
            p.Dock = DockStyle.Top;
            mainPanel.Controls.Add(p);
            p.BringToFront();

            //Thresh list box
            selectT.Tag = "Select Threshold";
            selectT.Width = 60;
            selectT.Location = new Point(53, 1);
            selectT.Items.Add("None");
            selectT.DropDownStyle = ComboBoxStyle.DropDownList;
            selectT.SelectedIndex = 0;
            selectT.AutoSize = false;
            p.Controls.Add(selectT);
            selectT.BringToFront();
            selectT.MouseHover += Control_MouseOver;
            selectT.SelectedIndexChanged += selectT_IndexChange;

            //ThreshType box
            tType.Tag = "Set threshold type";
            tType.Width = 60;
            tType.Location = new Point(120, 1);
            tType.Items.AddRange(new string[] { "Pixels", "% Max"});
            tType.DropDownStyle = ComboBoxStyle.DropDownList;
            tType.SelectedIndex = 0;
            tType.AutoSize = false;
            p.Controls.Add(tType);
            tType.BringToFront();
            tType.MouseHover += Control_MouseOver;
            tType.SelectedIndexChanged += TType_IndexChange;
            //Color btn
            {
                Button btn = new Button();
                ColBtn = btn;

                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderSize = 0;
                btn.BackColor = Color.Red;
                btn.ForeColor = Color.Black;
                btn.Location = new Point(3, 1);
                btn.Text = "";
                btn.Tag = "Select color for the spots";
                btn.Height = 22;
                btn.Width = 25;
                p.Controls.Add(btn);
                btn.BringToFront();
                btn.Visible = true;
                btn.MouseHover += Control_MouseOver;
                btn.MouseDown += ColBtn_Click;
            }
            //<>btn
            {
                Button btn = new Button();
                TailType = btn;

                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderSize = 0;
                btn.ForeColor = Color.White;
                btn.Location = new Point(28, 1);
                btn.Text = ">";
                btn.Tag = "Set spot intensity diapason";
                btn.Height = 22;
                btn.Width = 25;
                p.Controls.Add(btn);
                btn.BringToFront();
                btn.Visible = true;
                btn.MouseHover += Control_MouseOver;
                btn.Click += TailType_Click;
            }

            #endregion Options
        }
        private void Control_MouseOver(object sender, EventArgs e)
        {
            var ctr = (Control)sender;
            TurnOnToolTip.SetToolTip(ctr, ctr.Tag.ToString());
        }
        #endregion Initialize

        #region Load Settings
        public void ReloadImage(TifFileInfo fi)
        {
            #region selectT load

            selectT.Items.Clear();
            selectT.Items.Add("None");
            selectT.Items.Add("Min");

            if(IA.Segmentation.SegmentationCBox.SelectedIndex == 1)
                for(int i = 0; i< fi.thresholds[fi.cValue];i++)
                    selectT.Items.Add("T" + (i + 1).ToString());

            selectT.Items.Add("Max");

            if (fi.SelectedSpotThresh[fi.cValue] < selectT.Items.Count)
                selectT.SelectedIndex = fi.SelectedSpotThresh[fi.cValue];
            else
                selectT.SelectedIndex = 0;

            #endregion region selectT load
            #region type thresh
            tType.SelectedIndex = fi.typeSpotThresh[fi.cValue];
            switch (tType.SelectedIndex)
            {
                case 0:
                    thresh.Refresh(fi.SpotThresh[fi.cValue], 0, Convert.ToInt32(fi.sizeX * fi.sizeY * 0.1));
                    break;
                case 1:
                    if(fi.SpotThresh[fi.cValue] <= 100)
                        thresh.Refresh(fi.SpotThresh[fi.cValue], 0, 100);
                    else
                    {
                        fi.SpotThresh[fi.cValue] = 0;
                        thresh.Refresh(0, 0, 100);
                    }  
                    break;
            }
            #endregion type thresh

            Color col = fi.SpotColor[fi.cValue];
            if (col != Color.Transparent)
            {
                ColBtn.BackColor = col;
                ColBtn.Text = "";
            }
            else
            {
                ColBtn.BackColor = Color.White;
                ColBtn.Text = "NaN";
            }

            TailType.Text = fi.SpotTailType[fi.cValue];

            sensitivity.Refresh(fi.spotSensitivity[fi.cValue], 1, 100);
        }
        #endregion Load Settings

        #region Events
        private void ColBtn_Click(object sender, MouseEventArgs e)
        {
            TifFileInfo fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
            Button btn = (Button)sender;

            if (e.Button == MouseButtons.Left)
            {
                //disable/enable color
                if (fi.SpotColor[fi.cValue] == Color.Transparent)
                    fi.SpotColor[fi.cValue] = fi.RefSpotColor[fi.cValue];
                else
                    fi.SpotColor[fi.cValue] = Color.Transparent;

                #region apply to history
                //segmentation.SpotDetector(chanelN,ThreshVal,HTML color, tail type, thresh type)
                Color val = fi.RefSpotColor[fi.cValue];
                Color oldVal = Color.Transparent;

                if (fi.SpotColor[fi.cValue] == Color.Transparent)
                { val = Color.Transparent; oldVal = fi.RefSpotColor[fi.cValue]; }

                fi.delHist = true;
                IA.delHist = true;
                IA.UnDoBtn.Enabled = true;
                IA.DeleteFromHistory();
                fi.History.Add("segmentation.SpotDetector("
                     + fi.cValue.ToString() + "," +
                     fi.SpotThresh[fi.cValue].ToString() + "," +
                     fi.spotSensitivity[fi.cValue].ToString() + "," +
                     ColorTranslator.ToHtml(oldVal) + "," +
                     fi.SpotTailType[fi.cValue] + "," +
                     fi.SelectedSpotThresh[fi.cValue].ToString() + "," +
                     fi.typeSpotThresh[fi.cValue].ToString() + ")");
               fi.History.Add("segmentation.SpotDetector("
                     + fi.cValue.ToString() + "," +
                     fi.SpotThresh[fi.cValue].ToString() + "," +
                     fi.spotSensitivity[fi.cValue].ToString() + "," +
                     ColorTranslator.ToHtml(val) + "," +
                     fi.SpotTailType[fi.cValue] + "," +
                     fi.SelectedSpotThresh[fi.cValue].ToString() + "," +
                     fi.typeSpotThresh[fi.cValue].ToString() + ")");
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
                colorDialog1.Color = fi.RefSpotColor[fi.cValue];
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
                    //segmentation.SpotDetector(chanelN,ThreshVal,HTML color, tail type, thresh type)
                    fi.delHist = true;
                    IA.delHist = true;
                    IA.UnDoBtn.Enabled = true;
                    IA.DeleteFromHistory();
                    fi.History.Add("segmentation.SpotDetector("
                         + fi.cValue.ToString() + "," +
                         fi.SpotThresh[fi.cValue].ToString() + "," +
                         fi.spotSensitivity[fi.cValue].ToString() + "," +
                         ColorTranslator.ToHtml(fi.RefSpotColor[fi.cValue]) + "," +
                         fi.SpotTailType[fi.cValue] + "," +
                         fi.SelectedSpotThresh[fi.cValue].ToString() + "," +
                         fi.typeSpotThresh[fi.cValue].ToString() + ")");
                    fi.History.Add("segmentation.SpotDetector("
                         + fi.cValue.ToString() + "," +
                         fi.SpotThresh[fi.cValue].ToString() + "," +
                         fi.spotSensitivity[fi.cValue].ToString() + "," +
                         ColorTranslator.ToHtml(colorDialog1.Color) + "," +
                         fi.SpotTailType[fi.cValue] + "," +
                         fi.SelectedSpotThresh[fi.cValue].ToString() + "," +
                         fi.typeSpotThresh[fi.cValue].ToString() + ")");
                    IA.UpdateUndoBtns();
                    IA.MarkAsNotSaved();
                    #endregion apply to history

                    fi.RefSpotColor[fi.cValue]= colorDialog1.Color;
                    fi.SpotColor[fi.cValue] = colorDialog1.Color;
                    IA.ReloadImages();
                }
            }
        }
        private void TailType_Click(object sender, EventArgs e)
        {
            TifFileInfo fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
            Button btn = (Button)sender;
            string oldVal = "";
            string val = "";

            if(btn.Text == "<")
            {
                fi.SpotTailType[fi.cValue] = ">";
                oldVal = "<";
                val = ">";
            }
            else
            {
                fi.SpotTailType[fi.cValue] = "<";
                oldVal = ">";
                val = "<";
            }

            #region apply to history
            //segmentation.SpotDetector(chanelN,ThreshVal,HTML color, tail type, thresh type)
            fi.delHist = true;
            IA.delHist = true;
            IA.UnDoBtn.Enabled = true;
            IA.DeleteFromHistory();
            fi.History.Add("segmentation.SpotDetector("
                 + fi.cValue.ToString() + "," +
                 fi.SpotThresh[fi.cValue].ToString() + "," +
                 fi.spotSensitivity[fi.cValue].ToString() + "," +
                 ColorTranslator.ToHtml(fi.SpotColor[fi.cValue]) + "," +
                 oldVal + "," +
                 fi.SelectedSpotThresh[fi.cValue].ToString() + "," +
                 fi.typeSpotThresh[fi.cValue].ToString() + ")");
            fi.History.Add("segmentation.SpotDetector("
                 + fi.cValue.ToString() + "," +
                 fi.SpotThresh[fi.cValue].ToString() + "," +
                 fi.spotSensitivity[fi.cValue].ToString() + "," +
                 ColorTranslator.ToHtml(fi.SpotColor[fi.cValue]) + "," +
                 val + "," +
                 fi.SelectedSpotThresh[fi.cValue].ToString() + "," +
                 fi.typeSpotThresh[fi.cValue].ToString() + ")");
            IA.UpdateUndoBtns();
            IA.MarkAsNotSaved();
            #endregion apply to history

            IA.ReloadImages();
        }
        private void sensitivity_valueChange(int val)
        {
            TifFileInfo fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;

            #region apply to history
            //segmentation.SpotDetector(chanelN,ThreshVal,HTML color, tail type, thresh type)
            fi.delHist = true;
            IA.delHist = true;
            IA.UnDoBtn.Enabled = true;
            IA.DeleteFromHistory();
            fi.History.Add("segmentation.SpotDetector("
                 + fi.cValue.ToString() + "," +
                 fi.SpotThresh[fi.cValue].ToString() + "," +
                 fi.spotSensitivity[fi.cValue].ToString() + "," +
                 ColorTranslator.ToHtml(fi.SpotColor[fi.cValue]) + "," +
                 fi.SpotTailType[fi.cValue] + "," +
                 fi.SelectedSpotThresh[fi.cValue].ToString() + "," +
                 fi.typeSpotThresh[fi.cValue].ToString() + ")");
            fi.History.Add("segmentation.SpotDetector("
                 + fi.cValue.ToString() + "," +
                 fi.SpotThresh[fi.cValue].ToString() + "," +
                 val.ToString() + "," +
                 ColorTranslator.ToHtml(fi.SpotColor[fi.cValue]) + "," +
                 fi.SpotTailType[fi.cValue] + "," +
                 fi.SelectedSpotThresh[fi.cValue].ToString() + "," +
                 fi.typeSpotThresh[fi.cValue].ToString() + ")");
            IA.UpdateUndoBtns();
            IA.MarkAsNotSaved();
            #endregion apply to history
            fi.spotSensitivity[fi.cValue] = val;

            IA.ReloadImages();
        }
        private void thresh_valueChange(int val)
        {
            TifFileInfo fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;

            #region apply to history
            //segmentation.SpotDetector(chanelN,ThreshVal,HTML color, tail type, thresh type)
            fi.delHist = true;
            IA.delHist = true;
            IA.UnDoBtn.Enabled = true;
            IA.DeleteFromHistory();
            fi.History.Add("segmentation.SpotDetector("
                 + fi.cValue.ToString() + "," +
                 fi.SpotThresh[fi.cValue].ToString() + "," +
                 fi.spotSensitivity[fi.cValue].ToString() + "," +
                 ColorTranslator.ToHtml(fi.SpotColor[fi.cValue]) + "," +
                 fi.SpotTailType[fi.cValue] + "," +
                 fi.SelectedSpotThresh[fi.cValue].ToString() + "," +
                 fi.typeSpotThresh[fi.cValue].ToString() + ")");
            fi.History.Add("segmentation.SpotDetector("
                 + fi.cValue.ToString() + "," +
                 val.ToString() + "," +
                 fi.spotSensitivity[fi.cValue].ToString() + "," +
                 ColorTranslator.ToHtml(fi.SpotColor[fi.cValue]) + "," +
                 fi.SpotTailType[fi.cValue] + "," +
                 fi.SelectedSpotThresh[fi.cValue].ToString() + "," +
                 fi.typeSpotThresh[fi.cValue].ToString() + ")");
            IA.UpdateUndoBtns();
            IA.MarkAsNotSaved();
            #endregion apply to history
            fi.SpotThresh[fi.cValue] = val;

            IA.ReloadImages();
        }
        private void TType_IndexChange(object sender, EventArgs e)
        {
            ComboBox cb = (ComboBox)sender;
            if (cb.Focused == false) return;

            TifFileInfo fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;

            if (fi.typeSpotThresh[fi.cValue] == cb.SelectedIndex) return;
            
            #region apply to history
            //segmentation.SpotDetector(chanelN,ThreshVal,HTML color, tail type, thresh type)
            fi.delHist = true;
            IA.delHist = true;
            IA.UnDoBtn.Enabled = true;
            IA.DeleteFromHistory();
            fi.History.Add("segmentation.SpotDetector("
                 + fi.cValue.ToString() + "," +
                 fi.SpotThresh[fi.cValue].ToString() + "," +
                 fi.spotSensitivity[fi.cValue].ToString() + "," +
                 ColorTranslator.ToHtml(fi.SpotColor[fi.cValue]) + "," +
                 fi.SpotTailType[fi.cValue] + "," +
                 fi.SelectedSpotThresh[fi.cValue].ToString() + "," +
                 fi.typeSpotThresh[fi.cValue].ToString() + ")");
            fi.History.Add("segmentation.SpotDetector("
                 + fi.cValue.ToString() + "," +
                  "0," +
                  fi.spotSensitivity[fi.cValue].ToString() + "," +
                 ColorTranslator.ToHtml(fi.SpotColor[fi.cValue]) + "," +
                 fi.SpotTailType[fi.cValue] + "," +
                 fi.SelectedSpotThresh[fi.cValue].ToString() + "," +
                 cb.SelectedIndex.ToString() + ")");
            IA.UpdateUndoBtns();
            IA.MarkAsNotSaved();
            #endregion apply to history

            fi.SpotThresh[fi.cValue] = 0;
            fi.typeSpotThresh[fi.cValue] = cb.SelectedIndex;
            IA.ReloadImages();
        }
        private void selectT_IndexChange(object sender,EventArgs e)
        {
            ComboBox cb = (ComboBox)sender;
            if (cb.Focused == false) return;

            TifFileInfo fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;

            if (fi.SelectedSpotThresh[fi.cValue] == cb.SelectedIndex) return;

            #region apply to history
            //segmentation.SpotDetector(chanelN,ThreshVal,HTML color, tail type, thresh type)
            fi.delHist = true;
            IA.delHist = true;
            IA.UnDoBtn.Enabled = true;
            IA.DeleteFromHistory();
            fi.History.Add("segmentation.SpotDetector("
                 + fi.cValue.ToString() + "," +
                 fi.SpotThresh[fi.cValue].ToString() + "," +
                 fi.spotSensitivity[fi.cValue].ToString() + "," +
                 ColorTranslator.ToHtml(fi.SpotColor[fi.cValue]) + "," +
                 fi.SpotTailType[fi.cValue] + "," +
                 fi.SelectedSpotThresh[fi.cValue].ToString() + "," +
                 fi.typeSpotThresh[fi.cValue].ToString() + ")");
            fi.History.Add("segmentation.SpotDetector("
                 + fi.cValue.ToString() + "," +
                 fi.SpotThresh[fi.cValue].ToString() + "," +
                 fi.spotSensitivity[fi.cValue].ToString() + "," +
                 ColorTranslator.ToHtml(fi.SpotColor[fi.cValue]) + "," +
                 fi.SpotTailType[fi.cValue] + "," +
                 cb.SelectedIndex.ToString() + "," +
                 fi.typeSpotThresh[fi.cValue].ToString() + ")");
            IA.UpdateUndoBtns();
            IA.MarkAsNotSaved();
            #endregion apply to history

            fi.SelectedSpotThresh[fi.cValue] = cb.SelectedIndex;
            IA.ReloadImages();
        }
        #endregion Events

        #region Processing
        public int[] CalculateBorders(TifFileInfo fi, int C, int imageN = -1)
        {
            try {
                if (fi.SelectedSpotThresh[C] == 0) return new int[] { 0, 0 };
                //find borders
                List<int> l = new List<int>();
                l.Add(0);
                l.Add(0);
                if (fi.SegmentationCBoxIndex[C] != 0)
                    for (int i = 1; i <= fi.thresholds[C]; i++)
                        l.Add(fi.thresholdValues[C][i]);
                
                //calculate histogram
                int[] h = null;
                switch (fi.bitsPerPixel)
                {
                    case 8:
                        h = calculate8bitHisogram(fi, C, imageN);
                        l.Add(byte.MaxValue);
                        l.Add(byte.MaxValue);
                        break;
                    case 16:
                        h = calculate16bitHisogram(fi, C, imageN);
                        l.Add(ushort.MaxValue);
                        l.Add(ushort.MaxValue);
                        break;
                }

                //calculate large diapason
                int minVal = l[fi.SelectedSpotThresh[C] - 1];
                int maxVal = l[fi.SelectedSpotThresh[C]];
                if (fi.SpotTailType[C] == ">")
                {
                    minVal = l[fi.SelectedSpotThresh[C]];
                    maxVal = l[fi.SelectedSpotThresh[C] + 1];
                }
                int step = 101 - fi.spotSensitivity[C];
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

                int val = fi.SpotThresh[C];
                if (fi.typeSpotThresh[C] == 1) { val = Convert.ToInt32((MaxVal / 100) * val); }

                bool changed = false;
                switch (fi.SpotTailType[C])
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
                if(changed == false) return new int[] { 0, 0 };

                res[0] *= step;
                res[1] *= step;

                return res;
            }
            catch
            {
                return new int[] { 0, 0 };
            }
        }

        private int[] calculate8bitHisogram(TifFileInfo fi, int C, int imageN = -1)
        {
            FrameCalculator FC = new FrameCalculator();
            int frame = FC.FrameC(fi, C);
            if (imageN != -1) frame = imageN;

            int step = 101 - fi.spotSensitivity[C];
            int[] res = new int[Convert.ToInt32(byte.MaxValue/step) + 1];
            
            //calculate histogram
            foreach (byte[] row in fi.image8bitFilter[frame])
                foreach (byte val in row)
                    res[Convert.ToInt32(val / step)]++;

            return res;
        }
        private int[] calculate16bitHisogram(TifFileInfo fi, int C, int imageN = -1)
        {
            FrameCalculator FC = new FrameCalculator();
            int frame = FC.FrameC(fi, C);
            if (imageN != -1) frame = imageN;

            int step = 101 - fi.spotSensitivity[C];
            int[] res = new int[Convert.ToInt32(ushort.MaxValue / step) + 1];

            //calculate histogram
            foreach (ushort[] row in fi.image16bitFilter[frame])
                foreach (ushort val in row)
                    res[Convert.ToInt32(val / step)]++;

            return res;
        }
        #endregion Processing

    }
}
