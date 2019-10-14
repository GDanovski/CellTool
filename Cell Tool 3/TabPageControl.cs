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
    class TabPageControl
    {
        private Form MainForm;
        //Image Analyser
        public ImageAnalyser IA;
        //FileBrowser
        public CTFileBrowser FileBrowser;
        //
        public bool hidePropAndBrows = false;
        //TabPages
        public List<TabPage> TabCollections = new List<TabPage>();
        //Properties panel
        public int ActiveAccountIndex = 0;
        public Panel propertiesPanel = new Panel();
        private ToolTip TurnOnToolTip = new ToolTip();
        //Selected Tab Index
        public int SelectedIndex;
        public FileDecoder myFileDecoder = new FileDecoder();
        //colors
        private Color BackGroundColor1 = Color.DimGray;
        private Color BackGround2Color1 = Color.FromArgb(255, 60, 60, 60);
        private Color ShriftColor1 = Color.White;
        private Color TitlePanelColor1 = Color.CornflowerBlue;
        private Color TaskBtnColor1;
        private Color TaskBtnClickColor1;
        //panels
        private Panel Body = new Panel();
         public Panel OpenPanel = new Panel();
        public Panel TitlePanel = new Panel();
        public Panel ImageMainPanel = new Panel();
        public Panel MainPanel;
        public Panel ResultsExtractorMainPanel = new Panel();
        //Tab Page List
        public List<List<Control>> Collections = new List<List<Control>>();
        //global start tab index
        private int startAt = 0;
        //Scroll Buttons
        private Button ScrollForwBtn = new Button();
        private Button ScrollBackBtn = new Button();
        private Timer t;
        //Rearange Tabs
        private int MoveTabIndex = -1;
        //Resize Properties panel
        public Panel PropertiesBody = new Panel();
        private bool propertiesPanel_Resize = false;
        private Panel ResizePanel = new Panel();
        private int oldX;
        //Frames and Zstack trackbars
        public CTTrackBar zTrackBar = new CTTrackBar();
        public CTTrackBar tTrackBar = new CTTrackBar();
        public Button tPlayStop = new Button();
        public Button zPlayStop = new Button();
        public void Initialize(Form MainForm, int ActiveAccountIndex1, Panel MainPanel1, Color BackGroundColor, Color BackGround2Color, Color ShriftColor, Color TitlePanelColor, Color TaskBtnColor, Color TaskBtnClickColor)
        {
            this.MainForm = MainForm;
            this.MainPanel = MainPanel1;
            
            Body.SuspendLayout();
            OpenPanel.SuspendLayout();
            TitlePanel.SuspendLayout();
            ImageMainPanel.SuspendLayout();
            ResultsExtractorMainPanel.SuspendLayout();
            PropertiesBody.SuspendLayout();
            ResizePanel.SuspendLayout();

            BackGroundColor1 = BackGroundColor;
            BackGround2Color1 = BackGround2Color;
            ShriftColor1 = ShriftColor;
            TitlePanelColor1 = TitlePanelColor;
            TaskBtnColor1 = TaskBtnColor;
            TaskBtnClickColor1 = TaskBtnClickColor;

            Body.Controls.Add(OpenPanel);
            OpenPanel.BackColor = BackGround2Color1;
            OpenPanel.VisibleChanged += new EventHandler(OpenPanel_VisibleChange);

            Body.BackColor = BackGround2Color;
            Body.ForeColor = ShriftColor;
            Body.Dock = DockStyle.Fill;
            MainPanel1.Controls.Add(Body);
            Body.BringToFront();

            TitlePanel.Dock = DockStyle.Top;
            TitlePanel.BackColor = BackGroundColor;
            TitlePanel.Height = 21;
            Body.Controls.Add(TitlePanel);
            TitlePanel.BringToFront();
            TitlePanel.Resize += new EventHandler(TitlePanel_Resize);

            Panel LinePanel = new Panel();
            LinePanel.Dock = DockStyle.Top;
            LinePanel.BackColor = TitlePanelColor;
            LinePanel.Height = 2;
            Body.Controls.Add(LinePanel);
            LinePanel.BringToFront();

            ImageMainPanel.BackColor = BackGround2Color;
            ImageMainPanel.ForeColor = ShriftColor;
            ImageMainPanel.Dock = DockStyle.Fill;
            Body.Controls.Add(ImageMainPanel);
            ImageMainPanel.BringToFront();
            ImageMainPanel.VisibleChanged += ImageMainPanel_VisibleChanged;

            ResultsExtractorMainPanel.BackColor = BackGround2Color;
            ResultsExtractorMainPanel.ForeColor = ShriftColor;
            ResultsExtractorMainPanel.Dock = DockStyle.Fill;
            Body.Controls.Add(ResultsExtractorMainPanel);
            ResultsExtractorMainPanel.BringToFront();

            // Scroll Panel
            Panel scrollPanel = new Panel();
            scrollPanel.Dock = DockStyle.Right;
            scrollPanel.Width = 56;
            TitlePanel.Controls.Add(scrollPanel);

            ScrollBackBtn.Text = "";
            ScrollBackBtn.BackColor = BackGroundColor;
            ScrollBackBtn.FlatStyle = FlatStyle.Flat;
            ScrollBackBtn.FlatAppearance.BorderSize = 0;
            ScrollBackBtn.Width = 18;
            ScrollBackBtn.Height = 21;
            ScrollBackBtn.Location = new System.Drawing.Point(20, 0);
            ScrollBackBtn.Visible = false;
            ScrollBackBtn.Image = Properties.Resources.scroll_minus_last;
            scrollPanel.Controls.Add(ScrollBackBtn);
            ScrollBackBtn.Click += new EventHandler(ScrollBackBtn_Click);
            ScrollBackBtn.MouseDown += ScrollBackBtn_MouseDown;
            ScrollBackBtn.MouseUp += ScrollBtn_MouseUp;

            ScrollForwBtn.Text = "";
            ScrollForwBtn.BackColor = BackGroundColor;
            ScrollForwBtn.FlatStyle = FlatStyle.Flat;
            ScrollForwBtn.FlatAppearance.BorderSize = 0;
            ScrollForwBtn.Width = 18;
            ScrollForwBtn.Height = 21;
            ScrollForwBtn.Location = new System.Drawing.Point(39, 0);
            ScrollForwBtn.Visible = false;
            ScrollForwBtn.Image = Properties.Resources.scroll_plus_last;
            scrollPanel.Controls.Add(ScrollForwBtn);
            ScrollForwBtn.Click += new EventHandler(ScrollForwBtn_Click);
            ScrollForwBtn.MouseDown += ScrollForwBtn_MouseDown;
            ScrollForwBtn.MouseUp += ScrollBtn_MouseUp;
            //Properties panel
            propertiesPanel.Dock = DockStyle.Right;
            propertiesPanel.BackColor = BackGround2Color;
            propertiesPanel.Width = 300;
            Body.Controls.Add(propertiesPanel);
            propertiesPanel.BringToFront();

            //add Turn on/off button
            Button TurnOnBtn = new Button();
            TurnOnBtn.Tag = "Show/Hide Properties";
            TurnOnBtn.BackColor = TaskBtnColor;
            TurnOnBtn.Text = "";
            TurnOnBtn.Dock = DockStyle.Left;
            TurnOnBtn.ForeColor = TaskBtnColor;
            TurnOnBtn.FlatStyle = FlatStyle.Flat;
            TurnOnBtn.TextImageRelation = TextImageRelation.Overlay;
            TurnOnBtn.FlatAppearance.BorderColor = TurnOnBtn.BackColor;
            //TurnOnBtn.Image = Properties.Resources.DataSourcesIcon;
            TurnOnBtn.Width = 10;
            propertiesPanel.Controls.Add(TurnOnBtn);
            //add tool tip to turn on/off button
            {
                TurnOnBtn.MouseHover += new EventHandler(Control_MouseOver);
                //Hide and show File Browser
                TurnOnBtn.Click += new EventHandler(delegate (Object o, EventArgs a)
                {
                    var btn = (Control)o;
                    Properties.Settings settings = Properties.Settings.Default;

                    if (propertiesPanel.Width == 15)
                    {
                        propertiesPanel.Width = int.Parse(settings.PropertiesPanelWidth[ActiveAccountIndex]);
                        settings.PropertiesPanelVisible[ActiveAccountIndex] = "y";
                        Histograms_Reload();
                        IA.refresh_controls(propertiesPanel);

                    }
                    else
                    {
                        propertiesPanel.Width = 15;
                        settings.PropertiesPanelVisible[ActiveAccountIndex] = "n";
                    }
                    settings.Save();
                });
            }

            //Add verticalTitle panel
            Panel verticalTitle = new Panel();
            verticalTitle.Dock = DockStyle.Left;
            verticalTitle.BackColor = BackGroundColor;
            verticalTitle.Width = 5;
            propertiesPanel.Controls.Add(verticalTitle);
            //Resize Panel
            verticalTitle.MouseMove += new MouseEventHandler(PropertiesPanel_MouseMove);
            verticalTitle.MouseDown += new MouseEventHandler(PropertiesPanel_MouseDown);
            verticalTitle.MouseUp += new MouseEventHandler(PropertiesPanel_MouseUp);
            ResizePanel.Anchor = (AnchorStyles.Top | AnchorStyles.Bottom);
            ResizePanel.Visible = false;
            ResizePanel.BackColor = Color.FromArgb(100, 10, 10, 10);
            ResizePanel.Width = 5;
            Body.Controls.Add(ResizePanel);
            ResizePanel.BringToFront();

            //properties title
            //Add Data Source title
            Panel proprtiesTitlePanel = new Panel();
            proprtiesTitlePanel.Dock = DockStyle.Top;
            proprtiesTitlePanel.BackColor = BackGroundColor;
            proprtiesTitlePanel.Height = 21;
            propertiesPanel.Controls.Add(proprtiesTitlePanel);
            proprtiesTitlePanel.BringToFront();
            PropertiesBody.AutoScroll = true;

            Label propertiesTitlelabel = new Label();
            propertiesTitlelabel.ForeColor = ShriftColor;
            propertiesTitlelabel.Width = 150;
            propertiesTitlelabel.Text = "Properties";
            propertiesTitlelabel.Location = new System.Drawing.Point(10, 5);
            proprtiesTitlePanel.Controls.Add(propertiesTitlelabel);

            PropertiesBody.Dock = DockStyle.Fill;
            propertiesPanel.Controls.Add(PropertiesBody);
            PropertiesBody.BringToFront();
            

            //Frames and Z track bars
            {
                CTTrackBar tb = tTrackBar;
                tb.Initialize();
                tb.Panel.Dock = DockStyle.Bottom;
                tb.BackColor(BackGround2Color);
                tb.ForeColor(ShriftColor);
                tb.Panel.Visible = false;
                tb.Refresh(0, 0, 10);
                tb.Name.Text = "Time stack";
                tb.Name.Location = new System.Drawing.Point(tb.Name.Location.X + 5, tb.Name.Location.Y);
                tb.NamePanel.Width = 100;
                Body.Controls.Add(tb.Panel);
                tb.Panel.BringToFront();
                tb.Value.Changed += new ChangedValueEventHandler(delegate (Object o, ChangeValueEventArgs a)
                {
                  IA.Input.ChangeValueFunction("ChangeT(" + (int.Parse(a.Value) - 1).ToString() + ")");
                });

                Button btn = tPlayStop;
                btn.Text = "";
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderSize = 0;
                btn.Image = Properties.Resources.Play;
                btn.Width = 20;
                btn.Height = 20;
                btn.Location = new Point(tb.TrackBar1.Location.X -20, tb.Name.Location.Y - 3);
               tb.Panel.Controls.Add(btn);
               btn.BringToFront();

            }
            {
                CTTrackBar tb = zTrackBar;
                tb.Initialize();
                tb.Panel.Dock = DockStyle.Bottom;
                tb.BackColor(BackGround2Color);
                tb.ForeColor(ShriftColor);
                tb.Panel.Visible = false;
                tb.Refresh(0, 0, 10);
                tb.Name.Text = "Z stack";
                tb.NamePanel.Width = 100;
                tb.Name.Location = new System.Drawing.Point(tb.Name.Location.X + 5, tb.Name.Location.Y);
                Body.Controls.Add(tb.Panel);
                tb.Panel.BringToFront();
                tb.Value.Changed += new ChangedValueEventHandler(delegate (Object o, ChangeValueEventArgs a)
                {
                    IA.Input.ChangeValueFunction("ChangeZ(" + (int.Parse(a.Value) - 1).ToString() + ")");
                });

                Button btn = zPlayStop;
                btn.FlatStyle = FlatStyle.Flat;
                btn.Image = Properties.Resources.Play;
                btn.FlatAppearance.BorderSize = 0;
                btn.Text = "";
                btn.Width = 20;
                btn.Height = 20;
                btn.Location = new Point(tb.TrackBar1.Location.X - 20, tb.Name.Location.Y - 3);
                tb.Panel.Controls.Add(btn);
                btn.BringToFront();
            }
            //shrinck image panel
           
            ImageMainPanel.BringToFront();

            ResultsExtractorMainPanel.Visible = false;
            //ImageMainPanel.Visible = false;
            propertiesPanel.Visible = false;
            
            Body.ResumeLayout(false);
            OpenPanel.ResumeLayout(false);
            TitlePanel.ResumeLayout(false);
            ImageMainPanel.ResumeLayout(false);
            ResultsExtractorMainPanel.ResumeLayout(false);
            PropertiesBody.ResumeLayout(false);
            ResizePanel.ResumeLayout(false);

        }
        /// <summary>
        /// Reloads the BandC and Segmentation histogram upon properties panel resizing
        /// </summary>
        public void Histograms_Reload()
        {
            //Fix chart redrawing
            TifFileInfo fi = null;
            try
            {
                if (SelectedIndex >= 0 && SelectedIndex < TabCollections.Count)
                    fi = TabCollections[SelectedIndex].tifFI;
            }
            catch { }

            if (fi != null)
            {
                //IA.BandC.Chart1.DrawToScreen(fi);
                if (fi.selectedPictureBoxColumn == 0 & fi.cValue < fi.sizeC
         & fi.tpTaskbar.ColorBtnList[fi.cValue].ImageIndex == 0
         & fi.tpTaskbar.MethodsBtnList[0].ImageIndex == 0)
                {
                    var size = IA.BandC.Chart1.CA.Size;
                    IA.BandC.Chart1.CA.Size = new Size(size.Width, size.Height + 1);
                    IA.BandC.Chart1.CA.Size = size;

                    IA.BandC.Chart1.CA.DrawToScreen(fi);
                    IA.BandC.Chart1.CA.Update();
                    IA.BandC.Chart1.CA.PerformLayout();


                    IA.ReloadImages();
                }
                else if (fi.selectedPictureBoxColumn == 1 & fi.cValue < fi.sizeC
        & fi.tpTaskbar.ColorBtnList[fi.cValue].ImageIndex == 0
        & fi.tpTaskbar.MethodsBtnList[1].ImageIndex == 0)
                {
                    var size = IA.Segmentation.Chart1.Size;
                    IA.Segmentation.Chart1.Size = new Size(size.Width, size.Height + 1);
                    IA.Segmentation.Chart1.Size = size;

                    IA.Segmentation.Chart1.DrawToScreen(fi);
                    IA.Segmentation.Chart1.Update();
                    IA.Segmentation.Chart1.PerformLayout();

                    IA.ReloadImages();
                }
            }
        }
        private void ImageMainPanel_VisibleChanged(object sender, EventArgs e)
        {
            propertiesPanel.Visible = ((Panel)sender).Visible;
            if (propertiesPanel.Visible)
            {
                zTrackBar.Panel.BringToFront();
                tTrackBar.Panel.BringToFront();
                ImageMainPanel.BringToFront();
            }
        }
        public void AddPlugIns()
        {
            //add plugins
            {
                //Brightness and Contrast
                IA.BandC.Initialize(propertiesPanel, PropertiesBody);
                IA.BandC.BackColor(BackGround2Color1);
                IA.BandC.ForeColor(ShriftColor1);
                IA.BandC.TitleColor(TitlePanelColor1);
                try
                {
                    IA.BandC.panel.Height = int.Parse(IA.settings.BandC[FileBrowser.ActiveAccountIndex]);
                }
                catch
                {
                    IA.BandC.panel.Height = int.Parse(IA.settings.BandC[0]);
                }
                IA.BandC.panel.Resize += new EventHandler(BandC_heightChange);
                //Metadata
                IA.Meta.Initialize(propertiesPanel, PropertiesBody,IA);
                IA.Meta.BackColor(BackGround2Color1);
                IA.Meta.ForeColor(ShriftColor1);
                IA.Meta.TitleColor(TitlePanelColor1);
                try
                {
                    IA.Meta.panel.Height = int.Parse(IA.settings.Meta[FileBrowser.ActiveAccountIndex]);
                }
                catch
                {
                    IA.Meta.panel.Height = int.Parse(IA.settings.Meta[0]);
                }
                IA.Meta.panel.Resize += new EventHandler(Meta_heightChange);
                //Segmentation
                IA.Segmentation = new Segmentation(propertiesPanel, PropertiesBody, IA);
                IA.Segmentation.BackColor(BackGround2Color1);
                IA.Segmentation.ForeColor(ShriftColor1);
                IA.Segmentation.TitleColor(TitlePanelColor1);
                //Segmentation Lib
                IA.Segmentation.LibPanel.Height = 200;
                IA.Segmentation.LibPanel.Resize += new EventHandler(SegmentationLibPanel_heightChange);
                //Segmentation Data
                IA.Segmentation.DataPanel.Height = 150;                
                IA.Segmentation.DataPanel.Resize += new EventHandler(SegmentationDataPanel_heightChange);
                //Segmentation Histogram
                try
                {
                    IA.Segmentation.HistogramPanel.Height = int.Parse(IA.settings.SegmentHistPanelHeight[FileBrowser.ActiveAccountIndex]);
                }
                catch
                {
                    IA.Segmentation.HistogramPanel.Height = int.Parse(IA.settings.SegmentHistPanelHeight[0]);
                }
                IA.Segmentation.HistogramPanel.Resize += new EventHandler(SegmentationHistogramPanel_heightChange);
                //Segmentation Tresholds
                IA.Segmentation.tresholdsPanel.Height = 56;
                IA.Segmentation.tresholdsPanel.Resize += new EventHandler(SegmentationTresholdsPanel_heightChange);
                //Segmentation Spot Detector
                IA.Segmentation.SpotDetPanel.Height = 104;
                IA.Segmentation.SpotDetPanel.Resize += new EventHandler(SegmentationSpotDetPanel_heightChange);
                //Tracking Manager
                IA.Tracking = new TrackSpots(propertiesPanel, PropertiesBody, IA);
                IA.Tracking.BackColor(BackGround2Color1);
                IA.Tracking.ForeColor(ShriftColor1);
                IA.Tracking.TitleColor(TitlePanelColor1);
                IA.Tracking.panel.Height = 80;
                IA.Tracking.panel.Resize += new EventHandler(TrackingPanel_heightChange);
                //Roi manager
                IA.RoiMan = new RoiManager(propertiesPanel, PropertiesBody,IA);
                IA.RoiMan.BackColor(BackGround2Color1);
                IA.RoiMan.ForeColor(ShriftColor1);
                IA.RoiMan.TitleColor(TitlePanelColor1);
                try
                {
                    IA.RoiMan.panel.Height = int.Parse(IA.settings.RoiManHeight[FileBrowser.ActiveAccountIndex]);
                }
                catch
                {
                    IA.RoiMan.panel.Height = int.Parse(IA.settings.RoiManHeight[0]);
                }
                IA.RoiMan.panel.Resize += RoiManPanel_heightChange;
                //Chart
                IA.chart = new CTChart(IA);
                IA.chart.Properties = new CTChart_Properties(propertiesPanel, PropertiesBody, IA);

                IA.chart.Properties.BackColor(BackGround2Color1);
                IA.chart.Properties.ForeColor(ShriftColor1);
                IA.chart.Properties.TitleColor(TitlePanelColor1);
                
                IA.chart.Properties.panel.Resize += chart_Properties_heightChange;

                IA.chart.Series = new CTChart_Series(propertiesPanel, PropertiesBody, IA);

                IA.chart.Series.BackColor(BackGround2Color1);
                IA.chart.Series.ForeColor(ShriftColor1);
                IA.chart.Series.TitleColor(TitlePanelColor1);

                IA.chart.Series.panel.Resize += chart_Series_heightChange;
            }
        }
        private void chart_Series_heightChange(object sender, EventArgs e)
        {
            if (IA.chart.Series.panel.Height <= 26)
            {
                IA.settings.CTChart_SeriesVis[FileBrowser.ActiveAccountIndex] = "n";
            }
            else
            {
                IA.settings.CTChart_SeriesVis[FileBrowser.ActiveAccountIndex] = "y";

                if (IA.chart.Series.panel.Height < int.Parse(IA.settings.CTChart_SeriesHeight[0]))
                    IA.chart.Series.panel.Height = int.Parse(IA.settings.CTChart_SeriesHeight[0]);

                IA.settings.CTChart_SeriesHeight[FileBrowser.ActiveAccountIndex] = IA.chart.Series.panel.Height.ToString();
            }
            IA.settings.Save();
        }
        private void chart_Properties_heightChange(object sender, EventArgs e)
        {
            if (IA.chart.Properties.panel.Height <= 26)
            {
                IA.settings.CTChart_PropertiesVis[FileBrowser.ActiveAccountIndex] = "n";
            }
            else
            {
                IA.settings.CTChart_PropertiesVis[FileBrowser.ActiveAccountIndex] = "y";
            }
            IA.settings.Save();
        }
        private void TrackingPanel_heightChange(object sender, EventArgs e)
        {
            if (IA.Tracking.panel.Height <= 26)
            {
                IA.settings.TrackingVis[FileBrowser.ActiveAccountIndex] = "n";
            }
            else
            {
                IA.settings.TrackingVis[FileBrowser.ActiveAccountIndex] = "y";
            }
            IA.settings.Save();
        }
        private void RoiManPanel_heightChange(object sender, EventArgs e)
        {
            if (IA.RoiMan.panel.Height <= 26)
            {
                IA.settings.RoiManVis[FileBrowser.ActiveAccountIndex] = "n";
            }
            else
            {
                IA.settings.RoiManVis[FileBrowser.ActiveAccountIndex] = "y";

                if (IA.RoiMan.panel.Height < int.Parse(IA.settings.RoiManHeight[0]))
                    IA.RoiMan.panel.Height = int.Parse(IA.settings.RoiManHeight[0]);

                IA.settings.RoiManHeight[FileBrowser.ActiveAccountIndex] = IA.RoiMan.panel.Height.ToString();
            }
            IA.settings.Save();
        }
        private void SegmentationSpotDetPanel_heightChange(object sender, EventArgs e)
        {
            if (IA.Segmentation.SpotDetPanel.Height <= 26)
            {
                IA.settings.SegmentSpotDetPanelVis[FileBrowser.ActiveAccountIndex] = "n";
            }
            else
            {
                IA.settings.SegmentSpotDetPanelVis[FileBrowser.ActiveAccountIndex] = "y";
            }
            IA.settings.Save();
        }
        private void SegmentationLibPanel_heightChange(object sender, EventArgs e)
        {
            if (IA.Segmentation.LibPanel.Height <= 26)
            {
                IA.settings.SegmentLibPanelVis[FileBrowser.ActiveAccountIndex] = "n";
            }
            else
            {
                IA.settings.SegmentLibPanelVis[FileBrowser.ActiveAccountIndex] = "y";
            }
            IA.settings.Save();
        }
        private void SegmentationDataPanel_heightChange(object sender, EventArgs e)
        {
            if (IA.Segmentation.DataPanel.Height <= 26)
            {
                IA.settings.SegmentDataPanelVis[FileBrowser.ActiveAccountIndex] = "n";
            }
            else
            {
                IA.settings.SegmentDataPanelVis[FileBrowser.ActiveAccountIndex] = "y";
            }
            IA.settings.Save();
        }
        private void SegmentationHistogramPanel_heightChange(object sender, EventArgs e)
        {
            if (IA.Segmentation.HistogramPanel.Height <= 26)
            {
                IA.settings.SegmentHistPanelVis[FileBrowser.ActiveAccountIndex] = "n";
            }
            else
            {
                IA.settings.SegmentHistPanelVis[FileBrowser.ActiveAccountIndex] = "y";
                IA.settings.SegmentHistPanelHeight[FileBrowser.ActiveAccountIndex] = IA.Segmentation.HistogramPanel.Height.ToString();
            }
            IA.settings.Save();
        }
        private void SegmentationTresholdsPanel_heightChange(object sender, EventArgs e)
        {
            if (IA.Segmentation.tresholdsPanel.Height <= 26)
            {
                IA.settings.SegmentTreshPanelVis[FileBrowser.ActiveAccountIndex] = "n";
            }
            else
            {
                IA.settings.SegmentTreshPanelVis[FileBrowser.ActiveAccountIndex] = "y";
                switch (IA.Segmentation.SegmentationCBox.SelectedIndex)
                {
                    case 1:
                        IA.Segmentation.Otsu1D.sumHistogramsCheckBox.Visible = true;
                        IA.Segmentation.tresholdsPanel.Height = 56 +
                                        IA.Segmentation.Otsu1D.panel.Height;
                        IA.Segmentation.Otsu1D.panel.Visible = true;
                        break;
                    case 2:
                        IA.Segmentation.Otsu1D.sumHistogramsCheckBox.Visible = true;
                        IA.Segmentation.tresholdsPanel.Height = 56 +
                                        IA.Segmentation.Otsu1D.panel.Height;
                        IA.Segmentation.Otsu1D.panel.Visible = true;
                        break;
                    default:
                        IA.Segmentation.Otsu1D.panel.Visible = false;
                        IA.Segmentation.tresholdsPanel.Height = 56;
                        break;
                }
            }
            IA.settings.Save();
        }
        private void Meta_heightChange(object sender, EventArgs e)
        {
            if (IA.Meta.panel.Height <= 26)
            {
                IA.settings.MetaVis[FileBrowser.ActiveAccountIndex] = "n";
            }
            else
            {
                IA.settings.MetaVis[FileBrowser.ActiveAccountIndex] = "y";
                IA.settings.Meta[FileBrowser.ActiveAccountIndex] = IA.Meta.panel.Height.ToString();
            }
             IA.settings.Save();
        }
        private void BandC_heightChange(object sender,EventArgs e)
        {
            if (IA.BandC.panel.Height <= 26)
            {
                IA.settings.BandCVis[FileBrowser.ActiveAccountIndex] = "n";
            }
            else
            {
                IA.settings.BandCVis[FileBrowser.ActiveAccountIndex] = "y";

                if (IA.BandC.panel.Height < 100) IA.BandC.panel.Height = 100;
                IA.settings.BandC[FileBrowser.ActiveAccountIndex] = IA.BandC.panel.Height.ToString();
            }
            IA.settings.Save();
        }
        private void PropertiesPanel_MouseMove(object sender, MouseEventArgs e)
        {
            Panel pnl = sender as Panel;
            if (propertiesPanel_Resize == true)
            {
                int razlika = ResizePanel.Location.X - (oldX - e.X);
                {
                    if (razlika > 100 & razlika < Body.Width - 100)
                    {
                        oldX = e.X;
                        ResizePanel.Location = new System.Drawing.Point(razlika, propertiesPanel.Location.Y);
                    }
                }

            }
            else {

                if (propertiesPanel.Width >= 25)
                {
                    pnl.Cursor = Cursors.SizeWE;
                }
                else
                {
                    pnl.Cursor = Cursors.Default;
                }
            }
        }
        private void PropertiesPanel_MouseDown(object sender, MouseEventArgs e)
        {
            Panel pnl = sender as Panel;
            if (propertiesPanel.Width > 25)
            {
                propertiesPanel_Resize = true;
                ResizePanel.Location = new Point(propertiesPanel.Location.X, propertiesPanel.Location.Y);
                oldX = e.X;
                ResizePanel.Width = 5;
                ResizePanel.Height = propertiesPanel.Height;
                ResizePanel.BringToFront();
                ResizePanel.Visible = true;
            }
            else
            {
                ResizePanel.Visible = false;
                propertiesPanel_Resize = false;
            }
        }
        private void PropertiesPanel_MouseUp(object sender, MouseEventArgs e)
        {
            if (propertiesPanel_Resize == true)
            {
                propertiesPanel.Width = Body.Width - ResizePanel.Location.X ;

                if (propertiesPanel.Width < 100)
                {
                   propertiesPanel.Width = 100 ;
                }
                else if (propertiesPanel.Width > Body.Width - 100)
                {
                    propertiesPanel.Width =  Body.Width - 100;
                }
                
                Properties.Settings settings = Properties.Settings.Default;
                settings.PropertiesPanelWidth[ActiveAccountIndex] = Convert.ToString(propertiesPanel.Width);
                settings.Save();

                Panel pnl = sender as Panel;
                ResizePanel.Visible = false;
                propertiesPanel_Resize = false;
                pnl.Cursor = Cursors.Default;
            }
        }
        /////
        private void Control_MouseOver(object sender, EventArgs e)
        {
            var ctr = (Control)sender;
            TurnOnToolTip.SetToolTip(ctr, ctr.Tag.ToString());
        }
        private void ScrollBackBtn_Click(object sender, EventArgs e)
        {
            if (startAt > 0)
            {
                startAt -= 1;
                refreshTabsOrder(startAt);
            }
        }
        private void ScrollBackBtn_MouseDown(object sender, EventArgs e)
        {
            t = new Timer();
            t.Tick += new EventHandler(delegate(object o, EventArgs a)
            {
                ScrollBackBtn_Click(sender, e);
                
                if (ScrollBackBtn.Visible == false)
                    ScrollBtn_MouseUp(sender, e);
            });

            t.Interval = 200;
            t.Start();
        }
        private void ScrollForwBtn_MouseDown(object sender, EventArgs e)
        {
            t = new Timer();
            t.Tick += new EventHandler(delegate (object o, EventArgs a)
            {
                ScrollForwBtn_Click(sender, e);

                if (ScrollForwBtn.Visible == false)
                    ScrollBtn_MouseUp(sender, e);
            });

            t.Interval = 200;
            t.Start();
        }
        private void ScrollBtn_MouseUp(object sender, EventArgs e)
        {
            t.Stop();
        }
        private void ScrollForwBtn_Click(object sender, EventArgs e)
        {
            int Last = 0;
            int maxW = TitlePanel.Width - 56;
            int widthToCurControl = 0;
            
            for (int i = Collections.Count - 1; i >= 0; i--)
            {
                if (Collections[i][0].Visible == false)
                {
                    Last = i;
                }
                else
                {
                    break;
                }
            }
            for (int i = Last; i >= 0; i--)
            {
                if (widthToCurControl + Collections[i][0].Width < maxW)
                {
                    widthToCurControl += Collections[i][0].Width;
                }
                else
                {
                    startAt = i + 1;
                    refreshTabsOrder(i + 1);
                    break;
                }
            }

       }
        private void check_For_Scroll()
        {
            if(Collections.Count < 0)
            {
                ScrollForwBtn.Visible = false;
                ScrollBackBtn.Visible = false;
            }

            if(Collections[0][0].Visible == false)
            {
                ScrollBackBtn.Visible = true;
            }
            else
            {
                ScrollBackBtn.Visible = false;
            }

            if (Collections[Collections.Count - 1][0].Visible == false)
            {
                ScrollForwBtn.Visible = true;
            }
            else
            {
                ScrollForwBtn.Visible = false;
            }

        }
        private void TitlePanel_Resize(object sender, EventArgs e)
        {
            if (FormWindowState.Minimized == MainForm.WindowState) return;

            findStartIndex();
        }
        private void OpenPanel_VisibleChange(object sender, EventArgs e)
        {
            /*
            try
            {
                OpenPanel.Location = ImageMainPanel.Location;
                OpenPanel.Width = ImageMainPanel.Width;
                OpenPanel.Height = ImageMainPanel.Height;
            }
            catch { }
            if (zTrackBar.Panel.Visible == true) OpenPanel.Height += zTrackBar.Panel.Height;
            if (tTrackBar.Panel.Visible == true) OpenPanel.Height += tTrackBar.Panel.Height;
            */
            OpenPanel.Location = new Point(0, TitlePanel.Height + 2);
            OpenPanel.Width = Body.Width;
            OpenPanel.Height = Body.Height - TitlePanel.Height-2;
        }
        public void OpenEmptyResultsExtractor(object sender, EventArgs e)
        {
            string dir = "\\Results.CTData'";
            TifFileInfo fi = null;
            try
            {
                if(SelectedIndex>=0 && SelectedIndex < TabCollections.Count)
                    fi = TabCollections[SelectedIndex].tifFI;
            }
            catch { }

            if (fi != null)
            {
                dir = fi.Dir.Substring(0, fi.Dir.LastIndexOf("\\")) + dir;
            }
            
            FileBrowser.Openlabel.Tag = null;            
            FileBrowser.Openlabel.Text = "'" + dir;
            FileBrowser.Openlabel.Text = "";
            FileBrowser.Openlabel.Tag = null;
        }
        public void ExportResultsExtractorData(object sender,EventArgs e)
        {
            if(TabCollections[SelectedIndex].ResultsExtractor == null) return;

            ResultsExtractor.FileSaver.Export(
                (ResultsExtractor.MyForm)TabCollections[SelectedIndex].ResultsExtractor.myPanel);
        }
        public void Openlabel_textChanged(object sender, EventArgs e)
        {
            string str = (sender as Label).Text;
            if(str == "") { return; }
            if (myFileDecoder.decodeFileType(str.Substring(1,str.Length - 2)) == -1)
            {
                MessageBox.Show("Unsuported file type!");
                return;
            }
            
            TreeNode node = (sender as Label).Tag as TreeNode;
            //restore label
            (sender as Label).Tag = null;
            (sender as Label).Text = "";
            //open
            foreach (string name in str.Substring(1,str.Length - 1).Split(new[] { ",'" }, StringSplitOptions.None))
            {
                string strS = name.Substring(0, name.Length - 1);
                if (node == null)
                {
                    node = new TreeNode();
                    node.Text = FileNameFromDir(strS);
                    node.Tag = strS;
                    node.SelectedImageIndex = 1;
                    node.ImageIndex = 1;
                }
                OpenFile_Event(strS, node);
            }
        }
        private Boolean isAvailable(string Dir)
        {
            foreach (TabPage fi in TabCollections)
            {
                if(fi.tifFI!=null && fi.tifFI.Dir == Dir)
                {
                    return false;
                }
            }
            return true;
        }
        public void OpenFile_Event(string dir, TreeNode node)
        {
            Body.SuspendLayout();
            
            bool showResultsExtractorMainPanel = ResultsExtractorMainPanel.Visible;
            bool showImageMainPanel = ImageMainPanel.Visible;
            //Decode File Type
            int FileTypeIndex = myFileDecoder.decodeFileType(dir);

            //RoiSet
            if (FileTypeIndex == 1)//.RoiSet
            {
                IA.RoiMan.LoadRoiSet_DragDrop(dir);
                Body.ResumeLayout(true);
                return;
            }
            else if(FileTypeIndex == 2) //.PlugIn.dll
            {
                IA.PlugIns.InstallPlugIn(dir);
                Body.ResumeLayout(true);
                return;
            }
            else if (FileTypeIndex == 3) //.CTData
            {
                openResultsExtractor(dir, FileTypeIndex, node);
                Body.ResumeLayout(true);
                return;
            }

            int end = dir.LastIndexOf(".");
            //Check is it already open
            if (isAvailable(dir) == false | isAvailable(dir.Substring(0, end) + ".tif") == false)
            {
                Body.ResumeLayout(true);
                MessageBox.Show("File is already open!");
                return;
            }

            ResultsExtractorMainPanel.Visible = false;
            ImageMainPanel.Visible = true;

            //read image
            Panel CorePanel = myFileDecoder.OpenFile(TabCollections, dir, FileTypeIndex, IA);
            
            if (CorePanel == null)
            {
                ResultsExtractorMainPanel.Visible = showResultsExtractorMainPanel;
                ImageMainPanel.Visible = showImageMainPanel;
                Body.ResumeLayout(true);
                
                MessageBox.Show("Unsuported file type!");
                return;
            }

            ImageMainPanel.BackColor = BackGround2Color1;
            // add CorePanel
            CorePanel.Dock = DockStyle.Fill;
            
            List<Control> smallCollection = new List<Control>();
            dir = (string)CorePanel.Tag;
            CorePanel.Tag = null;

            Button NameBtn = new Button();
            NameBtn.Tag = node;
            NameBtn.Text = FileNameFromDir(dir.Substring(0, end) + ".tif");
            NameBtn.BackColor = TitlePanelColor1;
            NameBtn.FlatStyle = FlatStyle.Flat;
            NameBtn.FlatAppearance.BorderSize = 0;
            NameBtn.ForeColor = ShriftColor1;
            NameBtn.TextAlign = ContentAlignment.MiddleLeft;
            NameBtn.Width = TextRenderer.MeasureText(NameBtn.Text, NameBtn.Font).Width + 20;
            if (NameBtn.Width > 250) NameBtn.Width = 250;

            NameBtn.Height = 21;
            smallCollection.Add(NameBtn);
            TitlePanel.Controls.Add(NameBtn);
            NameBtn.Click += new EventHandler(SelectTabBtn_Click);
            NameBtn.BringToFront();
            NameBtn.MouseDown += new MouseEventHandler(NameBtn_MouseDown);
            NameBtn.MouseUp += new MouseEventHandler(NameBtn_MouseUp);
            NameBtn.MouseMove += new MouseEventHandler(NameBtn_MouseMove);
            NameBtn.MouseHover += NameBtn_MouseOver;

            Button xBtn = new Button();
            xBtn.Text = "X";
            xBtn.Font = new Font("Microsoft Sans Serif", 6, FontStyle.Bold);
            xBtn.FlatAppearance.BorderSize = 0;
            xBtn.FlatStyle = FlatStyle.Flat;
            xBtn.BackColor = TitlePanelColor1;
            xBtn.ForeColor = ShriftColor1;
            xBtn.Width = 15;
            xBtn.Height = 15;
            smallCollection.Add(xBtn);
            TitlePanel.Controls.Add(xBtn);
            xBtn.BringToFront();
            xBtn.Click += new EventHandler(DeleteTabbtn_Click);
                         
            Collections.Add(smallCollection);

            inactivate_Tabs();
            SelectedIndex = Collections.Count - 1;
            selectTab_event(SelectedIndex);

            findStartIndex();

            Body.ResumeLayout(true);
            Body.Invalidate();
            Body.Update();
            Body.Refresh();
            Application.DoEvents();
        }
        private void openResultsExtractor(string dir,int FileTypeIndex, TreeNode node)
        {
            ResultsExtractorMainPanel.Visible = true;
            ImageMainPanel.Visible = false;
            zTrackBar.Panel.Visible = false;
            tTrackBar.Panel.Visible = false;

            myFileDecoder.OpenFile(TabCollections, dir, FileTypeIndex, IA);
            ImageMainPanel.BackColor = BackGround2Color1;
            
            List<Control> smallCollection = new List<Control>();
            
            Button NameBtn = new Button();
            NameBtn.Tag = node;
            NameBtn.Text = FileNameFromDir(dir);
            NameBtn.BackColor = TitlePanelColor1;
            NameBtn.FlatStyle = FlatStyle.Flat;
            NameBtn.FlatAppearance.BorderSize = 0;
            NameBtn.ForeColor = ShriftColor1;
            NameBtn.TextAlign = ContentAlignment.MiddleLeft;
            NameBtn.Width = TextRenderer.MeasureText(NameBtn.Text, NameBtn.Font).Width + 20;
            if (NameBtn.Width > 250) NameBtn.Width = 250;

            NameBtn.Height = 21;
            smallCollection.Add(NameBtn);
            TitlePanel.Controls.Add(NameBtn);
            NameBtn.Click += new EventHandler(SelectTabBtn_Click);
            NameBtn.BringToFront();
            NameBtn.MouseDown += new MouseEventHandler(NameBtn_MouseDown);
            NameBtn.MouseUp += new MouseEventHandler(NameBtn_MouseUp);
            NameBtn.MouseMove += new MouseEventHandler(NameBtn_MouseMove);
            NameBtn.MouseHover += NameBtn_MouseOver;

            Button xBtn = new Button();
            xBtn.Text = "X";
            xBtn.Font = new Font("Microsoft Sans Serif", 6, FontStyle.Bold);
            xBtn.FlatAppearance.BorderSize = 0;
            xBtn.FlatStyle = FlatStyle.Flat;
            xBtn.BackColor = TitlePanelColor1;
            xBtn.ForeColor = ShriftColor1;
            xBtn.Width = 15;
            xBtn.Height = 15;
            smallCollection.Add(xBtn);
            TitlePanel.Controls.Add(xBtn);
            xBtn.BringToFront();
            xBtn.Click += new EventHandler(DeleteTabbtn_Click);

            Collections.Add(smallCollection);
            
            inactivate_Tabs();
            SelectedIndex = Collections.Count - 1;
            selectTab_event(SelectedIndex);

            findStartIndex();
        }
        private void NameBtn_MouseOver(object sender, EventArgs e)
        {
            var ctr = (Control)sender;

            if (TextRenderer.MeasureText(ctr.Text, ctr.Font).Width + 20 > 250)
                TurnOnToolTip.SetToolTip(ctr, ctr.Text);
            else
                TurnOnToolTip.SetToolTip(ctr, "");
        }
        public void findStartIndex()
        {
            if (Collections.Count - 1 < 0) { return; }

            int maxW = TitlePanel.Width - 56;
            int widthToCurControl = 0;
            Boolean count = false;
         
            for (int i = Collections.Count - 1; i >= 0; i--)
            {
                if (i == 0)
                {
                    startAt = 0;
                    refreshTabsOrder(0);
                }

                if (count == false & Collections[i][0].BackColor == TitlePanelColor1)
                {
                    count = true;
                }

                if (count == true 
                    & maxW > widthToCurControl + Collections[i][0].Width)
                {
                    widthToCurControl += Collections[i][0].Width;
                }
                else if (count == true)
                {
                    startAt = i + 1;
                    refreshTabsOrder(i + 1);
                    break;
                }
                
            }

        }
        public void refreshTabsOrder(int begin)
        {
            if (Collections.Count < 1)
            {
                IA.ReloadImages();
                ImageMainPanel.Visible = false;
                ResultsExtractorMainPanel.Visible = false;
                return;
            }
            int X = 0;
            for ( int i = 0; i < Collections.Count; i++)
            {
                if (i >= begin & X + Collections[i][0].Width < TitlePanel.Width - 56)
                {
                    Collections[i][0].Visible = true;
                    Collections[i][0].Location = new System.Drawing.Point(X, 0);
                    X += Collections[i][0].Width;
                    Collections[i][1].Location = new System.Drawing.Point(X - 18, 3);
                    Collections[i][1].Visible = true;
                }
                else
                {
                    Collections[i][0].Visible = false;
                    Collections[i][1].Visible = false;
                }
            }
            check_For_Scroll();
        }
        public void inactivate_Tabs()
        {
            foreach(List<Control> l in Collections)
            {
                if (l[0].BackColor != BackGroundColor1)
                {
                    l[0].BackColor = BackGroundColor1;
                    l[1].BackColor = BackGroundColor1;
                    TabCollections[Collections.IndexOf(l)].Visible(false);

                    if (TabCollections[Collections.IndexOf(l)].tifFI != null) 
                        TabCollections[Collections.IndexOf(l)].tifFI.selected = false;
                }
            }
            ResultsExtractorMainPanel.Controls.Clear();
        }
        public void DeleteTabbtn_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            for (int i = 0; i < Collections.Count; i++)
            {
                if (Collections[i][1] == btn)
                {
                    if (CheckIsItSaved(i) == false) { return; }
                    if (Collections[i][1].BackColor == TitlePanelColor1)
                    {
                        
                         if(Collections.Count > i + 1)
                        {
                            selectTab_event(i + 1);
                        }
                        else if(i - 1 >= 0)
                        {
                            selectTab_event(i - 1);
                        }
                         else if(Collections.Count == 1)
                        {
                            tTrackBar.Panel.Visible = false;
                            zTrackBar.Panel.Visible = false;
                        }
                    }
                    
                    {
                        Collections[i][1].Dispose();
                        Collections[i][0].Dispose();
                    }
                    Collections.RemoveAt(i);
                   
                     startAt = startAt - 1;
                     if (startAt < 0) { startAt = 0; }
                     refreshTabsOrder(startAt);
                    //Delete tab page
                    TabCollections[i].Delete();
                    TabCollections.RemoveAt(i);
                    if(TabCollections.Count < 1) { IA.GLControl1.Visible = false; }
                    if (i < SelectedIndex & SelectedIndex > 0) { SelectedIndex--; }
                    break;
                }
            }
        }
        public void DeleteSelected(object sender, EventArgs e)
        {
            int i = SelectedIndex;
            if (CheckIsItSaved(i) == false) { return; }
            if(Collections.Count <= 0) { return; }
            if (Collections[i][1].BackColor == TitlePanelColor1)
            {

                if (Collections.Count > i + 1)
                {
                    selectTab_event(i + 1);
                }
                else if (i - 1 >= 0)
                {
                    selectTab_event(i - 1);
                }
            }
            {
                Collections[i][1].Dispose();
                Collections[i][0].Dispose();
            }
            Collections.RemoveAt(i);

            startAt = startAt - 1;
            if (startAt < 0) { startAt = 0; }
            refreshTabsOrder(startAt);
            //Delete tab page
            TabCollections[i].Delete();
            TabCollections.RemoveAt(i);
            if (TabCollections.Count < 1) { IA.GLControl1.Visible = false; }
            if (i < SelectedIndex & SelectedIndex > 0) { SelectedIndex--; }
        }
        public void SaveFile(object sender,EventArgs e)
        {
            if (Collections.Count <= 0) { return; }
            int i = SelectedIndex;
            SaveItem(i);
            FileBrowser.Refresh_AfterSave();
        }
        public void SaveAllFile(object sender, EventArgs e)
        {
            if (Collections.Count <= 0) { return; }

            var res = MessageBox.Show("Do you want to save ALL opened images?"                   
                   , "Save All", MessageBoxButtons.YesNo);
            if (res == System.Windows.Forms.DialogResult.Yes)
            {
                for (int i = 0; i < TabCollections.Count; i++)
                {
                    SaveItem(i);
                }
                FileBrowser.Refresh_AfterSave();
                MessageBox.Show("Files saved");
            }
        }
        public void saveAs(object sender, EventArgs e)
        {
            if (Collections.Count <= 0) { return; }
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            TreeNode node = Collections[SelectedIndex][0].Tag as TreeNode;

            string formatMiniStr = myFileDecoder.Format_Extensions(node.Tag.ToString());
            // string formatStr = formatMiniStr.Substring(1, formatMiniStr.Length - 1) +
            // " files (*" + formatMiniStr + ")|*" + formatMiniStr;
            string formatStr = "";

            if (TabCollections[SelectedIndex].tifFI!=null)
                formatStr = "TIF files (*.tif)|*.tif";
            else if (TabCollections[SelectedIndex].ResultsExtractor != null)
                formatStr = "CTData files(*.CTData)| *.CTData";

            saveFileDialog1.Filter = formatStr;
            saveFileDialog1.FilterIndex = 1;
            saveFileDialog1.RestoreDirectory = false;
            saveFileDialog1.InitialDirectory = node.Tag.ToString().Substring(0, node.Tag.ToString().Length - (node.Text.Length + 1));
            saveFileDialog1.FileName = node.Text;
            saveFileDialog1.OverwritePrompt = true;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if (TabCollections[SelectedIndex].tifFI != null)
                {
                    string dir = OSStringConverter.GetWinString(saveFileDialog1.FileName);
                    int end = dir.LastIndexOf(".");
                    dir = dir.Substring(0, end) + ".tif";

                    TifFileInfo fi = TabCollections[SelectedIndex].tifFI;
                    fi.original = false;
                    fi.Dir = dir;
                    TabCollections[SelectedIndex].dir = dir;

                    SaveItem(SelectedIndex, true);
                    
                    try
                    {
                        dir = fi.Dir;
                        TreeNode n = null;

                        n = FileBrowser.CheckForFile(dir);
                        if (n == null)
                        {
                            n = new TreeNode();
                            n.Text = FileNameFromDir(dir);
                            n.Tag = dir;
                            n.SelectedImageIndex = node.SelectedImageIndex;
                            n.ImageIndex = node.ImageIndex;
                        }
                        findStartIndex();
                        Collections[SelectedIndex][0].Tag = n;
                        Collections[SelectedIndex][0].Text = n.Text;
                        Collections[SelectedIndex][0].Width = TextRenderer.MeasureText(Collections[SelectedIndex][0].Text, Collections[SelectedIndex][0].Font).Width + 20;
                        if (Collections[SelectedIndex][0].Width > 250) Collections[SelectedIndex][0].Width = 250;
                        refreshTabsOrder(startAt);
                        FileBrowser.Refresh_AfterSave();
                    }
                    catch { }
                }
                else if(TabCollections[SelectedIndex].ResultsExtractor != null)
                {
                    string dir = saveFileDialog1.FileName;
                    int end = dir.LastIndexOf(".");
                    dir = dir.Substring(0, end) + ".CTData";
                    
                    TabCollections[SelectedIndex].dir = dir;

                    SaveItem(SelectedIndex, true);

                    try
                    {
                        TreeNode n = null;

                        n = FileBrowser.CheckForFile(dir);
                        if (n == null)
                        {
                            n = new TreeNode();
                            n.Text = FileNameFromDir(dir);
                            n.Tag = dir;
                            n.SelectedImageIndex = node.SelectedImageIndex;
                            n.ImageIndex = node.ImageIndex;
                        }
                        findStartIndex();
                        Collections[SelectedIndex][0].Tag = n;
                        Collections[SelectedIndex][0].Text = n.Text;
                        Collections[SelectedIndex][0].Width = TextRenderer.MeasureText(Collections[SelectedIndex][0].Text, Collections[SelectedIndex][0].Font).Width + 20;
                        if (Collections[SelectedIndex][0].Width > 250) Collections[SelectedIndex][0].Width = 250;
                        refreshTabsOrder(startAt);
                        FileBrowser.Refresh_AfterSave();
                    }
                    catch { }
                }
            }
        }
        private void SaveItem(int i, bool newFile = false)
        {
            if (!newFile && TabCollections[i].ResultsExtractor != null &&
                !System.IO.File.Exists(OSStringConverter.StringToDir(TabCollections[i].dir)))
            {
                saveAs(new object(), new EventArgs());
                return;
            }

            TreeNode node = Collections[i][0].Tag as TreeNode;

            if (TabCollections[i].tifFI != null)
            {
                if (TabCollections[i].tifFI.available == false)
                {
                    if (TabCollections[i].Saved == false)
                    {
                        MessageBox.Show(node.Text + " cannot be saved because images are not loaded.");
                        return;
                    }
                    else
                    {
                        return;
                    }
                }
                if (TabCollections[i].tifFI.original == true)
                {
                    var res = MessageBox.Show("Do you want to save changes to "
                        + Collections[i][0].Text +
                        " ? \nThis file may contain additional metadata that will be lost!"
                        , "Save File", MessageBoxButtons.YesNo);
                    if (res != System.Windows.Forms.DialogResult.Yes)
                    { return; }
                    else
                    {
                        TabCollections[i].tifFI.original = false;
                    }
                }
            }

            TabCollections[i].Save(IA);
           
        }
        private Boolean CheckIsItSaved(int i)
        {
            if (TabCollections[i].Saved == true) { return true; }
            if (TabCollections[i].tifFI!= null && TabCollections[i].tifFI.available == false) { return false; }
            var res = MessageBox.Show("Do you want to save changes to " + Collections[i][0].Text + " ?","Save File", MessageBoxButtons.YesNoCancel);
            if (res == System.Windows.Forms.DialogResult.Yes)
            {
                TreeNode node = Collections[i][0].Tag as TreeNode;
                TabCollections[i].Save(IA);
                return true;
            }
            else if (res == System.Windows.Forms.DialogResult.No)
            {
                return true;
            }
            return false;
        }
        public void DeleteAll(object sender, EventArgs e)
        {
            if (Collections.Count <= 0) { return; }
            for (int i = Collections.Count - 1; i >= 0; i--)
            {
                if (CheckIsItSaved(i) == true)
                {
                    {
                        Collections[i][1].Dispose();
                        Collections[i][0].Dispose();
                    }
                    Collections.RemoveAt(i);
                    //Delete tab page
                    TabCollections[i].Delete();
                    TabCollections.RemoveAt(i);
                }
            }
            startAt = 0;
            refreshTabsOrder(startAt);
            if (TabCollections.Count < 1) { IA.GLControl1.Visible = false; }
        }
       public void SelectTabBtn_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            for (int i = 0; i < Collections.Count; i++)
            {
                if (Collections[i][0] != btn)
                {
                    if (Collections[i][0].BackColor != BackGroundColor1)
                    {
                        Collections[i][0].BackColor = BackGroundColor1;
                        Collections[i][1].BackColor = BackGroundColor1;
                        TabCollections[i].Visible(false);

                        if(TabCollections[i].tifFI!=null)
                            TabCollections[i].tifFI.selected = false;
                    }
                }
                else
                {
                    selectTab_event(i);
                }
            }
        }
        public string FileNameFromDir(string str)
        {
            int length = str.Length;
            int count = length - 1;
            while (count >= 0 & str.Substring(count, 1) != "\\")
            {
                count -= 1;
            }
            count += 1;
            string name = str.Substring(count, (length - count));

            return name;
        }
        public void selectTab_event(int index)
        {
            Body.SuspendLayout();
            ResultsExtractorMainPanel.Controls.Clear();
           
            if (Collections.Count > index)
            {
                Collections[index][0].BackColor = TitlePanelColor1;
                Collections[index][1].BackColor = TitlePanelColor1;
                TabCollections[index].Visible(true);

                SelectedIndex = index;
                if (TabCollections[index].tifFI != null && TabCollections[index].tifFI.loaded && TabCollections[index].tifFI.tpTaskbar != null)
                {

                    TabCollections[index].tifFI.selected = true;
                    
                    if (TabCollections[index].tifFI.sizeZ > 1)
                    {
                        zTrackBar.Refresh(TabCollections[index].tifFI.zValue + 1, 1, TabCollections[index].tifFI.sizeZ);
                        zTrackBar.Panel.Visible = true;
                    }
                    else
                    {
                        zTrackBar.Panel.Visible = false;
                    }

                    if (TabCollections[index].tifFI.sizeT > 1)
                    {
                        tTrackBar.Refresh(TabCollections[index].tifFI.frame + 1, 1, TabCollections[index].tifFI.sizeT);
                        tTrackBar.Panel.Visible = true;
                    }
                    else
                    {
                        tTrackBar.Panel.Visible = false;
                    }
                    //undo redo
                    IA.undo = TabCollections[index].tifFI.undo;
                    IA.redo = TabCollections[index].tifFI.redo;
                    IA.delHist = TabCollections[index].tifFI.delHist;
                    IA.UpdateUndoBtns();
                    IA.RoiMan.current = null;
                    //reload images
                    IA.Input.ChangeValueFunction("");
                    IA.oldComand = "";

                    ImageMainPanel.Visible = true;
                    ResultsExtractorMainPanel.Visible = false;
                    IA.IDrawer.FormImg.Show();
                    IA.Segmentation.FormSegmentation.Show();
                    IA.BandC.FormBrightnessContrast.Show();

                    IA.ReloadImages();
                    try
                    {
                        IA.GLControl1.Focus();
                    }
                    catch { };


                    try
                    {
                        if (TabCollections[index].tifFI.loaded && TabCollections[index].tifFI.tpTaskbar != null
                      && TabCollections[index].tifFI.tpTaskbar.TopBar.BackColor != FileBrowser.BackGroundColor1)
                            TabCollections[index].tifFI.tpTaskbar.TopBar.BackColor = FileBrowser.BackGroundColor1;
                        //TabCollections[index].tifFI.tpTaskbar.TopBar.BackColor = BackGroundColor1;
                    }
                    catch { }
                }
                else if (TabCollections[index].ResultsExtractor != null)
                {
                    tTrackBar.Panel.Visible = false;
                    zTrackBar.Panel.Visible = false;
                    ImageMainPanel.Visible = false;
                    ResultsExtractorMainPanel.Visible = true;
                    ResultsExtractorMainPanel.Controls.Add(TabCollections[index].ResultsExtractor.myPanel);
                    TabCollections[index].ResultsExtractor.myPanel.Dock = DockStyle.Fill;
                    IA.UpdateUndoBtns();

                    IA.IDrawer.FormImg.Hide();
                    IA.Segmentation.FormSegmentation.Hide();
                    IA.BandC.FormBrightnessContrast.Hide();
                }
            }

            Body.ResumeLayout(true);

            Body.Update();
            Body.Invalidate();
            Body.Refresh();
        }
        public void treeNode_Rename(object sender, EventArgs e)
        {
            Label label1 = sender as Label;
            if (label1.Text == "") { return; }
            //foreach (List<Control> l in Collections)
            for (int i = 0; i < Collections.Count; i++)
            {
                List<Control> l = Collections[i];
                TreeNode n = l[0].Tag as TreeNode;
                if (l[0].Text != n.Text)
                {
                    //rename tab
                    l[0].Text = n.Text;
                    l[0].Width = TextRenderer.MeasureText(l[0].Text, l[0].Font).Width + 20;
                    if (l[0].Width > 250) l[0].Width = 250;
                    //change the directory of file in file info class
                    if(TabCollections[i].tifFI!=null)
                        TabCollections[i].tifFI.Dir = n.Tag.ToString();
                    else
                        TabCollections[i].dir = n.Tag.ToString();
                }
            }
            refreshTabsOrder(startAt);
        }
     
        public void NameBtn_MouseDown(object sender, MouseEventArgs e)
        {
            Button btn = sender as Button;
            for (int i = 0; i < Collections.Count; i++)
            {
                if (btn == Collections[i][0])
                {
                    MoveTabIndex = i;
                }
            }
        }
        public void NameBtn_MouseUp(object sender, MouseEventArgs e)
        {
             MoveTabIndex = -1;
        }
        public void NameBtn_MouseMove(object sender, MouseEventArgs e)
        {
            if (MoveTabIndex == -1) { return; }

            Button btn = sender as Button;
            Point p = new Point(e.X + (sender as Button).Location.X, e.Y + (sender as Button).Location.Y);
            int NewTabIndex = -1;
            for (int i = 0; i < Collections.Count; i++)
            {
                if (Collections[i][0].Bounds.Contains(p) 
                    & btn != Collections[i][0] 
                    & Collections[i][0].Visible == true)
                {
                    NewTabIndex = i;
                }
            }
            
            if (NewTabIndex != MoveTabIndex & NewTabIndex != -1 & MoveTabIndex != -1)
            {
                List<Control> l = Collections[MoveTabIndex];
                Collections.RemoveAt(MoveTabIndex);
                Collections.Insert(NewTabIndex, l);
                TabPage tp = TabCollections[MoveTabIndex];
                TabCollections.RemoveAt(MoveTabIndex);
                TabCollections.Insert(NewTabIndex, tp);

                MoveTabIndex = NewTabIndex;
                refreshTabsOrder(startAt);
            }
           
        }
      
    }
}
