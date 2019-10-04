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
    class TrackSpots
    {
        //controls
        public ImageAnalyser IA;
        Wand wand;
        private PropertiesPanel_Item PropPanel;
        public Panel panel;
        //Options
        private CTTextBox MinSizeTB;
        private CTTextBox MaxSizeTB;
        private CTTextBox SpeedTB;
        //tracking global variables
        private bool[,] shablon;
        private Color MainColor;
        //tooltip 
        private ToolTip TurnOnToolTip = new ToolTip();
        public TrackSpots(Panel propertiesPanel, Panel PropertiesBody, ImageAnalyser IA)
        {
            this.IA = IA;
            wand = new Wand(IA);

            PropPanel = new PropertiesPanel_Item();
            PropPanel_Initialize(propertiesPanel, PropertiesBody);
            //GLControl event
            IA.GLControl1.MouseDown += GLControl_MouseClick_tracking;
        }
        private void Control_MouseOver(object sender, EventArgs e)
        {
            var ctr = (Control)sender;
            TurnOnToolTip.SetToolTip(ctr, ctr.Tag.ToString());
        }
        public void BackColor(Color color)
        {
            //format background color
            PropPanel.BackColor(color);
        }
        public void ForeColor(Color color)
        {
            //format fore color
            PropPanel.ForeColor(color);
        }
        public void TitleColor(Color color)
        {
            //format title color
            PropPanel.TitleColor(color);
        }
        private void PropPanel_Initialize(Panel propertiesPanel, Panel PropertiesBody)
        {
            //PropPanel properties
            PropPanel.Initialize(propertiesPanel);
            PropPanel.Resizable = false;
            PropPanel.Name.Text = "Tracking";
            PropertiesBody.Controls.Add(PropPanel.Panel);

            panel = PropPanel.Panel;

            panel.Visible = false;

            BuildOptions();
        }
        private void BuildOptions()
        {
            MinSizeTB = CTTextBox_Add(5, 30, panel, "Min Size:",
                "Set minimum size for the tracked particles in pixels");
            MinSizeTB.label.AutoSize = true;
            MinSizeTB.Value.Changed += MinSizeTB_ValueChanged;

            MaxSizeTB = CTTextBox_Add(5, 55, panel, "Max Size:",
               "Set maximum size for the tracked particles in pixels");
            MaxSizeTB.label.AutoSize = true;
            MaxSizeTB.Value.Changed += MaxSizeTB_ValueChanged;

            SpeedTB = CTTextBox_Add(5, 80, panel, "Max Speed:",
               "Set maximum speed for the tracked particles in pixels");
            SpeedTB.label.AutoSize = true;
            SpeedTB.Value.Changed += SpeedTB_ValueChanged;

        }
        private void AddParametarsToHistory(TifFileInfo fi)
        {
            fi.History.Add("TrackingParameters(" + fi.cValue.ToString() + ","
                    + fi.tracking_MaxSize[fi.cValue].ToString() + ","
                     + fi.tracking_MinSize[fi.cValue].ToString() + ","
                      + fi.tracking_Speed[fi.cValue].ToString() + ")");
        }
        private void MaxSizeTB_ValueChanged(object sender, ChangeValueEventArgs e)
        {
            //find selected image
            TifFileInfo fi = null;
            try
            {
                fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
            }
            catch { return; }
            if (fi == null) { return; }
            //Parse value
            fi.delHist = true;
            IA.delHist = true;
            IA.UnDoBtn.Enabled = true;
            IA.DeleteFromHistory();
          
            AddParametarsToHistory(fi);
            fi.tracking_MaxSize[fi.cValue] = int.Parse(e.Value);
            AddParametarsToHistory(fi);
            //ReloadImage
            IA.UpdateUndoBtns();
            IA.MarkAsNotSaved();
            IA.ReloadImages();
        }
        private void MinSizeTB_ValueChanged(object sender, ChangeValueEventArgs e)
        {
            //find selected image
            TifFileInfo fi = null;
            try
            {
                fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
            }
            catch { return; }
            if (fi == null) { return; }
            //Parse value
            fi.delHist = true;
            IA.delHist = true;
            IA.UnDoBtn.Enabled = true;
            IA.DeleteFromHistory();

            AddParametarsToHistory(fi);
            fi.tracking_MinSize[fi.cValue] = int.Parse(e.Value);
            AddParametarsToHistory(fi);
            //ReloadImage
            IA.UpdateUndoBtns();
            IA.MarkAsNotSaved();
            IA.ReloadImages();
        }
        private void SpeedTB_ValueChanged(object sender, ChangeValueEventArgs e)
        {
            //find selected image
            TifFileInfo fi = null;
            try
            {
                fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
            }
            catch { return; }
            if (fi == null) { return; }
            //Parse value
            fi.delHist = true;
            IA.delHist = true;
            IA.UnDoBtn.Enabled = true;
            IA.DeleteFromHistory();

            AddParametarsToHistory(fi);
            fi.tracking_Speed[fi.cValue] = int.Parse(e.Value);
            AddParametarsToHistory(fi);
            //ReloadImage
            IA.UpdateUndoBtns();
            IA.MarkAsNotSaved();
            IA.ReloadImages();
        }
        private CTTextBox CTTextBox_Add(int X, int Y, Panel p, string title, string tag)
        {
            /*
           CTTextBox proba = new CTTextBox();
           proba.SetValue("0");
           proba.panel.Location = new Point(40, 40);
           gb.Controls.Add(proba.panel);
           proba.Value.Changed += new ChangedValueEventHandler(
               delegate (object o, ChangeValueEventArgs e)
               {
                   MessageBox.Show(e.Value);
               });
               */

            Label lb = new Label();
            lb.Text = title;
            lb.Tag = tag;
            lb.Width = 65;
            lb.Location = new Point(X, Y + 3);
            p.Controls.Add(lb);
            lb.BringToFront();
            lb.MouseHover += Control_MouseOver;

            CTTextBox proba = new CTTextBox();
            proba.label = lb;
            proba.SetValue("0");
            proba.panel.Location = new Point(X + lb.Width, Y);
            proba.panel.Width = 80;
            p.Controls.Add(proba.panel);
            proba.panel.BringToFront();

            return proba;
        }
        public void LoadSettings(TifFileInfo fi)
        {
            MaxSizeTB.SetValue(fi.tracking_MaxSize[fi.cValue].ToString());
            MinSizeTB.SetValue(fi.tracking_MinSize[fi.cValue].ToString());
            SpeedTB.SetValue(fi.tracking_Speed[fi.cValue].ToString());
        }
        #region Tracking program
        public void GLControl_MouseClick_tracking(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            if (IA.RoiMan.RoiType != 1)
            {
                GLControl_MouseClick_staticWand(sender, e);
                return;
            }
            //find selected image
            TifFileInfo fi = null;
            try
            {
                fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
            }
            catch { return; }
            if (fi == null) { return; }

            //Check is filter image open
            if (fi.tpTaskbar.MethodsBtnList[1].ImageIndex == 1) return;
            if (fi.selectedPictureBoxColumn != 1) return;
            //Check is Z and T correct
            if (fi.sizeT > 1 & fi.sizeZ > 1)
            {
                MessageBox.Show("Tracking in 3D is currently not avaliable!");
                return;
            }
            //calculate the point
            Point p = IsPointInImage(fi, e);

            if (p.X == -10000 | p.Y == -10000) return;

            if (fi.available == false)
            {
                MessageBox.Show("The program is bussy!");
                return;
            }

            var bgw = new BackgroundWorker();
            bgw.WorkerReportsProgress = true;

            fi.available = false;
            
            bgw.DoWork += new DoWorkEventHandler(delegate (Object o, DoWorkEventArgs a)
            {
                try
                {
                    if (IA.RoiMan.RoiShape == 0 | IA.RoiMan.RoiShape == 1)
                    {
                        //var t = Task.Run(() =>
                        TrackRectAndOvalObject(fi, fi.cValue, p);
                        //t.Wait();
                    }
                    else if(IA.RoiMan.RoiShape == 5)
                    {
                        TrackPolygonalObject(fi, fi.cValue, p);
                    }
                    else if (IA.RoiMan.RoiShape == 2 | IA.RoiMan.RoiShape == 3 | IA.RoiMan.RoiShape == 4)
                    {
                        TrackPolygonalExactObject(fi, fi.cValue, p);
                    }
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
                }

                IA.FileBrowser.StatusLabel.Text = "Ready";
                fi.available = true;
            });

            //Start background worker
            IA.FileBrowser.StatusLabel.Text = "Tracking...";
            //start bgw
            bgw.RunWorkerAsync();
        }
        public void GLControl_MouseClick_staticWand(object sender, MouseEventArgs e)
        {
            if (IA.RoiMan.RoiType != 0) return;
            //find selected image
            TifFileInfo fi = null;
            try
            {
                fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
            }
            catch { return; }
            if (fi == null) { return; }

            //Check is filter image open
            if (fi.tpTaskbar.MethodsBtnList[1].ImageIndex == 1) return;
            if (fi.selectedPictureBoxColumn != 1) return;
            
            //calculate the point
            Point p = IsPointInImage(fi, e);

            if (p.X == -10000 | p.Y == -10000) return;

            if (fi.available == false)
            {
                MessageBox.Show("The program is bussy!");
                return;
            }

            var bgw = new BackgroundWorker();
            bgw.WorkerReportsProgress = true;

            fi.available = false;
            
            bgw.DoWork += new DoWorkEventHandler(delegate (Object o, DoWorkEventArgs a)
            {
                try
                {
                    if (IA.RoiMan.RoiShape == 5)
                    {
                        TrackPolygonalObject(fi, fi.cValue, p);
                    }
                    else if (IA.RoiMan.RoiShape == 4)
                    {
                        TrackPolygonalExactObject(fi, fi.cValue, p);
                    }
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
                }

                IA.FileBrowser.StatusLabel.Text = "Ready";
                fi.available = true;
            });

            //Start background worker
            IA.FileBrowser.StatusLabel.Text = "Tracking...";
            //start bgw
            bgw.RunWorkerAsync();
        }
        private Point IsPointInImage(TifFileInfo fi, MouseEventArgs e)
        {
            double zoom = fi.zoom;
            double X1 = e.X / zoom - IA.IDrawer.valX;
            double Y1 = e.Y / zoom - IA.IDrawer.valY;
            int X = Convert.ToInt32(X1);
            if (X > X1) { X--; }
            int Y = Convert.ToInt32(Y1);
            if (Y > Y1) { Y--; }

            Point p = new Point(X, Y);

            if (IA.IDrawer.coRect[1][fi.cValue].Contains(p) == true &
                fi.tpTaskbar.MethodsBtnList[1].ImageIndex == 0 &
                fi.selectedPictureBoxColumn == 1)
            {
                p.X -= IA.IDrawer.coRect[1][fi.cValue].X;
                p.Y -= IA.IDrawer.coRect[1][fi.cValue].Y;
            }
            else if (IA.IDrawer.coRect[0][fi.cValue].Contains(p) == true &
                fi.tpTaskbar.MethodsBtnList[0].ImageIndex == 0 &
                fi.selectedPictureBoxColumn == 0)
            {
                p.X -= IA.IDrawer.coRect[0][fi.cValue].X;
                p.Y -= IA.IDrawer.coRect[0][fi.cValue].Y;
            }
            else
            {
                p.X = -10000;
                p.Y = -10000;
            }

            return p;
        }
        private void TrackRectAndOvalObject(TifFileInfo fi, int C, Point p)
        {
            //Calculate frame
            FrameCalculator FC = new FrameCalculator();
            int imageN = FC.FrameC(fi, C);
            //prepare bool shablon of the frame
            shablon = new bool[fi.sizeY, fi.sizeX];
            //prepare neighborhood shablon 
            //Point[] Matrix = new Point[8];
            Point[] Matrix = new Point[4];
            {
                /*
                Matrix[0].X = -1;
                Matrix[0].Y = -1;

                Matrix[1].X = 0;
                Matrix[1].Y = -1;

                Matrix[2].X = 1;
                Matrix[2].Y = -1;

                Matrix[3].X = 1;
                Matrix[3].Y = 0;

                Matrix[4].X = 1;
                Matrix[4].Y = 1;

                Matrix[5].X = 0;
                Matrix[5].Y = 1;

                Matrix[6].X = -1;
                Matrix[6].Y = 1;

                Matrix[7].X = -1;
                Matrix[7].Y = 0;
                */
                Matrix[0].X = -1;
                Matrix[0].Y = 0;

                Matrix[1].X = 1;
                Matrix[1].Y = 0;

                Matrix[2].X = 0;
                Matrix[2].Y = -1;

                Matrix[3].X = 0;
                Matrix[3].Y = 1;
            }
            //calculate image
            Rectangle rect = new Rectangle();
            int biggestW = imageN;
            int biggestH = imageN;
            Point[][] resList = new Point[1][];
            resList[0] = new Point[fi.imageCount];

            if (fi.bitsPerPixel == 8)
            {
                byte[][] image = fi.image8bitFilter[imageN];
                rect = (Rectangle)BordersOfObject(fi, C, imageN, image, p, Matrix, Color.Empty);
                if (rect == Rectangle.Empty)
                {
                    MessageBox.Show("Object size is out of range!");
                    return;
                }
                //track for all frames

                resList[0][imageN] = rect.Location;
                //Up
                Point LastP = rect.Location;
                Rectangle newRect;

                for (int frame = imageN + fi.sizeC; frame < fi.imageCount; frame += fi.sizeC)
                {
                    image = fi.image8bitFilter[frame];
                    newRect = (Rectangle)BordersOfObject(fi, C, frame, image, LastP, Matrix, this.MainColor, true);
                    //check for other clusters
                    if (newRect == Rectangle.Empty)
                        while (newRect == Rectangle.Empty)
                        {
                            Point p1 = findClosestPoint(LastP, fi, C);
                            if (p1 == Point.Empty) break;

                            newRect = (Rectangle)BordersOfObject(fi, C, frame, image, p1, Matrix, this.MainColor, false, true);
                        }
                    //result
                    if (newRect == Rectangle.Empty)
                        resList[0][frame] = new Point(LastP.X, LastP.Y);
                    else
                    {
                        //Width
                        if (newRect.Width > rect.Width)
                        {
                            biggestW = frame;
                            rect.Width = newRect.Width;
                        }
                        //Height
                        if (newRect.Height > rect.Height)
                        {
                            biggestH = frame;
                            rect.Height = newRect.Height;
                        }
                        //Add point
                        LastP = newRect.Location;
                        resList[0][frame] = new Point(LastP.X, LastP.Y);
                    }
                }
                //Down
                LastP = rect.Location;
                for (int frame = imageN - fi.sizeC; frame >= 0; frame -= fi.sizeC)
                {
                    image = fi.image8bitFilter[frame];
                    newRect = (Rectangle)BordersOfObject(fi, C, frame, image, LastP, Matrix, this.MainColor, true);
                    //check for other clusters
                    if (newRect == Rectangle.Empty)
                        while (newRect == Rectangle.Empty)
                        {
                            Point p1 = findClosestPoint(LastP, fi, C);
                            if (p1 == Point.Empty) break;

                            newRect = (Rectangle)BordersOfObject(fi, C, frame, image, p1, Matrix, this.MainColor, false, true);
                        }
                    //result
                    if (newRect == Rectangle.Empty)
                        resList[0][frame] = new Point(LastP.X, LastP.Y);
                    else
                    {
                        //Width
                        if (newRect.Width > rect.Width)
                        {
                            biggestW = frame;
                            rect.Width = newRect.Width;
                        }
                        //Height
                        if (newRect.Height > rect.Height)
                        {
                            biggestH = frame;
                            rect.Height = newRect.Height;
                        }
                        //Add point
                        LastP = newRect.Location;
                        resList[0][frame] = new Point(LastP.X, LastP.Y);
                    }
                }

            }
            if (fi.bitsPerPixel == 16)
            {
                ushort[][] image = fi.image16bitFilter[imageN];
                rect = (Rectangle)BordersOfObject(fi, C, imageN, image, p, Matrix, Color.Empty);
                if (rect == Rectangle.Empty)
                {
                    MessageBox.Show("Object size is out of range!");
                    return;
                }
                //track for all frames

                resList[0][imageN] = rect.Location;
                //Up
                Point LastP = rect.Location;
                Rectangle newRect;

                for (int frame = imageN + fi.sizeC; frame < fi.imageCount; frame += fi.sizeC)
                {
                    image = fi.image16bitFilter[frame];

                    newRect = (Rectangle)BordersOfObject(fi, C, frame, image, LastP, Matrix, this.MainColor, true);
                    //check for other clusters
                    if (newRect == Rectangle.Empty)
                        while (newRect == Rectangle.Empty)
                        {
                            Point p1 = findClosestPoint(LastP, fi, C);
                            if (p1 == Point.Empty) break;

                            newRect = (Rectangle)BordersOfObject(fi, C, frame, image, p1, Matrix, this.MainColor, false, true);
                        }
                    //result
                    if (newRect == Rectangle.Empty)
                        resList[0][frame] = new Point(LastP.X, LastP.Y);
                    else
                    {
                        //Width
                        if (newRect.Width > rect.Width)
                        {
                            biggestW = frame;
                            rect.Width = newRect.Width;
                        }
                        //Height
                        if (newRect.Height > rect.Height)
                        {
                            biggestH = frame;
                            rect.Height = newRect.Height;
                        }
                        //Add point
                        LastP = newRect.Location;
                        resList[0][frame] = new Point(LastP.X, LastP.Y);
                    }
                }
                //Down
                LastP = rect.Location;
                for (int frame = imageN - fi.sizeC; frame >= 0; frame -= fi.sizeC)
                {
                    image = fi.image16bitFilter[frame];
                    newRect = (Rectangle)BordersOfObject(fi, C, frame, image, LastP, Matrix, this.MainColor, true);
                    //check for other clusters
                    if (newRect == Rectangle.Empty)
                        while (newRect == Rectangle.Empty)
                        {
                            Point p1 = findClosestPoint(LastP, fi, C);
                            if (p1 == Point.Empty) break;

                            newRect = (Rectangle)BordersOfObject(fi, C, frame, image, p1, Matrix, this.MainColor, false, true);
                        }
                    //result
                    if (newRect == Rectangle.Empty)
                        resList[0][frame] = new Point(LastP.X, LastP.Y);
                    else
                    {
                        //Width
                        if (newRect.Width > rect.Width)
                        {
                            biggestW = frame;
                            rect.Width = newRect.Width;
                        }
                        //Height
                        if (newRect.Height > rect.Height)
                        {
                            biggestH = frame;
                            rect.Height = newRect.Height;
                        }
                        //Add point
                        LastP = newRect.Location;
                        resList[0][frame] = new Point(LastP.X, LastP.Y);
                    }
                }
            }

            //create new current roi
            ROI current = new ROI(fi.ROICounter, fi.imageCount,
                IA.RoiMan.RoiShape, IA.RoiMan.RoiType,
                IA.RoiMan.turnOnStackRoi);
            fi.ROICounter++;

            current.FromT = 1;
            current.FromZ = 1;
            current.ToT = fi.sizeT;
            current.ToZ = fi.sizeZ;

            if (IA.RoiMan.turnOnStackRoi == true)
            {
                current.Stack = 1;
                current.D = 3;
            }

            int W = (int)(rect.Width / 2);
            int H = (int)(rect.Height / 2);
            //prepare location list
            for (int i = 0; i < resList[0].Length; i++)
                if (resList[0][i] != null)
                {
                    resList[0][i].X -= W;
                    resList[0][i].Y -= H;
                }

            current.SetLocationAll(resList);
            current.Width = rect.Width;
            current.Height = rect.Height;
            current.biggestH = biggestH;
            current.biggestW = biggestW;
            current.returnBiggest = true;

            IA.RoiMan.current = current;
            //Clear selected roi list
            IA.RoiMan.SelectedROIsList.Clear();
        }
        private Point findClosestPoint(Point p, TifFileInfo fi, int C)
        {
            for (int i = 0; i <= fi.tracking_Speed[C]; i++)
                for (int x = p.X - i; x <= p.X + i; x++)
                    for (int y = p.Y - i; y <= p.Y + i; y++)
                        if (x >= 0 & x < fi.sizeX & y >= 0 & y < fi.sizeY)
                            if (shablon[y, x] == true)
                                return new Point(x, y);
            return Point.Empty;
        }
        private object BordersOfObject(TifFileInfo fi, int C, int imageN, byte[][] image, Point p, Point[] Matrix, Color Main, bool FindClosestPoint = false, bool scipShablon = false)
        {
            //calculate spot detector diapasone       
            int[] SpotDiapason = IA.Segmentation.SpotDet.CalculateBorders(fi, C, imageN);
            //calculate point color
            Color MainCol;
            if (Main == Color.Empty)
                MainCol = PointColor(fi, C, (int)image[p.Y][p.X], SpotDiapason, fi.LutList[C]);
            else
                MainCol = Main;
            //fill shablon
            if (scipShablon == false)
                FillShablon(MainCol, image, fi, C, SpotDiapason, fi.LutList[C]);
            //find closest point
            Point pBuf = new Point(p.X, p.Y);
            if (FindClosestPoint == true) p = findClosestPoint(p, fi, C);

            if (p == Point.Empty) return Rectangle.Empty;

            //create list with pixels
            List<int>[] PxlList = new List<int>[2];
            PxlList[0] = new List<int>();//X
            PxlList[1] = new List<int>();//Y
            //apply to shablon
            shablon[p.Y, p.X] = false;
            PxlList[0].Add(p.X);
            PxlList[1].Add(p.Y);
            //Extend
            GrowingSeed(p, new Size(fi.sizeX, fi.sizeY), PxlList, Matrix, fi.tracking_MaxSize[C]);

            if (PxlList[0].Count >= fi.tracking_MaxSize[C] |
                PxlList[0].Count <= fi.tracking_MinSize[C])
            {
                return Rectangle.Empty;
            }
            //Calculate result
            Point MidPoint = PxlList_MidPoint(PxlList);
            //check is the speed distance correct
            if (FindClosestPoint == true && fi.tracking_Speed[C] <
                Math.Sqrt(Math.Pow(MidPoint.X - pBuf.X, 2) +
                Math.Pow(MidPoint.Y - pBuf.Y, 2)))
                return Rectangle.Empty;

            Size RxAndRy = PxlList_RxAndRy(PxlList, MidPoint);

            if (IA.RoiMan.RoiShape == 0 | IA.RoiMan.RoiShape == 1)
            {
                return new Rectangle(MidPoint, RxAndRy);
            }
            else
            {
                return Rectangle.Empty;
            }
        }
        private object BordersOfObject(TifFileInfo fi, int C, int imageN, ushort[][] image, Point p, Point[] Matrix, Color Main, bool FindClosestPoint = false, bool scipShablon = false)
        {
            //calculate spot detector diapasone       
            int[] SpotDiapason = IA.Segmentation.SpotDet.CalculateBorders(fi, C, imageN);
            //calculate point color
            Color MainCol;
            if (Main == Color.Empty)
                MainCol = PointColor(fi, C, (int)image[p.Y][p.X], SpotDiapason, fi.LutList[C]);
            else
                MainCol = Main;

            //fill shablon
            if (scipShablon == false)
                FillShablon(MainCol, image, fi, C, SpotDiapason, fi.LutList[C]);
            //find closest point
            Point pBuf = new Point(p.X, p.Y);
            if (FindClosestPoint == true) p = findClosestPoint(p, fi, C);

            if (p == Point.Empty) return Rectangle.Empty;
            //create list with pixels
            List<int>[] PxlList = new List<int>[2];
            PxlList[0] = new List<int>();//X
            PxlList[1] = new List<int>();//Y
            //apply to shablon
            shablon[p.Y, p.X] = false;
            PxlList[0].Add(p.X);
            PxlList[1].Add(p.Y);
            //Extend
            GrowingSeed(p, new Size(fi.sizeX, fi.sizeY), PxlList, Matrix, fi.tracking_MaxSize[C]);

            if (PxlList[0].Count >= fi.tracking_MaxSize[C] |
                PxlList[0].Count <= fi.tracking_MinSize[C])
            {
                return Rectangle.Empty;
            }
            //Calculate result
            Point MidPoint = PxlList_MidPoint(PxlList);
            //check is the speed distance correct
            if (FindClosestPoint == true && fi.tracking_Speed[C] <
                Math.Sqrt(Math.Pow(MidPoint.X - pBuf.X, 2) +
                Math.Pow(MidPoint.Y - pBuf.Y, 2)))
                return Rectangle.Empty;

            Size RxAndRy = PxlList_RxAndRy(PxlList, MidPoint);

            if (IA.RoiMan.RoiShape == 0 | IA.RoiMan.RoiShape == 1)
            {
                return new Rectangle(MidPoint, RxAndRy);
            }
            else
            {
                return Rectangle.Empty;
            }
        }
        
        private void GrowingSeed(Point p1, Size size, List<int>[] PxlList, Point[] Matrix, long MaxSize)
        {
            int iter = 0;
            List<Point> CurPxlList = new List<Point>() { p1 };
            List<Point> NewPxlList;

            while (iter < MaxSize & CurPxlList.Count > 0)
            {
                NewPxlList = new List<Point>();
                foreach (Point p in CurPxlList)
                {
                    iter++;
                    FindNeigbors(p, size, PxlList, Matrix, NewPxlList);
                }
                CurPxlList = NewPxlList;
            }
        }
        private void FindNeigbors(Point p, Size size, List<int>[] PxlList, Point[] Matrix, List<Point> CurPxlList)
        {
            Point pNew = new Point();
            foreach (Point pM in Matrix)
            {
                pNew.X = pM.X + p.X;
                pNew.Y = pM.Y + p.Y;

                if (pNew.X >= 0 & pNew.Y >= 0
                    & pNew.X < size.Width & pNew.Y < size.Height)
                    if (shablon[pNew.Y, pNew.X] == true)
                    {
                        //apply to shablon
                        shablon[pNew.Y, pNew.X] = false;
                        PxlList[0].Add(pNew.X);
                        PxlList[1].Add(pNew.Y);
                        //Extend
                        CurPxlList.Add(pNew);
                    }
            }
        }
        private Point PxlList_MidPoint(List<int>[] PxlList)
        {
            Point MidPoint = new Point();
            MidPoint.X = (int)PxlList[0].Average()+1;
            MidPoint.Y = (int)PxlList[1].Average()+1;
            return MidPoint;
        }
        private Size PxlList_RxAndRy(List<int>[] PxlList, Point MidPoint)
        {
            int W = MidPoint.X - PxlList[0].Min();
            int W1 = PxlList[0].Max() - MidPoint.X;
            if (W1 > W) W = W1;

            int H = MidPoint.Y - PxlList[1].Min();
            int H1 = PxlList[1].Max() - MidPoint.Y;
            if (H1 > H) H = H1;
            
            Size size = new Size(W + W, H + H);

            return size;
        }

        private void FillShablon(Color MainColor, ushort[][] image, TifFileInfo fi, int C, int[] SpotDiapason, Color LutCol)
        {
            for (int y = 0; y < fi.sizeY; y++)
                for (int x = 0; x < fi.sizeX; x++)
                {
                    int val = (int)image[y][x];

                    Color col;
                    //Coordinates
                    int Choise = fi.thresholds[C];
                    if (fi.SegmentationCBoxIndex[C] == 0) Choise = 0;
                    #region Colors
                    if (fi.SegmentationCBoxIndex[C] != 0 | fi.SelectedSpotThresh[C] != 0)
                    {
                        switch (Choise)
                        {
                            case 0:
                                if (val > SpotDiapason[0] & val < SpotDiapason[1])
                                    col = fi.SpotColor[C];
                                else
                                    col = Color.FromArgb((int)(fi.adjustedLUT[C][val] * 255), LutCol.R, LutCol.G, LutCol.B);
                                break;
                            default:
                                if (val > SpotDiapason[0] & val < SpotDiapason[1])
                                    col = fi.SpotColor[C];
                                else if (val < fi.thresholdValues[C][1])
                                    col = fi.thresholdColors[C][0];
                                else if (val < fi.thresholdValues[C][2])
                                    col = fi.thresholdColors[C][1];
                                else if (val < fi.thresholdValues[C][3])
                                    col = fi.thresholdColors[C][2];
                                else if (val < fi.thresholdValues[C][4])
                                    col = fi.thresholdColors[C][3];
                                else
                                    col = fi.thresholdColors[C][fi.thresholds[C]];

                                if (col == Color.Transparent)
                                    col = Color.FromArgb((int)(fi.adjustedLUT[C][val] * 255), LutCol.R, LutCol.G, LutCol.B);

                                break;
                        }
                    }
                    else
                        col = Color.FromArgb((int)(fi.adjustedLUT[C][val] * 255), LutCol.R, LutCol.G, LutCol.B);
                    #endregion Colors
                    if (col == MainColor)
                        shablon[y, x] = true;
                    else
                        shablon[y, x] = false;
                }
        }
        private void FillShablon(Color MainColor, byte[][] image, TifFileInfo fi, int C, int[] SpotDiapason, Color LutCol)
        {

            for (int y = 0; y < fi.sizeY; y++)
                for (int x = 0; x < fi.sizeX; x++)
                {
                    int val = (int)image[y][x];

                    Color col;
                    //Coordinates
                    int Choise = fi.thresholds[C];
                    if (fi.SegmentationCBoxIndex[C] == 0) Choise = 0;
                    #region Colors
                    if (fi.SegmentationCBoxIndex[C] != 0 | fi.SelectedSpotThresh[C] != 0)
                    {
                        switch (Choise)
                        {
                            case 0:
                                if (val > SpotDiapason[0] & val < SpotDiapason[1])
                                    col = fi.SpotColor[C];
                                else
                                    col = Color.FromArgb((int)(fi.adjustedLUT[C][val] * 255), LutCol.R, LutCol.G, LutCol.B);
                                break;
                            default:
                                if (val > SpotDiapason[0] & val < SpotDiapason[1])
                                    col = fi.SpotColor[C];
                                else if (val < fi.thresholdValues[C][1])
                                    col = fi.thresholdColors[C][0];
                                else if (val < fi.thresholdValues[C][2])
                                    col = fi.thresholdColors[C][1];
                                else if (val < fi.thresholdValues[C][3])
                                    col = fi.thresholdColors[C][2];
                                else if (val < fi.thresholdValues[C][4])
                                    col = fi.thresholdColors[C][3];
                                else
                                    col = fi.thresholdColors[C][fi.thresholds[C]];

                                if (col == Color.Transparent)
                                    col = Color.FromArgb((int)(fi.adjustedLUT[C][val] * 255), LutCol.R, LutCol.G, LutCol.B);

                                break;
                        }
                    }
                    else
                        col = Color.FromArgb((int)(fi.adjustedLUT[C][val] * 255), LutCol.R, LutCol.G, LutCol.B);
                    #endregion Colors
                    if (col == MainColor)
                        shablon[y, x] = true;
                    else
                        shablon[y, x] = false;
                }
        }
        private Color PointColor(TifFileInfo fi, int C, int val, int[] SpotDiapason, Color LutCol)
        {
            Color col;
            //Coordinates
            int Choise = fi.thresholds[C];
            if (fi.SegmentationCBoxIndex[C] == 0) Choise = 0;
            #region Colors
            if (fi.SegmentationCBoxIndex[C] != 0 | fi.SelectedSpotThresh[C] != 0)
            {
                switch (Choise)
                {
                    case 0:
                        if (val > SpotDiapason[0] & val < SpotDiapason[1])
                            col = fi.SpotColor[C];
                        else
                            col = Color.FromArgb((int)(fi.adjustedLUT[C][val] * 255), LutCol.R, LutCol.G, LutCol.B);
                        break;
                    default:
                        if (val > SpotDiapason[0] & val < SpotDiapason[1])
                            col = fi.SpotColor[C];
                        else if (val < fi.thresholdValues[C][1])
                            col = fi.thresholdColors[C][0];
                        else if (val < fi.thresholdValues[C][2])
                            col = fi.thresholdColors[C][1];
                        else if (val < fi.thresholdValues[C][3])
                            col = fi.thresholdColors[C][2];
                        else if (val < fi.thresholdValues[C][4])
                            col = fi.thresholdColors[C][3];
                        else
                            col = fi.thresholdColors[C][fi.thresholds[C]];

                        if (col == Color.Transparent)
                            col = Color.FromArgb((int)(fi.adjustedLUT[C][val] * 255), LutCol.R, LutCol.G, LutCol.B);

                        break;
                }
            }
            else
                col = Color.FromArgb((int)(fi.adjustedLUT[C][val] * 255), LutCol.R, LutCol.G, LutCol.B);
            #endregion Colors
            this.MainColor = col;
            return col;
        }

        #endregion Tracking program
        #region Magic Wand
        private Point[] DuplicateArray(Point[] source)
        {
            Point[] res = new Point[source.Length];
            for (int i = 0; i < source.Length; i++)
            {
                res[i].X = source[i].X;
                res[i].Y = source[i].Y;
            }
            return res; 
        }
        private void TrackPolygonalExactObject(TifFileInfo fi, int C, Point p)
        {
            //Calculate frame
            FrameCalculator FC = new FrameCalculator();
            int imageN = FC.FrameC(fi, C);
            //prepare bool shablon of the frame
            shablon = new bool[fi.sizeY, fi.sizeX];
            
            //calculate image
            Point[] temp;
            Point[] lastTemp;
            Point[][] resList;
            
            if (IA.RoiMan.RoiType == 0)
                resList = new Point[1][];
            else
                resList = new Point[fi.imageCount][];
            
            if (fi.bitsPerPixel == 8)
            {
                byte[][] image = fi.image8bitFilter[imageN];
                temp = ((List<Point>)BordersOfObjectExactPolygon(fi, C, imageN, image, p, Color.Empty)).ToArray();
                if (temp.Length == 0)
                {
                    MessageBox.Show("Object size is out of range!");
                    return;
                }
                Point MidP = PolygonMP(temp, fi.tracking_MaxSize[fi.cValue], fi.tracking_MinSize[fi.cValue]);
                if (MidP == Point.Empty)
                {
                    MessageBox.Show("Object size is out of range!");
                    return;
                }

                if (IA.RoiMan.IsPointInPolygon(MidP, temp) == false) MidP = p;

                Point OriginalMidP = new Point(MidP.X, MidP.Y);
                lastTemp = temp;

                if (IA.RoiMan.RoiType == 0)
                    resList[0] = temp.ToArray();
                else
                {
                    resList[imageN] = temp.ToArray();
                    //tracking part
                    //Up
                    Point LastP = MidP;
                    for (int frame = imageN + fi.sizeC; frame < fi.imageCount; frame += fi.sizeC)
                    {
                        image = fi.image8bitFilter[frame];
                        temp = ((List<Point>)BordersOfObjectExactPolygon(fi, C, imageN, image, LastP, this.MainColor,true)).ToArray();
                        //check for other clusters
                        MidP = PolygonMP(temp, fi.tracking_MaxSize[fi.cValue], fi.tracking_MinSize[fi.cValue],
                            fi.tracking_Speed[fi.cValue], LastP);

                        if (MidP == Point.Empty)
                        {
                            Point p1 = LastP;
                            while (temp.Length != 0 && MidP == Point.Empty)
                            {
                                CleanShablonFromObject(p1);
                                p1 = findClosestPoint(LastP, fi, C);
                                if (p1 == Point.Empty) break;
                                temp = ((List<Point>)BordersOfObjectExactPolygon(fi, C, imageN, image, p1, this.MainColor, false, true)).ToArray();
                                MidP = PolygonMP(temp, fi.tracking_MaxSize[fi.cValue], fi.tracking_MinSize[fi.cValue],
                                    fi.tracking_Speed[fi.cValue],LastP);
                            }
                        }
                        //result
                        
                        if (temp.Length <= 2 || MidP == Point.Empty)
                        {
                            resList[frame] = DuplicateArray(lastTemp);
                        }
                        else
                        {
                            //Add point
                            if (MidP != Point.Empty && IA.RoiMan.IsPointInPolygon(MidP, temp))
                                LastP = MidP;

                            resList[frame] = temp;
                            lastTemp = temp;
                        }
                    }
                    //Down
                    LastP = OriginalMidP;
                    lastTemp = resList[imageN];

                    for (int frame = imageN - fi.sizeC; frame >= 0; frame -= fi.sizeC)
                    {
                        image = fi.image8bitFilter[frame];
                        temp = ((List<Point>)BordersOfObjectExactPolygon(fi, C, imageN, image, LastP, this.MainColor, true)).ToArray();
                        //check for other clusters
                        MidP = PolygonMP(temp, fi.tracking_MaxSize[fi.cValue], fi.tracking_MinSize[fi.cValue],
                            fi.tracking_Speed[fi.cValue], LastP);
                        if (MidP == Point.Empty)
                        {
                            Point p1 = LastP;
                            while (temp.Length != 0 && MidP == Point.Empty)
                            {
                                CleanShablonFromObject(p1);
                                p1 = findClosestPoint(LastP, fi, C);
                                if (p1 == Point.Empty) break;
                                temp = ((List<Point>)BordersOfObjectExactPolygon(fi, C, imageN, image, p1, this.MainColor, false, true)).ToArray();
                                MidP = PolygonMP(temp, fi.tracking_MaxSize[fi.cValue], fi.tracking_MinSize[fi.cValue],
                                    fi.tracking_Speed[fi.cValue], LastP);
                            }
                        }
                        //result

                        if (temp.Length <= 2 || MidP == Point.Empty)
                        {
                            resList[frame] = DuplicateArray(lastTemp);
                        }
                        else
                        {
                            //Add point
                            if (MidP != Point.Empty && IA.RoiMan.IsPointInPolygon(MidP, temp))
                                LastP = MidP;

                            resList[frame] = temp;
                            lastTemp = temp;
                        }
                    }
                }
            }
            if (fi.bitsPerPixel == 16)
            {
                ushort[][] image = fi.image16bitFilter[imageN];
                temp = ((List<Point>)BordersOfObjectExactPolygon(fi, C, imageN, image, p, Color.Empty)).ToArray();
                if (temp.Length == 0)
                {
                    MessageBox.Show("Object size is out of range!");
                    return;
                }
                Point MidP = PolygonMP(temp, fi.tracking_MaxSize[fi.cValue], fi.tracking_MinSize[fi.cValue]);
                if (MidP == Point.Empty)
                {
                    MessageBox.Show("Object size is out of range!");
                    return;
                }

                Point OriginalMidP = new Point(MidP.X, MidP.Y);
                lastTemp = temp;

                if (IA.RoiMan.RoiType == 0)
                    resList[0] = temp.ToArray();
                else
                {
                    resList[imageN] = temp.ToArray();
                    //tracking part
                    //Up
                    Point LastP = MidP;
                    for (int frame = imageN + fi.sizeC; frame < fi.imageCount; frame += fi.sizeC)
                    {
                        image = fi.image16bitFilter[frame];
                        temp = ((List<Point>)BordersOfObjectExactPolygon(fi, C, imageN, image, LastP, this.MainColor, true)).ToArray();
                        //check for other clusters
                        MidP = PolygonMP(temp, fi.tracking_MaxSize[fi.cValue], fi.tracking_MinSize[fi.cValue],
                            fi.tracking_Speed[fi.cValue], LastP);

                        if (MidP == Point.Empty)
                        {
                            Point p1 = LastP;
                            while (temp.Length != 0 && MidP == Point.Empty)
                            {
                                CleanShablonFromObject(p1);
                                p1 = findClosestPoint(LastP, fi, C);
                                if (p1 == Point.Empty) break;
                                temp = ((List<Point>)BordersOfObjectExactPolygon(fi, C, imageN, image, p1, this.MainColor, false, true)).ToArray();
                                MidP = PolygonMP(temp, fi.tracking_MaxSize[fi.cValue], fi.tracking_MinSize[fi.cValue],
                                    fi.tracking_Speed[fi.cValue], LastP);
                            }
                        }
                        //result

                        if (temp.Length <= 2 || MidP == Point.Empty)
                        {
                            resList[frame] = DuplicateArray(lastTemp);
                        }
                        else
                        {
                            //Add point
                            if (MidP != Point.Empty && IA.RoiMan.IsPointInPolygon(MidP, temp))
                                LastP = MidP;

                            resList[frame] = temp;
                            lastTemp = temp;
                        }
                    }
                    //Down
                    LastP = OriginalMidP;
                    lastTemp = resList[imageN];

                    for (int frame = imageN - fi.sizeC; frame >= 0; frame -= fi.sizeC)
                    {
                        image = fi.image16bitFilter[frame];
                        temp = ((List<Point>)BordersOfObjectExactPolygon(fi, C, imageN, image, LastP, this.MainColor, true)).ToArray();
                        //check for other clusters
                        MidP = PolygonMP(temp, fi.tracking_MaxSize[fi.cValue], fi.tracking_MinSize[fi.cValue],
                            fi.tracking_Speed[fi.cValue], LastP);
                        if (MidP == Point.Empty)
                        {
                            Point p1 = LastP;
                            while (temp.Length != 0 && MidP == Point.Empty)
                            {
                                CleanShablonFromObject(p1);
                                p1 = findClosestPoint(LastP, fi, C);
                                if (p1 == Point.Empty) break;
                                temp = ((List<Point>)BordersOfObjectExactPolygon(fi, C, imageN, image, p1, this.MainColor, false, true)).ToArray();
                                MidP = PolygonMP(temp, fi.tracking_MaxSize[fi.cValue], fi.tracking_MinSize[fi.cValue],
                                    fi.tracking_Speed[fi.cValue], LastP);
                            }
                        }
                        //result

                        if (temp.Length <= 2 || MidP == Point.Empty)
                        {
                            resList[frame] = DuplicateArray(lastTemp);
                        }
                        else
                        {
                            //Add point
                            if (MidP != Point.Empty && IA.RoiMan.IsPointInPolygon(MidP, temp))
                                LastP = MidP;

                            resList[frame] = temp;
                            lastTemp = temp;
                        }
                    }
                }
            }
            //create new current roi
            ROI current = new ROI(fi.ROICounter, fi.imageCount,
            3, IA.RoiMan.RoiType,
            IA.RoiMan.turnOnStackRoi);
            fi.ROICounter++;

            current.FromT = 1;
            current.FromZ = 1;
            current.ToT = fi.sizeT;
            current.ToZ = fi.sizeZ;

            if (IA.RoiMan.turnOnStackRoi == true)
            {
                current.Stack = 1;
                current.D = 3;
            }

            //prepare location list

            current.SetLocationAll(resList);

            IA.RoiMan.current = current;
            //Clear selected roi list
            IA.RoiMan.SelectedROIsList.Clear();
        }
        private void TrackPolygonalObject(TifFileInfo fi, int C, Point p)
        {
            //Calculate frame
            FrameCalculator FC = new FrameCalculator();
            int imageN = FC.FrameC(fi, C);
            //prepare bool shablon of the frame
            shablon = new bool[fi.sizeY, fi.sizeX];
            //prepare neighborhood shablon 
            Point[] Matrix = new Point[4];
            {
                Matrix[0].X = -1;
                Matrix[0].Y = 0;

                Matrix[1].X = 1;
                Matrix[1].Y = 0;

                Matrix[2].X = 0;
                Matrix[2].Y = -1;

                Matrix[3].X = 0;
                Matrix[3].Y = 1;
            }
            //calculate image
            List<Point> temp = new List<Point>();
            Point[][] resList;
            if (IA.RoiMan.RoiType == 0)
                resList = new Point[1][];
            else
                resList = new Point[fi.imageCount][];

            if (fi.bitsPerPixel == 8)
            {
                byte[][] image = fi.image8bitFilter[imageN];
                temp = (List<Point>)BordersOfObjectPolygon(fi, C, imageN, image, p, Matrix, Color.Empty);
                if (temp.Count == 0)
                {
                    MessageBox.Show("Object size is out of range!");
                    return;
                }
                
                if (IA.RoiMan.RoiType == 0)
                    resList[0] = ConvexHull.MakeConvexHull(temp).ToArray();
                else
                {
                    resList[imageN] = ConvexHull.MakeConvexHull(temp).ToArray();
                    //tracking part

                }
            }
            if (fi.bitsPerPixel == 16)
            {
                ushort[][] image = fi.image16bitFilter[imageN];
                temp = (List<Point>)BordersOfObjectPolygon(fi, C, imageN, image, p, Matrix, Color.Empty);
                if (temp.Count == 0)
                {
                    MessageBox.Show("Object size is out of range!");
                    return;
                }

                if (IA.RoiMan.RoiType == 0)
                    resList[0] = ConvexHull.MakeConvexHull(temp).ToArray();
                else
                {
                    resList[imageN] = ConvexHull.MakeConvexHull(temp).ToArray();
                    //tracking part

                }
            }
            //create new current roi
            ROI current = new ROI(fi.ROICounter, fi.imageCount,
                3, IA.RoiMan.RoiType,
            IA.RoiMan.turnOnStackRoi);
            fi.ROICounter++;

            current.FromT = 1;
            current.FromZ = 1;
            current.ToT = fi.sizeT;
            current.ToZ = fi.sizeZ;

            if (IA.RoiMan.turnOnStackRoi == true)
            {
                current.Stack = 1;
                current.D = 3;
            }
            //prepare location list

            current.SetLocationAll(resList);
            
            IA.RoiMan.current = current;
            //Clear selected roi list
            IA.RoiMan.SelectedROIsList.Clear();
        }
        private object BordersOfObjectExactPolygon(TifFileInfo fi, int C, int imageN, byte[][] image, Point p, Color Main, bool FindClosestPoint = false, bool scipShablon = false)
        {
            //calculate spot detector diapasone       
            int[] SpotDiapason = IA.Segmentation.SpotDet.CalculateBorders(fi, C, imageN);
            //calculate point color
            Color MainCol;
            if (Main == Color.Empty)
                MainCol = PointColor(fi, C, (int)image[p.Y][p.X], SpotDiapason, fi.LutList[C]);
            else
                MainCol = Main;
            //fill shablon
            if (scipShablon == false)
                FillShablon(MainCol, image, fi, C, SpotDiapason, fi.LutList[C]);
            //find closest point
            Point pBuf = new Point(p.X, p.Y);
            if (FindClosestPoint == true) p = findClosestPoint(p, fi, C);

            if (p == Point.Empty) return new List<Point>();

            List<Point> PxlList =  wand.autoOutline(p.X, p.Y, shablon);
            
            return PxlList;
        }
        private object BordersOfObjectPolygon(TifFileInfo fi, int C, int imageN, byte[][] image, Point p, Point[] Matrix, Color Main, bool FindClosestPoint = false, bool scipShablon = false)
        {
            //calculate spot detector diapasone       
            int[] SpotDiapason = IA.Segmentation.SpotDet.CalculateBorders(fi, C, imageN);
            //calculate point color
            Color MainCol;
            if (Main == Color.Empty)
                MainCol = PointColor(fi, C, (int)image[p.Y][p.X], SpotDiapason, fi.LutList[C]);
            else
                MainCol = Main;
            //fill shablon
            if (scipShablon == false)
                FillShablon(MainCol, image, fi, C, SpotDiapason, fi.LutList[C]);
            //find closest point
            Point pBuf = new Point(p.X, p.Y);
            if (FindClosestPoint == true) p = findClosestPoint(p, fi, C);

            if (p == Point.Empty) return new List<Point>();
           
            //create list with pixels
            List<int>[] PxlList = new List<int>[2];
            PxlList[0] = new List<int>();//X
            PxlList[1] = new List<int>();//Y
            //apply to shablon
            shablon[p.Y, p.X] = false;
            PxlList[0].Add(p.X);
            PxlList[1].Add(p.Y);
            //Extend
            GrowingSeed(p, new Size(fi.sizeX, fi.sizeY), PxlList, Matrix, fi.tracking_MaxSize[C]);
            
            if (PxlList[0].Count >= fi.tracking_MaxSize[C] |
                PxlList[0].Count <= fi.tracking_MinSize[C])
            {
                return new List<Point>();
            }
            //Calculate result
            List<Point> newPxlList = new List<Point>();
            for(int i = 0; i < PxlList[0].Count; i++)
            {
                newPxlList.Add(new Point(PxlList[0][i], PxlList[1][i]));
            }
            
            return newPxlList;
        }
        private object BordersOfObjectExactPolygon(TifFileInfo fi, int C, int imageN, ushort[][] image, Point p, Color Main, bool FindClosestPoint = false, bool scipShablon = false)
        {
            //calculate spot detector diapasone       
            int[] SpotDiapason = IA.Segmentation.SpotDet.CalculateBorders(fi, C, imageN);
            //calculate point color
            Color MainCol;
            if (Main == Color.Empty)
                MainCol = PointColor(fi, C, (int)image[p.Y][p.X], SpotDiapason, fi.LutList[C]);
            else
                MainCol = Main;
            //fill shablon
            if (scipShablon == false)
                FillShablon(MainCol, image, fi, C, SpotDiapason, fi.LutList[C]);
            //find closest point
            Point pBuf = new Point(p.X, p.Y);
            if (FindClosestPoint == true) p = findClosestPoint(p, fi, C);

            if (p == Point.Empty) return new List<Point>();

            List<Point> PxlList = wand.autoOutline(p.X, p.Y, shablon);

            return PxlList;
        }
        private object BordersOfObjectPolygon(TifFileInfo fi, int C, int imageN, ushort[][] image, Point p, Point[] Matrix, Color Main, bool FindClosestPoint = false, bool scipShablon = false)
        {
            //calculate spot detector diapasone       
            int[] SpotDiapason = IA.Segmentation.SpotDet.CalculateBorders(fi, C, imageN);
            //calculate point color
            Color MainCol;
            if (Main == Color.Empty)
                MainCol = PointColor(fi, C, (int)image[p.Y][p.X], SpotDiapason, fi.LutList[C]);
            else
                MainCol = Main;
            //fill shablon
            if (scipShablon == false)
                FillShablon(MainCol, image, fi, C, SpotDiapason, fi.LutList[C]);
            //find closest point
            Point pBuf = new Point(p.X, p.Y);
            if (FindClosestPoint == true) p = findClosestPoint(p, fi, C);

            if (p == Point.Empty) return new List<Point>();

            //create list with pixels
            List<int>[] PxlList = new List<int>[2];
            PxlList[0] = new List<int>();//X
            PxlList[1] = new List<int>();//Y
            //apply to shablon
            shablon[p.Y, p.X] = false;
            PxlList[0].Add(p.X);
            PxlList[1].Add(p.Y);
            //Extend
            GrowingSeed(p, new Size(fi.sizeX, fi.sizeY), PxlList, Matrix, fi.tracking_MaxSize[C]);

            if (PxlList[0].Count >= fi.tracking_MaxSize[C] |
                PxlList[0].Count <= fi.tracking_MinSize[C])
            {
                return new List<Point>();
            }
            //Calculate result
            List<Point> newPxlList = new List<Point>();
            for (int i = 0; i < PxlList[0].Count; i++)
            {
                newPxlList.Add(new Point(PxlList[0][i], PxlList[1][i]));
            }

            return newPxlList;
        }
        private Point PolygonMP(Point[] PxlList, int MaxSize, int MinSize, int TrackingSpeed=0, Point LastP = new Point())
        {
            if (PxlList.Length == 0) return Point.Empty;

            int Polygoncorners = PxlList.Count();
            double avrX = 0;
            double avrY = 0;
            
            foreach (Point p in PxlList)
            {
                avrX += p.X;
                avrY += p.Y;
            }
            //count polygon
            double count = PolygonArea(PxlList);
           
            //results
            if (count <= MinSize | count >= MaxSize) return Point.Empty;
            
            avrX /= Polygoncorners;
            avrY /= Polygoncorners;

            Point MidP = new Point((int)avrX, (int)avrY);
            if (LastP != new Point())
                if (IA.RoiMan.IsPointInPolygon(MidP, PxlList))
                {
                    if (Math.Pow(TrackingSpeed, 2) < (Math.Pow(MidP.X - LastP.X, 2) +
                        Math.Pow(MidP.Y - LastP.Y, 2)))
                        MidP = Point.Empty;
                }
                else
                {
                    MidP = LastP;
                }

            return MidP;
        }
        public static double PolygonArea(Point[] polygon)
        {
            int Size = polygon.Length;
            if (Size < 3) return 0;

            Point first = polygon[0];
            Point last = first;

            double area = 0;
            
            foreach(Point p in polygon)
            {
                Point next = p;
                area += next.X * last.Y - last.X * next.Y;
                last = next;
            }
            area += first.X * last.Y - last.X * first.Y;

            return Math.Abs(area / 2);
        }
        private void CleanShablonFromObject(Point p1)
        {
            shablon[p1.Y, p1.X] = false;
            List<Point> CurPxlList = new List<Point>() { p1 };
            List<Point> NewPxlList;
            Point[] Matrix = new Point[4];
            {
                Matrix[0].X = -1;
                Matrix[0].Y = 0;

                Matrix[1].X = 1;
                Matrix[1].Y = 0;

                Matrix[2].X = 0;
                Matrix[2].Y = -1;

                Matrix[3].X = 0;
                Matrix[3].Y = 1;
            }
            Size size = new Size(shablon.GetLength(1), shablon.GetLength(0));

            while (CurPxlList.Count > 0)
            {
                NewPxlList = new List<Point>();
                foreach (Point p in CurPxlList)
                {
                    ClearNeigbors(p, Matrix, NewPxlList, size);
                }
                CurPxlList = NewPxlList;
            }
        }
        private void ClearNeigbors(Point p, Point[] Matrix, List<Point> CurPxlList, Size size)
        {
            Point pNew = new Point();
            foreach (Point pM in Matrix)
            {
                pNew.X = pM.X + p.X;
                pNew.Y = pM.Y + p.Y;

                if (pNew.X >= 0 && pNew.Y >= 0
                    && pNew.X < size.Width && pNew.Y < size.Height &&
                    shablon[pNew.Y, pNew.X] == true)
                    {
                        //apply to shablon
                        shablon[pNew.Y, pNew.X] = false;
                        //Extend
                        CurPxlList.Add(pNew);
                    }
            }
        }
        #endregion Magic Wand
    }
}
