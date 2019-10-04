/*
 CellTool - software for bio-image analysis
 Copyright (C) 2019  Georgi Danovski
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
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.ComponentModel;
using System.Drawing;
///using Microsoft.VisualBasic.Devices;
using BitMiracle.LibTiff.Classic;

namespace Cell_Tool_3
{
    class BigImagesReader
    {
        /// <summary>
        /// Check the file size and if there is no avaliable RAM - open only a fragment of the image
        /// </summary>
        /// <param name="path"></param>
        /// <param name="Collection"></param>
        /// <param name="tp"></param>
        /// <param name="IA1"></param>
        /// <returns></returns>
        public static bool OpenBigImage(string path, List<TabPage> Collection, TabPage tp, ImageAnalyser IA)
        {
            //Check the file size in bytes
            long FileSize = new System.IO.FileInfo(path).Length;
            //Check the free ram memory in bytes
            //long RAM = (long)new ComputerInfo().AvailablePhysicalMemory;
            long RAM = 10000000;
            if (FileSize < RAM)
                return false;
            else
            {
                loci.formats.ChannelSeparator reader;
                //read file metadata
                try
                {
                    reader = ReadMetadata(path, Collection, tp, IA);
                }
                catch
                {
                    MessageBox.Show("Error: Not avaliable file format!");
                    IA.FileBrowser.StatusLabel.Text = "Ready";
                    return false;
                }

                if (reader == null)
                {
                    MessageBox.Show("Error: Not avaliable file format!");
                    IA.FileBrowser.StatusLabel.Text = "Ready";
                    return false;
                }

                //get new file dimentions
                int[][] matrix = null;
                while (!CheckSize(RAM, matrix, tp.tifFI))
                {
                    matrix = SubstackDialog(tp.tifFI, IA.TabPages.FileBrowser.StatusLabel);

                    if (matrix == null)
                    {
                        reader.close();
                        reader = null;
                        IA.FileBrowser.StatusLabel.Text = "Ready";
                        return false;
                    }
                }

                return SubstackToolStripMenuItem_click(path, Collection, tp, IA, reader, matrix);
            }
        }
        private static bool CheckSize(long RAM, int[][] matrix, TifFileInfo fi)
        {
            long FileSize = 0;
            if (matrix != null)
                FileSize =
                (long)(fi.sizeX * fi.sizeY * fi.bitsPerPixel)//frame size in bits
                * (long)((matrix[0][1] - matrix[0][0] + 1) / matrix[0][2])// T slices
                * (long)((matrix[1][1] - matrix[1][0] + 1) / matrix[1][2]) //Z slices
                * (long)((matrix[2][1] - matrix[2][0] + 1) / matrix[2][2]); //C chanels
            else
                FileSize =
                (long)(fi.sizeX * fi.sizeY * fi.bitsPerPixel) * (long)fi.sizeT * (long)fi.sizeZ * (long)fi.sizeC;

            if (FileSize < RAM)
            {
                return true;
            }
            else
            {
                MessageBox.Show("Not enough RAM memory!\n You can open " + RAM / (fi.sizeX * fi.sizeY * fi.bitsPerPixel) + " frames");
                return false;
            }
        }
        private static loci.formats.ChannelSeparator ReadMetadata(string path, List<TabPage> Collection, TabPage tp, ImageAnalyser IA)
        {
            // read metadata using ImageReader            
            loci.formats.ImageReader FirstReader = new loci.formats.ImageReader();
            try
            {
                FirstReader.setId(path);
            }
            catch
            {
                FirstReader.close();
                IA.FileBrowser.StatusLabel.Text = "Ready";
                return null;
            }

            bool isRGB = FirstReader.isRGB();
            //check is it rgb colored
            loci.formats.ChannelSeparator reader = loci.formats.ChannelSeparator.makeChannelSeparator(FirstReader);
            FirstReader = null;

            TifFileInfo fi = tp.tifFI;
            fi.seriesCount = reader.getSeriesCount();
            //Select which series to open!!!!!
            int ser = SelectSeries(reader, IA.TabPages.FileBrowser.StatusLabel);
            if (ser == -1)
            {
                fi = null;
                reader.close();
                IA.FileBrowser.StatusLabel.Text = "Ready";
                return null;
            }
            else
            {
                reader.setSeries(ser);
            }
            //Check file bits per pixel - currently supported: 8 bit GrayScale, 16 bit GrayScale
            fi.bitsPerPixel = reader.getBitsPerPixel();
            if (fi.bitsPerPixel <= 8)
                fi.bitsPerPixel = 8;
            else if (fi.bitsPerPixel <= 16)
                fi.bitsPerPixel = 16;
            else
            {
                fi = null;
                reader.close();
                IA.FileBrowser.StatusLabel.Text = "Ready";
                return null;
            }
            //Check is the metadata complieted and return message if not
            /*
            if (reader.isMetadataComplete() == false)
            {
                MessageBox.Show("Metadata is not complete!");
            }
            */
            //read tags

            fi.imageCount = reader.getImageCount();
            fi.sizeX = reader.getSizeX();
            fi.sizeY = reader.getSizeY();
            fi.sizeZ = reader.getSizeZ();
            fi.sizeC = reader.getSizeC();
            fi.sizeT = reader.getSizeT();
            //fi.dimensionOrder = reader.getDimensionOrder();
            fi.dimensionOrder = "XYCZT";
            fi.umZ = 0;
            fi.umXY = 0;
            fi.pixelType = reader.getPixelType();
            fi.FalseColored = reader.isFalseColor();
            fi.isIndexed = reader.isIndexed();
            fi.MetadataComplete = reader.isMetadataComplete();
            fi.DatasetStructureDescription = reader.getDatasetStructureDescription();
            string description = getLibTifFileDescription(path);
            fi.FileDescription = string.Join("\n", new string[] {"-----------------","CoreMetadata:\n", reader.getCoreMetadataList().ToString() ,
            "-----------------","GlobalMetadata:\n", reader.getGlobalMetadata().ToString(),
             "-----------------","SeriesMetadata:\n", reader.getSeriesMetadata().ToString(),
             "-----------------","FileDescription:\n", description});
            //Apply def settings
            fi.Dir = path;
            fi.xAxisTB = IA.chart.Properties.xAxisTB.SelectedIndex;
            fi.yAxisTB = IA.chart.Properties.yAxisTB.SelectedIndex;
            fi.available = false;
            fi.original = false;
            //Create LUT table
            if (isRGB)
            {
                fi.LutList = new List<Color>() {
                    Color.FromArgb(255,255,0,0),
                    Color.FromArgb(255,0,255,0),
                    Color.FromArgb(255,0,0,255)
                };
            }
            else
            {
                fi.LutList = new List<Color>();
                for (int i = 0; i < fi.sizeC; i++)
                {
                    fi.LutList.Add(Color.White);
                }
            }
            //Create time steps table
            fi.TimeSteps = new List<double>();
            fi.TimeSteps.Add(fi.imageCount + 1);
            fi.TimeSteps.Add(1);

            //If its IQ3 format or Dragonflye - try to read the colors and the timesteps
            TryAndorDecoders(fi, description);

            if (fi.sizeC == 0) { fi.sizeC = 1; }
            if (fi.sizeZ == 0) { fi.sizeZ = 1; }
            if (fi.sizeT == 0) { fi.sizeT = 1; }

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
            return reader;
        }
        private static string getLibTifFileDescription(string dir)
        {
            string des = "";
            using (Tiff tif = Tiff.Open(dir, "r"))
            {
                if (tif == null)
                    return "";

                tif.SetDirectory(0);
                TiffTag tag = (TiffTag)270;
                FieldValue[] value = tif.GetField(tag);

                if (value != null)
                    for (int i = 0; i < value.Length; i++)
                        des += Encoding.UTF8.GetString(value[i].ToByteArray());

                tif.Close();
            }

            return des;
        }
        private static int SelectSeries(loci.formats.ChannelSeparator reader, ToolStripStatusLabel StatusLabel)
        {
            int res = -1;
            if (reader.getSeriesCount() == 1)
                res = 0;
            else
            {
                Form OptionForm = new Form();
                OptionForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                OptionForm.Text = "Series Options";
                OptionForm.StartPosition = FormStartPosition.CenterScreen;
                OptionForm.WindowState = FormWindowState.Normal;
                OptionForm.MinimizeBox = false;
                OptionForm.MaximizeBox = false;

                OptionForm.Width = 250;
                OptionForm.Height = 120;

                Label lab = new Label();
                lab.Text = "Name:";
                lab.Location = new Point(10, 10);
                lab.Width = 60;
                OptionForm.Controls.Add(lab);

                ComboBox cmbBox = new ComboBox();
                cmbBox.Width = 150;
                cmbBox.Location = new Point(70, 10);
                OptionForm.Controls.Add(cmbBox);

                for (int ind = 1; ind <= reader.getSeriesCount(); ind++)
                    cmbBox.Items.Add("Series " + ind);

                cmbBox.SelectedIndex = 0;

                Panel okBox = new Panel();
                okBox.Height = 40;
                okBox.Dock = DockStyle.Bottom;
                OptionForm.Controls.Add(okBox);

                Button okBtn = new Button();
                okBtn.Text = "Process";
                okBtn.BackColor = System.Drawing.SystemColors.ButtonFace;
                okBtn.ForeColor = System.Drawing.Color.Black;
                okBtn.Location = new System.Drawing.Point(20, 10);
                okBtn.Anchor = AnchorStyles.Top | AnchorStyles.Left;
                okBox.Controls.Add(okBtn);

                Button cancelBtn = new Button();
                cancelBtn.Text = "Cancel";
                cancelBtn.BackColor = System.Drawing.SystemColors.ButtonFace;
                cancelBtn.Location = new System.Drawing.Point(OptionForm.Width - cancelBtn.Width - 40, 10);
                cancelBtn.ForeColor = System.Drawing.Color.Black;
                cancelBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
                okBox.Controls.Add(cancelBtn);

                okBtn.Click += new EventHandler(delegate (object sender, EventArgs e)
                {
                    res = cmbBox.SelectedIndex;
                    OptionForm.Close();
                });

                cancelBtn.Click += new EventHandler(delegate (object sender, EventArgs e)
                {
                    OptionForm.Close();
                });

                OptionForm.KeyPreview = true;
                OptionForm.KeyDown += new KeyEventHandler(delegate (object sender, KeyEventArgs e)
                {
                    switch (e.KeyCode)
                    {
                        case Keys.Escape:
                            OptionForm.Close();
                            break;
                        case Keys.Enter:
                            okBtn.PerformClick();
                            break;
                        default:
                            break;
                    }
                });

                StatusLabel.Text = "Dialog open";
                OptionForm.ShowDialog();
                OptionForm.Dispose();
                StatusLabel.Text = "Ready";                
            }
            return res;
        }
        private static void TryAndorDecoders(TifFileInfo fi, string tagText)
        {
            //decoders
            try
            {
                andor_IQ_Decoder(tagText, fi.imageCount - 1, fi);
                Andor_Dragonfly_decoder(tagText, fi.imageCount - 1, fi);
            }
            catch { }

            if (fi.TimeSteps == null)
            {
                fi.TimeSteps = new List<double>();
                fi.TimeSteps.Add(fi.imageCount + 1);
                fi.TimeSteps.Add(1);
            }
        }
        private static void Andor_Dragonfly_decoder(string txt, int Pages, TifFileInfo fi)
        {
            if (txt.IndexOf("</OME:OME>") > -1)
            {
                string subStr = txt.Substring(txt.IndexOf("<OME:Pixels"), txt.IndexOf("</OME:Pixels>") - txt.IndexOf("<OME:Pixels"));
                string[] core = subStr.Substring(12, subStr.IndexOf(">") - 12).Split(new string[] { "\" " }, StringSplitOptions.None);
                //write core metadata to file info
                foreach (string str in core)
                {
                    string[] vals = str.Replace(" ", "").Split(new string[] { "=\"" }, StringSplitOptions.None);
                    switch (vals[0])
                    {
                        case "PhysicalSizeX":
                            fi.umXY = double.Parse(vals[1]);
                            break;
                        case "PhysicalSizeZ":
                            fi.umZ = double.Parse(vals[1]);
                            break;
                    }
                }

                string[] colors = subStr.Split(new string[] { "Color=\"" }, StringSplitOptions.None);

                List<Color> Lut = new List<Color>();
                for (int i = 0; i < fi.sizeC; i++)
                {
                    if (colors.Length - 1 > i)
                    {
                        string col = colors[i + 1].Substring(0, colors[i + 1].IndexOf("\""));
                        try
                        {
                            Lut.Add(ColorTranslator.FromOle(int.Parse(col)));
                        }
                        catch
                        {
                            Lut.Add(Color.White);
                        }
                    }
                    else
                    {
                        Lut.Add(Color.White);
                    }
                }

                if (Lut.Count == fi.sizeC)
                    fi.LutList = Lut;

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
                        case "PhysicalSizeX":
                            fi.umXY = double.Parse(vals[1]);
                            break;
                        case "PhysicalSizeZ":
                            fi.umZ = double.Parse(vals[1]);
                            break;
                    }
                }

                string[] colors = subStr.Split(new string[] { "Color=\"" }, StringSplitOptions.None);

                List<Color> Lut = new List<Color>();
                for (int i = 0; i < fi.sizeC; i++)
                {
                    if (colors.Length - 1 > i)
                    {
                        string col = colors[i + 1].Substring(0, colors[i + 1].IndexOf("\""));
                        try
                        {
                            Lut.Add(ColorTranslator.FromOle(int.Parse(col)));
                        }
                        catch
                        {
                            Lut.Add(Color.White);
                        }
                    }
                    else
                    {
                        Lut.Add(Color.White);
                    }
                }

                if (Lut.Count == fi.sizeC)
                    fi.LutList = Lut;
            }
        }
        private static void andor_IQ_Decoder(string txt, int Pages, TifFileInfo fi)
        {
            if (!(txt.IndexOf("Protocol") > -1)) return;

            int MoveXYcounter = 0;

            List<Color> LutList = new List<Color>();
            Boolean zCheck = false;

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
                            //fi.sizeZ = int.Parse(val);
                            fi.umZ /= (fi.sizeZ - 1);
                        }
                        else if (count == 0)
                        {
                            fi.umZ = double.Parse(val);
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

            //read time steps
            List<double> TimeSteps = new List<double>();
            foreach (string row in txt.Split(new[] { "\n" }, StringSplitOptions.None))
            {
                if (row.IndexOf("Repeat T - ") > -1)
                {
                    string str = row.Replace(" - fastest", "").Replace("	", "").Replace("Repeat T - ", "")
                        .Replace(" times (", "-").Replace(" time (", "-")
                        .Replace(" sec)", "").Replace(" ms)", "").Replace(" min)", "");
                    foreach (string val in str.Split(new[] { "-" }, StringSplitOptions.None))
                    {
                        TimeSteps.Add(double.Parse(val));
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
                    fi.Dir.LastIndexOf("\\") + 1,
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
                    for (int z = LutList.Count - 1; z >= 0; z--)
                        if (z != i)
                            LutList.RemoveAt(z);
                }
            }
            if (LutList.Count == fi.sizeC) { fi.LutList = LutList; }
        }
        private static int[][] SubstackDialog(TifFileInfo fi, ToolStripStatusLabel StatusLabel)
        {
            int[][] res = null;

            Form OptionForm = new Form();
            OptionForm.FormBorderStyle = FormBorderStyle.FixedDialog;
            OptionForm.Text = "Substack";
            OptionForm.StartPosition = FormStartPosition.CenterScreen;
            OptionForm.WindowState = FormWindowState.Normal;
            OptionForm.MinimizeBox = false;
            OptionForm.MaximizeBox = false;

            OptionForm.Width = 270;
            OptionForm.Height = 230;

            #region Titles
            {
                Label lab1 = new Label();
                lab1.Text = "Dimension:";
                lab1.Width = 70;
                lab1.Location = new System.Drawing.Point(5, 15);
                OptionForm.Controls.Add(lab1);
            }
            {
                Label lab1 = new Label();
                lab1.Text = "From:";
                lab1.Width = 50;
                lab1.Location = new System.Drawing.Point(80, 15);
                OptionForm.Controls.Add(lab1);
            }
            {
                Label lab1 = new Label();
                lab1.Text = "To:";
                lab1.Width = 50;
                lab1.Location = new System.Drawing.Point(135, 15);
                OptionForm.Controls.Add(lab1);
            }
            {
                Label lab1 = new Label();
                lab1.Text = "Step:";
                lab1.Width = 50;
                lab1.Location = new System.Drawing.Point(190, 15);
                OptionForm.Controls.Add(lab1);
            }
            #endregion Titles

            #region Time
            Label TimeLab = new Label();
            TimeLab.Text = "T";
            TimeLab.Width = 50;
            TimeLab.Location = new System.Drawing.Point(5, 47);
            OptionForm.Controls.Add(TimeLab);

            TextBox tbTimeFrom = new TextBox();
            tbTimeFrom.Text = "1";
            tbTimeFrom.Width = 50;
            tbTimeFrom.Location = new System.Drawing.Point(80, 45);
            OptionForm.Controls.Add(tbTimeFrom);

            TextBox tbTimeTo = new TextBox();
            tbTimeTo.Text = fi.sizeT.ToString();
            tbTimeTo.Width = 50;
            tbTimeTo.Location = new System.Drawing.Point(135, 45);
            OptionForm.Controls.Add(tbTimeTo);

            TextBox tbTimeStep = new TextBox();
            tbTimeStep.Text = "1";
            tbTimeStep.Width = 50;
            tbTimeStep.Location = new System.Drawing.Point(190, 45);
            OptionForm.Controls.Add(tbTimeStep);

            #endregion Time
            #region Z
            Label ZLab = new Label();
            ZLab.Text = "Z";
            ZLab.Width = 50;
            ZLab.Location = new System.Drawing.Point(5, 77);
            OptionForm.Controls.Add(ZLab);

            TextBox tbZFrom = new TextBox();
            tbZFrom.Text = "1";
            tbZFrom.Width = 50;
            tbZFrom.Location = new System.Drawing.Point(80, 75);
            OptionForm.Controls.Add(tbZFrom);

            TextBox tbZTo = new TextBox();
            tbZTo.Text = fi.sizeZ.ToString();
            tbZTo.Width = 50;
            tbZTo.Location = new System.Drawing.Point(135, 75);
            OptionForm.Controls.Add(tbZTo);

            TextBox tbZStep = new TextBox();
            tbZStep.Text = "1";
            tbZStep.Width = 50;
            tbZStep.Location = new System.Drawing.Point(190, 75);
            OptionForm.Controls.Add(tbZStep);

            #endregion Z

            #region C
            Label CLab = new Label();
            CLab.Text = "C";
            CLab.Width = 50;
            CLab.Location = new System.Drawing.Point(5, 107);
            OptionForm.Controls.Add(CLab);

            TextBox tbCFrom = new TextBox();
            tbCFrom.Text = "1";
            tbCFrom.Width = 50;
            tbCFrom.Location = new System.Drawing.Point(80, 105);
            OptionForm.Controls.Add(tbCFrom);

            TextBox tbCTo = new TextBox();
            tbCTo.Text = fi.sizeC.ToString();
            tbCTo.Width = 50;
            tbCTo.Location = new System.Drawing.Point(135, 105);
            OptionForm.Controls.Add(tbCTo);

            TextBox tbCStep = new TextBox();
            tbCStep.Text = "1";
            tbCStep.Width = 50;
            tbCStep.Location = new System.Drawing.Point(190, 105);
            OptionForm.Controls.Add(tbCStep);

            #endregion Z

            Panel okBox = new Panel();
            okBox.Height = 40;
            okBox.Dock = DockStyle.Bottom;
            OptionForm.Controls.Add(okBox);

            Button okBtn = new Button();
            okBtn.Text = "Process";
            okBtn.BackColor = System.Drawing.SystemColors.ButtonFace;
            okBtn.ForeColor = System.Drawing.Color.Black;
            okBtn.Location = new System.Drawing.Point(20, 10);
            okBtn.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            okBox.Controls.Add(okBtn);

            Button cancelBtn = new Button();
            cancelBtn.Text = "Cancel";
            cancelBtn.BackColor = System.Drawing.SystemColors.ButtonFace;
            cancelBtn.Location = new System.Drawing.Point(OptionForm.Width - cancelBtn.Width - 40, 10);
            cancelBtn.ForeColor = System.Drawing.Color.Black;
            cancelBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            okBox.Controls.Add(cancelBtn);

            okBtn.Click += new EventHandler(delegate (object sender, EventArgs e)
            {
                int Tfrom, Tto, Tstep, Zfrom, Zto, Zstep, Cfrom, Cto, Cstep;

                if (!int.TryParse(tbTimeFrom.Text, out Tfrom) ||
                !int.TryParse(tbTimeTo.Text, out Tto) ||
                !int.TryParse(tbTimeStep.Text, out Tstep) ||
                !int.TryParse(tbZFrom.Text, out Zfrom) ||
                !int.TryParse(tbZTo.Text, out Zto) ||
                !int.TryParse(tbZStep.Text, out Zstep) ||
                !int.TryParse(tbCFrom.Text, out Cfrom) ||
                !int.TryParse(tbCTo.Text, out Cto) ||
                !int.TryParse(tbCStep.Text, out Cstep))
                {
                    MessageBox.Show("Value must be numeric!");
                    return;
                }

                if (!(
                Tfrom > 0 && Tto <= fi.sizeT && Tfrom <= Tto && Tstep > 0 &&
                Zfrom > 0 && Zto <= fi.sizeZ && Zfrom <= Zto && Zstep > 0 &&
                Cfrom > 0 && Cto <= fi.sizeC && Cfrom <= Cto && Cstep > 0
                ))
                {
                    MessageBox.Show("Incorrect value!");
                    return;
                }

                //event
                res = new int[][] {
                    new int[] { Tfrom, Tto, Tstep },
                    new int[] { Zfrom, Zto, Zstep },
                    new int[] { Cfrom, Cto, Cstep }
                };

                OptionForm.Close();
            });

            cancelBtn.Click += new EventHandler(delegate (object sender, EventArgs e)
            {
                OptionForm.Close();
            });

            OptionForm.KeyPreview = true;
            OptionForm.KeyDown += new KeyEventHandler(delegate (object sender, KeyEventArgs e)
            {
                switch (e.KeyCode)
                {
                    case Keys.Escape:
                        OptionForm.Close();
                        break;
                    case Keys.Enter:
                        okBtn.PerformClick();
                        break;
                    default:
                        break;
                }
            });
            
            StatusLabel.Text = "Dialog open";
            OptionForm.ShowDialog();
            OptionForm.Dispose();
            StatusLabel.Text = "Ready";
            return res;
        }
        private static bool SubstackToolStripMenuItem_click(string path, List<TabPage> Collection, TabPage tp, ImageAnalyser IA, loci.formats.ChannelSeparator reader, int[][] dim)
        {
            TifFileInfo fi = tp.tifFI;

            if (dim == null) return false;
            ////////
            /////background worker
            bool loaded = false;
            //Add handlers to the backgroundworker
            var bgw = new BackgroundWorker();
            bgw.WorkerReportsProgress = true;

            TifFileInfo newFI = null;
            //Add event for projection here
            bgw.DoWork += new DoWorkEventHandler(delegate (Object o, DoWorkEventArgs a)
            {
                //Prepare matrix
                int T, Z, C, f = 0,
                curT, curZ, curC;
                List<int> matrix = new List<int>();
                for (T = 0, curT = dim[0][0] - 1; T < fi.sizeT; T++)
                    if (T == curT && T < dim[0][1])
                    {
                        for (Z = 0, curZ = dim[1][0] - 1; Z < fi.sizeZ; Z++)
                            if (Z == curZ && Z < dim[1][1])
                            {
                                for (C = 0, curC = dim[2][0] - 1; C < fi.sizeC; C++, f++)
                                    if (C == curC && C < dim[2][1])
                                    {
                                        //matrix.Add(f);
                                        matrix.Add(reader.getIndex(Z, C, T));
                                        curC += dim[2][2];
                                    }

                                curZ += dim[1][2];
                            }
                            else f += fi.sizeC;

                        curT += dim[0][2];
                    }
                    else f += fi.sizeC * fi.sizeZ;

                newFI = fi;
                newFI.Dir = newFI.Dir.Substring(0, newFI.Dir.LastIndexOf(".")) +
                "_Substack.tif";

                newFI.sizeT = 0;
                for (int i = dim[0][0] - 1; i < dim[0][1]; i += dim[0][2])
                    newFI.sizeT++;

                newFI.sizeZ = 0;
                for (int i = dim[1][0] - 1; i < dim[1][1]; i += dim[1][2])
                    newFI.sizeZ++;

                newFI.sizeC = 0;
                List<Color> colList = new List<Color>();
                for (int i = dim[2][0] - 1; i < dim[2][1]; i += dim[2][2])
                {
                    newFI.sizeC++;
                    colList.Add(fi.LutList[i]);
                }

                newFI.imageCount = matrix.Count;
                newFI.openedImages = matrix.Count;
                AddEmptyArraysToFI(newFI);
                newFI.LutList = colList;

                //read image

                //Read the first T
                //prepare array and read file
                if (isLibTifCompatible(path) && !reader.isRGB())
                {
                    int[] dimOrder = matrix.ToArray();
                    Tiff image = Tiff.Open(path, "r");
                    //prepare array and read file
                    int midFrame = fi.sizeC * fi.sizeZ;
                    switch (fi.bitsPerPixel)
                    {
                        case 8:
                            fi.image8bit = new byte[fi.imageCount][][];
                            for (int i = 0; i < midFrame; i++)
                            {
                                if (i >= fi.imageCount) { break; }
                                IA.TabPages.myFileDecoder.Image8bit_readFrame(i, image, fi, dimOrder);
                            }
                            break;
                        case 16:
                            fi.image16bit = new ushort[fi.imageCount][][];
                            for (int i = 0; i < midFrame; i++)
                            {
                                if (i >= fi.imageCount) { break; }
                                IA.TabPages.myFileDecoder.Image16bit_readFrame(i, image, fi, dimOrder);
                            }
                            break;
                    }
                    loaded = true;
                    //report progress
                    ((BackgroundWorker)o).ReportProgress(0);
                    //parallel readers
                    IA.TabPages.myFileDecoder.ImageReader_BGW(Collection, image, fi, IA, path, dimOrder);
                    //report progress
                    dimOrder = null;
                    image.Close();

                    ((BackgroundWorker)o).ReportProgress(1);
                }
                else
                {
                    byte[] buf;
                    switch (fi.bitsPerPixel)
                    {
                        case 8:
                            fi.image8bit = new byte[fi.imageCount][][];

                            buf = new byte[fi.sizeX * fi.sizeY];

                            for (int z = 0, i = 0; z < fi.sizeZ; z++)
                                for (int c = 0; c < fi.sizeC; c++, i++)
                                {
                                    fi.image8bit[i] = Image8bit_readFrame(
                                        reader.openBytes(
                                            matrix[i], buf),
                                        fi.sizeX, fi.sizeY);

                                    //fi.LutList[c] = ReadLut(reader);
                                }

                            break;
                        case 16:
                            fi.image16bit = new ushort[fi.imageCount][][];
                            buf = new byte[fi.sizeX * fi.sizeY * 2];

                            for (int z = 0, i = 0; z < fi.sizeZ; z++)
                                for (int c = 0; c < fi.sizeC; c++, i++)
                                {
                                    fi.image16bit[i] = Image16bit_readFrame(
                                        reader.openBytes(
                                            matrix[i], buf),
                                        fi.sizeX, fi.sizeY);

                                    //fi.LutList[c] = ReadLut(reader);
                                }
                            break;
                    }
                    loaded = true;
                    //report progress
                    ((BackgroundWorker)o).ReportProgress(0);
                    //Read the rest T 
                    byte[][] bigBuf = new byte[fi.imageCount][];

                    int ZCcount = fi.sizeC * fi.sizeZ;

                    for (int t = 1, i = ZCcount; t < fi.sizeT; t++)
                        for (int z = 0; z < fi.sizeZ; z++)
                            for (int c = 0; c < fi.sizeC; c++, i++)
                                if (fi.image8bit != null || fi.image16bit != null)
                                {
                                    bigBuf[i] = reader.openBytes(matrix[i]);
                                }
                    //Format the arrays for CellTool
                    Parallel.For(ZCcount, fi.imageCount, (int i) =>
                    {
                        try
                        {
                            switch (fi.bitsPerPixel)
                            {
                                case 8:
                                    if (fi.image8bit != null)
                                    {
                                        fi.image8bit[i] = Image8bit_readFrame(bigBuf[i],
                                        fi.sizeX, fi.sizeY);
                                    }
                                    bigBuf[i] = null;
                                    break;
                                case 16:
                                    if (fi.image16bit != null)
                                    {
                                        fi.image16bit[i] = Image16bit_readFrame(bigBuf[i],
                                                 fi.sizeX, fi.sizeY);
                                    }
                                    bigBuf[i] = null;
                                    break;
                            }
                        }
                        catch { }
                    });
                    bigBuf = null;
                    //report progress
                    ((BackgroundWorker)o).ReportProgress(1);
                }

            });
            bgw.ProgressChanged += new ProgressChangedEventHandler(delegate (Object o, ProgressChangedEventArgs a)
            {
                if (a.ProgressPercentage == 0)
                {

                    fi.openedImages = fi.sizeC * fi.sizeZ;
                    try
                    {
                        IA.ReloadImages();
                    }
                    catch { }
                }
                else if (a.ProgressPercentage == 1)
                {
                    //dispose the reader
                    reader.close();
                    reader = null;
                    //mark as loaded
                    fi.openedImages = fi.imageCount;
                    fi.available = true;
                    fi.loaded = true;
                    newFI.original = false;

                    IA.TabPages.myFileDecoder.CalculateAllRois(fi);

                    IA.ReloadImages();
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
                        IA.TabPages.myFileDecoder.loadingTimer.Stop();
                        IA.FileBrowser.StatusLabel.Text = "Ready";
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
            IA.FileBrowser.StatusLabel.Text = "Reading Image...";
            //Clear OldImage
            IA.IDrawer.ClearImage();

            //start bgw
            bgw.RunWorkerAsync();
            //add taskbar
            tp.tifFI.tpTaskbar.Initialize(IA.TabPages.ImageMainPanel, IA, fi);

            try
            {
                if (loaded == true)
                {
                    IA.ReloadImages();
                }
            }
            catch { }
            //output
            return true;
        }
        private static int[] GetFrameIndexes(loci.formats.ChannelSeparator reader, TifFileInfo fi)
        {
            int[] indexes = new int[fi.imageCount];

            for (int t = 0, i = 0; t < fi.sizeT; t++)
                for (int z = 0; z < fi.sizeZ; z++)
                    for (int c = 0; c < fi.sizeC; c++, i++)
                    {
                        indexes[i] = reader.getIndex(z, c, t);
                    }

            return indexes;
        }
        private static byte[][] Image8bit_readFrame(byte[] buf, int sizeX, int sizeY)
        {
            byte[][] image = new byte[sizeY][];

            for (int y = 0, start = 0; y < sizeY; y++, start += sizeX)
            {
                byte[] row = new byte[sizeX];
                Array.Copy(buf, start, row, 0, sizeX);
                image[y] = row;
            }
            return image;
        }
        private static ushort[][] Image16bit_readFrame(byte[] buf, int sizeX, int sizeY)
        {
            ushort[][] image = new ushort[sizeY][];
            int step = sizeX * 2;

            for (int y = 0, start = 0; y < sizeY; y++, start += step)
            {
                ushort[] row = new ushort[sizeX];
                Buffer.BlockCopy(buf, start, row, 0, step);
                image[y] = row;
            }
            return image;
        }
        private static bool isLibTifCompatible(string dir)
        {
            if (!dir.EndsWith(".tif")) return false;

            using (Tiff tif = Tiff.Open(dir, "r"))
            {
                if (tif == null)
                {
                    tif.Close();
                    return false;
                }

                try
                {
                    tif.SetDirectory((short)(tif.NumberOfDirectories() - 1));
                }
                catch
                {
                    tif.Close();
                    return false;
                }

                int bitsPerPixel = tif.GetField(TiffTag.BITSPERSAMPLE)[0].ToInt();

                tif.Close();
            }

            return true;
        }
        private static void AddEmptyArraysToFI(TifFileInfo fi)
        {
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
    }
}
