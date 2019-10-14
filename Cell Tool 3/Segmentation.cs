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
    class Segmentation
    {
        public ImageAnalyser IA = null;
        //Lib Panel 
        private PropertiesPanel_Item LibPropPanel;
        public Panel LibPanel;
        public AutoApplySettingsClass AutoSetUp;
        //Spot detector Panel
        private PropertiesPanel_Item SpotDetPropPanel;
        public Panel SpotDetPanel;
        public SpotDetector SpotDet;
        //Convolution Methods Panel
        public PropertiesPanel_Item DataPropPanel;
        public Panel DataPanel;
        public Filters MyFilters;
        //histogram Panel
        private PropertiesPanel_Item HistogramPropPanel;
        public Panel HistogramPanel;
        public BrightnessAndContrast_ChartPanel Chart1 = new BrightnessAndContrast_ChartPanel();
        private BrightnessAndContrast_ChartPanel.BrightnessAndContrast_Chart.Series Values = new BrightnessAndContrast_ChartPanel.BrightnessAndContrast_Chart.Series();
        private BrightnessAndContrast_ChartPanel.BrightnessAndContrast_Chart.Series[] Otsu1dSeries = new BrightnessAndContrast_ChartPanel.BrightnessAndContrast_Chart.Series[5];
        private BrightnessAndContrast_ChartPanel.BrightnessAndContrast_Chart.Series Spots = new BrightnessAndContrast_ChartPanel.BrightnessAndContrast_Chart.Series();
        public Form_auxiliary FormSegmentation; // For holding the GL control of the segmentation histogram

        //Segmentation Panel
        private PropertiesPanel_Item tresholdsPropPanel;
        public Panel tresholdsPanel;
        public OtsuSegmentation Otsu1D;
        public KMeansSegmentation Kmeans;
        public MyWatershed Watershed;
        //tooltip 
        private ToolTip TurnOnToolTip = new ToolTip();
        //items
        public ComboBox ProtocolCBox = new ComboBox();
        public ComboBox SegmentationCBox = new ComboBox();
        public Button AutoBtn = new Button();

        #region Initializing componends

        public Segmentation(Panel propertiesPanel, Panel PropertiesBody, ImageAnalyser IA)
        {
            //Global Class
            this.IA = IA;
            //tresholds Panel initialize
            tresholdsPropPanel = new PropertiesPanel_Item();
            tresholdsPanel_Initialize(propertiesPanel, PropertiesBody);
            //histogram Panel initialize
            HistogramPropPanel = new PropertiesPanel_Item();
            HistogramPanel_Initialize(propertiesPanel, PropertiesBody);
            //Data Panel initialize
            DataPropPanel = new PropertiesPanel_Item();
            DataPanel_Initialize(propertiesPanel, PropertiesBody);
            DataPropPanel.Panel.Resize += new EventHandler(delegate (object o, EventArgs e)
            {
                //Resize event -> 26 is minimized box
                if (DataPropPanel.Height != 26)
                {
                    DataPropPanel.Height = 180;
                }
            });
            //Lib Panel initialize
            LibPropPanel = new PropertiesPanel_Item();
            LibPanel_Initialize(propertiesPanel, PropertiesBody);
            LibPropPanel.Panel.Resize += new EventHandler(delegate (object o, EventArgs e)
            {
                //Resize event -> 26 is minimized box
                if (LibPropPanel.Height != 26)
                {
                    LibPropPanel.Height = 50;
                }
            });
            //SpotDet Panel initialize
            SpotDetPropPanel = new PropertiesPanel_Item();
            SpotDetPanel_Initialize(propertiesPanel, PropertiesBody);
        }
        private void Control_MouseOver(object sender, EventArgs e)
        {
            //ToolTip with information for the user
            var ctr = (Control)sender;
            TurnOnToolTip.SetToolTip(ctr, ctr.Text.ToString());
        }
        public void BackColor(Color color)
        {
            //format background color
            LibPropPanel.BackColor(color);
            DataPropPanel.BackColor(color);
            HistogramPropPanel.BackColor(color);
            tresholdsPropPanel.BackColor(color);
            SpotDetPropPanel.BackColor(color);
        }
        public void ForeColor(Color color)
        {
            //format fore color
            LibPropPanel.ForeColor(color);
            DataPropPanel.ForeColor(color);
            HistogramPropPanel.ForeColor(color);
            tresholdsPropPanel.ForeColor(color);
            SpotDetPropPanel.ForeColor(color);
        }
        public void TitleColor(Color color)
        {
            //format title color
            LibPropPanel.TitleColor(color);
            DataPropPanel.TitleColor(color);
            HistogramPropPanel.TitleColor(color);
            tresholdsPropPanel.TitleColor(color);
            SpotDetPropPanel.TitleColor(color);
        }
        private void SpotDetPanel_Initialize(Panel propertiesPanel, Panel PropertiesBody)
        {
            //PropPanel properties
            SpotDetPropPanel.Initialize(propertiesPanel);
            SpotDetPropPanel.Resizable = false;
            SpotDetPropPanel.Name.Text = "Spot Detector";
            PropertiesBody.Controls.Add(SpotDetPropPanel.Panel);

            SpotDetPanel = SpotDetPropPanel.Panel;

            SpotDetPanel.Visible = false;

            SpotDet = new SpotDetector(SpotDetPanel, IA);
        }
        private void LibPanel_Initialize(Panel propertiesPanel, Panel PropertiesBody)
        {
            //PropPanel properties
            LibPropPanel.Initialize(propertiesPanel);
            LibPropPanel.Resizable = false;
            LibPropPanel.Name.Text = "Auto Processing";
            PropertiesBody.Controls.Add(LibPropPanel.Panel);

            LibPanel = LibPropPanel.Panel;

            //LibPanel.Visible = false;
            //items
            Label LibName = new Label();
            LibName.Text = "Protocol:";
            LibName.Width = TextRenderer.MeasureText(LibName.Text, LibName.Font).Width;
            LibName.Location = new Point(5, 30);
            LibPanel.Controls.Add(LibName);
            LibName.BringToFront();

            ComboBox LibTB = ProtocolCBox;
            LibTB.Text = "None";
            LibTB.Items.Add("None");
            int w = LibPanel.Width - 126;
            if (w < 20) { w = 20; }
            LibTB.Width = w;
            LibTB.Width = 150;
            LibTB.Location = new Point(80, 26);
            LibPanel.Controls.Add(LibTB);
            LibTB.BringToFront();
            LibTB.MouseHover += Control_MouseOver;

            LibTB.DropDownStyle = ComboBoxStyle.DropDownList;
            LibTB.SelectedIndex = 0;
            LibTB.AutoSize = false;


            LibPanel.Resize += new EventHandler(delegate (object o, EventArgs a)
            {
                int x1 = LibPanel.Width - 126;
                if (x1 < 20) { x1 = 20; }
                LibTB.Width = x1;
            });
            //Add button on top
            {
                Button btn = new Button();
                btn.Width = 21;
                btn.Height = 21;
                btn.FlatStyle = FlatStyle.Flat;
                btn.BackColor = IA.FileBrowser.BackGround2Color1;
                btn.ForeColor = IA.FileBrowser.ShriftColor1;
                btn.Image = new Bitmap(Properties.Resources.settings, new Size(18, 18));
                btn.FlatAppearance.BorderSize = 0;
                btn.Text = "";
                LibPanel.Controls.Add(btn);
                btn.BringToFront();
                btn.Location = new Point(LibPanel.Width - 25, 26);
                btn.Anchor = AnchorStyles.Right | AnchorStyles.Top;
                btn.MouseHover += new EventHandler(delegate (object o, EventArgs e)
                {
                    TurnOnToolTip.SetToolTip(btn, "Edit protocol");
                });
                AutoSetUp = new AutoApplySettingsClass(btn, LibTB, IA);
            }
            //Apply Protocol
            {

                Button btn = AutoBtn;
                btn.Width = 21;
                btn.Height = 21;
                btn.FlatStyle = FlatStyle.Flat;
                btn.BackColor = IA.FileBrowser.BackGround2Color1;
                btn.ForeColor = IA.FileBrowser.ShriftColor1;
                btn.Image = new Bitmap(Properties.Resources.CheckMarkTrack, new Size(20, 20));
                btn.FlatAppearance.BorderSize = 0;
                btn.Text = "";
                LibPanel.Controls.Add(btn);
                btn.BringToFront();
                btn.Location = new Point(LibPanel.Width - 46, 26);
                btn.Anchor = AnchorStyles.Right | AnchorStyles.Top;
                btn.MouseHover += new EventHandler(delegate (object o, EventArgs e)
                {
                    TurnOnToolTip.SetToolTip(btn, "Apply protocol");
                });
                btn.Click += AutoSetUp.LibBtn_Click;
            }


        }
        private void DataPanel_Initialize(Panel propertiesPanel, Panel PropertiesBody)
        {
            #region Filters Libs
            MyFilters = new Filters(IA);
            Watershed = new MyWatershed(IA);
            #endregion Filters Libs

            //PropPanel properties
            DataPropPanel.Initialize(propertiesPanel);
            DataPropPanel.Resizable = false;
            DataPropPanel.Name.Text = "Filters";
            PropertiesBody.Controls.Add(DataPropPanel.Panel);

            DataPanel = DataPropPanel.Panel;

            DataPanel.Visible = false;
            DataPanel.Controls.Add((Panel)MyFilters);
            MyFilters.BringToFront();
            /*
            //items
            {
                Label LibName = new Label();
                LibName.Text = "Method:";
                LibName.Width = TextRenderer.MeasureText(LibName.Text, LibName.Font).Width;
                LibName.Location = new Point(7, 30);
                DataPanel.Controls.Add(LibName);
                LibName.BringToFront();
                int Y = 26;
                int W = DataPanel.Width - 105;
                int H = 26;
                
                #region Identity button
                Button btn = ConvolutionMethodBtn_addBtn( DataPanel, Y);
                btn.Text = "Reset";
                btn.Tag = 0;
                btn.Width = W;
                Y += H;
                btn.MouseHover += Control_MouseOver;
                btn.Click += DataSourceCBox_SelectedIndexChange;
                #endregion Identity button
                #region Sharpen button
                btn = ConvolutionMethodBtn_addBtn(DataPanel, Y);
                btn.Text = "Sharpen";
                btn.Tag = 1;
                btn.Width = W;
                Y += H;
                btn.MouseHover += Control_MouseOver;
                btn.Click += DataSourceCBox_SelectedIndexChange;
                #endregion Sharpen button
                #region Box blur 3x3 button
                btn = ConvolutionMethodBtn_addBtn(DataPanel, Y);
                btn.Text = "3x3 Box blur";
                btn.Tag = 2;
                btn.Width = W;
                Y += H;
                btn.MouseHover += Control_MouseOver;
                btn.Click += DataSourceCBox_SelectedIndexChange;
                #endregion Box blur 3x3 button
                #region Box blur 5x5 button
                btn = ConvolutionMethodBtn_addBtn(DataPanel, Y);
                btn.Text = "5x5 Box blur";
                btn.Tag = 3;
                btn.Width = W;
                Y += H;
                btn.MouseHover += Control_MouseOver;
                btn.Click += DataSourceCBox_SelectedIndexChange;
                #endregion Box blur 5x5 button
                #region Gaussian blur 3x3 button
                btn = ConvolutionMethodBtn_addBtn(DataPanel, Y);
                btn.Text = "3x3 Gaussian blur";
                btn.Tag = 4;
                btn.Width = W;
                Y += H;
                btn.MouseHover += Control_MouseOver;
                btn.Click += DataSourceCBox_SelectedIndexChange;
                #endregion Gaussian blur 3x3 button
                #region Gaussian blur 5x5 button
                btn = ConvolutionMethodBtn_addBtn(DataPanel, Y);
                btn.Text = "5x5 Gaussian blur";
                btn.Tag = 5;
                btn.Width = W;
                Y += H;
                btn.MouseHover += Control_MouseOver;
                btn.Click += DataSourceCBox_SelectedIndexChange;
                #endregion Gaussian blur 5x5 button
                #region Unsharp masking 5x5 button
                btn = ConvolutionMethodBtn_addBtn(DataPanel, Y);
                btn.Text = "5x5 Unsharp masking";
                btn.Tag = 6;
                btn.Width = W;
                Y += H;
                btn.MouseHover += Control_MouseOver;
                btn.Click += DataSourceCBox_SelectedIndexChange;
                #endregion Unsharp masking 5x5 button
                #region Edge detection button
                btn = ConvolutionMethodBtn_addBtn(DataPanel, Y);
                btn.Text = "Edge detection";
                btn.Tag = 7; 
                btn.Width = W;
                Y += H;
                btn.MouseHover += Control_MouseOver;
                btn.Click += DataSourceCBox_SelectedIndexChange;
                #endregion Edge detection button
                #region Watershed
                btn = ConvolutionMethodBtn_addBtn(DataPanel, Y);
                btn.Text = "Watershed";
                btn.Width = W;
                Y += H;
                btn.MouseHover += Control_MouseOver;
                btn.Click += Watershed.StartDialog;
                #endregion Watershed
            }
            */
        }
        private Button ConvolutionMethodBtn_addBtn(Panel p, int Y)
        {
            Button btn = new Button();
            int X = 80;
            btn.FlatStyle = FlatStyle.Standard;
            btn.BackColor = SystemColors.ButtonFace;
            btn.ForeColor = Color.Black;
            p.Controls.Add(btn);
            btn.BringToFront();
            btn.Location = new Point(X, Y);
            btn.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            return btn;
        }

        private void DataSourceCBox_SelectedIndexChange(object sender, EventArgs e)
        {
            TifFileInfo fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;

            if (fi.available == false)
            {
                MessageBox.Show("Image is not ready yet! \nTry again later.");
                return;
            }

            int selectedInd = (int)((Button)sender).Tag;

            ApplyFilter(fi, selectedInd);
        }
        public BackgroundWorker ApplyFilter(TifFileInfo fi, int selectedInd)
        {
            var bgw = new BackgroundWorker();
            bgw.WorkerReportsProgress = true;

            //apply to fi
            int oldSelectedInd = fi.DataSourceInd;
            fi.DataSourceInd = selectedInd;

            //restore image if there was an watershed
            if (fi.watershedList.Count > 0)
            {
                fi.FilterHistory.Clear();
                fi.watershedList.Clear();

                switch (fi.bitsPerPixel)
                {
                    case 8:
                        fi.image8bitFilter = fi.image8bit;
                        break;
                    case 16:
                        fi.image16bitFilter = fi.image16bit;
                        break;
                }
            }

            if (selectedInd == 0)
                fi.FilterHistory.Clear();
            else
                fi.FilterHistory.Add(selectedInd);

            fi.available = false;

            bgw.DoWork += new DoWorkEventHandler(delegate (Object o, DoWorkEventArgs a)
            {
                try
                {
                    switch (selectedInd)
                    {
                        case 0:
                            switch (fi.bitsPerPixel)
                            {
                                case 8:
                                    fi.image8bitFilter = fi.image8bit;
                                    break;
                                case 16:
                                    fi.image16bitFilter = fi.image16bit;
                                    break;
                            }
                            break;
                        case 7:
                            detectEdges(fi);
                            break;
                        default:
                            SmoothImage(selectedInd, fi);
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

        private void HistogramPanel_Initialize(Panel propertiesPanel, Panel PropertiesBody)
        {

            //PropPanel properties
            HistogramPropPanel.Initialize(propertiesPanel);
            HistogramPropPanel.Resizable = true;
            HistogramPropPanel.Name.Text = "Histogram";
            PropertiesBody.Controls.Add(HistogramPropPanel.Panel);

            HistogramPanel = HistogramPropPanel.Panel;

            HistogramPanel.Visible = false;

            //Chart
            Chart1.CA.BackGroundColor = IA.FileBrowser.BackGroundColor1;
            Chart1.ForeColor = IA.FileBrowser.ShriftColor1;
            //Chart series
            Values.Enabled = true;
            Values.UseGradientStyle = true;
            Values.BorderColor = Color.White;
            Values.BackSecondaryColor = Color.White;
            Values.Color = Color.Black;
            Chart1.Series.Add(Values);

            //1DOtsu colors btns
            for (int i = 0; i < Otsu1dSeries.Length; i++)
            {
                var ser = new BrightnessAndContrast_ChartPanel.BrightnessAndContrast_Chart.Series();
                ser.Enabled = true;
                Values.UseGradientStyle = false;
                ser.BorderColor = Color.White;
                ser.Color = Color.White;
                Chart1.Series.Add(ser);
                Otsu1dSeries[i] = ser;
            }
            //Spot Detector
            {
                var ser = new BrightnessAndContrast_ChartPanel.BrightnessAndContrast_Chart.Series();
                ser.Enabled = true;
                Values.UseGradientStyle = false;
                ser.BorderColor = Color.White;
                ser.Color = Color.White;
                Chart1.Series.Add(ser);
                Spots = ser;
            }

            Chart1.Dock = DockStyle.Fill;
            Chart1.BackColor = HistogramPropPanel.Body.BackColor;
            //HistogramPropPanel.Panel.Controls.Add(Chart1);

            this.FormSegmentation = new Form_auxiliary(this.HistogramPropPanel.Body, 10, 30, -10, -40);
            this.FormSegmentation.Controls.Add(Chart1);
            //this.FormSegmentation.Show();

            Chart1.BringToFront();
        }
        private void tresholdsPanel_Initialize(Panel propertiesPanel, Panel PropertiesBody)
        {
            //PropPanel properties
            tresholdsPropPanel.Initialize(propertiesPanel);
            tresholdsPropPanel.Resizable = false;
            tresholdsPropPanel.Name.Text = "Segmentation";
            PropertiesBody.Controls.Add(tresholdsPropPanel.Panel);

            tresholdsPanel = tresholdsPropPanel.Panel;

            tresholdsPanel.Visible = false;

            //add method
            Panel p = new Panel();
            p.Dock = DockStyle.Top;
            p.Height = 30;
            tresholdsPanel.Controls.Add(p);
            p.BringToFront();

            Label LibName = new Label();
            LibName.Text = "Method:";
            LibName.Width = TextRenderer.MeasureText(LibName.Text, LibName.Font).Width;
            LibName.Location = new Point(5, 9);
            p.Controls.Add(LibName);
            LibName.BringToFront();

            ComboBox LibTB = SegmentationCBox;
            LibTB.Text = "None";
            LibTB.Items.Add("None");

            int w = tresholdsPanel.Width - 105;
            if (w < 20) { w = 20; }
            LibTB.Width = w;
            LibTB.Width = 150;
            LibTB.Location = new Point(80, 6);
            p.Controls.Add(LibTB);
            LibTB.BringToFront();
            LibTB.MouseHover += Control_MouseOver;

            LibTB.DropDownStyle = ComboBoxStyle.DropDownList;
            LibTB.SelectedIndex = 0;
            LibTB.AutoSize = false;

            LibTB.SelectedIndexChanged += SegmentationCBox_SelectedIndexChange;

            tresholdsPanel.Resize += new EventHandler(delegate (object o, EventArgs a)
            {
                int x1 = tresholdsPanel.Width - 105;
                if (x1 < 20) { x1 = 20; }
                LibTB.Width = x1;
            });

            #region Types of segmentation
            //Add Otsu segmentation
            //LibTB.Items.Add("1D Maximum Between-class Variance");
            LibTB.Items.Add("Otsu Thresholding");
            Otsu1D = new OtsuSegmentation(tresholdsPanel, IA);
            LibTB.Items.Add("K-Means");
            Kmeans = new KMeansSegmentation(IA);
            #endregion Types of segmentation
        }

        #endregion Initializing Componends

        #region Segmentation
        public void SegmentationCBox_SelectedIndexChange(object sender, EventArgs e)
        {
            //apply to fi
            TifFileInfo fi = null;
            try
            {
                fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
            }
            catch { return; }
            if (fi == null) return;
            //apply to history
            if (((ComboBox)sender).Focused == true &
                fi.SegmentationCBoxIndex[fi.cValue] != SegmentationCBox.SelectedIndex)
            {
                #region apply to history
                fi.delHist = true;
                IA.delHist = true;
                IA.UnDoBtn.Enabled = true;
                IA.DeleteFromHistory();
                fi.History.Add("segmentation.ChangeMethod(" + fi.cValue.ToString() + ","
                    + fi.SegmentationCBoxIndex[fi.cValue].ToString() + ")");
                fi.History.Add("segmentation.ChangeMethod(" + fi.cValue.ToString() + ","
                    + SegmentationCBox.SelectedIndex.ToString() + ")");
                IA.UpdateUndoBtns();
                IA.MarkAsNotSaved();
                #endregion apply to history
            }
            //set value
            fi.SegmentationCBoxIndex[fi.cValue] = SegmentationCBox.SelectedIndex;
            //hide panels
            Otsu1D.panel.Visible = false;
            //Show panels
            if (IA.settings.SegmentTreshPanelVis[IA.TabPages.ActiveAccountIndex] != "y")
            {
                tresholdsPanel.Height = 26;
            }
            else
            {
                switch (SegmentationCBox.SelectedIndex)
                {
                    case 1:
                        Otsu1D.sumHistogramsCheckBox.Visible = true;
                        tresholdsPanel.Height = 56 + Otsu1D.panel.Height;
                        Otsu1D.panel.Visible = true;
                        break;
                    case 2:
                        Otsu1D.sumHistogramsCheckBox.Visible = true;
                        tresholdsPanel.Height = 56 + Otsu1D.panel.Height;
                        Otsu1D.panel.Visible = true;
                        break;
                    default:
                        tresholdsPanel.Height = 56;
                        Otsu1D.panel.Visible = false;
                        break;
                }
            }
            if (((ComboBox)sender).Focused == true)
            {
                IA.ReloadImages();
            }
        }
        #endregion Segmentation

        #region Smooth the image
        //source https://en.wikipedia.org/wiki/Kernel_(image_processing)
        public void SmoothImage(int method, TifFileInfo fi)
        {
            //choose kernel table
            int[][] kernel = KernelMatrix(method);
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
                    byte[][][] image8bit = SmoothAllStack8bit(ImagePxlMatr, fi, pxlVals, coeficient);
                    fi.image8bitFilter = image8bit;
                    break;
                case 16:
                    ushort[][][] image16bit = SmoothAllStack16bit(ImagePxlMatr, fi, pxlVals, coeficient);
                    fi.image16bitFilter = image16bit;
                    break;
            }
        }
        private ushort[][][] SmoothAllStack16bit(Point[][][] ImagePxlMatr, TifFileInfo fi, int[] pxlVals, double coeficient)
        {
            ushort[][][] image = fi.image16bitFilter;//source image
            ushort[][][] newImage = new ushort[image.Length][][]; //target image
            ushort maxVal = ushort.MaxValue; //16 bit image max intensity
            double MinValue = ushort.MinValue; //0

            Parallel.For(0, image.Length, frame =>
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
                newImage[frame] = selectedImage;
            });
            //return result image
            return newImage;
        }
        private byte[][][] SmoothAllStack8bit(Point[][][] ImagePxlMatr, TifFileInfo fi, int[] pxlVals, double coeficient)
        {
            byte[][][] image = fi.image8bitFilter;//source image
            byte[][][] newImage = new byte[image.Length][][];//target image
            double maxVal = byte.MaxValue - 2;//255
            double MinValue = byte.MinValue;//0

            Parallel.For(0, image.Length, frame =>
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
                newImage[frame] = selectedImage;
            });
            //return result image
            return newImage;
        }

        private Point[][][] ImagePxlMatrix(Point[] pxlCords, TifFileInfo fi, int[][] kernel)
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
        private int[] KernelPxlVal(int[][] kernel)
        {
            List<int> val = new List<int>();
            //check the table for non zero values and apply them to array
            for (int y = 0; y < kernel.Length; y++)
                for (int x = 0; x < kernel[y].Length; x++)
                    if (kernel[y][x] != 0)
                        val.Add(kernel[y][x]);

            int[] pxlVal = val.ToArray();

            return pxlVal;
        }
        private Point[] KernelPxlCord(int[][] kernel)
        {
            List<Point> pixels = new List<Point>();
            int index = -(kernel.Length - 1) / 2;
            //check the table for non zero values and calculate the coordinates
            for (int y = 0; y < kernel.Length; y++)
                for (int x = 0; x < kernel[y].Length; x++)
                    if (kernel[y][x] != 0)
                        pixels.Add(new Point(x + index, y + index));

            Point[] pxlArray = pixels.ToArray();

            return pxlArray;
        }

        private int KernelCoeficient(int[][] kernel)
        {
            int sum = 0;
            //calculate coefition = sum(all values in the table)
            foreach (int[] row in kernel)
                foreach (int val in row)
                    sum += val;

            return sum;
        }
        private int[][] KernelMatrix(int method)
        {
            //calculates the kernel
            int[][] kernel = null;
            switch (method)
            {
                case 0:
                    //Identity
                    kernel = new int[3][];
                    kernel[0] = new int[] { 0, 0, 0 };
                    kernel[1] = new int[] { 0, 1, 0 };
                    kernel[2] = new int[] { 0, 0, 0 };
                    break;
                case 1:
                    //Sharpen
                    kernel = new int[3][];
                    kernel[0] = new int[] { 0, -1, 0 };
                    kernel[1] = new int[] { -1, 5, -1 };
                    kernel[2] = new int[] { 0, -1, 0 };
                    break;
                case 2:
                    //Box blur 3x3
                    kernel = new int[3][];
                    kernel[0] = new int[] { 1, 1, 1 };
                    kernel[1] = new int[] { 1, 1, 1 };
                    kernel[2] = new int[] { 1, 1, 1 };
                    break;
                case 3:
                    //Box blur 5x5
                    kernel = new int[5][];
                    kernel[0] = new int[] { 1, 1, 1, 1, 1 };
                    kernel[1] = new int[] { 1, 1, 1, 1, 1 };
                    kernel[2] = new int[] { 1, 1, 1, 1, 1 };
                    kernel[3] = new int[] { 1, 1, 1, 1, 1 };
                    kernel[4] = new int[] { 1, 1, 1, 1, 1 };
                    break;
                case 4:
                    //Gaussian blur 3x3
                    kernel = new int[3][];
                    kernel[0] = new int[] { 1, 2, 1 };
                    kernel[1] = new int[] { 2, 4, 2 };
                    kernel[2] = new int[] { 1, 2, 1 };
                    break;
                case 5:
                    //Gaussian blur 5x5
                    kernel = new int[5][];
                    kernel[0] = new int[] { 1, 4, 6, 4, 1 };
                    kernel[1] = new int[] { 4, 16, 24, 16, 4 };
                    kernel[2] = new int[] { 6, 24, 36, 24, 6 };
                    kernel[3] = new int[] { 4, 16, 24, 16, 4 };
                    kernel[4] = new int[] { 1, 4, 6, 4, 1 };
                    break;
                case 6:
                    //Unsharp masking
                    kernel = new int[5][];
                    kernel[0] = new int[] { 1, 4, 6, 4, 1 };
                    kernel[1] = new int[] { 4, 16, 24, 16, 4 };
                    kernel[2] = new int[] { 6, 24, -476, 24, 6 };
                    kernel[3] = new int[] { 4, 16, 24, 16, 4 };
                    kernel[4] = new int[] { 1, 4, 6, 4, 1 };
                    break;
                case 7:
                    //Edge detection bottom
                    kernel = new int[3][];
                    kernel[0] = new int[] { 1, 2, 1 };
                    kernel[1] = new int[] { 0, 0, 0 };
                    kernel[2] = new int[] { -1, -2, -1 };
                    break;
                case 8:
                    //Edge detection right
                    kernel = new int[3][];
                    kernel[0] = new int[] { -1, 0, 1 };
                    kernel[1] = new int[] { -2, 0, 2 };
                    kernel[2] = new int[] { -1, 0, 1 };
                    break;
            }
            return kernel;
        }
        #endregion Smooth the image

        #region Edge detection
        public void detectEdges(TifFileInfo fi)
        {
            //choose kernel table
            int[][] kernelTopDown = KernelMatrix(7);
            int[][] kernelLeftRight = KernelMatrix(8);

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
                        ImagePxlMatrTopDown, ImagePxlMatrLeftRight, fi,
                        pxlValsTopDown, pxlValsLeftRight);
                    fi.image8bitFilter = image8bit;
                    break;
                case 16:
                    ushort[][][] image16bit = detectEdgesAllStack16bit(
                        ImagePxlMatrTopDown, ImagePxlMatrLeftRight, fi,
                        pxlValsTopDown, pxlValsLeftRight);
                    fi.image16bitFilter = image16bit;
                    break;
            }

        }
        private byte[][][] detectEdgesAllStack8bit(Point[][][] ImagePxlMatrTopDown, Point[][][] ImagePxlMatrLeftRight,
            TifFileInfo fi, int[] pxlValsTopDown, int[] pxlValsLeftRight)
        {
            byte[][][] image = fi.image8bitFilter;
            byte[][][] newImage = new byte[image.Length][][];
            double maxVal = byte.MaxValue - 2;
            double MinValue = byte.MinValue;
            Parallel.For(0, image.Length, frame =>
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
                newImage[frame] = selectedImage;
            });

            return newImage;
        }
        private ushort[][][] detectEdgesAllStack16bit(Point[][][] ImagePxlMatrTopDown, Point[][][] ImagePxlMatrLeftRight,
            TifFileInfo fi, int[] pxlValsTopDown, int[] pxlValsLeftRight)
        {
            ushort[][][] image = fi.image16bitFilter;
            ushort[][][] newImage = new ushort[image.Length][][];
            ushort maxVal = ushort.MaxValue;
            double MinValue = ushort.MinValue;
            Parallel.For(0, image.Length, frame =>
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
                newImage[frame] = selectedImage;
            });
            return newImage;
        }
        #endregion Edge detection

        #region Histogram
        public void Segmentation_LoadHistogramToChart(TifFileInfo fi)
        {
            if (fi.selectedPictureBoxColumn != 1 && fi.selectedPictureBoxColumn != 2) return;
            TifFileInfo oldFI = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
            if (fi.image16bitFilter == null & fi.image8bitFilter == null) { return; }
            oldFI = fi;
            FrameCalculator FC = new FrameCalculator();
            int frame = FC.Frame(fi);
            if (fi.openedImages < frame) { return; }

            //create histogram array
            int[] histArray = null;
            switch (fi.bitsPerPixel)
            {
                case 8:
                    histArray = calculateHistogram8bit(fi, frame);
                    break;
                case 16:
                    histArray = calculateHistogram16bit(fi, frame);
                    break;
            }
            //load to chart
            loadHistogramArray(fi, histArray);
        }
        private void loadHistogramArray(TifFileInfo fi, int[] histArray)
        {
            //find range
            int MaxBrightness = fi.MaxBrightness[fi.cValue];
            for (int i = histArray.Length - 1; i > 0; i--)
            {
                if (histArray[i] > 0) { MaxBrightness = i; break; }
            }
            int correction = Convert.ToInt32(MaxBrightness * 0.2);
            if (correction < 10) { correction = 10; }
            int range = MaxBrightness;
            int step = 1;
            if (range > byte.MaxValue)
            {
                step = range / byte.MaxValue;
            }
            //add values
            if (Values.Points.Count > 0) { Values.Points.Clear(); }
            //1dOtsu
            int[] otsu1dColorList = new int[6];
            for (int i = 0; i < Otsu1dSeries.Length; i++)
            {
                var ser = Otsu1dSeries[i];
                if (ser.Points.Count > 0)
                {
                    ser.Points.Clear();
                }

                ser.Color = fi.thresholdColors[fi.cValue][i];

                if (i <= fi.thresholds[fi.cValue])
                {
                    otsu1dColorList[i] = fi.thresholdValues[fi.cValue][i];
                    if (i == 4) { otsu1dColorList[5] = ushort.MaxValue; }
                }
                else if (i == fi.thresholds[fi.cValue] + 1)
                    otsu1dColorList[i] = ushort.MaxValue;

                if (fi.SegmentationCBoxIndex[fi.cValue] == 1 ||
                    fi.SegmentationCBoxIndex[fi.cValue] == 2)
                    ser.Enabled = true;
                else
                    ser.Enabled = false;
            }
            //calculate spot detector diapasone
            int[] SpotDiapason = IA.Segmentation.SpotDet.CalculateBorders(fi, fi.cValue);
            if (Spots.Points.Count > 0)
                Spots.Points.Clear();

            Spots.Color = fi.SpotColor[fi.cValue];

            if (fi.SelectedSpotThresh[fi.cValue] != 0)
                Spots.Enabled = true;
            else
                Spots.Enabled = false;

            //Calculations
            int length = histArray.Length;
            for (int i = 0; i <= MaxBrightness; i += step)
            {
                int val = 0;

                for (int j = i; j < i + step; j++)
                {
                    if (0 <= j & j < length)
                    {
                        val += histArray[j];
                    }
                }

                Values.Points.AddXY(i, val);

                //1dOtsu
                for (int z = 0; z < Otsu1dSeries.Length; z++)
                {
                    var ser = Otsu1dSeries[z];
                    if (i <= otsu1dColorList[z + 1] && i >= otsu1dColorList[z]
                        && ser.Color != Color.Transparent)
                        ser.Points.AddXY(i, val);
                }
                //spot detector
                if (i <= SpotDiapason[1] && i >= SpotDiapason[0]
                        && Spots.Color != Color.Transparent)
                    Spots.Points.AddXY(i, val);
            }

            //Color
            Values.BackSecondaryColor = fi.LutList[fi.cValue];
            Values.UseGradientStyle = true;
            Chart1.DrawToScreen(fi);
        }

        private int[] calculateHistogram8bit(TifFileInfo fi, int frame)
        {
            int[] array = new int[byte.MaxValue + 1];
            try
            {
                int cVal = fi.cValue;

                foreach (byte[] row in fi.image8bitFilter[frame])
                    foreach (byte val in row)
                        array[val] += 1;

            }
            catch { }

            return array;
        }
        private int[] calculateHistogram16bit(TifFileInfo fi, int frame)
        {
            int[] array = new int[ushort.MaxValue + 1];
            try
            {
                int cVal = fi.cValue;

                foreach (ushort[] row in fi.image16bitFilter[frame])
                    foreach (ushort val in row)
                        array[val] += 1;

            }
            catch { }

            return array;
        }
        #endregion Histogram
    }
}