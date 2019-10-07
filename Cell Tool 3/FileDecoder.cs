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
using System.IO;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
///using Microsoft.VisualBasic.Devices;
//LibTif
using BitMiracle.LibTiff.Classic;
//BioFormats
using loci.formats.@in;


namespace Cell_Tool_3
{
    class FileDecoder
    {
        private ImageAnalyser IA = null;
        public List<string> Formats = new List<string>();
        public ToolStripStatusLabel StatusLabel;
        public ToolStripProgressBar StatusBar;
        public System.Windows.Forms.Timer loadingTimer = new System.Windows.Forms.Timer();
        public List<TabPage> tabCollection;

        public void LoadExtensions()
        {
            //Load formats
            Formats.Add(".tif");
            Formats.Add(".RoiSet");
            Formats.Add(".CTPlugIn.dll");
            Formats.Add(".CTData");
            Formats.Add(".png");
            Formats.Add(".jpg");
            Formats.Add(".bmp");
            Formats.AddRange(BioFormats_Reader.GetFormatsSuff());
            //Loading Bar options
            loadingTimer.Interval = 300;
            loadingTimer.Tick += new EventHandler(loadingTimer_Tick);
        }
        public string[] Boiformats_Formats()
        {
            return new string[]
            {
                ".ims",//Bitplane Imaris
                ".lif",//Leica LAS AF LIF (Leica Image File Format)
                ".tiff",//MetaMorph 7.5 TIFF
                ".nd2",//Nikon NIS-Elements ND2
                ".oif",//Olympus FluoView FV1000
                ".czi",//Zeiss CZI
                ".zvi"//Zeiss AxioVision ZVI (Zeiss Vision Image)
            };
        }
        private void loadingTimer_Tick(object sender, EventArgs e)
        {
            if (StatusBar.Minimum != 0) { StatusBar.Minimum = 0; }
            if (StatusBar.Step != 1) { StatusBar.Step = 1; }

            bool hideStatusBar = true;
            foreach (TabPage tp in tabCollection)
            {
                if (tp.tifFI != null && tp.tifFI.available == false && tp.tifFI.selected == true)
                {
                    if (tp.tifFI.imageCount > tp.tifFI.openedImages)
                    {
                        hideStatusBar = false;
                        break;
                    }
                }
            }

            StatusBar.Visible = !hideStatusBar;
        }
        public Panel OpenFile(List<TabPage> Collection,string path,int FileTypeIndex, ImageAnalyser IA1)
        {
            IA = IA1;
            TabPage tp = new TabPage();
            tp.OpenFile(FileTypeIndex);
            
            switch (FileTypeIndex)
            {
                default:
                    if (OpenTif(Collection, path, tp, IA1) == false)
                    {
                        return null;
                    }
                    path = tp.tifFI.Dir;
                    break;
                
                case 1:
                    return null;
                case 2:
                    return null;
                case 3:
                    ResultsExtractor resExtr = new ResultsExtractor();
                    tp.CorePanel = resExtr.Input(path, IA);
                    tp.ResultsExtractor = resExtr;
                    break;
                    /*
                case 3:
                    if (OpenBitmap(Collection, path, tp, IA1) == false)
                    {
                        return null;
                    }
                    break;
                case 4:
                    if (OpenBitmap(Collection, path, tp, IA1) == false)
                    {
                        return null;
                    }
                    break;
                case 5:
                    if (OpenBitmap(Collection, path, tp, IA1) == false)
                    {
                        return null;
                    }
                    break;
               */
                    
                //Add new open hendler for new file format
            }
            tp.dir = path;
            Collection.Add(tp);
            tabCollection = Collection;
            loadingTimer.Start();

            tp.CorePanel.Tag = path;

            return tp.CorePanel;
        }
        /*
        private Boolean OpenBitmap(List<TabPage> Collection, string path, TabPage tp, ImageAnalyser IA1)
        {
            StatusLabel.Text = "Reading Image...";

            Bitmap bmp = new Bitmap(path);
            //Send to bioformats;
            if("Format24bppRgb"!=bmp.PixelFormat.ToString() && "Format8bppIndexed" != bmp.PixelFormat.ToString())
                return BioFormats_Reader.OpenFile(Collection, path, tp, IA1, StatusLabel); ;
            //adjust methadata
            TifFileInfo fi = tp.tifFI;
            fi.xAxisTB = IA.chart.Properties.xAxisTB.SelectedIndex;
            fi.yAxisTB = IA.chart.Properties.yAxisTB.SelectedIndex;

            fi.Dir = path.Replace(Formats[tp.FileTypeIndex],".tif");
            tp.FileTypeIndex = 0;
           
            
            fi.available = false;
            fi.seriesCount = 1;
           
            fi.sizeX = bmp.Width;
            fi.sizeY = bmp.Height;
            fi.sizeZ = 1;
            fi.umZ = 0;
            fi.umXY = 0;
            fi.sizeT = 1;
            fi.TimeSteps = new List<double> { 1, 1 };
            fi.dimensionOrder = "XYCZT";
            fi.FalseColored = true;
            fi.isIndexed = true;
            fi.MetadataComplete = true;
            fi.original = false;
            fi.DatasetStructureDescription = "";
            

            // Lock the bitmap's bits.
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadWrite, bmp.PixelFormat);
            // Get the address of the first line.
            IntPtr ptr = bmpData.Scan0;
            //store rgb values
            
            fi.bitsPerPixel = 8;
            fi.pixelType = 0;

            switch (fi.bitsPerPixel)
            {
                case 8:
                    if ("Format24bppRgb" == bmp.PixelFormat.ToString())
                    {
                        int bytes = Math.Abs(bmpData.Stride) * bmp.Height;
                        byte[] rgbValues = new byte[bytes];
                        // Copy the RGB values into the array
                        System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);
                        fi.imageCount = 3;
                        fi.sizeC = 3;
                        fi.LutList = new List<Color>() { Color.FromArgb(255, 0, 0), Color.FromArgb(0, 255, 0), Color.FromArgb(0, 0, 255) };

                        byte[][][] image8bit = new byte[3][][];

                        for (int c = 0; c < fi.sizeC; c++)
                        {
                            image8bit[c] = new byte[fi.sizeY][];
                            for (int y = 0; y < fi.sizeY; y++)
                                image8bit[c][y] = new byte[fi.sizeX];
                        }

                        int position = 0;

                        for (int y = 0; y < fi.sizeY; y++)
                            for (int x = 0; x < fi.sizeX; x++)
                                for (int c = fi.sizeC - 1; c >= 0; c--)
                                {
                                    image8bit[c][y][x] = rgbValues[position];
                                    position++;
                                }
                        fi.image8bit = image8bit;
                        fi.image8bitFilter = fi.image8bit;
                    }
                   else if("Format8bppIndexed" == bmp.PixelFormat.ToString())
                    {
                        int bytes = Math.Abs(bmpData.Stride) * bmp.Height;
                        byte[] rgbValues = new byte[bytes];
                        // Copy the RGB values into the array
                        System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);
                        fi.imageCount = 1;
                        fi.sizeC = 1;
                        fi.LutList = new List<Color>() { Color.FromArgb(255, 255, 255) };

                        byte[][][] image8bit = new byte[1][][];
                        int position = 0;

                        image8bit[0] = new byte[fi.sizeY][];
                        for (int y = 0; y < fi.sizeY; y++)
                        {
                            image8bit[0][y] = new byte[fi.sizeX];
                            for (int x = 0; x < fi.sizeX; x++)
                            {
                                image8bit[0][y][x] = rgbValues[position];
                                position++;
                            }
                        }
                        fi.image8bit = image8bit;
                        fi.image8bitFilter = fi.image8bit;
                    }

                    break;
            }
            #region Segmentation variables
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

            //add taskbar
            tp.tifFI.tpTaskbar.Initialize(IA1.TabPages.ImageMainPanel, IA1, fi);

            fi.openedImages = fi.imageCount;
            fi.available = true;
            fi.loaded = true;
            Boolean check = true;
            foreach (TabPage tp1 in Collection)
            {
                if (tp1.tifFI.available == false)
                {
                    check = false;
                    break;
                }
            }
            if (check == true)
            {
                loadingTimer.Stop();
                StatusLabel.Text = "Ready";
            }

            IA1.ReloadImages();

            if (IA.Segmentation.AutoSetUp.LibTB.SelectedIndex > 0 &&
               IA.Segmentation.AutoSetUp.ApplyToNewCheckB.Checked &&
                    MessageBox.Show("Do you want to open the image with the following protocol:\n" +
                    IA.Segmentation.AutoSetUp.LibTB.Text,
                               "", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
            {
                IA.Segmentation.AutoSetUp.ApplyCT3Tags(IA.Segmentation.AutoSetUp.protocols[
                IA.Segmentation.AutoSetUp.LibTB.SelectedIndex].Split(
                new string[] { ";\n" }, StringSplitOptions.None), fi);
            }
            //out put
            return true;
        }
        */
        private bool OpenTif(List<TabPage> Collection, string path, TabPage tp, ImageAnalyser IA1)
        {

            StatusLabel.Text = "Reading Metadata...";
            
            //Check for CellTool 3 format
            try
            {
                /*
                if (BigImagesReader.OpenBigImage(path, Collection, tp, IA1)) return true;
                else if (new System.IO.FileInfo(path).Length >= (long)new ComputerInfo().AvailablePhysicalMemory)
                {
                    MessageBox.Show("Not enough RAM memory!");
                    return false;
                }
                else 
                */
                if (CellTool3_ReadMetadata(path, Collection, tp, IA1)) return true;
                else if (CellTool2_ReadMetadata(path, Collection, tp, IA1)) return true;
                else if (BioFormats_Reader.OpenFile(Collection, path, tp, IA1, StatusLabel)) return true;
                else StatusLabel.Text = "Ready";

            }
            catch { MessageBox.Show("Error: Loading file error!"); }

            StatusLabel.Text = "Ready";
            return false;
        }
        /*
        private bool OpenTif1(List<TabPage> Collection, string path, TabPage tp, ImageAnalyser IA1)
        {
           
            StatusLabel.Text = "Reading Metadata...";

            //Check for CellTool 3 format
            try
            {
                if (CellTool3_ReadMetadata(path, Collection, tp, IA1)) return true;
                if (CellTool2_ReadMetadata(path, Collection, tp, IA1)) return true;
                if(BioFormats_Reader.OpenFile(Collection, path, tp, IA1, StatusLabel)) return true;
            }
            catch { }
            return false;
            
            TifFileInfo fi = tp.tifFI;
            fi.xAxisTB = IA.chart.Properties.xAxisTB.SelectedIndex;
            fi.yAxisTB = IA.chart.Properties.yAxisTB.SelectedIndex;
            fi.Dir = path;

            // read data from byte array using ImageReader
            loci.formats.@in.TiffReader reader = new loci.formats.@in.TiffReader();
            
            reader.setId(path);
            fi.bitsPerPixel = reader.getBitsPerPixel();            

            if (fi.bitsPerPixel != 8 & fi.bitsPerPixel != 16)
            {
                reader.close();
                return false;
            }
            if (reader.isMetadataComplete() == false)
            {
                MessageBox.Show("Metadata is not complete!");
            }

            //read tags
            fi.available = false;
            fi.seriesCount = reader.getSeriesCount();
            fi.imageCount = reader.getImageCount();
            fi.sizeX = reader.getSizeX();
            fi.sizeY = reader.getSizeY();
            fi.sizeZ = reader.getSizeZ();

            fi.sizeC = reader.getSizeC();
            fi.LutList = new List<Color>();
            for (int i = 0; i < fi.sizeC; i++)
            {
                fi.LutList.Add(Color.White);
            }
            fi.umZ = 0;
            fi.umXY = 0;
            fi.sizeT = reader.getSizeT();

            fi.dimensionOrder = reader.getDimensionOrder();
            fi.pixelType = reader.getPixelType();
            fi.FalseColored = reader.isFalseColor();
            fi.isIndexed = reader.isIndexed();
            fi.MetadataComplete = reader.isMetadataComplete();
           
            fi.DatasetStructureDescription = reader.getDatasetStructureDescription();
            reader.close();
            
            //Get specific tags with LibTif
            Tiff image = Tiff.Open(path, "r");
            {
                image.SetDirectory(0);
                TiffTag tag = (TiffTag)270;
                FieldValue[] value = image.GetField(tag);
                if (value != null)
                {
                    string tagText = "";
                    for (int i = 0; i < value.Length; i++)
                    {
                        //tagText += value[i].ToString();
                        tagText += Encoding.UTF8.GetString(value[i].ToByteArray());
                    }
                    fi.FileDescription = tagText.Replace("->","-");
                    //decoders
                    try
                    {
                        ImageJ_decoder(tagText, image.NumberOfDirectories() - 1, fi);
                        andor_IQ_Decoder(tagText, image.NumberOfDirectories() - 1, fi);
                        Andor_Dragonfly_decoder(tagText, image.NumberOfDirectories() - 1, fi);
                    }
                    catch { }

                    if (fi.TimeSteps == null)
                    {
                        fi.TimeSteps = new List<double>();
                        fi.TimeSteps.Add(fi.imageCount + 1);
                        fi.TimeSteps.Add(1);
                    }
                }

                //Cneck is it thi original file
                fi.original = false;
                //Prepare Tags
                {
                    if (fi.sizeC == 0) { fi.sizeC = 1; }
                    if (fi.sizeZ == 0) { fi.sizeZ = 1; }
                }
                #region Segmentation variables
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
            }
            
            int[] dimOrder = FrameCalculator.GetDimmensionMatrix(fi);
            fi.dimensionOrder = "XYCZT";
            //background worker
            var bgw = new BackgroundWorker();
            bgw.WorkerReportsProgress = true;
            bool loaded = false;
            //Add handlers to the backgroundworker
            bgw.DoWork += new DoWorkEventHandler(delegate (Object o, DoWorkEventArgs a)
            {
                //prepare array and read file
                int midFrame = fi.sizeC * fi.sizeZ;
                switch (fi.bitsPerPixel)
                {
                    case 8:
                        fi.image8bit = new byte[image.NumberOfDirectories()][][];
                        for (int i = 0; i < midFrame; i++)
                        {
                            if (i >= fi.imageCount) { break; }
                            Image8bit_readFrame(i, image, fi, dimOrder);
                        }
                        break;
                    case 16:
                        fi.image16bit = new ushort[image.NumberOfDirectories()][][];
                        for (int i = 0; i < midFrame; i++)
                        {
                            if (i >= fi.imageCount) { break; }
                            Image16bit_readFrame(i, image, fi, dimOrder);
                        }
                        break;
                }
                loaded = true;
                   //report progress
                   ((BackgroundWorker)o).ReportProgress(0);
                    //parallel readers
                    ImageReader_BGW(Collection, image, fi, IA1, path, dimOrder);
                //report progress
               
                    ((BackgroundWorker)o).ReportProgress(1);
                
                image.Close();
            });
            bgw.ProgressChanged += new ProgressChangedEventHandler(delegate (Object o, ProgressChangedEventArgs a)
            {
               if (a.ProgressPercentage == 0)
                {
                    
                    fi.openedImages = fi.sizeC * fi.sizeZ;
                    try
                    {
                        IA1.ReloadImages();
                    }
                    catch { }
                }
                else if(a.ProgressPercentage == 1)
                {
                    fi.openedImages = fi.imageCount;
                    fi.available = true;
                    fi.loaded = true;

                    CalculateAllRois(fi);

                    IA1.ReloadImages();
                    Boolean check = true;
                    foreach (TabPage tp1 in Collection)
                    {
                        if (tp1.tifFI.available == false)
                        {
                            check = false;
                            break;
                        }
                    }
                    if (check == true)
                    {
                        loadingTimer.Stop();
                        StatusLabel.Text = "Ready";
                    }

                    if (IA.Segmentation.AutoSetUp.LibTB.SelectedIndex > 0 &&
               IA.Segmentation.AutoSetUp.ApplyToNewCheckB.Checked &&
                    MessageBox.Show("Do you want to open the image with the following protocol:\n" +
                    IA.Segmentation.AutoSetUp.LibTB.Text,
                               "", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                    {
                        IA.Segmentation.AutoSetUp.ApplyCT3Tags(IA.Segmentation.AutoSetUp.protocols[
                        IA.Segmentation.AutoSetUp.LibTB.SelectedIndex].Split(
                        new string[] { ";\n" }, StringSplitOptions.None), fi);
                    }
                }
            });

            //Start background worker
            StatusLabel.Text = "Reading Tif Image...";
            //Clear OldImage
            IA1.IDrawer.ClearImage();
            
            //start bgw
            bgw.RunWorkerAsync();
            //add taskbar
            tp.tifFI.tpTaskbar.Initialize(IA1.TabPages.ImageMainPanel, IA1, fi);

            try
            {
                if (loaded == true)
                {
                    IA1.ReloadImages();
                }
            }
            catch { }
            //out put
            return true;
        }
        */
        public void Image8bit_readFrame(int i, Tiff image, TifFileInfo fi, int[] dimOrder = null)
        {

            if (fi.image8bit == null) { return; }
            if (dimOrder != null && dimOrder.Length <= i) return;

            if (dimOrder != null)
                image.SetDirectory((short)dimOrder[i]);
            else
                image.SetDirectory((short)i);

            int scanlineSize = image.ScanlineSize();

            byte[][] buffer8 = new byte[fi.sizeY][];

            for (int j = 0; j < fi.sizeY; j++)
            {
                buffer8[j] = new byte[scanlineSize];
                image.ReadScanline(buffer8[j], j);
            }
            if (fi.image8bit == null) { return; }
            try
            {
                fi.image8bit[i] = buffer8;
            }
            catch
            {

            }

        }
        public void Image16bit_readFrame(int i, Tiff image, TifFileInfo fi, int[] dimOrder = null)
        {
            if (fi.image16bit == null) { return; }
            if (dimOrder != null && dimOrder.Length <= i) return;

            if (dimOrder != null)
                image.SetDirectory((short)dimOrder[i]);
            else
                image.SetDirectory((short)i);

            int scanlineSize = image.ScanlineSize();

            ushort[][] buffer16 = new ushort[fi.sizeY][];

            for (int j = 0; j < fi.sizeY; j++)
            {
                byte[] line = new byte[scanlineSize];
                buffer16[j] = new ushort[scanlineSize / 2];
                image.ReadScanline(line, j);
                Buffer.BlockCopy(line, 0, buffer16[j], 0, line.Length);
            }
            if (fi.image16bit == null) { return; }
            try
            {
                fi.image16bit[i] = buffer16;
            }
            catch
            {
            }

        }
        public void ImageReader_BGW(List<TabPage> Collection, Tiff image1,TifFileInfo fi, ImageAnalyser IA,string path, int[] dimOrder = null)
        {
            
            //Tiff.ByteArrayToShorts
            int height = fi.sizeY;
            int BitsPerPixel = fi.bitsPerPixel;
            int midFrame = fi.sizeC * fi.sizeZ;
            switch (BitsPerPixel)
            {
                    case 8:
                    Parallel.For(midFrame, (int)image1.NumberOfDirectories(), i =>
                    {
                        using (Tiff image = Tiff.Open(OSStringConverter.StringToDir(path), "r"))
                        {
                            Image8bit_readFrame(i, image, fi, dimOrder);
                            image.Close();
                        }
                    });
                    break;
                    case 16:
                    Parallel.For(midFrame, (int)image1.NumberOfDirectories(), i =>
                    {
                        
                        using (Tiff image = Tiff.Open(OSStringConverter.StringToDir(path), "r"))
                        {
                            Image16bit_readFrame(i, image, fi, dimOrder);
                            image.Close();
                        }
                    });
                    break;
                }
        }
        
        private bool ImageReader_BGW_FileChains(List<TabPage> Collection, Tiff image1, TifFileInfo fi, ImageAnalyser IA, string path)
        {
            string newPath = FileEncoder.CheckForFileChain(path);
            newPath = newPath.Substring(0, newPath.Length - 4);

            string curPath = FileEncoder.FileChain_GetName(0, newPath);
            if (!File.Exists(OSStringConverter.StringToDir(curPath))) return false;
            //Tiff.ByteArrayToShorts
            int height = fi.sizeY;
            int BitsPerPixel = fi.bitsPerPixel;
            int midFrame = fi.sizeC * fi.sizeZ;
            switch (BitsPerPixel)
            {
                case 8:
                    Parallel.For(midFrame, (int)image1.NumberOfDirectories(), i =>
                    {
                        using (Tiff image = Tiff.Open(OSStringConverter.StringToDir(curPath), "r"))
                        {
                            Image8bit_readFrame(i, image, fi);
                            image.Close();
                        }
                    });

                    midFrame = (int)image1.NumberOfDirectories();
                    break;
                case 16:
                    Parallel.For(midFrame, (int)image1.NumberOfDirectories(), i =>
                    {

                        using (Tiff image = Tiff.Open(OSStringConverter.StringToDir(curPath), "r"))
                        {
                            Image16bit_readFrame(i, image, fi);
                            image.Close();
                        }
                    });
                    midFrame = (int)image1.NumberOfDirectories();
                    break;
            }

            int chainN = 1;
            curPath = FileEncoder.FileChain_GetName(chainN, newPath);

            while (File.Exists(OSStringConverter.StringToDir(curPath)))
            {
                int NumbOfDirs = 0;
                using (Tiff image = Tiff.Open(OSStringConverter.StringToDir(curPath), "r"))
                {
                    NumbOfDirs = (int)image.NumberOfDirectories();
                    image.Close();
                }
                
                switch (BitsPerPixel)
                {
                    case 8:
                        Parallel.For(0, NumbOfDirs, i =>
                        {
                            using (Tiff image = Tiff.Open(OSStringConverter.StringToDir(curPath), "r"))
                            {
                                Image8bit_FileChains(i + midFrame, i, image, fi);
                                image.Close();
                            }
                        });

                        midFrame += NumbOfDirs;
                        break;
                    case 16:
                        Parallel.For(0, NumbOfDirs, i =>
                        {
                            using (Tiff image = Tiff.Open(OSStringConverter.StringToDir(curPath), "r"))
                            {
                                Image16bit_FileChains(i+midFrame,i, image, fi);
                                image.Close();
                            }
                        });
                        midFrame += NumbOfDirs;
                        break;
                }

                chainN++;
                curPath = FileEncoder.FileChain_GetName(chainN, newPath);
            }
            if (midFrame != fi.imageCount)
            {
                MessageBox.Show("Loading error: Missing files!");
            }
            return true;
        }
        private void Image8bit_FileChains(int i, int dirInd, Tiff image, TifFileInfo fi)
        {

            if (fi.image8bit == null) { return; }

            image.SetDirectory((short)dirInd);

            int scanlineSize = image.ScanlineSize();

            byte[][] buffer8 = new byte[fi.sizeY][];

            for (int j = 0; j < fi.sizeY; j++)
            {
                buffer8[j] = new byte[scanlineSize];
                image.ReadScanline(buffer8[j], j);
            }
            if (fi.image8bit == null) { return; }
            try
            {
                fi.image8bit[i] = buffer8;
            }
            catch
            {

            }

        }
        private void Image16bit_FileChains(int i,int dirInd, Tiff image, TifFileInfo fi)
        {
            if (fi.image16bit == null) { return; }

            image.SetDirectory((short)dirInd);

            int scanlineSize = image.ScanlineSize();

            ushort[][] buffer16 = new ushort[fi.sizeY][];

            for (int j = 0; j < fi.sizeY; j++)
            {
                byte[] line = new byte[scanlineSize];
                buffer16[j] = new ushort[scanlineSize / 2];
                image.ReadScanline(line, j);
                Buffer.BlockCopy(line, 0, buffer16[j], 0, line.Length);
            }
            if (fi.image16bit == null) { return; }
            try
            {
                fi.image16bit[i] = buffer16;
            }
            catch
            {
            }

        }
        /*
        private void ImageJ_decoder(string txt, int Pages, TifFileInfo fi)
        {
            if (txt.IndexOf("ImageJ") > -1)
            {
                
                //count Luts
                fi.sizeC = fi.LutList.Count;
                //calculate Z
                fi.sizeT = fi.sizeZ;
                //calculate T
                fi.sizeZ = Pages / (fi.sizeC * fi.sizeZ);
            }
        }
        private void Andor_Dragonfly_decoder(string txt, int Pages, TifFileInfo fi)
        {
            if (txt.IndexOf("</OME:OME>") > -1)
            {
                string subStr = txt.Substring(txt.IndexOf("<OME:Pixels"), txt.IndexOf("</OME:Pixels>")- txt.IndexOf("<OME:Pixels"));
                string[] core = subStr.Substring(12, subStr.IndexOf(">")-12).Split(new string[] { "\" " },StringSplitOptions.None);
                //write core metadata to file info
                foreach(string str in core)
                {
                    string[] vals = str.Replace(" ", "").Split(new string[] { "=\"" }, StringSplitOptions.None);
                    switch (vals[0])
                    {
                        case "DimensionOrder":
                            fi.dimensionOrder = vals[1];
                            break;
                        case "SizeX":
                            fi.sizeX = int.Parse(vals[1]);
                            break;
                        case "SizeY":
                            fi.sizeY = int.Parse(vals[1]);
                            break;
                        case "SizeZ":
                            fi.sizeZ = int.Parse(vals[1]);
                            break;
                        case "SizeC":
                            fi.sizeC = int.Parse(vals[1]);
                            break;
                        case "SizeT":
                            fi.sizeT = int.Parse(vals[1]);
                            break;
                        case "PhysicalSizeX":
                            fi.umXY = double.Parse(vals[1]);
                            break;
                        case "PhysicalSizeZ":
                            fi.umZ = double.Parse(vals[1]);
                            break;
                    }
                }

                string[] colors = subStr.Split(new string[] { "Color=\"" }, StringSplitOptions.None);
                
                fi.LutList.Clear();
                for (int i = 0; i < fi.sizeC; i++)
                {
                    if (colors.Length - 1 > i)
                    {
                        string col = colors[i + 1].Substring(0, colors[i + 1].IndexOf("\""));
                        try
                        {
                            fi.LutList.Add(ColorTranslator.FromOle(int.Parse(col)));
                        }
                        catch
                        {
                            fi.LutList.Add(Color.White);
                        }
                    }
                    else
                    {
                        fi.LutList.Add(Color.White);
                    }
                }
                
                
                //File.WriteAllText(@"C:\AndorDragonflyDemo\meta.txt", core);

            }
            else if (txt.IndexOf("</OME>") > -1)
            {
                string subStr = txt.Substring(txt.IndexOf("<Pixels"), txt.IndexOf("</Pixels>") - txt.IndexOf("<Pixels"));
                string[] core = subStr.Substring(12, subStr.IndexOf(">") - 12).Split(new string[] { "\" " }, StringSplitOptions.None);
                //write core metadata to file info
                foreach (string str in core)
                {
                    string[] vals = str.Replace(" ", "").Split(new string[] { "=\"" }, StringSplitOptions.None);
                    switch (vals[0])
                    {
                        case "DimensionOrder":
                            fi.dimensionOrder = vals[1];
                            break;
                        case "SizeX":
                            fi.sizeX = int.Parse(vals[1]);
                            break;
                        case "SizeY":
                            fi.sizeY = int.Parse(vals[1]);
                            break;
                        case "SizeZ":
                            fi.sizeZ = int.Parse(vals[1]);
                            break;
                        case "SizeC":
                            fi.sizeC = int.Parse(vals[1]);
                            break;
                        case "SizeT":
                            fi.sizeT = int.Parse(vals[1]);
                            break;
                        case "PhysicalSizeX":
                            fi.umXY = double.Parse(vals[1]);
                            break;
                        case "PhysicalSizeZ":
                            fi.umZ = double.Parse(vals[1]);
                            break;
                    }
                }

                string[] colors = subStr.Split(new string[] { "Color=\"" }, StringSplitOptions.None);

                fi.LutList.Clear();
                for (int i = 0; i < fi.sizeC; i++)
                {
                    if (colors.Length - 1 > i)
                    {
                        string col = colors[i + 1].Substring(0, colors[i + 1].IndexOf("\""));
                        try
                        {
                            fi.LutList.Add(ColorTranslator.FromOle(int.Parse(col)));
                        }
                        catch
                        {
                            fi.LutList.Add(Color.White);
                        }
                    }
                    else
                    {
                        fi.LutList.Add(Color.White);
                    }
                }

                //File.WriteAllText(@"C:\AndorDragonflyDemo\meta.txt", core);

            }
        }
        private void andor_IQ_Decoder(string txt, int Pages, TifFileInfo fi)
        {
            if (!(txt.IndexOf("Protocol") > -1)) return;
            
            int MoveXYcounter = 0;

            List<Color> LutList = new List<Color>();
            Boolean zCheck = false;

            fi.sizeZ = 1;
            fi.umZ = 0;
            //check is it a Z stack
            if (!(txt.IndexOf("Repeat Z") > -1)) zCheck = true;

            foreach (string row in txt.Split(new[] { "\n" }, StringSplitOptions.None))
            {
                if (zCheck == false & row.IndexOf("Repeat Z") > -1)
                {
                    // Z stack size and Z size in um
                    string row1 = row.Replace("	", "").Replace("Repeat Z - ", "").
                           Replace(" um in ", "\t").Replace(" planes ", "\t");
                    int count = 0;
                    foreach (string val in row1.Split(new[] { "\t" }, StringSplitOptions.None))
                    {
                        if (count == 1)
                        {
                            fi.sizeZ = int.Parse(val);
                            fi.umZ /= (fi.sizeZ - 1);
                        }
                        else if (count == 0)
                        {
                            fi.umZ = Double.Parse(val);
                        }
                        count += 1;
                    }
                    zCheck = true;
                }
                else if (row.IndexOf("End - T") > -1 & zCheck == true)
                {
                    break;
                }
                // detect Colors
                if (row.IndexOf("LUT - ") > -1)
                {
                    string col = row.Replace("LUT - ", "").Replace("	", "").Replace(" ", "");
                    try
                    {
                        LutList.Add(ColorTranslator.FromHtml(col));
                    }
                    catch
                    {
                        LutList.Add(Color.White);
                        MessageBox.Show("Unknown color: " + col);
                    }

                }
                //For more then one field protocols
                if (row.IndexOf("Move XY") > -1) MoveXYcounter++;
                if (MoveXYcounter >= 2) break;
            }
            if (LutList.Count > 0) { fi.LutList = LutList; }
            //count Luts
            fi.sizeC = fi.LutList.Count;
            //calculate T
            fi.sizeT = (Pages / (fi.sizeC * fi.sizeZ)) + 1;

            //read time steps
            List<double> TimeSteps = new List<double>();
            foreach (string row in txt.Split(new[] { "\n" }, StringSplitOptions.None))
            {
                if (row.IndexOf("Repeat T - ") > -1)
                {
                    string str = row.Replace(" - fastest","").Replace("	", "").Replace("Repeat T - ", "")
                        .Replace(" times (", "-").Replace(" time (", "-")
                        .Replace(" sec)", "").Replace(" ms)", "").Replace(" min)", "");
                    foreach (string val in str.Split(new[] { "-" }, StringSplitOptions.None))
                    {
                        TimeSteps.Add(Single.Parse(val));
                    }
                    //change time unit to sec
                    if (row.IndexOf(" ms)") > -1)
                    {
                        int z = TimeSteps.Count - 1;
                        TimeSteps[z] /= 1000;
                    }
                    else if (row.IndexOf(" min)") > -1)
                    {
                        int z = TimeSteps.Count - 1;
                        TimeSteps[z] *= 60;
                    }
                }
                else if (row.IndexOf("Frappa - MICROPOINT (MicroPoint, Selected, _FRAPPA)") > -1
                    & TimeSteps.Count > 1)
                {
                    //Find Micropoint frame
                    double count = 0;
                    for (int i = 0; i < TimeSteps.Count; i += 2)
                    {
                        count += TimeSteps[i];
                    }
                    fi.Micropoint = count;
                }
            }
            if (TimeSteps.Count > 1) { fi.TimeSteps = TimeSteps; }

            //Check the size
            {
                string str = fi.Dir.Substring(
                    fi.Dir.LastIndexOf("\\")+1,
                    fi.Dir.LastIndexOf(".") - fi.Dir.LastIndexOf("\\") - 1);

                int i = -1;
                foreach (string val in str.Split(new string[] { "_" }, StringSplitOptions.None))
                {

                    if (val.Length == 5 && val.IndexOf("w") == 0)
                    {
                        try
                        {
                            i = int.Parse(val.Substring(1, 4));
                            break;
                        }
                        catch { continue; }
                    }
                }

                if (i != -1)
                {
                    for (int z = fi.LutList.Count - 1; z >= 0; z--)
                        if (z != i)
                            fi.LutList.RemoveAt(z);

                    fi.sizeC = fi.LutList.Count;
                    fi.sizeT = (Pages / (fi.sizeC * fi.sizeZ)) + 1;
                }
            }
        }
        */
        public int decodeFileType(string dir)
        {
            for (int i = 0; i < Formats.Count; i++)
            {
                if(dir.EndsWith(Formats[i]))
                {
                    return i;
                }
            }
            return -1;
        }
        public string Format_Extensions(string str)
        {
            string name = "";
            int length = str.Length;
            int count = length - 1;

            while (count >= 1 & str.Substring(count, 1) != ".")
            {
                count -= 1;
            }
            if (count >= 1)
            {
                name = str.Substring(count, (length - count));
            }
            return name;

        }
        #region Read CellTool2 metadata
        private int[] ApplyCT2Tags(string[] vals, TifFileInfo fi)
        {
            int[] FilterList = new int[] { 0 };
            for(int i = 1; i < vals.Length; i++)
            {
                switch (i)
                {
                    case 1:
                        break;
                    case 3:
                        fi.zoom = int.Parse(vals[i].Replace("x ", ""));
                        break;
                    case 5:
                        fi.MaxBrightness = new int[fi.sizeC];
                        for(int z = 0; z<fi.sizeC;z++)
                            fi.MaxBrightness[z] = int.Parse(vals[i ]);
                        break;
                    case 7:
                        FilterList[0] = 1 + int.Parse(vals[i ]);
                        if (FilterList[0] > 3) FilterList[0] = 3;
                        break;
                    case 9:
                        break;
                    case 11:
                        for (int z = 0; z < fi.sizeC; z++) {
                            fi.thresholdValues[z][1] = int.Parse(vals[i]);
                            fi.thresholdColors[z][0] = Color.Black;
                            fi.thresholdColors[z][1] = Color.Orange;
                            fi.SegmentationCBoxIndex[z] = 1;
                            fi.thresholdsCBoxIndex[z] = 1;
                            fi.thresholds[z] = 1;
                        }
                        break;
                    case 13:
                        break;
                    case 15:
                        break;
                    case 17:
                        fi.SpotThresh = new int[fi.sizeC];
                        for (int z = 0; z < fi.sizeC; z++)
                            fi.SpotThresh[z] = int.Parse(vals[i]);

                        fi.SelectedSpotThresh = new int[fi.sizeC];
                        for (int z = 0; z < fi.sizeC; z++)
                            fi.SelectedSpotThresh[z] = 3;
                        break;
                    case 19:
                        fi.tracking_Speed = new int[fi.sizeC];
                        for (int z = 0; z < fi.sizeC; z++)
                            fi.tracking_Speed[z] = int.Parse(vals[i]);
                        break;
                    case 21:
                        fi.tracking_MinSize = new int[fi.sizeC];
                        for (int z = 0; z < fi.sizeC; z++)
                            fi.tracking_MinSize[z] = int.Parse(vals[i ]);
                        break;
                    case 23:
                        break;
                    case 25:
                        break;
                    case 27:
                        //here is the note
                        break;
                    default:
                        if(i == vals.Length - 3)
                        {
                            fi.spotSensitivity = new int[fi.sizeC];
                            for (int z = 0; z < fi.sizeC; z++)
                                fi.spotSensitivity[z] = 100-int.Parse(vals[i ]);
                        }
                        else if(i == vals.Length - 1){
                            fi.MinBrightness = new int[fi.sizeC];
                            for (int z = 0; z < fi.sizeC; z++)
                                fi.MinBrightness[z] = int.Parse(vals[i]);
                        }
                       break;
                }
            }
            return FilterList;
        }
        private void ReadCT2CoreMeta(string[] vals, TifFileInfo fi)
        {
            foreach (string val in vals)
            {
                string[] newVal = val.Split(new string[] { "=" }, StringSplitOptions.None);
                switch (newVal[0])
                {
                    case "channels":
                        fi.sizeC = int.Parse(newVal[1]);
                        break;
                    case "z":
                        fi.sizeZ = int.Parse(newVal[1]);
                        break;
                    case "colors":
                        List<Color> LUTS = new List<Color>();
                        foreach (string col in newVal[1].Split(new string[] { "\t" }, StringSplitOptions.None))
                            LUTS.Add(ColorTranslator.FromHtml(col));
                        fi.LutList = LUTS;
                        break;
                    case "xCompenstation":
                        fi.xCompensation = int.Parse(newVal[1]);
                        break;
                    case "yCompenstation":
                        fi.yCompensation = int.Parse(newVal[1]);
                        break;
                    default:
                        break;
                }
            }

            fi.sizeT = fi.imageCount / (fi.sizeZ * fi.sizeC);
        }
        private void CT2_ReadRoiSet(string[] vals, TifFileInfo fi)
        {
            int ID = 0;
            string val = "";
            for (int i = 0; i < vals.Length; i++)
                vals[i] = vals[i].Replace("\r", "");
            
            for (int roi = 0; roi < vals.Length; roi += 33)
            {
                string type = "0";
                if (vals[2 + roi] == "Tracking") type = "1";

                if (int.Parse(vals[32 + roi]) == 0)
                    vals[32 + roi] = "1";
                else
                    vals[32 + roi] = "0";
                

                for (int c = 0; c < fi.sizeC; c++)
                {
                    val = "roi.new(" + c.ToString() + "," +
                    ID.ToString() + ",{" +
                    vals[32 + roi] + "\n" +
                    type + "\n" +
                    (int.Parse(vals[6 + roi]) * 2).ToString() + "\n" +
                    (int.Parse(vals[8 + roi]) * 2).ToString() + "\n" +
                    "1\n" +
                    vals[10 + roi] + "\n" +
                    vals[4 + roi].Replace("-", "\n") + "\n" +
                    "1\n" + fi.sizeZ.ToString() + "\n" +
                    vals[26 + roi] + "\n" +
                    vals[26 + roi] + "\n" +
                    (true).ToString() + "\n" +
                     (true).ToString() + "\n" +
                     CT2_GetRoiLocation(vals[28 + roi], vals[30 + roi], vals[6 + roi], vals[8 + roi]) +
                    "\n})";

                    roi_new(val, fi);
                    ID++;
                }
            }

            fi.ROICounter = ID;
        }
        private string CT2_GetRoiLocation(string CxStr, string CyStr, string wStr, string hStr)
        {
            int W = int.Parse(wStr);
            int H = int.Parse(hStr);
            string[] Cx = CxStr.Split(new string[] { "\t" }, StringSplitOptions.None);
            string[] Cy = CyStr.Split(new string[] { "\t" }, StringSplitOptions.None);
            List<string> resList = new List<string>();
            for(int i = 0; i<Cx.Length; i++)
            {
                resList.Add((int.Parse(Cx[i]) - W).ToString());
                resList.Add((int.Parse(Cy[i]) - H).ToString());
            }

            return string.Join("\t", resList) ;
        }
        private bool CellTool2_ReadMetadata(string path, List<TabPage> Collection, TabPage tp, ImageAnalyser IA1)
        {
            try {
                //return false;
                //reading part
                TifFileInfo fi = tp.tifFI;
                fi.xAxisTB = 0;
                fi.yAxisTB = 1;
                fi.Dir = path;
                fi.original = false;

                string[] vals = null;
                Tiff image = Tiff.Open(OSStringConverter.StringToDir(path), "r");
                {
                    fi.sizeX = int.Parse(image.GetField(TiffTag.IMAGEWIDTH)[0].ToString());
                    fi.sizeY = int.Parse(image.GetField(TiffTag.IMAGELENGTH)[0].ToString());
                    fi.imageCount = image.NumberOfDirectories();
                    image.SetDirectory(0);
                    // read auto-registered tag 40000
                    FieldValue[] value = image.GetField((TiffTag)270);//CellTool3 tif tag
                    if (value != null)
                    {
                        vals = value[0].ToString().Split(new string[] { "\n" }, StringSplitOptions.None);
                        fi.FileDescription = value[0].ToString();
                    }
                    else
                    {
                        image.Close();
                        return false;
                    }

                }

                ReadCT2CoreMeta(vals, fi);
                {
                    try
                    {
                        image.SetDirectory((short)(image.NumberOfDirectories() - 1));
                    }
                    catch
                    {
                        return false;
                    }
                    // read auto-registered tag 40000
                    FieldValue[] value = image.GetField((TiffTag)40000);//CellTool3 tif tag
                    if (value != null)
                    {
                        //File.WriteAllText("D:\\Work\\Metadata\\CT2Meta.txt", value[1].ToString());
                        //return false;
                        vals = value[1].ToString().Split(new string[] { "\n" }, StringSplitOptions.None);
                    }
                    else
                    {
                        image.Close();
                        return false;
                    }
                    image.SetDirectory(0);
                }

                //read tags
                #region Segmentation variables
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

                int[] FilterHistory = ApplyCT2Tags(vals, fi);

                fi.bitsPerPixel = 16;
                fi.autoDetectBandC = true;

                vals = null;
                //time steps part
                {
                    image.SetDirectory((short)(image.NumberOfDirectories() - 1));
                    // read auto-registered tag 40000
                    FieldValue[] value = image.GetField((TiffTag)40003);//CellTool3 tif tag
                    fi.TimeSteps = new List<double>();
                    if (value != null)
                    {
                        //File.WriteAllText("D:\\Work\\Metadata\\CT2Meta.txt", value[1].ToString());
                        //return false;
                        vals = value[1].ToString().Split(new string[] { "\r\n" }, StringSplitOptions.None);
                        foreach (string val in vals)
                        {
                            fi.TimeSteps.Add(double.Parse(val));
                        }
                    }
                    else
                    {
                        fi.TimeSteps.Add((double)fi.imageCount);
                        fi.TimeSteps.Add((double)1);
                    }
                    image.SetDirectory(0);
                }
                //description part
                {
                    image.SetDirectory((short)(image.NumberOfDirectories() - 1));
                    // read auto-registered tag 40000
                    FieldValue[] value = image.GetField((TiffTag)40004);//CellTool3 tif tag
                    if (value != null)
                    {
                        //File.WriteAllText("D:\\Work\\Metadata\\CT2Meta.txt", value[1].ToString());
                        //return false;
                        //vals = value[1].ToString().Split(new string[] { "\n" }, StringSplitOptions.None);
                        fi.FileDescription = value[1].ToString();
                    }

                    image.SetDirectory(0);
                }
                //roi part
                {
                    image.SetDirectory((short)(image.NumberOfDirectories() - 1));
                    // read auto-registered tag 40000
                    FieldValue[] value = image.GetField((TiffTag)40001);//CellTool3 tif tag
                    if (value != null)
                    {
                        //File.WriteAllText("D:\\Work\\Metadata\\CT2Meta.txt", value[1].ToString());
                        //return false;
                        vals = value[1].ToString().Split(new string[] { "\n" }, StringSplitOptions.None);
                    }

                    image.SetDirectory(0);
                }
                CT2_ReadRoiSet(vals, fi);
                vals = null;
                //background worker
                var bgw = new BackgroundWorker();
                bgw.WorkerReportsProgress = true;
                bool loaded = false;
                //Add handlers to the backgroundworker
                bgw.DoWork += new DoWorkEventHandler(delegate (Object o, DoWorkEventArgs a)
                {
                //prepare array and read file
                int midFrame = fi.sizeC * fi.sizeZ;
                    switch (fi.bitsPerPixel)
                    {
                        case 8:
                            fi.image8bit = new byte[image.NumberOfDirectories()][][];
                            for (int i = 0; i < midFrame; i++)
                            {
                                if (i >= fi.imageCount) { break; }
                                Image8bit_readFrame(i, image, fi);
                            }
                            break;
                        case 16:
                            fi.image16bit = new ushort[image.NumberOfDirectories()][][];
                            for (int i = 0; i < midFrame; i++)
                            {
                                if (i >= fi.imageCount) { break; }
                                Image16bit_readFrame(i, image, fi);
                            }
                            break;
                    }
                    loaded = true;
                //report progress
                ((BackgroundWorker)o).ReportProgress(0);
                //parallel readers
                ImageReader_BGW(Collection, image, fi, IA1, path);

                    switch (fi.bitsPerPixel)
                    {
                        case 8:
                            fi.image8bitFilter = fi.image8bit;
                            break;
                        case 16:
                            fi.image16bitFilter = fi.image16bit;
                            break;
                    }

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

                    if (FilterHistory != null)
                        foreach (int i in FilterHistory)
                            for (int C = 0; C < fi.sizeC; C++)
                            {
                                string str = IA.Segmentation.MyFilters.DecodeOldFilters(C, i);
                                fi.tempNewFilterHistory[C].Add(str);
                            }
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
                    //report progress
                    ((BackgroundWorker)o).ReportProgress(1);

                    image.Close();
                });

                bgw.ProgressChanged += new ProgressChangedEventHandler(delegate (Object o, ProgressChangedEventArgs a)
                {
                    if (a.ProgressPercentage == 0)
                    {

                        fi.openedImages = fi.sizeC * fi.sizeZ;
                        try
                        {
                            IA1.ReloadImages();
                        }
                        catch { }

                    }
                    else if (a.ProgressPercentage == 1)
                    {
                        if (FilterHistory.Length > 0 &&
                                fi.tpTaskbar.MethodsBtnList[1].ImageIndex != 0)
                        {
                            fi.available = true;
                            fi.tpTaskbar.MethodsBtnList[1].ImageIndex = 0;
                            IA1.ReloadImages();
                        }

                        fi.openedImages = fi.imageCount;
                        fi.available = true;
                        fi.loaded = true;

                        CalculateAllRois(fi);

                        IA1.ReloadImages();
                        bool check = true;
                        foreach (TabPage tp1 in Collection)
                        {
                            if (tp1.tifFI != null && tp1.tifFI.available == false)
                            {
                                check = false;
                                break;
                            }
                        }
                        if (check == true)
                        {
                            loadingTimer.Stop();
                            StatusLabel.Text = "Ready";
                        }

                        if (IA.Segmentation.AutoSetUp.LibTB.SelectedIndex > 0 &&
               IA.Segmentation.AutoSetUp.ApplyToNewCheckB.Checked &&
                    MessageBox.Show("Do you want to open the image with the following protocol:\n" +
                    IA.Segmentation.AutoSetUp.LibTB.Text,
                               "", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                        {
                            IA.Segmentation.AutoSetUp.ApplyCT3Tags(IA.Segmentation.AutoSetUp.protocols[
                            IA.Segmentation.AutoSetUp.LibTB.SelectedIndex].Split(
                            new string[] { ";\n" }, StringSplitOptions.None), fi);
                        }
                    }
                });


                //Clear OldImage
                IA1.IDrawer.ClearImage();

                //start bgw
                //Start background worker
                StatusLabel.Text = "Reading Tif Image...";
                fi.available = false;
                bgw.RunWorkerAsync();
                //add taskbar
                tp.tifFI.tpTaskbar.Initialize(IA1.TabPages.ImageMainPanel, IA1, fi);

                if (FilterHistory.Length > 0 &&
                                fi.tpTaskbar.MethodsBtnList[1].ImageIndex != 0)
                {
                    fi.tpTaskbar.MethodsBtnList[1].ImageIndex = 0;
                }

                try
                {
                    if (loaded == true)
                    {
                        IA1.ReloadImages();
                    }
                }
                catch { }
                //out put
                return true;
            }
            catch { return false; }
        }
        #endregion Read CellTool2 metadata

        #region Read CellTool3 metadata
        private void readWholeFile(string path)
        {
            // read bytes of an image
            byte[] buffer = File.ReadAllBytes(OSStringConverter.StringToDir(path));

            // create a memory stream out of them
            MemoryStream ms = new MemoryStream(buffer);

            // open a Tiff stored in the memory stream
            using (Tiff image = Tiff.ClientOpen("in-memory", "r", ms, new TiffStream()))
            {
                image.SetDirectory((short)(image.NumberOfDirectories() - 1));
                MessageBox.Show("Done!");
            }
        }
        private bool CellTool3_ReadMetadata(string path, List<TabPage> Collection, TabPage tp, ImageAnalyser IA1)
        {
            string[] vals = null;
            //Check for file
            string newPath = FileEncoder.CheckForFileChain(path);
            bool isFileChain = false;
            if (newPath != path)
            {
                newPath = newPath.Substring(0, newPath.Length - 4);

                string curPath = FileEncoder.FileChain_GetName(0, newPath);
                if (File.Exists(OSStringConverter.StringToDir(curPath)))
                {
                    path = curPath;
                    isFileChain = true;
                }
            }
            
            Tiff image = Tiff.Open(OSStringConverter.StringToDir(path), "r");
            {
                try
                {
                    image.SetDirectory((short)(image.NumberOfDirectories() - 1));
                }
                catch
                {
                   return false;
                }
                
                //image.SetDirectory((short)(image.NumberOfDirectories() - 1));
                // read auto-registered tag 50341
                FieldValue[] value = image.GetField((TiffTag)40005);//CellTool3 tif tag
                if(value != null)
                {
                    //File.WriteAllText("D:\\Work\\Metadata\\CTMeta.txt", value[1].ToString());
                    vals = value[1].ToString().Split(new string[] { ";\n" }, StringSplitOptions.None);
                }
                else
                {
                    image.Close();
                    return false;
                }
                image.SetDirectory(0);
            }
            //reading part
            TifFileInfo fi = tp.tifFI;
            fi.Dir = path;
            fi.original = false;
            //fi.available = false;
            //read tags
            int[] FilterHistory = ApplyCT3Tags(vals, fi);
            
            vals = null;
            //background worker
            var bgw = new BackgroundWorker();
            bgw.WorkerReportsProgress = true;
            bool loaded = false;
            //Add handlers to the backgroundworker
            bgw.DoWork += new DoWorkEventHandler(delegate (Object o, DoWorkEventArgs a)
            {
                //prepare array and read file
                int midFrame = fi.sizeC * fi.sizeZ;
                switch (fi.bitsPerPixel)
                {
                    case 8:
                        fi.image8bit = new byte[fi.imageCount/*image.NumberOfDirectories()*/][][];
                        for (int i = 0; i < midFrame; i++)
                        {
                            if (i >= fi.imageCount) { break; }
                            Image8bit_readFrame(i, image, fi);
                        }
                        break;
                    case 16:
                        fi.image16bit = new ushort[fi.imageCount/*image.NumberOfDirectories()*/][][];
                        for (int i = 0; i < midFrame; i++)
                        {
                            if (i >= fi.imageCount) { break; }
                            Image16bit_readFrame(i, image, fi);
                        }
                        break;
                }
                loaded = true;
                //report progress
                ((BackgroundWorker)o).ReportProgress(0);
                //parallel readers
                if (!isFileChain)
                {
                    ImageReader_BGW(Collection, image, fi, IA1, path);
                }
                else
                {
                    ImageReader_BGW_FileChains(Collection, image, fi, IA1, path);
                }

                switch (fi.bitsPerPixel)
                {
                    case 8:
                        fi.image8bitFilter = fi.image8bit;
                        break;
                    case 16:
                        fi.image16bitFilter = fi.image16bit;
                        break;
                }

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

                if (FilterHistory != null)
                    foreach (int i in FilterHistory)
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
                //report progress
                ((BackgroundWorker)o).ReportProgress(1);

                image.Close();
            });

            bgw.ProgressChanged += new ProgressChangedEventHandler(delegate (Object o, ProgressChangedEventArgs a)
            {
                if (a.ProgressPercentage == 0)
                {

                    fi.openedImages = fi.sizeC * fi.sizeZ;
                    try
                    {
                        IA1.ReloadImages();
                    }
                    catch { }
                    
                }
                else if (a.ProgressPercentage == 1)
                {
                    if (fi.roiList != null)
                        foreach (List<ROI> rList in fi.roiList)
                            if (rList != null && rList.Count > 0)
                            {
                                fi.tpTaskbar.MethodsBtnList[2].ImageIndex = 0;
                                break;
                            }

                    //old filters check
                    if (FilterHistory != null && FilterHistory.Length > 0 &&
                                    fi.tpTaskbar.MethodsBtnList[1].ImageIndex != 0)
                    {
                        fi.tpTaskbar.MethodsBtnList[1].ImageIndex = 0;
                    }
                    //new filters check
                    if (fi.newFilterHistory != null)
                        foreach (List<string> strL in fi.newFilterHistory)
                            if (strL != null && strL.Count > 0)
                            {
                                fi.tpTaskbar.MethodsBtnList[1].ImageIndex = 0;
                                break;
                            }

                    fi.openedImages = fi.imageCount;
                    fi.available = true;
                    fi.loaded = true;

                    CalculateAllRois(fi);
                    IA1.ReloadImages();
                    bool check = true;
                    foreach (TabPage tp1 in Collection)
                    {
                        if (tp1.tifFI!= null && tp1.tifFI.available == false)
                        {
                            check = false;
                            break;
                        }
                    }
                    if (check == true)
                    {
                        loadingTimer.Stop();
                        StatusLabel.Text = "Ready";
                    }

                    if (IA.Segmentation.AutoSetUp.LibTB.SelectedIndex > 0 &&
                IA.Segmentation.AutoSetUp.ApplyToNewCheckB.Checked &&
                     MessageBox.Show("Do you want to open the image with the following protocol:\n" +
                     IA.Segmentation.AutoSetUp.LibTB.Text,
                                "", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                    {
                        IA.Segmentation.AutoSetUp.ApplyCT3Tags(IA.Segmentation.AutoSetUp.protocols[
                        IA.Segmentation.AutoSetUp.LibTB.SelectedIndex].Split(
                        new string[] { ";\n" }, StringSplitOptions.None), fi);
                    }
                }
            });

            //Clear OldImage
            IA1.IDrawer.ClearImage();
            
            //start bgw
            //Start background worker
            StatusLabel.Text = "Reading Tif Image...";
            fi.available = false;
            bgw.RunWorkerAsync();
            //add taskbar
            tp.tifFI.tpTaskbar.Initialize(IA1.TabPages.ImageMainPanel, IA1, fi);

            //old filters check
            if (FilterHistory != null && FilterHistory.Length > 0 &&
                            fi.tpTaskbar.MethodsBtnList[1].ImageIndex != 0)
            {
                fi.tpTaskbar.MethodsBtnList[1].ImageIndex = 0;
            }
            //new filters check
            if (fi.newFilterHistory != null)
                foreach (List<string> strL in fi.newFilterHistory)
                    if (strL != null && strL.Count > 0)
                    {
                        fi.tpTaskbar.MethodsBtnList[1].ImageIndex = 0;
                        break;
                    }

            try
            {
                if (loaded == true)
                {
                    IA1.ReloadImages();
                }
            }
            catch { }
            //out put
            return true;
        }
        public void CalculateAllRois(TifFileInfo fi)
        {
            if (fi.roiList != null)
                for (int i = 0; i < fi.sizeC; i++)
                    if (fi.roiList[i] != null)
                        foreach (ROI roi in fi.roiList[i])
                        {
                            RoiMeasure.Measure(roi, fi, i, IA);
                        }
        }
        public int[] ApplyCT3Tags(string[] BigVals, TifFileInfo fi)
        {
            int[] FilterHistory = null;
            string[] vals = null;

            foreach (string val in BigVals)
            {
                try
                {
                    vals = val.Split(new string[] { "->" }, StringSplitOptions.None);
                    switch (vals[0])
                    {
                        case ("seriesCount"):
                            fi.seriesCount = StringToTagValue(fi.seriesCount, vals[1]);
                            break;
                        case ("imageCount"):
                            fi.imageCount = StringToTagValue(fi.imageCount, vals[1]);
                            break;
                        case ("sizeX"):
                            fi.sizeX = StringToTagValue(fi.sizeX, vals[1]);
                            break;
                        case ("sizeY"):
                            fi.sizeY = StringToTagValue(fi.sizeY, vals[1]);
                            break;
                        case ("sizeC"):
                            fi.sizeC = StringToTagValue(fi.sizeC, vals[1]);
                            fi.roiList = new List<ROI>[fi.sizeC];
                            break;
                        case ("sizeZ"):
                            fi.sizeZ = StringToTagValue(fi.sizeZ, vals[1]);
                            break;
                        case ("sizeT"):
                            fi.sizeT = StringToTagValue(fi.sizeT, vals[1]);
                            break;
                        case ("umXY"):
                            fi.umXY = StringToTagValue(fi.umXY, vals[1]);
                            break;
                        case ("umZ"):
                            fi.umZ = StringToTagValue(fi.umZ, vals[1]);
                            break;
                        case ("bitsPerPixel"):
                            fi.bitsPerPixel = StringToTagValue(fi.bitsPerPixel, vals[1]);
                            break;
                        case ("dimensionOrder"):
                            fi.dimensionOrder = StringToTagValue(fi.dimensionOrder, vals[1]);
                            break;
                        case ("pixelType"):
                            fi.pixelType = StringToTagValue(fi.pixelType, vals[1]);
                            break;
                        case ("FalseColored"):
                            fi.FalseColored = StringToTagValue(fi.FalseColored, vals[1]);
                            break;
                        case ("isIndexed"):
                            fi.isIndexed = StringToTagValue(fi.isIndexed, vals[1]);
                            break;
                        case ("MetadataComplete"):
                            fi.MetadataComplete = StringToTagValue(fi.MetadataComplete, vals[1]);
                            break;
                        case ("DatasetStructureDescription"):
                            fi.DatasetStructureDescription = StringToTagValue(fi.DatasetStructureDescription, vals[1]);
                            break;
                        case ("Micropoint"):
                            fi.Micropoint = StringToTagValue(fi.Micropoint, vals[1]);
                            break;
                        case ("autoDetectBandC"):
                            fi.autoDetectBandC = StringToTagValue(fi.autoDetectBandC, vals[1]);
                            break;
                        case ("applyToAllBandC"):
                            fi.applyToAllBandC = StringToTagValue(fi.applyToAllBandC, vals[1]);
                            break;
                        case ("xCompensation"):
                            fi.xCompensation = StringToTagValue(fi.xCompensation, vals[1]);
                            break;
                        case ("yCompensation"):
                            fi.yCompensation = StringToTagValue(fi.yCompensation, vals[1]);
                            break;
                        case ("DataSourceInd"):
                            fi.DataSourceInd = StringToTagValue(fi.DataSourceInd, vals[1]);
                            break;
                        case ("LutList"):
                            fi.LutList = StringToTagValue(fi.LutList, vals[1]);
                            break;
                        case ("TimeSteps"):
                            fi.TimeSteps = StringToTagValue(fi.TimeSteps, vals[1]);
                            break;
                        case ("MinBrightness"):
                            fi.MinBrightness = StringToTagValue(fi.MinBrightness, vals[1]);
                            break;
                        case ("MaxBrightness"):
                            fi.MaxBrightness = StringToTagValue(fi.MaxBrightness, vals[1]);
                            break;
                        case ("tracking_MaxSize"):
                            fi.tracking_MaxSize = StringToTagValue(fi.tracking_MaxSize, vals[1]);
                            break;
                        case ("tracking_MinSize"):
                            fi.tracking_MinSize = StringToTagValue(fi.tracking_MinSize, vals[1]);
                            break;
                        case ("tracking_Speed"):
                            fi.tracking_Speed = StringToTagValue(fi.tracking_Speed, vals[1]);
                            break;
                        case ("SegmentationProtocol"):
                            fi.SegmentationProtocol = StringToTagValue(fi.SegmentationProtocol, vals[1]);
                            break;
                        case ("SegmentationCBoxIndex"):
                            fi.SegmentationCBoxIndex = StringToTagValue(fi.SegmentationCBoxIndex, vals[1]);
                            break;
                        case ("thresholdsCBoxIndex"):
                            fi.thresholdsCBoxIndex = StringToTagValue(fi.thresholdsCBoxIndex, vals[1]);
                            break;
                        case ("SelectedSpotThresh"):
                            fi.SelectedSpotThresh = StringToTagValue(fi.SelectedSpotThresh, vals[1]);
                            break;
                        case ("typeSpotThresh"):
                            fi.typeSpotThresh = StringToTagValue(fi.typeSpotThresh, vals[1]);
                            break;
                        case ("SpotThresh"):
                            fi.SpotThresh = StringToTagValue(fi.SpotThresh, vals[1]);
                            break;
                        case ("spotSensitivity"):
                            fi.spotSensitivity = StringToTagValue(fi.spotSensitivity, vals[1]);
                            break;
                        case ("thresholds"):
                            fi.thresholds = StringToTagValue(fi.thresholds, vals[1]);
                            break;
                        case ("SpotColor"):
                            fi.SpotColor = StringToTagValue(fi.SpotColor, vals[1]);
                            break;
                        case ("RefSpotColor"):
                            fi.RefSpotColor = StringToTagValue(fi.RefSpotColor, vals[1]);
                            break;
                        case ("sumHistogramChecked"):
                            fi.sumHistogramChecked = StringToTagValue(fi.sumHistogramChecked, vals[1]);
                            break;
                        case ("SpotTailType"):
                            fi.SpotTailType = StringToTagValue(fi.SpotTailType, vals[1]);
                            break;
                        case ("thresholdColors"):
                            fi.thresholdColors = StringToTagValue(fi.thresholdColors, vals[1]);
                            break;
                        case ("RefThresholdColors"):
                            fi.RefThresholdColors = StringToTagValue(fi.RefThresholdColors, vals[1]);
                            break;
                        case ("thresholdValues"):
                            fi.thresholdValues = StringToTagValue(fi.thresholdValues, vals[1]);
                            break;
                        case ("FileDescription"):
                            fi.FileDescription = StringToTagValue(fi.FileDescription, vals[1]);
                            break;
                        case ("roi.new"):
                            roi_new(vals[1], fi);
                            break;
                        case ("FilterHistory"):
                            FilterHistory = StringToTagValue(FilterHistory, vals[1]);
                            break;
                        case ("xAxisTB"):
                            fi.xAxisTB = int.Parse(vals[1]);
                            break;
                        case ("yAxisTB"):
                            fi.yAxisTB = int.Parse(vals[1]);

                            if (fi.yAxisTB >= IA.chart.Properties.yAxisTB.Items.Count)
                                fi.yAxisTB = 1;

                            break;
                        case ("yFormula"):                            
                            int ind = IA.chart.Properties.GetFunctionIndex(vals[1] + ";");
                            if (ind != -1)
                                fi.yAxisTB = ind;
                            break;
                        case ("watershed"):
                            fi.tempWatershedList.Clear();
                            fi.tempWatershedList = vals[1].Split(new string[] { "\n" }, StringSplitOptions.None).ToList();
                            break;
                        case ("newFilters"):
                            //if sizeC is changed or image is newly opened
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
                            fi.tempNewFilterHistory = StringToTagValue(fi.newFilterHistory, vals[1], fi.sizeC);
                            break;
                        default:
                            MessageBox.Show(vals[0]);
                            break;
                    }

                }
                catch
                {
                    MessageBox.Show("Error: \n" + vals[0] + ":\t" + vals[1]);
                }
            }
            return FilterHistory;
        }
        public static void roi_new(string val, TifFileInfo fi)
        {
            
            string[] vals = val.Substring(8,val.Length - 9).Split(new string[] { "," }, StringSplitOptions.None);
            int chanel = Convert.ToInt32(vals[0]);
            int RoiID = Convert.ToInt32(vals[1]);
            
            string RoiInfo = vals[2];

            ROI current = new ROI();
            current.CreateFromHistory(RoiID, RoiInfo);

            if (fi.roiList[chanel] == null) fi.roiList[chanel] = new List<ROI>();
            fi.roiList[chanel].Add(current);

            if (fi.ROICounter <= RoiID) fi.ROICounter = RoiID + 1;
        }
        private string StringToTagValue(string d, string val)
        {
            return val;
        }
        private int StringToTagValue(int d, string val)
        {
            return int.Parse(val);
        }
        private double StringToTagValue(double d, string val)
        {
            try
            {
                return double.Parse(val);
            }
            catch
            {
                if (val.Contains("."))
                    return double.Parse(val.Replace(".", ","));
                else if (val.Contains(","))
                    return double.Parse(val.Replace(",", "."));
                else
                    return 0;
            }
        }
        private bool StringToTagValue(bool d, string val)
        {
            return bool.Parse(val);
        }
        private int[][] StringToTagValue(int[][] d,string val)
        {
            List<int[]> res = new List<int[]>();
            foreach (string line in val.Split(new string[] { "\n" }, StringSplitOptions.None))
            {
                List<int> smallRes = new List<int>();
                foreach (string i in line.Split(new string[] { "\t" }, StringSplitOptions.None))
                    if (i != "")
                        smallRes.Add(int.Parse(i));
                res.Add(smallRes.ToArray());
            }

            return res.ToArray();
        }
        private List<string>[] StringToTagValue(List<string>[] d, string val, int c)
        {
            string[] vals = val.Split(new string[] { "\n" }, StringSplitOptions.None);

            List<string>[] res = new List<string>[c];

            for (int i = 0; i < c; i++)
                if (vals[i] != "")
                    res[i] = vals[i].Split(new string[] { "{}" }, StringSplitOptions.None).ToList();
           
            return res;
        }
        private string[] StringToTagValue(string[] d, string val)
        {
            List<string> res = new List<string>();
            foreach (string i in val.Split(new string[] { "\t" }, StringSplitOptions.None))
                if (i != "")
                    res.Add(i);

            return res.ToArray();
        }
        private List<double> StringToTagValue(List<double> d, string val)
        {
            List<double> res = new List<double>();
            foreach (string i in val.Split(new string[] { "\t" }, StringSplitOptions.None))
                if (i != "")
                    try
                    {
                        res.Add(double.Parse(i));
                    }
                    catch
                    {
                        if (i.Contains("."))
                            res.Add(double.Parse(i.Replace(".", ",")));
                        else if (i.Contains(","))
                            res.Add(double.Parse(i.Replace(",", ".")));
                    }

            return res;
        }
        private int[] StringToTagValue(int[] intArr, string val)
        {
            List<int> res = new List<int>();
            foreach (string i in val.Split(new string[] { "\t" }, StringSplitOptions.None))
                if (i != "")
                    try
                    {
                        res.Add(int.Parse(i));
                    }
                    catch { }

            return res.ToArray();
        }
        private bool[] StringToTagValue(bool[] bArr, string val)
        {
            List<bool> res = new List<bool>();
            foreach (string i in val.Split(new string[] { "\t" }, StringSplitOptions.None))
                if (i != "")
                    res.Add(bool.Parse(i));

            return res.ToArray();
        }
        private Color[][] StringToTagValue(Color[][] cBigList, string val)
        {
            List<Color[]> res = new List<Color[]>();
            foreach (string line in val.Split(new string[] { "\n" }, StringSplitOptions.None))
            {
                List<Color> smallRes = new List<Color>();
                foreach (string i in line.Split(new string[] { "\t" }, StringSplitOptions.None))
                    if (i != "")
                        smallRes.Add(ColorTranslator.FromHtml(i));
                res.Add(smallRes.ToArray());
            }

            return res.ToArray();
        }
        private Color[] StringToTagValue(Color[] cList, string val)
        {
            List<Color> smallRes = new List<Color>();
            foreach (string i in val.Split(new string[] { "\t" }, StringSplitOptions.None))
                if (i != "")
                    smallRes.Add(ColorTranslator.FromHtml(i));

            return smallRes.ToArray();
        }
        private List<Color> StringToTagValue(List<Color> cList, string val)
        {
            List<Color> smallRes = new List<Color>();
            foreach (string i in val.Split(new string[] { "\t" }, StringSplitOptions.None))
                if (i != "")
                    smallRes.Add(ColorTranslator.FromHtml(i));

            return smallRes;
        }
        #endregion Read CellTool3 metadata
    }
}
