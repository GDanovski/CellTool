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
using System.Windows.Forms;
using OpenTK;
using System.Drawing;
using System.Threading;

namespace Cell_Tool_3
{
    class ImageAnalyser
    {
        public bool DeleteEmptyEnabled = true;
        private bool SpotDetectorEnabled = false;
        public Properties.Settings settings = Properties.Settings.Default;
        public ChangeValueControl Input = new ChangeValueControl();
        public TabPageControl TabPages;
        public CTFileBrowser FileBrowser;
        public ToolStripButton UnDoBtn;
        public ToolStripButton ReDoBtn;
        public bool redo = false;
        public bool undo = false;
        public bool delHist = false;
        public string oldComand = "";

        //PlugIns
        public PlugInEngine PlugIns;
        public CTChart chart;
        public BrightnessAndContrast BandC = new BrightnessAndContrast();
        public MetadataProp Meta = new MetadataProp();
        public Segmentation Segmentation;
        public TrackSpots Tracking;
        public RoiManager RoiMan;
        public ImageDrawer IDrawer = new ImageDrawer();
        public OpenTK.GLControl GLControl1 = new GLControl();
        public Panel GLControl1_VerticalPanel = new Panel();
        public Panel GLControl1_HorizontalPanel = new Panel();
        public Panel GLControl1_TraserPanel = new Panel();


        public ToolStripComboBox zoomValue = null;
              
        public void zoomValue_Change(object sender,EventArgs e)
        {
            /*ZoomValue.Items.AddRange(new string[]
                       {"6,25 %","12,5 %","25 %","50 %",
                        "100 %", "200 %", "300 %", "400 %", "500 %",
                        "600 %", "700 %", "800 %", "900 %", "1000 %" });*/
            TifFileInfo fi = null;
            try
            {
                fi = TabPages.TabCollections[TabPages.SelectedIndex].tifFI;
            }
            catch { return; }
            //Calculate zoom
            if(fi == null) { return; }
            if (RoiMan.DrawNewRoiMode == true) return;

            double zoom = Convert.ToDouble(zoomValue.Text.Substring(0, zoomValue.Text.Length - 2)) / 100;
            if (fi.zoom != zoom)
            {
                fi.zoom = zoom;
                //reload image
                ReloadImages();
               //MarkAsNotSaved();
            }
        }
        private void zoomValue_Set(double val)
        {
            zoomValue.SelectedItem = (val * 100).ToString() + " %";
        }
        public void Initialize(ToolStripButton UnDoBtn1, ToolStripButton ReDoBtn1)
        {
            Input.Changed += Input_TextChange;
            UnDoBtn = UnDoBtn1;
            ReDoBtn = ReDoBtn1;
            UnDoBtn.Enabled = false;
            ReDoBtn.Enabled = false;
            UnDoBtn.Click += new EventHandler(undo_Click);
            ReDoBtn.Click += new EventHandler(redo_Click);

            zoomValue.SelectedIndexChanged += new EventHandler(zoomValue_Change);
            IDrawer.Initialize(GLControl1);

            SpotDetectorEnabled=System.IO.File.Exists(Application.StartupPath + "/PlugIns/SpotDetector.txt");
            DeleteEmptyEnabled = !System.IO.File.Exists(Application.StartupPath + "/PlugIns/DeleteEmptyResults.txt");



        }
        
        private void Input_TextChange(object sender, ChangeValueEventArgs e)
        {
            if (e.Value == "") return; 
            if (e.Value == oldComand) return;

            oldComand = e.Value;
            string cmd = e.Value;
            //
            delHist = true;
            TabPages.TabCollections[TabPages.SelectedIndex].tifFI.delHist = true;
            SwitchCaseEventOperator(cmd);
            UpdateUndoBtns();
        }
        public void UpdateUndoBtns()
        {
            ReDoBtn.Enabled = false;
            UnDoBtn.Enabled = false;
            if (TabPages.TabCollections[TabPages.SelectedIndex].tifFI != null)
                if (TabPages.TabCollections[TabPages.SelectedIndex].tifFI.History != null)
                {
                    if (TabPages.TabCollections[TabPages.SelectedIndex].tifFI.History.Count > 0)
                    {
                        if ((TabPages.TabCollections[TabPages.SelectedIndex].tifFI.HistoryPlace >= -1 & undo == false) |
                        (TabPages.TabCollections[TabPages.SelectedIndex].tifFI.HistoryPlace >= 1))
                        {
                            UnDoBtn.Enabled = true;
                        }

                        if (TabPages.TabCollections[TabPages.SelectedIndex].tifFI.HistoryPlace <
                            TabPages.TabCollections[TabPages.SelectedIndex].tifFI.History.Count - 1 &
                            TabPages.TabCollections[TabPages.SelectedIndex].tifFI.HistoryPlace != -1)
                        {
                            ReDoBtn.Enabled = true;
                        }
                    }
                }

        }
        public void DeleteFromHistory()
        {
            //undo - redo restore
            if (TabPages.TabCollections[TabPages.SelectedIndex].tifFI == null) return;

            if ((redo == true | undo == true) & delHist == true)
            {
                if (undo == true) { TabPages.TabCollections[TabPages.SelectedIndex].tifFI.HistoryPlace -= 2; }
                while (TabPages.TabCollections[TabPages.SelectedIndex].tifFI.HistoryPlace + 2 <
                       TabPages.TabCollections[TabPages.SelectedIndex].tifFI.History.Count - 1)
                {
                    TabPages.TabCollections[TabPages.SelectedIndex].tifFI.History.RemoveAt(
                        TabPages.TabCollections[TabPages.SelectedIndex].tifFI.History.Count - 1);
                    TabPages.TabCollections[TabPages.SelectedIndex].tifFI.History.RemoveAt(
                       TabPages.TabCollections[TabPages.SelectedIndex].tifFI.History.Count - 1);
                }

                TabPages.TabCollections[TabPages.SelectedIndex].tifFI.HistoryPlace = -1;
                undo = false;
                redo = false;
                TabPages.TabCollections[TabPages.SelectedIndex].tifFI.undo = false;
                TabPages.TabCollections[TabPages.SelectedIndex].tifFI.redo = false;
            }
            TifFileInfo fi = TabPages.TabCollections[TabPages.SelectedIndex].tifFI;
            CheckSizeHistory(fi);
        }
      
        public void undo_Click(object sender, EventArgs e)
        {
            TifFileInfo fi = TabPages.TabCollections[TabPages.SelectedIndex].tifFI;
            if (fi == null) return;
            if (!fi.available)
            {
                MessageBox.Show("Image is not avaliable now!\nTry again later.");
                return;
            }

            if (fi.HistoryPlace == -1) { fi.HistoryPlace = fi.History.Count - 2; }

            if (undo == true)
            {
                fi.HistoryPlace -= 2;
            }
            else
            {
                fi.undo = true;
                fi.redo = false;
                undo = true;
                redo = false;
            }
           
            StartFromHistory(fi.HistoryPlace);
        }
        public void redo_Click(object sender, EventArgs e)
        {
            TifFileInfo fi = TabPages.TabCollections[TabPages.SelectedIndex].tifFI;
            if (fi == null) return;
            if (!fi.available)
            {
                MessageBox.Show("Image is not avaliable now!\nTry again later.");
                return;
            }

            if (fi.HistoryPlace == -1)
            { fi.HistoryPlace = fi.History.Count - 2; }
            
            if (redo == true)
            {
                fi.HistoryPlace += 2;
            }
            else
            {
                undo = false;
                redo = true;
                fi.undo = false;
                fi.redo = true;
            }
            StartFromHistory(fi.HistoryPlace + 1);
            if (fi.HistoryPlace == fi.History.Count - 2)
            {
                fi.HistoryPlace = -1;
                undo = false;
                redo = false;
                fi.undo = false;
                fi.redo = false;
            }
        }
     
        private void StartFromHistory(int ind)
        {
            delHist = false;
            TabPages.TabCollections[TabPages.SelectedIndex].tifFI.delHist = false;
            if (ind >= 0 & ind < TabPages.TabCollections[TabPages.SelectedIndex].tifFI.History.Count)
            {
                SwitchCaseEventOperator(TabPages.TabCollections[TabPages.SelectedIndex].tifFI.History[ind]);
            }
            if (ind < 1) { UnDoBtn.Enabled = false; } else { UnDoBtn.Enabled = true; }
            if (ind > TabPages.TabCollections[TabPages.SelectedIndex].tifFI.History.Count - 2)
            { ReDoBtn.Enabled = false; }
            else { ReDoBtn.Enabled = true; }
        }
        
        public void CheckSizeHistory(TifFileInfo fi)
        {
            while (fi.History.Count > 4000)
            {
                fi.History.RemoveAt(0);
                fi.History.RemoveAt(0);
            }
            
        }
        public void MarkAsNotSaved()
        {
            TabPages.TabCollections[TabPages.SelectedIndex].Saved = false;
        }
        public void AddToHistoryOld(string str)
        {
            //find selected image
            TifFileInfo fi = null;
            try
            {
                fi = TabPages.TabCollections[TabPages.SelectedIndex].tifFI;
            }
            catch { return; }
            if (fi == null) { return; }

            fi.delHist = true;
            delHist = true;
            UnDoBtn.Enabled = true;
            DeleteFromHistory();
            fi.History.Add(str);
        }
        public void AddToHistoryNew(string str)
        {
            //find selected image
            TifFileInfo fi = null;
            try
            {
                fi = TabPages.TabCollections[TabPages.SelectedIndex].tifFI;
            }
            catch { return; }
            if (fi == null) { return; }

            fi.History.Add(str);
            
            UpdateUndoBtns();
            MarkAsNotSaved();
            CheckSizeHistory(fi);
        }

        private void OpenFile(string Val)
        {
            FileBrowser.Openlabel.Text = Val;
            FileBrowser.Openlabel.Text = "";
        }
        private void SelectImage(string Val)
        {
            for (int i = 0; i < TabPages.Collections.Count; i++)
            {
                if(TabPages.Collections[i][0].Tag as string == Val)
                {
                    TabPages.inactivate_Tabs();
                    TabPages.selectTab_event(i);
                    if (TabPages.TabCollections[i].tifFI.History.Count > 0)
                    {
                        UnDoBtn.Enabled = true;
                    }
                    else
                    {
                        UnDoBtn.Enabled = false;
                    }

                    if (TabPages.TabCollections[i].tifFI.HistoryPlace == -1)
                    {
                        ReDoBtn.Enabled = false;
                    }
                    else
                    {
                        ReDoBtn.Enabled = true;
                    }
                    
                }
            }
        }
        private void SelectImageIndex(string Val)
        {
            int i = int.Parse(Val);
            if (TabPages.Collections[i][0].Tag as string == Val)
            {
                TabPages.inactivate_Tabs();
                TabPages.selectTab_event(i);
                if (TabPages.TabCollections[i].tifFI.History.Count > 0)
                {
                    UnDoBtn.Enabled = true;
                }
                else
                {
                    UnDoBtn.Enabled = false;
                }
               
                if (TabPages.TabCollections[i].tifFI.HistoryPlace == -1)
                {
                    ReDoBtn.Enabled = false;
                }
                else
                {
                    ReDoBtn.Enabled = true;
                }
                
            }
        } 
        public void EnabletrackBars(bool val)
        {
            TabPages.tTrackBar.TrackBar1.Enabled = val;
            TabPages.zTrackBar.TrackBar1.Enabled = val;
            TabPages.tTrackBar.TextBox1.Enabled = val;
            TabPages.zTrackBar.TextBox1.Enabled = val;
        }
        public void ReloadImages()
        { 
            //TabPages.PropertiesBody.SuspendLayout();
            TabPages.propertiesPanel.SuspendLayout();


            IDrawer.FormImg.Show();

            if (TabPages.Collections.Count < 1) 
            {
                chart.Series.panel.Visible = false;
                chart.Properties.panel.Visible = false;
                TabPages.tTrackBar.Panel.Visible = false;
                TabPages.zTrackBar.Panel.Visible = false;
                BandC.panel.Visible = false;
                Meta.panel.Visible = false;
               // Segmentation.LibPanel.Visible = false;
                Segmentation.DataPanel.Visible = false;
                Segmentation.HistogramPanel.Visible = false;
                Segmentation.tresholdsPanel.Visible = false;
                Segmentation.SpotDetPanel.Visible = false;
                RoiMan.panel.Visible = false;
                Tracking.panel.Visible = false;
                UnDoBtn.Enabled = false;
                ReDoBtn.Enabled = false;
                GLControl1_VerticalPanel.Visible = false;
                GLControl1_TraserPanel.Visible = false;
                //TabPages.PropertiesBody.ResumeLayout();
                TabPages.propertiesPanel.ResumeLayout();
                
                return;
            }
            bool turnOnRoiMan = false;

            TifFileInfo fi = null;
            try
            {
                fi = TabPages.TabCollections[TabPages.SelectedIndex].tifFI;
            }
            catch { }

            if (fi != null)
            {
                try
                {
                    
                    if (fi.sizeZ > 1 && TabPages.zTrackBar.Panel.Visible != true)
                    {
                        TabPages.zTrackBar.Refresh(fi.zValue + 1, 1, fi.sizeZ);
                        TabPages.zTrackBar.Panel.Visible = true;
                    }
                    else if(fi.sizeZ <= 1 && TabPages.zTrackBar.Panel.Visible != false)
                    {
                        TabPages.zTrackBar.Panel.Visible = false;
                    }

                    if (fi.sizeT > 1 && TabPages.tTrackBar.Panel.Visible != true)
                    {
                        TabPages.tTrackBar.Refresh(fi.frame + 1, 1, fi.sizeT);
                        TabPages.tTrackBar.Panel.Visible = true;
                    }
                    else if(fi.sizeT <= 1 && TabPages.tTrackBar.Panel.Visible != false)
                    {
                        TabPages.tTrackBar.Panel.Visible = false;
                    }
                   

                    if (/*fi.loaded && */fi.tpTaskbar != null
                        && fi.tpTaskbar.TopBar.BackColor != FileBrowser.BackGroundColor1)
                    {
                        fi.tpTaskbar.TopBar.BackColor = FileBrowser.BackGroundColor1;
                        fi.tpTaskbar.TopBar.Invalidate();
                        fi.tpTaskbar.TopBar.Update();
                        fi.tpTaskbar.TopBar.Refresh();
                        
                        Application.DoEvents();
                    }
                }
                catch { }
                //Chart properties refresh
                chart.Properties.LoadFI(fi);
                chart.Series.LoadFI(fi);
                //set z and t
                EnabletrackBars(fi.loaded);
                //
                if (fi.openedImages < fi.sizeC * fi.sizeZ)
                {
                    //TabPages.propertiesPanel.ResumeLayout();
                    //TabPages.PropertiesBody.ResumeLayout();
                    return;
                }

                //zoomValue refresh
                zoomValue_Set(fi.zoom);
                BandC.autoDetect.Checked = fi.autoDetectBandC;
                BandC.applyToAll.Checked = fi.applyToAllBandC;
                //Segmentation
                //Segmentation.ProtocolCBox.SelectedIndex = fi.SegmentationProtocol[fi.cValue];
                Segmentation.SegmentationCBox.SelectedIndex = fi.SegmentationCBoxIndex[fi.cValue];
                Segmentation.Otsu1D.thresholdsNumCB.SelectedIndex = fi.thresholdsCBoxIndex[fi.cValue];
                Segmentation.Otsu1D.sumHistogramsCheckBox.Checked = fi.sumHistogramChecked[fi.cValue];
                Segmentation.Otsu1D.loadThreshAndColorBtns(fi);
                Segmentation.SpotDet.ReloadImage(fi);
                Segmentation.MyFilters.LoadImageInfo(fi);
                //Tracking
                Tracking.LoadSettings(fi);
                //RoiManager
                RoiMan.FillRoiManagerList(fi);
                RoiMan.fillTextBox(fi);
                RoiMan.calculateRoiAccessRect(fi);
                //Image Drawer
                try
                {
                   IDrawer.DrawToScreen();
                }
                catch { }
                //Prop Panel settings
                #region Raw Image
                if (fi.selectedPictureBoxColumn == 0 & fi.cValue < fi.sizeC 
                    & fi.tpTaskbar.ColorBtnList[fi.cValue].ImageIndex == 0
                    & fi.tpTaskbar.MethodsBtnList[0].ImageIndex == 0)
                {
                    turnOnRoiMan = true;

                    if (settings.BandCVis[TabPages.ActiveAccountIndex] != "y")
                    {
                        BandC.panel.Height = 26;
                    }
                    BandC.panel.Visible = true;

                    // Hide the Segmentation historgram and show the Brigtness one on top
                    Segmentation.FormSegmentation.Hide();
                    BandC.FormBrightnessContrast.BringToFront();
                    BandC.FormBrightnessContrast.Show();
                   

                    if (settings.MetaVis[TabPages.ActiveAccountIndex] != "y")
                    {
                        Meta.panel.Height = 26;
                    }
                    else
                    {
                        if (Meta.panel.Height != 126)
                        {
                            Meta.panel.Height = 126;
                            refresh_controls(Meta.panel);
                        }
                    }
                    Meta.UpdateInfo();
                    Meta.panel.Visible = true;
                    Meta.panel.BringToFront();
                }
                else
                {
                    BandC.panel.Visible = false;
                    Meta.panel.Visible = false;
                }
                #endregion Raw Image

                #region Filtered image
                if (fi.selectedPictureBoxColumn == 1 & fi.cValue < fi.sizeC
                    & fi.tpTaskbar.ColorBtnList[fi.cValue].ImageIndex == 0
                    & fi.tpTaskbar.MethodsBtnList[1].ImageIndex == 0)
                {
                    turnOnRoiMan = true;
                    //Tracking panel
                    if (settings.TrackingVis[TabPages.ActiveAccountIndex] != "y")
                    {
                        Tracking.panel.Height = 26;
                    }
                    else
                    {
                        if (Tracking.panel.Height != 105)
                        {
                            Tracking.panel.Height = 105;
                            refresh_controls(Tracking.panel);
                        }
                    }
                    //LibPanel
                    /*
                    if (settings.SegmentLibPanelVis[TabPages.ActiveAccountIndex] != "y")
                    {
                        Segmentation.LibPanel.Height = 26;
                    }
                    else
                    {
                        Segmentation.LibPanel.Height = 50;
                    }*/
                    //Data Panel
                    if (settings.SegmentDataPanelVis[TabPages.ActiveAccountIndex] != "y")
                    {
                        Segmentation.DataPanel.Height = 26;
                    }
                    else
                    {
                        if (Segmentation.DataPanel.Height != 180) { 
                            Segmentation.DataPanel.Height = 180;
                            refresh_controls(Segmentation.DataPanel);
                    }

                    //HistogramPanel
                    if (settings.SegmentHistPanelVis[TabPages.ActiveAccountIndex] != "y")
                    {
                        Segmentation.HistogramPanel.Height = 26;
                    }
                    else
                    {
                            if (Segmentation.HistogramPanel.Height != int.Parse(settings.SegmentHistPanelHeight[TabPages.ActiveAccountIndex]))
                            {
                                Segmentation.HistogramPanel.Height = int.Parse(settings.SegmentHistPanelHeight[TabPages.ActiveAccountIndex]);
                                refresh_controls(Segmentation.HistogramPanel);
                            }
                     }
                        //load histogram
                        Segmentation.Segmentation_LoadHistogramToChart(fi);

                        // Hide the Brightness historgram and show the Segmentation one on top
                        BandC.FormBrightnessContrast.Hide();
                        Segmentation.FormSegmentation.Show();
                    }

                    //tresholdsPanel
                    if (settings.SegmentTreshPanelVis[TabPages.ActiveAccountIndex] != "y")
                    {
                        Segmentation.tresholdsPanel.Height = 26;
                    }
                    else
                    {
                        int old_height = Segmentation.tresholdsPanel.Height;

                        switch (Segmentation.SegmentationCBox.SelectedIndex)
                        {
                            case 1:
                                Segmentation.Otsu1D.sumHistogramsCheckBox.Visible = true;
                                Segmentation.tresholdsPanel.Height = 56 +
                                    Segmentation.Otsu1D.panel.Height;
                                break;
                            case 2:
                                Segmentation.Otsu1D.sumHistogramsCheckBox.Visible = true;
                                Segmentation.tresholdsPanel.Height = 56 +
                                   Segmentation.Otsu1D.panel.Height;
                                break;
                            default:
                                Segmentation.tresholdsPanel.Height = 56;
                                break;
                        }

                        if (old_height == 26)
                        {
                            refresh_controls(Segmentation.tresholdsPanel);
                            refresh_controls(Segmentation.Otsu1D.panel);
                        }
                    }
                    //SpotDetPanel
                    if (settings.SegmentSpotDetPanelVis[TabPages.ActiveAccountIndex] != "y")
                    {
                        Segmentation.SpotDetPanel.Height = 26;
                    }
                    else
                    {
                        if (Segmentation.SpotDetPanel.Height != 104)
                        {
                            Segmentation.SpotDetPanel.Height = 104;
                            refresh_controls(Segmentation.SpotDetPanel);
                        }
                    }
                    //reorder

                    if (Segmentation.LibPanel.Visible != true)
                    {
                        //Segmentation.LibPanel.Visible = true;
                        //Segmentation.LibPanel.BringToFront();
                        
                    }

                    if (Segmentation.HistogramPanel.Visible != true)
                    {
                        Segmentation.HistogramPanel.Visible = true;
                        Segmentation.HistogramPanel.BringToFront();
                        refresh_controls(Segmentation.HistogramPanel);
                    }
                    
                    if (Segmentation.DataPanel.Visible != true)
                    {
                        Segmentation.DataPanel.Visible = true;
                        Segmentation.DataPanel.BringToFront();
                        refresh_controls(Segmentation.DataPanel);
                    }

                    if (Segmentation.tresholdsPanel.Visible != true)
                    {
                        Segmentation.tresholdsPanel.Visible = true;
                        Segmentation.tresholdsPanel.BringToFront();
                        refresh_controls(Segmentation.tresholdsPanel);
                    }

                    if (/*SpotDetectorEnabled &&*/ Segmentation.SpotDetPanel.Visible != true)
                    {
                        Segmentation.SpotDetPanel.Visible = true;
                        Segmentation.SpotDetPanel.BringToFront();
                        refresh_controls(Segmentation.SpotDetPanel);
                    }

                    if (Tracking.panel.Visible != true)
                    {
                        Tracking.panel.Visible = true;
                        Tracking.panel.BringToFront();
                        refresh_controls(Tracking.panel);
                    }
                }
                else
                {
                    //Segmentation.LibPanel.Visible = false;
                    Segmentation.DataPanel.Visible = false;
                    Segmentation.HistogramPanel.Visible = false;
                    Segmentation.tresholdsPanel.Visible = false;
                    Segmentation.SpotDetPanel.Visible = false;
                    Tracking.panel.Visible = false;
                }
                #endregion Filtered image
                
                #region Chart
                if (fi.selectedPictureBoxColumn == 2 & fi.cValue < fi.sizeC
                    & fi.tpTaskbar.ColorBtnList[fi.cValue].ImageIndex == 0 & fi.tpTaskbar.MethodsBtnList[2].ImageIndex == 0)
                {
                    turnOnRoiMan = true;
                    if (settings.CTChart_PropertiesVis[TabPages.ActiveAccountIndex] != "y")
                    {
                        chart.Properties.panel.Height = 26;
                    }
                    else
                    {
                        if (chart.Properties.panel.Height != 90)
                        {
                            chart.Properties.panel.Height = 90;
                            refresh_controls(chart.Properties.panel);
                        }
                    }
                    

                    if (settings.CTChart_SeriesVis[TabPages.ActiveAccountIndex] != "y")
                    {
                        chart.Series.panel.Height = 26;
                    }
                    else
                    {
                        if (chart.Series.panel.Height == 26)
                        {
                            chart.Series.panel.Height = int.Parse(settings.CTChart_SeriesHeight[TabPages.ActiveAccountIndex]);
                            refresh_controls(chart.Series.panel);
                        }
                        
                    }

                    if (chart.Properties.panel.Visible != true)
                    {
                        chart.Properties.panel.Visible = true;
                        chart.Properties.panel.BringToFront();
                        refresh_controls(chart.Properties.panel);
                    }

                    if (chart.Series.panel.Visible != true)
                    {
                        chart.Series.panel.Visible = true;
                        chart.Series.panel.BringToFront();
                        refresh_controls(chart.Series.panel);
                    }

                    Segmentation.FormSegmentation.Hide();
                    BandC.FormBrightnessContrast.Hide();
                }
                else
                {
                    chart.Properties.panel.Visible = false;
                    chart.Series.panel.Visible = false;
                }
                #endregion Chart

                #region Roi Manager
                //Roi manager
                if (turnOnRoiMan == true)
                {
                    if (settings.RoiManVis[TabPages.ActiveAccountIndex] != "y")
                        RoiMan.panel.Height = 26;
                    else if (RoiMan.panel.Height != int.Parse(settings.RoiManHeight[TabPages.ActiveAccountIndex]))
                    {
                        RoiMan.panel.Height = int.Parse(settings.RoiManHeight[TabPages.ActiveAccountIndex]);
                        refresh_controls(RoiMan.panel);
                    }
                    RoiMan.panel.Visible = true;
                    RoiMan.panel.BringToFront();
                    refresh_controls(RoiMan.panel);

                }
                else
                    RoiMan.panel.Visible = false;

                #endregion Roi Manager
            }
            //LibPanel
            if (settings.SegmentLibPanelVis[TabPages.ActiveAccountIndex] != "y")
            {
                Segmentation.LibPanel.Height = 26;
            }
            else
            {
                if (Segmentation.LibPanel.Height != 50)
                {
                    Segmentation.LibPanel.Height = 50;
                    refresh_controls(Segmentation.LibPanel);
                }
            }


            Application.DoEvents();

            BandC.Chart1.BringToFront();
            //refresh_controls(BandC.Chart1.CA);
            TabPages.propertiesPanel.ResumeLayout();
            //TabPages.PropertiesBody.ResumeLayout();

        }

        

        public void refresh_controls(Control ctrl)
        {
            

            //ctrl.ResumeLayout(true);
            ctrl.Invalidate();
            Application.DoEvents();
            
            foreach (Control ctrl2 in ctrl.Controls) { refresh_controls(ctrl2); }

        }
        
        private void ChangeT(string Val)
        {
            TifFileInfo fi = TabPages.TabCollections[TabPages.SelectedIndex].tifFI;
           
            int oldVal = fi.frame;
            fi.frame = int.Parse(Val);
            if (IsFrameAvaliable(fi) == false)
            {
                fi.frame = oldVal;
                TabPages.tTrackBar.Value.ChangeValueFunction((oldVal + 1).ToString());
                TabPages.tTrackBar.TextBox1.Text = (oldVal + 1).ToString();
                TabPages.tTrackBar.TrackBar1.Value = oldVal + 1;
                oldComand = "";
                return;
            }
            else if (TabPages.tTrackBar.TrackBar1.Value != fi.frame + 1 | TabPages.tTrackBar.TextBox1.Text != (fi.frame + 1).ToString())
            {
                TabPages.tTrackBar.TextBox1.Text = (fi.frame + 1).ToString();
                TabPages.tTrackBar.TrackBar1.Value = fi.frame + 1;
            }

            ReloadImages();
        }
        private void ChangeZ(string Val)
        {
             TifFileInfo fi = TabPages.TabCollections[TabPages.SelectedIndex].tifFI;
            int oldVal = fi.zValue;
            fi.zValue = int.Parse(Val);
            if (IsFrameAvaliable(fi) == false)
            {
                fi.zValue = oldVal;
                TabPages.zTrackBar.Value.ChangeValueFunction((oldVal + 1).ToString());
                TabPages.zTrackBar.TextBox1.Text = (oldVal + 1).ToString();
                TabPages.zTrackBar.TrackBar1.Value = oldVal + 1;
                oldComand = "";
                return;
            }
            else if (TabPages.zTrackBar.TrackBar1.Value != fi.zValue + 1 | TabPages.tTrackBar.TextBox1.Text != (fi.zValue + 1).ToString())
            {
                TabPages.zTrackBar.TextBox1.Text = (fi.zValue + 1).ToString();
                TabPages.zTrackBar.TrackBar1.Value = fi.zValue + 1;
            }
            ReloadImages();
            
        }
        private void ChangeC(string Val)
        {
            TifFileInfo fi = TabPages.TabCollections[TabPages.SelectedIndex].tifFI;
            if (undo == false & redo == false)
            {
                //fi.History.Add("ChangeC(" + fi.cValue.ToString() + ")");
            }
            int oldVal = fi.cValue;
            fi.cValue = int.Parse(Val);
            if (IsFrameAvaliable(fi) == false)
            {
                return;
            }
            if (undo == false & redo == false)
            {
                //fi.History.Add("ChangeC(" + fi.cValue.ToString() + ")");
            }
            //UnDoBtn.Enabled = true;
            //CheckSizeHistory(fi);

            ReloadImages();
        }
        private void ChangeLUT(string val)
        {
            TifFileInfo fi = TabPages.TabCollections[TabPages.SelectedIndex].tifFI;

            int sep = val.IndexOf(",");
            int chanel = Convert.ToInt32(val.Substring(0, sep));
            Color col = ColorTranslator.FromHtml(val.Substring(sep + 1, val.Length - sep - 1));
            if (undo == false & redo == false)
            {
               fi.History.Add("LUT(" + chanel.ToString() + "," +
                    ColorTranslator.ToHtml(fi.LutList[chanel]).ToString() + ")");
            }
            fi.LutList[chanel] = col;
            
            if (IsFrameAvaliable(fi) == false)
            {
                return;
            }
            if (undo == false & redo == false)
            {
                fi.History.Add("LUT(" + chanel.ToString() + "," +
                    ColorTranslator.ToHtml(fi.LutList[chanel]).ToString() + ")");
            }
            UnDoBtn.Enabled = true;
            CheckSizeHistory(fi);
            //create bitmap
            Bitmap bmp = new Bitmap(20, 20);

            using (Graphics gr = Graphics.FromImage(bmp))
            {
                gr.FillRectangle(new SolidBrush(fi.LutList[chanel]), new Rectangle(1, 1, 17, 17));
                gr.DrawRectangle(new Pen(Color.Black), new Rectangle(1, 1, 17, 17));
            }
            var ctr = fi.tpTaskbar.ColorBtnList[chanel];
            ctr.ImageList.Images[0] = bmp;
            ctr.Focus();

            if (fi.tpTaskbar.ColorBtnList.Count > 1)
            {
                Bitmap bmp1 = new Bitmap(20, 20);

                using (Graphics gr = Graphics.FromImage(bmp1))
                {
                    gr.FillRectangle(new SolidBrush(fi.LutList[1]), new Rectangle(0, 0, 16, 16));
                    gr.DrawRectangle(new Pen(Color.Black), new Rectangle(0, 0, 16, 16));
                    gr.FillRectangle(new SolidBrush(fi.LutList[0]), new Rectangle(3, 3, 16, 16));
                    gr.DrawRectangle(new Pen(Color.Black), new Rectangle(3, 3, 16, 16));
                }
                fi.tpTaskbar.ColorBtnList[fi.tpTaskbar.ColorBtnList.Count - 1].ImageList.Images[0] = bmp1;
                fi.tpTaskbar.ColorBtnList[fi.tpTaskbar.ColorBtnList.Count - 1].Focus();
            }

            ReloadImages();
            MarkAsNotSaved();
        }
        private void enableColorChanel(string val)
        {
            TifFileInfo fi = TabPages.TabCollections[TabPages.SelectedIndex].tifFI;

            int sep = val.IndexOf(",");
            int chanel = Convert.ToInt32(val.Substring(0, sep));
            int index = 0;
            if (val.Substring(sep + 1, val.Length - sep - 1) == "false")
                index = 1;

            var ctr = fi.tpTaskbar.ColorBtnList[chanel];

            if(ctr.ImageIndex == index) return;

            ctr.ImageIndex = index;
            ctr.Focus();

            if (undo == false & redo == false)
            {
                if (index == 1)
                {
                    fi.History.Add("enableColorChanel("
                           + chanel.ToString() + ",true)");
                    fi.History.Add("enableColorChanel("
                        + chanel.ToString() + ",false)");
                }
                else
                {
                    fi.History.Add("enableColorChanel("
                       + chanel.ToString() + ",false)");
                    fi.History.Add("enableColorChanel("
                          + chanel.ToString() + ",true)");
                }
            }

            if (IsFrameAvaliable(fi) == false) return;
            
            UnDoBtn.Enabled = true;
            CheckSizeHistory(fi);
            ReloadImages();
            MarkAsNotSaved();
        }
        private void enableMethodView(string val)
        {
            TifFileInfo fi = TabPages.TabCollections[TabPages.SelectedIndex].tifFI;

            int sep = val.IndexOf(",");
            int chanel = Convert.ToInt32(val.Substring(0, sep));
            int index = 0;
            if (val.Substring(sep + 1, val.Length - sep - 1) == "false")
                index = 1;

            var ctr = fi.tpTaskbar.MethodsBtnList[chanel];

            if (ctr.ImageIndex == index) return;

            ctr.ImageIndex = index;
            ctr.Focus();

            if (undo == false & redo == false)
            {
                if (index == 1)
                {
                    fi.History.Add("enableMethodView("
                           + chanel.ToString() + ",true)");
                    fi.History.Add("enableMethodView("
                        + chanel.ToString() + ",false)");
                }
                else
                {
                    fi.History.Add("enableMethodView("
                       + chanel.ToString() + ",false)");
                    fi.History.Add("enableMethodView("
                          + chanel.ToString() + ",true)");
                }
            }

            if (IsFrameAvaliable(fi) == false) return;

            UnDoBtn.Enabled = true;
            CheckSizeHistory(fi);
            ReloadImages();
            MarkAsNotSaved();
        }
        private void segmentation_ChangeMethod(string val)
        {
            TifFileInfo fi = TabPages.TabCollections[TabPages.SelectedIndex].tifFI;

            int sep = val.IndexOf(",");
            int chanel = Convert.ToInt32(val.Substring(0, sep));
            int index = Convert.ToInt32(val.Substring(sep + 1, val.Length - sep - 1));
            
            if (fi.SegmentationCBoxIndex[chanel] == index) return;
            
            if (undo == false & redo == false)
            {
                fi.History.Add("segmentation.ChangeMethod(" + chanel.ToString() + ","
                    + fi.SegmentationCBoxIndex[fi.cValue].ToString() + ")");
                fi.History.Add("segmentation.ChangeMethod(" + chanel.ToString() + ","
                    + index.ToString() + ")");
            }

            GLControl1.Focus();
            fi.SegmentationCBoxIndex[chanel] = index;

            if (IsFrameAvaliable(fi) == false) return;

            UnDoBtn.Enabled = true;
            CheckSizeHistory(fi);
            ReloadImages();
            MarkAsNotSaved();
        }

        private void TrackingParameters_SetParametars(string val)
        {
            TifFileInfo fi = TabPages.TabCollections[TabPages.SelectedIndex].tifFI;

            string[] vals = val.Split(new string[] { "," }, StringSplitOptions.None);
            int chanel = Convert.ToInt32(vals[0]);
            int MaxSize = Convert.ToInt32(vals[1]);
            int MinSize = Convert.ToInt32(vals[2]);
            int MaxSpeed = Convert.ToInt32(vals[3]);

            if (undo == false & redo == false)
            {
                fi.History.Add("TrackingParameters(" + fi.cValue.ToString() + ","
                    + fi.tracking_MaxSize[fi.cValue].ToString() + ","
                     + fi.tracking_MinSize[fi.cValue].ToString() + ","
                      + fi.tracking_Speed[fi.cValue].ToString() + ")");
                fi.History.Add("TrackingParameters(" + chanel.ToString() + ","
                    + MaxSize.ToString() + ","
                     + MinSize.ToString() + ","
                      + MaxSpeed.ToString() + ")");
            }

            fi.tracking_MaxSize[chanel] = MaxSize;
            fi.tracking_MinSize[chanel] = MinSize;
            fi.tracking_Speed[chanel] = MaxSpeed;

            GLControl1.Focus();

            if (IsFrameAvaliable(fi) == false) return;

            UnDoBtn.Enabled = true;
            CheckSizeHistory(fi);
            ReloadImages();
            MarkAsNotSaved();
        }
        private void segmentation_SetThreshOld(string val)
        {
            TifFileInfo fi = TabPages.TabCollections[TabPages.SelectedIndex].tifFI;

            string[] vals = val.Split(new string[] { "," }, StringSplitOptions.None);
            int chanel = Convert.ToInt32(vals[0]);
            int index = Convert.ToInt32(vals[1]);
            int newVal = Convert.ToInt32(vals[2]);

            if (fi.thresholdValues[chanel][index] == newVal 
                | index > fi.thresholds[fi.cValue]) return;

            if (undo == false & redo == false)
            {
                fi.History.Add("segmentation.SetThreshOld(" + chanel.ToString() + ","
                    + index.ToString() + "," +
                    fi.thresholdValues[chanel][index].ToString() + ")");
                fi.History.Add("segmentation.SetThreshOld(" + chanel.ToString() + ","
                    + index.ToString() + "," +
                    newVal.ToString() + ")");
            }

            GLControl1.Focus();
            fi.thresholdValues[chanel][index] = newVal;

            if (IsFrameAvaliable(fi) == false) return;

            UnDoBtn.Enabled = true;
            CheckSizeHistory(fi);
            ReloadImages();
            MarkAsNotSaved();
        }
        private void segmentation_SetSpotDetector(string val)
        {
            TifFileInfo fi = TabPages.TabCollections[TabPages.SelectedIndex].tifFI;
            
            string[] vals = val.Split(new string[] { "," }, StringSplitOptions.None);
            int cVal = Convert.ToInt32(vals[0]);
            int spotThresh = Convert.ToInt32(vals[1]);
            int spotSensitivity = Convert.ToInt32(vals[2]);
            Color SpotColor = ColorTranslator.FromHtml(vals[3]);
            string SpotTailType = vals[4];
            int selectedSpotThresh = Convert.ToInt32(vals[5]);
            int typeSpotThresh = Convert.ToInt32(vals[6]);

            if (fi.cValue == cVal & 
                fi.SpotThresh[fi.cValue] == spotThresh &
                fi.spotSensitivity[fi.cValue] == spotSensitivity &
                 fi.SpotColor[fi.cValue] == SpotColor &
                 fi.SpotTailType[fi.cValue] == SpotTailType &
                 fi.SelectedSpotThresh[fi.cValue] == selectedSpotThresh &
                 fi.typeSpotThresh[fi.cValue] == typeSpotThresh) return;

            if (undo == false & redo == false)
            {
                fi.History.Add("segmentation.SpotDetector("
                 + fi.cValue.ToString() + "," +
                 fi.SpotThresh[fi.cValue].ToString() + "," +
                 fi.spotSensitivity[fi.cValue].ToString() + "," +
                 ColorTranslator.ToHtml(fi.SpotColor[fi.cValue]) + "," +
                 fi.SpotTailType[fi.cValue] + "," +
                 fi.SelectedSpotThresh[fi.cValue].ToString() + "," +
                 fi.typeSpotThresh[fi.cValue].ToString() + ")");

                fi.History.Add("segmentation.SpotDetector("
                 + cVal.ToString() + "," +
                 spotThresh.ToString() + "," +
                 spotSensitivity.ToString() + "," +
                 ColorTranslator.ToHtml(SpotColor) + "," +
                 SpotTailType + "," +
                 selectedSpotThresh.ToString() + "," +
                typeSpotThresh.ToString() + ")");
            }

            GLControl1.Focus();

            fi.SpotThresh[fi.cValue] = spotThresh;
            fi.spotSensitivity[fi.cValue] = spotSensitivity;
            fi.SpotColor[fi.cValue] = SpotColor;
            fi.SpotTailType[fi.cValue] = SpotTailType;
            fi.SelectedSpotThresh[fi.cValue] = selectedSpotThresh;
            fi.typeSpotThresh[fi.cValue] = typeSpotThresh;

            if (IsFrameAvaliable(fi) == false) return;

            UnDoBtn.Enabled = true;
            CheckSizeHistory(fi);
            ReloadImages();
            MarkAsNotSaved();
        }
        private void segmentation_SetColor(string val)
        {
            TifFileInfo fi = TabPages.TabCollections[TabPages.SelectedIndex].tifFI;

            string[] vals = val.Split(new string[] { "," }, StringSplitOptions.None);
            int chanel = Convert.ToInt32(vals[0]);
            int index = Convert.ToInt32(vals[1]);
            Color col = ColorTranslator.FromHtml(vals[2]);

            if (fi.thresholdColors[chanel][index] == col) return;

            if (undo == false & redo == false)
            {
                fi.History.Add("segmentation.SetColor("
                         + fi.cValue + "," + index.ToString() + ","
                         + ColorTranslator.ToHtml(fi.thresholdColors[fi.cValue][index]) + ")");
                fi.History.Add("segmentation.SetColor("
                    + fi.cValue + "," + index.ToString() + ","
                    + vals[2] + ")");
            }
            GLControl1.Focus();

            if(col != Color.Transparent)
                fi.RefThresholdColors[chanel][index] = col;

            fi.thresholdColors[chanel][index] = col;
            
            if (IsFrameAvaliable(fi) == false) return;

            UnDoBtn.Enabled = true;
            CheckSizeHistory(fi);
            ReloadImages();
            MarkAsNotSaved();
        }
        private void ChangeBandC(string val)
        {
            TifFileInfo fi = TabPages.TabCollections[TabPages.SelectedIndex].tifFI;
            if (val == "auto")
            {
                fi.autoDetectBandC = true;
                ReloadImages();
                //MarkAsNotSaved();
            }
            else
            {
                int sep = val.IndexOf(",");
                int sep1 = val.LastIndexOf(",");
                int chanel = Convert.ToInt32(val.Substring(0, sep));
                int min = Convert.ToInt32(val.Substring(sep + 1, sep1 - sep - 1));
                int max = Convert.ToInt32(val.Substring(sep1 + 1, val.Length - sep1 - 1));

                fi.applyToAllBandC = false;
                fi.autoDetectBandC = false;

                BandC.adjustBrightness(min, max, fi, chanel);

                ReloadImages();
               // MarkAsNotSaved();
            }
        }
       public bool IsFrameAvaliable(TifFileInfo fi)
        {
            if (fi.available == true) { return true; }
            FrameCalculator FC = new FrameCalculator();
            int frame = FC.Frame(fi);
            if (frame < fi.openedImages - 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private void roi_new(string val)
        {
            TifFileInfo fi = TabPages.TabCollections[TabPages.SelectedIndex].tifFI;

            string[] vals = val.Split(new string[] { "," }, StringSplitOptions.None);
            int chanel = Convert.ToInt32(vals[0]);
            int RoiID = Convert.ToInt32(vals[1]);

            foreach (List<ROI> roiList in fi.roiList)
                if (roiList != null)
                    foreach (ROI roi in roiList)
                        if (RoiID == roi.getID)
                        {
                            MessageBox.Show("Error: ID already exists!");
                            return;
                        }

            string RoiInfo = vals[2];

            ROI current = new ROI();
            current.CreateFromHistory(RoiID, RoiInfo);
            fi.roiList[chanel].Add(current);

            RoiMeasure.Measure(current, fi, chanel, this);

            if (undo == false & redo == false)
            {
                #region History
                RoiMan.addToHistoryOldInfo(RoiMan.roi_delete(chanel, RoiID), fi);
                RoiMan.addToHistoryNewInfo(RoiMan.roi_new(chanel, current), fi);
                #endregion History
            }

            GLControl1.Focus();

            if (IsFrameAvaliable(fi) == false) return;
            
            ReloadImages();
        }
        private void roi_delete(string val)
        {
            TifFileInfo fi = TabPages.TabCollections[TabPages.SelectedIndex].tifFI;

            string[] vals = val.Split(new string[] { "," }, StringSplitOptions.None);
            int chanel = 0;
            int RoiID = Convert.ToInt32(vals[1]);
            foreach (List<ROI> roiList in fi.roiList)
            {
                if (roiList != null)
                    foreach (ROI roi in roiList)
                        if (RoiID == roi.getID)
                        {
                            if (undo == false & redo == false)
                            {
                                #region History
                                RoiMan.addToHistoryOldInfo(RoiMan.roi_new(chanel, roi), fi);
                                RoiMan.addToHistoryNewInfo(RoiMan.roi_delete(chanel, RoiID), fi);
                                #endregion History
                            }
                            if (RoiMan.current == roi) RoiMan.current = null;
                            fi.roiList[chanel].Remove(roi);
                            RoiMan.SelectedROIsList.Remove(roi);

                            GLControl1.Focus();

                            if (IsFrameAvaliable(fi) == false) return;

                            ReloadImages();
                            return;
                        }
                chanel++;
            }
            MessageBox.Show("Error: THere is not ROI with such ID");
        }
        private void roi_change(string val)
        {
            TifFileInfo fi = TabPages.TabCollections[TabPages.SelectedIndex].tifFI;
            //roi.change(chanelN,roiID,imageN,stat = val)
            string[] vals = val.Split(new string[] { "," }, StringSplitOptions.None);
            int chanel = Convert.ToInt32(vals[0]);
            int RoiID = Convert.ToInt32(vals[1]);
            int imageN = Convert.ToInt32(vals[2]);

            string stat = vals[3].Split(new string[] { "=" }, StringSplitOptions.None)[0];

            foreach (List<ROI> roiList in fi.roiList)
                if (roiList != null)
                    foreach (ROI roi in roiList)
                        if (RoiID == roi.getID)
                        {
                            
                            if (undo == false && redo == false && 
                                fi.roiList[fi.cValue].IndexOf(roi) > -1)
                                RoiMan.addToHistoryOldInfo(RoiMan.roi_getStat(roi, fi, stat), fi);

                            roi.setStatus(vals[3], imageN);

                            RoiMeasure.Measure(roi, fi, chanel, this);

                            if (undo == false && redo == false &&
                                fi.roiList[fi.cValue].IndexOf(roi) > -1)
                                RoiMan.addToHistoryNewInfo(RoiMan.roi_getStat(roi, fi, stat), fi);
                            
                            GLControl1.Focus();

                            if (IsFrameAvaliable(fi) == false) return;

                            ReloadImages();
                            return;
                        }
        }
        private void roi_resize(string val)
        {
            //roi.resize(chanelN,roiID,imageN,W,H,Location)
            TifFileInfo fi = TabPages.TabCollections[TabPages.SelectedIndex].tifFI;

            string[] vals = val.Split(new string[] { "," }, StringSplitOptions.None);
            int chanel = Convert.ToInt32(vals[0]);
            int RoiID = Convert.ToInt32(vals[1]);
            int imageN = Convert.ToInt32(vals[2]);
            int W = Convert.ToInt32(vals[3]);
            int H = Convert.ToInt32(vals[4]);
            string LocStr = vals[5];

            foreach (List<ROI> roiList in fi.roiList)
                if (roiList != null)
                    foreach (ROI roi in roiList)
                        if (RoiID == roi.getID)
                        {
                            if (undo == false & redo == false)
                            {
                                #region History
                                RoiMan.addToHistoryOldInfo(roi.getRoiResizeToHistory(chanel, imageN), fi);
                                //RoiMan.addToHistoryNewInfo(roi.getRoiResizeToHistory(chanel, imageN), fi);
                                #endregion History
                            }

                            roi.Width = W;
                            roi.Height = H;
                            if(roi.Type == 1 &&(roi.Shape == 0|| roi.Shape == 1))
                            {
                                Point p = roi.GetLocation(imageN)[0];
                                roi.locationFromHist(LocStr, imageN);
                                Point p1 = roi.GetLocation(imageN)[0];
                                int dX = p.X - p1.X;
                                int dY = p.Y - p1.Y;
                                Point[][] allP = roi.GetLocationAll();
                                for(int i = fi.cValue; i< allP[0].Length; i+=fi.sizeC)
                                {
                                    if (i == imageN) continue;
                                    allP[0][i].X -= dX;
                                    allP[0][i].Y -= dY;
                                }
                                roi.SetLocationAll(allP);
                            }
                            else
                                roi.locationFromHist(LocStr, imageN);

                            if (undo == false & redo == false)
                            {
                                #region History
                                RoiMan.addToHistoryNewInfo(roi.getRoiResizeToHistory(chanel, imageN), fi);
                                #endregion History
                            }

                            RoiMeasure.Measure(roi, fi, chanel, this);

                            GLControl1.Focus();

                            if (IsFrameAvaliable(fi) == false) return;

                            ReloadImages();
                            return;
                        }
        }
        private void ChangeChart_xAxisType(string val)
        {
            TifFileInfo fi = TabPages.TabCollections[TabPages.SelectedIndex].tifFI;
            GLControl1.Focus();

            int index = int.Parse(val);
            if (fi.xAxisTB == index) return;

            if (undo == false & redo == false)
            {
                fi.delHist = true;
                delHist = true;
                UnDoBtn.Enabled = true;
                DeleteFromHistory();
                chart.Properties.AddXAxisTBToHistory(fi);
            }

           // MessageBox.Show(index.ToString());
            fi.xAxisTB = index;

            if (undo == false & redo == false)
            {
                chart.Properties.AddXAxisTBToHistory(fi);
                //ReloadImage
                UpdateUndoBtns();
                MarkAsNotSaved();
            }

            CheckSizeHistory(fi);
            ReloadImages();
        }
        private void ChangeChart_yAxisType(string val)
        {
            TifFileInfo fi = TabPages.TabCollections[TabPages.SelectedIndex].tifFI;
            GLControl1.Focus();

            int index = int.Parse(val);

            if (fi.yAxisTB == index) return;

            if (undo == false & redo == false)
            {
                fi.delHist = true;
                delHist = true;
                UnDoBtn.Enabled = true;
                DeleteFromHistory();
                chart.Properties.AddYAxisTBToHistory(fi);
            }

            fi.yAxisTB = index;

            if (undo == false & redo == false)
            {
                chart.Properties.AddYAxisTBToHistory(fi);
                //ReloadImage
                UpdateUndoBtns();
                MarkAsNotSaved();
            }
            
            CheckSizeHistory(fi);
            ReloadImages();
        }

        private void Chart_SetSeriesColor(string val)
        {
            TifFileInfo fi = TabPages.TabCollections[TabPages.SelectedIndex].tifFI;
            GLControl1.Focus();
            if (fi.roiList == null) return;
            //Chart.SetSeriesColor(chanel,roiID, index, color.HTML) -> change the color of the series
            string[] vals = val.Split(new string[] { "," }, StringSplitOptions.None);

            int chanel = int.Parse(vals[0]);
            int roiID = int.Parse(vals[1]);
            int ind = int.Parse(vals[2]);
            Color col = ColorTranslator.FromHtml(vals[3]);

            if (fi.roiList[chanel] == null) return;

            ROI roi = null;

            foreach (ROI roi1 in fi.roiList[chanel])
                if (roi1.getID == roiID)
                {
                    roi = roi1;
                    break;
                }

            if (roi == null) return;

            if (undo == false & redo == false)
            {
                AddToHistoryOld("Chart.SetSeriesColor(" + chanel.ToString() + ","
                    + roi.getID.ToString() + "," + ind.ToString() + "," + ColorTranslator.ToHtml(roi.colors[ind]) + ")");

            }

            roi.colors[ind] = col;

            if (undo == false & redo == false)
            {
                AddToHistoryNew("Chart.SetSeriesColor(" + chanel.ToString() + ","
                    + roi.getID.ToString() + "," + ind.ToString() + "," + ColorTranslator.ToHtml(roi.colors[ind]) + ")");

            }
            ReloadImages();
        }

        private void Chart_SetSeriesChecked(string val)
        {
            TifFileInfo fi = TabPages.TabCollections[TabPages.SelectedIndex].tifFI;
            GLControl1.Focus();
            if (fi.roiList == null) return;

            string[] vals = val.Split(new string[] { "," }, StringSplitOptions.None);

            int chanel = int.Parse(vals[0]);
            int roiID = int.Parse(vals[1]);
            int ind = int.Parse(vals[2]);
            bool b = bool.Parse(vals[3]);

            if (fi.roiList[chanel] == null) return;

            ROI roi = null;

            foreach (ROI roi1 in fi.roiList[chanel])
                if (roi1.getID == roiID)
                {
                    roi = roi1;
                    break;
                }

            if (roi == null) return;

            if (undo == false & redo == false)
            {
                AddToHistoryOld("Chart.SetSeriesChecked(" + chanel.ToString() + ","
                    + roi.getID.ToString() + "," + ind.ToString() + "," + roi.ChartUseIndex[ind].ToString() + ")");

            }

            roi.ChartUseIndex[ind] = b;

            if (undo == false & redo == false)
            {
                AddToHistoryNew("Chart.SetSeriesChecked(" + chanel.ToString() + ","
                     + roi.getID.ToString() + "," + ind.ToString() + "," + roi.ChartUseIndex[ind].ToString() + ")");

            }
            ReloadImages();
        }
        //Add events here

        private void SwitchCaseEventOperator(string cmd)
        {
            try
            {
                int l = cmd.IndexOf("(");
                string Val = cmd.Substring(l + 1, cmd.Length - l - 2);

                switch (cmd.Substring(0, l))
                {
                    case "newFilter":
                        Segmentation.MyFilters.StartFromHistory(Val);
                        break;
                    case "open":
                        OpenFile(Val);
                        break;
                    case "SelectImage":
                        SelectImage(Val);
                        break;
                    case "SelectImageIndex":
                        SelectImageIndex(Val);
                        break;
                    case "ChangeT":
                        ChangeT(Val);
                        break;
                    case "ChangeZ":
                        ChangeZ(Val);
                        break;
                    case "ChangeC":
                        ChangeC(Val);
                        break;
                    case "B&C":
                        DeleteFromHistory();
                        ChangeBandC(Val);
                        break;
                    case "LUT":
                        DeleteFromHistory();
                        ChangeLUT(Val);
                        break;
                    case "enableColorChanel":
                        DeleteFromHistory();
                        enableColorChanel(Val);
                        break;
                    case "enableMethodView":
                        DeleteFromHistory();
                        enableMethodView(Val);
                        break;
                    case "segmentation.ChangeMethod":
                        DeleteFromHistory();
                        segmentation_ChangeMethod(Val);
                        break;
                    case "segmentation.SetThreshOld":
                        DeleteFromHistory();
                        segmentation_SetThreshOld(Val);
                        break;
                    case "segmentation.SetColor":
                        DeleteFromHistory();
                        segmentation_SetColor(Val);
                        break;
                    case "segmentation.SpotDetector":
                        DeleteFromHistory();
                        segmentation_SetSpotDetector(Val);
                        break;
                    case "TrackingParameters":
                        DeleteFromHistory();
                        TrackingParameters_SetParametars(Val);
                        break;
                    case "roi.new":
                        DeleteFromHistory();
                        roi_new(Val);
                        break;
                    case "roi.delete":
                        DeleteFromHistory();
                        roi_delete(Val);
                        break;
                    case "roi.change":
                        DeleteFromHistory();
                        roi_change(Val);
                        break;
                    case "roi.resize":
                        DeleteFromHistory();
                        roi_resize(Val);
                        break;
                        
                    case "Chart.XAxisType":
                        DeleteFromHistory();
                        ChangeChart_xAxisType(Val);
                        break;
                    case "Chart.YAxisType":
                        DeleteFromHistory();
                        ChangeChart_yAxisType(Val);
                        break;
                    case "Chart.SetSeriesColor":
                        DeleteFromHistory();
                        Chart_SetSeriesColor(Val);
                        break;
                    case "Chart.SetSeriesChecked":
                        DeleteFromHistory();
                        Chart_SetSeriesChecked(Val);
                        break;
                    //add other cases
                    default:
                        MessageBox.Show("Error:\n" + cmd);
                        break;

                }
                CheckIsProcessFinished();

            }
            catch
            {
                MessageBox.Show("Error:\n" + cmd);
            }
        }
        private void CheckIsProcessFinished()
        {
            //find selected image
            TifFileInfo fi = null;
            try
            {
                fi = TabPages.TabCollections[TabPages.SelectedIndex].tifFI;
            }
            catch { return; }
            if(fi == null) { return; }
            //start task
            //var t = Task.Run(() => {
                try
                {
                    while (fi.available == false)
                    {
                        Application.DoEvents();
                        Thread.Sleep(10);
                    }
                }
                catch { }
            //});
            //wait the process to finish
            //t.Wait();
        }
    }
}
