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

namespace Cell_Tool_3
{
    class AutoApplySettingsClass:Form
    {
        private ImageAnalyser IA;
        private Button startBtn;
        public ComboBox LibTB;

        //Controls
        private CheckBox TrackingCB = new CheckBox();
        private CheckBox SpotDetCB = new CheckBox();
        private CheckBox SegmentationCB = new CheckBox();
        private CheckBox FiltersCB = new CheckBox();
        private CheckBox ChartAxisCB = new CheckBox();
        private CheckBox TimeStepCB = new CheckBox();
        private TextBox titleTB = new TextBox();
        public CheckBox ApplyToNewCheckB = new CheckBox();

        //Protocols string
        private bool Loading = false;
        public List<string> protocols = new List<string>();

        public AutoApplySettingsClass(Button startBtn, ComboBox LibTB, ImageAnalyser IA)
        {
            this.startBtn = startBtn;
            this.LibTB = LibTB;
            this.IA = IA;

            Form dialog = this;
            dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
            dialog.Text = "Auto Settings";
            dialog.StartPosition = FormStartPosition.CenterScreen;
            dialog.WindowState = FormWindowState.Normal;
            dialog.BackColor = IA.FileBrowser.BackGround2Color1;
            dialog.ForeColor = IA.FileBrowser.ShriftColor1;
            dialog.MinimizeBox = false;
            dialog.MaximizeBox = false;
            dialog.Width = 250;
            dialog.Height = 335;

            #region Add controls
            this.SuspendLayout();

            //ApplyToNewImage checkbox
            
            ApplyToNewCheckB.Text = "Apply settings when new image is opened";
            ApplyToNewCheckB.Location = new System.Drawing.Point(5,5);
            ApplyToNewCheckB.Width = 250;
            this.Controls.Add(ApplyToNewCheckB);
            ApplyToNewCheckB.CheckedChanged += ApplyToNewCheckB_Checked;
            //Title
            Label titleLabel = new Label();
            titleLabel.Text = "Name:";
            titleLabel.Width = 50;
            titleLabel.Location = new System.Drawing.Point(5, 30);
            this.Controls.Add(titleLabel);

            //Title
            
            titleTB.Text = "";
            titleTB.Width = 165;
            titleTB.Location = new System.Drawing.Point(55, 28);
            this.Controls.Add(titleTB);

            //GroupBox
            GroupBox GB = new GroupBox();
            GB.Text = "Apply the following setiings:";
            GB.ForeColor = System.Drawing.Color.White;
            GB.Dock = DockStyle.Bottom;
            GB.Height = 200;
            this.Controls.Add(GB);
            

            CheckBox CB = FiltersCB;
            {
                CB.Text = "Filters";
                CB.Location = new System.Drawing.Point(30,20);
                CB.ForeColor = System.Drawing.Color.White;
                GB.Controls.Add(CB);
            }
            
            CB = SegmentationCB;
            {
                CB.Text = "Segmentation";
                CB.Location = new System.Drawing.Point(30, 50);
                CB.ForeColor = System.Drawing.Color.White;
                GB.Controls.Add(CB);
            }

            
            CB = SpotDetCB;
            {
                CB.Text = "Spot detector";
                CB.Location = new System.Drawing.Point(30, 80);
                CB.ForeColor = System.Drawing.Color.White;
                GB.Controls.Add(CB);
            }

            CB = TrackingCB;
            {
                CB.Text = "Tracking";
                CB.Location = new System.Drawing.Point(30, 110);
                CB.ForeColor = System.Drawing.Color.White;
                GB.Controls.Add(CB);
            }

            CB = ChartAxisCB;
            {
                CB.Text = "Chart Axis";
                CB.Location = new System.Drawing.Point(30, 140);
                CB.ForeColor = System.Drawing.Color.White;
                GB.Controls.Add(CB);
            }

            
            CB = TimeStepCB;
            {
                CB.Text = "Time Intervals";
                CB.Location = new System.Drawing.Point(30, 170);
                CB.ForeColor = System.Drawing.Color.White;
                GB.Controls.Add(CB);
            }

            //EndPanel
            Panel endP = new Panel();
            endP.Dock = DockStyle.Bottom;
            endP.Height = 40;
            this.Controls.Add(endP);
            
            Button EditBtn = new Button();
            Button btn = EditBtn;
            {
                btn.Text = "Save";
                btn.ForeColor = System.Drawing.Color.Black;
                btn.BackColor = System.Drawing.SystemColors.ButtonFace;
                btn.Width = 55;
                btn.Location = new System.Drawing.Point(10, 5);
                endP.Controls.Add(btn);
                btn.Click += EditBtn_Click;
            }

            Button SaveBtn = new Button();
            btn = SaveBtn;
            {
                btn.Text = "Save As";
                btn.ForeColor = System.Drawing.Color.Black;
                btn.BackColor = System.Drawing.SystemColors.ButtonFace;
                btn.Width = 55;
                btn.Location = new System.Drawing.Point(65, 5);
                endP.Controls.Add(btn);
                btn.Click += SaveBtn_Click;
            }

            Button DeleteBtn = new Button();
            btn = DeleteBtn;
            {
                btn.Text = "Delete";
                btn.ForeColor = System.Drawing.Color.Black;
                btn.BackColor = System.Drawing.SystemColors.ButtonFace;
                btn.Width = 55;
                btn.Location = new System.Drawing.Point(120, 5);
                endP.Controls.Add(btn);
                btn.Click += deleteBtn_Click;
            }

            Button CancelBtn = new Button();
            btn = CancelBtn;
            {
                btn.Text = "Cancel";
                btn.ForeColor = System.Drawing.Color.Black;
                btn.BackColor = System.Drawing.SystemColors.ButtonFace;
                btn.Width = 55;
                btn.Location = new System.Drawing.Point(175, 5);
                endP.Controls.Add(btn);
                btn.Click += cancelBtn_Click;
            }

            this.ResumeLayout();
            #endregion Add controls

            startBtn.Click += startBtn_Click;
            this.FormClosing += Form_Clossing;
            LibTB.SelectedIndexChanged += LibTB_SelectedIndexChanged;
        }
        public void LibTB_SelectedIndexChanged(object sender, EventArgs e)
        {
            TifFileInfo fi = findFI();
            if (fi == null) return;

            if (!Loading && LibTB.SelectedIndex != 0 && MessageBox.Show(
                "Do you want to load the following protocol:\n" +
                    IA.Segmentation.AutoSetUp.LibTB.Text, "", 
                MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                ApplyCT3Tags(protocols[LibTB.SelectedIndex].Split(new string[] { ";\n" }, StringSplitOptions.None), fi);
            }
        }
        public void LibBtn_Click(object sender, EventArgs e)
        {
            TifFileInfo fi = findFI();
            if (fi == null) return;

            if (!Loading && LibTB.SelectedIndex != 0 && MessageBox.Show(
                "Do you want to load the following protocol:\n" +
                    IA.Segmentation.AutoSetUp.LibTB.Text, "",
                MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                ApplyCT3Tags(protocols[LibTB.SelectedIndex].Split(new string[] { ";\n" }, StringSplitOptions.None), fi);
            }
        }
        public void LoadSettings()
        {
            protocols = Properties.Settings.Default.ProtocolSettingsList[IA.TabPages.ActiveAccountIndex].Split(new string[] { "||" }, StringSplitOptions.None).ToList();
            LoadProtocolsToComboBox();
            LibTB.SelectedIndex = 0;
        }
        private void LoadProtocolsToComboBox()
        {
            Loading = true;
            LibTB.Items.Clear();
            LibTB.Items.Add("None");

            for (int i = 1; i < protocols.Count; i++)
            {
                LibTB.Items.Add(protocols[i].Substring(0, protocols[i].IndexOf("\t")));
            }

            Loading = false;
        }
        private void ApplyToNewCheckB_Checked(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.AutoProtocolSettings[IA.TabPages.ActiveAccountIndex] !=
                ApplyToNewCheckB.Checked.ToString())
            {
                Properties.Settings.Default.AutoProtocolSettings[IA.TabPages.ActiveAccountIndex] =
                    ApplyToNewCheckB.Checked.ToString();

					Security.SaveSettings(Properties.Settings.Default);
            }
        }
        private void startBtn_Click(object sender, EventArgs e)
        {

            TifFileInfo fi = findFI();
            if (fi == null) return;

            if(LibTB.SelectedIndex == 0)
            {
                titleTB.Text = "";
                FiltersCB.Checked = true;
                SegmentationCB.Checked = true;
                SpotDetCB.Checked = true;
                TrackingCB.Checked = true;
                ChartAxisCB.Checked = true;
            }
            else if(LibTB.SelectedIndex < protocols.Count)
            {
                LoadSettings(protocols[LibTB.SelectedIndex].Split(new string[] { ";\n" }, StringSplitOptions.None), fi);
            }

            // Linux change
            IA.FileBrowser.StatusLabel.Text = "Dialog open";
            this.ShowDialog();
            IA.FileBrowser.StatusLabel.Text = "Ready";
        }
        private void deleteBtn_Click(object sender, EventArgs e)
        {
            this.Hide();

            if(LibTB.SelectedIndex > 0 && LibTB.SelectedIndex < protocols.Count)
            {
                protocols.RemoveAt(LibTB.SelectedIndex);
            }

            LoadProtocolsToComboBox();

            Loading = true;
            LibTB.SelectedIndex = 0;
            Loading = false;

            SaveToHardDisck();
        }
        private void cancelBtn_Click(object sender, EventArgs e)
        {
            this.Hide();
        }
        private void Form_Clossing(object sender, FormClosingEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
        }

        private void SaveBtn_Click(object sender, EventArgs e)
        {
            this.Hide();
            TifFileInfo fi = findFI();
            if (fi == null) return;

            string val = calculateCTTagValue(fi,IA);

            protocols.Add(val);
            LoadProtocolsToComboBox();

            Loading = true;
            LibTB.SelectedIndex = protocols.Count - 1;
            Loading = false;

            SaveToHardDisck();
        }

        private void EditBtn_Click(object sender, EventArgs e)
        {
            this.Hide();
            TifFileInfo fi = findFI();
            if (fi == null) return;
            if (LibTB.SelectedIndex > 0 && LibTB.SelectedIndex < protocols.Count)
            {
                int ind = LibTB.SelectedIndex;
                string val = calculateCTTagValue(fi, IA);
                protocols[ind] = val;
                LoadProtocolsToComboBox();
                Loading = true;
                LibTB.SelectedIndex = ind;
                Loading = false;
                SaveToHardDisck();
            }
            else
            {
                SaveBtn_Click(sender, e);
            }
        }
        private void SaveToHardDisck()
        {
            string val = string.Join("||", protocols);
            Properties.Settings.Default.ProtocolSettingsList[IA.TabPages.ActiveAccountIndex] = val;
			Security.SaveSettings(Properties.Settings.Default);
        }
        private TifFileInfo findFI()
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

        private string calculateCTTagValue(TifFileInfo fi, ImageAnalyser IA)
        {
            List<string> vals = new List<string>();

            string val = titleTB.Text + "\t" +
                FiltersCB.Checked.ToString() + "\t" +
                SegmentationCB.Checked.ToString() + "\t" +
                SpotDetCB.Checked.ToString() + "\t" +
                TrackingCB.Checked.ToString() + "\t" +
                fi.sizeC + "\t" + 
                ChartAxisCB.Checked.ToString() + "\t" +
                TimeStepCB.Checked.ToString();

            vals.Add(val);

            if (FiltersCB.Checked)
            {
                vals.Add("tracking_MaxSize->" + FileEncoder.TagValueToString(fi.tracking_MaxSize));
                vals.Add("tracking_MinSize->" + FileEncoder.TagValueToString(fi.tracking_MinSize));
                vals.Add("tracking_Speed->" + FileEncoder.TagValueToString(fi.tracking_Speed));
            }

            if (SegmentationCB.Checked)
            {
                vals.Add("SegmentationProtocol->" + FileEncoder.TagValueToString(fi.SegmentationProtocol));
                vals.Add("SegmentationCBoxIndex->" + FileEncoder.TagValueToString(fi.SegmentationCBoxIndex));
                vals.Add("thresholdsCBoxIndex->" + FileEncoder.TagValueToString(fi.thresholdsCBoxIndex));
                vals.Add("RefSpotColor->" + FileEncoder.TagValueToString(fi.RefSpotColor));
                vals.Add("sumHistogramChecked->" + FileEncoder.TagValueToString(fi.sumHistogramChecked));
                vals.Add("thresholdColors->" + FileEncoder.TagValueToString(fi.thresholdColors));
                vals.Add("RefThresholdColors->" + FileEncoder.TagValueToString(fi.RefThresholdColors));
                vals.Add("thresholdValues->" + FileEncoder.TagValueToString(fi.thresholdValues));
                vals.Add("thresholds->" + FileEncoder.TagValueToString(fi.thresholds));
            }

            if (SpotDetCB.Checked)
            {
                vals.Add("SelectedSpotThresh->" + FileEncoder.TagValueToString(fi.SelectedSpotThresh));
                vals.Add("typeSpotThresh->" + FileEncoder.TagValueToString(fi.typeSpotThresh));
                vals.Add("SpotThresh->" + FileEncoder.TagValueToString(fi.SpotThresh));
                vals.Add("spotSensitivity->" + FileEncoder.TagValueToString(fi.spotSensitivity));
                vals.Add("SpotColor->" + FileEncoder.TagValueToString(fi.SpotColor));
                vals.Add("SpotTailType->" + string.Join("\t", fi.SpotTailType));
            }

            if (FiltersCB.Checked)
            {
                if (fi.newFilterHistory != null)
                    vals.Add("newFilters->" + FileEncoder.TagValueToString(fi.newFilterHistory));

                //vals.Add("FilterHistory->" + FileEncoder.TagValueToString(fi.FilterHistory.ToArray()));

                // if (fi.watershedList.Count!=0)
                //vals.Add("watershed->" + string.Join("\n", fi.watershedList));
            }

            if (ChartAxisCB.Checked)
            {
                vals.Add("xAxisTB->" + fi.xAxisTB.ToString());
                vals.Add("yAxisTB->" + fi.yAxisTB.ToString());
            }

            if (TimeStepCB.Checked)
            {
                vals.Add("TimeSteps->" + FileEncoder.TagValueToString(fi.TimeSteps));
            }

            return string.Join(";\n", vals);
        }

        public void ApplyCT3Tags(string[] BigVals, TifFileInfo fi)
        {
            int ind = LibTB.SelectedIndex;
            LibTB.SuspendLayout();
            string[] newBigVals = new string[BigVals.Length - 1];
            Array.Copy(BigVals,1, newBigVals,0, newBigVals.Length);

            string[] vals = BigVals[0].Split(new string[] { "\t"}, StringSplitOptions.None);

            titleTB.Text = vals[0];
            FiltersCB.Checked = bool.Parse(vals[1]);
            SegmentationCB.Checked = bool.Parse(vals[2]);
            SpotDetCB.Checked = bool.Parse(vals[3]);
            TrackingCB.Checked = bool.Parse(vals[4]);

            int sizeC = int.Parse(vals[5]);

            if(vals.Length > 6)
            {
                ChartAxisCB.Checked = bool.Parse(vals[6]);
            }
            else
            {
                ChartAxisCB.Checked = false;
            }

            if (vals.Length > 7)
            {
                TimeStepCB.Checked = bool.Parse(vals[7]);
            }
            else
            {
                TimeStepCB.Checked = false;
            }

            if (sizeC == fi.sizeC)
            {
                fi.available = false;
                fi.watershedList.Clear();
                fi.tempWatershedList.Clear();
                fi.FilterHistory.Clear();
                fi.image16bitFilter = fi.image16bit;
                fi.image8bitFilter = fi.image8bit;
                fi.newFilterHistory = null;
                fi.tempNewFilterHistory = null;
                //Apply settings
                int[] filters = IA.TabPages.myFileDecoder.ApplyCT3Tags(newBigVals, fi);
                //background worker
                              
                var bgw = new BackgroundWorker();
                bgw.WorkerReportsProgress = true;

                //Add event for projection here
                bgw.DoWork += new DoWorkEventHandler(delegate (Object o, DoWorkEventArgs a)
                {
                    if (fi.tempNewFilterHistory == null || fi.tempNewFilterHistory.Length != fi.sizeC)
                    {
                        fi.isBinary = new bool[fi.sizeC];
                        fi.tempNewFilterHistory = new List<string>[fi.sizeC];
                        for (int i = 0; i < fi.sizeC; i++)
                        {
                            fi.tempNewFilterHistory[i] = new List<string>();
                            fi.isBinary[i] = false;
                        }
                    }

                    if (filters != null)
                        foreach (int i in filters)
                            for (int C = 0; C < fi.sizeC; C++)
                            {
                                string str = IA.Segmentation.MyFilters.DecodeOldFilters(C, i);
                                fi.tempNewFilterHistory[C].Add(str);
                            }
                    
                    if (fi.tempWatershedList != null)
                        foreach (string val in fi.tempWatershedList)
                        {
                            int C = int.Parse(val.Split(new string[] { "\t" }, StringSplitOptions.None)[0]);
                            fi.tempNewFilterHistory[C].Add("wshed\t" + val);
                        }
                    fi.tempWatershedList.Clear();
                    //new filters
                    if (fi.tempNewFilterHistory != null)
                    {
                        try
                        {
                            foreach (List<string> commands in fi.tempNewFilterHistory)
                                if (commands != null)
                                    foreach (string command in commands)
                                    {
                                        IA.Segmentation.MyFilters.FilterFromString(command, fi);
                                    }
                            fi.newFilterHistory = fi.tempNewFilterHistory;

                            for (int i = 0; i < fi.sizeC; i++)
                                if (fi.newFilterHistory[i] == null)
                                    fi.newFilterHistory[i] = new List<string>();

                            fi.tempNewFilterHistory = null;
                        }
                        catch { }
                    }

                    ((BackgroundWorker)o).ReportProgress(0);
                });

                bgw.ProgressChanged += new ProgressChangedEventHandler(delegate (Object o, ProgressChangedEventArgs a)
                {
                    
                    IA.ReloadImages();
                    fi.available = true;
                    IA.FileBrowser.StatusLabel.Text = "Ready";
                    
                    Loading = true;
                    LibTB.SelectedIndex = ind;
                    LibTB.ResumeLayout();
                    Loading = false;
                });
                //Start background worker
                IA.FileBrowser.StatusLabel.Text = "Loading settings...";
                //start bgw
                bgw.RunWorkerAsync();
            }
            else
            {
                MessageBox.Show("Selected settings can be applied only on image with " + sizeC + " chanels!");
            }
           
        }
        private void LoadSettings(string[] BigVals, TifFileInfo fi)
        {
            string[] vals = BigVals[0].Split(new string[] { "\t" }, StringSplitOptions.None);

            titleTB.Text = vals[0];
            FiltersCB.Checked = bool.Parse(vals[1]);
            SegmentationCB.Checked = bool.Parse(vals[2]);
            SpotDetCB.Checked = bool.Parse(vals[3]);
            TrackingCB.Checked = bool.Parse(vals[4]);

            if (vals.Length > 6)
            {
                ChartAxisCB.Checked = bool.Parse(vals[6]);
            }
            else
            {
                ChartAxisCB.Checked = false;
            }

            if (vals.Length > 7)
            {
                TimeStepCB.Checked = bool.Parse(vals[7]);
            }
            else
            {
                TimeStepCB.Checked = false;
            }
        }
    }
}
