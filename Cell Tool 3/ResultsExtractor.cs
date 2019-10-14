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
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using System.ComponentModel;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using System.Drawing.Imaging;
using NCalc;
using System.Threading;
using System.Globalization;

namespace Cell_Tool_3
{
   class ResultsExtractor
    {
        public Panel myPanel = null;
        private ImageDrawer IDrawer;
        public Panel Input(string dir, ImageAnalyser IA)
        {
            MyForm form1 = new MyForm(dir,IA);
            this.myPanel = form1;

            if (File.Exists(OSStringConverter.StringToDir(dir)))
                ResultsExtractor.FileSaver.ReadCTDataFile(form1, dir);

            return form1;
            //form1.Show();
        }
        public class Parametars
        {
            //Colors
            public static Color BackGroundColor = Color.DimGray;
            public static Color BackGround2Color = Color.FromArgb(255, 60, 60, 60);
            public static Color ShriftColor = Color.White;
            public static Color TaskBtnClickedColor = Color.FromArgb(255, 150, 150, 150);
            public static Color TaskBtnColor = Color.DarkGray;
            public static Color TitlePanelColor = Color.CornflowerBlue;
        }
        public class MyForm : Panel
        {
            public ImageAnalyser IA;
            /// StatusBar
            public ToolStripProgressBar StatusProgressBar = new ToolStripProgressBar();
            public ToolStripStatusLabel StatusLabel = new ToolStripStatusLabel();
            //Panels
            private PropertiesPanel_Item FitChartPanel = new PropertiesPanel_Item();
            private PropertiesPanel_Item ParametersPanel = new PropertiesPanel_Item();
            private PropertiesPanel_Item FitHistoryPanel = new PropertiesPanel_Item();
            private Panel FitPropertiesPanel = new Panel();
            PropertiesPanel_Item ResultPanel = new PropertiesPanel_Item();
            PropertiesPanel_Item SettingsPanel = new PropertiesPanel_Item();
            PropertiesPanel_Item FiltersPanel = new PropertiesPanel_Item();
            PropertiesPanel_Item DataPanel = new PropertiesPanel_Item();
            Panel ExtrPropertiesPanel = new Panel();
            
            private bool resizing = false;
            public FilterTV filterTV;
            public DataTV dataTV;
            public Label CheckCounter = new Label();
            public ResultsChart resultsCh;
            public RepeatsChart repeatsCh;
            public ChartSolverSettings solverClass;

            public CheckBox NormCB = new CheckBox();
            public CheckBox dNormCB = new CheckBox();
            public RadioButton NormTo1 = new RadioButton();
            public RadioButton NormFrom0To1 = new RadioButton();
            public CheckBox StDevCB = new CheckBox();
            public CheckBox SubsetCB = new CheckBox();
            
            public MyForm(string dir, ImageAnalyser IA)
            {
                this.IA = IA;
                filterTV = new FilterTV(this);
                dataTV = new DataTV(dir, StatusLabel, this);
                resultsCh = new ResultsChart(this);
                repeatsCh = new RepeatsChart(this);
                solverClass = new ChartSolverSettings(this);

                this.BackColor = Parametars.BackGroundColor;
                this.Width = 800;
                this.Height = 500;                
                this.Text = "Results Extractor";
                this.SuspendLayout();

                AddMenuStrip();
                AddFooter();
                AddSettings();
                AddFitSettings();
                AddCharts();

                AdjustSize();
                AddResizeHandlers();

                this.ResumeLayout();
            }
            private void AddResizeHandlers()
            {
                this.FiltersPanel.Panel.SizeChanged += Resize_propPan;
                this.ResultPanel.Panel.SizeChanged += Resize_propPan;
                this.FitChartPanel.Panel.SizeChanged += Resize_propPan;
                this.FitHistoryPanel.Panel.SizeChanged += Resize_propPan;
            }
            
            private void Resize_propPan(object sender, EventArgs e)
            {
                if (((Panel)sender).Height > 30)
                {
                    SaveSize();
                }
            }
           
            public void AdjustSize()
            {
                if (Properties.Settings.Default.ResultsExtractorSizes[IA.FileBrowser.ActiveAccountIndex] == "@")
                {
                    SaveSize();
                    return;
                }

                string[] vals = Properties.Settings.Default.ResultsExtractorSizes[IA.FileBrowser.ActiveAccountIndex].Split('\t');

                this.Width = int.Parse(vals[0]);
                this.Height = int.Parse(vals[1]);

                this.FiltersPanel.Height = int.Parse(vals[2]);
                if (this.FiltersPanel.Panel.Height > 30)
                    this.FiltersPanel.Panel.Height = int.Parse(vals[2]);

                this.ExtrPropertiesPanel.Width = int.Parse(vals[3]);

                this.ResultPanel.Height = int.Parse(vals[4]);
                if (this.ResultPanel.Panel.Height > 30)
                    this.ResultPanel.Panel.Height = int.Parse(vals[4]);

                this.FitPropertiesPanel.Width = int.Parse(vals[5]);

                this.FitChartPanel.Height = int.Parse(vals[6]);
                if (this.FitChartPanel.Panel.Height > 30)
                    this.FitChartPanel.Panel.Height = int.Parse(vals[6]);

                this.FitHistoryPanel.Height = int.Parse(vals[7]);
                if (this.FitHistoryPanel.Panel.Height > 30)
                    this.FitHistoryPanel.Panel.Height = int.Parse(vals[7]);

                //Check sizes
                if (this.Width - (this.FitPropertiesPanel.Width + this.ExtrPropertiesPanel.Width) < 60)
                {
                    int W = (int)(this.Width / 2) - 60;
                    this.FitPropertiesPanel.Width = W;
                    this.ExtrPropertiesPanel.Width = W;
                }
            }
            public void SaveSize()
            {
                if (this.Dock != DockStyle.Fill || this.Parent==null || !this.Parent.Visible) return;

                string[] vals = new string[]
                {
                    this.Width.ToString(),
                    this.Height.ToString(),
                    this.FiltersPanel.Height.ToString(),
                    this.ExtrPropertiesPanel.Width.ToString(),
                    this.ResultPanel.Height.ToString(),
                    this.FitPropertiesPanel.Width.ToString(),
                    this.FitChartPanel.Height.ToString(),
                    this.FitHistoryPanel.Height.ToString()
                };

                Properties.Settings.Default.ResultsExtractorSizes[IA.FileBrowser.ActiveAccountIndex] =
                    string.Join("\t",vals);
                Properties.Settings.Default.Save();
                
            }
            private void AddCharts()
            {
                Panel chartPanel = new Panel();
                chartPanel.AutoScroll = true;
                chartPanel.Dock = DockStyle.Fill;
                this.Controls.Add(chartPanel);
                chartPanel.BringToFront();
                //chartPanel.MinimumSize = new Size(60, 100);

                PropertiesPanel_Item RepeatsChartPanel = new PropertiesPanel_Item();
                RepeatsChartPanel.Initialize(chartPanel);
                RepeatsChartPanel.BackColor(Parametars.BackGround2Color);
                RepeatsChartPanel.ForeColor(Parametars.ShriftColor);
                RepeatsChartPanel.TitleColor(Parametars.TitlePanelColor);
                RepeatsChartPanel.Resizable = false;
                RepeatsChartPanel.Name.Text = "Repeats";
                RepeatsChartPanel.Panel.Visible = true;
                RepeatsChartPanel.Panel.Dock = DockStyle.Fill;
                chartPanel.Controls.Add(RepeatsChartPanel.Panel);
                                
                ResultPanel.Initialize(chartPanel);
                ResultPanel.BackColor(Parametars.BackGround2Color);
                ResultPanel.ForeColor(Parametars.ShriftColor);
                ResultPanel.TitleColor(Parametars.TitlePanelColor);
                ResultPanel.Resizable = true;
                ResultPanel.Name.Text = "Results";
                ResultPanel.Panel.Visible = true;
                ResultPanel.Height = 250;
                ResultPanel.Panel.Height = 250;
                chartPanel.Controls.Add(ResultPanel.Panel);

                RepeatsChartPanel.Body.Controls.Add(repeatsCh);
                ResultPanel.Body.Controls.Add(resultsCh);
            }
            private void AddFitSettings()
            {
                Panel propertiesPanel = FitPropertiesPanel;
                propertiesPanel.AutoScroll = true;
                propertiesPanel.BackColor = Parametars.BackGroundColor;
                propertiesPanel.Width = 200;
                propertiesPanel.Dock = DockStyle.Right;
                this.Controls.Add(propertiesPanel);
                propertiesPanel.BringToFront();
                propertiesPanel.MinimumSize = new Size(100, 100);

                Panel midPanel = new Panel();
                midPanel.Tag = propertiesPanel;
                midPanel.Width = 5;
                midPanel.Dock = DockStyle.Right;
                this.Controls.Add(midPanel);
                midPanel.BringToFront();
                midPanel.MouseDown += Resize_MouseDown;
                midPanel.MouseUp += Resize_MouseUp;
                midPanel.MouseMove += Resize_MouseMove;
                midPanel.MouseEnter += Resize_MouseEnter;
                midPanel.MouseLeave += Resize_MouseLeave;
                
                FitHistoryPanel.Initialize(propertiesPanel);
                FitHistoryPanel.BackColor(Parametars.BackGround2Color);
                FitHistoryPanel.ForeColor(Parametars.ShriftColor);
                FitHistoryPanel.TitleColor(Parametars.TitlePanelColor);
                FitHistoryPanel.Resizable = true;
                FitHistoryPanel.Name.Text = "Fits";
                FitHistoryPanel.Panel.Visible = true;
                FitHistoryPanel.Panel.Height = 160;
                FitHistoryPanel.Height = 160;
                propertiesPanel.Controls.Add(FitHistoryPanel.Panel);
                FitHistoryPanel.Body.Controls.Add(solverClass.fitData);
                solverClass.fitData.Dock = DockStyle.Fill;
                solverClass.fitData.BringToFront();
                
                ParametersPanel.Initialize(propertiesPanel);
                ParametersPanel.BackColor(Parametars.BackGround2Color);
                ParametersPanel.ForeColor(Parametars.ShriftColor);
                ParametersPanel.TitleColor(Parametars.TitlePanelColor);
                ParametersPanel.Resizable = false;
                ParametersPanel.Name.Text = "Solver Settings";
                ParametersPanel.Panel.Visible = true;
                ParametersPanel.Panel.Height = 80;
                ParametersPanel.Height = 80;
                propertiesPanel.Controls.Add(ParametersPanel.Panel);
                ParametersPanel.Body.Controls.Add(solverClass.parametersPanel);
                solverClass.parametersPanel.BringToFront();
                ParametersPanel.Body.BringToFront();
                solverClass.parametersPanel.SizeChanged += new EventHandler(delegate (object o, EventArgs a)
                {
                    if (ParametersPanel.Panel.Height > 30)
                    {
                        ParametersPanel.Height = 35 + solverClass.parametersPanel.Height;
                        ParametersPanel.Panel.Height = ParametersPanel.Height;
                    }
                });
                ParametersPanel.Height = 35 + solverClass.parametersPanel.Height;
                ParametersPanel.Panel.Height = ParametersPanel.Height;

               
                FitChartPanel.Initialize(propertiesPanel);
                FitChartPanel.BackColor(Parametars.BackGround2Color);
                FitChartPanel.ForeColor(Parametars.ShriftColor);
                FitChartPanel.TitleColor(Parametars.TitlePanelColor);
                FitChartPanel.Resizable = true;
                FitChartPanel.Panel.Visible = true;
                FitChartPanel.Panel.Height = 160;
                FitChartPanel.Height = 160;
                FitChartPanel.Name.Text = "Fitting Result";
                FitChartPanel.Panel.Visible = true;
                propertiesPanel.Controls.Add(FitChartPanel.Panel);

                FitChartPanel.Body.Controls.Add(solverClass.fitChart1);
                FitChartPanel.Body.Controls.Add(solverClass.fitChart1.Titles);
                
                /*
                PropertiesPanel_Item DataPanel = new PropertiesPanel_Item();
                DataPanel.Initialize(propertiesPanel);
                DataPanel.BackColor(Parametars.BackGround2Color);
                DataPanel.ForeColor(Parametars.ShriftColor);
                DataPanel.TitleColor(Parametars.TitlePanelColor);
                DataPanel.Resizable = false;
                DataPanel.Name.Text = "Data";
                DataPanel.Panel.Visible = true;
                DataPanel.Panel.Dock = DockStyle.Fill;
                propertiesPanel.Controls.Add(DataPanel.Panel);

                PropertiesPanel_Item SettingsPanel = new PropertiesPanel_Item();
                SettingsPanel.Initialize(propertiesPanel);
                SettingsPanel.BackColor(Parametars.BackGround2Color);
                SettingsPanel.ForeColor(Parametars.ShriftColor);
                SettingsPanel.TitleColor(Parametars.TitlePanelColor);
                SettingsPanel.Resizable = false;
                SettingsPanel.Name.Text = "Settings";
                SettingsPanel.Panel.Visible = true;
                SettingsPanel.Panel.Height = 80;
                SettingsPanel.Height = 80;
                propertiesPanel.Controls.Add(SettingsPanel.Panel);

                PropertiesPanel_Item FiltersPanel = new PropertiesPanel_Item();
                FiltersPanel.Initialize(propertiesPanel);
                FiltersPanel.BackColor(Parametars.BackGround2Color);
                FiltersPanel.ForeColor(Parametars.ShriftColor);
                FiltersPanel.TitleColor(Parametars.TitlePanelColor);
                FiltersPanel.Resizable = true;
                FiltersPanel.Name.Text = "Filters";
                FiltersPanel.Panel.Visible = true;
                FiltersPanel.Panel.Height = 80;
                FiltersPanel.Height = 80;
                propertiesPanel.Controls.Add(FiltersPanel.Panel);

                FiltersPanel.Body.Controls.Add(filterTV);
                AddSettings(SettingsPanel.Body);
                DataPanel.Body.Controls.Add(dataTV);

                Panel checkCounterPanel = new Panel();
                checkCounterPanel.AutoScroll = false;
                checkCounterPanel.Height = 20;
                checkCounterPanel.Dock = DockStyle.Bottom;
                DataPanel.Body.Controls.Add(checkCounterPanel);

                CheckCounter.Location = new Point(5, 5);
                checkCounterPanel.Controls.Add(CheckCounter);

                LoadOldFilters();
                */
            }
            private void AddSettings()
            {
                Panel propertiesPanel = ExtrPropertiesPanel;
                propertiesPanel.AutoScroll = true;
                propertiesPanel.Width = 200;
                propertiesPanel.Dock = DockStyle.Left;
                this.Controls.Add(propertiesPanel);
                propertiesPanel.BringToFront();
                propertiesPanel.MinimumSize = new Size(100, 100);

                Panel midPanel = new Panel();
                midPanel.Tag = propertiesPanel;
                midPanel.Width = 5;
                midPanel.Dock = DockStyle.Left;
                this.Controls.Add(midPanel);
                midPanel.BringToFront();
                midPanel.MouseDown += Resize_MouseDown;
                midPanel.MouseUp += Resize_MouseUp;
                midPanel.MouseMove += Resize_MouseMove;
                midPanel.MouseEnter += Resize_MouseEnter;
                midPanel.MouseLeave += Resize_MouseLeave;
               
                DataPanel.Initialize(propertiesPanel);
                DataPanel.BackColor(Parametars.BackGround2Color);
                DataPanel.ForeColor(Parametars.ShriftColor);
                DataPanel.TitleColor(Parametars.TitlePanelColor);
                DataPanel.Resizable = false;
                DataPanel.Name.Text = "Data";
                DataPanel.Panel.Visible = true;
                DataPanel.Panel.Dock = DockStyle.Fill;
                propertiesPanel.Controls.Add(DataPanel.Panel);

                
                SettingsPanel.Initialize(propertiesPanel);
                SettingsPanel.BackColor(Parametars.BackGround2Color);
                SettingsPanel.ForeColor(Parametars.ShriftColor);
                SettingsPanel.TitleColor(Parametars.TitlePanelColor);
                SettingsPanel.Resizable = false;
                SettingsPanel.Name.Text = "Settings";
                SettingsPanel.Panel.Visible = true;
                SettingsPanel.Panel.Height = 165;
                SettingsPanel.Height = 165;
                propertiesPanel.Controls.Add(SettingsPanel.Panel);

               
                FiltersPanel.Initialize(propertiesPanel);
                FiltersPanel.BackColor(Parametars.BackGround2Color);
                FiltersPanel.ForeColor(Parametars.ShriftColor);
                FiltersPanel.TitleColor(Parametars.TitlePanelColor);
                FiltersPanel.Resizable = true;
                FiltersPanel.Name.Text = "Filters";
                FiltersPanel.Panel.Visible = true;
                FiltersPanel.Panel.Height = 80;
                FiltersPanel.Height = 80;
                propertiesPanel.Controls.Add(FiltersPanel.Panel);

                FiltersPanel.Body.Controls.Add(filterTV);
                AddSettings(SettingsPanel.Body);
                DataPanel.Body.Controls.Add(dataTV);

                Panel checkCounterPanel = new Panel();
                checkCounterPanel.AutoScroll = false;
                checkCounterPanel.Height = 20;
                checkCounterPanel.Dock = DockStyle.Bottom;
                DataPanel.Body.Controls.Add(checkCounterPanel);

                CheckCounter.Location = new Point(5, 5);
                checkCounterPanel.Controls.Add(CheckCounter);

                LoadOldFilters();
            }
            public void LoadOldFilters()
            {
                foreach (string str in
                    Properties.Settings.Default.ResultsExtractorFilters
                    [IA.FileBrowser.ActiveAccountIndex].Split(
                        new string[] { "\t" }, StringSplitOptions.None))
                    if (str != "@")
                    {
                        TreeNode n = new TreeNode(str);
                        n.Checked = false;
                        filterTV.Nodes.Add(n);
                    }
            }
            public void AddSettings(Panel p)
            {
                p.SuspendLayout();
                
                StDevCB.Text = "Standard Deviation";
                StDevCB.Width = 150;
                StDevCB.Checked = true;
                StDevCB.Location = new Point(5, 5);
                StDevCB.CheckedChanged += CheckBox_Checked;
                p.Controls.Add(StDevCB);

                NormCB.Text = "Normalise";
                NormCB.Checked = false;
                NormCB.Location = new Point(5, 25);
                NormCB.CheckedChanged += CheckBox_Checked;
                NormCB.Tag = 0;
                p.Controls.Add(NormCB);

                NormTo1.Tag = 0;
                NormTo1.Text = "Max = 1";
                NormTo1.ForeColor = Parametars.ShriftColor;
                NormTo1.Checked = true;
                NormTo1.Location = new Point(25, 45);
                p.Controls.Add(NormTo1);
                NormTo1.Enabled = false;
                NormTo1.CheckedChanged += RadioBtn_Check;

                NormFrom0To1.Tag = 1;
                NormFrom0To1.ForeColor = Parametars.ShriftColor;
                NormFrom0To1.Text = "Max = 1 && Min = 0";
                NormFrom0To1.Checked = false;
                NormFrom0To1.Width += 10;
                NormFrom0To1.Location = new Point(25, 65);
                p.Controls.Add(NormFrom0To1);
                NormFrom0To1.Enabled = false;
                NormFrom0To1.CheckedChanged += RadioBtn_Check;

                dNormCB.Text = "Double Normalise";
                dNormCB.Width = 150;
                dNormCB.Checked = false;
                dNormCB.Enabled = false;
                dNormCB.Location = new Point(25, 90);
                dNormCB.CheckedChanged += CheckBox_Checked;
                dNormCB.Tag = 0;
                p.Controls.Add(dNormCB);


                SubsetCB.Text = "Subset";
                SubsetCB.Width = 150;
                SubsetCB.Checked = false;
                SubsetCB.Location = new Point(5, 115);
                SubsetCB.CheckedChanged += CreateSubset;
                p.Controls.Add(SubsetCB);

                p.ResumeLayout();
            }
            private void CreateSubset(object sender,EventArgs e)
            {
                CheckBox cb = (CheckBox)sender;
                if (dataTV.OriginalXaxis == null || 
                    dataTV.OriginalXaxis.Length < 1
                    || !cb.Focused) return;

                double[] range = new double[2] {
                dataTV.OriginalXaxis[0],
               dataTV.OriginalXaxis[dataTV.OriginalXaxis.Length - 1] };

                if (!cb.Checked)
                {
                    cb.Tag = null;
                    cb.Text = "Subset";
                }
                else
                {
                    range[0] = dataTV.OriginalXaxis[0];
                    range[1] = dataTV.OriginalXaxis[dataTV.OriginalXaxis.Length - 1];

                    double[] newRange = RangeDialog(range);

                    if (!(newRange[0] == 0 && newRange[1] == 0))
                    {
                        range = newRange;
                        cb.Tag = range;
                        cb.Text = "Subset(" + range[0].ToString() + " - " + range[1].ToString() + ")";
                    }
                    else
                    {
                        cb.Tag = null;
                        cb.Text = "Subset";
                        cb.Checked = false;
                    }
                }
                
                cb.Width = TextRenderer.MeasureText(cb.Text, cb.Font).Width + 50;
                foreach (var parent in dataTV.Store)
                {
                    dataTV.Xaxis = FileReader.Subset(parent, dataTV.OriginalXaxis, range);
                }

                foreach (var parent in dataTV.Store)
                {
                    FileReader.Normalize(parent, (int)NormCB.Tag);
                }

                repeatsCh.DrawToScreen();
                resultsCh.AddData();
            }
            private double[] RangeDialog(double[] range)
            {
                double[] newRange = new double[2];

                Form OptionForm = new Form();
                OptionForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                OptionForm.Text = "Subset range";
                OptionForm.StartPosition = FormStartPosition.CenterScreen;
                OptionForm.WindowState = FormWindowState.Normal;
                OptionForm.MinimizeBox = false;
                OptionForm.MaximizeBox = false;
                OptionForm.ForeColor = Parametars.ShriftColor;
                OptionForm.BackColor = Parametars.BackGroundColor;

                OptionForm.Width = 220;
                OptionForm.Height = 150;


                Label l1 = new Label();
                l1.Text = "Start from:";
                l1.Width = 80;
                l1.Location = new Point(5, 15);
                OptionForm.Controls.Add(l1);

                TextBox startTB = new TextBox();
                startTB.Text = range[0].ToString();
                startTB.Location = new Point(100, 13);
                OptionForm.Controls.Add(startTB);

                Label l2 = new Label();
                l2.Text = "End at:";
                l2.Width = 80;
                l2.Location = new Point(5, 45);
                OptionForm.Controls.Add(l2);

                TextBox endTB = new TextBox();
                endTB.Text = range[1].ToString();
                endTB.Location = new Point(100, 43);
                OptionForm.Controls.Add(endTB);

                Button btn = new Button();
                btn.Text = "Calculate";
                btn.Location = new Point(70, 75);
                btn.BackColor = SystemColors.ButtonFace;
                btn.ForeColor = Color.Black;
                OptionForm.Controls.Add(btn);
                btn.Click += new EventHandler(delegate (object o, EventArgs a) 
                {
                    double val;

                    if (!double.TryParse(startTB.Text, out val) ||
                    !double.TryParse(endTB.Text, out val))
                    {
                        MessageBox.Show("Incorect value!");
                    }
                    else
                    {
                        newRange[0] = double.Parse(startTB.Text);
                        newRange[1] = double.Parse(endTB.Text);
                        range[0] = double.Parse(startTB.Text);
                        range[1] = double.Parse(endTB.Text);

                        OptionForm.Hide();
                        OptionForm.Dispose();
                    }
                });

                OptionForm.KeyPreview = true;
                OptionForm.KeyDown += new KeyEventHandler(delegate (object sender, KeyEventArgs e)
                {
                    switch (e.KeyCode)
                    {
                        case Keys.Escape:
                            OptionForm.Hide();
                            OptionForm.Dispose();
                            break;
                        case Keys.Enter:
                            btn.PerformClick();
                            break;
                        default:
                            break;
                    }
                });

                // Linux change
                IA.FileBrowser.StatusLabel.Text = "Dialog open";
                OptionForm.ShowDialog();
                IA.FileBrowser.StatusLabel.Text = "Ready";

                return newRange;
            }
            private void RadioBtn_Check(object sender, EventArgs e)
            {
                
                    RadioButton rb = (RadioButton)sender;
                    if (rb.Checked)
                    {
                        NormCB.Tag = rb.Tag;
                        foreach (var parent in dataTV.Store)
                        {
                            FileReader.Normalize(parent, (int)NormCB.Tag);
                        }
                        repeatsCh.DrawToScreen();
                        resultsCh.AddData();
                    }
                
            }
            private void CheckBox_Checked(object sender, EventArgs e)
            {
                
                    CheckBox cb = (CheckBox)sender;
                    
                    if (cb == NormCB)
                    {
                        NormTo1.Enabled = cb.Checked;
                        NormFrom0To1.Enabled = cb.Checked;
                        dNormCB.Enabled = cb.Checked;
                    }
                    repeatsCh.DrawToScreen();
                    resultsCh.AddData();
               
            }
            public void CheckCounter_Count(object sender, EventArgs e)
            {
                int NChecked = 0;
                int NAll = 0;

                foreach (TreeNode node in dataTV.Nodes)
                    foreach (TreeNode n in node.Nodes)
                    {
                        NAll++;
                        if (n.Checked)
                            NChecked++;
                    }

                CheckCounter.Text = "Checked: " + NChecked.ToString() +
                    " (Total: " + NAll.ToString() + ")";
                CheckCounter.Width = TextRenderer.MeasureText(CheckCounter.Text, CheckCounter.Font).Width;
            }
            private void Resize_MouseEnter(object sender, EventArgs e)
            {
                ((Panel)sender).Cursor = Cursors.SizeWE;
            }
            private void Resize_MouseLeave(object sender, EventArgs e)
            {
                ((Panel)sender).Cursor = Cursors.Default;
            }

            private Panel rP = new Panel();
            private void Resize_MouseDown(object sender, MouseEventArgs e)
            {
                
                if (e.Button != MouseButtons.Left) return;
                resizing = true;
                Panel source = (Panel)sender;
                rP = new Panel();
                rP.Width = source.Width;
                rP.Height = source.Height;
                rP.BackColor = Color.FromArgb(150, 10, 10, 10);
                rP.Location = source.Location;
                //source.Tag = rP;
                this.Controls.Add(rP);
                rP.BringToFront();                
            }
            private void Resize_MouseMove(object sender, MouseEventArgs e)
            {
                if (!resizing) return;

                Point p = ((Panel)sender).Location;
                p.X = e.X + p.X;
                rP.Location = p;

            }
            private void Resize_MouseUp(object sender, MouseEventArgs e)
            {
                if (!resizing) return;
                resizing = false;
                int X = 0;
                Panel source = (Panel)sender;
               
                if (source.Dock == DockStyle.Left)
                {
                    //((Panel)source.Tag).Width = e.X + source.Location.X;
                    X = e.X + source.Location.X;

                    if (this.Width - (this.FitPropertiesPanel.Width + X) < 60)
                        X = this.Width - (this.FitPropertiesPanel.Width + 60);

                    if (X > 0)
                    {
                        this.ExtrPropertiesPanel.Width = X;
                        SaveSize();
                    }
                }
                else if (source.Dock == DockStyle.Right)
                {
                    // ((Panel)source.Tag).Width -= e.X;
                    X = this.FitPropertiesPanel.Width - e.X;

                    if (this.Width - (this.ExtrPropertiesPanel.Width + X) < 60)
                        X = this.Width - (this.ExtrPropertiesPanel.Width + 60);

                    if (X > 0)
                    {
                        this.FitPropertiesPanel.Width = X;
                        SaveSize();
                    }
                }
                
                rP.Dispose();
            }
            private void AddMenuStrip()
            {
                Panel MenuPanel = new Panel();
                MenuPanel.Height = 30;
                MenuPanel.Dock = DockStyle.Top;
                //MenuPanel.BackColor = Parametars.TaskBtnColor;
                this.Controls.Add(MenuPanel);

                MenuStrip Menu = new MenuStrip();
                Menu.BackColor = Parametars.BackGroundColor;
                Menu.ForeColor = Parametars.ShriftColor;
                MenuPanel.Controls.Add(Menu);

                //add Work dir
                ToolStripMenuItem AddDirBtn = new ToolStripMenuItem();
                AddDirBtn.Text = "Add work directory";
                AddDirBtn.BackColor = Parametars.BackGroundColor;
                AddDirBtn.ForeColor = Parametars.ShriftColor;
                Menu.Items.Add(AddDirBtn);
                AddDirBtn.Click += new EventHandler(delegate (object o, EventArgs a)
                {
                    this.dataTV.AddWorkDirectory("");
                });
                //Open Btn
                ToolStripMenuItem OpenBtn = new ToolStripMenuItem();
                OpenBtn.Text = "Open";
                OpenBtn.BackColor = Parametars.BackGroundColor;
                OpenBtn.ForeColor = Parametars.ShriftColor;
                Menu.Items.Add(OpenBtn);
                OpenBtn.Click += new EventHandler(delegate (object o, EventArgs a)
                {
                    FileSaver.Open(this);
                });

                //Save Btn
                ToolStripMenuItem SaveBtn = new ToolStripMenuItem();
                SaveBtn.Text = "Save";
                SaveBtn.BackColor = Parametars.BackGroundColor;
                SaveBtn.ForeColor = Parametars.ShriftColor;
                Menu.Items.Add(SaveBtn);
                SaveBtn.Click += new EventHandler(delegate (object o, EventArgs a)
                {
                    FileSaver.Save(this);
                });

                //Export Btn
                ToolStripMenuItem ExportBtn = new ToolStripMenuItem();
                ExportBtn.Text = "Export";
                ExportBtn.BackColor = Parametars.BackGroundColor;
                ExportBtn.ForeColor = Parametars.ShriftColor;
                Menu.Items.Add(ExportBtn);
                ExportBtn.Click += new EventHandler(delegate (object o, EventArgs a)
                {
                    FileSaver.Export(this);
                });
            }
            private void AddFooter()
            {
                //add Status Controlers
                Panel StatusPanel = new Panel();
                StatusPanel.Height = 30;
                StatusPanel.Dock = DockStyle.Bottom;
                StatusPanel.BackColor = Parametars.BackGroundColor;
                //this.Controls.Add(StatusPanel);
                StatusPanel.BringToFront();

                StatusStrip MainStatusBar = new StatusStrip();
                MainStatusBar.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
                MainStatusBar.BackColor = Parametars.TitlePanelColor;
                MainStatusBar.ForeColor = Parametars.ShriftColor;
                MainStatusBar.Dock = DockStyle.Bottom;
                StatusPanel.Controls.Add(MainStatusBar);
                StatusPanel.Height = MainStatusBar.Height;
                {

                    StatusLabel.Text = "Ready";
                    StatusLabel.TextChanged += StatusLabel_TextChange;
                    MainStatusBar.Items.Add(StatusLabel);

                    StatusProgressBar.Visible = false;
                    MainStatusBar.Items.Add(StatusProgressBar);

                    ToolStripStatusLabel AuthorLabel = new ToolStripStatusLabel();

                    AuthorLabel.Alignment = ToolStripItemAlignment.Right;
                    AuthorLabel.Text = "Copyright © <2018> Georgi Danovski";
                    MainStatusBar.Items.Add(AuthorLabel);
                }
            }
            //Status Bar

            private void StatusLabel_TextChange(object sender, EventArgs e)
            {
                IA.FileBrowser.StatusLabel.Text = StatusLabel.Text;
                /*
                if (StatusLabel.Text == "Ready")
                {
                    this.Cursor = Cursors.Default;
                    StatusProgressBar.Visible = false;
                }
                else
                {
                    this.Cursor = Cursors.WaitCursor;
                    if (StatusProgressBar.Style != ProgressBarStyle.Marquee)
                    {
                        StatusProgressBar.Minimum = 1;
                        StatusProgressBar.Maximum = 100;
                        StatusProgressBar.Step = 10;
                        StatusProgressBar.Value = 10;
                        StatusProgressBar.MarqueeAnimationSpeed = 30;
                        StatusProgressBar.Style = ProgressBarStyle.Marquee;
                    }
                    StatusProgressBar.Visible = true;
                }*/
            }
        }

       public class DataNode : TreeNode
        {
            public string Comment = "";
            public string RoiName = "";
            public double[] Series;
            public double[] OriginalSeries;
            public double[] NormSeries;
        }
        public class DataTV : TreeView
        {
            public MyForm form1;
            public double[] Xaxis;
            public double[] OriginalXaxis;
            public string XaxisTitle;
            public string YaxisTitle;
            public List<TreeNode> Store = new List<TreeNode>();
            public List<Color> colors;
            public ContextMenu ContextMenu = new ContextMenu();
            private MenuItem NewBtn = new MenuItem();
            private MenuItem RefreshBtn = new MenuItem();
            private MenuItem acqBlCorrBtn = new MenuItem();
            private MenuItem fociFrapCorrBtn = new MenuItem();
            private MenuItem DeleteBtn = new MenuItem();
            public string lastDir = "";
            private ToolTip TurnOnToolTip = new ToolTip();

            private ToolStripStatusLabel StatusLabel;
            public DataTV(string dir, ToolStripStatusLabel StatusLabel, MyForm form1)
            {
                this.form1 = form1;
                this.StatusLabel = StatusLabel;

                this.Dock = DockStyle.Fill;
                this.ShowNodeToolTips = false;
                this.BorderStyle = BorderStyle.None;
                this.BackColor = Parametars.BackGround2Color;
                this.ForeColor = Parametars.ShriftColor;
                this.CheckBoxes = true;
                this.ShowRootLines = false;
                this.ShowPlusMinus = false;

                //Build the control
                createImagesForTV();
                lastDir = dir;
                //AddWorkDirectory(dir);
                BuildContextMenu();

                this.KeyDown += dataTV_KeyDown;
                this.NodeMouseClick += ContextMenu_NodeShow;
                this.MouseUp += ContextMenu_Show;
                this.AfterCheck += Node_AfterCheck;
                this.NodeMouseHover += Node_MouseHover;
                this.MouseLeave += dataTV_MouseLeave;
                this.MouseMove += dataTV_MouseLeaveNode;
                this.AfterSelect += dataTV_selectedNodeChange;

            }
            private void Node_MouseHover(object sender, TreeNodeMouseHoverEventArgs e)
            {
                //this.SelectedNode = e.Node;
                string str = "";
                TreeNode source = (TreeNode)e.Node.Tag;
                
                if (e.Node.ImageIndex != 0)
                {
                    TurnOnToolTip.ToolTipTitle = "ROI name: " + source.Text.Split('\t')[1];
                    str +="File name: " + source.Text.Split('\t')[0];
                    if (((DataNode)source).Comment != "")
                        str += "\nComment: " + ((DataNode)source).Comment;
                    else
                        str = "";
                        //str += "\nNo comment";
                }
                else
                {
                    TurnOnToolTip.ToolTipTitle = "Folder name: " + source.Text;
                    str = "Path: " + (string)source.Tag;
                }

                TurnOnToolTip.AutoPopDelay = 5000;
                TurnOnToolTip.InitialDelay = 1000;
                TurnOnToolTip.ReshowDelay = 500;
                
                Point p = new Point(e.Node.Bounds.X + e.Node.Bounds.Width, e.Node.Bounds.Y);
                if (p.X > this.Width) p.X = this.Width;
                TurnOnToolTip.Show(str, this, p);
            }
            
            private void dataTV_selectedNodeChange(object sender, EventArgs e)
            {
                form1.repeatsCh.DrawToScreen();
            }
            private void dataTV_MouseLeave(object sender, EventArgs e)
            {
                TurnOnToolTip.Hide(this);
            }
            private void dataTV_MouseLeaveNode(object sender, MouseEventArgs e)
            {
                if (this.GetNodeAt(e.Location) == null)
                {
                    TurnOnToolTip.Hide(this);
                }
            }
            private void createImagesForTV()
            {
                colors = new List<Color>() { Color.Empty };
                //Add embedet resource image to the tree view
                ImageList il = new ImageList();
                il.ImageSize = new Size(13, 15);

               il.Images.Add(new Bitmap(Properties.Resources.FolderIcon));

                /*
                _imageStream = _assembly.GetManifestResourceStream("Results_Extractor.tifIcon.png");
                il.Images.Add(new Bitmap(_imageStream));
                */

                //add colors
                string[] colMatrix = new string[] {"blue","red","#00b300", "#b300b3", "#00bfff", "#ffcc00", "#ff471a", "#cc6699", "#39e600"
                , "#00b3b3", "#ffcc66", "#7575a3", "#ff1a1a", "#ff0055", "#8a00e6", "#bf8040",
                "#53c68c", "#ace600", "#b33c00", "#ff6666"};

                foreach (string str in colMatrix)
                {
                    Color col = ColorTranslator.FromHtml(str);
                    colors.Add(col);
                    Bitmap bmp = new Bitmap(13, 15);

                    using (Graphics gr = Graphics.FromImage(bmp))
                    {
                        SolidBrush blueBrush = new SolidBrush(Color.FromArgb(0, 0, 0, 0));
                        gr.FillRectangle(blueBrush, 0, 0, bmp.Width, bmp.Height);
                        blueBrush = new SolidBrush(col);
                        gr.FillRectangle(blueBrush, 2, 1, bmp.Width - 2, bmp.Height - 2);
                    }
                    il.Images.Add(bmp);
                }

                this.ImageList = il;
            }
            public void BindColorsToNodes()
            {
                int ind = 1;
                int up = colors.Count;

                foreach (TreeNode parent in this.Nodes)
                    foreach (TreeNode node in parent.Nodes)
                    {
                        node.ImageIndex = ind;
                        node.SelectedImageIndex = ind;
                        ind++;
                        if (ind == up) ind = 1;
                    }

                this.Refresh();
            }
            private void ContextMenu_NodeShow(object sender, TreeNodeMouseClickEventArgs e)
            {
                if (e.Button != MouseButtons.Right) return;
                this.SelectedNode = e.Node;
                DeleteBtn.Enabled = true;
                ContextMenu.Show(this, new System.Drawing.Point(e.X, e.Y));
            }
            private void ContextMenu_Show(object sender, MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Left)
                {
                    TreeNode n = this.GetNodeAt(new System.Drawing.Point(e.X, e.Y));
                    if (n == null)
                    {
                        this.SelectedNode = null;
                        form1.resultsCh.AddData();
                        form1.repeatsCh.DrawToScreen();
                    }
                }

                if (e.Button != MouseButtons.Right) return;
                DeleteBtn.Enabled = false;
                ContextMenu.Show(this, new System.Drawing.Point(e.X, e.Y));
            }
            private void dataTV_KeyDown(object sender, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.F5)
                {
                    foreach (TreeNode node in Store)
                    {
                        TreeNode n = new TreeNode();
                        n.Tag = (string)node.Tag;
                        TreeNode_Refresh(n);
                    }

                    return;
                }

                if (e.Modifiers != Keys.Control) return;
                switch (e.KeyCode)
                {
                    case (Keys.N):
                        NewNode(sender, e);
                        e.Handled = true;
                        e.SuppressKeyPress = true;
                        break;
                    case (Keys.D):
                        DeleteNode(sender, e);
                        e.Handled = true;
                        e.SuppressKeyPress = true;
                        break;
                }
            }
            private void BuildContextMenu()
            {
                NewBtn.Text = "Add work directory";
                NewBtn.Click += NewNode;
                NewBtn.Click += new EventHandler(delegate (object sender, EventArgs e)
                {
                    SendKeys.Send("{ESC}");
                });
                ContextMenu.MenuItems.Add(NewBtn);
                
                acqBlCorrBtn.Text = "Acquisition bleaching correction";
                acqBlCorrBtn.Click += new EventHandler(delegate (object sender, EventArgs e)
                {
                    FRAPA_Model.FrappaNormalise(this.form1);
                    SendKeys.Send("{ESC}");
                });
                ContextMenu.MenuItems.Add(acqBlCorrBtn);

                //fociFrapCorrBtn
                fociFrapCorrBtn.Text = "Foci FRAPA normalization";
                fociFrapCorrBtn.Click += new EventHandler(delegate (object sender, EventArgs e)
                {
                    FRAPA_Model.FociFrappaNormalise(this.form1);
                    SendKeys.Send("{ESC}");
                });
                ContextMenu.MenuItems.Add(fociFrapCorrBtn);

                RefreshBtn.Click += new EventHandler(delegate (object sender, EventArgs e)
                {
                    foreach (TreeNode node in Store)
                    {
                        TreeNode n = new TreeNode();
                        n.Tag = (string)node.Tag;
                        TreeNode_Refresh(n);
                    }

                    SendKeys.Send("{ESC}");
                });
                ContextMenu.MenuItems.Add(RefreshBtn);

                DeleteBtn.Text = "Release";
                DeleteBtn.Click += DeleteNode;
                DeleteBtn.Click += new EventHandler(delegate (object sender, EventArgs e)
                {
                    SendKeys.Send("{ESC}");
                });
                ContextMenu.MenuItems.Add(DeleteBtn);

                ContextMenu.MenuItems.Add("-");

                MenuItem selectAllMI = new MenuItem();
                selectAllMI.Text = "Check All";
                //selectAllMI.Click += DeleteNode;
                selectAllMI.Click += new EventHandler(delegate (object sender, EventArgs e)
                {
                    SendKeys.Send("{ESC}");
                    foreach (TreeNode parent in this.Nodes)
                    {
                        parent.Checked = true;
                        ((TreeNode)parent.Tag).Checked = true;
                        foreach (TreeNode node in parent.Nodes)
                        {
                            node.Checked = true;
                            ((TreeNode)node.Tag).Checked = true;
                        }
                    }

                    form1.CheckCounter_Count(this, new EventArgs());
                    form1.repeatsCh.DrawToScreen();
                    form1.resultsCh.AddData();
                });
                ContextMenu.MenuItems.Add(selectAllMI);

                MenuItem unSelectAllMI = new MenuItem();
                unSelectAllMI.Text = "Uncheck All";
                //unSelectAllMI.Click += DeleteNode;
                unSelectAllMI.Click += new EventHandler(delegate (object sender, EventArgs e)
                {
                    SendKeys.Send("{ESC}");
                    foreach (TreeNode parent in this.Nodes)
                    {
                        parent.Checked = false;
                        ((TreeNode)parent.Tag).Checked = false;
                        foreach (TreeNode node in parent.Nodes)
                        {
                            node.Checked = false;
                            ((TreeNode)node.Tag).Checked = false;
                        }
                    }
                    form1.CheckCounter_Count(this, new EventArgs());
                    form1.repeatsCh.DrawToScreen();
                    form1.resultsCh.AddData();
                });
                ContextMenu.MenuItems.Add(unSelectAllMI);

                RefreshBtn.Text = "Refresh";
                
            }
            private void NewNode(object sender, EventArgs e)
            {
                AddWorkDirectory(lastDir);
            }
            private void DeleteNode(object sender, EventArgs e)
            {
                if (this.SelectedNode == null) return;
                if (this.SelectedNode.SelectedImageIndex != 0) return;

                TreeNode source = (TreeNode)this.SelectedNode.Tag;

                this.Store.Remove(source);
                this.Nodes.Remove(this.SelectedNode);

                form1.CheckCounter_Count(sender, e);
                form1.repeatsCh.DrawToScreen();
            }
            public void AddWorkDirectory(string dir)
            {
                FolderBrowserDialog fbd = new FolderBrowserDialog();
                fbd.Description = "Add work directory:";
                
                if (dir != "" && dir.IndexOf("\\") > -1 &&
                    !Directory.Exists(OSStringConverter.StringToDir(dir)))
                    dir = dir.Substring(0, dir.LastIndexOf("\\"));
                else if (lastDir != "" && lastDir.IndexOf("\\") > -1 &&
                    !Directory.Exists(OSStringConverter.StringToDir(lastDir)))
                    dir = lastDir.Substring(0, lastDir.LastIndexOf("\\"));
                
                if (Directory.Exists(OSStringConverter.StringToDir(dir)))
                {
                    fbd.SelectedPath = OSStringConverter.StringToDir(dir);
                    lastDir = dir;
                }

                DialogResult result = fbd.ShowDialog();
                // OK button was pressed.
                if (result == DialogResult.OK)
                {
                    dir = OSStringConverter.GetWinString(fbd.SelectedPath);
                    TreeNode n = new TreeNode();
                    n.Tag = dir;
                    n.Checked = true;

                    TreeNode_Refresh(n);

                }
            }
            private void TreeNode_Refresh(TreeNode n, bool bindColors = true)
            {
                TreeNode lastVisible = LastVisible();

                string dir = (string)n.Tag;

                n.Text = GetName(dir);
                n.Nodes.Clear();
                n.ImageIndex = 0;
                n.SelectedImageIndex = 0;
                //background worker
                var bgw = new BackgroundWorker();
                bgw.WorkerReportsProgress = true;
                //Add event for projection here
                bgw.DoWork += new DoWorkEventHandler(delegate (Object o, DoWorkEventArgs a)
                {
                    List<string> l = new List<string>() { dir };
                    List<string> oldL;

                    do
                    {
                        oldL = new List<string>();

                        foreach (string str in l)
                            oldL.AddRange(TreeNode_fill(n, str));

                        l = oldL;
                    }
                    while (oldL.Count > 0);

                //Read Data
                FileReader.ReadTreeNode(n, form1);

                    if (form1.SubsetCB.Checked == true)
                    {
                        CheckBox cb = form1.SubsetCB;
                        double[] range = (double[])form1.SubsetCB.Tag;
                        cb.Text = "Subset(" + range[0].ToString() + " - " + range[1].ToString() + ")";
                        cb.Width = TextRenderer.MeasureText(cb.Text, cb.Font).Width + 50;
                        form1.dataTV.Xaxis = FileReader.Subset(n, form1.dataTV.OriginalXaxis, range);
                    }
                    //Normalize the data
                    if (form1.NormCB == null || form1.NormCB.Tag == null)
                        FileReader.Normalize(n, 0);
                    else
                        FileReader.Normalize(n,(int)form1.NormCB.Tag);

                    ((BackgroundWorker)o).ReportProgress(0);
                });

                bgw.ProgressChanged += new ProgressChangedEventHandler(
                    delegate (Object o, ProgressChangedEventArgs a)
                    {
                        if (a.ProgressPercentage == 0)
                        {
                            this.SuspendLayout();

                            n.ExpandAll();

                            NodeToStore(n);

                            StoreToNodes(n);

                            if (bindColors == true)
                                BindColorsToNodes();

                            form1.CheckCounter_Count(o, a);

                            form1.repeatsCh.DrawToScreen();

                            this.ResumeLayout();
                        }

                        RefreshLastVisible(lastVisible);
                        lastDir = dir;
                        StatusLabel.Text = "Ready";
                    });
                //Start background worker
                StatusLabel.Text = "Adding work directory...";
                //start bgw
                
                bgw.RunWorkerAsync();
            }
            public void NodeToStore(TreeNode node)
            {
                //find the old node
                TreeNode target = null;
                foreach (TreeNode tn in this.Store)
                    if ((string)tn.Tag == (string)node.Tag)
                    {
                        target = tn;
                        node.Checked = tn.Checked;
                        break;
                    }
                //if there is no old node - directly add the new one
                if (target == null)
                {
                    this.Store.Add(node);
                    return;
                }
                //Restore existing settings
                foreach (TreeNode tn in node.Nodes)
                    foreach (TreeNode targetTn in target.Nodes)
                        if ((string)tn.Tag == (string)targetTn.Tag && tn.Text == targetTn.Text)
                        {
                            tn.Checked = targetTn.Checked;
                            target.Nodes.Remove(targetTn);
                            break;
                        }
                //apply to the store            
                foreach (TreeNode tn in this.Store)
                    if ((string)tn.Tag == (string)node.Tag)
                    {
                        this.Store[this.Store.IndexOf(tn)] = node;
                        break;
                    }

            }
            public void RefreshAllNodes()
            {
                TreeNode lastVisible = LastVisible();

                foreach (TreeNode node in this.Store)
                    StoreToNodes(node);
                
                BindColorsToNodes();

                form1.repeatsCh.DrawToScreen();
                form1.resultsCh.AddData();
                form1.CheckCounter_Count(this, new EventArgs());
                RefreshLastVisible(lastVisible);
            }
           
            private TreeNode LastVisible()
            {
                this.SuspendLayout();
                TreeNode node = null;
                foreach(TreeNode parent in this.Nodes)
                {
                    if (parent.IsVisible)
                        node = (TreeNode)parent.Tag;

                    foreach (TreeNode n in parent.Nodes)
                        if (n.IsVisible)
                            node = (TreeNode)n.Tag;
                }

                return node;
            }
            private void RefreshLastVisible(TreeNode node)
            {
                foreach (TreeNode parent in this.Nodes)
                {
                    if (node == (TreeNode)parent.Tag)
                    {
                        parent.EnsureVisible();
                        return;
                    }

                    foreach (TreeNode n in parent.Nodes)
                        if (node == (TreeNode)n.Tag)
                        {
                            n.EnsureVisible();
                            return;
                        }
                }

                this.ResumeLayout();
            }
            public void StoreToNodes(TreeNode node)
            {
                
                TreeNode target = new TreeNode();

                foreach (TreeNode tn in this.Nodes)
                    if ((string)((TreeNode)tn.Tag).Tag == (string)node.Tag)
                    {
                        target = tn;
                        target.Nodes.Clear();
                        break;
                    }

                target.Text = node.Text;
                target.Checked = node.Checked;
                target.Tag = node;
                target.ImageIndex = 0;
                target.SelectedImageIndex = 0;
                
                foreach (TreeNode tn in node.Nodes)
                    if (IncludeInTreeView(tn.Text +"."+ ((DataNode)tn).Comment))
                    {
                        TreeNode newN = new TreeNode();
                        newN.Text = tn.Text;
                        newN.Checked = tn.Checked;
                        newN.Tag = tn;
                        target.Nodes.Add(newN);
                    }

                if (!(this.Nodes.IndexOf(target) > -1))
                {
                    this.Nodes.Add(target);
                    target.ExpandAll();
                }
            }
            private bool IncludeInTreeView(string str)
            {
                foreach (TreeNode tn in form1.filterTV.Nodes)
                    if (tn.Checked)
                    {
                        if (tn.Text.StartsWith("!") &&
                            (str.IndexOf(tn.Text.Substring(1)) > -1))
                        {
                            return false;
                        }
                        else if (!tn.Text.StartsWith("!") &&
                            !(str.IndexOf(tn.Text) > -1))
                        {
                            return false;
                        }
                    }

                return true;
            }
            private List<string> TreeNode_fill(TreeNode n, string Dir)
            {
                List<string> l = new List<string>();

                if (!Directory.Exists(OSStringConverter.StringToDir(Dir))) { return l; }

                DirectoryInfo directoryInfo = new DirectoryInfo(OSStringConverter.StringToDir(Dir));
                
                foreach (var directory in directoryInfo.GetDirectories())
                {
                    l.Add(OSStringConverter.GetWinString(directory.FullName));
                }

                foreach (var file in directoryInfo.GetFiles())
                    if (file.FullName.EndsWith(".txt"))
                        using (StreamReader sr = new StreamReader(file.FullName))
                        {
                            string str = sr.ReadLine();
                            if (str != null && str.StartsWith("CTResults:"))
                            {
                                str = sr.ReadLine();
                                string[] comments = str.Split(new string[] { "\t" }, StringSplitOptions.None);
                                str = sr.ReadLine();
                                string[] titles = str.Split(new string[] { "\t" }, StringSplitOptions.None);
                                for (int i = 1; i < titles.Length; i++)
                                {
                                    DataNode n1 = new DataNode();
                                    n1.Tag = OSStringConverter.GetWinString(file.FullName);
                                    n1.Text = file.Name + "\t" + titles[i];
                                    n1.Comment = comments[i];
                                    n1.RoiName = titles[i];
                                    n1.ImageIndex = 1;
                                    n1.SelectedImageIndex = 1;
                                    n1.Checked = true;
                                    n.Nodes.Add(n1);
                                }
                            }
                        }
                return l;
            }
            private string GetName(string Dir)
            {
                if (Dir.IndexOf("\\") > -1)
                    return Dir.Substring(Dir.LastIndexOf("\\") + 1,
                        Dir.Length - Dir.LastIndexOf("\\") - 1);
                else
                    return Dir;
            }

            #region NodeCheck
           private bool suppressCheck = false;
            private void Node_AfterCheck(object sender, TreeViewEventArgs e)
            {
                if (e.Action != TreeViewAction.Unknown && !suppressCheck)
                {
                    suppressCheck = true;

                    TreeNode source = (TreeNode)e.Node.Tag;
                    source.Checked = e.Node.Checked;

                    if (e.Node.Nodes.Count > 0)
                        foreach (TreeNode node in e.Node.Nodes)
                        {
                            node.Checked = e.Node.Checked;

                            source = (TreeNode)node.Tag;
                            source.Checked = e.Node.Checked;
                        }
                    else if (e.Node.ImageIndex != 0 & e.Node.Checked == false)
                    {
                        e.Node.Parent.Checked = e.Node.Checked;

                        source = (TreeNode)e.Node.Parent.Tag;
                        source.Checked = e.Node.Checked;
                    }
                    form1.CheckCounter_Count(this, new EventArgs());
                    form1.repeatsCh.DrawToScreen();

                    suppressCheck = false;
                }
            }
            #endregion NodeCheck
        }
        public class FilterTV : TreeView
        {
            public MyForm form1;
            public ContextMenu ContextMenu = new ContextMenu();
            private MenuItem NewBtn = new MenuItem();
            private MenuItem EditBtn = new MenuItem();
            private MenuItem DeleteBtn = new MenuItem();

            public FilterTV(MyForm form1)
            {
                this.form1 = form1;
                this.Dock = DockStyle.Fill;
                this.ShowNodeToolTips = false;
                this.BorderStyle = BorderStyle.None;
                this.BackColor = Parametars.BackGround2Color;
                this.ForeColor = Parametars.ShriftColor;
                this.CheckBoxes = true;
                this.ShowRootLines = false;
                this.ShowPlusMinus = false;

                BuildContextMenu();

                this.NodeMouseClick += ContextMenu_NodeShow;
                this.MouseUp += ContextMenu_Show;
                this.KeyDown += filterTV_KeyDown;
                this.AfterCheck += Node_Checked;
            }
            private void RefreshToHardDrive()
            {
                try
                {
                    List<string> str = new List<string>();
                    str.Add("@");

                    foreach (TreeNode node in this.Nodes)
                        str.Add(node.Text);

                    Properties.Settings.Default.ResultsExtractorFilters[
                        form1.IA.TabPages.ActiveAccountIndex] =
                        string.Join("\t", str);

                    Properties.Settings.Default.Save();
                }
                catch { }
            }
            private void filterTV_KeyDown(object sender, KeyEventArgs e)
            {
                if (e.Modifiers != Keys.Control) return;
                switch (e.KeyCode)
                {
                    case (Keys.N):
                        NewBtn_Click(sender, e);
                        e.Handled = true;
                        e.SuppressKeyPress = true;
                        break;
                    case (Keys.E):
                        EditBtn_Click(sender, e);
                        e.Handled = true;
                        e.SuppressKeyPress = true;
                        break;
                    case (Keys.D):
                        DeleteNode(sender, e);
                        e.Handled = true;
                        e.SuppressKeyPress = true;
                        break;
                }
            }
            private void DeleteNode(object sender, EventArgs e)
            {
                if (this.SelectedNode == null) return;
                this.Nodes.Remove(this.SelectedNode);
                form1.dataTV.RefreshAllNodes();
                RefreshToHardDrive();
            }

            private void EditBtn_Click(object sender, EventArgs e)
            {
                if (this.SelectedNode == null) return;

                TextBox tb = new TextBox();
                tb.Tag = this.SelectedNode;
                tb.Text = this.SelectedNode.Text;
                tb.Location = new System.Drawing.Point(this.SelectedNode.Bounds.X, this.SelectedNode.Bounds.Y - 2);
                tb.Width = this.SelectedNode.Bounds.Width;
                tb.Height = this.SelectedNode.Bounds.Height;
                tb.Visible = true;
                this.Controls.Add(tb);
                tb.Focus();
                if (!String.IsNullOrEmpty(tb.Text))
                {
                    tb.SelectionStart = 0;
                    tb.SelectionLength = tb.Text.Length;
                }
                tb.TextChanged += new EventHandler(delegate (Object o, EventArgs a)
                {
                    int w = TextRenderer.MeasureText(tb.Text, tb.Font).Width + 5;
                    if (tb.Width < w)
                    {
                        tb.Width = w;
                    }
                });

                tb.LostFocus += new EventHandler(delegate (Object o, EventArgs a)
                {
                    tb.Dispose();
                });
                tb.KeyDown += new KeyEventHandler(delegate (Object o, KeyEventArgs a)
                {
                    if (a.KeyCode == Keys.Enter && tb.Text != "")
                    {
                        a.Handled = true;
                        a.SuppressKeyPress = true;
                        this.SelectedNode.Text = tb.Text;
                        tb.Dispose();
                        form1.dataTV.RefreshAllNodes();
                        RefreshToHardDrive();
                    }

                });
                this.AfterSelect += new TreeViewEventHandler(delegate (Object o, TreeViewEventArgs a)
                {
                    tb.Dispose();
                });
                this.MouseWheel += new MouseEventHandler(delegate (Object o, MouseEventArgs a)
                {
                    tb.Dispose();
                });
            }
            private void NewBtn_Click(object sender, EventArgs e)
            {
                TreeNode n = new TreeNode("new");
                n.Checked = true;
                this.Nodes.Add(n);
                this.SelectedNode = n;
                EditBtn_Click(sender, e);
            }
            private void ContextMenu_NodeShow(object sender, TreeNodeMouseClickEventArgs e)
            {
                if (e.Button != MouseButtons.Right) return;
                this.SelectedNode = e.Node;
                EditBtn.Enabled = true;
                DeleteBtn.Enabled = true;
                ContextMenu.Show(this, new System.Drawing.Point(e.X, e.Y));
               
            }
            private void ContextMenu_Show(object sender, MouseEventArgs e)
            {
                if (e.Button != MouseButtons.Right) return;
                EditBtn.Enabled = false;
                DeleteBtn.Enabled = false;
                ContextMenu.Show(this, new System.Drawing.Point(e.X, e.Y));
                
            }
            private void BuildContextMenu()
            {
                NewBtn.Text = "New";
                NewBtn.Click += NewBtn_Click;
                NewBtn.Click += new EventHandler(delegate (object sender, EventArgs e)
                {
                    SendKeys.Send("{ESC}");
                });
                ContextMenu.MenuItems.Add(NewBtn);

                EditBtn.Text = "Edit";
                EditBtn.Click += EditBtn_Click;
                EditBtn.Click += new EventHandler(delegate (object sender, EventArgs e)
                {
                    SendKeys.Send("{ESC}");
                });
                ContextMenu.MenuItems.Add(EditBtn);


                DeleteBtn.Text = "Delete";
                DeleteBtn.Click += DeleteNode;
                DeleteBtn.Click += new EventHandler(delegate (object sender, EventArgs e)
                {
                    SendKeys.Send("{ESC}");
                });
                ContextMenu.MenuItems.Add(DeleteBtn);
            }
            private void Node_Checked(object sender, EventArgs e)
            {
                form1.dataTV.RefreshAllNodes();
            }
        }

        public class FileReader
        {
            public static void ReadTreeNode(TreeNode node, MyForm form1)
            {
                if (node.Nodes.Count == 0) return;

                List<string> dirs = new List<string>();

                foreach (TreeNode n in node.Nodes)
                    if (!(dirs.IndexOf((string)n.Tag) > -1))
                        dirs.Add((string)n.Tag);

                List<DataNode>[] data = new List<DataNode>[dirs.Count];

                for (int i = 0; i < data.Length; i++)
                    data[i] = new List<DataNode>();

                foreach (TreeNode n in node.Nodes)
                    data[dirs.IndexOf((string)n.Tag)].Add((DataNode)n);

                int[] counters = new int[data.Length];

                Parallel.For(0, data.Length, index =>
                {
                    Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-GB");

                    List<DataNode> curData = data[index];
                    string dir = dirs[index];
                    string str = "";

                    if (File.Exists(OSStringConverter.StringToDir(dir)))
                    {
                        List<string> vals = new List<string>();
                        using (StreamReader sr = new StreamReader(OSStringConverter.StringToDir(dir)))
                        {
                            str = sr.ReadLine();
                            while (str != null)
                            {
                                if (str != "") vals.Add(str);

                                str = sr.ReadLine();
                            }
                        }
                        //string[] allRows = vals.ToArray();
                        
                        /*
                        using (StreamReader sr = new StreamReader(dir))
                        {
                            str = sr.ReadToEnd();
                        }

                        string[] allRows = str.Split(new string[] { "\n" }, StringSplitOptions.None);
                        */
                        str = "";

                        counters[index] = vals.Count;

                        List<string> titles = vals[2].Split(new string[] { "\t" }, StringSplitOptions.None).ToList();

                        DataNode[] nodes = new DataNode[titles.Count];

                        foreach (DataNode n in curData)
                        {
                            n.Series = new double[vals.Count - 3];
                            n.OriginalSeries = new double[vals.Count - 3];
                            nodes[titles.IndexOf(n.RoiName)] = n;
                        }

                        string[] curRow;
                        for (int row = 3, i = 0; row < vals.Count; row++, i++)
                        {
                            curRow = vals[row].Split(new string[] { "\t" }, StringSplitOptions.None);
                            for (int nCount = 1; nCount < nodes.Length; nCount++)
                                if (curRow.Length > nCount)
                                {
                                    nodes[nCount].Series[i] = double.Parse(curRow[nCount]);
                                    nodes[nCount].OriginalSeries[i] = double.Parse(curRow[nCount]);
                                }
                                else
                                {
                                    nodes[nCount].OriginalSeries[i] = 0;
                                    nodes[nCount].Series[i] = 0;
                                }
                        }
                    }
                });

                int a = counters.Max();
                for (int h = 0; h < counters.Length; h++)
                    if (a == counters[h])
                    {
                        a = h;
                        break;
                    }

                if (form1.dataTV.OriginalXaxis == null ||
                    (form1.dataTV.Store.Count == 1 &&
                    (string)form1.dataTV.Store[0].Tag == (string)node.Tag) ||
                    form1.dataTV.OriginalXaxis.Length <= data[a][0].Series.Length)
                {
                    form1.dataTV.Xaxis = new double[data[a][0].Series.Length];
                    form1.dataTV.OriginalXaxis = new double[data[a][0].Series.Length];

                    if (File.Exists(OSStringConverter.StringToDir((string)data[a][0].Tag)))
                    {
                        string str = "";
                        /*
                        using (StreamReader sr = new StreamReader((string)data[a][0].Tag))
                        {
                            str = sr.ReadToEnd();
                        }


                        string[] allRows = str.Split(new string[] { "\n" }, StringSplitOptions.None);
                        List<string> allRowsList = allRows.ToList();
                        for (int i = allRows.Length-1; i >=0; i--)
                        {
                            if (allRowsList[i] == "") allRowsList.RemoveAt(i);
                        }
                        */
                        List<string> allRowsList = new List<string>();
                        using (StreamReader sr = new StreamReader(OSStringConverter.StringToDir((string)data[a][0].Tag)))
                        {
                            str = sr.ReadLine();
                            while (str != null)
                            {
                                if (str != "") allRowsList.Add(str);
                                str = sr.ReadLine();
                            }
                        }

                        str = "";

                        form1.dataTV.XaxisTitle = (allRowsList[2].Split(new string[] { "\t" }, StringSplitOptions.None))[0];
                        form1.dataTV.YaxisTitle = (allRowsList[0].Split(new string[] { "\t" }, StringSplitOptions.None))[0].Replace("CTResults:  ", "");

                        for (int row = 3, i = 0; row < allRowsList.Count & i<form1.dataTV.Xaxis.Length; row++, i++)
                        {
                            form1.dataTV.Xaxis[i] = double.Parse((allRowsList[row].Split(new string[] { "\t" }, StringSplitOptions.None))[0]);
                            form1.dataTV.OriginalXaxis[i] = double.Parse((allRowsList[row].Split(new string[] { "\t" }, StringSplitOptions.None))[0]);
                        }
                    }
                }
            }
            public static double[] Subset(TreeNode parent, double[] Xvals, double[] Range)
            {
                int Start = 0;
                int End = Xvals.Length;

                for (int i = 0; i < Xvals.Length; i++)
                    if (Range[0] <= Xvals[i])
                    {
                        Start = i;
                        break;
                    }

                for (int i = Start; i < Xvals.Length; i++)
                    if (Xvals[i] >= Range[1])
                    {
                        End = i;
                        break;
                    }

                double[] target = new double[End - Start];

                double zero = Xvals[Start];
                //if (Start-1>0) zero = Xvals[Start - 1];

                for(int i = Start, j=0; i < End; i++,j++)
                {
                    target[j] = Xvals[i] - zero;
                }
                                
                Parallel.For(0, parent.Nodes.Count, (n) =>
                {
                    DataNode dN = (DataNode)parent.Nodes[n];
                    double[] source = dN.OriginalSeries;

                    int length = source.Length;
                    if (length > Start + target.Length) length = Start + target.Length;
                    if (length < 0) length = 0 ;

                    double[] target1 = new double[length-Start];

                    for (int i = Start, j = 0; i < length; i++, j++)
                    {
                        target1[j] = source[i];
                    }

                    dN.Series = target1;

                });

                return target;
            }
            public static void Normalize(TreeNode parent, int NormIndex)
            {
                //foreach(TreeNode node in parent.Nodes)
                Parallel.For(0, parent.Nodes.Count, (i) =>
                {
                    DataNode dN = (DataNode)parent.Nodes[i];
                    double[] source = dN.Series;
                    double[] target = new double[source.Length];
                    //Fund Max and Min
                    double Max = double.MinValue;
                    double Min = double.MaxValue;

                    foreach (double val in source)
                    {
                        if (val > Max) Max = val;

                        if (val < Min) Min = val;
                    }
                    /*
                    if (Math.Abs(Min) > Math.Abs(Max))
                    {
                        Array.Copy(dN.Series, source, source.Length);
                        for (int ind = 0; ind < source.Length; ind++)
                            source[ind] -= Min;

                        Max -= Min;
                    }*/

                    if (NormIndex == 1)
                        Max -= Min;

                    if (Max == 0) Max = 1;
                    for (int ind = 0; ind < source.Length; ind++)
                    {
                        if (NormIndex == 0)
                            target[ind] = source[ind] / Max;
                        else if (NormIndex == 1)
                            target[ind] = (source[ind] - Min) / Max;
                    }

                    dN.NormSeries = target;
                });
            }
        }

        public class FileSaver
        {
            public static void ReadCTDataFile(MyForm form1, string dir)
            {
                Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-GB");

                form1.dataTV.Nodes.Clear();
                form1.filterTV.Nodes.Clear();
                form1.solverClass.fitData.Nodes.Clear();

                foreach (var ser in form1.solverClass.fitChart1.ChartSeries)
                {
                    ser.Points.Clear();
                }

                TreeNode filters = new TreeNode();
                //Add event for projection here

                using (StreamReader sr = new StreamReader(OSStringConverter.StringToDir(dir)))
                {
                    string str = sr.ReadToEnd();
                    form1.dataTV.Store.Clear();
                    form1.SubsetCB.Checked = false;
                    form1.dNormCB.Checked = false;
                    form1.SubsetCB.Text = "Subset";
                    form1.SubsetCB.Tag = null;

                    foreach (string strLine in str.Split(new string[] { ";" }, StringSplitOptions.None))
                    {
                        StrTranslator(strLine, form1, filters);
                    }
                }

                form1.dataTV.SuspendLayout();
                foreach (TreeNode n in filters.Nodes)
                    form1.filterTV.Nodes.Add(n);

                filters = null;

                foreach (TreeNode n in form1.dataTV.Store)
                {

                    n.ExpandAll();

                    form1.dataTV.StoreToNodes(n);

                }

                form1.dataTV.BindColorsToNodes();

                form1.CheckCounter_Count(form1.dataTV, new EventArgs());

                form1.repeatsCh.DrawToScreen();

                form1.dataTV.ResumeLayout();

                #region subset
                if (form1.SubsetCB.Tag != null)
                {
                    CheckBox cb = form1.SubsetCB;
                    double[] range = (double[])form1.SubsetCB.Tag;
                    cb.Text = "Subset(" + range[0].ToString() + " - " + range[1].ToString() + ")";
                    cb.Width = TextRenderer.MeasureText(cb.Text, cb.Font).Width + 50;
                    foreach (var parent in form1.dataTV.Store)
                    {
                        form1.dataTV.Xaxis = FileReader.Subset(parent, form1.dataTV.OriginalXaxis, range);
                    }

                    foreach (var parent in form1.dataTV.Store)
                    {
                        FileReader.Normalize(parent, (int)form1.NormCB.Tag);
                    }

                    form1.repeatsCh.DrawToScreen();
                    form1.resultsCh.AddData();
                }
                #endregion subset
                if (form1.solverClass.fitData.Nodes.Count > 0)
                {
                    form1.solverClass.parametersPanel.LoadFitFromHistory(
                        form1.solverClass.fitData.GetFitSetingsFromNode(form1.solverClass.fitData.Nodes[0]));
                    form1.solverClass.parametersPanel.LoadFitFromHistory(
                        form1.solverClass.fitData.GetFitSetingsFromNode(form1.solverClass.fitData.Nodes[0]));
                }

            }
            public static void Open(MyForm form1)
            {
                OpenFileDialog ofd = new OpenFileDialog();
                string formatMiniStr = ".CTData";
                string formatStr = formatMiniStr.Substring(1, formatMiniStr.Length - 1) +
                    " files (*" + formatMiniStr + ")|*" + formatMiniStr;

                ofd.Filter = formatStr;
                ofd.FilterIndex = 1;
                ofd.RestoreDirectory = true;
                ofd.Title = "Open:";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    ReadCTDataFile(form1, OSStringConverter.GetWinString(ofd.FileName));
                }
            }
            private static void StrTranslator(string str, MyForm form1, TreeNode filters)
            {
                string[] vals = str.Split(new string[] { "=" }, StringSplitOptions.None);
                try
                {
                    switch (vals[0])
                    {
                        case "XaxisTitle":
                            form1.dataTV.XaxisTitle = vals[1];
                            break;
                        case "YaxisTitle":
                            form1.dataTV.YaxisTitle = vals[1];
                            break;
                        case "lastDir":
                            form1.dataTV.lastDir = vals[1];
                            break;
                        case "NormCB":
                            form1.NormCB.Checked = bool.Parse(vals[1]);
                            form1.NormTo1.Enabled = form1.NormCB.Checked;
                            form1.NormFrom0To1.Enabled = form1.NormCB.Checked;
                            form1.dNormCB.Enabled = form1.NormCB.Checked;
                            break;
                        case "NormTo1":
                            form1.NormTo1.Checked = bool.Parse(vals[1]);
                            break;
                        case "NormFrom0To1":
                            form1.NormFrom0To1.Checked = bool.Parse(vals[1]);
                            break;
                        case "IndNormCB":
                            form1.NormCB.Tag = int.Parse(vals[1]);
                            break;
                        case "dNormCB":
                            form1.dNormCB.Checked = bool.Parse(vals[1]);
                            break;
                        case "Subset":
                            string[] set = vals[1].Split(new string[] { "\t" }, StringSplitOptions.None);
                            form1.SubsetCB.Checked = true;
                            form1.SubsetCB.Tag = new double[] { double.Parse(set[0]), double.Parse(set[1]) };
                            break;
                        case "StDevCB":
                            form1.StDevCB.Checked = bool.Parse(vals[1]);
                            break;
                        case "XaxisVals":
                            string[] valsAll = vals[1].Split(new string[] { "\t" }, StringSplitOptions.None);
                            form1.dataTV.Xaxis = new double[valsAll.Length];
                            form1.dataTV.OriginalXaxis = new double[valsAll.Length];
                            for (int i = 0; i < valsAll.Length; i++)
                            {
                                form1.dataTV.Xaxis[i] = double.Parse(valsAll[i]);
                                form1.dataTV.OriginalXaxis[i] = double.Parse(valsAll[i]);
                            }
                            break;
                        case "Filters":
                            if (vals[1] == "") return;
                            string[] filtAll = vals[1].Split(new string[] { "\t" }, StringSplitOptions.None);

                            for (int i = 0; i < filtAll.Length - 1; i += 2)
                            {
                                TreeNode node = new TreeNode();
                                node.Text = filtAll[i];
                                node.Checked = bool.Parse(filtAll[i + 1]);
                                //form1.filterTV.Nodes.Add(node);
                                filters.Nodes.Add(node);
                            }
                            break;
                        case "WorkDirs":
                            if (vals[1] == "") return;
                            string[] wdAll = vals[1].Split(new string[] { "\n" }, StringSplitOptions.None);
                            for (int i = 0; i < wdAll.Length; i += 3)
                            {
                                TreeNode node = new TreeNode();
                                node.Text = wdAll[i];
                                node.Tag = wdAll[i + 1];
                                node.Checked = bool.Parse(wdAll[i + 2]);
                                form1.dataTV.Store.Add(node);
                            }
                            break;
                        case "DataNode":
                            if (vals[1] == "") return;
                            string[] nodeAll = vals[1].Split(new string[] { "\n" }, StringSplitOptions.None);

                            DataNode dN = new DataNode();
                            dN.Text = nodeAll[1];
                            dN.Tag = nodeAll[2];
                            dN.Checked = bool.Parse(nodeAll[3]);
                            dN.Comment = nodeAll[4];
                            dN.RoiName = nodeAll[5];

                            //series
                            string[] dN_Ser = nodeAll[6].Split(new string[] { "\t" }, StringSplitOptions.None);
                            string[] dN_Norm = nodeAll[7].Split(new string[] { "\t" }, StringSplitOptions.None);
                            dN.OriginalSeries = new double[dN_Ser.Length];
                            dN.Series = new double[dN_Ser.Length];
                            dN.NormSeries = new double[dN_Norm.Length];
                            for (int i = 0; i < dN_Ser.Length; i++)
                            {
                                dN.Series[i] = double.Parse(dN_Ser[i]);
                                dN.OriginalSeries[i] = double.Parse(dN_Ser[i]);

                                if(i< dN_Norm.Length)
                                    dN.NormSeries[i] = double.Parse(dN_Norm[i]);
                            }

                            form1.dataTV.Store[int.Parse(nodeAll[0])].Nodes.Add(dN);
                            break;
                        case "FitData":
                            if(vals[1]!= null && vals[1]!="")
                                form1.solverClass.fitData.StringToData(vals[1]);
                            break;
                        case "FitF":
                            if (vals[1] != null && vals[1] != "")
                                AddUsedFormulas(form1,vals[1]);
                            break;
                    }
                }
                catch
                {
                   MessageBox.Show("Error: " + vals[0].ToString());
                }
            }
            public static void Save(MyForm form1)
            {
                SaveFileDialog saveFileDialog1 = new SaveFileDialog();
                string formatMiniStr = ".CTData";
                string formatStr = "CTData files (*" + formatMiniStr + ")|*" + formatMiniStr;
                saveFileDialog1.Filter = formatStr;
                saveFileDialog1.FilterIndex = 1;
                saveFileDialog1.RestoreDirectory = true;
                saveFileDialog1.OverwritePrompt = true;
                saveFileDialog1.Title = "Save As:";

                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    string dir = OSStringConverter.GetWinString(saveFileDialog1.FileName);
                    SaveCTDataFile(form1, dir);
                }
            }
            public static BackgroundWorker SaveCTDataFile(MyForm form1, string dir)
            {
                
                if (!dir.EndsWith(".CTData"))
                    dir += ".CTData";

                try
                {
                    if (File.Exists(OSStringConverter.StringToDir(dir)))
                        File.Delete(OSStringConverter.StringToDir(dir));
                }
                catch
                {
                    MessageBox.Show("Save error!\nFile is opened in other program!");
                    return null;
                }

                List<string> strL = new List<string>();

                //background worker
                var bgw = new BackgroundWorker();
                bgw.WorkerReportsProgress = true;
                //Add event for projection here
                bgw.DoWork += new DoWorkEventHandler(delegate (Object o, DoWorkEventArgs a)
                {
                    Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-GB");

                    strL.Add("XaxisTitle=" + form1.dataTV.XaxisTitle);
                    strL.Add("YaxisTitle=" + form1.dataTV.YaxisTitle);
                    strL.Add("lastDir=" + form1.dataTV.lastDir);
                    strL.Add("XaxisVals=" + string.Join("\t", form1.dataTV.OriginalXaxis));
                    strL.Add("NormCB=" + form1.NormCB.Checked.ToString());
                    strL.Add("NormTo1=" + form1.NormTo1.Checked.ToString());
                    strL.Add("NormFrom0To1=" + form1.NormFrom0To1.Checked.ToString());
                    strL.Add("IndNormCB=" + ((int)form1.NormCB.Tag).ToString());
                    strL.Add("dNormCB=" + form1.dNormCB.Checked.ToString());

                    if (form1.SubsetCB.Checked == true)
                    {
                        double[] set = (double[])form1.SubsetCB.Tag;
                        strL.Add("Subset=" + set[0].ToString() + "\t" + set[1].ToString());
                    }
                    strL.Add("StDevCB=" + form1.StDevCB.Checked.ToString());

                    List<string> filtL = new List<string>();
                    foreach (TreeNode node in form1.filterTV.Nodes)
                    {
                        filtL.Add(node.Text);
                        filtL.Add(node.Checked.ToString());
                    }
                    strL.Add("Filters=" + string.Join("\t", filtL));

                    List<string> wdL = new List<string>();
                    foreach (TreeNode node in form1.dataTV.Store)
                    {
                        wdL.Add(node.Text);
                        wdL.Add((string)node.Tag);
                        wdL.Add(node.Checked.ToString());
                    }
                    strL.Add("WorkDirs=" + string.Join("\n", wdL));

                    for (int i = 0; i < form1.dataTV.Store.Count; i++)
                        foreach (TreeNode n in form1.dataTV.Store[i].Nodes)
                        {
                            DataNode node = (DataNode)n;

                            List<string> nodeTag = new List<string>();
                            nodeTag.Add(i.ToString());
                            nodeTag.Add(node.Text);
                            nodeTag.Add((string)node.Tag);
                            nodeTag.Add(node.Checked.ToString());
                            nodeTag.Add(node.Comment);
                            nodeTag.Add(node.RoiName);
                            nodeTag.Add(string.Join("\t", node.OriginalSeries));
                            nodeTag.Add(string.Join("\t", node.NormSeries));

                            strL.Add("DataNode=" + string.Join("\n", nodeTag));
                        }
                    ((BackgroundWorker)o).ReportProgress(0);
                });

                bgw.ProgressChanged += new ProgressChangedEventHandler(delegate (Object o, ProgressChangedEventArgs a)
                {
                    if (a.ProgressPercentage == 0)
                    {
                        //Export fits
                        strL.Add("FitData=" + form1.solverClass.fitData.DataToString());
                        strL.Add("FitF=" + GetUsedFormulas(form1));
                        //Save file
                        File.WriteAllText(OSStringConverter.StringToDir(dir), string.Join(";", strL));
                    }
                    form1.StatusLabel.Text = "Ready";
                });
                //Start background worker
                form1.StatusLabel.Text = "Saving...";
                //start bgw
                bgw.RunWorkerAsync();
                return bgw;
            }
            private static string GetUsedFormulas(MyForm form1)
            {
                List<string> strL = new List<string>();
                foreach (TreeNode n1 in form1.solverClass.fitData.Nodes)
                {
                    var fit =
                    form1.solverClass.parametersPanel.GetFunctionValueFormulas(
                        form1.solverClass.fitData.GetFitSetingsFromNode(n1));
                    if (fit == null) continue;

                    string str = fit.ToStringTabs().Replace(";", "#").Replace("=","$");

                    if (!strL.Contains(str))
                        strL.Add(str);
                }

                return string.Join("|", strL);
            }
            private static void AddUsedFormulas(MyForm form1, string input)
            {
                foreach (string strL in input.Split(new string[] { "|" }, StringSplitOptions.None))
                {
                    string str = strL.Replace("#", ";").Replace("$", "=");
                    
                    var fit = new SolverFunctions.FunctionValue(str);
                    MySolver.FitSettings fitSet = new MySolver.FitSettings(fit.GetFormula1, fit.GetFormula2,
                        fit.GetFormulaIF, fit.GetParametersDictionary(), new double[0], new double[0]);

                    form1.solverClass.parametersPanel.CheckForFormulas(fitSet, fit.GetName);
                }
            }
            public static void Export1(MyForm form1)
            {
                SaveFileDialog saveFileDialog1 = new SaveFileDialog();
                string formatMiniStr = ".txt";
                string formatStr = "TAB delimited files (*" + formatMiniStr + ")|*" + formatMiniStr;
                saveFileDialog1.Filter = formatStr;
                saveFileDialog1.FilterIndex = 1;
                saveFileDialog1.RestoreDirectory = true;
                saveFileDialog1.OverwritePrompt = true;
                saveFileDialog1.Title = "Export Data";

                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    string dir = OSStringConverter.GetWinString( saveFileDialog1.FileName);
                    if (dir.EndsWith(formatMiniStr) == false)
                        dir += formatMiniStr;

                    try
                    {
                        if (File.Exists(OSStringConverter.StringToDir(dir)))
                            File.Delete(OSStringConverter.StringToDir(dir));
                    }
                    catch
                    {
                        MessageBox.Show("Save error!\nFile is opened in other program!");
                        return;
                    }

                    double[] xAxis = form1.dataTV.Xaxis;

                    string xAxisTitle = form1.dataTV.XaxisTitle;
                    string yAxisTitle = form1.dataTV.YaxisTitle;

                    int TotalN = 0;
                    foreach (TreeNode tn in form1.dataTV.Nodes)
                        TotalN += tn.Nodes.Count;

                    TotalN += TotalN + 5;

                    string[][] res = new string[xAxis.Length + 3][];

                    //background worker
                    var bgw = new BackgroundWorker();
                    bgw.WorkerReportsProgress = true;
                    //Add event for projection here
                    bgw.DoWork += new DoWorkEventHandler(delegate (Object o, DoWorkEventArgs a)
                    {
                        for (int i = 0; i < res.Length; i++)
                            res[i] = new string[TotalN];

                        res[0][0] = "DataTable: " + xAxisTitle + "/" + yAxisTitle;
                        res[1][0] = "Names:";
                        res[2][0] = "Comments:";

                        for (int row = 3, i = 0; row < res.Length; row++, i++)
                        {
                            res[row][0] = xAxis[i].ToString();
                        }

                        int position = 1;
                        //data
                        foreach (TreeNode parent in form1.dataTV.Nodes)
                            foreach (TreeNode node in parent.Nodes)
                                if (node.Checked)
                                {
                                    DataNode dN = (DataNode)node.Tag;
                                    res[1][position] = dN.Text.Replace("\t", "_");
                                    res[2][position] = dN.Comment;
                                    for (int row = 3, i = 0; row < res.Length &&
                                    i < dN.NormSeries.Length; row++, i++)
                                    {
                                        res[row][position] = dN.Series[i].ToString();
                                    }

                                    position++;
                                }
                        //norm data
                        foreach (TreeNode parent in form1.dataTV.Nodes)
                            foreach (TreeNode node in parent.Nodes)
                                if (node.Checked)
                                {
                                    DataNode dN = (DataNode)node.Tag;
                                    res[1][position] = "n" + dN.Text.Replace("\t", "_");
                                    res[2][position] = dN.Comment;

                                    for (int row = 3, i = 0; row < res.Length &&
                                    i < dN.NormSeries.Length; row++, i++)
                                    {
                                        res[row][position] = dN.NormSeries[i].ToString();
                                    }

                                    position++;
                                }
                        //Mid And StDev

                        int position1 = position + 1;
                        double[][] Navg = AddData(form1, false);

                        for (int row = 3, i = 0; row < res.Length && i < Navg[0].Length; row++, i++)
                        {
                            res[row][position] = Navg[0][i].ToString();
                            res[row][position1] = Navg[1][i].ToString();
                        }

                        res[1][position] = "Navg";
                        res[1][position1] = "StDev";

                        position += 2;
                        position1 += 2;
                        Navg = AddData(form1, true);
                        for (int row = 3, i = 0; row < res.Length && i < Navg[0].Length; row++, i++)
                        {
                            res[row][position] = Navg[0][i].ToString();
                            res[row][position1] = Navg[1][i].ToString();
                        }
                        res[1][position] = "nNavg";
                        res[1][position1] = "nStDev";

                        ((BackgroundWorker)o).ReportProgress(0);
                    });

                    bgw.ProgressChanged += new ProgressChangedEventHandler(delegate (Object o, ProgressChangedEventArgs a)
                    {
                        if (a.ProgressPercentage == 0)
                        {
                            //Save file
                            using (StreamWriter write = new StreamWriter(OSStringConverter.StringToDir(dir)))
                            {
                                for (int i = 0; i < res.Length; i++)
                                {
                                    write.WriteLine(string.Join("\t", res[i]));
                                }
                            }
                        }
                        form1.StatusLabel.Text = "Ready";
                    });
                    //Start background worker
                    form1.StatusLabel.Text = "Exporting...";
                    //start bgw
                    bgw.RunWorkerAsync();

                }
            }
            public static void Export(MyForm form1)
            {
                SaveFileDialog saveFileDialog1 = new SaveFileDialog();
                string formatMiniStr = ".txt";
                string formatStr = "TAB delimited files (*" + formatMiniStr + ")|*" + formatMiniStr;
                saveFileDialog1.Filter = formatStr;
                saveFileDialog1.FilterIndex = 1;
                saveFileDialog1.RestoreDirectory = true;
                saveFileDialog1.OverwritePrompt = true;
                saveFileDialog1.Title = "Export Data";

                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    string dir = OSStringConverter.GetWinString(saveFileDialog1.FileName);

                    if (dir.EndsWith(formatMiniStr) == false)
                        dir += formatMiniStr;

                    try
                    {
                        if (File.Exists(OSStringConverter.StringToDir(dir)))
                            File.Delete(OSStringConverter.StringToDir(dir));
                    }
                    catch
                    {
                        MessageBox.Show("Save error!\nFile is opened in other program!");
                        return;
                    }

                    double[] xAxis = form1.dataTV.Xaxis;

                    string xAxisTitle = form1.dataTV.XaxisTitle;
                    string yAxisTitle = form1.dataTV.YaxisTitle;

                    #region Time
                    //Add first column
                    string[] TimeColumn = new string[xAxis.Length + 3];
                    TimeColumn[0] = "DataTable: " + xAxisTitle + "/" + yAxisTitle;
                    TimeColumn[1] = "Names:";
                    TimeColumn[2] = "Comments:";

                    for (int i = 3,j=0; i < TimeColumn.Length; i++,j++)
                        TimeColumn[i] = xAxis[j].ToString();

                    #endregion Time

                    #region Raw and Norm Data
                    //Raw data
                    List<string>[] RawData = new List<string>[xAxis.Length + 3];
                    List<string>[] NormData = new List<string>[xAxis.Length + 3];
                    for (int i = 0; i < RawData.Length; i++)
                    {
                        RawData[i] = new List<string>();
                        NormData[i] = new List<string>();
                    }

                    double[] Mean = new double[xAxis.Length];
                    double[] stDev = new double[xAxis.Length];
                    double[] nMean = new double[xAxis.Length];
                    double[] nStDev = new double[xAxis.Length];
                    int counter = 0;

                    //extract data from nodes
                    foreach (TreeNode parent in form1.dataTV.Nodes)
                        foreach (TreeNode node in parent.Nodes)
                            if (node.Checked)
                            {
                                DataNode dN = (DataNode)node.Tag;
                                RawData[0].Add("");
                                RawData[1].Add(dN.Text.Replace("\t", "_"));
                                RawData[2].Add(dN.Comment);
                                NormData[0].Add("");
                                NormData[1].Add("n" + dN.Text.Replace("\t", "_"));
                                NormData[2].Add(dN.Comment);
                                
                                for (int row = 3, i = 0; row < RawData.Length; row++, i++)
                                {
                                    if (i < dN.Series.Length)
                                    {
                                        RawData[row].Add(dN.Series[i].ToString());
                                        Mean[i] += dN.Series[i];
                                        NormData[row].Add(dN.NormSeries[i].ToString());
                                        nMean[i] += dN.NormSeries[i];
                                    }
                                    else
                                    {
                                        RawData[row].Add("");
                                    }
                                }
                                counter++;
                            }

                    if(counter == 0)
                    {
                        MessageBox.Show("No selected data for exporting!");
                        return;
                    }
                    //Calculate mean
                    for(int i = 0; i< Mean.Length; i++)
                    {
                        Mean[i] /= counter;
                        nMean[i] /= counter;
                    }
                    //Calculate StDev

                    RawData[0].Add("");
                    RawData[0].Add("");
                    RawData[1].Add("Mean");
                    RawData[1].Add("StDev");
                    RawData[2].Add("");
                    RawData[2].Add("");

                    NormData[0].Add("");
                    NormData[0].Add("");
                    NormData[0].Add("");
                    NormData[1].Add("nMean");
                    NormData[1].Add("nStDev");
                    NormData[1].Add("nnMean");
                    NormData[2].Add("");
                    NormData[2].Add("");
                    NormData[2].Add("");

                    foreach (TreeNode parent in form1.dataTV.Nodes)
                        foreach (TreeNode node in parent.Nodes)
                            if (node.Checked)
                            {
                                DataNode dN = (DataNode)node.Tag;                                

                                for (int i = 0; i < RawData.Length; i++)
                                    if (i < dN.Series.Length)
                                    {
                                        stDev[i] += Math.Pow(Mean[i] - dN.Series[i],2);
                                        nStDev[i] += Math.Pow(nMean[i] - dN.NormSeries[i], 2);
                                    }
                            }
                    //Add Mean, StDev and Norm to the results
                    double max = nMean.Max();
                    if (max == 0) max = 1;

                    for (int i = 0,row=3; i < Mean.Length && row<RawData.Length; i++,row++)
                    {
                        RawData[row].Add(Mean[i].ToString());
                        RawData[row].Add((Math.Sqrt(stDev[i]/(counter-1))).ToString());
                        NormData[row].Add(nMean[i].ToString());
                        NormData[row].Add((Math.Sqrt(nStDev[i] / (counter - 1))).ToString());
                        NormData[row].Add((nMean[i]/max).ToString());
                    }
                    #endregion Raw and Norm Data

                    #region Filter Data
                    List<List<string>> FitRes = new List<List<string>>();
                    foreach (TreeNode n in form1.solverClass.fitData.Nodes)
                        if (n.Checked)
                        {
                            MySolver.FitSettings curFit = form1.solverClass.fitData.GetFitSetingsFromNode(n);
                            if (curFit.GetFormulaIF.StartsWith("FRAP"))//send to frapa model
                            {
                                List<List<string>> smallFitRes =
                                    FRAPA_Model.AllModels.ExportResults(curFit, n.Text);

                                foreach (var l in smallFitRes)
                                    FitRes.Add(l);

                                smallFitRes = null;
                            }
                            else//custom models
                            {
                                List<string> constNames = new List<string>() { "", "Const_" + n.Text, "" };
                                List<string> constVals = new List<string>() { "", "Value_" + n.Text, "" };
                                List<string> XValsL = new List<string>() { "", "Xvals_" + n.Text, "" };
                                List<string> YValsL = new List<string>() { "", "Raw_" + n.Text, "" };
                                List<string> FitValsL = new List<string>() { "", "Fit_" + n.Text, "" };

                                foreach (var p in curFit.Parameters)
                                {
                                    constNames.Add(p.Value.Name);
                                    constVals.Add(p.Value.Value.ToString());
                                }

                                double[] Xvals = curFit.XVals;
                                double[] Yvals = curFit.YVals;

                                string formula = curFit.FormulaForNcalc();
                                Expression e = new Expression(formula);

                                foreach (var kvp in curFit.Parameters)
                                {
                                    MySolver.Parameter p = kvp.Value;
                                    e.Parameters[p.Name] = p.Value;
                                    //val = val.Replace(p.Name, p.Value.ToString());
                                }

                                double StDev = 0;
                                double[] fitVals = new double[Xvals.Length];

                                for (int i = 0; i < Xvals.Length && i < Yvals.Length; i++)
                                {
                                    //fit
                                    e.Parameters["t"] = Xvals[i];

                                    double val = 0;
                                    double.TryParse(e.Evaluate().ToString(), out val);

                                    fitVals[i] = val;
                                    FitValsL.Add(val.ToString());
                                    XValsL.Add(Xvals[i].ToString());
                                    YValsL.Add(Yvals[i].ToString());

                                    StDev += Math.Pow(Yvals[i] - val, 2);
                                }
                                curFit.StDev = Math.Sqrt(StDev / (Xvals.Length));

                                constNames.Add("");
                                constVals.Add("");
                                constNames.Add("Root mean square deviation");
                                constVals.Add(curFit.StDev.ToString());
                                constNames.Add("Coefficient of correlation:");
                                constVals.Add(FRAPA_Model.ComputeCorelationCoeff(Yvals, fitVals).ToString());
                                constNames.Add("R - squared:");
                                constVals.Add(Math.Pow(FRAPA_Model.ComputeCorelationCoeff(Yvals, fitVals),2).ToString());
                                constNames.Add("");
                                constVals.Add("");
                                constNames.Add("IF:");
                                constVals.Add(curFit.GetFormulaIF);
                                constNames.Add("f1:");
                                constVals.Add(curFit.GetFormula1);
                                constNames.Add("f2");
                                constVals.Add(curFit.GetFormula2);

                                #region Fit Statistics
                                double FitMax = fitVals.Max();
                                double HalfVal = FitMax / 2;
                                double HalfUp = double.NaN;
                                double HalfDown = double.NaN;
                                double HalfDownRec = double.NaN;

                                int ind = 0;
                                //find max position
                                for (int i = 0; i < fitVals.Length; i++)
                                    if (fitVals[i] == FitMax)
                                    {
                                        ind = i;
                                        break;
                                    }
                                //calculate end point

                                e.Parameters["t"] = 10000;
                                double end = 0;
                                double.TryParse(e.Evaluate().ToString(), out end);
                                ResultsExtractor_HalfTimeCalculator htCalc = new ResultsExtractor_HalfTimeCalculator();
                                //calculate halftime of recruitment
                                for (int i = 0; i < ind; i++)
                                    if (fitVals[i] >= HalfVal)
                                    {
                                        if (Xvals.Length > i)
                                            HalfUp = Xvals[i];
                                        break;
                                    }
                                HalfUp = htCalc.SolveHalfTime(e, 1000, Xvals[0], Xvals[ind], HalfVal, HalfUp);
                                //calculate halftime of removal
                                if (FitMax - fitVals[fitVals.Length - 1] > 0)
                                {
                                    for (int i = fitVals.Length - 1; i > ind; i--)
                                        if (fitVals[i] >= HalfVal)
                                        {
                                            if (Xvals.Length > i && i != fitVals.Length - 1)
                                                HalfDown = Xvals[i];
                                            break;
                                        }
                                    HalfDown = htCalc.SolveHalfTime(e, 1000, Xvals[ind], Xvals[fitVals.Length - 1], HalfVal, HalfDown);

                                }
                                //recalculated halftime of removal
                                HalfVal = (FitMax - end) / 2;
                                if (HalfVal > 0)
                                {
                                    for (int i = fitVals.Length - 1; i > ind; i--)
                                        if (fitVals[i] >= HalfVal)
                                        {
                                            if (Xvals.Length > i && i != fitVals.Length - 1)
                                                HalfDownRec = Xvals[i];

                                            break;
                                        }
                                    HalfDownRec = htCalc.SolveHalfTime(e, 1000, Xvals[ind], Xvals[fitVals.Length - 1], HalfVal, HalfDownRec);

                                }
                                fitVals = null;
                                //add vals
                                constNames.Add("");
                                constVals.Add("");
                                constNames.Add("Maximum");
                                constVals.Add(FitMax.ToString());
                                constNames.Add("HalfTimeOfRecruitment");
                                constVals.Add(HalfUp.ToString());
                                constNames.Add("HalfTimeOfRemoval");
                                constVals.Add(HalfDown.ToString());
                                constNames.Add("HalfTimeOfRemoval(Recalculated)");
                                constVals.Add(HalfDownRec.ToString());

                                #endregion Fit Statistics

                                FitRes.Add(constNames);
                                FitRes.Add(constVals);
                                FitRes.Add(XValsL);
                                FitRes.Add(YValsL);
                                FitRes.Add(FitValsL);
                            }
                        }
                    #endregion Filter Data

                    #region Save file
                    using (StreamWriter write = new StreamWriter(OSStringConverter.StringToDir(dir)))
                    {
                        max = 0;

                        foreach (List<string> s in FitRes)
                            if (s.Count > max) max = s.Count;

                        for (int i = 0; i < TimeColumn.Length; i++)
                        {
                            /*
                            string str = TimeColumn[i] + "\t" +
                             string.Join("\t", RawData[i]) + "\t" +
                             string.Join("\t", NormData[i]);
                             */
                            string str = TimeColumn[i];

                            foreach (var s in RawData[i])
                                str += "\t" + s;

                            foreach (var s in NormData[i])
                                str += "\t" + s;

                            foreach (List<string> s in FitRes)
                                if (i < s.Count)
                                    str += "\t" + s[i];
                                else
                                    str += "\t";

                            write.WriteLine(str);
                        }

                        if (max != 0 && max > TimeColumn.Length)
                        {
                            string strList = string.Join("\t", new string[1 + RawData[0].Count + NormData[0].Count]);
                            for (int i = TimeColumn.Length; i < max; i++)
                            {
                                string str = strList;

                                foreach (List<string> s in FitRes)
                                    if (i < s.Count)
                                        str += "\t" + s[i];
                                    else
                                        str += "\t";

                                write.WriteLine(str);
                            }
                        }

                        TimeColumn = null;
                        RawData = null;
                        NormData = null;
                        FitRes = null;
                    }
                    #endregion Save file
                }
            }
            public static double[][] AddData(MyForm form1, bool Norm)
            {
                double[] ser = new double[form1.dataTV.Xaxis.Length];
                double[] dev = new double[form1.dataTV.Xaxis.Length];

                int counter = 0;

                //calc navg ser
                foreach (TreeNode parent in form1.dataTV.Nodes)
                    foreach (TreeNode node in parent.Nodes)
                        if (node.Checked)
                        {
                            DataNode curN = (DataNode)node.Tag;
                            double[] source = null;

                            if (Norm)
                                source = curN.NormSeries;
                            else
                                source = curN.Series;

                            counter++;
                            for (int i = 0; i < source.Length && i < ser.Length; i++)
                                ser[i] += source[i];
                        }

                for (int i = 0; i < ser.Length; i++)
                    ser[i] /= counter;

                foreach (TreeNode parent in form1.dataTV.Nodes)
                    foreach (TreeNode node in parent.Nodes)
                        if (node.Checked)
                        {
                            DataNode curN = (DataNode)node.Tag;

                            double[] source = null;

                            if (Norm)
                                source = curN.NormSeries;
                            else
                                source = curN.Series;

                            for (int i = 0; i < source.Length && i < ser.Length; i++)
                                dev[i] += Math.Pow(source[i] - ser[i], 2);
                        }

                for (int i = 0; i < dev.Length; i++)
                    dev[i] = Math.Sqrt(dev[i] / (counter - 1));
                return new double[][] { ser, dev };
            }
        }
        public class ResultsChart : ResultsExtractor_CTChart
        {
            private MyForm form1;
            public ResultsExtractor_CTChart.Series Navg;
            private ResultsExtractor_CTChart.Series StDev;
            private ResultsExtractor_CTChart.Series Samp;
            public ResultsChart(MyForm form1)
            {
                this.Visible = false;
                this.form1 = form1;
                this.Build(form1);
                //Chart

                StDev = new ResultsExtractor_CTChart.Series();
                StDev.Color = Color.FromArgb(50, Color.Blue);
                StDev.ErrorBar = true;
                this.ChartSeries.Add(StDev);

                Navg = new ResultsExtractor_CTChart.Series();
                Navg.Color = Color.Blue;
                this.ChartSeries.Add(Navg);
                
                Samp = new ResultsExtractor_CTChart.Series();
                Samp.Color = Color.Blue;
                this.ChartSeries.Add(Samp);
            }
            public void AddData()
            {
                Navg.Points.Clear();
                StDev.Points.Clear();
                StDev.ErrorVals.Clear();
                Samp.Points.Clear();

                if (form1.dataTV.Xaxis == null) return;
                double[] ser = new double[form1.dataTV.Xaxis.Length];
                double[] dev = new double[form1.dataTV.Xaxis.Length];
                int counter = 0;

                //calc navg ser
                foreach (TreeNode parent in form1.dataTV.Nodes)
                    foreach (TreeNode node in parent.Nodes)
                        if (node.Checked)
                        {
                            DataNode curN = (DataNode)node.Tag;
                            double[] source = null;

                            if (form1.NormCB.Checked)
                                source = curN.NormSeries;
                            else
                                source = curN.Series;

                            counter++;
                            for (int i = 0; i < source.Length && i < ser.Length; i++)
                                ser[i] += source[i];
                        }

                if (counter == 0) counter = 1;
                for (int i = 0; i < ser.Length; i++)
                    ser[i] /= counter;

                if (form1.NormCB.Checked && form1.dNormCB.Checked)
                {
                    double max = ser.Max();
                    if (max == 0) max = 1;
                    for (int i = 0; i < ser.Length; i++)
                        ser[i] /= max;
                }

                if (form1.StDevCB.Checked)
                {
                    foreach (TreeNode parent in form1.dataTV.Nodes)
                        foreach (TreeNode node in parent.Nodes)
                            if (node.Checked)
                            {
                                DataNode curN = (DataNode)node.Tag;

                                double[] source = null;

                                if (form1.NormCB.Checked)
                                    source = curN.NormSeries;
                                else
                                    source = curN.Series;

                                for (int i = 0; i < source.Length && i < ser.Length; i++)
                                    dev[i] += Math.Pow(source[i] - ser[i], 2);
                            }

                    for (int i = 0; i < dev.Length; i++)
                        dev[i] = Math.Sqrt(dev[i] / (counter - 1));
                }
                double[] Xdata = form1.dataTV.Xaxis;

                for (int i = 0; i < dev.Length; i++)
                {
                    Navg.Points.AddXY(Xdata[i], ser[i]);
                    if (form1.StDevCB.Checked)
                    {
                        StDev.Points.AddXY(Xdata[i], ser[i]);
                        StDev.ErrorVals.Add(dev[i]);
                    }
                }

                if (form1.dataTV.SelectedNode != null &&
                   form1.dataTV.SelectedNode.Checked &&
                   form1.dataTV.SelectedNode.ImageIndex != 0)
                {
                    DataNode curN = (DataNode)form1.dataTV.SelectedNode.Tag;
                    double[] serN = null;

                    if (form1.NormCB.Checked)
                        serN = curN.NormSeries;
                    else
                        serN = curN.Series;

                    for (int i = 0; i < Xdata.Length && i < serN.Length; i++)
                    {
                        Samp.Points.AddXY(Xdata[i], serN[i]);
                    }
                    Samp.Color = form1.dataTV.colors[form1.dataTV.SelectedNode.ImageIndex];
                    Samp.Enabled = true;
                }
                else
                {
                    Samp.Enabled = false;
                }

                this.Dock = DockStyle.Fill;
                this.Visible = true;
                this.GLDrawing_Start();
            }
        } 
        public class RepeatsChart : GLControl
        {
            MyForm form1;
            private int id;
            public RepeatsChart(MyForm form1)
            {
                this.form1 = form1;

                this.MakeCurrent();
                GL.ClearColor(Parametars.BackGround2Color);
                //ImageTexture
                ReserveTextureID();

                this.Paint += GLControl_Paint;
                this.Resize += GLControl_Resize;
                this.MouseClick += GLControl_MouseClick;
            }
            public void DrawToScreen()
            {
                GLDrawing_Start();
            }

            #region GLControl_Events
            private void ClearImage()
            {
                //Activate Control
                this.MakeCurrent();
                //Load background
                GL.ClearColor(Color.White);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                //Prepare MatrixMode
                GL.MatrixMode(MatrixMode.Projection);
                GL.LoadIdentity();

                this.SwapBuffers();
            }

            private void GLControl_Resize(object sender, EventArgs e)
            {

                GLDrawing_Start();
                //Activate Control
                //this.MakeCurrent();

                //GL.Viewport(0, 0, this.Width, this.Height);
                //GL.MatrixMode(MatrixMode.Projection);
                //GL.LoadIdentity();

            }

            private void GLControl_Paint(object sender, EventArgs e)
            {
                //Global variables
                GLDrawing_Start();
            }
            #endregion GLControl_Events
            private void GLDrawing_Start()
            {
                this.Dock = DockStyle.Fill;
                if (this.Visible == false) { this.Visible = true; }
                //Activate Control
                this.MakeCurrent();
                GL.Disable(EnableCap.Texture2D);
                //Load background
                GL.ClearColor(Color.White);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                //Prepare MatrixMode
                GL.MatrixMode(MatrixMode.Projection);
                GL.LoadIdentity();
                //Prepare Projection
                GL.Ortho(0.0, (double)this.Width, (double)this.Height, 0.0, -1.0, 1.0);

                GL.MatrixMode(MatrixMode.Modelview);
                //Set viewpoint
                GL.Viewport(0, 0, this.Width, this.Height);

                //draw chart
                try
                {
                    Render();
                }
                catch
                {
                    ClearImage();
                    this.Visible = false;
                }

                this.SwapBuffers();
                try
                {
                    form1.resultsCh.AddData();
                }
                catch { }
            }

            #region Rendering
            private void GLControl_MouseClick(object sender, MouseEventArgs e)
            {
                form1.dataTV.Focus();
                if (rect == null || stepX == 0 || stepY == 0) return;

                double X = (e.X - rect.X) / stepX;

                int i = 0;
                for (i = 0; i < form1.dataTV.Xaxis.Length; i++)
                    if (X < form1.dataTV.Xaxis[i])
                    {
                        i--;
                        break;
                    }

                if (i < 0 || i >= form1.dataTV.Xaxis.Length - 1)
                {
                    form1.dataTV.SelectedNode = null;
                    GLDrawing_Start();
                    form1.resultsCh.AddData();
                    return;
                }

                double bestDY = double.MaxValue;
                TreeNode tempN = null;

                foreach (TreeNode parent in form1.dataTV.Nodes)
                    foreach (TreeNode node in parent.Nodes)
                        if (node.Checked)
                        {
                            DataNode dN = (DataNode)node.Tag;
                            double[] ser = null;

                            if (form1.NormCB.Checked)
                                ser = dN.NormSeries;
                            else
                                ser = dN.Series;

                            if (ser.Length <= i + 1) continue;
                            //double dY = e.Y - (rect.Y + rect.Height - (ser[i] - MinY) * stepY);

                            double y1 = (rect.Y + rect.Height - (ser[i + 1] - MinY) * stepY);
                            double y0 = (rect.Y + rect.Height - (ser[i + 1] - MinY) * stepY);
                            double x1 = form1.dataTV.Xaxis[i + 1] * stepX + rect.X;
                            double x0 = form1.dataTV.Xaxis[i] * stepX + rect.X;

                            double dY = e.Y - ((((y1 - y0) / (x1 - x0)) * (e.X - x0)) + y0);

                            if (Math.Abs(dY) <= 10 && bestDY > Math.Abs(dY))
                            {
                                tempN = node;
                                bestDY = Math.Abs(dY);
                                //form1.dataTV.Focus();
                                //return;
                            }



                        }
               form1.dataTV.SelectedNode = tempN;
                if (tempN == null)
                {
                    GLDrawing_Start();
                    form1.resultsCh.AddData();
                }
                //form1.dataTV.Focus();
            }

            private RectangleF rect;
            private double stepX = 0;
            private double stepY = 0;
            private double MinY = 0;

            private void Render()
            {
                Rectangle OldRect = new Rectangle(0, 0, this.Width, this.Height);

                double MaxY = 0;
                MinY = 0;

                foreach (TreeNode parent in form1.dataTV.Nodes)
                    foreach (TreeNode node in parent.Nodes)
                        if (node.Checked)
                        {
                            DataNode dN = (DataNode)node.Tag;
                            double[] ser = null;

                            if (form1.NormCB.Checked)
                                ser = dN.NormSeries;
                            else
                                ser = dN.Series;

                            foreach (double val in ser)
                                if (val > MaxY) MaxY = val;
                                else if (val < MinY) MinY = val;
                        }

                double MaxX = 0;
                foreach (double val in form1.dataTV.Xaxis)
                    if (val > MaxX) MaxX = val;

                rect = renderChartArea(OldRect, MinY, MaxY, MaxX);

                //load series
                stepX = rect.Width / MaxX;
                stepY = rect.Height / (MaxY - MinY);
                GL.Enable(EnableCap.LineSmooth);
                foreach (TreeNode parent in form1.dataTV.Nodes)
                    foreach (TreeNode node in parent.Nodes)
                        if (node.Checked)
                        {
                            DataNode dN = (DataNode)node.Tag;

                            GL.Begin(PrimitiveType.LineStrip);
                            GL.Color3(form1.dataTV.colors[node.ImageIndex]);

                            double[] XaxisData = form1.dataTV.Xaxis;
                            double[] YaxisData = null;

                            if (form1.NormCB.Checked)
                                YaxisData = dN.NormSeries;
                            else
                                YaxisData = dN.Series;

                            if (XaxisData.Length <= YaxisData.Length)
                            {
                                for (int ind = 0; ind < XaxisData.Length; ind++)
                                    GL.Vertex2(rect.X + XaxisData[ind] * stepX, rect.Y + rect.Height - (YaxisData[ind] - MinY) * stepY);
                            }
                            else
                            {
                                for (int ind = 0; ind < YaxisData.Length; ind++)
                                    GL.Vertex2(rect.X + XaxisData[ind] * stepX, rect.Y + rect.Height - (YaxisData[ind] - MinY) * stepY);
                            }

                            if (XaxisData.Length == 1)
                                GL.Vertex2(rect.X + XaxisData[0] * stepX, rect.Y + rect.Height);

                            GL.End();

                        }

                if (form1.dataTV.SelectedNode != null &&
                    form1.dataTV.SelectedNode.ImageIndex != 0 &&
                    form1.dataTV.SelectedNode.Checked)
                {
                    DataNode dN = (DataNode)form1.dataTV.SelectedNode.Tag;

                    double[] XaxisData = form1.dataTV.Xaxis;
                    double[] YaxisData = null;

                    if (form1.NormCB.Checked)
                        YaxisData = dN.NormSeries;
                    else
                        YaxisData = dN.Series;


                    for (int i = -2; i < 3; i++)
                    {
                        GL.Begin(PrimitiveType.LineStrip);
                        GL.Color3(form1.dataTV.colors[form1.dataTV.SelectedNode.ImageIndex]);

                        if (XaxisData.Length == 1)
                        {
                            GL.Vertex2(rect.X + XaxisData[0] * stepX + i, rect.Y + rect.Height - (YaxisData[0] - MinY) * stepY + i);
                            GL.Vertex2(rect.X + XaxisData[0] * stepX + i, rect.Y + rect.Height);
                        }
                        else if (XaxisData.Length <= YaxisData.Length)
                        {
                            for (int ind = 0; ind < XaxisData.Length; ind++)
                                GL.Vertex2(rect.X + XaxisData[ind] * stepX, rect.Y + rect.Height - (YaxisData[ind] - MinY) * stepY + i);
                        }
                        else
                        {
                            for (int ind = 0; ind < YaxisData.Length; ind++)
                                GL.Vertex2(rect.X + XaxisData[ind] * stepX, rect.Y + rect.Height - (YaxisData[ind] - MinY) * stepY + i);
                        }
                        /*
                            for (int ind = 0; ind < XaxisData.Length; ind++)
                                GL.Vertex2(rect.X + XaxisData[ind] * stepX, rect.Y + rect.Height - (YaxisData[ind] - MinY) * stepY + i);
                        */
                        GL.End();
                    }


                }
                GL.Disable(EnableCap.LineSmooth);
            }
            private RectangleF renderChartArea(Rectangle rect, double minY, double maxY, double maxX)
            {
                float W = 13f;
                float H = 15f;

                W *= 3;
                H *= 2;

                if (W > (float)rect.Width / 3 || H > (float)rect.Height / 3) return rect;

                GL.Begin(PrimitiveType.LineStrip);
                GL.Color3(Color.Black);

                GL.Vertex2(rect.X + W, rect.Y + H);
                GL.Vertex2(rect.X + W, rect.Y + rect.Height - H - H * 0.5);
                GL.Vertex2(rect.X + rect.Width - H, rect.Y + rect.Height - H - H * 0.5);

                GL.End();

                RectangleF microRect = new RectangleF(rect.X + W, rect.Y + H, rect.Width - W - H, rect.Height - 2 * H - 0.5f * H);

                double stepX = microRect.Width / (40);
                double stepY = microRect.Height / (30);

                double valX = maxX / stepX;
                double valY = (maxY - minY) / stepY;

                for (double x = microRect.X, i = 0; x <= microRect.X + microRect.Width; x += microRect.Width / stepX, i += valX)
                {
                    GL.Begin(PrimitiveType.Lines);
                    GL.Color3(Color.Black);

                    GL.Vertex2(x, rect.Y + rect.Height - H - 0.5f * H);
                    GL.Vertex2(x, rect.Y + rect.Height - 0.5f * H - H * 0.8);

                    GL.End();

                    double i1 = Math.Round(i, 1);

                    if ((i1).ToString().Length > 6)
                        BitmapFromString((i1).ToString("0.0E0"), new PointF((float)x, (rect.Y + rect.Height - 0.5f * H - H / 2)));
                    else
                        BitmapFromString((i1).ToString(), new PointF((float)x, (rect.Y + rect.Height - 0.5f * H - H / 2)));
                }

                for (double y = microRect.Y + microRect.Height, i = minY; y >= microRect.Y; y -= microRect.Height / stepY, i += valY)
                {
                    GL.Begin(PrimitiveType.Lines);
                    GL.Color3(Color.Black);

                    GL.Vertex2(rect.X + W, y);
                    GL.Vertex2(rect.X + W - H * 0.2, y);

                    GL.End();

                    double i1 = Math.Round(i, 1);

                    if ((i1).ToString().Length > 5)
                        BitmapFromString((i1).ToString("0.0E0"), new PointF(rect.X + W / 2, (float)y));
                    else
                        BitmapFromString((i1).ToString(), new PointF(rect.X + W / 2, (float)y));
                }

                if (form1.dataTV.YaxisTitle.Length < 6)
                    BitmapFromString(form1.dataTV.YaxisTitle, new PointF(rect.X + W / 2 + 1, rect.Y + H / 2), true);
                else
                    BitmapFromString(form1.dataTV.YaxisTitle.Substring(0, 6), new PointF(rect.X + W / 2 + 1, rect.Y + H / 2), true);

                BitmapFromString(form1.dataTV.XaxisTitle, new PointF(rect.X + rect.Width / 2, (rect.Y + rect.Height - H / 2)), true);

                return microRect;

            }

            private void BitmapFromString(string str, PointF p, bool title = false)
            {
                Font font = new Font("Times New Roman", 9, FontStyle.Regular);
                if (title) font = new Font("Times New Roman", 9, FontStyle.Bold);

                Bitmap bmp = new Bitmap(TextRenderer.MeasureText(str, font).Width,
                    TextRenderer.MeasureText(str, font).Height,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                RectangleF rect = new Rectangle(0, 0,
                    TextRenderer.MeasureText(str, font).Width,
                    TextRenderer.MeasureText(str, font).Height);

                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.FillRectangle(Brushes.White, rect);
                    g.DrawString(str, font, Brushes.Black, rect);
                    g.Flush();
                }

                int ID = LoadTexture(bmp);

                GL.Enable(EnableCap.Texture2D);

                GL.BindTexture(TextureTarget.Texture2D, ID);

                rect = new RectangleF(p.X - rect.Width / 2, p.Y - rect.Height / 2,
                    p.X + rect.Width / 2, p.Y + rect.Height / 2);

                GL.Begin(PrimitiveType.Quads);

                GL.Color3(Color.White);

                GL.TexCoord2(0, 0);
                GL.Vertex2(rect.X, rect.Y);

                GL.TexCoord2(0, 1);
                GL.Vertex2(rect.X, rect.Height);

                GL.TexCoord2(1, 1);
                GL.Vertex2(rect.Width, rect.Height);

                GL.TexCoord2(1, 0);
                GL.Vertex2(rect.Width, rect.Y);

                GL.End();

                GL.Disable(EnableCap.Texture2D);
            }

            private void ReserveTextureID()
            {
                id = GL.GenTexture();
            }
            private int LoadTexture(Bitmap bmp)
            {
                //Load texture from file
                Bitmap texture_source = bmp;
                
                //Link empty texture to texture2d
                GL.BindTexture(TextureTarget.Texture2D, id);

                //Lock pixel data to memory and prepare for pass through
                BitmapData bitmap_data = texture_source.LockBits(
                    new Rectangle(0, 0, texture_source.Width,
                    texture_source.Height), ImageLockMode.ReadOnly,
                    System.Drawing.Imaging.PixelFormat.Format32bppRgb);

                //Tell gl to write the data from are bitmap image/data to the bound texture
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, texture_source.Width, texture_source.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bitmap_data.Scan0);
                //Release from memory
                texture_source.UnlockBits(bitmap_data);
                //SetUp parametars
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Linear);

                return id;
            }
            #endregion Rendering
        }
        class PropertiesPanel_Item
        {
            private Panel PropertiesPanel;

            public Panel Panel = new Panel();
            public Panel Body = new Panel();
            public Label Name = new Label();
            public bool Resizable = false;

            private Panel NamePanel = new Panel();
            public Panel ResizePanel = new Panel();
            private Color TitleBackColor;

            private ToolTip TurnOnToolTip = new ToolTip();
            public int Height = 200;

            private bool resizing = false;
            private int oldY = 0;
            public void Initialize(Panel PropertiesPanel)
            {
                this.PropertiesPanel = PropertiesPanel;

                Panel.Dock = DockStyle.Top;
                Panel.Resize += new EventHandler(Panel_HeightChange);

                NamePanel.Dock = DockStyle.Top;
                NamePanel.Height = 21;
                Panel.Controls.Add(NamePanel);
                NamePanel.MouseHover += new EventHandler(Control_MouseOver);
                NamePanel.Click += new EventHandler(Control_Click);
                NamePanel.MouseEnter += new EventHandler(Title_HighLight);
                NamePanel.MouseLeave += new EventHandler(Title_Normal);

                Name.Width = 150;
                Name.Location = new System.Drawing.Point(10, 5);
                NamePanel.Controls.Add(Name);
                Name.MouseHover += new EventHandler(Control_MouseOver);
                Name.Click += new EventHandler(Control_Click);
                Name.MouseEnter += new EventHandler(Title_HighLight);
                Name.MouseLeave += new EventHandler(Title_Normal);

                Panel Resize1 = new Panel();
                Resize1.Tag = PropertiesPanel;
                Resize1.Dock = DockStyle.Bottom;
                Resize1.Height = 5;
                Panel.Controls.Add(Resize1);
                Resize1.MouseDown += new MouseEventHandler(Resize1_MouseDown);
                Resize1.MouseUp += new MouseEventHandler(Resize1_MouseUp);
                Resize1.MouseMove += new MouseEventHandler(Resize1_MouseMove);

                Body.Dock = DockStyle.Fill;
                Panel.Controls.Add(Body);
                Body.BringToFront();

                ResizePanel.Anchor = (AnchorStyles.Top | AnchorStyles.Bottom);
                ResizePanel.Visible = false;
                ResizePanel.BackColor = Color.FromArgb(100, 10, 10, 10);
                ResizePanel.Width = 5;
                PropertiesPanel.Controls.Add(ResizePanel);

                //reorder panels

                ResizePanel.BringToFront();
            }
            #region Title Panel Hendlers

            private void Control_MouseOver(object sender, EventArgs e)
            {
                var ctr = (Control)sender;
                TurnOnToolTip.SetToolTip(ctr, "Show/Hide " + Name.Text);
            }
            //Add handler for resize
            private void Control_Click(object sender, EventArgs e)
            {
                Panel p = PropertiesPanel;
                if (Panel.Height != 26)
                {
                    Panel.Height = 26;
                }
                else
                {
                    Panel.Height = Height;
                }
                //PropertiesPanel.Refresh();
                p.Refresh();
            }
            private void Title_HighLight(object sender, EventArgs e)
            {
                Panel ctr = NamePanel;
                int R = ctr.BackColor.R;
                int G = ctr.BackColor.G;
                int B = ctr.BackColor.B;
                if (R + 40 <= 255) { R += 40; } else { R = 255; }
                if (G + 40 <= 255) { G += 40; } else { G = 255; }
                if (B + 40 <= 255) { B += 40; } else { B = 255; }
                ctr.BackColor = Color.FromArgb(255, R, G, B);
                Name.BackColor = Color.FromArgb(255, R, G, B);
            }
            private void Title_Normal(object sender, EventArgs e)
            {
                NamePanel.BackColor = TitleBackColor;
                Name.BackColor = TitleBackColor;
            }
            #endregion

            #region Color options
            public Color BackColor(Color Color)
            {
                Panel.BackColor = Color;
                return Color;
            }
            public Color ForeColor(Color Color)
            {
                Panel.ForeColor = Color;
                Name.ForeColor = Color;
                return Color;
            }
            public Color TitleColor(Color Color)
            {
                Name.BackColor = Color;
                NamePanel.BackColor = Color;
                TitleBackColor = Color;
                return Color;
            }
            #endregion

            #region Resize Panel

            private void Panel_HeightChange(object sender, EventArgs e)
            {
                if (Panel.Height != 26)
                {
                    Height = Panel.Height;
                }
            }
            private void Resize1_MouseDown(object sender, MouseEventArgs e)
            {
                if (Resizable == false) { return; }
                if (Panel.Height > 26)
                {
                    Panel pnl = sender as Panel;
                    Panel PropertiesPanel = pnl.Tag as Panel;
                    ResizePanel.Location = new Point(0, pnl.Location.Y + Panel.Location.Y);
                    ResizePanel.Width = PropertiesPanel.Width;
                    ResizePanel.Height = 5;
                    ResizePanel.BringToFront();
                    ResizePanel.Visible = true;
                    resizing = true;
                    oldY = e.Y;
                }
            }
            private void Resize1_MouseUp(object sender, MouseEventArgs e)
            {
                if (Resizable == false) { return; }
                if (resizing == true)
                {
                    Panel pnl = sender as Panel;
                    Panel PropertiesPanel = pnl.Tag as Panel;

                    Panel.Height = ResizePanel.Location.Y
                        - (Panel.Location.Y);
                    if (Panel.Height < 38) { Panel.Height = 40; }
                    Height = Panel.Height;
                    ResizePanel.Visible = false;
                    resizing = false;
                    pnl.Cursor = Cursors.Default;
                }
            }
            private void Resize1_MouseMove(object sender, MouseEventArgs e)
            {
                if (Resizable == false) { return; }
                Panel pnl = sender as Panel;
                if (resizing == true)
                {
                    int razlika = ResizePanel.Location.Y - (oldY - e.Y);
                    Panel PropertiesPanel = pnl.Tag as Panel;
                    if (razlika > Panel.Location.Y + 59)
                    {
                        oldY = e.Y;
                        ResizePanel.Location = new System.Drawing.Point(0, razlika);
                        ResizePanel.BringToFront();
                    }
                    ResizePanel.Visible = true;
                }
                else if (Panel.Height != 26)
                {
                    pnl.Cursor = Cursors.SizeNS;
                    ResizePanel.Visible = false;
                }
                else
                {
                    pnl.Cursor = Cursors.Default;
                    ResizePanel.Visible = false;
                }
            }
            #endregion
        }
    }
}
