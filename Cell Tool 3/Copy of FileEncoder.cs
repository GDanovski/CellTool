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
using BitMiracle.LibTiff.Classic;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cell_Tool_3
{
    class FileEncoder
    {
        public static void SaveTif(TifFileInfo fi, string dir, ImageAnalyser IA)
        {
            try {
                
                //return;
                //Save pixel data
                switch (fi.bitsPerPixel)
                {
                    case 8:
                        SaveTif_8bitRawData(fi, dir,IA);
                        break;
                    case 16:
                        SaveTif_16bitRawData(fi, dir,IA);
                        break;
                    default:
                        return;
                }
            //save metadata
            //string value = calculateCTTagValue(fi,IA);
            //image.SetField(TIFFTAG_CellTool_METADATA, value);
            //AddTag(dir, value);
            }
            catch
            {
                System.Windows.Forms.MessageBox.Show("Save file error!");
            }
        }
        #region Write Directory

        private static void SaveTif_8bitRawData(TifFileInfo fi, string fileName, ImageAnalyser IA)
        {
            // Register the extender callback 
            m_parentExtender = Tiff.SetTagExtender(TagExtender);
            string value = calculateCTTagValue(fi, IA);

            int numberOfPages = fi.imageCount;

            int width = fi.sizeX;
            int height = fi.sizeY;
            int samplesPerPixel = 1;
            int bitsPerSample = fi.bitsPerPixel;

            //check the size of the image and start the writer
            int writersCount = NumberOfTiffWriters(fi);
            if (writersCount == 1)
            {
                using (Tiff output = Tiff.Open(OSStringConverter.StringToDir(fileName), "w"))
                {
                    for (int page = 0; page < numberOfPages; page++)
                    {
                        byte[][] firstPageBuffer = fi.image8bit[page];
                        output.SetField(TiffTag.IMAGELENGTH, height);
                        output.SetField(TiffTag.IMAGEWIDTH, width);
                        output.SetField(TiffTag.SAMPLESPERPIXEL, samplesPerPixel);
                        output.SetField(TiffTag.BITSPERSAMPLE, bitsPerSample);
                        output.SetField(TiffTag.ORIENTATION, Orientation.TOPLEFT);
                        output.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG);
                        output.SetField(TiffTag.PHOTOMETRIC, Photometric.MINISBLACK);
                        output.SetField(TiffTag.ROWSPERSTRIP, output.DefaultStripSize(0));
                        output.SetField(TiffTag.COMPRESSION, Compression.NONE);
                        output.SetField(TiffTag.IMAGEDESCRIPTION, ImageDetails());
                        // specify that it's a page within the multipage file
                        output.SetField(TiffTag.SUBFILETYPE, FileType.PAGE);
                        // specify the page number
                        output.SetField(TiffTag.PAGENUMBER, page, numberOfPages);

                        if (page == numberOfPages - 1)
                        {
                            // set the custom tag
                            output.SetField(TIFFTAG_CellTool_METADATA, value);
                        }
                        for (int j = 0; j < height; ++j)
                            output.WriteScanline(firstPageBuffer[j], j);

                        output.WriteDirectory();
                    }
                }
            }
            else
            {
                string newFileName = CheckForFileChain(fileName);//Remove time chain suffix
                newFileName = newFileName.Substring(0, newFileName.Length - 4);//remove .tif extension

                int pageMaxPerWriter = NumberOfFramesPerPart(fi);//get maximal pages per writer

                fi.Dir = FileChain_GetName(0, newFileName);//set the filename to the first file from the chain
                
                Parallel.For(0, writersCount, (ind) =>
                        {
                            int start = ind * pageMaxPerWriter;
                            int stop = start + pageMaxPerWriter;
                            using (Tiff output = Tiff.Open(OSStringConverter.StringToDir(
                                FileChain_GetName(ind, newFileName)), "w"))
                            {
                                for (int page = 0; start < numberOfPages && start < stop; page++, start++)
                                {
                                    byte[][] firstPageBuffer = fi.image8bit[start];
                                    output.SetField(TiffTag.IMAGELENGTH, height);
                                    output.SetField(TiffTag.IMAGEWIDTH, width);
                                    output.SetField(TiffTag.SAMPLESPERPIXEL, samplesPerPixel);
                                    output.SetField(TiffTag.BITSPERSAMPLE, bitsPerSample);
                                    output.SetField(TiffTag.ORIENTATION, Orientation.TOPLEFT);
                                    output.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG);
                                    output.SetField(TiffTag.PHOTOMETRIC, Photometric.MINISBLACK);
                                    output.SetField(TiffTag.ROWSPERSTRIP, output.DefaultStripSize(0));
                                    output.SetField(TiffTag.COMPRESSION, Compression.NONE);
                                    output.SetField(TiffTag.IMAGEDESCRIPTION, ImageDetails());
                                    // specify that it's a page within the multipage file
                                    output.SetField(TiffTag.SUBFILETYPE, FileType.PAGE);
                                    // specify the page number
                                    output.SetField(TiffTag.PAGENUMBER, page, pageMaxPerWriter);

                                    if (start == numberOfPages - 1 || start == stop - 1)
                                    {
                                        // set the custom tag
                                        output.SetField(TIFFTAG_CellTool_METADATA, value);
                                    }
                                    for (int j = 0; j < height; ++j)
                                        output.WriteScanline(firstPageBuffer[j], j);

                                    output.WriteDirectory();
                                }
                            }
                        });
            }
            // restore previous tag extender
            Tiff.SetTagExtender(m_parentExtender);
        }
        private static void SaveTif_16bitRawData1(TifFileInfo fi, string fileName, ImageAnalyser IA)
        {
            // Register the extender callback 
            m_parentExtender = Tiff.SetTagExtender(TagExtender);
            string value = calculateCTTagValue(fi, IA);

            int numberOfPages = fi.imageCount;

            int width = fi.sizeX;
            int height = fi.sizeY;
            int samplesPerPixel = 1;
            int bitsPerSample = fi.bitsPerPixel;

            using (Tiff output = Tiff.Open(OSStringConverter.StringToDir(fileName), "w"))
            {
                for (int page = 0; page < numberOfPages; page++)
                {
                    ushort[][] image = fi.image16bit[page];
                    output.SetField(TiffTag.IMAGELENGTH, height);
                    output.SetField(TiffTag.IMAGEWIDTH, width);
                    output.SetField(TiffTag.SAMPLESPERPIXEL, samplesPerPixel);
                    output.SetField(TiffTag.BITSPERSAMPLE, bitsPerSample);
                    output.SetField(TiffTag.ORIENTATION, Orientation.TOPLEFT);
                    output.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG);
                    output.SetField(TiffTag.PHOTOMETRIC, Photometric.MINISBLACK);
                    output.SetField(TiffTag.ROWSPERSTRIP, output.DefaultStripSize(0));
                    output.SetField(TiffTag.COMPRESSION, Compression.NONE);
                    output.SetField(TiffTag.IMAGEDESCRIPTION, ImageDetails());
                    // specify that it's a page within the multipage file
                    output.SetField(TiffTag.SUBFILETYPE, FileType.PAGE);
                    // specify the page number
                    output.SetField(TiffTag.PAGENUMBER, page, numberOfPages);

                    if (page == numberOfPages - 1)
                    {
                        // set the custom tag  
                        output.SetField(TIFFTAG_CellTool_METADATA, value);
                    }
                    
                    for (int i = 0; i < height; i++)
                    {
                        ushort[] samples = image[i];

                        byte[] buffer = new byte[samples.Length * sizeof(ushort)];
                        Buffer.BlockCopy(samples, 0, buffer, 0, buffer.Length);

                        output.WriteScanline(buffer, i);
                    }

                    output.WriteDirectory();
                }
            }

            // restore previous tag extender
            Tiff.SetTagExtender(m_parentExtender);
        }
        private static int NumberOfTiffWriters(TifFileInfo fi)
        {
            double maxSize = (double)1.90 * 1024 * 1024 * 1024;// - 2 GB

            double fileSize = (double)fi.sizeX * fi.sizeY * fi.sizeC * fi.sizeZ * fi.sizeT;
            if (fi.bitsPerPixel == 16) fileSize *= 2;

            double ratio = fileSize / maxSize;
            int parts = (int)(Math.Floor(ratio) + 1);

            return parts;
        }
        private static int NumberOfFramesPerPart(TifFileInfo fi)
        {
            double maxSize = (double)1.90 * 1024 * 1024 * 1024;// -2 GB

            double fileSize = (double)fi.sizeX * fi.sizeY * fi.sizeC * fi.sizeZ;
            if (fi.bitsPerPixel == 16) fileSize *= 2;

            double ratio = maxSize / fileSize;
            int parts = (int)Math.Floor(ratio) * fi.sizeC * fi.sizeZ;
            //System.Windows.Forms.MessageBox.Show(parts.ToString());
            return parts;
        }
        public static string FileChain_GetName(int ind, string FileName)
        {
            string newInd = ind.ToString();
            for (int i = newInd.Length; i < 4; i++)
                newInd = "0" + newInd;

            return FileName + "_t" + newInd + ".tif";
        }
        public static string CheckForFileChain(string name)
        {
            string shortName = name.Substring(name.LastIndexOf("\\") + 1, name.Length - 4 - (name.LastIndexOf("\\") + 1));

            if (!shortName.Contains("_t")) return name;

            string[] vals = shortName.Split(new string[] { "_t" }, StringSplitOptions.None);

            string val = vals[vals.Length - 1].Replace(".tif","");
            vals = null;

	    int a;
            if (val.Length == 4 && int.TryParse(val, out a)) {
                return name.Substring(0, name.LastIndexOf("_")) + ".tif";
		}
            else
                return name;
        }
        private static void SaveTif_16bitRawData(TifFileInfo fi, string fileName, ImageAnalyser IA)
        {
            
            // Register the extender callback 
            m_parentExtender = Tiff.SetTagExtender(TagExtender);
            string value = calculateCTTagValue(fi, IA);

            int numberOfPages = fi.imageCount;

            int width = fi.sizeX;
            int height = fi.sizeY;
            int samplesPerPixel = 1;
            int bitsPerSample = fi.bitsPerPixel;

            //check the size of the image and start the writer
            int writersCount = NumberOfTiffWriters(fi);
            if (writersCount == 1)
            {
                using (Tiff output = Tiff.Open(OSStringConverter.StringToDir(fileName), "w"))
                {
                    for (int page = 0; page < numberOfPages; page++)
                    {
                        ushort[][] image = fi.image16bit[page];
                        output.SetField(TiffTag.IMAGELENGTH, height);
                        output.SetField(TiffTag.IMAGEWIDTH, width);
                        output.SetField(TiffTag.SAMPLESPERPIXEL, samplesPerPixel);
                        output.SetField(TiffTag.BITSPERSAMPLE, bitsPerSample);
                        output.SetField(TiffTag.ORIENTATION, Orientation.TOPLEFT);
                        output.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG);
                        output.SetField(TiffTag.PHOTOMETRIC, Photometric.MINISBLACK);
                        output.SetField(TiffTag.ROWSPERSTRIP, output.DefaultStripSize(0));
                        output.SetField(TiffTag.COMPRESSION, Compression.NONE);
                        output.SetField(TiffTag.IMAGEDESCRIPTION, ImageDetails());
                        // specify that it's a page within the multipage file
                        output.SetField(TiffTag.SUBFILETYPE, FileType.PAGE);
                        // specify the page number
                        output.SetField(TiffTag.PAGENUMBER, page, numberOfPages);

                        if (page == numberOfPages - 1)
                        {
                            // set the custom tag  
                            output.SetField(TIFFTAG_CellTool_METADATA, value);
                        }

                        for (int i = 0; i < height; i++)
                        {
                            ushort[] samples = image[i];

                            byte[] buffer = new byte[samples.Length * sizeof(ushort)];
                            Buffer.BlockCopy(samples, 0, buffer, 0, buffer.Length);

                            output.WriteScanline(buffer, i);
                        }

                        output.WriteDirectory();
                    }
                }
            }
            else
            {
                string newFileName = CheckForFileChain(fileName);//Remove time chain suffix
                newFileName = newFileName.Substring(0, newFileName.Length - 4);//remove .tif extension

                int pageMaxPerWriter = NumberOfFramesPerPart(fi);//get maximal pages per writer

                fi.Dir = FileChain_GetName(0, newFileName);//set the filename to the first file from the chain

                Parallel.For(0, writersCount, (ind)=>
                {
                    int start = ind * pageMaxPerWriter;
                    int stop = start + pageMaxPerWriter;
                    using (Tiff output = Tiff.Open(OSStringConverter.StringToDir(FileChain_GetName(ind, newFileName)), "w"))
                    {
                        for (int page = 0; start < numberOfPages && start < stop; page++, start++)
                        {
                            ushort[][] image = fi.image16bit[start];
                            output.SetField(TiffTag.IMAGELENGTH, height);
                            output.SetField(TiffTag.IMAGEWIDTH, width);
                            output.SetField(TiffTag.SAMPLESPERPIXEL, samplesPerPixel);
                            output.SetField(TiffTag.BITSPERSAMPLE, bitsPerSample);
                            output.SetField(TiffTag.ORIENTATION, Orientation.TOPLEFT);
                            output.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG);
                            output.SetField(TiffTag.PHOTOMETRIC, Photometric.MINISBLACK);
                            output.SetField(TiffTag.ROWSPERSTRIP, output.DefaultStripSize(0));
                            output.SetField(TiffTag.COMPRESSION, Compression.NONE);
                            output.SetField(TiffTag.IMAGEDESCRIPTION, ImageDetails());
                            // specify that it's a page within the multipage file
                            output.SetField(TiffTag.SUBFILETYPE, FileType.PAGE);
                            // specify the page number
                            output.SetField(TiffTag.PAGENUMBER, page, pageMaxPerWriter);

                            if (start == numberOfPages - 1 || start == stop - 1)
                            {
                                // set the custom tag  
                                output.SetField(TIFFTAG_CellTool_METADATA, value);
                            }

                            for (int i = 0; i < height; i++)
                            {
                                ushort[] samples = image[i];

                                byte[] buffer = new byte[samples.Length * sizeof(ushort)];
                                Buffer.BlockCopy(samples, 0, buffer, 0, buffer.Length);

                                output.WriteScanline(buffer, i);
                            }

                            output.WriteDirectory();
                        }
                    }
                });
            }
            // restore previous tag extender
            Tiff.SetTagExtender(m_parentExtender);
        }
        private static string ImageDetails()
        {
            string val = "CellTool tif file\nProgram name: CellTool\nProgram version:" +
                System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString() +
                "\n";
            return val;
        }
        #endregion Write Directory

        #region Custom Tag
        private const TiffTag TIFFTAG_CellTool_METADATA = (TiffTag)40005;

        private static Tiff.TiffExtendProc m_parentExtender;

        private static void TagExtender(Tiff tif)
        {
            TiffFieldInfo[] tiffFieldInfo =
            {
                new TiffFieldInfo(TIFFTAG_CellTool_METADATA, -1, -1, TiffType.ASCII,
                    FieldBit.Custom, true, false, "CellTool_Metadata"),
            };
            
            tif.MergeFieldInfo(tiffFieldInfo, tiffFieldInfo.Length);

            if (m_parentExtender != null)
                m_parentExtender(tif);
        }
        private static string calculateCTTagValue(TifFileInfo fi, ImageAnalyser IA)
        {
            List<string> vals = new List<string>();
            vals.Add("seriesCount->" + fi.seriesCount.ToString());
            vals.Add("imageCount->" + fi.imageCount.ToString());
            vals.Add("sizeX->" + fi.sizeX.ToString());
            vals.Add("sizeY->" + fi.sizeY.ToString());
            vals.Add("sizeC->" + fi.sizeC.ToString());
            vals.Add("sizeZ->" + fi.sizeZ.ToString());
            vals.Add("sizeT->" + fi.sizeT.ToString());
            vals.Add("umXY->" + fi.umXY.ToString());
            vals.Add("umZ->" + fi.umZ.ToString());
            vals.Add("bitsPerPixel->" + fi.bitsPerPixel.ToString());
            vals.Add("dimensionOrder->" + fi.dimensionOrder);
            vals.Add("pixelType->" + fi.pixelType.ToString());
            vals.Add("FalseColored->" + fi.FalseColored.ToString());
            vals.Add("isIndexed->" + fi.isIndexed.ToString());
            vals.Add("MetadataComplete->" + fi.MetadataComplete.ToString());
            vals.Add("DatasetStructureDescription->" + fi.DatasetStructureDescription);
            vals.Add("Micropoint->" + fi.Micropoint.ToString());
            vals.Add("autoDetectBandC->" + fi.autoDetectBandC.ToString());
            vals.Add("applyToAllBandC->" + fi.applyToAllBandC.ToString());
            vals.Add("xCompensation->" + fi.xCompensation.ToString());
            vals.Add("yCompensation->" + fi.yCompensation.ToString());
            vals.Add("DataSourceInd->" + fi.DataSourceInd.ToString());
            vals.Add("LutList->" + TagValueToString(fi.LutList));
            vals.Add("TimeSteps->" + TagValueToString(fi.TimeSteps));
            vals.Add("MinBrightness->" + TagValueToString(fi.MinBrightness));
            vals.Add("MaxBrightness->" + TagValueToString(fi.MaxBrightness));
            vals.Add("tracking_MaxSize->" + TagValueToString(fi.tracking_MaxSize));
            vals.Add("tracking_MinSize->" + TagValueToString(fi.tracking_MinSize));
            vals.Add("tracking_Speed->" + TagValueToString(fi.tracking_Speed));
            vals.Add("SegmentationProtocol->" + TagValueToString(fi.SegmentationProtocol));
            vals.Add("SegmentationCBoxIndex->" + TagValueToString(fi.SegmentationCBoxIndex));
            vals.Add("thresholdsCBoxIndex->" + TagValueToString(fi.thresholdsCBoxIndex));
            vals.Add("SelectedSpotThresh->" + TagValueToString(fi.SelectedSpotThresh));
            vals.Add("typeSpotThresh->" + TagValueToString(fi.typeSpotThresh));
            vals.Add("SpotThresh->" + TagValueToString(fi.SpotThresh));
            vals.Add("spotSensitivity->" + TagValueToString(fi.spotSensitivity));
            vals.Add("thresholds->" + TagValueToString(fi.thresholds));
            vals.Add("SpotColor->" + TagValueToString(fi.SpotColor));
            vals.Add("RefSpotColor->" + TagValueToString(fi.RefSpotColor));
            vals.Add("sumHistogramChecked->" + TagValueToString(fi.sumHistogramChecked));
            vals.Add("SpotTailType->" + string.Join("\t", fi.SpotTailType));
            vals.Add("thresholdColors->" + TagValueToString(fi.thresholdColors));
            vals.Add("RefThresholdColors->" + TagValueToString(fi.RefThresholdColors));
            vals.Add("thresholdValues->" + TagValueToString(fi.thresholdValues));
            vals.Add("FileDescription->" + fi.FileDescription.Replace(";",".").Replace("->",": "));
            vals.Add("xAxisTB->" + fi.xAxisTB.ToString());
            vals.Add("yAxisTB->" + fi.yAxisTB.ToString());
            if (fi.yAxisTB >= 5)
            {
                vals.Add("yFormula->" + IA.chart.Properties.GetFunction(fi.yAxisTB).Replace(";", ""));
            }
            
            //Roi part
            int c = 0;
            foreach (List<ROI> roiList in fi.roiList)
            {
                if (roiList != null)
                    foreach (ROI roi in roiList)
                        vals.Add("roi.new->"+IA.RoiMan.roi_new(c, roi));
                c++;
            }

            //it is important FilterHistory to be the last
            //vals.Add("FilterHistory->" + TagValueToString(fi.FilterHistory.ToArray()));

            //if (fi.watershedList.Count != 0)
                //vals.Add("watershed->" + string.Join("\n", fi.watershedList));

            if(fi.newFilterHistory!= null)
                vals.Add("newFilters->" + TagValueToString(fi.newFilterHistory));

            return string.Join(";\n", vals);
        }
        public static string TagValueToString(List<string>[] intList)
        {
            string val = "";

            foreach (List<string> line in intList)
            {
                val += string.Join("{}",line) +  "\n";
            }

            return val;
        }
        public static string TagValueToString(int[][] intList)
        {
            string val = "";
            foreach (int[] line in intList)
            {
                foreach (int i in line)
                    val += i.ToString() + "\t";
                val += "\n";
            }
            return val;
        }
        public static string TagValueToString(List<double> dList)
        {
            string val = "";
            foreach (double d in dList)
                val += d.ToString() + "\t";
            return val;
        }
        public static string TagValueToString(int[] intArr)
        {
            string val = "";
            foreach (int i in intArr)
                val += i.ToString() + "\t";
            return val;
        }
        public static string TagValueToString(bool[] bArr)
        {
            string val = "";
            foreach (bool b in bArr)
                val += b.ToString() + "\t";
            return val;
        }
        public static string TagValueToString(Color[][] cBigList)
        {
            string val = "";

            foreach (Color[] cList in cBigList)
            {
                foreach (Color c in cList)
                    val += ColorTranslator.ToHtml(c) + "\t";
                val += "\n";
            }

            return val;
        }
        public static string TagValueToString(Color[] cList)
        {
            string val = "";
            foreach (Color c in cList)
                val += ColorTranslator.ToHtml(c) + "\t";
            return val;
        }
        public static string TagValueToString(List<Color> cList)
        {
            string val = "";
            foreach (Color c in cList)
                val += ColorTranslator.ToHtml(c) + "\t";
            return val;
        }
        /*
        private static void AddTag(string dir, string value)
        {
            // Register the extender callback 
            m_parentExtender = Tiff.SetTagExtender(TagExtender);
            
            using (Tiff image = Tiff.Open(dir, "a"))
            {
                image.SetDirectory((short)(image.NumberOfDirectories() - 1));
                // we should rewind to first directory (first image) because of append mode
                
                // set the custom tag  
                image.SetField(TIFFTAG_CellTool_METADATA, value);

                // rewrites directory saving new tag
                image.CheckpointDirectory();
            }

            // restore previous tag extender
            Tiff.SetTagExtender(m_parentExtender);
        }
        */
        #endregion Custom Tag
    }
}
