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
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace Cell_Tool_3
{
    class BrightnessAndContrast
    {
        public ImageAnalyser IA = null;
        private PropertiesPanel_Item PropPanel = new PropertiesPanel_Item();
        public Panel panel;
        //Chart
        public BrightnessAndContrast_ChartPanel Chart1 = new BrightnessAndContrast_ChartPanel();
        public BrightnessAndContrast_ChartPanel.BrightnessAndContrast_Chart.Series Values = new BrightnessAndContrast_ChartPanel.BrightnessAndContrast_Chart.Series();

        //CheckBoxes
        public CheckBox applyToAll = new CheckBox();
        public CheckBox autoDetect = new CheckBox();
        //previous info
        private int lastMin = 0;
        private int lastMax = 0;
        //tooltip 
        private ToolTip TurnOnToolTip = new ToolTip();
        //Chart_move event
        private bool moveMax = false;
        private bool moveMin = false;

        public Form_auxiliary FormBrightnessContrast; // For holding theGLControl of the historgram

        public void BackColor(Color color)
        {
            PropPanel.BackColor(color);
        }
        public void ForeColor(Color color)
        {
            PropPanel.ForeColor(color);
        }
        public void TitleColor(Color color)
        {
            PropPanel.TitleColor(color);
        }
        private void Control_MouseOver(object sender, EventArgs e)
        {
            var ctr = (Control)sender;
            TurnOnToolTip.SetToolTip(ctr, ctr.Tag.ToString());
        }
        public void Initialize(Panel propertiesPanel, Panel PropertiesBody)
        {
            //PropPanel properties
            PropPanel.Initialize(propertiesPanel);
            PropPanel.Resizable = true;
            PropPanel.Name.Text = "Brightness And Contrast";
            PropertiesBody.Controls.Add(PropPanel.Panel);

            panel = PropPanel.Panel;
            panel.Visible = false;
            //Create Interface
            //Prop Panel
            Panel Panel1 = new Panel();
            Panel1.Dock = DockStyle.Top;
            Panel1.Height = 25;
            PropPanel.Panel.Controls.Add(Panel1);
            Panel1.BringToFront();

            applyToAll.Text = "Apply to all channels";
            applyToAll.Tag = "Apply minimum and maximum to \nthe look-up tables of all color channels";
            applyToAll.Width = TextRenderer.MeasureText(applyToAll.Text, applyToAll.Font).Width + 20;
            applyToAll.Checked = false;
            applyToAll.Visible = false;
            autoDetect.Location = new Point(10, 3);
            applyToAll.MouseHover += new EventHandler(Control_MouseOver);
            applyToAll.CheckedChanged += new EventHandler(applyToAll_Checked);
            Panel1.Controls.Add(applyToAll);

            autoDetect.Text = "Auto bounds";
            autoDetect.Tag = "Automaticly adjust brightness and contrast \nby calculating minimum and maximum for the look-up table";
            autoDetect.Width = TextRenderer.MeasureText(autoDetect.Text, autoDetect.Font).Width + 20;
            autoDetect.Checked = true;
            int x = autoDetect.Width + 35;
            if (x + 40 > Panel1.Width) { x = Panel1.Width - 40; }
            applyToAll.Location = new Point(x, 3);
            autoDetect.CheckedChanged += new EventHandler(autoDetect_Checked);
            autoDetect.MouseHover += new EventHandler(Control_MouseOver);
            Panel1.Controls.Add(autoDetect);
            applyToAll.BringToFront();

            Panel1.Resize += new EventHandler(delegate (object o, EventArgs a)
            {
                int x1 = autoDetect.Width + 35;
                if (x1 + 40 > Panel1.Width) { x1 = Panel1.Width - 40; }
                applyToAll.Location = new Point(x1, 3);

            });
            //Chart
            BrightnessAndContrast_ChartPanel.BrightnessAndContrast_Chart CA = Chart1.CA;
            //CA.BackColor = PropPanel.Body.BackColor;
            CA.BackGroundColor = IA.FileBrowser.BackGroundColor1;
            CA.ShowMinAndMax = true;

            CA.Visible = true;

            Values.Enabled = true;
            Values.UseGradientStyle = true;
            Values.BorderColor = Color.White;
            Values.BackSecondaryColor = Color.White;
            Values.Color = Color.Black;
            Chart1.Series.Add(Values);

            Chart1.Dock = DockStyle.Fill;
            Chart1.BackColor = PropPanel.Body.BackColor;
            
            this.FormBrightnessContrast = new Form_auxiliary(this.panel, 10, 50, -10, -50);
            this.FormBrightnessContrast.Controls.Add(Chart1);
            //this.FormBrightnessContrast.Show();
            

            //PropPanel.Panel.Controls.Add(Chart1);
            CA.MouseMove += new MouseEventHandler(Chart1_MouseMove);
            CA.MouseDown += new MouseEventHandler(Chart1_MouseDown);
            CA.MouseUp += new MouseEventHandler(Chart1_MouseUp);
            CA.MouseClick += new MouseEventHandler(Chart1_MouseClick);
            //Chart1.MouseHover += new MouseEventHandler(Chart1_MouseHover);
            Chart1.BringToFront();
        }

        private void OptionForm_KeyPress(object sender, KeyEventArgs e)
        {

            if (e.KeyCode == Keys.Enter)
            {
                ((sender as Form).Tag as Button).Focus();
                ((sender as Form).Tag as Button).PerformClick();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                (sender as Form).Close();
                e.Handled = true;
            }
        }
        private void Chart1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) { return; }
            TifFileInfo fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;

            Form OptionForm = new Form();
            OptionForm.FormBorderStyle = FormBorderStyle.FixedDialog;
            OptionForm.Width = 200;
            OptionForm.Height = 180;
            OptionForm.Text = "LUT";
            OptionForm.StartPosition = FormStartPosition.CenterScreen;
            OptionForm.WindowState = FormWindowState.Normal;
            OptionForm.BackColor = PropPanel.Body.BackColor;
            //Add hot keys to main form
            OptionForm.KeyPreview = true;
            OptionForm.KeyDown += new KeyEventHandler(OptionForm_KeyPress);
            {
                Label l = new Label();
                l.Text = "Color:";
                l.Width = TextRenderer.MeasureText(l.Text, l.Font).Width;
                l.ForeColor = Color.White;
                l.Location = new Point(15, 15);
                OptionForm.Controls.Add(l);

                Button btn = new Button();
                btn.FlatAppearance.BorderSize = 0;
                btn.FlatStyle = FlatStyle.Flat;
                btn.Click += new EventHandler(changeLUT);
                btn.BackColor = fi.LutList[fi.cValue];
                btn.Location = new Point(85, 10);
                btn.Width = 75;
                btn.Height = 20;
                btn.KeyDown += new KeyEventHandler(OptionForm_KeyPress);
                OptionForm.Controls.Add(btn);

                btn.BringToFront();
            }
            TextBox tbMin = new TextBox();
            {
                Label l = new Label();
                l.Text = "Minimum:";
                l.Width = TextRenderer.MeasureText(l.Text, l.Font).Width;
                l.ForeColor = Color.White;
                l.Location = new Point(15, 45);
                OptionForm.Controls.Add(l);

                tbMin.Location = new Point(85, 40);
                tbMin.Width = 75;
                tbMin.Height = 20;
                tbMin.Text = fi.MinBrightness[fi.cValue].ToString();
                OptionForm.Controls.Add(tbMin);
                tbMin.BringToFront();
            }
            TextBox tbMax = new TextBox();
            {
                Label l = new Label();
                l.Text = "Maximum:";
                l.Width = TextRenderer.MeasureText(l.Text, l.Font).Width;
                l.ForeColor = Color.White;
                l.Location = new Point(15, 75);
                OptionForm.Controls.Add(l);

                tbMax.Location = new Point(85, 70);
                tbMax.Width = 75;
                tbMax.Height = 20;
                tbMax.Text = fi.MaxBrightness[fi.cValue].ToString();
                OptionForm.Controls.Add(tbMax);
                tbMax.BringToFront();
            }
            Button OkBtn = new Button();
            OptionForm.Tag = OkBtn;
            OkBtn.Text = "Apply";
            OkBtn.Width = 80;
            OkBtn.BackColor = SystemColors.ButtonFace;
            OkBtn.Location = new Point(50, 105);
            OptionForm.Controls.Add(OkBtn);
            OkBtn.Click += new EventHandler(delegate (object o, EventArgs a)
            {
                int min = 0;
                int max = 0;
                try { min = int.Parse(tbMin.Text); }
                catch
                {
                    MessageBox.Show("Incorrect minimum value!");
                    tbMin.Focus();
                    return;
                }

                if (min < 0)
                {
                    min = 0;
                }

                try { max = int.Parse(tbMax.Text); }
                catch
                {
                    MessageBox.Show("Incorrect maximum value!");
                    tbMax.Focus();
                    return;
                }
                if (max >= fi.histogramArray[fi.cValue].Length)
                {
                    max = fi.histogramArray[fi.cValue].Length - 1;
                    //IA.MarkAsNotSaved();
                }

                if (min > max)
                {
                    MessageBox.Show("Minimum must be lower then the maximum!");
                    return;
                }
                if (min != fi.MinBrightness[fi.cValue] | max != fi.MaxBrightness[fi.cValue])
                {
                    //turn of auto
                    if (autoDetect.Checked != false)
                    {
                        autoDetect.Checked = false;
                        fi.autoDetectBandC = false;
                        applyToAll.Visible = true;
                    }
                    //adjust brightness
                    MinPrew = fi.MinBrightness[fi.cValue];
                    MaxPrew = fi.MaxBrightness[fi.cValue];

                    adjustBrightness(min, max, fi, -1);

                    applyToHistory(fi.cValue, min, max);

                    //IA.MarkAsNotSaved();
                    //Load array to Chart
                    loadHistogramArray(fi);
                }
                OptionForm.Close();
            });

            // Linux change
            IA.FileBrowser.StatusLabel.Text = "Dialog open";
            OptionForm.ShowDialog();
            IA.FileBrowser.StatusLabel.Text = "Ready";
            foreach (Button btn in fi.tpTaskbar.ColorBtnList)
            {
                btn.Focus();
            }
        }

        private int MinPrew = 0;
        private int MaxPrew = 0;
        private void applyToHistory(int chanel, int min, int max)
        {
            TifFileInfo fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
            IA.delHist = true;
            IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI.delHist = true;
            IA.DeleteFromHistory();

            if (applyToAll.Checked == true & applyToAll.Visible == true)
            {
                for (int i = 0; i < fi.sizeC; i++)
                {
                    WriteApplyToHistory(i, min, max, fi);
                }
            }
            else
            {
                WriteApplyToHistory(chanel, min, max, fi);
            }

            IA.UpdateUndoBtns();
        }
        private void applyToAll_toHistory(TifFileInfo fi)
        {
            int curMin1 = fi.MinBrightness[fi.cValue];
            int curMax1 = fi.MaxBrightness[fi.cValue];
            int min = 0;
            int max = 0;

            IA.delHist = true;
            IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI.delHist = true;
            IA.DeleteFromHistory();

            for (int i = 0; i < fi.sizeC; i++)
            {
                min = fi.MinBrightness[i];
                max = fi.MaxBrightness[i];

                if (min != curMin1 | max != curMax1)
                {
                    fi.History.Add("B&C(" + i.ToString() + "," +
                    min.ToString() + "," +
                    max.ToString() + ")");
                    fi.History.Add("B&C(" + i.ToString() + "," +
                        curMin1.ToString() + "," +
                        curMax1.ToString() + ")");
                }
            }
            IA.UpdateUndoBtns();
        }
        private void autoBandC_toHistory(TifFileInfo fi)
        {
            int min = 0;
            int max = 0;

            IA.delHist = true;
            IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI.delHist = true;
            IA.DeleteFromHistory();

            for (int i = 0; i < fi.sizeC; i++)
            {
                min = fi.MinBrightness[i];
                max = fi.MaxBrightness[i];
                fi.History.Add("B&C(" + i.ToString() + "," +
                    min.ToString() + "," +
                    max.ToString() + ")");
                fi.History.Add("B&C(auto)");
            }
            IA.UpdateUndoBtns();
        }
        private void WriteApplyToHistory(int chanel, int min, int max, TifFileInfo fi)
        {
            string val = "B&C(" + chanel.ToString() + "," +
                    min.ToString() + "," +
                    max.ToString() + ")";
            if (IA.oldComand == val) { return; }

            fi.History.Add("B&C(" + chanel.ToString() + "," +
                    MinPrew.ToString() + "," +
                    MaxPrew.ToString() + ")");

            fi.History.Add(val);
            IA.oldComand = val;
            IA.CheckSizeHistory(fi);

        }

        private void changeLUT(object sender, EventArgs e)
        {
            TifFileInfo fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;

            ColorDialog colorDialog1 = new ColorDialog();
            colorDialog1.AllowFullOpen = true;
            colorDialog1.AnyColor = true;
            colorDialog1.FullOpen = true;
            colorDialog1.Color = fi.LutList[fi.cValue];
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
            // See if user pressed ok.
            if (result == DialogResult.OK)
            {
                (sender as Button).BackColor = colorDialog1.Color;

                IA.Input.ChangeValueFunction("LUT(" + fi.cValue.ToString() + "," +
                        ColorTranslator.ToHtml(colorDialog1.Color).ToString() + ")");

            }
        }
        public void SetBrightness(int chanel, int min, int max)
        {
            TifFileInfo fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
            if (chanel == -1) { chanel = fi.cValue; }
            fi.MinBrightness[chanel] = min;
            fi.MaxBrightness[chanel] = max;
        }
        private void Chart1_MouseDown(object sender, MouseEventArgs e)
        {
            double min = Chart1.CA.ValueToPixelPosition(lastMin);
            double max = Chart1.CA.ValueToPixelPosition(lastMax);
            MinPrew = lastMin;
            MaxPrew = lastMax;

            if ((e.X > min - 2 & e.X < min + 2))
            {
                Chart1.Cursor = Cursors.SizeWE;
                //var rc = Chart1.RectangleToScreen(new Rectangle(Point.Empty, Chart1.ClientSize));
                //System.Windows.Forms.Cursor.Clip = rc;
                //Chart1.Capture = true;
                moveMax = false;
                moveMin = true;
            }
            else if (e.X > max - 2 & e.X < max + 2)
            {
                Chart1.Cursor = Cursors.SizeWE;
                //var rc = Chart1.RectangleToScreen(new Rectangle(Point.Empty, Chart1.ClientSize));
                //System.Windows.Forms.Cursor.Clip = rc;
                //Chart1.Capture = true;

                moveMax = true;
                moveMin = false;
            }
            else
            {
                moveMax = false;
                moveMin = false;
            }
        }
        private void Chart1_MouseUp(object sender, MouseEventArgs e)
        {
            TifFileInfo fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
            moveMax = false;
            moveMin = false;

            applyToHistory(fi.cValue,
                fi.MinBrightness[fi.cValue],
                fi.MaxBrightness[fi.cValue]);

            Chart1.Cursor = Cursors.Default;
            //Chart1.Capture = false;
            //System.Windows.Forms.Cursor.Clip = new Rectangle(0, 0, 0, 0);
            calculateHistogramArray(fi, true);
            if (applyToAll.Checked == true & autoDetect.Checked == false)
            {
                int curC = fi.cValue;
                for (int c = 0; c < fi.sizeC; c++)
                {
                    fi.cValue = c;
                    IA.BandC.calculateHistogramArray(fi, true);
                }
                fi.cValue = curC;
                calculateHistogramArray(fi, true);
            }

            IA.ReloadImages();
        }
        private void Chart1_MouseMoveImageReload()
        {
            TifFileInfo fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;

            calculateHistogramArray(fi, true);
            if (applyToAll.Checked == true & autoDetect.Checked == false)
            {
                int curC = fi.cValue;
                for (int c = 0; c < fi.sizeC; c++)
                {
                    fi.cValue = c;
                    IA.BandC.calculateHistogramArray(fi, true);
                }
                fi.cValue = curC;
                calculateHistogramArray(fi, true);
            }

            Chart1.DrawToScreen(fi);
            Chart1.Update();
            Chart1.PerformLayout();

            IA.ReloadImages();
        }
        private void Chart1_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                TifFileInfo oldFI = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
                if (oldFI.histogramArray == null) { return; }
                if (moveMin == true)
                {
                    //higher then max
                    int MineX = e.X;
                    int maxeX = Convert.ToInt32(Chart1.CA.ValueToPixelPosition(oldFI.MaxBrightness[oldFI.cValue]) - 1);
                    if (e.X > maxeX) { MineX = maxeX; }
                    //lower then min
                    if (MineX < 0) { MineX = 0; }
                    int newMin = Convert.ToInt32(Chart1.CA.PixelPositionToValue(MineX));
                    //calculate
                    if (newMin != oldFI.MinBrightness[oldFI.cValue] & newMin < oldFI.MaxBrightness[oldFI.cValue] & newMin >= 0)
                    {
                        if (autoDetect.Checked != false)
                        {
                            autoDetect.Checked = false;
                            oldFI.autoDetectBandC = false;
                            applyToAll.Visible = true;
                        }

                        oldFI.MinBrightness[oldFI.cValue] = newMin;

                        // IA.MarkAsNotSaved();
                        Chart1_MouseMoveImageReload();
                    }

                }
                else if (moveMax == true)
                {
                    //calculate abs max
                    int absMax = 254;

                    TifFileInfo fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
                    if (fi == null) { return; }

                    if (fi.bitsPerPixel != 8) absMax = ushort.MaxValue;
                    //bigger then max
                    int maxeX = Convert.ToInt32(Chart1.CA.Width);
                    if (e.X < maxeX) { maxeX = e.X; }
                    //lower then min
                    int mineX = Convert.ToInt32(Chart1.CA.ValueToPixelPosition(oldFI.MinBrightness[oldFI.cValue])) + 1;
                    if (e.X < mineX) { maxeX = mineX; }
                    int newMax = Convert.ToInt32(Chart1.CA.PixelPositionToValue(maxeX));

                    //calculate max
                    if (newMax != oldFI.MaxBrightness[oldFI.cValue] & newMax > oldFI.MinBrightness[oldFI.cValue] & newMax <= absMax)
                    {
                        if (autoDetect.Checked != false)
                        {
                            autoDetect.Checked = false;
                            oldFI.autoDetectBandC = false;
                            applyToAll.Visible = true;
                        }

                        oldFI.MaxBrightness[oldFI.cValue] = newMax;

                        Chart1_MouseMoveImageReload();
                    }
                }
                else
                {
                    double min = Chart1.CA.ValueToPixelPosition(lastMin);
                    double max = Chart1.CA.ValueToPixelPosition(lastMax);
                    if ((e.X > min - 2 & e.X < min + 2) | (e.X > max - 2 & e.X < max + 2))
                    {
                        Chart1.Cursor = Cursors.SizeWE;
                    }
                    else
                    {
                        Chart1.Cursor = Cursors.Default;
                    }
                }
            }
            catch { }
        }

        private void autoDetect_Checked(object sender, EventArgs e)
        {
            if (autoDetect.Focused == false) { return; }

            TifFileInfo fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
            if (fi == null) { return; }
            if (autoDetect.Checked == true)
            {
                autoBandC_toHistory(fi);
            }
            //apply settings
            fi.autoDetectBandC = autoDetect.Checked;
            //reload
            calculateHistogramArray(fi, true);
            if (applyToAll.Checked == true & autoDetect.Checked == false)
            {
                int curC = fi.cValue;
                for (int c = 0; c < fi.sizeC; c++)
                {
                    fi.cValue = c;
                    IA.BandC.calculateHistogramArray(fi, true);
                }
                fi.cValue = curC;
                calculateHistogramArray(fi, true);
            }
            IA.ReloadImages();
            //IA.MarkAsNotSaved();
        }
        private void applyToAll_Checked(object sender, EventArgs e)
        {
            if (applyToAll.Focused == false) { return; }

            TifFileInfo fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
            if (fi == null) { return; }
            if (applyToAll.Checked == true & autoDetect.Checked == false)
            {
                applyToAll_toHistory(fi);
            }
            //apply settings
            fi.applyToAllBandC = applyToAll.Checked;
            //
            calculateHistogramArray(fi, true);
            //reload
            if (applyToAll.Checked == true & autoDetect.Checked == false)
            {
                int curC = fi.cValue;
                for (int c = 0; c < fi.sizeC; c++)
                {
                    fi.cValue = c;
                    IA.BandC.calculateHistogramArray(fi, true);
                }
                fi.cValue = curC;
                calculateHistogramArray(fi, true);
            }
            IA.ReloadImages();
            //IA.MarkAsNotSaved();
        }
        public void PrepareArray(TifFileInfo fi)
        {
            //prepare int array
            fi.histogramArray = new int[fi.sizeC][];
            fi.adjustedLUT = new float[fi.sizeC][];
            switch (fi.bitsPerPixel)
            {
                case 8:
                    for (int i = 0; i < fi.sizeC; i++)
                    {
                        fi.histogramArray[i] = new int[byte.MaxValue + 1];
                        fi.adjustedLUT[i] = new float[byte.MaxValue + 1];
                    }

                    break;
                case 16:
                    for (int i = 0; i < fi.sizeC; i++)
                    {
                        fi.histogramArray[i] = new int[ushort.MaxValue + 1];
                        fi.adjustedLUT[i] = new float[ushort.MaxValue + 1];
                    }

                    break;
            }
            //prepare min and max arrays
            if (fi.MinBrightness == null)
                fi.MinBrightness = new int[fi.sizeC];
            if (fi.MaxBrightness == null)
                fi.MaxBrightness = new int[fi.sizeC];
        }
        public void calculateHistogramArray(TifFileInfo fi, bool LoadChart)
        {
            TifFileInfo oldFI = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
            if (fi.image16bit == null & fi.image8bit == null) { return; }
            oldFI = fi;
            FrameCalculator FC = new FrameCalculator();
            int frame = FC.Frame(fi);
            if (fi.openedImages < frame) { return; }
            //Chech is array empty and fill it
            if (fi.histogramArray == null)
            {
                PrepareArray(fi);
            }
            //Empty current array
            for (int i = 0; i < fi.histogramArray[fi.cValue].Length; i++)
            {
                fi.histogramArray[fi.cValue][i] = 0;
            }
            //Fill the array
            bool check = false;
            switch (fi.bitsPerPixel)
            {
                case 8:
                    check = calculateHistogram8bit(fi, frame);
                    break;
                case 16:
                    check = calculateHistogram16bit(fi, frame);
                    break;
            }
            if (check == false)
            {
                return;
            }
            //Load array to Chart
            if (LoadChart == true)
            {
                loadHistogramArray(fi);
            }
        }
        private bool calculateHistogram8bit(TifFileInfo fi, int frame)
        {
            try
            {
                int cVal = fi.cValue;

                foreach (byte[] row in fi.image8bit[frame])
                {
                    foreach (byte val in row)
                    {
                        fi.histogramArray[cVal][val] += 1;
                    }
                }
                return true;
            }
            catch { return false; }

        }
        private bool calculateHistogram16bit(TifFileInfo fi, int frame)
        {
            try
            {
                int cVal = fi.cValue;
                foreach (ushort[] row in fi.image16bit[frame])
                {
                    foreach (ushort val in row)
                    {
                        fi.histogramArray[cVal][val] += 1;
                    }
                }
                return true;
            }
            catch { return false; }
        }
        private void loadHistogramArray(TifFileInfo fi)
        {
            //B&C min and B&C max
            DetectBandC(fi);
            //Don't reload chart if chart mouse down active
            if (moveMax || moveMin) return;
            //find range
            int correction = Convert.ToInt32(fi.MaxBrightness[fi.cValue] * 0.2);
            if (correction < 10) { correction = 10; }
            int range = fi.MaxBrightness[fi.cValue] + correction;
            int step = 1;
            if (range > byte.MaxValue)
            {
                step = range / byte.MaxValue;
            }
            //add values
            if (Values.Points.Count > 0) { Values.Points.Clear(); }

            int length = fi.histogramArray[fi.cValue].Length;
            for (int i = 0; i <= fi.MaxBrightness[fi.cValue] + correction; i += step)
            {
                int val = 0;

                for (int j = i; j < i + step; j++)
                {
                    if (0 <= j & j < length)
                    {
                        val += fi.histogramArray[fi.cValue][j];
                    }

                }
                Values.Points.AddXY(i, val);

            }

            //Color
            Values.BackSecondaryColor = fi.LutList[fi.cValue];


            Chart1.DrawToScreen(fi);

        }

        public void autoBrightness(TifFileInfo fi)
        {
            //pragova stoinost
            int dev = 0;
            //values
            int min = -1;
            int max = 0;
            for (int i = 0; i < fi.histogramArray[fi.cValue].Length; i++)
            {
                if (min == -1 & fi.histogramArray[fi.cValue][i] > dev) { min = i; }
                if (fi.histogramArray[fi.cValue][i] > dev) { max = i; }
            }
            if (min == -1) { min = 0; }
            //apply changes to fi
            adjustBrightness(min, max, fi, -1);
        }

        private void DetectBandC(TifFileInfo fi)
        {
            if (autoDetect.Checked == true)
            {
                applyToAll.Visible = false;
            }
            else
            {
                applyToAll.Visible = true;
            }

            if (autoDetect.Checked == true)
            {
                autoBrightness(fi);
            }
            else if (fi.MaxBrightness[fi.cValue] == 0)
            {
                autoBrightness(fi);
                /*
                int newMax = byte.MaxValue;
                if(fi.bitsPerPixel == 16) { newMax = ushort.MaxValue; }
                adjustBrightness(0, newMax, fi,-1);*/
            }
            else if (fi.MaxBrightness[fi.cValue] > 0)
            {
                adjustBrightness(fi.MinBrightness[fi.cValue], fi.MaxBrightness[fi.cValue], fi, -1);
            }
            else
            {
                adjustBrightness(lastMin, lastMax, fi, -1);
            }

        }
        public void adjustBrightness(int min, int max, TifFileInfo fi, int chanel)
        {
            if (chanel == -1) { chanel = fi.cValue; }
            if (applyToAll.Checked == true & applyToAll.Visible == true)
            {

                for (int i = 0; i < fi.MinBrightness.Length; i++)
                {
                    SetBrightness(i, min, max);
                }
            }
            else
            {
                SetBrightness(chanel, min, max);

            }
            lastMax = max;
            lastMin = min;
            adjustLUT(fi, chanel);
        }
        private void adjustLUT(TifFileInfo fi, int chanel)
        {
            try
            {
                //less then MinBrightness
                float val = 0f;

                for (int i = 0; i <= fi.MinBrightness[chanel]; i++)
                {
                    fi.adjustedLUT[chanel][i] = val;
                }
                //LUT
                float step = 0f;

                if (fi.bitsPerPixel == 16)
                {
                    float delitel = ushort.MaxValue;
                    step = (delitel / (fi.MaxBrightness[chanel] - fi.MinBrightness[chanel])) / delitel;
                }
                else if (fi.bitsPerPixel == 8)
                {
                    step = (((float)byte.MaxValue / (float)(fi.MaxBrightness[chanel] - fi.MinBrightness[chanel])) / (float)byte.MaxValue);
                }

                for (int i = fi.MinBrightness[chanel]; i <= fi.MaxBrightness[chanel]; i++, val += step)
                {
                    fi.adjustedLUT[chanel][i] = val;
                    if (fi.adjustedLUT[chanel][i] > 1f) fi.adjustedLUT[chanel][i] = 1f;
                }
                //Higher then Maxbrightness
                val = 1f;
                for (int i = fi.MaxBrightness[chanel] + 1; i < fi.adjustedLUT[chanel].Length; i++)
                {
                    fi.adjustedLUT[chanel][i] = val;
                }
            }
            catch { }
        }
    }
}