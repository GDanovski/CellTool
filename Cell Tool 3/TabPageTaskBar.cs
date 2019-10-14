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

using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace Cell_Tool_3
{
    
    public class PanelUnscrollable : Panel
    {
        public ToolStripComboBox zoomValue;
        public bool scrolling = false;
       public void AddEvents()
        {
            
            for (int i = 0; i < Controls.Count; i++)
            {
                Controls[i].MouseDown += Item_MouseDown;
                Controls[i].MouseUp += Item_MouseUp;
                Controls[i].MouseMove += Item_MouseMove;
                Controls[i].MouseHover += Item_MouseHover;
            }
       }
        void Item_MouseHover(object sender, EventArgs e)
        {
            if (base.Focused == false)
            {
                base.Focus();
            }
        }
        void Item_MouseDown(object sender, MouseEventArgs e)
        {
            
            if (e.Button == MouseButtons.Middle)
            {
                GLControl pb = (GLControl)sender;
                int xTrans = e.X + pb.Location.X;
                int yTrans = e.Y + pb.Location.Y;
                MouseEventArgs eTrans = new MouseEventArgs(
                    e.Button, e.Clicks, xTrans,yTrans, e.Delta);
                base.OnMouseDown(eTrans);
                scrolling = true;
            }
       }
        void Item_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle)
            {
                GLControl pb = (GLControl)sender;
                int xTrans = e.X + pb.Location.X;
                int yTrans = e.Y + pb.Location.Y;
                MouseEventArgs eTrans = new MouseEventArgs(
                    e.Button, e.Clicks, xTrans,yTrans, e.Delta);
                base.OnMouseUp(eTrans);
                scrolling = false;
            }
        }
        void Item_MouseMove(object sender, MouseEventArgs e)
        {
            GLControl pb = (GLControl)sender;
            if (e.Button == MouseButtons.Middle & scrolling == true)
            {
                
                int xTrans = e.X + pb.Location.X;
                int yTrans = e.Y + pb.Location.Y;
                MouseEventArgs eTrans = new MouseEventArgs(
                    e.Button, e.Clicks, xTrans, yTrans, e.Delta);
               
                base.OnMouseMove(eTrans);
            }
            else
            {
                this.OnMouseMove(e);
            }
        }
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (ModifierKeys == Keys.Control)
            {
                //base.OnMouseWheel(e);
                if (e.Delta < 0 & zoomValue.SelectedIndex > 1)
                {
                    zoomValue.SelectedIndex -= 1;
                }
                else if (e.Delta > 0 & zoomValue.SelectedIndex < zoomValue.Items.Count - 1)
                {
                    zoomValue.SelectedIndex += 1;
                }
            }
            else if(ModifierKeys == Keys.Shift & base.HorizontalScroll.Visible == true & base.HorizontalScroll.Enabled == true)
            {
                //base.OnMouseWheel(e);
                if (e.Delta > 0 & base.HorizontalScroll.Maximum >= base.HorizontalScroll.Value + base.HorizontalScroll.SmallChange*2 )
                {
                    base.HorizontalScroll.Value += base.HorizontalScroll.SmallChange*2; 
                }
                else if (e.Delta < 0 & base.HorizontalScroll.Minimum <= base.HorizontalScroll.Value - base.HorizontalScroll.SmallChange*2 )
                {
                    base.HorizontalScroll.Value -= base.HorizontalScroll.SmallChange*2;
                }
            }
            else
            {
                base.OnMouseWheel(e);
            }
        }
    }
    class TabPageTaskBar
    {
        public TifFileInfo tifFI;
        public ImageAnalyser IA;

        public Panel TopBar = new Panel();
        private Panel ColorPanel = new Panel();
        private Panel MethodsPanel = new Panel();
        public Panel XYValPanel = new Panel();

        public List<Button> ColorBtnList = new List<Button>();
        public List<Button> MethodsBtnList = new List<Button>();

        private ToolTip TurnOnToolTip = new ToolTip();

        public Label ValLabel = new Label();
        public Label YLabel = new Label();
        public Label XLabel = new Label();

        public void Initialize(Panel MainPanel, ImageAnalyser IA1, TifFileInfo tifFI1)
        {
            tifFI = tifFI1;
            IA = IA1;

            int w = 30;
            int h = 30;

            TopBar.Width = MainPanel.Width - w;
            TopBar.Height = h;
            TopBar.Dock = DockStyle.Top;
            //MainPanel.Controls.Add(TopBar);
            IA.IDrawer.corePanel.Height = TopBar.Height;
            IA.IDrawer.corePanel.Controls.Add(TopBar);
            TopBar.BringToFront();

            ColorPanel.Dock = DockStyle.Right;
            ColorPanel.BackColor = IA.FileBrowser.BackGround2Color1;
            TopBar.Controls.Add(ColorPanel);
            ColorPanel.BringToFront();

            XYValPanel.Controls.Add(XLabel);
            XLabel.Font = new Font(XLabel.Font.Name, 6.9F);
            XLabel.Location = new Point(0, 15);
            XLabel.Text = "X: 10000";
            XLabel.Visible = false;
            XLabel.Width = 45;

            XYValPanel.Controls.Add(YLabel);
            YLabel.Font = new Font(XLabel.Font.Name, 6.9F);
            YLabel.Location = new Point(47, 15);
            YLabel.Text = "Y: 10000";
            YLabel.Visible = false;
            YLabel.Width = 45;

            XYValPanel.Controls.Add(ValLabel);
            ValLabel.Font = new Font(XLabel.Font.Name, 6.9F);
            ValLabel.Location = new Point(94, 15);
            ValLabel.Text = "Value: 10000";
            ValLabel.Width = 70;
            ValLabel.Visible = false;

            XYValPanel.Dock = DockStyle.Right;
            XYValPanel.Width = 165;
            XYValPanel.BackColor = IA.FileBrowser.BackGroundColor1;
            TopBar.Controls.Add(XYValPanel);
            XYValPanel.BringToFront();

            Panel SmallColorPanel = new Panel();
            SmallColorPanel.BackColor = IA.FileBrowser.BackGroundColor1;
            SmallColorPanel.Dock = DockStyle.Top;
            SmallColorPanel.Height = 5;
            ColorPanel.Controls.Add(SmallColorPanel);

            MethodsPanel.Dock = DockStyle.Left;
            MethodsPanel.BackColor = IA.FileBrowser.BackGroundColor1;
            MethodsPanel.Width = 100;
            TopBar.Controls.Add(MethodsPanel);
            MethodsPanel.BringToFront();

            Refresh();

            VisualizeColorBtns();
            VisualizeMethodsBtns();

            
            this.TopBar.Invalidate();
            this.TopBar.Update();
            this.TopBar.Refresh();
            Application.DoEvents();

        }

        private void Control_MouseDown(object sender, MouseEventArgs e)
        {

            Button ctr = (Button)sender;
            int i = 0;
            foreach (Button btn in ColorBtnList)
            {
                if (btn == ctr)
                {
                    break;
                }
                i++;
            }

            if (e.Button == MouseButtons.Left)
            {
                if (ctr.ImageIndex == 0)
                {
                    int count = 0;
                    foreach (Button btn in ColorBtnList)
                    {
                        if (btn.ImageIndex == 0)
                        {
                            count++;
                        }
                    }

                    if (count < 2) { return; }
                    ctr.ImageIndex = 1;

                    #region apply to history
                    TifFileInfo fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
                    fi.delHist = true;
                    IA.delHist = true;
                    IA.UnDoBtn.Enabled = true;
                    IA.DeleteFromHistory();
                    fi.History.Add("enableColorChanel("
                       + ColorBtnList.IndexOf(ctr).ToString() + ",true)");
                    fi.History.Add("enableColorChanel("
                        + ColorBtnList.IndexOf(ctr).ToString() + ",false)");
                    IA.UpdateUndoBtns();
                    #endregion apply to history
                    //apply settings

                    if (i < tifFI.sizeC & i == tifFI.cValue)
                    {
                        IA.RoiMan.current = null;
                        for (int j = 0; j < ColorBtnList.Count - 1; j++)
                        {
                            if (ColorBtnList[j].ImageIndex == 0)
                            {
                                tifFI.cValue = j;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    ctr.ImageIndex = 0;

                    #region apply to history
                    TifFileInfo fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
                    fi.delHist = true;
                    IA.delHist = true;
                    IA.UnDoBtn.Enabled = true;
                    IA.DeleteFromHistory();
                    fi.History.Add("enableColorChanel("
                        + ColorBtnList.IndexOf(ctr).ToString() + ",false)");
                    fi.History.Add("enableColorChanel("
                        + ColorBtnList.IndexOf(ctr).ToString() + ",true)");
                    IA.UpdateUndoBtns();
                    #endregion apply to history

                    if (ColorBtnList[tifFI.cValue].ImageIndex != 0)
                    {
                        for (int j = 0; j < ColorBtnList.Count - 1; j++)
                        {
                            if (ColorBtnList[j].ImageIndex == 0)
                            {
                                IA.RoiMan.current = null;
                                tifFI.cValue = j;
                                break;
                            }
                        }
                    }
                }

                IA.ReloadImages();
            }
            else if (e.Button == MouseButtons.Right)
            {

                if (i >= tifFI.LutList.Count) return;

                ColorDialog colorDialog1 = new ColorDialog();
                colorDialog1.AllowFullOpen = true;
                colorDialog1.AnyColor = true;
                colorDialog1.FullOpen = true;

                colorDialog1.Color = tifFI.LutList[i];
                //set Custom Colors
                if (IA.settings.CustomColors[IA.FileBrowser.ActiveAccountIndex] != "@")
                {
                    List<int> colorsList = new List<int>();
                    foreach (string j in IA.settings.CustomColors[IA.FileBrowser.ActiveAccountIndex]
                        .Split(new[] { "\t" }, StringSplitOptions.None))
                    {
                        colorsList.Add(int.Parse(j));
                    }
                    colorDialog1.CustomColors = colorsList.ToArray();
                }
                // Show the color dialog.
                DialogResult result = colorDialog1.ShowDialog();
                //Copy Custom Colors
                int[] colors = (int[])colorDialog1.CustomColors.Clone();
                string txt = "@";
                if (colors.Length > 0)
                {
                    txt = colors[0].ToString();
                    for (int j = 1; j < colors.Length; j++)
                    {
                        txt += "\t" + colors[j].ToString();
                    }
                }
                IA.settings.CustomColors[IA.FileBrowser.ActiveAccountIndex] = txt;
                IA.settings.Save();

                if (result == DialogResult.OK)
                {
                    IA.Input.ChangeValueFunction("LUT(" + i.ToString() + "," +
                            ColorTranslator.ToHtml(colorDialog1.Color).ToString() + ")");

                }
            }
        }

        private void Control_MouseOver(object sender, EventArgs e)
        {
            Button ctr = (Button)sender;
            string txt = ctr.Tag.ToString();
            if (ctr.ImageIndex == 0)
            {
                txt += " - Enabled \nLeft mouse click to disable chanel.";
            }
            else
            {
                txt += " - Disabled \nLeft mouse click to enable chanel.";
            }
            if (ctr != ColorBtnList[ColorBtnList.Count - 1] | ColorBtnList.Count == 1)
            {
                txt += "\nRight mouse click to change Look-Up table.";
            }
            TurnOnToolTip.SetToolTip(ctr, txt);
        }
        public void VisualizeColorBtns()
        {
            int w = 0;
            foreach (Button btn in ColorBtnList)
            {
                ColorPanel.Controls.Add(btn);
                btn.Location = new Point(w, 5);
                w += btn.Width;
            }
            ColorPanel.Width = w;
        }
        public void VisualizeMethodsBtns()
        {
            int w = 0;
            foreach (Button btn in MethodsBtnList)
            {
                MethodsPanel.Controls.Add(btn);
                btn.Location = new Point(w, 5);
                w += btn.Width;
            }
            MethodsPanel.Width = w;
        }
        public void Refresh()
        {
            AddColorBtn();
            AddMethodBtn();
        }
        public void AddColorBtn()
        {
            for (int i = ColorBtnList.Count - 1; i >= 0; i--)
                ColorBtnList[i].Dispose();

            ColorBtnList = new List<Button>();

            Bitmap GrayBmp = new Bitmap(20, 20);

            using (Graphics gr = Graphics.FromImage(GrayBmp))
            {
                gr.FillRectangle(new SolidBrush(Color.DarkGray), new Rectangle(1, 1, 17, 17));
                gr.DrawRectangle(new Pen(Color.Black), new Rectangle(1, 1, 17, 17));
            }

            for (int i = 0; i < tifFI.LutList.Count; i++)
            {
                //create bitmap
                Bitmap bmp = new Bitmap(20, 20);

                using (Graphics gr = Graphics.FromImage(bmp))
                {
                    gr.FillRectangle(new SolidBrush(tifFI.LutList[i]), new Rectangle(1, 1, 17, 17));
                    gr.DrawRectangle(new Pen(Color.Black), new Rectangle(1, 1, 17, 17));
                }

                Button btn = new Button();
                btn.FlatAppearance.BorderSize = 0;
                ImageList il = new ImageList();
                il.ImageSize = new Size(20, 20);
                il.Images.Add(bmp);
                il.Images.Add(GrayBmp);
                btn.ImageList = il;
                btn.ImageIndex = 0;
                btn.FlatStyle = FlatStyle.Flat;
                btn.BackColor = Color.FromArgb(0, 255, 255, 255);
                btn.Tag = "Chanel " + (i + 1).ToString();
                btn.Width = btn.Height = 25;
                ColorBtnList.Add(btn);
                btn.MouseHover += new EventHandler(Control_MouseOver);
                btn.MouseDown += new MouseEventHandler(Control_MouseDown);
                btn.BringToFront();
            }

            //composite
            if (ColorBtnList.Count > 1)
            {
                //create bitmap
                Bitmap bmp = new Bitmap(20, 20);

                using (Graphics gr = Graphics.FromImage(bmp))
                {
                    gr.FillRectangle(new SolidBrush(tifFI.LutList[1]), new Rectangle(0, 0, 16, 16));
                    gr.DrawRectangle(new Pen(Color.Black), new Rectangle(0, 0, 16, 16));
                    gr.FillRectangle(new SolidBrush(tifFI.LutList[0]), new Rectangle(3, 3, 16, 16));
                    gr.DrawRectangle(new Pen(Color.Black), new Rectangle(3, 3, 16, 16));
                }

                Bitmap GrayBmpComp = new Bitmap(20, 20);

                using (Graphics gr = Graphics.FromImage(GrayBmpComp))
                {
                    gr.FillRectangle(new SolidBrush(Color.DarkGray), new Rectangle(0, 0, 16, 16));
                    gr.DrawRectangle(new Pen(Color.Black), new Rectangle(0, 0, 16, 16));
                    gr.FillRectangle(new SolidBrush(Color.DarkGray), new Rectangle(3, 3, 16, 16));
                    gr.DrawRectangle(new Pen(Color.Black), new Rectangle(3, 3, 16, 16));
                }

                Button btn = new Button();
                ImageList il = new ImageList();
                il.ImageSize = new Size(20, 20);
                il.Images.Add(bmp);
                il.Images.Add(GrayBmpComp);
                btn.ImageList = il;
                btn.ImageIndex = 0;
                btn.FlatAppearance.BorderSize = 0;
                btn.FlatStyle = FlatStyle.Flat;
                btn.BackColor = Color.FromArgb(0, 255, 255, 255);
                btn.Tag = "Composite image";
                btn.Width = btn.Height = 25;
                ColorBtnList.Add(btn);
                btn.MouseHover += new EventHandler(Control_MouseOver);
                btn.MouseDown += new MouseEventHandler(Control_MouseDown);
                btn.BringToFront();
            }

        }
        private void AddMethodBtn()
        {
            for (int i = MethodsBtnList.Count - 1; i >= 0; i--)
                MethodsBtnList[i].Dispose();

            MethodsBtnList = new List<Button>();

            #region RawImage
            {
                Button btn = new Button();

                ImageList il = new ImageList();
                il.ImageSize = new Size(20, 20);
                il.Images.Add(Properties.Resources.rawImage);
                il.Images.Add(Properties.Resources.rawImageGS);
                btn.ImageList = il;
                btn.ImageIndex = 0;

                btn.FlatAppearance.BorderSize = 0;
                btn.FlatStyle = FlatStyle.Flat;
                btn.BackColor = IA.FileBrowser.BackGroundColor1;
                btn.Tag = "Raw image";
                btn.Width = btn.Height = 25;
                MethodsBtnList.Add(btn);
                btn.MouseHover += new EventHandler(MethodControl_MouseOver);
                btn.MouseDown += new MouseEventHandler(MethodControl_MouseDown);
            }
            #endregion RawImage

            #region Filtered Image
            {
                Button btn = new Button();

                ImageList il = new ImageList();
                il.ImageSize = new Size(20, 20);
                il.Images.Add(Properties.Resources.processedImage);
                il.Images.Add(Properties.Resources.processedImageGS);
                btn.ImageList = il;
                btn.ImageIndex = 1;

                btn.FlatAppearance.BorderSize = 0;
                btn.FlatStyle = FlatStyle.Flat;
                btn.BackColor = IA.FileBrowser.BackGroundColor1;
                btn.Tag = "Processed image";
                btn.Width = btn.Height = 25;
                MethodsBtnList.Add(btn);
                btn.MouseHover += new EventHandler(MethodControl_MouseOver);
                btn.MouseDown += new MouseEventHandler(MethodControl_MouseDown);
            }
            #endregion Filtered Image

            #region Chart
            {
                Button btn = new Button();

                ImageList il = new ImageList();
                il.ImageSize = new Size(20, 20);
                il.Images.Add(Properties.Resources.chart_mini);
                il.Images.Add(Properties.Resources.chart_miniGS);
                btn.ImageList = il;
                btn.ImageIndex = 1;

                btn.FlatAppearance.BorderSize = 0;
                btn.FlatStyle = FlatStyle.Flat;
                btn.BackColor = IA.FileBrowser.BackGroundColor1;
                btn.Tag = "Results chart";
                btn.Width = btn.Height = 25;
                MethodsBtnList.Add(btn);
                btn.MouseHover += new EventHandler(MethodControl_MouseOver);
                btn.MouseDown += new MouseEventHandler(MethodControl_MouseDown);
            }
            #endregion Chart
        }
        private void MethodControl_MouseOver(object sender, EventArgs e)
        {
            Button ctr = (Button)sender;
            string txt = ctr.Tag.ToString();
            if (ctr.ImageIndex == 0)
            {
                txt += " - Enabled \nLeft mouse click to disable chanel.";
            }
            else
            {
                txt += " - Disabled \nLeft mouse click to enable chanel.";
            }
            TurnOnToolTip.SetToolTip(ctr, txt);
        }
        private void MethodControl_MouseDown(object sender, MouseEventArgs e)
        {

            Button ctr = (Button)sender;

            if (e.Button == MouseButtons.Left)
            {
                if (ctr.ImageIndex == 0)
                {
                    int i = 0;
                    foreach (Button btn in MethodsBtnList)
                    {
                        if (btn == ctr)
                        {
                            break;
                        }
                        i++;
                    }

                    int count = 0;
                    foreach (Button btn in MethodsBtnList)
                    {
                        if (btn.ImageIndex == 0)
                        {
                            count++;
                        }
                    }

                    if (count < 2) { return; }

                    ctr.ImageIndex = 1;

                    #region apply to history
                    TifFileInfo fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
                    fi.delHist = true;
                    IA.delHist = true;
                    IA.UnDoBtn.Enabled = true;
                    IA.DeleteFromHistory();
                    fi.History.Add("enableMethodView("
                       + MethodsBtnList.IndexOf(ctr).ToString() + ",true)");
                    fi.History.Add("enableMethodView("
                        + MethodsBtnList.IndexOf(ctr).ToString() + ",false)");
                    IA.UpdateUndoBtns();
                    #endregion apply to history

                    if (i == tifFI.selectedPictureBoxColumn)
                    {
                        for (int j = 0; j < MethodsBtnList.Count; j++)
                        {
                            if (MethodsBtnList[j].ImageIndex == 0)
                            {
                                tifFI.selectedPictureBoxColumn = j;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    ctr.ImageIndex = 0;

                    #region apply to history
                    TifFileInfo fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
                    fi.delHist = true;
                    IA.delHist = true;
                    IA.UnDoBtn.Enabled = true;
                    IA.DeleteFromHistory();
                    fi.History.Add("enableMethodView("
                        + MethodsBtnList.IndexOf(ctr).ToString() + ",false)");
                    fi.History.Add("enableMethodView("
                       + MethodsBtnList.IndexOf(ctr).ToString() + ",true)");
                    IA.UpdateUndoBtns();
                    #endregion apply to history

                    if (MethodsBtnList[tifFI.selectedPictureBoxColumn].ImageIndex != 0)
                    {
                        for (int j = 0; j < MethodsBtnList.Count; j++)
                        {
                            if (MethodsBtnList[j].ImageIndex == 0)
                            {
                                tifFI.selectedPictureBoxColumn = j;
                                break;
                            }
                        }
                    }
                }
                IA.ReloadImages();
            }
        }
    }
}
