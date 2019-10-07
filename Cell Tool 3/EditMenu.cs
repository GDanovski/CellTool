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
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Cell_Tool_3
{
    class EditMenu
    {
        private ImageAnalyser IA = null;
        private Form ProjectionForm = new Form();

        public ToolStripMenuItem Convert16bitToolStripMenuItem = new ToolStripMenuItem();
        public ToolStripMenuItem Convert8bitToolStripMenuItem = new ToolStripMenuItem();
        public ToolStripMenuItem DeleteTToolStripMenuItem = new ToolStripMenuItem();
        public ToolStripMenuItem SplitTToolStripMenuItem = new ToolStripMenuItem();
        public ToolStripMenuItem AddTToolStripMenuItem = new ToolStripMenuItem();
        public ToolStripMenuItem DeleteZToolStripMenuItem = new ToolStripMenuItem();
        public ToolStripMenuItem SplitZToolStripMenuItem = new ToolStripMenuItem();
        public ToolStripMenuItem AddZToolStripMenuItem = new ToolStripMenuItem();
        public ToolStripMenuItem DeleteCToolStripMenuItem = new ToolStripMenuItem();
        public ToolStripMenuItem SplitCToolStripMenuItem = new ToolStripMenuItem();
        public ToolStripMenuItem AddCToolStripMenuItem = new ToolStripMenuItem();
        public ToolStripMenuItem ProcessedAsRawMenuItem = new ToolStripMenuItem();
        public ToolStripMenuItem CropToolStripMenuItem = new ToolStripMenuItem();
        public ToolStripMenuItem ProjectionToolStripMenuItem = new ToolStripMenuItem();
        public ToolStripMenuItem SubstackToolStripMenuItem = new ToolStripMenuItem();
        public EditMenu(ToolStripMenuItem parent)
        {
            parent.DropDownItems.Add(new ToolStripSeparator());

            ToolStripMenuItem ConvertToolStripMenuItem = new ToolStripMenuItem();
            ConvertToolStripMenuItem.Text = "Convert";
            parent.DropDownItems.Add(ConvertToolStripMenuItem);
            {
                Convert8bitToolStripMenuItem.Text = "8 bit";
                Convert8bitToolStripMenuItem.Click += ConvertTo8bit;
                ConvertToolStripMenuItem.DropDownItems.Add(Convert8bitToolStripMenuItem);

                Convert16bitToolStripMenuItem.Text = "16 bit";
                Convert16bitToolStripMenuItem.Click += ConvertTo16bit;
                ConvertToolStripMenuItem.DropDownItems.Add(Convert16bitToolStripMenuItem);

                ConvertToolStripMenuItem.DropDownOpening += new EventHandler(delegate (object sender, EventArgs e)
                {
                    TifFileInfo fi = null;
                    try
                    {
                        fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
                        if (fi != null)
                        {
                            Convert16bitToolStripMenuItem.Enabled = true;
                            Convert8bitToolStripMenuItem.Enabled = true;
                            Convert16bitToolStripMenuItem.Image = null;
                            Convert8bitToolStripMenuItem.Image = null;
                            switch (fi.bitsPerPixel)
                            {
                                case 8:
                                    Convert8bitToolStripMenuItem.Image = Properties.Resources.CheckMark;
                                    break;
                                case 16:
                                    Convert16bitToolStripMenuItem.Image = Properties.Resources.CheckMark;
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                    catch
                    {
                        Convert16bitToolStripMenuItem.Enabled = false;
                        Convert8bitToolStripMenuItem.Enabled = false;
                    }
                });
            }

            parent.DropDownItems.Add(new ToolStripSeparator());
            
            ProjectionToolStripMenuItem.Text = "Projection";
            parent.DropDownItems.Add(ProjectionToolStripMenuItem);
            ProjectionToolStripMenuItem.Click += new EventHandler(delegate (object sender, EventArgs e) 
            {
                TifFileInfo fi = null;
                try
                {
                    fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
                }
                catch { return; }
                //Calculate zoom
                if (fi == null) { return; }
                if(fi.available == false)
                {
                    MessageBox.Show("Image is not avaliable yet!");
                    return;
                }

                // Linux change
                IA.FileBrowser.StatusLabel.Text = "Edit...";
                ProjectionForm.ShowDialog();
                IA.FileBrowser.StatusLabel.Text = "Ready";

            });
           
            CropToolStripMenuItem.Text = "Crop";
            CropToolStripMenuItem.Click += CropToolStripMenuItem_click;
            parent.DropDownItems.Add(CropToolStripMenuItem);

            //SubstackToolStripMenuItem
            SubstackToolStripMenuItem.Text = "Substack";
            SubstackToolStripMenuItem.Click += SubstackToolStripMenuItem_click;
            parent.DropDownItems.Add(SubstackToolStripMenuItem);

            ProcessedAsRawMenuItem.Text = "Export processed image";
            ProcessedAsRawMenuItem.Click += ProcessedAsRawMenuItem_click;
            parent.DropDownItems.Add(ProcessedAsRawMenuItem);

            parent.DropDownItems.Add(new ToolStripSeparator());
            
            AddCToolStripMenuItem.Text = "Merge channels";
            AddCToolStripMenuItem.Click += AddCToolStripMenuItem_click;
            parent.DropDownItems.Add(AddCToolStripMenuItem);
           
            SplitCToolStripMenuItem.Text = "Extract channel";
            SplitCToolStripMenuItem.Click += SplitCToolStripMenuItem_Click;
            parent.DropDownItems.Add(SplitCToolStripMenuItem);
            
            DeleteCToolStripMenuItem.Text = "Delete channel";
            DeleteCToolStripMenuItem.Click += DeleteCToolStripMenuItem_Click;
            parent.DropDownItems.Add(DeleteCToolStripMenuItem);

            parent.DropDownItems.Add(new ToolStripSeparator());
            
            AddZToolStripMenuItem.Text = "Merge Z planes";
            AddZToolStripMenuItem.Click += AddZToolStripMenuItem_click;
            parent.DropDownItems.Add(AddZToolStripMenuItem);
            
            SplitZToolStripMenuItem.Text = "Extract Z plane";
            SplitZToolStripMenuItem.Click += SplitZToolStripMenuItem_Click;
            parent.DropDownItems.Add(SplitZToolStripMenuItem);
            
            DeleteZToolStripMenuItem.Text = "Delete Z plane";
            DeleteZToolStripMenuItem.Click += DeleteZToolStripMenuItem_Click;
            parent.DropDownItems.Add(DeleteZToolStripMenuItem);

            parent.DropDownItems.Add(new ToolStripSeparator());
            
            AddTToolStripMenuItem.Text = "Merge T slices";
            AddTToolStripMenuItem.Click += AddTToolStripMenuItem_click;
            parent.DropDownItems.Add(AddTToolStripMenuItem);

            
            SplitTToolStripMenuItem.Text = "Extract T slice";
            SplitTToolStripMenuItem.Click += SplitTToolStripMenuItem_Click;
            parent.DropDownItems.Add(SplitTToolStripMenuItem);

            DeleteTToolStripMenuItem.Text = "Delete T slice";
            DeleteTToolStripMenuItem.Click += DeleteTToolStripMenuItem_Click;
            parent.DropDownItems.Add(DeleteTToolStripMenuItem);
            /*
            ToolStripMenuItem TestToolStripMenuItem = new ToolStripMenuItem();
            TestToolStripMenuItem.Text = "Test";
            TestToolStripMenuItem.Tag =
                    "-1,-2,-1," +
                    "0,0,0," +
                    "1,2,1zz" +
                    "-1,0,1," +
                    "-2,0,2," +
                    "-1,0,1";
                    
            parent.DropDownItems.Add(TestToolStripMenuItem);
            TestToolStripMenuItem.Click += new EventHandler(delegate (object o, EventArgs a)
            {
                Filters filt = new Filters(IA);
                filt.ConvolutionBtn_Click(o, a);
            });*/
            
            parent.DropDownOpening += new EventHandler(delegate (object sender, EventArgs e) 
            {
                TifFileInfo fi = null;
                try
                {
                    fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
                    if (fi != null)
                    {
                        ProcessedAsRawMenuItem.Enabled = true;
                        SubstackToolStripMenuItem.Enabled = true;

                        if (IA.RoiMan.roiTV.Nodes.Count > 0)
                            CropToolStripMenuItem.Enabled = true;
                        else
                            CropToolStripMenuItem.Enabled = false;

                        ConvertToolStripMenuItem.Enabled = true;
                        ProjectionToolStripMenuItem.Enabled = true;
                        AddCToolStripMenuItem.Enabled = true;
                        if (fi.sizeC > 1)
                        {
                            SplitCToolStripMenuItem.Enabled = true;
                            DeleteCToolStripMenuItem.Enabled = true;
                        }
                        else
                        {
                            SplitCToolStripMenuItem.Enabled = false;
                            DeleteCToolStripMenuItem.Enabled = false;
                        }

                        AddZToolStripMenuItem.Enabled = true;
                        if (fi.sizeZ > 1)
                        {
                            SplitZToolStripMenuItem.Enabled = true;
                            DeleteZToolStripMenuItem.Enabled = true;
                        }
                        else
                        {
                            SplitZToolStripMenuItem.Enabled = false;
                            DeleteZToolStripMenuItem.Enabled = false;
                        }
                        
                        AddTToolStripMenuItem.Enabled = true;
                        if (fi.sizeT > 1)
                        {
                            SplitTToolStripMenuItem.Enabled = true;
                            DeleteTToolStripMenuItem.Enabled = true;
                        }
                        else
                        {
                            SplitTToolStripMenuItem.Enabled = false;
                            DeleteTToolStripMenuItem.Enabled = false;
                        }

                        if(fi.sizeC > 1 || fi.sizeZ > 1 || fi.sizeT > 1)
                        {
                            ProjectionToolStripMenuItem.Enabled = true;
                        }
                        else
                        {
                            ProjectionToolStripMenuItem.Enabled = false;
                        }
                    }
                    else
                    {
                        CropToolStripMenuItem.Enabled = false;
                        ConvertToolStripMenuItem.Enabled = false;
                        ProjectionToolStripMenuItem.Enabled = false;
                        AddCToolStripMenuItem.Enabled = false;
                        SplitCToolStripMenuItem.Enabled = false;
                        DeleteCToolStripMenuItem.Enabled = false;
                        AddZToolStripMenuItem.Enabled = false;
                        SplitZToolStripMenuItem.Enabled = false;
                        DeleteZToolStripMenuItem.Enabled = false;
                        AddTToolStripMenuItem.Enabled = false;
                        SplitTToolStripMenuItem.Enabled = false;
                        DeleteTToolStripMenuItem.Enabled = false;
                        ProcessedAsRawMenuItem.Enabled = false;
                        SubstackToolStripMenuItem.Enabled = false;
                    }
                }
                catch
                {
                    CropToolStripMenuItem.Enabled = false;
                    ConvertToolStripMenuItem.Enabled = false; 
                    ProjectionToolStripMenuItem.Enabled = false;
                    AddCToolStripMenuItem.Enabled = false;
                    SplitCToolStripMenuItem.Enabled = false;
                    DeleteCToolStripMenuItem.Enabled = false;
                    AddZToolStripMenuItem.Enabled = false;
                    SplitZToolStripMenuItem.Enabled = false;
                    DeleteZToolStripMenuItem.Enabled = false;
                    AddTToolStripMenuItem.Enabled = false;
                    SplitTToolStripMenuItem.Enabled = false;
                    DeleteTToolStripMenuItem.Enabled = false;
                    ProcessedAsRawMenuItem.Enabled = false;
                    SubstackToolStripMenuItem.Enabled = false;
                }
            });
            
            ProjectionForm_Initialize();
        }
        public ImageAnalyser setIA
        {
            get
            {
                return this.IA;
            }
            set
            {
                this.IA = value;

                ProjectionForm.BackColor = IA.FileBrowser.BackGround2Color1;
                ProjectionForm.ForeColor = IA.FileBrowser.ShriftColor1;
            }
        }
        #region Substack
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
            tbCTo.Location = new System.Drawing.Point(135,105);
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
                Tfrom >0 && Tto <= fi.sizeT && Tfrom <= Tto && Tstep>0 &&
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
        private void SubstackToolStripMenuItem_click(object sender, EventArgs e)
        {
            TifFileInfo fi = null;
            try
            {
                fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
            }
            catch { return; }
            if (fi == null) { return; }
            if (fi.available == false) return;

            int[][] dim = SubstackDialog(fi,this.IA.FileBrowser.StatusLabel);

            if (dim == null) return;

            ////////
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
                                for (C = 0, curC = dim[2][0] - 1; C < fi.sizeC; C++,f++)
                                    if (C == curC && C < dim[2][1])
                                    {
                                        matrix.Add(f);
                                        curC += dim[2][2];
                                    }

                                curZ += dim[1][2];
                            }
                            else f += fi.sizeC;

                        curT += dim[0][2];
                    }
                    else f += fi.sizeC * fi.sizeZ;
                
                newFI = DuplicateFI(fi);
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

                switch (fi.bitsPerPixel)
                {
                    case 8:
                        byte[][][] image8bit = new byte[newFI.imageCount][][];
                        Parallel.For(0,matrix.Count, frame => {
                            image8bit[frame] = new byte[fi.sizeY][];
                            for (int y = 0; y < fi.sizeY; y++)
                            {
                                image8bit[frame][y] = new byte[fi.sizeX];
                                Array.Copy(fi.image8bit[matrix[frame]][y], image8bit[frame][y], fi.sizeX);
                            }
                        });
                        newFI.image8bit = image8bit;
                        newFI.image8bitFilter = newFI.image8bit;
                        break;
                    case 16:
                        ushort[][][] image16bit = new ushort[newFI.imageCount][][];
                        Parallel.For(0, matrix.Count, frame => {
                            image16bit[frame] = new ushort[fi.sizeY][];
                            for (int y = 0; y < fi.sizeY; y++)
                            {
                                image16bit[frame][y] = new ushort[fi.sizeX];
                                Array.Copy(fi.image16bit[matrix[frame]][y], image16bit[frame][y], fi.sizeX);
                            }
                        });
                        newFI.image16bit = image16bit;
                        newFI.image16bitFilter = newFI.image16bit;
                        break;
                }
                ((BackgroundWorker)o).ReportProgress(0);
            });

            bgw.ProgressChanged += new ProgressChangedEventHandler(delegate (Object o, ProgressChangedEventArgs a)
            {
                if (a.ProgressPercentage == 0)
                {
                    //add to tabpage
                    addTabPage(newFI);
                    //show on screen
                    IA.MarkAsNotSaved();
                    newFI.loaded = true;
                    newFI.original = false;
                    newFI.available = true;
                    IA.ReloadImages();
                }
                fi.available = true;
                IA.FileBrowser.StatusLabel.Text = "Ready";
            });

            //Start background worker
            IA.FileBrowser.StatusLabel.Text = "Substack...";
            //start bgw
            bgw.RunWorkerAsync();

        }
        #endregion Substack
        #region Crop
        private void CropToolStripMenuItem_click1(object sender, EventArgs e)
        {
            TifFileInfo fi = null;
            try
            {
                fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
            }
            catch { return; }
            if (fi == null) { return; }
            if (fi.available == false) return;

            if (IA.RoiMan.SelectedROIsList == null || IA.RoiMan.SelectedROIsList.Count == 0)
            {
                MessageBox.Show("There is no selected ROI!");
                return;
            }

            List<BackgroundWorker> bgwList = new List<BackgroundWorker>();
                        
            foreach (ROI roi in IA.RoiMan.SelectedROIsList)
            {
                //background worker
                var bgw = new BackgroundWorker();
                
                bgw.WorkerReportsProgress = true;
                fi.available = false;
                TifFileInfo newFI = null;
                //Add event for projection here
                bgw.DoWork += new DoWorkEventHandler(delegate (Object o, DoWorkEventArgs a)
                {
                Rectangle rect = Rectangle.Empty;
                //find rectangles

                switch (roi.Type)
                {
                    case 0:
                        if (roi.Shape == 1 | roi.Shape == 0)
                        {
                            Point p = roi.GetLocation(fi.cValue)[0];
                            Size size = new Size(roi.Width, roi.Height);

                            rect = new Rectangle(p, size);

                        }
                        else if (roi.Shape == 2 || roi.Shape == 3 || roi.Shape == 4 || roi.Shape == 5)
                        {
                            Point[] pList = roi.GetLocation(fi.cValue);

                            int X = int.MaxValue;
                            int Y = int.MaxValue;
                            int W = int.MinValue;
                            int H = int.MinValue;

                            foreach (Point p1 in pList)
                            {
                                if (p1.X < X) X = p1.X;
                                if (p1.Y < Y) Y = p1.Y;
                                if (p1.X > W) W = p1.X;
                                if (p1.Y > H) H = p1.Y;
                            }

                            Point p = new Point(X, Y);
                            Size size = new Size(W - X, H - Y);

                            rect = new Rectangle(p, size);

                        }
                        break;
                    case 1:
                        if (roi.Shape == 1 | roi.Shape == 0)
                        {
                            Point[] pList = roi.GetLocationAll()[0];

                            int X = int.MaxValue;
                            int Y = int.MaxValue;
                            int W = int.MinValue;
                            int H = int.MinValue;

                            for (int i = fi.cValue; i < fi.imageCount; i += fi.sizeC)
                            {
                                Point p1 = pList[i];
                                if (p1 != null)
                                {
                                    if (p1.X < X) X = p1.X;
                                    if (p1.Y < Y) Y = p1.Y;
                                    if (p1.X > W) W = p1.X;
                                    if (p1.Y > H) H = p1.Y;
                                }
                            }

                            Point p = new Point(X, Y);
                            Size size = new Size(W - X + roi.Width, H - Y + roi.Height);

                            rect = new Rectangle(p, size);

                        }
                        else if (roi.Shape == 2 || roi.Shape == 3 || roi.Shape == 4 || roi.Shape == 5)
                        {
                            int X = int.MaxValue;
                            int Y = int.MaxValue;
                            int W = int.MinValue;
                            int H = int.MinValue;
                            Point[] pList;

                            for (int i = fi.cValue; i < fi.imageCount; i += fi.sizeC)
                            {
                                pList = roi.GetLocation(i);

                                foreach (Point p1 in pList)
                                {
                                    if (p1.X < X) X = p1.X;
                                    if (p1.Y < Y) Y = p1.Y;
                                    if (p1.X > W) W = p1.X;
                                    if (p1.Y > H) H = p1.Y;
                                }
                            }

                            Point p = new Point(X, Y);
                            Size size = new Size(W - X, H - Y);

                            rect = new Rectangle(p, size);
                        }
                        break;
                }

                //crop the rectangle
                newFI = DuplicateFI(fi);
                newFI.Dir = newFI.Dir.Substring(0, newFI.Dir.LastIndexOf(".")) + "_ROI"
                    + (fi.roiList[fi.cValue].IndexOf(roi) + 1).ToString() + ".tif";

                newFI.sizeX = rect.Width;
                newFI.sizeY = rect.Height;
                newFI.xCompensation = rect.X;
                newFI.yCompensation = rect.Y;

                newFI.imageCount = fi.imageCount;
                newFI.openedImages = newFI.imageCount;
                AddEmptyArraysToFI(newFI);


                switch (fi.bitsPerPixel)
                {
                    case 8:
                        byte[][][] image8bit = new byte[fi.imageCount][][];
                        Parallel.For(0, fi.imageCount, frame=>{
                                image8bit[frame] = new byte[rect.Height][];
                                for (int y = rect.Y, yNew = 0; y < rect.Y + rect.Height; y++, yNew++)
                                {
                                    image8bit[frame][yNew] = new byte[rect.Width];

                                    for (int x = rect.X, xNew = 0; x < rect.X + rect.Width; x++, xNew++)
                                        if (x >= 0 && y >= 0 && x < fi.sizeX && y < fi.sizeY )
                                            image8bit[frame][yNew][xNew] = fi.image8bit[frame][y][x];
                                }
                        });
                            newFI.image8bit = image8bit;
                            newFI.image8bitFilter = newFI.image8bit;
                            break;
                        case 16:
                            ushort[][][] image16bit = new ushort[fi.imageCount][][];
                            Parallel.For(0, fi.imageCount, frame =>
                            {
                                image16bit[frame] = new ushort[rect.Height][];
                                for (int y = rect.Y, yNew = 0; y < rect.Y + rect.Height; y++, yNew++)
                                {
                                    image16bit[frame][yNew] = new ushort[rect.Width];

                                    for (int x = rect.X, xNew = 0; x < rect.X + rect.Width; x++, xNew++)
                                        if (x >= 0 && y >= 0 && x < fi.sizeX && y < fi.sizeY)
                                            image16bit[frame][yNew][xNew] = fi.image16bit[frame][y][x];
                                }
                            });
                            newFI.image16bit = image16bit;
                            newFI.image16bitFilter = newFI.image16bit;
                            break;
                    }
                    ((BackgroundWorker)o).ReportProgress(0);
                });

                bgw.ProgressChanged += new ProgressChangedEventHandler(delegate (Object o, ProgressChangedEventArgs a)
                {
                    if (a.ProgressPercentage == 0)
                    {
                        //add to tabpage
                        addTabPage(newFI);
                        //show on screen
                        IA.MarkAsNotSaved();
                        newFI.loaded = true;
                        newFI.original = false;
                        IA.ReloadImages();
                    }
                    fi.available = true;

                    bool finishWork = true;

                    bgwList.Remove(bgw);

                    foreach (BackgroundWorker bgw1 in bgwList)
                        if (bgw1.IsBusy == true) finishWork = false;

                    if(finishWork == true)
                        IA.FileBrowser.StatusLabel.Text = "Ready";
                });
                //Start background worker
                IA.FileBrowser.StatusLabel.Text = "Cropping...";
                //start bgw
                bgwList.Add(bgw);
                bgw.RunWorkerAsync();
            }
        }
        private void CropToolStripMenuItem_click(object sender, EventArgs e)
        {
            TifFileInfo fi = null;
            try
            {
                fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
            }
            catch { return; }
            if (fi == null) { return; }
            if (fi.available == false) return;

            if (IA.RoiMan.SelectedROIsList == null || IA.RoiMan.SelectedROIsList.Count == 0)
            {
                MessageBox.Show("There is no selected ROI!");
                return;
            }

            List<BackgroundWorker> bgwList = new List<BackgroundWorker>();

            foreach (ROI roi in IA.RoiMan.SelectedROIsList)
            {
                int fromT = roi.FromT-1;
                int toT = roi.ToT-1;
                int fromZ = roi.FromZ-1;
                int toZ = roi.ToZ-1;

                //MessageBox.Show(fromT.ToString() + "\n" + toT.ToString() + "\n" + fromZ.ToString() + "\n" + toZ.ToString() + "\n");

                int curZ = 0;
                int curT = 0;
                int curC = 0;
                //background worker
                var bgw = new BackgroundWorker();

                bgw.WorkerReportsProgress = true;
                fi.available = false;
                TifFileInfo newFI = null;
                //Add event for projection here
                bgw.DoWork += new DoWorkEventHandler(delegate (Object o, DoWorkEventArgs a)
                {
                    Rectangle rect = Rectangle.Empty;
                    //find rectangles

                    switch (roi.Type)
                    {
                        case 0:
                            if (roi.Shape == 1 | roi.Shape == 0)
                            {
                                Point p = roi.GetLocation(fi.cValue)[0];
                                Size size = new Size(roi.Width, roi.Height);

                                rect = new Rectangle(p, size);

                            }
                            else if (roi.Shape == 2 || roi.Shape == 3 || roi.Shape == 4 || roi.Shape == 5)
                            {
                                Point[] pList = roi.GetLocation(fi.cValue);

                                int X = int.MaxValue;
                                int Y = int.MaxValue;
                                int W = int.MinValue;
                                int H = int.MinValue;

                                foreach (Point p1 in pList)
                                {
                                    if (p1.X < X) X = p1.X;
                                    if (p1.Y < Y) Y = p1.Y;
                                    if (p1.X > W) W = p1.X;
                                    if (p1.Y > H) H = p1.Y;
                                }

                                Point p = new Point(X, Y);
                                Size size = new Size(W - X, H - Y);

                                rect = new Rectangle(p, size);

                            }
                            break;
                        case 1:
                            if (roi.Shape == 1 | roi.Shape == 0)
                            {
                                Point[] pList = roi.GetLocationAll()[0];

                                int X = int.MaxValue;
                                int Y = int.MaxValue;
                                int W = int.MinValue;
                                int H = int.MinValue;

                                for (int i = fi.cValue; i < fi.imageCount; i += fi.sizeC)
                                {
                                    if (curZ >= fromZ && curT >= fromT && 
                                    curZ<=toZ && curT <= toT)
                                    {
                                        Point p1 = pList[i];
                                        if (p1 != null)
                                        {
                                            if (p1.X < X) X = p1.X;
                                            if (p1.Y < Y) Y = p1.Y;
                                            if (p1.X > W) W = p1.X;
                                            if (p1.Y > H) H = p1.Y;
                                        }
                                    }

                                    curZ++;
                                    if (curZ >= fi.sizeZ)
                                    {
                                        curZ = 0;
                                        curT++;
                                    }
                                }

                                Point p = new Point(X, Y);
                                Size size = new Size(W - X + roi.Width, H - Y + roi.Height);

                                rect = new Rectangle(p, size);

                            }
                            else if (roi.Shape == 2 || roi.Shape == 3 || roi.Shape == 4 || roi.Shape == 5)
                            {
                                int X = int.MaxValue;
                                int Y = int.MaxValue;
                                int W = int.MinValue;
                                int H = int.MinValue;
                                Point[] pList;

                                for (int i = fi.cValue; i < fi.imageCount; i += fi.sizeC)
                                {
                                    if (curZ >= fromZ && curT >= fromT &&
                                   curZ <= toZ && curT <= toT)
                                    {
                                        pList = roi.GetLocation(i);

                                        foreach (Point p1 in pList)
                                        {
                                            if (p1.X < X) X = p1.X;
                                            if (p1.Y < Y) Y = p1.Y;
                                            if (p1.X > W) W = p1.X;
                                            if (p1.Y > H) H = p1.Y;
                                        }
                                    }

                                    curZ++;
                                    if (curZ >= fi.sizeZ)
                                    {
                                        curZ = 0;
                                        curT++;
                                    }
                                }

                                Point p = new Point(X, Y);
                                Size size = new Size(W - X, H - Y);

                                rect = new Rectangle(p, size);
                            }
                            break;
                    }

                    //crop the rectangle
                    newFI = DuplicateFI(fi);
                    newFI.Dir = newFI.Dir.Substring(0, newFI.Dir.LastIndexOf(".")) + "_ROI"
                        + (fi.roiList[fi.cValue].IndexOf(roi) + 1).ToString() + ".tif";

                    newFI.sizeX = rect.Width;
                    newFI.sizeY = rect.Height;
                    newFI.xCompensation = rect.X;
                    newFI.yCompensation = rect.Y;

                    newFI.sizeT = toT - fromT + 1;
                    newFI.sizeZ = toZ - fromZ + 1;
                    newFI.imageCount = newFI.sizeC* newFI.sizeZ* newFI.sizeT;
                    newFI.openedImages = newFI.imageCount;
                    AddEmptyArraysToFI(newFI);
                    
                    curZ = 0;
                    curT = 0;

                    int[] matr = new int[newFI.imageCount];
                    for (int frame = 0, position = 0; frame < fi.imageCount; frame++)
                    {
                        if (curZ >= fromZ && curT >= fromT &&
                           curZ <= toZ && curT <= toT)
                        {
                            matr[position] = frame;
                            position++;
                        }

                        curC++;
                        if (curC >= fi.sizeC)
                        {
                            curC = 0;
                            curZ++;
                            if (curZ >= fi.sizeZ)
                            {
                                curZ = 0;
                                curT++;
                            }
                        }
                    }

                    switch (fi.bitsPerPixel)
                    {
                        case 8:
                            byte[][][] image8bit = new byte[newFI.imageCount][][];
                            Parallel.For(0, matr.Length, frame => {
                                image8bit[frame] = new byte[rect.Height][];
                                for (int y = rect.Y, yNew = 0; y < rect.Y + rect.Height; y++, yNew++)
                                {
                                    image8bit[frame][yNew] = new byte[rect.Width];

                                    for (int x = rect.X, xNew = 0; x < rect.X + rect.Width; x++, xNew++)
                                        if (x >= 0 && y >= 0 && x < fi.sizeX && y < fi.sizeY)
                                            image8bit[frame][yNew][xNew] = fi.image8bit[matr[frame]][y][x];
                                }
                            });
                            newFI.image8bit = image8bit;
                            newFI.image8bitFilter = newFI.image8bit;
                            break;
                        case 16:
                            ushort[][][] image16bit = new ushort[newFI.imageCount][][];
                            Parallel.For(0, matr.Length, frame =>
                            {
                                image16bit[frame] = new ushort[rect.Height][];
                                for (int y = rect.Y, yNew = 0; y < rect.Y + rect.Height; y++, yNew++)
                                {
                                    image16bit[frame][yNew] = new ushort[rect.Width];

                                    for (int x = rect.X, xNew = 0; x < rect.X + rect.Width; x++, xNew++)
                                        if (x >= 0 && y >= 0 && x < fi.sizeX && y < fi.sizeY)
                                            image16bit[frame][yNew][xNew] = fi.image16bit[matr[frame]][y][x];
                                }
                            });
                            newFI.image16bit = image16bit;
                            newFI.image16bitFilter = newFI.image16bit;
                            break;
                    }
                    
                    ((BackgroundWorker)o).ReportProgress(0);
                });

                bgw.ProgressChanged += new ProgressChangedEventHandler(delegate (Object o, ProgressChangedEventArgs a)
                {
                    if (a.ProgressPercentage == 0)
                    {
                        //add to tabpage
                        addTabPage(newFI);
                        //show on screen
                        IA.MarkAsNotSaved();
                        newFI.loaded = true;
                        newFI.original = false;
                        IA.ReloadImages();
                    }
                    fi.available = true;

                    bool finishWork = true;

                    bgwList.Remove(bgw);

                    foreach (BackgroundWorker bgw1 in bgwList)
                        if (bgw1.IsBusy == true) finishWork = false;

                    if (finishWork == true)
                        IA.FileBrowser.StatusLabel.Text = "Ready";
                });
                //Start background worker
                IA.FileBrowser.StatusLabel.Text = "Cropping...";
                //start bgw
                bgwList.Add(bgw);
                bgw.RunWorkerAsync();
            }
        }
        #endregion Crop

        #region Projection
        private void ProjectionForm_Initialize()
        {
            Form OptionForm = ProjectionForm;
            OptionForm.FormBorderStyle = FormBorderStyle.FixedDialog;
            OptionForm.Text = "Projection";
            OptionForm.StartPosition = FormStartPosition.CenterScreen;
            OptionForm.WindowState = FormWindowState.Normal;
            OptionForm.MinimizeBox = false;
            OptionForm.MaximizeBox = false;
            
            OptionForm.Width = 220;
            OptionForm.Height = 240;

            Panel stackBox = new Panel();
            stackBox.Height = 80;
            stackBox.Dock = DockStyle.Bottom;
            OptionForm.Controls.Add(stackBox);

            Label stackL = new Label();
            stackL.Location = new System.Drawing.Point(10, 15);
            stackL.Text = "Select data set:";
            stackBox.Controls.Add(stackL);
            
            RadioButton CRadiobtn = new RadioButton();
            CRadiobtn.Text = "C stack";
            CRadiobtn.Location = new System.Drawing.Point(110, 10);
            stackBox.Controls.Add(CRadiobtn);

            RadioButton ZRadiobtn = new RadioButton();
            ZRadiobtn.Text = "Z stack";
            ZRadiobtn.Checked = true;
            ZRadiobtn.Location = new System.Drawing.Point(110, 30);
            stackBox.Controls.Add(ZRadiobtn);

            RadioButton TRadiobtn = new RadioButton();
            TRadiobtn.Text = "T stack";
            TRadiobtn.Location = new System.Drawing.Point(110, 50);
            stackBox.Controls.Add(TRadiobtn);
            
            Panel typeBox = new Panel();
            typeBox.Height = 80;
            typeBox.Dock = DockStyle.Bottom;
            OptionForm.Controls.Add(typeBox);

            Label typeL = new Label();
            typeL.Location = new System.Drawing.Point(10, 15);
            typeL.Text = "Type projection:";
            typeBox.Controls.Add(typeL);
            
            RadioButton MaxRadiobtn = new RadioButton();
            MaxRadiobtn.Text = "Maximum";
            MaxRadiobtn.Checked = true;
            MaxRadiobtn.Location = new System.Drawing.Point(110, 10);
            typeBox.Controls.Add(MaxRadiobtn);

            RadioButton MinRadiobtn = new RadioButton();
            MinRadiobtn.Text = "Minimum";
            MinRadiobtn.Location = new System.Drawing.Point(110, 30);
            typeBox.Controls.Add(MinRadiobtn);

            RadioButton AvgRadiobtn = new RadioButton();
            AvgRadiobtn.Text = "Average";
            AvgRadiobtn.Location = new System.Drawing.Point(110, 50);
            typeBox.Controls.Add(AvgRadiobtn);

            Panel okBox = new Panel();
            okBox.Height = 40;
            okBox.Dock = DockStyle.Bottom;
            OptionForm.Controls.Add(okBox);

            Button okBtn = new Button();
            okBtn.Text = "Process";
            okBtn.BackColor = System.Drawing.SystemColors.ButtonFace;
            okBtn.ForeColor = System.Drawing.Color.Black;
            okBtn.Location = new System.Drawing.Point(20, 10);
            okBox.Controls.Add(okBtn);

            Button cancelBtn = new Button();
            cancelBtn.Text = "Cancel";
            cancelBtn.BackColor = System.Drawing.SystemColors.ButtonFace;
            cancelBtn.Location = new System.Drawing.Point(OptionForm.Width - cancelBtn.Width - 40, 10);
            cancelBtn.ForeColor = System.Drawing.Color.Black;
            okBox.Controls.Add(cancelBtn);

            okBtn.Click += new EventHandler(delegate (object sender, EventArgs e)
            {
                ProjectionEvent(CRadiobtn.Checked, ZRadiobtn.Checked, TRadiobtn.Checked,
                    MaxRadiobtn.Checked, MinRadiobtn.Checked, AvgRadiobtn.Checked);
                OptionForm.Hide();
            });

            cancelBtn.Click += new EventHandler(delegate (object sender, EventArgs e)
            {
                OptionForm.Hide();
            });

            OptionForm.KeyPreview = true;
            OptionForm.KeyDown += new KeyEventHandler(delegate (object sender, KeyEventArgs e)
            {
                switch (e.KeyCode)
                {
                    case Keys.Escape:
                        OptionForm.Hide();
                        break;
                    case Keys.Enter:
                        okBtn.PerformClick();
                        break;
                    default:
                        break;
                }
            });

            OptionForm.VisibleChanged += new EventHandler(delegate (object sender, EventArgs e)
            {
                if (OptionForm.Visible == false) return;
                TifFileInfo fi = null;
                try
                {
                    fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
                }
                catch { return; }

                //Calculate zoom
                if (fi == null) { return; }

                if (fi.sizeZ > 1)
                    ZRadiobtn.Enabled = true;
                else
                    ZRadiobtn.Enabled = false;

                if (fi.sizeC > 1)
                    CRadiobtn.Enabled = true;
                else
                    CRadiobtn.Enabled = false;

                if (fi.sizeT > 1)
                    TRadiobtn.Enabled = true;
                else
                    TRadiobtn.Enabled = false;

                bool findNew = true;

                if (ZRadiobtn.Enabled == false
                & ZRadiobtn.Checked == true)
                    ZRadiobtn.Checked = false;
                else if (CRadiobtn.Enabled == false
                & CRadiobtn.Checked == true)
                    CRadiobtn.Checked = false;
                else if (TRadiobtn.Enabled == false
                & TRadiobtn.Checked == true)
                    TRadiobtn.Checked = false;
                else
                    findNew = false;
                
                if (findNew == true)
                    if (ZRadiobtn.Enabled == true)
                        ZRadiobtn.Checked = true;
                    else if (TRadiobtn.Enabled == true)
                        TRadiobtn.Checked = true;
                    else if (CRadiobtn.Enabled == true)
                        CRadiobtn.Checked = true;

            });
        }
        private void ProjectionEvent(bool C,bool Z, bool T, bool max, bool min, bool avg)
        {
            // calculate fileinfo
            TifFileInfo fi = null;
            try
            {
                fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
            }
            catch { return; }

            if (fi == null) { return; }

            if (fi.available == false) return;

            bool zTrackChange = false;
            bool tTrackChange = false;
            bool LutNChange = false;

            var bgw = new BackgroundWorker();
            bgw.WorkerReportsProgress = true;

            //Add event for projection here
            bgw.DoWork += new DoWorkEventHandler(delegate (Object o, DoWorkEventArgs a)
            {
                if (C)
                {
                    int final_ImageCount = fi.imageCount / fi.sizeC;
                    switch (fi.bitsPerPixel)
                    {
                        case 8:
                            byte[][][][] ResImage8 = new byte[final_ImageCount][][][];
                            //order the frames for processing
                            for (int i = 0, frame = 0; i < final_ImageCount; i++)
                            {
                                ResImage8[i] = new byte[fi.sizeC][][];
                                for (int cVal = 0; cVal < fi.sizeC; cVal++, frame++)
                                    ResImage8[i][cVal] = fi.image8bit[frame];
                            }
                            fi.image8bit = ProjectionEvent(ResImage8, fi, max, min, avg);
                            fi.image8bitFilter = fi.image8bit;
                            break;
                        case 16:
                            ushort[][][][] ResImage16 = new ushort[final_ImageCount][][][];
                            //order the frames for processing
                            for (int i = 0, frame = 0; i < final_ImageCount; i++)
                            {
                                ResImage16[i] = new ushort[fi.sizeC][][];
                                for (int cVal = 0; cVal < fi.sizeC; cVal++, frame++)
                                    ResImage16[i][cVal] = fi.image16bit[frame];
                            }
                            fi.image16bit = ProjectionEvent(ResImage16, fi, max, min, avg);
                            fi.image16bitFilter = fi.image16bit;
                            break;
                    }

                    fi.imageCount = final_ImageCount;
                    fi.cValue = 0;
                    fi.sizeC = 1;
                    FI_reduseC(fi);
                    fi.LutList.Clear();
                    fi.LutList.Add(System.Drawing.Color.White);
                    
                    LutNChange = true;
                }
                else if (Z)
                {
                    int final_ImageCount = fi.imageCount / fi.sizeZ;
                    int ZC = (fi.sizeC * fi.sizeZ);
                    switch (fi.bitsPerPixel)
                    {
                        case 8:
                            byte[][][][] ResImage8 = new byte[final_ImageCount][][][];

                            for (int i = 0; i < final_ImageCount; i++)
                                ResImage8[i] = new byte[fi.sizeZ][][];
                            //order the frames for processing

                            for (int i = 0, frame = 0; i < fi.imageCount; i += ZC, frame += fi.sizeC)
                                for (int c = 0; c < fi.sizeC; c++)
                                    for (int imageN = i + c, z = 0; imageN < i + ZC; imageN += fi.sizeC, z++)
                                        ResImage8[frame + c][z] = fi.image8bit[imageN];

                            fi.image8bit = ProjectionEvent(ResImage8, fi, max, min, avg);
                            fi.image8bitFilter = fi.image8bit;
                            //
                            break;
                        case 16:
                            ushort[][][][] ResImage16 = new ushort[final_ImageCount][][][];

                            for (int i = 0; i < final_ImageCount; i++)
                                ResImage16[i] = new ushort[fi.sizeZ][][];
                            //order the frames for processing

                            for (int i = 0, frame = 0; i < fi.imageCount; i += ZC, frame += fi.sizeC)
                                for (int c = 0; c < fi.sizeC; c++)
                                    for (int imageN = i + c, z = 0; imageN < i + ZC; imageN += fi.sizeC, z++)
                                        ResImage16[frame + c][z] = fi.image16bit[imageN];

                            fi.image16bit = ProjectionEvent(ResImage16, fi, max, min, avg);
                            fi.image16bitFilter = fi.image16bit;
                            break;
                    }
                    fi.FilterHistory.Clear();

                    if (fi.newFilterHistory != null)
                        for (int i = 0; i < fi.newFilterHistory.Length; i++)
                        {
                            fi.newFilterHistory[i].Clear();
                            fi.isBinary[i] = false;
                        }

                    fi.imageCount = final_ImageCount;
                    fi.sizeZ = 1;
                    fi.zValue = 0;
                    
                    zTrackChange = true;
                }
                else if (T)
                {
                    int final_ImageCount = fi.imageCount / (fi.sizeT);
                    int ZC = (fi.sizeC * fi.sizeZ);
                    switch (fi.bitsPerPixel)
                    {
                        case 8:
                            byte[][][][] ResImage8 = new byte[final_ImageCount][][][];

                            for (int i = 0; i < final_ImageCount; i++)
                                ResImage8[i] = new byte[fi.sizeT + 1][][];
                            //order the frames for processing
                            for (int indZC = 0; indZC < ZC; indZC++)
                                for (int i = indZC, frame = 0; i < fi.imageCount; i += ZC, frame++)
                                    ResImage8[indZC][frame] = fi.image8bit[i];

                            fi.image8bit = ProjectionEvent(ResImage8, fi, max, min, avg);
                            fi.image8bitFilter = fi.image8bit;
                            //
                            break;
                        case 16:
                            ushort[][][][] ResImage16 = new ushort[final_ImageCount][][][];

                            for (int i = 0; i < final_ImageCount; i++)
                                ResImage16[i] = new ushort[fi.sizeT + 1][][];
                            //order the frames for processing
                            for (int indZC = 0; indZC < ZC; indZC++)
                                for (int i = indZC, frame = 0; i < fi.imageCount; i += ZC, frame++)
                                    ResImage16[indZC][frame] = fi.image16bit[i];

                            fi.image16bit = ProjectionEvent(ResImage16, fi, max, min, avg);
                            fi.image16bitFilter = fi.image16bit;
                            break;
                    }
                    fi.FilterHistory.Clear();
                    if (fi.newFilterHistory != null)
                        for (int i = 0; i < fi.newFilterHistory.Length; i++)
                        {
                            fi.newFilterHistory[i].Clear();
                            fi.isBinary[i] = false;
                        }

                    fi.imageCount = final_ImageCount;
                    fi.sizeT = 1;
                    fi.frame = 0;

                    tTrackChange = true;
                }
                ((BackgroundWorker)o).ReportProgress(0);
            });

            bgw.ProgressChanged += new ProgressChangedEventHandler(delegate (Object o, ProgressChangedEventArgs a)
            {
                if (a.ProgressPercentage == 0)
                {
                    IA.RoiMan.SelectedROIsList.Clear();
                    IA.RoiMan.current = null;

                    if (tTrackChange)
                        IA.TabPages.tTrackBar.Panel.Visible = false;
                    if (zTrackChange)
                        IA.TabPages.zTrackBar.Panel.Visible = false;
                    if (LutNChange)
                    {
                        fi.tpTaskbar.AddColorBtn();
                        fi.tpTaskbar.VisualizeColorBtns();
                    }
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
            IA.FileBrowser.StatusLabel.Text = "Projecting...";
            //start bgw
            bgw.RunWorkerAsync();
        }
        private void FI_reduseC(TifFileInfo fi)
        {
            #region Segmentation variables
            fi.SegmentationCBoxIndex = new int[] { fi.SegmentationCBoxIndex[fi.cValue]};
            fi.SegmentationProtocol = new int[] { fi.SegmentationProtocol[fi.cValue] };
            fi.thresholdsCBoxIndex = new int[] { fi.thresholdsCBoxIndex[fi.cValue] };
            fi.sumHistogramChecked = new bool[] { fi.sumHistogramChecked[fi.cValue] };
            fi.thresholdValues = new int[][] { fi.thresholdValues[fi.cValue] };
            fi.thresholdColors = new Color[][] { fi.thresholdColors[fi.cValue] }; 
            fi.RefThresholdColors = new Color[][] { fi.RefThresholdColors[fi.cValue] }; 
            fi.thresholds = new int[] { fi.thresholds[fi.cValue] }; 
            fi.SpotColor = new Color[] { fi.SpotColor[fi.cValue] }; 
            fi.RefSpotColor = new Color[] { fi.RefSpotColor[fi.cValue] }; 
            fi.SelectedSpotThresh = new int[] { fi.SelectedSpotThresh[fi.cValue] }; 
            fi.SpotThresh = new int[] { fi.SpotThresh[fi.cValue] }; 
            fi.typeSpotThresh = new int[] { fi.typeSpotThresh[fi.cValue] }; 
            fi.SpotTailType = new string[] { fi.SpotTailType[fi.cValue] }; 
            fi.spotSensitivity = new int[] { fi.spotSensitivity[fi.cValue] }; 
            fi.roiList = new List<ROI>[] { fi.roiList[fi.cValue] }; 
            fi.tracking_MaxSize = new int[] { fi.tracking_MaxSize[fi.cValue] }; 
            fi.tracking_MinSize = new int[] { fi.tracking_MinSize[fi.cValue] }; 
            fi.tracking_Speed = new int[] { fi.tracking_Speed[fi.cValue] }; 
           
            #endregion Segmentation variables
        }
        private byte[][][] ProjectionEvent(byte[][][][] ResImage, TifFileInfo fi, bool max, bool min, bool avg)
        {
            Parallel.For(0, ResImage.Length, i =>
            {
                byte[][][] images = ResImage[i];
                
                if (max)
                    for (int x = 0; x < fi.sizeX; x++)
                        for (int y = 0; y < fi.sizeY; y++)
                        {
                            byte val = 0;

                            foreach (byte[][] image in images)
                                if(image!=null && val < image[y][x])
                                    val = image[y][x];
                            
                            images[0][y][x] = val;
                        }
                else if (min)
                    for (int x = 0; x < fi.sizeX; x++)
                        for (int y = 0; y < fi.sizeY; y++)
                        {
                            byte val = byte.MaxValue;

                            foreach (byte[][] image in images)
                                if (image != null && val > image[y][x])
                                    val = image[y][x];

                            images[0][y][x] = val;
                        }
                else if (avg)
                    for (int x = 0; x < fi.sizeX; x++)
                        for (int y = 0; y < fi.sizeY; y++)
                        {
                            long val = 0;

                            foreach (byte[][] image in images)
                                if (image != null)
                                    val += image[y][x];

                            val /= images.Length;

                            if (val > byte.MaxValue)
                                images[0][y][x] = byte.MaxValue;
                            else
                                images[0][y][x] = (byte)val;
                        }
            });

            byte[][][] newImage = new byte[ResImage.Length][][];
            for (int i = 0; i < ResImage.Length; i++)
                newImage[i] = ResImage[i][0];

            return newImage;
        }
        private ushort[][][] ProjectionEvent(ushort[][][][] ResImage, TifFileInfo fi, bool max, bool min, bool avg)
        {
            Parallel.For(0, ResImage.Length, i =>
            {
                ushort[][][] images = ResImage[i];

                if (max)
                    for (int x = 0; x < fi.sizeX; x++)
                        for (int y = 0; y < fi.sizeY; y++)
                        {
                            ushort val = 0;

                            foreach (ushort[][] image in images)
                                if (image != null && val < image[y][x])
                                    val = image[y][x];

                            images[0][y][x] = val;
                        }
                else if (min)
                    for (int x = 0; x < fi.sizeX; x++)
                        for (int y = 0; y < fi.sizeY; y++)
                        {
                            ushort val = ushort.MaxValue;

                            foreach (ushort[][] image in images)
                                if (image != null && val > image[y][x])
                                    val = image[y][x];

                            images[0][y][x] = val;
                        }
                else if (avg)
                    for (int x = 0; x < fi.sizeX; x++)
                        for (int y = 0; y < fi.sizeY; y++)
                        {
                            long val = 0;

                            foreach (ushort[][] image in images)
                                if (image != null)
                                    val += image[y][x];

                            val /= images.Length;

                            if (val > ushort.MaxValue)
                                images[0][y][x] = ushort.MaxValue;
                            else
                                images[0][y][x] = (ushort)val;
                        }
            });

            ushort[][][] newImage = new ushort[ResImage.Length][][];
            for (int i = 0; i < ResImage.Length; i++)
                newImage[i] = ResImage[i][0];

            return newImage;
        }
        #endregion Projection

        #region Image convert
        private void ConvertTo8bit(object sender, EventArgs e)
        {
            if (((ToolStripMenuItem)sender).Image == Properties.Resources.CheckMark) return;
            // calculate fileinfo
            TifFileInfo fi = null;
            try
            {
                fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
            }
            catch { return; }

            if (fi == null) { return; }

            if (fi.available == false) return;

            if (fi.bitsPerPixel == 8) return;

            var bgw = new BackgroundWorker();
            bgw.WorkerReportsProgress = true;

            //Add event for projection here
            bgw.DoWork += new DoWorkEventHandler(delegate (Object o, DoWorkEventArgs a)
            {
                //Prepare array
                byte[][][] resImage = new byte[fi.imageCount][][];

                Parallel.For(0, fi.imageCount, frame =>
                    {
                        byte[][] oneImage = new byte[fi.sizeY][];
                        ushort[][] oldImage = fi.image16bit[frame];

                        for (int y = 0; y < fi.sizeY; y++)
                        {
                            byte[] row = new byte[fi.sizeX];
                            for (int x = 0; x < fi.sizeX; x++)
                            {
                                int val = (int)(oldImage[y][x] / 256);
                                if (val <= 254)
                                    row[x] = (byte)val;
                                else
                                    row[x] = 254;

                            }
                            oneImage[y] = row;
                        }
                        resImage[frame] = oneImage;
                    });

                //Refresh settings
                fi.adjustedLUT = null;
                fi.histogramArray = null;
                fi.autoDetectBandC = true;
                fi.bitsPerPixel = 8;
                fi.image16bit = null;
                fi.image16bitFilter = null;
                fi.image8bit = resImage;
                fi.image8bitFilter = fi.image8bit;
                fi.FilterHistory.Clear();
                //report progress
                ((BackgroundWorker)o).ReportProgress(0);
            });

            bgw.ProgressChanged += new ProgressChangedEventHandler(delegate (Object o, ProgressChangedEventArgs a)
            {
                if (a.ProgressPercentage == 0)
                {
                    //reload images to screen
                    RemeasureRois(fi);
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
            IA.FileBrowser.StatusLabel.Text = "Converting...";
            //start bgw
            bgw.RunWorkerAsync();
        }
        private void ConvertTo16bit(object sender, EventArgs e)
        {
            if (((ToolStripMenuItem)sender).Image == Properties.Resources.CheckMark) return;
            // calculate fileinfo
            TifFileInfo fi = null;
            try
            {
                fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
            }
            catch { return; }

            if (fi == null) { return; }
            if (fi.available == false) return;
            if (fi.bitsPerPixel == 16) return;

            var bgw = new BackgroundWorker();
            bgw.WorkerReportsProgress = true;

            //Add event for projection here
            bgw.DoWork += new DoWorkEventHandler(delegate (Object o, DoWorkEventArgs a)
            {
                //Prepare array
                ushort[][][] resImage = new ushort[fi.imageCount][][];

                Parallel.For(0, fi.imageCount, frame =>
                {
                    ushort[][] oneImage = new ushort[fi.sizeY][];
                    byte[][] oldImage = fi.image8bit[frame];

                    for (int y = 0; y < fi.sizeY; y++)
                    {
                        ushort[] row = new ushort[fi.sizeX];
                        for (int x = 0; x < fi.sizeX; x++)
                        {
                            int val = (int)(oldImage[y][x] * 256);
                            if (val < ushort.MaxValue)
                                row[x] = (ushort)val;
                            else
                                row[x] = ushort.MaxValue;
                        }

                        oneImage[y] = row;
                    }
                    resImage[frame] = oneImage;
                });

                //Refresh settings
                fi.adjustedLUT = null;
                fi.histogramArray = null;
                fi.autoDetectBandC = true;
                fi.bitsPerPixel = 16;
                fi.image8bit = null;
                fi.image8bitFilter = null;
                fi.image16bit = resImage;
                fi.image16bitFilter = fi.image16bit;
                fi.FilterHistory.Clear();
                //report progress
                ((BackgroundWorker)o).ReportProgress(0);
            });

            bgw.ProgressChanged += new ProgressChangedEventHandler(delegate (Object o, ProgressChangedEventArgs a)
            {
                if (a.ProgressPercentage == 0)
                {
                    //reload images to screen
                    RemeasureRois(fi);
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
            IA.FileBrowser.StatusLabel.Text = "Converting...";
            //start bgw
            bgw.RunWorkerAsync();
        }

        #endregion Image convert

        #region Image Concatenator

        private int MeargeTForm_Initialize(string title, List<int> indexList)
        {
            int res = -1;

            Form OptionForm = new Form();
            OptionForm.FormBorderStyle = FormBorderStyle.FixedDialog;
            OptionForm.Text = title;
            OptionForm.StartPosition = FormStartPosition.CenterScreen;
            OptionForm.WindowState = FormWindowState.Normal;
            OptionForm.MinimizeBox = false;
            OptionForm.MaximizeBox = false;

            OptionForm.Width = 220;
            OptionForm.Height = 150;

            Label label1 = new Label();
            label1.Text = "Merge " + IA.TabPages.Collections[IA.TabPages.SelectedIndex][0].Text;
            label1.Width = TextRenderer.MeasureText(label1.Text, label1.Font).Width+5;
            label1.Location = new Point(5, 15);
            label1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            OptionForm.Controls.Add(label1);
            
            Label label2 = new Label();
            label2.Text = "With:";
            label2.Width = 35;
            label2.Location = new Point(5, 45);
            OptionForm.Controls.Add(label2);

            ComboBox cmbBox = new ComboBox();
            cmbBox.DropDown += AdjustWidthComboBox_DropDown;
            cmbBox.Width = 160;
            cmbBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cmbBox.Location = new Point(40, 43);
            OptionForm.Controls.Add(cmbBox);
            foreach (int ind in indexList)
                cmbBox.Items.Add(IA.TabPages.Collections[ind][0].Text);
            cmbBox.SelectedIndex = 0;

            Panel okBox = new Panel();
            okBox.Height = 40;
            okBox.Dock = DockStyle.Bottom;
            OptionForm.Controls.Add(okBox);

            Button okBtn = new Button();
            okBtn.Text = "Merge";
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
                //event
                res = indexList[cmbBox.SelectedIndex];
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

            if(20 + label1.Width > OptionForm.Width)  OptionForm.Width = 20 + label1.Width;
            okBtn.Location = new Point(OptionForm.Width/2 - 10 -okBtn.Width, 10);
            cancelBtn.Location = new Point(OptionForm.Width / 2 - 5, 10);

            // Linux change
            IA.FileBrowser.StatusLabel.Text = "Dialog open";
            OptionForm.ShowDialog();
            IA.FileBrowser.StatusLabel.Text = "Ready";

            return res;
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
        private void AddTToolStripMenuItem_click(object sender, EventArgs e)
        {
            TifFileInfo fi = findFI();
            if (fi == null) return;
            TifFileInfo curFI = null;

            List<int> indexList = new List<int>();
            for (int i = 0; i < IA.TabPages.TabCollections.Count; i++)
            {
                curFI = IA.TabPages.TabCollections[i].tifFI;
                if (curFI != null &&
                    curFI != fi &&
                    curFI.bitsPerPixel == fi.bitsPerPixel &&
                    fi.sizeX == curFI.sizeX &&
                    fi.sizeY == curFI.sizeY &&
                    fi.sizeC == curFI.sizeC &&
                    fi.sizeZ == curFI.sizeZ)
                    indexList.Add(i);
            }

            if (indexList.Count == 0)
            {
                MessageBox.Show("There is not compatible image for merging!");
                return;
            }

            int res = MeargeTForm_Initialize("Merge T slices", indexList);
            if (res == -1) return;
            curFI = IA.TabPages.TabCollections[res].tifFI;
            if (curFI.available == false) return;

            fi.sizeT += curFI.sizeT;
            fi.imageCount += curFI.imageCount;
            fi.openedImages += fi.openedImages;
            //add T event
            int counter = 0;
            //background worker
            var bgw = new BackgroundWorker();
            bgw.WorkerReportsProgress = true;
            fi.available = false;
            curFI.available = false;
            //Add event for projection here
            bgw.DoWork += new DoWorkEventHandler(delegate (Object o, DoWorkEventArgs a)
            {
                switch (fi.bitsPerPixel)
                {
                    case 8:
                        byte[][][] newImage = new byte[fi.imageCount][][];

                        foreach (byte[][] image in fi.image8bit)
                        {
                            newImage[counter] = image;
                            counter++;
                        }

                        foreach (byte[][] image in curFI.image8bit)
                        {
                            newImage[counter] = image;
                            counter++;
                        }

                        fi.image8bit = newImage;

                        fi.image8bitFilter = fi.image8bit;
                        fi.FilterHistory.Clear();

                        if (fi.newFilterHistory != null)
                            for (int i = 0; i < fi.newFilterHistory.Length; i++)
                            {
                                fi.newFilterHistory[i].Clear();
                                fi.isBinary[i] = false;
                            }

                        break;
                    case 16:
                        ushort[][][] newImage16 = new ushort[fi.imageCount][][];

                        foreach (ushort[][] image in fi.image16bit)
                        {
                            newImage16[counter] = image;
                            counter++;
                        }

                        foreach (ushort[][] image in curFI.image16bit)
                        {
                            newImage16[counter] = image;
                            counter++;
                        }

                        fi.image16bit = newImage16;

                        fi.image16bitFilter = fi.image16bit;
                        fi.FilterHistory.Clear();
                        if (fi.newFilterHistory != null)
                            for (int i = 0; i < fi.newFilterHistory.Length; i++)
                            {
                                fi.newFilterHistory[i].Clear();
                                fi.isBinary[i] = false;
                            }
                        break;
                }
                ((BackgroundWorker)o).ReportProgress(0);
            });

            bgw.ProgressChanged += new ProgressChangedEventHandler(delegate (Object o, ProgressChangedEventArgs a)
            {
                if (a.ProgressPercentage == 0)
                {

                    //
                    IA.TabPages.TabCollections[res].Saved = true;
                    ((Button)IA.TabPages.Collections[res][1]).PerformClick();
                    IA.TabPages.selectTab_event(IA.TabPages.SelectedIndex);

                    IA.MarkAsNotSaved();
                    ClearRois(fi);
                    RemeasureRois(fi);
                    IA.ReloadImages();
                }
                fi.available = true;
                curFI.available = true;
                IA.FileBrowser.StatusLabel.Text = "Ready";
            });
            //Start background worker
            IA.FileBrowser.StatusLabel.Text = "Merging T slices...";
            //start bgw
            bgw.RunWorkerAsync();
        }
        private void AddCToolStripMenuItem_click(object sender, EventArgs e)
        {
            TifFileInfo fi = findFI();
            if (fi == null) return;
            TifFileInfo curFI = null;

            List<int> indexList = new List<int>();
            for (int i = 0; i < IA.TabPages.TabCollections.Count; i++)
            {
                curFI = IA.TabPages.TabCollections[i].tifFI;
                if (curFI != null &&
                    curFI != fi &&
                    curFI.bitsPerPixel == fi.bitsPerPixel &&
                    fi.sizeX == curFI.sizeX &&
                    fi.sizeY == curFI.sizeY &&
                    fi.sizeT == curFI.sizeT &&
                    fi.sizeZ == curFI.sizeZ)
                    indexList.Add(i);
            }

            if (indexList.Count == 0)
            {
                MessageBox.Show("There is not compatible image for merging!");
                return;
            }

            int res = MeargeTForm_Initialize("Merge chanels", indexList);
            if (res == -1) return;
            curFI = IA.TabPages.TabCollections[res].tifFI;
            if (curFI.available == false) return;

            fi.imageCount += curFI.imageCount;
            fi.openedImages += fi.openedImages;
            //add C event
            int counter = 0;
            int c = 0;
            //background worker
            var bgw = new BackgroundWorker();
            bgw.WorkerReportsProgress = true;
            fi.available = false;
            curFI.available = false;
            //Add event for projection here
            bgw.DoWork += new DoWorkEventHandler(delegate (Object o, DoWorkEventArgs a)
            {
                switch (fi.bitsPerPixel)
                {
                    case 8:
                        byte[][][] newImage = new byte[fi.imageCount][][];

                        foreach (byte[][] image in fi.image8bit)
                        {
                            newImage[counter] = image;
                            counter++;
                            c++;

                            if (c == fi.sizeC)
                            {
                                c = 0;
                                counter += curFI.sizeC;
                            }
                        }

                        counter = fi.sizeC;
                        c = 0;

                        foreach (byte[][] image in curFI.image8bit)
                        {
                            newImage[counter] = image;
                            counter++;
                            c++;

                            if (c == curFI.sizeC)
                            {
                                c = 0;
                                counter += fi.sizeC;
                            }
                        }

                        fi.image8bit = newImage;

                        fi.image8bitFilter = fi.image8bit;
                        fi.FilterHistory.Clear();
                        if (fi.newFilterHistory != null)
                            for (int i = 0; i < fi.newFilterHistory.Length; i++)
                            {
                                fi.newFilterHistory[i].Clear();
                                fi.isBinary[i] = false;
                            }
                        break;
                    case 16:
                        ushort[][][] newImage16 = new ushort[fi.imageCount][][];
                        foreach (ushort[][] image in fi.image16bit)
                        {
                            newImage16[counter] = image;
                            counter++;
                            c++;
                            if (c == fi.sizeC)
                            {
                                c = 0;
                                counter += curFI.sizeC;
                            }
                        }

                        counter = fi.sizeC;

                        foreach (ushort[][] image in curFI.image16bit)
                        {
                            newImage16[counter] = image;
                            counter++;
                            c++;
                            if (c == curFI.sizeC)
                            {
                                c = 0;
                                counter += fi.sizeC;
                            }
                        }

                        fi.image16bit = newImage16;

                        fi.image16bitFilter = fi.image16bit;
                        fi.FilterHistory.Clear();
                        if (fi.newFilterHistory != null)
                            for (int i = 0; i < fi.newFilterHistory.Length; i++)
                            {
                                fi.newFilterHistory[i].Clear();
                                fi.isBinary[i] = false;
                            }
                        break;
                }

                ((BackgroundWorker)o).ReportProgress(0);
            });

            bgw.ProgressChanged += new ProgressChangedEventHandler(delegate (Object o, ProgressChangedEventArgs a)
            {
                if (a.ProgressPercentage == 0)
                {
                    fi.sizeC += curFI.sizeC;
                    FI_increaseC(fi, curFI);
                    fi.tpTaskbar.AddColorBtn();
                    fi.tpTaskbar.VisualizeColorBtns();
                    //

                    IA.TabPages.TabCollections[res].Saved = true;
                    ((Button)IA.TabPages.Collections[res][1]).PerformClick();

                    IA.MarkAsNotSaved();
                    ClearRois(fi);
                    RemeasureRois(fi);
                    IA.ReloadImages();
                }
                fi.available = true;
                curFI.available = true;
                IA.FileBrowser.StatusLabel.Text = "Ready";
            });
            //Start background worker
            IA.FileBrowser.StatusLabel.Text = "Merging chanels...";
            //start bgw
            bgw.RunWorkerAsync();
        }
        private void FI_increaseC(TifFileInfo fi, TifFileInfo curFI)
        {
            #region Segmentation variables
            foreach (Color col in curFI.LutList)
                fi.LutList.Add(col);

            IA.RoiMan.current = null;

            fi.histogramArray = CopyArray(fi.histogramArray,curFI.histogramArray);
            fi.MinBrightness = CopyArray(fi.MinBrightness, curFI.MinBrightness);
            fi.MaxBrightness = CopyArray(fi.MaxBrightness, curFI.MaxBrightness);
            fi.adjustedLUT = CopyArray(fi.adjustedLUT, curFI.adjustedLUT);
            fi.SegmentationCBoxIndex = CopyArray(fi.SegmentationCBoxIndex, curFI.SegmentationCBoxIndex);
            fi.SegmentationProtocol = CopyArray(fi.SegmentationProtocol, curFI.SegmentationProtocol);
            fi.thresholdsCBoxIndex = CopyArray(fi.thresholdsCBoxIndex, curFI.thresholdsCBoxIndex);
            fi.sumHistogramChecked = CopyArray(fi.sumHistogramChecked, curFI.sumHistogramChecked);
            fi.thresholdValues = CopyArray(fi.thresholdValues, curFI.thresholdValues);
            fi.thresholdColors = CopyArray(fi.thresholdColors, curFI.thresholdColors);
            fi.RefThresholdColors = CopyArray(fi.RefThresholdColors, curFI.RefThresholdColors);
            fi.thresholds = CopyArray(fi.thresholds, curFI.thresholds);
            fi.SpotColor = CopyArray(fi.SpotColor, curFI.SpotColor);
            fi.RefSpotColor = CopyArray(fi.RefSpotColor, curFI.RefSpotColor);
            fi.SelectedSpotThresh = CopyArray(fi.SelectedSpotThresh, curFI.SelectedSpotThresh);
            fi.SpotThresh = CopyArray(fi.SpotThresh, curFI.SpotThresh);
            fi.typeSpotThresh = CopyArray(fi.typeSpotThresh, curFI.typeSpotThresh);
            fi.SpotTailType = CopyArray(fi.SpotTailType, curFI.SpotTailType);
            fi.spotSensitivity = CopyArray(fi.spotSensitivity, curFI.spotSensitivity);
            fi.roiList = CopyArray(fi.roiList, curFI.roiList);
            fi.tracking_MaxSize = CopyArray(fi.tracking_MaxSize, curFI.tracking_MaxSize);
            fi.tracking_MinSize = CopyArray(fi.tracking_MinSize, curFI.tracking_MinSize);
            fi.tracking_Speed = CopyArray(fi.tracking_Speed, curFI.tracking_Speed);

            #endregion Segmentation variables
        }
        private List<ROI>[] CopyArray(List<ROI>[] front, List<ROI>[] back)
        {
            if (front == null | back == null) return null;

            List<ROI>[] combined = new List<ROI>[front.Length + back.Length];
            //Array.Copy(front, combined, front.Length);
            //Array.Copy(back, 0, combined, front.Length, back.Length);
            for (int i = 0; i < combined.Length; i++)
                combined[i] = new List<ROI>();

            return combined;
        }
        private float[][] CopyArray(float[][] front, float[][] back)
        {
            if (front == null | back == null) return null;

            float[][] combined = new float[front.Length + back.Length][];
            Array.Copy(front, combined, front.Length);
            Array.Copy(back, 0, combined, front.Length, back.Length);

            return combined;
        }
        private int[] CopyArray(int[] front, int[] back)
        {
            if (front == null | back == null) return null;

            int[] combined = new int[front.Length + back.Length];
            Array.Copy(front, combined, front.Length);
            Array.Copy(back, 0, combined, front.Length, back.Length);
            
            return combined;
        }
        private string[] CopyArray(string[] front, string[] back)
        {
            if (front == null | back == null) return null;

            string[] combined = new string[front.Length + back.Length];
            Array.Copy(front, combined, front.Length);
            Array.Copy(back, 0, combined, front.Length, back.Length);

            return combined;
        }
        private int[][] CopyArray(int[][] front, int[][] back)
        {
            if (front == null | back == null) return null;

            int[][] combined = new int[front.Length + back.Length][];
            Array.Copy(front, combined, front.Length);
            Array.Copy(back, 0, combined, front.Length, back.Length);

            int[] refer = null;
            foreach (int[] l in combined)
                if (l.Length > 0)
                {
                    refer = l;
                    break;
                }

            for (int i = 0; i < combined.Length; i++)
                if (combined[i].Length == 0)
                {
                    combined[i] = duplicateColorArray(refer);
                }

            return combined;
        }
        private int[] duplicateColorArray(int[] l)
        {
            if (l == null) return null;
            int[] refer = new int[l.Length];
            for (int i = 0; i < refer.Length; i++)
            {
                refer[i] = l[i];
            }
            return refer;
        }
        private bool[] CopyArray(bool[] front, bool[] back)
        {
            if (front == null | back == null) return null;

            bool[] combined = new bool[front.Length + back.Length];
            Array.Copy(front, combined, front.Length);
            Array.Copy(back, 0, combined, front.Length, back.Length);

            return combined;
        }
        private Color[] CopyArray(Color[] front, Color[] back)
        {
            if (front == null | back == null) return null;

            Color[] combined = new Color[front.Length + back.Length];
            Array.Copy(front, combined, front.Length);
            Array.Copy(back, 0, combined, front.Length, back.Length);

            return combined;
        }
        private Color[][] CopyArray(Color[][] front, Color[][] back)
        {
            if (front == null | back == null) return null;

            Color[][] combined = new Color[front.Length + back.Length][];
            Array.Copy(front, combined, front.Length);
            Array.Copy(back, 0, combined, front.Length, back.Length);

            Color[] refer = null;

            foreach (Color[] l in combined)
                if (l.Length > 0)
                {
                    refer = l;
                    break;
                }

            for (int i = 0; i < combined.Length; i++)
                if (combined[i].Length == 0)
                {
                    combined[i] = duplicateColorArray(refer);
                }

           return combined;
        }
        private Color[] duplicateColorArray(Color[] l)
        {
            if (l == null) return null;
            Color[] refer = new Color[l.Length];
            for (int i = 0; i < refer.Length; i++)
            {
                refer[i] = l[i];
            }
            return refer;
        }
        private void AddZToolStripMenuItem_click(object sender, EventArgs e)
        {
            TifFileInfo fi = findFI();
            if (fi == null) return;
            TifFileInfo curFI = null;

            List<int> indexList = new List<int>();
            for (int i = 0; i < IA.TabPages.TabCollections.Count; i++)
            {
                curFI = IA.TabPages.TabCollections[i].tifFI;
                if (curFI != null &&
                    curFI != fi &&
                    curFI.bitsPerPixel == fi.bitsPerPixel &&
                    fi.sizeX == curFI.sizeX &&
                    fi.sizeY == curFI.sizeY &&
                    fi.sizeC == curFI.sizeC &&
                    fi.sizeT == curFI.sizeT)
                    indexList.Add(i);
            }

            if (indexList.Count == 0)
            {
                MessageBox.Show("There is not compatible image for merging!");
                return;
            }

            int res = MeargeTForm_Initialize("Merge Z planes", indexList);
            if (res == -1) return;
            curFI = IA.TabPages.TabCollections[res].tifFI;
            if (curFI.available == false) return;

            fi.imageCount += curFI.imageCount;
            fi.openedImages += fi.openedImages;
            //add Z event
            int counter = 0;
            int c = 0;

            //background worker
            var bgw = new BackgroundWorker();
            bgw.WorkerReportsProgress = true;
            fi.available = false;
            curFI.available = false;
            //Add event for projection here
            bgw.DoWork += new DoWorkEventHandler(delegate (Object o, DoWorkEventArgs a)
            {
                switch (fi.bitsPerPixel)
                {
                    case 8:
                        byte[][][] newImage = new byte[fi.imageCount][][];

                        foreach (byte[][] image in fi.image8bit)
                        {
                            newImage[counter] = image;
                            counter++;
                            c++;

                            if (c == fi.sizeC * fi.sizeZ)
                            {
                                c = 0;
                                counter += curFI.sizeC * curFI.sizeZ;
                            }
                        }

                        counter = fi.sizeC * fi.sizeZ;
                        c = 0;

                        foreach (byte[][] image in curFI.image8bit)
                        {
                            newImage[counter] = image;
                            counter++;
                            c++;

                            if (c == curFI.sizeC * curFI.sizeZ)
                            {
                                c = 0;
                                counter += fi.sizeC * fi.sizeZ;
                            }
                        }

                        fi.image8bit = newImage;

                        fi.image8bitFilter = fi.image8bit;
                        fi.FilterHistory.Clear();

                        if (fi.newFilterHistory != null)
                            for (int i = 0; i < fi.newFilterHistory.Length; i++)
                            {
                                fi.newFilterHistory[i].Clear();
                                fi.isBinary[i] = false;
                            }
                        break;
                    case 16:
                        ushort[][][] newImage16 = new ushort[fi.imageCount][][];
                        foreach (ushort[][] image in fi.image16bit)
                        {
                            newImage16[counter] = image;
                            counter++;
                            c++;
                            if (c == fi.sizeC * fi.sizeZ)
                            {
                                c = 0;
                                counter += curFI.sizeC * curFI.sizeZ;
                            }
                        }

                        counter = fi.sizeC * fi.sizeZ;

                        foreach (ushort[][] image in curFI.image16bit)
                        {
                            newImage16[counter] = image;
                            counter++;

                            c++;
                            if (c == curFI.sizeC * curFI.sizeZ)
                            {
                                c = 0;
                                counter += fi.sizeC * fi.sizeZ;
                            }
                        }

                        fi.image16bit = newImage16;

                        fi.image16bitFilter = fi.image16bit;
                        fi.FilterHistory.Clear();
                        if (fi.newFilterHistory != null)
                            for (int i = 0; i < fi.newFilterHistory.Length; i++)
                            {
                                fi.newFilterHistory[i].Clear();
                                fi.isBinary[i] = false;
                            }
                        break;
                }

                 ((BackgroundWorker)o).ReportProgress(0);
            });

            bgw.ProgressChanged += new ProgressChangedEventHandler(delegate (Object o, ProgressChangedEventArgs a)
            {
                if (a.ProgressPercentage == 0)
                {

                    fi.sizeZ += curFI.sizeZ;
                    //

                    IA.TabPages.TabCollections[res].Saved = true;
                    ((Button)IA.TabPages.Collections[res][1]).PerformClick();
                    IA.TabPages.selectTab_event(IA.TabPages.SelectedIndex);

                    IA.MarkAsNotSaved();
                    ClearRois(fi);
                    RemeasureRois(fi);
                    IA.ReloadImages();
                }
                fi.available = true;
                curFI.available = true;
                IA.FileBrowser.StatusLabel.Text = "Ready";
            });
            //Start background worker
            IA.FileBrowser.StatusLabel.Text = "Merging Z planes...";
            //start bgw
            bgw.RunWorkerAsync();
        }
        #endregion Image Concatenator

        #region Image Deleter
        private void DeleteCToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TifFileInfo fi = findFI();
            if (fi == null) return;
            int count = 0;
            //background worker
            var bgw = new BackgroundWorker();
            bgw.WorkerReportsProgress = true;
            fi.available = false;
            //Add event for projection here
            bgw.DoWork += new DoWorkEventHandler(delegate (Object o, DoWorkEventArgs a)
            {
                switch (fi.bitsPerPixel)
                {
                    case 8:
                        for (int i = fi.cValue; i < fi.imageCount; i += fi.sizeC, count++)
                            fi.image8bit[i] = null;

                        fi.imageCount -= count;
                        fi.openedImages = fi.imageCount;

                        fi.image8bit = shrinkImage(fi.imageCount, fi.image8bit);

                        fi.image8bitFilter = fi.image8bit;
                        break;
                    case 16:
                        for (int i = fi.cValue; i < fi.imageCount; i += fi.sizeC, count++)
                            fi.image16bit[i] = null;

                        fi.imageCount -= count;
                        fi.openedImages = fi.imageCount;

                        fi.image16bit = shrinkImage(fi.imageCount, fi.image16bit);

                        fi.image16bitFilter = fi.image16bit;
                        break;
                }
                ((BackgroundWorker)o).ReportProgress(0);
            });

            bgw.ProgressChanged += new ProgressChangedEventHandler(delegate (Object o, ProgressChangedEventArgs a)
            {
                if (a.ProgressPercentage == 0)
                {
                    shrinkCInfo(fi);

                    fi.sizeC -= 1;
                    fi.cValue -= 1;
                    if (fi.cValue < 0) fi.cValue = 0;

                    IA.TabPages.selectTab_event(IA.TabPages.SelectedIndex);
                    IA.MarkAsNotSaved();

                    fi.tpTaskbar.AddColorBtn();
                    fi.tpTaskbar.VisualizeColorBtns();
                    IA.ReloadImages();
                }
                fi.available = true;
                IA.FileBrowser.StatusLabel.Text = "Ready";
            });
            //Start background worker
            IA.FileBrowser.StatusLabel.Text = "Deleting chanel...";
            //start bgw
            bgw.RunWorkerAsync();
        }
        private int[][] shrinkArray(int[][] source, int index)
        {
            int[][] target = new int[source.Length - 1][];

            for(int i = 0, a=0; i< source.Length; i++)
                if (i != index)
                {
                    target[a] = source[i];
                    a++;
                }

            return target;
        }
        private Color[][] shrinkArray(Color[][] source, int index)
        {
            Color[][] target = new Color[source.Length - 1][];

            for (int i = 0, a = 0; i < source.Length; i++)
                if (i != index)
                {
                    target[a] = source[i];
                    a++;
                }

            return target;
        }
        private int[] shrinkArray(int[] source, int index)
        {
            int[] target = new int[source.Length - 1];

            for (int i = 0, a = 0; i < source.Length; i++)
                if (i != index)
                {
                    target[a] = source[i];
                    a++;
                }

            return target;
        }
        private Color[] shrinkArray(Color[] source, int index)
        {
            Color[] target = new Color[source.Length - 1];

            for (int i = 0, a = 0; i < source.Length; i++)
                if (i != index)
                {
                    target[a] = source[i];
                    a++;
                }

            return target;
        }
        private bool[] shrinkArray(bool[] source, int index)
        {
            bool[] target = new bool[source.Length - 1];

            for (int i = 0, a = 0; i < source.Length; i++)
                if (i != index)
                {
                    target[a] = source[i];
                    a++;
                }

            return target;
        }
        private float[] shrinkArray(float[] source, int index)
        {
            float[] target = new float[source.Length - 1];

            for (int i = 0, a = 0; i < source.Length; i++)
                if (i != index)
                {
                    target[a] = source[i];
                    a++;
                }

            return target;
        }
        private float[][] shrinkArray(float[][] source, int index)
        {
            float[][] target = new float[source.Length - 1][];

            for (int i = 0, a = 0; i < source.Length; i++)
                if (i != index)
                {
                    target[a] = source[i];
                    a++;
                }

            return target;
        }
        private string[] shrinkArray(string[] source, int index)
        {
            string[] target = new string[source.Length - 1];

            for (int i = 0, a = 0; i < source.Length; i++)
                if (i != index)
                {
                    target[a] = source[i];
                    a++;
                }

            return target;
        }
        private void shrinkCInfo(TifFileInfo fi)
        {
            #region Segmentation variables
            int i = fi.cValue;
            
            fi.LutList.RemoveAt(i);
            fi.histogramArray = shrinkArray(fi.histogramArray, i);
            fi.MinBrightness = shrinkArray(fi.MinBrightness, i);
            fi.MaxBrightness = shrinkArray(fi.MaxBrightness, i);
            fi.adjustedLUT = shrinkArray(fi.adjustedLUT, i);
            fi.SegmentationCBoxIndex = shrinkArray(fi.SegmentationCBoxIndex, i);
            fi.SegmentationProtocol = shrinkArray(fi.SegmentationProtocol, i);
            fi.thresholdsCBoxIndex = shrinkArray(fi.thresholdsCBoxIndex, i);
            fi.sumHistogramChecked = shrinkArray(fi.sumHistogramChecked, i);
            fi.thresholdValues = shrinkArray(fi.thresholdValues, i);
            fi.thresholdColors = shrinkArray(fi.thresholdColors, i);
            fi.RefThresholdColors = shrinkArray(fi.RefThresholdColors, i);
            fi.thresholds = shrinkArray(fi.thresholds, i);
            fi.SpotColor = shrinkArray(fi.SpotColor, i);
            fi.RefSpotColor = shrinkArray(fi.RefSpotColor, i);
            fi.SelectedSpotThresh = shrinkArray(fi.SelectedSpotThresh, i);
            fi.SpotThresh = shrinkArray(fi.SpotThresh, i);
            fi.typeSpotThresh = shrinkArray(fi.typeSpotThresh, i);
            fi.SpotTailType = shrinkArray(fi.SpotTailType, i);
            fi.spotSensitivity = shrinkArray(fi.spotSensitivity, i);
            fi.tracking_MaxSize = shrinkArray(fi.tracking_MaxSize, i);
            fi.tracking_MinSize = shrinkArray(fi.tracking_MinSize, i);
            fi.tracking_Speed = shrinkArray(fi.tracking_Speed, i);

            //fi.roiList = shrinkArray(fi.roiList, i);
            IA.RoiMan.current = null;
            IA.RoiMan.SelectedROIsList.Clear();
            fi.roiList = new List<ROI>[fi.roiList.Length-1];
            for (int a = 0; a < fi.roiList.Length; a++)
                fi.roiList[a] = new List<ROI>();

            #endregion Segmentation variables
        }
        private void DeleteZToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TifFileInfo fi = findFI();
            if (fi == null) return;

            int curZ = fi.zValue;
            int CZ = fi.sizeC * fi.sizeZ;
            ShrinkRois(fi, -1, curZ);

            int count = 0;
            //background worker
            var bgw = new BackgroundWorker();
            bgw.WorkerReportsProgress = true;
            fi.available = false;
            //Add event for projection here
            bgw.DoWork += new DoWorkEventHandler(delegate (Object o, DoWorkEventArgs z)
            {
                switch (fi.bitsPerPixel)
                {
                    case 8:
                        for (int i = fi.sizeC * curZ; i < fi.imageCount; i += CZ)
                            for (int a = i; a < i + fi.sizeC; a++, count++)
                                fi.image8bit[a] = null;

                        fi.imageCount -= count;
                        fi.openedImages = fi.imageCount;

                        fi.image8bit = shrinkImage(fi.imageCount, fi.image8bit);

                        fi.image8bitFilter = fi.image8bit;
                        break;
                    case 16:
                        for (int i = fi.sizeC * curZ; i < fi.imageCount; i += CZ)
                            for (int a = i; a < i + fi.sizeC; a++, count++)
                                fi.image16bit[a] = null;

                        fi.imageCount -= count;
                        fi.openedImages = fi.imageCount;

                        fi.image16bit = shrinkImage(fi.imageCount, fi.image16bit);

                        fi.image16bitFilter = fi.image16bit;
                        break;
                }

                ((BackgroundWorker)o).ReportProgress(0);
            });

            bgw.ProgressChanged += new ProgressChangedEventHandler(delegate (Object o, ProgressChangedEventArgs a)
            {
                if (a.ProgressPercentage == 0)
                {
                    fi.sizeZ -= 1;
                    fi.zValue -= 1;
                    if (fi.zValue < 0) fi.zValue = 0;

                    IA.TabPages.selectTab_event(IA.TabPages.SelectedIndex);
                    IA.MarkAsNotSaved();
                    RemeasureRois(fi);
                    IA.ReloadImages();
                }
                fi.available = true;
                IA.FileBrowser.StatusLabel.Text = "Ready";
            });
            //Start background worker
            IA.FileBrowser.StatusLabel.Text = "Deleting Z plane...";
            //start bgw 
            bgw.RunWorkerAsync();
        }
        private void DeleteTToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TifFileInfo fi = findFI();
            if (fi == null) return;

            int curT = fi.frame;
            int CZ = fi.sizeC * fi.sizeZ;
            ShrinkRois(fi, curT, -1);

            fi.imageCount -= CZ;
            fi.openedImages = fi.imageCount;
            //background worker
            var bgw = new BackgroundWorker();
            bgw.WorkerReportsProgress = true;
            fi.available = false;
            //Add event for projection here
            bgw.DoWork += new DoWorkEventHandler(delegate (Object o, DoWorkEventArgs a)
            {
                switch (fi.bitsPerPixel)
                {
                    case 8:
                        for (int i = curT * CZ; i < curT * CZ + CZ; i++)
                            fi.image8bit[i] = null;

                        fi.image8bit = shrinkImage(fi.imageCount, fi.image8bit);

                        fi.image8bitFilter = fi.image8bit;
                        break;
                    case 16:
                        for (int i = curT * CZ; i < curT * CZ + CZ; i++)
                            fi.image16bit[i] = null;

                        fi.image16bit = shrinkImage(fi.imageCount, fi.image16bit);

                        fi.image16bitFilter = fi.image16bit;
                        break;
                }
                ((BackgroundWorker)o).ReportProgress(0);
            });

            bgw.ProgressChanged += new ProgressChangedEventHandler(delegate (Object o, ProgressChangedEventArgs a)
            {
                if (a.ProgressPercentage == 0)
                {

                    fi.sizeT -= 1;
                    fi.frame -= 1;
                    if (fi.frame < 0) fi.frame = 0;

                    IA.TabPages.selectTab_event(IA.TabPages.SelectedIndex);
                    IA.MarkAsNotSaved();
                    RemeasureRois(fi);
                    IA.ReloadImages();
                }
                fi.available = true;
                IA.FileBrowser.StatusLabel.Text = "Ready";
            });
            //Start background worker
            IA.FileBrowser.StatusLabel.Text = "Deleting T slice...";
            //start bgw
            bgw.RunWorkerAsync();
        }
        private void ShrinkRois(TifFileInfo fi, int fT, int fZ)
        {
            
            if (fi.roiList != null)
                for (int c = 0; c < fi.sizeC; c++)
                    if (fi.roiList[c] != null)
                        foreach (ROI roi in fi.roiList[c])
                            if (roi.Type == 1)
                            {
                                Point[][] oldLocations = roi.GetLocationAll();
                                List<Point[]> newLocations = new List<Point[]>();
                                for (int i = 0, t = 0, z = 0, c1 = 0; i < oldLocations.Length; i++, c1++)
                                {
                                    if (c1 == fi.sizeC)
                                    {
                                        c1 = 0;
                                        z++;
                                    }

                                    if (z == fi.sizeZ)
                                    {
                                        z = 0;
                                        t++;
                                    }

                                    if (t == fi.sizeT) t = 0;

                                    if (t != fT && z != fZ)
                                        newLocations.Add(oldLocations[i]);
                                }

                                roi.SetLocationAll(newLocations.ToArray());
                            }
        }
        private void ClearRois(TifFileInfo fi)
        {
            if (fi.roiList != null)
                for (int c = 0; c < fi.sizeC; c++)
                    if (fi.roiList[c] != null)
                        fi.roiList[c].Clear();
        }
        private void RemeasureRois(TifFileInfo fi)
        {
            if (fi.roiList != null)
                for (int c = 0; c < fi.sizeC; c++)
                    if (fi.roiList[c] != null)
                        foreach (ROI roi in fi.roiList[c])
                        {
                        RoiMeasure.Measure(roi, fi, c, IA);
                    }
        }
        private byte[][][] shrinkImage(int ImageCount, byte[][][] source)
        {
            byte[][][] target = new byte[ImageCount][][];
            for(int i = 0, t=0; i<source.Length; i++)
            {
                if (source[i] == null) continue;
                target[t] = source[i];
                t++;
            }
            return target;
        }
        private ushort[][][] shrinkImage(int ImageCount, ushort[][][] source)
        {
            ushort[][][] target = new ushort[ImageCount][][];
            for (int i = 0, t = 0; i < source.Length; i++)
            {
                if (source[i] == null) continue;
                target[t] = source[i];
                t++;
            }
            return target;
        }
        #endregion Image Deleter


        #region Image Spliter

        private void SplitCToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TifFileInfo fi = findFI();
            if (fi == null) return;
            
            int curC = fi.cValue;
            int CZ = fi.sizeC * fi.sizeZ;
            List<int> indexList = new List<int>();

            TifFileInfo newFI = DuplicateFI(fi);
            newFI.Dir = newFI.Dir.Substring(0, newFI.Dir.LastIndexOf(".")) + "_C" + curC + ".tif";
            newFI.sizeC = 1;
            newFI.imageCount = fi.imageCount / fi.sizeC;
            newFI.openedImages = newFI.imageCount;
            for (int i = newFI.LutList.Count-1; i >= 0; i--)
                if (i != curC)
                    newFI.LutList.RemoveAt(i);

            AddEmptyArraysToFI(newFI);
            //calculate desired image indexes
            for (int i = fi.cValue; i < fi.imageCount; i += fi.sizeC)
                indexList.Add(i);
            
            //background worker
            var bgw = new BackgroundWorker();
            bgw.WorkerReportsProgress = true;
            fi.available = false;
            //Add event for projection here
            bgw.DoWork += new DoWorkEventHandler(delegate (Object o, DoWorkEventArgs a)
            {
                //duplicate image
                DuplicateImageFragment(fi, newFI, indexList.ToArray());
                ((BackgroundWorker)o).ReportProgress(0);
            });

            bgw.ProgressChanged += new ProgressChangedEventHandler(delegate (Object o, ProgressChangedEventArgs a)
            {
                if (a.ProgressPercentage == 0)
                {
                    //add to tabpage
                    addTabPage(newFI);
                    //show on screen
                    IA.MarkAsNotSaved();
                    newFI.loaded = true;
                    newFI.original = false;
                    IA.ReloadImages();
                }
                fi.available = true;
                IA.FileBrowser.StatusLabel.Text = "Ready";
            });
            //Start background worker
            IA.FileBrowser.StatusLabel.Text = "Extracting chanel...";
            //start bgw
            bgw.RunWorkerAsync();
        }
        private void SplitZToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TifFileInfo fi = findFI();
            if (fi == null) return;

            int curZ = fi.zValue;
            int CZ = fi.sizeC * fi.sizeZ;
            List<int> indexList = new List<int>();

            TifFileInfo newFI = DuplicateFI(fi);
            newFI.Dir = newFI.Dir.Substring(0, newFI.Dir.LastIndexOf(".")) + "_Z" + curZ + ".tif";
            newFI.sizeZ = 1;
            newFI.imageCount = fi.imageCount / fi.sizeZ;
            newFI.openedImages = newFI.imageCount;
            AddEmptyArraysToFI(newFI);
            //calculate desired image indexes

            for (int i = fi.sizeC * curZ; i < fi.imageCount; i += CZ)
                for (int a = i; a < i + fi.sizeC; a++)
                    indexList.Add(a);

            //background worker
            var bgw = new BackgroundWorker();
            bgw.WorkerReportsProgress = true;
            fi.available = false;
            //Add event for projection here
            bgw.DoWork += new DoWorkEventHandler(delegate (Object o, DoWorkEventArgs a)
            {
                //duplicate image
                DuplicateImageFragment(fi, newFI, indexList.ToArray());
                ((BackgroundWorker)o).ReportProgress(0);
            });

            bgw.ProgressChanged += new ProgressChangedEventHandler(delegate (Object o, ProgressChangedEventArgs a)
            {
                if (a.ProgressPercentage == 0)
                {
                    //add to tabpage
                    addTabPage(newFI);
                    //show on screen
                    IA.MarkAsNotSaved();
                    newFI.loaded = true;
                    newFI.original = false;
                    IA.ReloadImages();
                }
                fi.available = true;
                IA.FileBrowser.StatusLabel.Text = "Ready";
            });
            //Start background worker
            IA.FileBrowser.StatusLabel.Text = "Extracting Z plane...";
            //start bgw
            bgw.RunWorkerAsync();
        }
        private void SplitTToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TifFileInfo fi = findFI();
            if (fi == null) return;

            int curT = fi.frame;
            int CZ = fi.sizeC * fi.sizeZ;
            int[] indexList = new int[CZ];

            TifFileInfo newFI = DuplicateFI(fi);
            newFI.Dir = newFI.Dir.Substring(0, newFI.Dir.LastIndexOf(".")) + "_T" + curT + ".tif";
            newFI.sizeT = 1;
            newFI.imageCount = CZ;
            newFI.openedImages = newFI.imageCount;
            AddEmptyArraysToFI(newFI);
            //calculate desired image indexes
            for (int i = curT * CZ, a = 0; i < curT * CZ + CZ; i++, a++)
                indexList[a] = i;

            //background worker
            var bgw = new BackgroundWorker();
            bgw.WorkerReportsProgress = true;
            fi.available = false;
            //Add event for projection here
            bgw.DoWork += new DoWorkEventHandler(delegate (Object o, DoWorkEventArgs a)
            {
                //duplicate image
                DuplicateImageFragment(fi, newFI, indexList);
                ((BackgroundWorker)o).ReportProgress(0);
            });

            bgw.ProgressChanged += new ProgressChangedEventHandler(delegate (Object o, ProgressChangedEventArgs a)
            {
                if (a.ProgressPercentage == 0)
                {
                    //add to tabpage
                    addTabPage(newFI);
                    //show on screen
                    IA.MarkAsNotSaved();
                    newFI.loaded = true;
                    newFI.original = false;
                    IA.ReloadImages();
                }
                fi.available = true;
                IA.FileBrowser.StatusLabel.Text = "Ready";
            });
            //Start background worker
            IA.FileBrowser.StatusLabel.Text = "Extracting T slice...";
            //start bgw
            bgw.RunWorkerAsync();
        }
        private void DuplicateImageFragment(TifFileInfo fi, TifFileInfo newFI, int[] indexList)
        {
            switch (fi.bitsPerPixel)
            {
                case 8:
                    byte[][][] image = new byte[indexList.Length][][];
                    
                    Parallel.For(0, indexList.Length, i => 
                    {
                        image[i] = DuplicateImageArray(fi.image8bit[indexList[i]]);
                    });

                    newFI.image8bit = image;
                    newFI.image8bitFilter = newFI.image8bit;

                    break;
                case 16:
                    ushort[][][] image16 = new ushort[indexList.Length][][];

                    Parallel.For(0, indexList.Length, i =>
                    {
                        image16[i] = DuplicateImageArray(fi.image16bit[indexList[i]]);
                    });

                    newFI.image16bit = image16;
                    newFI.image16bitFilter = newFI.image16bit;
                    break;
            }

        }
        private ushort[][] DuplicateImageArray(ushort[][] image)
        {
            //return (ushort[][])image.Clone();
            
            ushort[][] newImage = new ushort[image.Length][];
            for (int i = 0; i < image.Length; i++)
                newImage[i] = (ushort[])image[i].Clone();
            return newImage;
        }
        private byte[][] DuplicateImageArray(byte[][] image)
        {
            //return (byte[][])image.Clone();

            byte[][] newImage = new byte[image.Length][];
            for (int i = 0; i < image.Length; i++)
                newImage[i] = (byte[])image[i].Clone();
            return newImage;
        }
        private void addTabPage(TifFileInfo fi)
        {
            #region Create new TabPage
            //Decode File Type
            int FileTypeIndex = IA.TabPages.myFileDecoder.decodeFileType(fi.Dir);

            ///
            TabPage tp = new TabPage();
            tp.tifFI = fi;
            tp.tifFI.tpTaskbar.Initialize(IA.TabPages.ImageMainPanel, IA, fi);

            IA.TabPages.TabCollections.Add(tp);
            IA.TabPages.myFileDecoder.tabCollection = IA.TabPages.TabCollections;
            IA.TabPages.myFileDecoder.loadingTimer.Start();
            
            Panel CorePanel = tp.CorePanel;

            IA.TabPages.ImageMainPanel.BackColor = IA.FileBrowser.BackGround2Color1;
            // add CorePanel
            CorePanel.Dock = DockStyle.Fill;

            List<Control> smallCollection = new List<Control>();

            Button NameBtn = new Button();

            TreeNode node = new TreeNode();
            node.Text = IA.TabPages.FileNameFromDir(fi.Dir);
            node.Tag = fi.Dir;
            node.SelectedImageIndex = 1;
            node.ImageIndex = 1;
            NameBtn.Tag = node;

            NameBtn.Text = IA.TabPages.FileNameFromDir(fi.Dir);
            NameBtn.BackColor = IA.FileBrowser.TitlePanelColor1;
            NameBtn.FlatStyle = FlatStyle.Flat;
            NameBtn.FlatAppearance.BorderSize = 0;
            NameBtn.ForeColor = IA.FileBrowser.ShriftColor1;
            NameBtn.TextAlign = ContentAlignment.MiddleLeft;
            NameBtn.Width = TextRenderer.MeasureText(NameBtn.Text, NameBtn.Font).Width + 20;
            NameBtn.Height = 21;
            smallCollection.Add(NameBtn);
            IA.TabPages.TitlePanel.Controls.Add(NameBtn);
            NameBtn.Click += new EventHandler(IA.TabPages.SelectTabBtn_Click);
            NameBtn.BringToFront();
            NameBtn.MouseDown += new MouseEventHandler(IA.TabPages.NameBtn_MouseDown);
            NameBtn.MouseUp += new MouseEventHandler(IA.TabPages.NameBtn_MouseUp);
            NameBtn.MouseMove += new MouseEventHandler(IA.TabPages.NameBtn_MouseMove);

            Button xBtn = new Button();
            xBtn.Text = "X";
            xBtn.Font = new Font("Microsoft Sans Serif", 6, FontStyle.Bold);
            xBtn.FlatAppearance.BorderSize = 0;
            xBtn.FlatStyle = FlatStyle.Flat;
            xBtn.BackColor = IA.FileBrowser.TitlePanelColor1;
            xBtn.ForeColor = IA.FileBrowser.ShriftColor1;
            xBtn.Width = 15;
            xBtn.Height = 15;
            smallCollection.Add(xBtn);
            IA.TabPages.TitlePanel.Controls.Add(xBtn);
            xBtn.BringToFront();
            xBtn.Click += new EventHandler(IA.TabPages.DeleteTabbtn_Click);

            IA.TabPages.Collections.Add(smallCollection);

            IA.TabPages.inactivate_Tabs();
            IA.TabPages.SelectedIndex = IA.TabPages.Collections.Count - 1;
            IA.TabPages.selectTab_event(IA.TabPages.SelectedIndex);

            IA.TabPages.findStartIndex();

            #endregion Create new TabPage
        }
        private void AddEmptyArraysToFI(TifFileInfo fi)
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
        private TifFileInfo DuplicateFI(TifFileInfo fi)
        {
            TifFileInfo newFi = new TifFileInfo();

            newFi.LutList = new List<Color>();
            foreach (Color col in fi.LutList)
                newFi.LutList.Add(col);

            newFi.Dir = fi.Dir;
            newFi.seriesCount =fi.seriesCount;
            newFi.imageCount =fi.imageCount;
            newFi.sizeX =fi.sizeX;
            newFi.sizeY =fi.sizeY;
            newFi.sizeZ =fi.sizeZ;
            newFi.umZ = fi.umZ;
            newFi.umXY =fi.umXY;
            newFi.sizeC =fi.sizeC;
            newFi.sizeT =fi.sizeT;
            newFi.bitsPerPixel =fi.bitsPerPixel;
            newFi.dimensionOrder =fi.dimensionOrder;
            newFi.pixelType =fi.pixelType;
            newFi.FalseColored =fi.FalseColored;
            newFi.isIndexed =fi.isIndexed;
            newFi.MetadataComplete =fi.MetadataComplete;
            newFi.DatasetStructureDescription =fi.DatasetStructureDescription;
            
            newFi.TimeSteps =fi.TimeSteps;
            newFi.Micropoint = fi.Micropoint;
            newFi.xAxisTB = fi.xAxisTB;
            newFi.yAxisTB = fi.yAxisTB;
            //Metadata protocol info
            newFi.FileDescription =fi.FileDescription;
            newFi.xCompensation =fi.xCompensation;
            newFi.yCompensation =fi.yCompensation;
            return newFi;
        }

        #endregion Image Spliter
        private void AdjustWidthComboBox_DropDown(object sender, System.EventArgs e)
        {
            ComboBox senderComboBox = (ComboBox)sender;
            int width = senderComboBox.DropDownWidth;
            Graphics g = senderComboBox.CreateGraphics();
            Font font = senderComboBox.Font;
            int vertScrollBarWidth =
                (senderComboBox.Items.Count > senderComboBox.MaxDropDownItems)
                ? SystemInformation.VerticalScrollBarWidth : 0;

            int newWidth;
            foreach (string s in ((ComboBox)sender).Items)
            {
                newWidth = (int)g.MeasureString(s, font).Width
                    + vertScrollBarWidth;
                if (width < newWidth)
                {
                    width = newWidth;
                }
            }
            senderComboBox.DropDownWidth = width;
        }

        #region Processed as Row image
        private void ProcessedAsRawMenuItem_click(object sender, EventArgs e)
        {
            TifFileInfo fi = findFI();
            if (fi == null) return;

            TifFileInfo newFI = DuplicateFI(fi);
            

            newFI.Dir = newFI.Dir.Substring(0, newFI.Dir.LastIndexOf(".")) + "_Processed.tif";
            
            AddEmptyArraysToFI(newFI);
            newFI.sizeZ = fi.sizeZ;
            newFI.imageCount = fi.imageCount;
            newFI.openedImages = newFI.imageCount;
            //background worker
            var bgw = new BackgroundWorker();
            bgw.WorkerReportsProgress = true;
            fi.available = false;
            //Add event for projection here
            bgw.DoWork += new DoWorkEventHandler(delegate (Object o, DoWorkEventArgs a)
            {
                //duplicate image
                DuplicateProcessedImageFragment(fi, newFI);
                ((BackgroundWorker)o).ReportProgress(0);
            });

            bgw.ProgressChanged += new ProgressChangedEventHandler(delegate (Object o, ProgressChangedEventArgs a)
            {
                if (a.ProgressPercentage == 0)
                {
                    //add to tabpage
                    addTabPage(newFI);
                    //show on screen
                    IA.MarkAsNotSaved();
                    newFI.loaded = true;
                    newFI.original = false;
                    IA.ReloadImages();
                }
                fi.available = true;
                IA.FileBrowser.StatusLabel.Text = "Ready";
            });
            //Start background worker
            IA.FileBrowser.StatusLabel.Text = "Extracting Processed Image...";
            //start bgw
            bgw.RunWorkerAsync();
        }
        private void DuplicateProcessedImageFragment(TifFileInfo fi, TifFileInfo newFI)
        {
            switch (fi.bitsPerPixel)
            {
                case 8:
                    byte[][][] image = new byte[fi.imageCount][][];

                    if (fi.image8bitFilter == null) fi.image8bitFilter = fi.image8bit;

                    Parallel.For(0, fi.imageCount, i =>
                    {
                        image[i] = DuplicateImageArray(fi.image8bitFilter[i]);
                    });

                    newFI.image8bit = image;
                    newFI.image8bitFilter = newFI.image8bit;

                    break;
                case 16:
                    ushort[][][] image16 = new ushort[fi.imageCount][][];

                    if (fi.image16bitFilter == null) fi.image16bitFilter = fi.image16bit;

                    Parallel.For(0, fi.imageCount, i =>
                    {
                        image16[i] = DuplicateImageArray(fi.image16bitFilter[i]);
                    });

                    newFI.image16bit = image16;
                    newFI.image16bitFilter = newFI.image16bit;
                    break;
            }
        }
        #endregion Processed as Row image
    }
}
