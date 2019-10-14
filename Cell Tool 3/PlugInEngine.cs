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
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Windows.Forms;
using CellToolDK;
using System.ComponentModel;
using System.Drawing;

namespace Cell_Tool_3
{
    class PlugInEngine
    {
        private ToolStripMenuItem DeveloperToolStripMenuItem;
        private ImageAnalyser IA;  
        public PlugInEngine(ToolStripMenuItem DeveloperToolStripMenuItem, ImageAnalyser IA)
        {
            this.DeveloperToolStripMenuItem = DeveloperToolStripMenuItem;
            this.IA = IA;
            
            PlugIns_TakeNames();
        }
        private void UnInstallPlugIn_Click(object sender, EventArgs e)
        {
            Form msgForm = new Form();
 
            msgForm.Height = 600;
            msgForm.Width = 600;
            msgForm.Icon = Properties.Resources.CT_done;
            msgForm.Text = "Uninstall PlugIns";

            TreeView rtb = new TreeView();
            rtb.CheckBoxes = true;
            rtb.ShowLines = false;
            rtb.Dock = DockStyle.Fill;
            //Fill the box with names
            string path = OSStringConverter.StringToDir(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\CellToolPlugIns");
            bool unisntalled = false;

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            DirectoryInfo directoryInfo = new DirectoryInfo(path);

            foreach (var file in directoryInfo.GetFiles())
            {
                if (file.Name.EndsWith(".CTPlugIn.dll") &&
                    !file.Name.EndsWith("Results_Extractor.CTPlugIn.dll"))
                {
                    string namePlugIn = file.Name.Replace(".CTPlugIn.dll", "").Replace("_", " ");
                    string dirPlugIn = file.FullName;
                    TreeNode n = new TreeNode(namePlugIn);
                    n.Checked = false;
                    n.Tag = dirPlugIn;
                    rtb.Nodes.Add(n);
                }
            }
            //end filling
            msgForm.Controls.Add(rtb);

            Panel okBox = new Panel();
            okBox.Height = 40;
            okBox.Dock = DockStyle.Bottom;
            msgForm.Controls.Add(okBox);

            Button okBtn = new Button();
            okBtn.Text = "Uninstall";
            okBtn.BackColor = System.Drawing.SystemColors.ButtonFace;
            okBtn.ForeColor = System.Drawing.Color.Black;
            okBtn.Location = new System.Drawing.Point(20, 10);
            okBtn.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            okBox.Controls.Add(okBtn);

            Button cancelBtn = new Button();
            cancelBtn.Text = "Cancel";
            cancelBtn.BackColor = System.Drawing.SystemColors.ButtonFace;
            cancelBtn.Location = new System.Drawing.Point(msgForm.Width - cancelBtn.Width - 40, 10);
            cancelBtn.ForeColor = System.Drawing.Color.Black;
            cancelBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            okBox.Controls.Add(cancelBtn);

            okBtn.Click += new EventHandler(delegate (object o, EventArgs a)
            {
                foreach(TreeNode n in rtb.Nodes)
                if(n.Checked && File.Exists((string)n.Tag))
                {
                        try
                        {
                            File.Delete((string)n.Tag);
                            unisntalled = true;
                        }
                        catch
                        {
                            MessageBox.Show(
                                "To uninstall the PlugIn, restart the program and try again!");
                        }
                }

                msgForm.Close();
            });

            cancelBtn.Click += new EventHandler(delegate (object o, EventArgs a)
            {
                msgForm.Close();
            });

            // Linux change
            IA.FileBrowser.StatusLabel.Text = "Dialog open";
            msgForm.ShowDialog();
            IA.FileBrowser.StatusLabel.Text = "Ready";

            if (unisntalled)
            {
                PlugIns_TakeNames();
                MessageBox.Show("Selected plugins are uninstalled.");
            }
            msgForm.Dispose();
        }
        private void PlugIns_TakeNames()
        {
            DeveloperToolStripMenuItem.DropDownItems.Clear();

            ToolStripMenuItem InstallPlugInTS = new ToolStripMenuItem();
            InstallPlugInTS.Text = "Install PlugIn";
            InstallPlugInTS.Click += InstallPlugIn_Click;
            DeveloperToolStripMenuItem.DropDownItems.Add(InstallPlugInTS);

            ToolStripMenuItem UnInstallPlugInTS = new ToolStripMenuItem();
            UnInstallPlugInTS.Text = "Uninstall PlugIn";
            UnInstallPlugInTS.Click += UnInstallPlugIn_Click;
            DeveloperToolStripMenuItem.DropDownItems.Add(UnInstallPlugInTS);

            DeveloperToolStripMenuItem.DropDownItems.Add(new ToolStripSeparator());

           // AddResultsExtractor();

            // string path = Application.StartupPath + "\\PlugIns";
            string path = OSStringConverter.StringToDir(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\CellToolPlugIns");


            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            DirectoryInfo directoryInfo = new DirectoryInfo(path);

            foreach (var file in directoryInfo.GetFiles())
            {
                if (file.Name.EndsWith(".CTPlugIn.dll") && 
                    !file.Name.EndsWith("Results_Extractor.CTPlugIn.dll"))
                {
                    string namePlugIn = file.Name.Replace(".CTPlugIn.dll", "").Replace("_", " ");
                    string dirPlugIn = file.FullName;
                    ToolStripMenuItem plugInTS = new ToolStripMenuItem();
                    plugInTS.Text = namePlugIn;
                    plugInTS.Tag = dirPlugIn;
                    plugInTS.Click += PlugInToolStripMenuItem_Click;

                    DeveloperToolStripMenuItem.DropDownItems.Add(plugInTS);
                }
            }
        }
        private void AddResultsExtractor()
        {
            ToolStripMenuItem plugInTS = new ToolStripMenuItem();
            plugInTS.Text = "Results Extractor";
            plugInTS.Click += new EventHandler(delegate(object sender, EventArgs z) 
            {
                TifFileInfo oldFI = null;
                try
                {
                    oldFI = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
                }
                catch { }

                ResultsExtractor resExtr = new ResultsExtractor();

                    
                //resExtr.Input(oldFI,IA);
                    
            });

            DeveloperToolStripMenuItem.DropDownItems.Add(plugInTS);
        }
        private void PlugInToolStripMenuItem_Click(object sender, EventArgs e)
        {

            string dirPlugIn = (string)((ToolStripMenuItem)sender).Tag;
            PlugIn_Load(dirPlugIn);
        }

        private void PlugIn_Load(string path)
        {
            TifFileInfo oldFI = null;
            try
            {
                oldFI = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
            }
            catch { }


            CellToolDK.TifFileInfo fi = FItoCellToolDK(oldFI);

            var DLL = Assembly.LoadFile(path);

            foreach (Type type in DLL.GetExportedTypes())
            {
                try
                {
                    Transmiter e = new Transmiter();

                    e.Changed += new TransmiterEventHandler(delegate (object o, TransmiterEventArgs a)
                    {
                        try
                        {
                            if (fi != null && oldFI != null)
                            {
                                if (fi.sizeC != oldFI.sizeC)
                                {
                                    oldFI.sizeC = fi.sizeC;
                                    FI_reduseC(fi);
                                    FI_reduseC(oldFI);
                                }

                                CellToolDKtoFI(fi, oldFI);
                                
                                if (oldFI.sizeZ > 1)
                                {
                                    IA.TabPages.zTrackBar.Refresh(oldFI.zValue + 1, 1, oldFI.sizeZ);
                                    IA.TabPages.zTrackBar.Panel.Visible = true;
                                }
                                else
                                {
                                    IA.TabPages.zTrackBar.Panel.Visible = false;
                                }

                                if (oldFI.sizeT > 1)
                                {
                                    IA.TabPages.tTrackBar.Refresh(oldFI.frame + 1, 1, oldFI.sizeT);
                                    IA.TabPages.tTrackBar.Panel.Visible = true;
                                }
                                else
                                {
                                    IA.TabPages.tTrackBar.Panel.Visible = false;
                                }
                                 
                            }
                        }
                        catch { MessageBox.Show("Error with reporting back!"); }
                       
                        IA.ReloadImages();
                    });
                    
                    var c = Activator.CreateInstance(type);

                    try
                    {
                        type.InvokeMember("Input", BindingFlags.InvokeMethod, null, c, new object[] { fi, e });
                    }
                    catch { MessageBox.Show("Input void is not avaliable!"); }
                    break;
                }
                catch { }
            }
           
        }
        private void FI_reduseC(CellToolDK.TifFileInfo fi)
        {
            List<Color> l = new List<Color>();

            for (int i = 0; i < fi.sizeC; i++)
                if (i < fi.LutList.Count)
                    l.Add(fi.LutList[i]);
                else
                    l.Add(System.Drawing.Color.White);

            fi.LutList = l;

            fi.cValue = 0;

            #region Segmentation variables
            fi.histogramArray = null;
            fi.adjustedLUT = null;
            fi.MaxBrightness = null;
            fi.MinBrightness = null;
            fi.SegmentationCBoxIndex = new int[fi.sizeC];
            fi.SegmentationProtocol = new int[fi.sizeC];
            fi.thresholdsCBoxIndex = new int[fi.sizeC];
            fi.sumHistogramChecked = new bool[fi.sizeC];
            fi.thresholdValues = new int[fi.sizeC][];
            fi.thresholdColors = new Color[fi.sizeC][];
            fi.RefThresholdColors = new Color[fi.sizeC][];
            fi.thresholds = new int[fi.sizeC];
            fi.SpotColor = new Color[fi.sizeC];
            fi.RefSpotColor = new Color[fi.sizeC];
            fi.SelectedSpotThresh = new int[fi.sizeC];
            fi.SpotThresh = new int[fi.sizeC];
            fi.typeSpotThresh = new int[fi.sizeC];
            fi.SpotTailType = new string[fi.sizeC];
            fi.spotSensitivity = new int[fi.sizeC];
            fi.roiList = new List<CellToolDK.ROI>[fi.sizeC];
            fi.tracking_MaxSize = new int[fi.sizeC];
            fi.tracking_MinSize = new int[fi.sizeC];
            fi.tracking_Speed = new int[fi.sizeC];
            for (int i = 0; i < fi.sizeC; i++)
            {
                fi.sumHistogramChecked[i] = false;
                fi.thresholdValues[i] = new int[5];
                fi.thresholdColors[i] = new Color[]
                { Color.Transparent,Color.Transparent,Color.Transparent,Color.Transparent,Color.Transparent };
                fi.RefThresholdColors[i] = new Color[]
                {Color.Black,Color.Orange,Color.Green,Color.Blue,Color.Magenta};
                fi.SpotColor[i] = Color.Red;
                fi.RefSpotColor[i] = Color.Red;
                fi.SpotTailType[i] = "<";
                fi.spotSensitivity[i] = 100;
                fi.tracking_MaxSize[i] = 10000;
                fi.tracking_MinSize[i] = 5;
                fi.tracking_Speed[i] = 5;
            }
            #endregion Segmentation variables
        }
        private void FI_reduseC(TifFileInfo fi)
        {
            List<Color> l = new List<Color>();

            for (int i = 0; i < fi.sizeC; i++)
                if (i < fi.LutList.Count)
                    l.Add(fi.LutList[i]);
                else
                    l.Add(System.Drawing.Color.White);

            fi.LutList = l;

            fi.cValue = 0;

            #region Segmentation variables
            fi.histogramArray = null;
            fi.adjustedLUT = null;
            fi.MaxBrightness = null;
            fi.MinBrightness = null;
            fi.SegmentationCBoxIndex = new int[fi.sizeC];
            fi.SegmentationProtocol = new int[fi.sizeC];
            fi.thresholdsCBoxIndex = new int[fi.sizeC];
            fi.sumHistogramChecked = new bool[fi.sizeC];
            fi.thresholdValues = new int[fi.sizeC][];
            fi.thresholdColors = new Color[fi.sizeC][];
            fi.RefThresholdColors = new Color[fi.sizeC][];
            fi.thresholds = new int[fi.sizeC];
            fi.SpotColor = new Color[fi.sizeC];
            fi.RefSpotColor = new Color[fi.sizeC];
            fi.SelectedSpotThresh = new int[fi.sizeC];
            fi.SpotThresh = new int[fi.sizeC];
            fi.typeSpotThresh = new int[fi.sizeC];
            fi.SpotTailType = new string[fi.sizeC];
            fi.spotSensitivity = new int[fi.sizeC];
            fi.roiList = new List<ROI>[fi.sizeC];
            fi.tracking_MaxSize = new int[fi.sizeC];
            fi.tracking_MinSize = new int[fi.sizeC];
            fi.tracking_Speed = new int[fi.sizeC];
            for (int i = 0; i < fi.sizeC; i++)
            {
                fi.sumHistogramChecked[i] = false;
                fi.thresholdValues[i] = new int[5];
                fi.thresholdColors[i] = new Color[]
                { Color.Transparent,Color.Transparent,Color.Transparent,Color.Transparent,Color.Transparent };
                fi.RefThresholdColors[i] = new Color[]
                {Color.Black,Color.Orange,Color.Green,Color.Blue,Color.Magenta};
                fi.SpotColor[i] = Color.Red;
                fi.RefSpotColor[i] = Color.Red;
                fi.SpotTailType[i] = "<";
                fi.spotSensitivity[i] = 100;
                fi.tracking_MaxSize[i] = 10000;
                fi.tracking_MinSize[i] = 5;
                fi.tracking_Speed[i] = 5;
            }
            #endregion Segmentation variables

            fi.tpTaskbar.AddColorBtn();
            fi.tpTaskbar.VisualizeColorBtns();
        }
        private void InstallPlugIn_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            string formatMiniStr = ".CTPlugIn.dll";
            string formatStr = formatMiniStr.Substring(1, formatMiniStr.Length - 1) +
                " files (*" + formatMiniStr + ")|*" + formatMiniStr;

            ofd.Filter = formatStr;
            ofd.FilterIndex = 1;
            ofd.RestoreDirectory = true;
            ofd.Title = "Install PlugIn:";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                InstallPlugIn(ofd.FileName);
            }
        }
        public void InstallPlugIn(string path)
        { 
            path = OSStringConverter.GetWinString(path);
            //background worker
            var bgw = new BackgroundWorker();
            bgw.WorkerReportsProgress = true;
            //Add event for projection here
            bgw.DoWork += new DoWorkEventHandler(delegate (Object o, DoWorkEventArgs a)
            {
                string newPath = OSStringConverter.StringToDir(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\CellToolPlugIns\\" +
                path.Substring(path.LastIndexOf("\\") + 1, path.Length - path.LastIndexOf("\\") - 1));
                if (File.Exists(newPath))
                    ((BackgroundWorker)o).ReportProgress(1);
                else {
                    File.Copy(path, newPath, false);
                    ((BackgroundWorker)o).ReportProgress(0);
                }
            });

            bgw.ProgressChanged += new ProgressChangedEventHandler(delegate (Object o, ProgressChangedEventArgs a)
                        {
                            if (a.ProgressPercentage == 0)
                            {
                                PlugIns_TakeNames();
                                MessageBox.Show("Plugin installed!");
                            }
                            else if (a.ProgressPercentage == 1)
                            {
                                MessageBox.Show("Plugin already installed!"
                                    );
                            }
                            IA.FileBrowser.StatusLabel.Text = "Ready";
                        });
            //Start background worker
            IA.FileBrowser.StatusLabel.Text = "Installing PlugIn...";
            //start bgw
            bgw.RunWorkerAsync();



        }
        private CellToolDK.TifFileInfo FItoCellToolDK(TifFileInfo fi)
        {
            if (fi == null) return null;
            CellToolDK.TifFileInfo newFI = new CellToolDK.TifFileInfo();

            newFI.zoom = fi.zoom;
            newFI.Xposition = fi.Xposition;
            newFI.Yposition = fi.Yposition;
            //Chart
            newFI.xAxisTB = fi.xAxisTB;
            newFI.yAxisTB = fi.yAxisTB;
            
            //Tracking
            newFI.tracking_MaxSize = fi.tracking_MaxSize;
            newFI.tracking_MinSize = fi.tracking_MinSize;
            newFI.tracking_Speed = fi.tracking_Speed;
            /// <summary>
            /// Segmentation
            /// </summary>
            //filter history
            newFI.FilterHistory = fi.FilterHistory;
            //controls
            newFI.DataSourceInd = fi.DataSourceInd;
            newFI.SegmentationProtocol = fi.SegmentationProtocol;
            newFI.SegmentationCBoxIndex = fi.SegmentationCBoxIndex;
            newFI.thresholdsCBoxIndex = fi.thresholdsCBoxIndex;
            newFI.sumHistogramChecked = fi.sumHistogramChecked;
            //spotdetector controls
            newFI.SpotThresh = fi.SpotThresh;
            newFI.SpotColor = fi.SpotColor;
            newFI.RefSpotColor = fi.RefSpotColor;
            newFI.SelectedSpotThresh = fi.SelectedSpotThresh;
            newFI.typeSpotThresh = fi.typeSpotThresh;
            newFI.SpotTailType = fi.SpotTailType;
            newFI.spotSensitivity = fi.spotSensitivity;
            //specific values
            newFI.thresholds = fi.thresholds;
            newFI.thresholdValues = fi.thresholdValues;
            newFI.thresholdColors = fi.thresholdColors;
            newFI.RefThresholdColors = fi.RefThresholdColors;
            //BandC
            newFI.autoDetectBandC = fi.autoDetectBandC;
            newFI.applyToAllBandC = fi.applyToAllBandC;
            //
            newFI.selectedPictureBoxColumn = fi.selectedPictureBoxColumn;
            //History
            newFI.History = fi.History;
            newFI.HistoryPlace = fi.HistoryPlace;
            newFI.undo = fi.undo;
            newFI.redo = fi.redo;
            newFI.delHist = fi.delHist;
            //info var
            newFI.frame = fi.frame;
            newFI.zValue = fi.zValue;
            newFI.cValue = fi.cValue;
            newFI.original = fi.original;
            //Not available
            newFI.loaded = fi.loaded;
            newFI.available = fi.available;
            newFI.selected = fi.selected;
            newFI.openedImages = fi.openedImages;
            //tif image
            newFI.image8bit = fi.image8bit;
            newFI.image16bit = fi.image16bit;
            newFI.image8bitFilter = fi.image8bitFilter;
            newFI.image16bitFilter = fi.image16bitFilter;
            //tif tags
            newFI.Dir = fi.Dir;
            newFI.seriesCount = fi.seriesCount;
            newFI.imageCount = fi.imageCount;
            newFI.sizeX = fi.sizeX;
            newFI.sizeY = fi.sizeY;
            newFI.sizeZ = fi.sizeZ;
            newFI.umZ = fi.umZ;
            newFI.umXY = fi.umXY;
            newFI.sizeC = fi.sizeC;
            newFI.sizeT = fi.sizeT;
            newFI.bitsPerPixel = fi.bitsPerPixel;
            newFI.dimensionOrder = fi.dimensionOrder;
            newFI.pixelType = fi.pixelType;
            newFI.FalseColored = fi.FalseColored;
            newFI.isIndexed = fi.isIndexed;
            newFI.MetadataComplete = fi.MetadataComplete;
            newFI.DatasetStructureDescription = fi.DatasetStructureDescription;
            newFI.LutList = fi.LutList;
            newFI.TimeSteps = fi.TimeSteps;
            newFI.Micropoint = fi.Micropoint;
            //Metadata protocol info
            newFI.FileDescription = fi.FileDescription;
            newFI.xCompensation = fi.xCompensation;
            newFI.yCompensation = fi.yCompensation;
            //properties
            newFI.histogramArray = fi.histogramArray;
            newFI.MinBrightness = fi.MinBrightness;
            newFI.MaxBrightness = fi.MaxBrightness;
            newFI.adjustedLUT = fi.adjustedLUT;

            //RoiManager
            newFI.roiList = new List<CellToolDK.ROI>[fi.roiList.Length];
            for (int c = 0; c < fi.roiList.Length; c++)
                if (fi.roiList[c] != null)
                {
                    newFI.roiList[c] = new List<CellToolDK.ROI>();
                    foreach (ROI roi in fi.roiList[c])
                        newFI.roiList[c].Add(ROItoCellToolDK(roi));
                        
                }
            
            newFI.ROICounter = fi.ROICounter;

            return newFI;
        }

        private void CellToolDKtoFI(CellToolDK.TifFileInfo fi, TifFileInfo oldFI)
        {
            if (fi == null)
            {
                oldFI = null;
                return;
            }

            TifFileInfo newFI = oldFI;

            newFI.zoom = fi.zoom;
            newFI.Xposition = fi.Xposition;
            newFI.Yposition = fi.Yposition;
            //Chart
            newFI.xAxisTB = fi.xAxisTB;
            newFI.yAxisTB = fi.yAxisTB;
            
            //Tracking
            newFI.tracking_MaxSize = fi.tracking_MaxSize;
            newFI.tracking_MinSize = fi.tracking_MinSize;
            newFI.tracking_Speed = fi.tracking_Speed;
            /// <summary>
            /// Segmentation
            /// </summary>
            //filter history
            newFI.FilterHistory = fi.FilterHistory;
            //controls
            newFI.DataSourceInd = fi.DataSourceInd;
            newFI.SegmentationProtocol = fi.SegmentationProtocol;
            newFI.SegmentationCBoxIndex = fi.SegmentationCBoxIndex;
            newFI.thresholdsCBoxIndex = fi.thresholdsCBoxIndex;
            newFI.sumHistogramChecked = fi.sumHistogramChecked;
            //spotdetector controls
            newFI.SpotThresh = fi.SpotThresh;
            newFI.SpotColor = fi.SpotColor;
            newFI.RefSpotColor = fi.RefSpotColor;
            newFI.SelectedSpotThresh = fi.SelectedSpotThresh;
            newFI.typeSpotThresh = fi.typeSpotThresh;
            newFI.SpotTailType = fi.SpotTailType;
            newFI.spotSensitivity = fi.spotSensitivity;
            //specific values
            newFI.thresholds = fi.thresholds;
            newFI.thresholdValues = fi.thresholdValues;
            newFI.thresholdColors = fi.thresholdColors;
            newFI.RefThresholdColors = fi.RefThresholdColors;
            //BandC
            newFI.autoDetectBandC = fi.autoDetectBandC;
            newFI.applyToAllBandC = fi.applyToAllBandC;
            //
            newFI.selectedPictureBoxColumn = fi.selectedPictureBoxColumn;
            //History
            newFI.History = fi.History;
            newFI.HistoryPlace = fi.HistoryPlace;
            newFI.undo = fi.undo;
            newFI.redo = fi.redo;
            newFI.delHist = fi.delHist;
            //info var
            newFI.frame = fi.frame;
            newFI.zValue = fi.zValue;
            newFI.cValue = fi.cValue;
            newFI.original = fi.original;
            //Not available
            newFI.loaded = fi.loaded;
            newFI.available = fi.available;
            newFI.selected = fi.selected;
            newFI.openedImages = fi.openedImages;
            //tif image
            newFI.image8bit = fi.image8bit;
            newFI.image16bit = fi.image16bit;
            newFI.image8bitFilter = fi.image8bitFilter;
            newFI.image16bitFilter = fi.image16bitFilter;
            //tif tags
            newFI.Dir = fi.Dir;
            newFI.seriesCount = fi.seriesCount;
            newFI.imageCount = fi.imageCount;
            newFI.sizeX = fi.sizeX;
            newFI.sizeY = fi.sizeY;
            newFI.sizeZ = fi.sizeZ;
            newFI.umZ = fi.umZ;
            newFI.umXY = fi.umXY;
            newFI.sizeC = fi.sizeC;
            newFI.sizeT = fi.sizeT;
            newFI.bitsPerPixel = fi.bitsPerPixel;
            newFI.dimensionOrder = fi.dimensionOrder;
            newFI.pixelType = fi.pixelType;
            newFI.FalseColored = fi.FalseColored;
            newFI.isIndexed = fi.isIndexed;
            newFI.MetadataComplete = fi.MetadataComplete;
            newFI.DatasetStructureDescription = fi.DatasetStructureDescription;
            newFI.LutList = fi.LutList;
            newFI.TimeSteps = fi.TimeSteps;
            newFI.Micropoint = fi.Micropoint;
            //Metadata protocol info
            newFI.FileDescription = fi.FileDescription;
            newFI.xCompensation = fi.xCompensation;
            newFI.yCompensation = fi.yCompensation;
            //properties
            newFI.histogramArray = fi.histogramArray;
            newFI.MinBrightness = fi.MinBrightness;
            newFI.MaxBrightness = fi.MaxBrightness;
            newFI.adjustedLUT = fi.adjustedLUT;
            //RoiManager
            newFI.roiList = new List<ROI>[fi.roiList.Length];
            for (int c = 0; c < fi.roiList.Length; c++)
                if (fi.roiList[c] != null)
                {
                    newFI.roiList[c] = new List<ROI>();
                    foreach (CellToolDK.ROI roi in fi.roiList[c])
                    {
                        ROI newROI = CellToolDKtoROI(roi);
                        newFI.roiList[c].Add(newROI);
                        RoiMeasure.Measure(newROI, newFI, c, IA);
                    }
                }

            newFI.ROICounter = fi.ROICounter;
        }

        private CellToolDK.ROI ROItoCellToolDK(ROI roi)
        {
            CellToolDK.ROI newRoi = new CellToolDK.ROI(roi.getID, 1, roi.Shape, roi.Type, roi.turnOnStackRoi);

            newRoi.Checked = roi.Checked;
            newRoi.Comment = roi.Comment;

            newRoi.Width = roi.Width;
            newRoi.Height= roi.Height;

            newRoi.Stack = roi.Stack;
            newRoi.D = roi.D;

            newRoi.FromT = roi.FromT;
            newRoi.ToT = roi.ToT;
            newRoi.FromZ = roi.FromZ;
            newRoi.ToZ = roi.ToZ;

            newRoi.SetLocationAll(roi.GetLocationAll());

            return newRoi;
        }
        private ROI CellToolDKtoROI(CellToolDK.ROI roi)
        {
            ROI newRoi = new ROI(roi.getID, 1, roi.Shape, roi.Type, roi.turnOnStackRoi);

            newRoi.Checked = roi.Checked;
            newRoi.Comment = roi.Comment;

            newRoi.Width = roi.Width;
            newRoi.Height = roi.Height;

            newRoi.Stack = roi.Stack;
            newRoi.D = roi.D;

            newRoi.FromT = roi.FromT;
            newRoi.ToT = roi.ToT;
            newRoi.FromZ = roi.FromZ;
            newRoi.ToZ = roi.ToZ;

            newRoi.SetLocationAll(roi.GetLocationAll());

            return newRoi;
        }
    }
}
