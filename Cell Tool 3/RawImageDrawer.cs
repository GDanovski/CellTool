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
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace Cell_Tool_3
{
    class ImageDrawer
    {
        public ImageAnalyser IA = null;
        public Panel corePanel = new Panel();
        public ContentPipe ImageTexture = new ContentPipe();
        private ImagesTextures imagesTextures = new ImagesTextures();
        public ImageDrawer_3D imageDrawer_3D = new ImageDrawer_3D();

        public VScrollBar verticalScrollBar;
        public HScrollBar horizontalScrollBar;

        #region Position on screen
        public Rectangle[][] coRect;
        bool[] colors;
        bool composite;
        //scale
        double oldScale = 1;
        //translation
        public double valX = 0;
        public double valY = 0;
        bool changeXY = true;
        #endregion

        #region New Image Drawing
        public void Initialize(GLControl GLControl1)
        {
            GLControl1.Load += GLControl_Load;
            GLControl1.Paint += GLControl_Paint;
            GLControl1.Resize += GLControl_Resize;
            GLControl1.MouseMove += ShowXYVal;
            GLControl1.MouseLeave += HideXYVal;
            GLControl1.MouseClick += GLControl1_MouseClick;
            GLControl1.MouseWheel += GLControl1_MouseWheel;
            GLControl1.MouseDoubleClick += HidePropertiesAndFileBrowser_DoubleClick;
            GLControl1.MouseDown += GLControl1_MouseDown;
            GLControl1.MouseMove += GLControl1_MouseMove;
            GLControl1.MouseUp += GLControl1_MouseUp;

            GLControl1.Dock = DockStyle.Fill;
            TabPageControl tpContr = IA.TabPages;
            tpContr.ImageMainPanel.SuspendLayout();
            
            tpContr.ImageMainPanel.Controls.Add(GLControl1);
           //new scrollBars
           verticalScrollBar = new VScrollBar()
            {
                Dock = DockStyle.None,
                Width = 17,
                Visible = false,
                Value = 0,
                Minimum = 0,
                Maximum = 0
            };
            
            tpContr.ImageMainPanel.Controls.Add(verticalScrollBar);
            horizontalScrollBar = new HScrollBar()
            {
                Dock = DockStyle.Fill,
                Visible = false,
                Value = 0,
                Minimum = 0,
                Maximum = 0
            };
            
           Panel bottomScrollPanel = new Panel()
            {
                Dock = DockStyle.Bottom,
                Visible = true,
                Height = 17
            };

            Panel rightSpacePanel = new Panel()
            {
                Dock = DockStyle.Right,
                Visible = false,
                Width = verticalScrollBar.Width,
                BackColor = Color.White
            };
            horizontalScrollBar.VisibleChanged += new EventHandler(delegate (object sender, EventArgs a)
            {
                rightSpacePanel.Visible = horizontalScrollBar.Visible;
            });

            bottomScrollPanel.Controls.Add(horizontalScrollBar);
            bottomScrollPanel.Controls.Add(rightSpacePanel);
            tpContr.ImageMainPanel.Controls.Add(bottomScrollPanel);
            
            verticalScrollBar.Scroll += VerticalScroll_ValueChanged;
            horizontalScrollBar.Scroll += HorizontalScroll_ValueChanged;
            //Top Bar
            corePanel.Dock = DockStyle.Top;
            tpContr.ImageMainPanel.Controls.Add(corePanel);
            
            tpContr.ImageMainPanel.ResumeLayout(true);
        }
        private void HorizontalScroll_ValueChanged(object sender, EventArgs e)
        {
            if (changeXY == false) { return; }
            TifFileInfo fi;
            try
            {
                fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
            }
            catch
            {
                return;
            }
            if (fi == null) { return; }
            if (fi.Xposition != this.horizontalScrollBar.Value / fi.zoom)
            {
                fi.Xposition = this.horizontalScrollBar.Value / fi.zoom;
                IA.ReloadImages(false);
            }
        }
        private void VerticalScroll_ValueChanged(object sender, EventArgs e)
        {
            if (changeXY == false) { return; }
            TifFileInfo fi;
            try
            {
                fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
            }
            catch
            {
                return;
            }
            if (fi == null) { return; }
            if (fi.Yposition != this.verticalScrollBar.Value / fi.zoom)
            {
                fi.Yposition = this.verticalScrollBar.Value / fi.zoom;
                IA.ReloadImages(false);
            }
        }
        public void ClearImage()
        {
            try
            {
                //Activate Control
                IA.GLControl1.MakeCurrent();
                //Load background
                GL.ClearColor(IA.FileBrowser.BackGround2Color1);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                //Prepare MatrixMode
                GL.MatrixMode(MatrixMode.Projection);
                GL.LoadIdentity();
                IA.GLControl1.SwapBuffers();
            }
            catch { }
        }
        public void DrawToScreen(bool toRecalculateImages = true, int recalcChannel = -1, int recalcMethod = -1)
        {
            GLDrawing_Start(IA.GLControl1, toRecalculateImages, recalcChannel,recalcMethod);
        }
        #region GLControl_Events
        public void GLControl_Load(object sender, EventArgs e)
        {
            GLControl GLControl1 = sender as GLControl;
            GLControl1.MakeCurrent();

            GL.ClearColor(IA.FileBrowser.BackGround2Color1);
            //ImageTexture
            ImageTexture.ReserveTextureID();
            ImageTexture.GenerateNumberTextures();
        }
        public void GLControl_Resize(object sender, EventArgs e)
        {
            try
            {
                //Global variables
                TifFileInfo fi;
                try
                {
                    fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
                }
                catch
                {
                    return;
                }
                if (fi == null) { return; }
                GLControl GLControl1 = sender as GLControl;

                //Activate Control
                GLControl1.MakeCurrent();

                GL.Viewport(0, 0, GLControl1.Width, GLControl1.Height);
                GL.MatrixMode(MatrixMode.Projection);
                GL.LoadIdentity();
            }
            catch { }
        }

        public void GLControl_Paint(object sender, EventArgs e)
        {
            //Global variables
            GLControl GLControl1 = sender as GLControl;
            GLDrawing_Start(GLControl1,false);
        }
        private void GLDrawing_Start(GLControl GLControl1,bool toRecalculateImages = true, int recalcChannel = -1, int recalcMethod = -1)
        {
            //try
            {
                TifFileInfo fi;
                try
                {
                    fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
                }
                catch
                {
                    return;
                }
                if (fi == null)
                {
                    return;
                }

                if (GLControl1.Visible == false) { GLControl1.Visible = true; }

                Rectangle fieldRect = coRect_Calculate(GLControl1);

                //Calculate B&C
                CalculateImages(fi);

                fi.tpTaskbar.TopBar.SendToBack();

                //Start Drawing
                //Activate Control
                GLControl1.MakeCurrent();
                GL.Disable(EnableCap.Texture2D);
                //Load background
                GL.ClearColor(IA.FileBrowser.BackGround2Color1);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                //if the 3D is enabled - send to imageDrawer_3D
                if (imageDrawer_3D.isImage3D(fi))
                {
                    //Set viewpoint
                    GL.Viewport(0, 0, GLControl1.Width, GLControl1.Height);
                    //scale the image
                    if (oldScale != fi.zoom)
                    {
                        double factor = fi.zoom / oldScale;
                        oldScale = fi.zoom;
                        if (factor != 1)
                        {
                            GL.Scale(factor, factor, 1);
                        }
                    }

                    imageDrawer_3D.Calculate3Dfi(fi);
                    imageDrawer_3D.StartDrawing(GLControl1, fi);

                    return;
                }

                //GL.UseProgram(0); // Remove the shader program from the 3D view

                //Prepare MatrixMode
                GL.MatrixMode(MatrixMode.Projection);
                GL.LoadIdentity();
                //Prepare Projection
                GL.Ortho(0.0, (double)GLControl1.Width, (double)GLControl1.Height, 0.0, -1.0, 1.0);

                GL.MatrixMode(MatrixMode.Modelview);
                //GL.LoadIdentity();
                GL.Translate(-valX, -valY, 0);
                valX = 0;
                valY = 0;

                //Set viewpoint
                GL.Viewport(0, 0, GLControl1.Width, GLControl1.Height);
                TranslationAndScale(fi, fieldRect, GLControl1);
                
                //make colors transparent
                GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.One);
                SelectedImage_DrawBorder(fi);
                DrawBackgrounds_Global(fi);
                GL.Enable(EnableCap.Blend);
                GL.ShadeModel(ShadingModel.Flat);

                List<Button> MethodsBtnList = fi.tpTaskbar.MethodsBtnList;
                
                if (toRecalculateImages)
                {
                    bool toDrawRawImage = MethodsBtnList[0].ImageIndex == 0;
                    bool toDrawFilteredImage = MethodsBtnList[1].ImageIndex == 0;
                    //prepare the image list
                    this.imagesTextures.PrepareImageList(fi.sizeC * 2);
                                                            
                    for (int index = 0; index < fi.sizeC * 2; index++)
                        if ((index < fi.sizeC && toDrawRawImage) || (index >= fi.sizeC && toDrawFilteredImage))
                        {
                            int C = fi.sizeC > index ? index : index - fi.sizeC;
                            
                            if (!(colors[C] || composite)) continue;//exit if channel and composite images are disabled
                            if (recalcChannel != -1 && recalcChannel != C) continue;//exit if the channel is disabled
                            if (recalcMethod == 0 && index >= fi.sizeC) continue;//exit if the method 1 is disabled
                            if (recalcMethod == 1 && index < fi.sizeC) continue;//exit if the method 2 is disabled

                            //create image texture
                            if (fi.sizeC > index)
                            {
                                this.imagesTextures.GenerateImageData(fi, index, C);
                            }
                            else
                            {
                                int[] SpotDiapason = IA.Segmentation.SpotDet.CalculateBorders(fi, C);
                                this.imagesTextures.GenerateFilteredImageData(fi, index, C, SpotDiapason);
                            }
                            //Load the images textures
                            this.imagesTextures.LoadTextures(fi, index);
                        }
                }
                //draw the images
                GL.Enable(EnableCap.Texture2D);
                if (MethodsBtnList[0].ImageIndex == 0)
                    DrawRawImages(fi);

                if (MethodsBtnList[1].ImageIndex == 0)
                    DrawFilteredImages(fi);

                GL.Disable(EnableCap.Texture2D);

                GL.Disable(EnableCap.Blend);

                //draw chart
                try
                {
                    if (MethodsBtnList[2].ImageIndex == 0)
                        IA.chart.LoadFI(fi);
                }
                catch { }
                //draw rois
                drawRoi(fi);
                if (IA.RoiMan.current != null) drawCurrentRoi(fi);

                GLControl1.SwapBuffers();
            }
            //catch { }

        }
        private void TranslationAndScale(TifFileInfo fi, Rectangle fieldRect, GLControl glcontrol1)
        {
            int vertValue = (int)(fi.Yposition * fi.zoom);
            int vertMax = (int)(fieldRect.Height * fi.zoom) - IA.GLControl1.Height+10;
            int horValue = (int)(fi.Xposition * fi.zoom);
            int horMax = (int)(fieldRect.Width * fi.zoom) - IA.GLControl1.Width+10;
            //scale the image
            if (oldScale != fi.zoom)
            {
                double factor = fi.zoom / oldScale;
                oldScale = fi.zoom;
                if (factor != 1)
                {
                   GL.Scale(factor, factor, 1);
                   
                    if (vertMax < glcontrol1.Height + 10) fi.Yposition = 0;
                    if (horMax < glcontrol1.Width + 10) fi.Xposition = 0;
                }                   
            }
            changeXY = false;

            //Y position             
             if (vertValue <= vertMax)
            {
                if (verticalScrollBar.Maximum >= vertValue)
                {
                    verticalScrollBar.Value = vertValue;
                    verticalScrollBar.Maximum = vertMax;
                }
                else if(verticalScrollBar.Value <= vertMax)
                {
                    verticalScrollBar.Maximum = vertMax;
                    verticalScrollBar.Value = vertValue;
                }
                else
                {
                    verticalScrollBar.Value = 0;
                    verticalScrollBar.Maximum = vertMax;
                    verticalScrollBar.Value = vertValue;
                }

                if (verticalScrollBar.Location.X != glcontrol1.Width - verticalScrollBar.Width)
                    verticalScrollBar.Location = new Point(glcontrol1.Width - verticalScrollBar.Width, glcontrol1.Location.Y);
                if (verticalScrollBar.Height != glcontrol1.Height)
                    verticalScrollBar.Height = glcontrol1.Height;
                verticalScrollBar.BringToFront();

                verticalScrollBar.Visible = true;
            }
            else
            {
                fi.Yposition = 0;
                verticalScrollBar.Value = 0;
                verticalScrollBar.Maximum = 1;
                verticalScrollBar.Visible = false;
            }
            //X position
            if (horValue <= horMax)
            {
                if (horizontalScrollBar.Maximum >= horValue)
                {
                    horizontalScrollBar.Value = horValue;
                    horizontalScrollBar.Maximum = horMax;
                }
                else if (horizontalScrollBar.Value <= horMax)
                {
                    horizontalScrollBar.Maximum = horMax;
                    horizontalScrollBar.Value = horValue;
                }
                else
                {
                    horizontalScrollBar.Value = 0;
                    horizontalScrollBar.Maximum = horMax;
                    horizontalScrollBar.Value = horValue;
                }

                horizontalScrollBar.Visible = true;
            }
            else
            {
                fi.Xposition = 0;
                horizontalScrollBar.Value = 0;
                horizontalScrollBar.Maximum = 1;
                horizontalScrollBar.Visible = false;
            }

            valX = -fi.Xposition;
            valY = -fi.Yposition;

            GL.Translate(valX, valY, 0);
            changeXY = true;            
        }
        private void DrawLine()
        {
            GL.EnableClientState(ArrayCap.VertexArray);
            float[] para_vertex =
{
    50,270,
    100,30,
    54,270,
    104,30,
    58,270,
    108,30
};
            float[] para_color = new float[]
            {
    1f,0f,0f,    //red
};
            GL.VertexPointer(2, VertexPointerType.Float, 0, para_vertex);
            GL.ColorPointer(3, ColorPointerType.Float, 0, para_color);
            GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 6);

            GL.DisableClientState(ArrayCap.VertexArray);
        }
        #endregion

        private void DrawBackgrounds_Global(TifFileInfo fi)
        {
            //singlechanels or composite
            for (int C = 0; C < fi.sizeC; C++)
            {
                if (colors[C] == true)
                {
                    //RawImage
                    RawImage_DrawBackColor(coRect[0][C]);
                    //FilterImage
                    RawImage_DrawBackColor(coRect[1][C]);
                    //Chart
                    Chart_DrawBackColor(coRect[2][C]);
                }
            }
            if (composite == true)
            {
                RawImage_DrawBackColor(coRect[0][fi.sizeC]);
                RawImage_DrawBackColor(coRect[1][fi.sizeC]);
            }
        }
        private void SelectedImage_DrawBorder(TifFileInfo fi)
        {
            int C = fi.cValue;
            if (colors[C] == false) { return; }

            int column = fi.selectedPictureBoxColumn;
            Rectangle rect = coRect[column][C];
            float lineWidth = (float)(3 / fi.zoom);

            float X = rect.X - lineWidth;
            float Y = rect.Y - lineWidth;
            float W = rect.X + rect.Width + lineWidth;
            float H = rect.Y + rect.Height + lineWidth;

            GL.Begin(PrimitiveType.Quads);
            GL.Color4(0.8f, 0.8f, 0.8f, 1f);

            GL.Vertex2(X, Y);
            GL.Vertex2(X, H);
            GL.Vertex2(W, H);
            GL.Vertex2(W, Y);

            GL.End();

        }
        private void RawImage_DrawBackColor(Rectangle rect)
        {
            int W = rect.X + rect.Width;
            int H = rect.Y + rect.Height;
            //start drawing
            GL.Begin(PrimitiveType.Quads);

            GL.Color4(0f, 0f, 0f, 1f);

            GL.Vertex2(rect.X, rect.Y);
            GL.Vertex2(rect.X, H);
            GL.Vertex2(W, H);
            GL.Vertex2(W, rect.Y);

            GL.End();

        }
        private void Chart_DrawBackColor(Rectangle rect)
        {
            int W = rect.X + rect.Width;
            int H = rect.Y + rect.Height;
            //start drawing
            GL.Begin(PrimitiveType.Quads);

            GL.Color3(1f, 1f, 1f);

            GL.Vertex2(rect.X, rect.Y);
            GL.Vertex2(rect.X, H);
            GL.Vertex2(W, H);
            GL.Vertex2(W, rect.Y);

            GL.End();

            Chart_drawBorder(rect.X, rect.Y, rect.Width, rect.Height);
        }
        private void Chart_drawBorder(float x, float y, float w, float h)
        {
            w += x;
            h += y;

            GL.Begin(PrimitiveType.LineLoop);
            GL.Color3(0f, 0f, 0f);

            GL.Vertex2(x, y);
            GL.Vertex2(w, y);
            GL.Vertex2(w, h);
            GL.Vertex2(x, h);

            GL.End();
        }
        private void DrawImage(TifFileInfo fi, int C, int rectC)
        {
            try
            {
                Rectangle rect = new Rectangle(
                    coRect[0][rectC].X,
                    coRect[0][rectC].Y,
                    coRect[0][rectC].X + fi.sizeX,
                    coRect[0][rectC].Y + fi.sizeY);
                imagesTextures.DrawTexture(C, rect);
            }
            catch { }
        }
        private void DrawFilteredImage(TifFileInfo fi, int C, int rectC)
        {
            try
            {
                Rectangle rect = new Rectangle(
                    coRect[1][rectC].X,
                    coRect[1][rectC].Y,
                    coRect[1][rectC].X + fi.sizeX,
                    coRect[1][rectC].Y + fi.sizeY);
                imagesTextures.DrawTexture(fi.sizeC+C, rect);
            }
            catch { }
        }        
        private void DrawFilteredImages(TifFileInfo fi)
        {
            //singlechanels or composite

            int col = coRect[0].Length - 1;
            for (int i = 0; i < col; i++)
            {
                if (colors[i] == true)
                {
                    col = i;
                    break;
                }
            }
            for (int C = 0; C < fi.sizeC; C++)
            {
                if (colors[C] == true)
                {
                    switch (fi.bitsPerPixel)
                    {
                        case 8:
                            DrawFilteredImage(fi, C, C);
                            break;
                        case 16:
                            DrawFilteredImage(fi, C, C);
                            break;
                    }
                }
            }
            if (composite == true)
            {
                for (int C1 = 0; C1 < fi.sizeC; C1++)
                {
                    switch (fi.bitsPerPixel)
                    {
                        case 8:
                            DrawFilteredImage(fi, C1, fi.sizeC);
                            break;
                        case 16:
                            DrawFilteredImage(fi, C1, fi.sizeC);
                            break;
                    }
                }
            }
        }
        private void DrawRawImages(TifFileInfo fi)
        {
            //singlechanels or composite
            
            int col = coRect[0].Length - 1;
            for (int i = 0; i < col; i++)
            {
                if (colors[i] == true)
                {
                    col = i;
                    break;
                }
            }    
            for (int C = 0; C < fi.sizeC; C++)
            {
                if (colors[C] == true)
                {
                    switch (fi.bitsPerPixel)
                    {
                        case 8:
                            DrawImage(fi, C, C);
                            break;
                        case 16:
                            DrawImage(fi, C, C);
                            break;
                    }
                }
            }
            if (composite == true)
            {                
                for (int C1 = 0; C1 < fi.sizeC; C1++)
                {
                    switch (fi.bitsPerPixel)
                    {
                        case 8:
                            DrawImage(fi, C1, fi.sizeC);
                            break;
                        case 16:
                            DrawImage(fi, C1, fi.sizeC);
                            break;
                    }
                }
            }
        }
        public Rectangle coRect_Calculate(GLControl GLControl1)
        {
            TifFileInfo fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
            //Prepare coRect
            coRect = new Rectangle[3][];

            //create color array
            colors = new bool[fi.sizeC];
            List<Button> MethodsBtnList = fi.tpTaskbar.MethodsBtnList;
            List<Button> ColorBtnList = fi.tpTaskbar.ColorBtnList;
            for (int i = 0; i < fi.sizeC; i++)
            {
                if (ColorBtnList[i].ImageIndex == 0)
                {
                    colors[i] = true;
                }
                else
                {
                    colors[i] = false;
                }
            }
            //composite image

            if (fi.sizeC > 1)
            {
                if (ColorBtnList[fi.sizeC].ImageIndex == 0)
                {
                    composite = true;
                }
                else
                {
                    composite = false;
                }
            }
            else
            {
                composite = false;
            }
            //Take Size

            int sizeH = 0;
            int sizeW = 0;

            #region RawImages - index 0
            int z = fi.sizeC;
            if (z > 1) { z++; }
            coRect[0] = new Rectangle[z];

            int biggestW = 0;
            int biggestH = 0;
            int interval = (int)(10 / fi.zoom);
            sizeH = interval;
            sizeW = interval;
            //foreach (bool val in colors)
            for (int i = 0; i < colors.Length; i++)
            {
                if (colors[i] == true & MethodsBtnList[0].ImageIndex == 0)
                {
                    //position
                    Rectangle newRect =
                        new Rectangle((int)sizeW, (int)sizeH,
                        (int)(fi.sizeX), (int)(fi.sizeY));
                    coRect[0][i] = newRect;
                    //MessageBox.Show("X - " + sizeW.ToString() + "\tY - " + sizeH.ToString() + "\tW - " + fi.sizeX.ToString() + "\tH - " + fi.sizeY);
                    //size
                    sizeH += (fi.sizeY + interval);
                    if (biggestW < fi.sizeX) { biggestW = fi.sizeX; }
                }
            }
            if (composite == true & MethodsBtnList[0].ImageIndex == 0)
            {
                //position
                Rectangle newRect =
                    new Rectangle((int)sizeW, (int)sizeH,
                    (int)(fi.sizeX), (int)(fi.sizeY));
                coRect[0][fi.sizeC] = newRect;
                //size
                sizeH += (int)(fi.sizeY + interval);
                if (biggestW < fi.sizeX) { biggestW = fi.sizeX; }

            }

            if (biggestW != interval)
            {
                //size
                sizeW += (int)(biggestW + interval);
            }
            if (biggestH < sizeH) { biggestH = sizeH; }
            #endregion RawImages - index 0

            #region Filtered image - index 1
            z = fi.sizeC;
            if (z > 1) { z++; }
            coRect[1] = new Rectangle[z];

            biggestW = 0;
            sizeH = interval;

            for (int i = 0; i < colors.Length; i++)
            {
                if (colors[i] == true & MethodsBtnList[1].ImageIndex == 0)
                {
                    //position
                    Rectangle newRect =
                        new Rectangle((int)sizeW, (int)sizeH,
                        (int)(fi.sizeX), (int)(fi.sizeY));
                    coRect[1][i] = newRect;
                    //size
                    sizeH += (fi.sizeY + interval);
                    if (biggestW < fi.sizeX) { biggestW = fi.sizeX; }
                }
            }

            if (composite == true & MethodsBtnList[1].ImageIndex == 0)
            {
                //position
                Rectangle newRect =
                    new Rectangle((int)sizeW, (int)sizeH,
                    (int)(fi.sizeX), (int)(fi.sizeY));
                coRect[1][fi.sizeC] = newRect;
                //size
                sizeH += (int)(fi.sizeY + interval);
                if (biggestW < fi.sizeX) { biggestW = fi.sizeX; }

            }

            if (biggestW != interval)
            {
                //size
                sizeW += (int)(biggestW + interval);
            }
            if (biggestH < sizeH) { biggestH = sizeH; }
            #endregion Filtered image - index 1

            #region Chart - index 1
            z = fi.sizeC;
            coRect[2] = new Rectangle[z];

            biggestW = 0;
            sizeH = interval;

            for (int i = 0; i < colors.Length; i++)
            {
                if (colors[i] == true & MethodsBtnList[2].ImageIndex == 0)
                {
                    //position
                    Rectangle newRect =
                        new Rectangle((int)sizeW, (int)sizeH,
                        (int)(fi.sizeY * 1.5), (int)(fi.sizeY));
                    coRect[2][i] = newRect;
                    //size
                    sizeH += (int)(fi.sizeY + interval);
                    // if (biggestW < fi.sizeX) { biggestW = fi.sizeX; }
                    if (biggestW < newRect.Width) { biggestW = newRect.Width; }
                }
            }
            if (biggestW != interval)
            {
                //size
                sizeW += (int)(biggestW + interval);
            }
            if (biggestH < sizeH) { biggestH = sizeH; }
            #endregion Chart - index 1
            //Calculate Field
            Rectangle result = new Rectangle(0, 0, (int)sizeW, (int)biggestH);

            return result;
        }
        public void CalculateImages(TifFileInfo fi)
        {
            if (IA.BandC.autoDetect.Checked == true | fi.adjustedLUT == null)
            {
                int curC = fi.cValue;
                for (int c = 0; c < fi.sizeC; c++)
                {
                    fi.cValue = c;
                    IA.BandC.calculateHistogramArray(fi, true);
                }
                fi.cValue = curC;
            }
            IA.BandC.calculateHistogramArray(fi, true);
        }
        public void ShowXYVal(object sender, MouseEventArgs e)
        {
            TifFileInfo fi;
            try
            {
                fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;

                if (fi == null) { return; }
                //if the 3D is enabled - send to imageDrawer_3D
                if (imageDrawer_3D.isImage3D(fi)) return;

                double zoom = fi.zoom;
                double X1 = e.X / zoom - valX;
                double Y1 = e.Y / zoom - valY;
                int X = Convert.ToInt32(X1);
                if (X > X1) { X--; }
                int Y = Convert.ToInt32(Y1);
                if (Y > Y1) { Y--; }


                Point p = new Point(X, Y);
                int C = -1;
                int metod = -1;
                for (int i = 0; i < fi.sizeC; i++)
                {
                    if (colors[i] == true)
                    {
                        if (coRect[0][i].Contains(p) == true &
                            fi.tpTaskbar.MethodsBtnList[0].ImageIndex == 0)
                        {
                            C = i;
                            metod = 0;
                            X -= coRect[0][i].X;
                            Y -= coRect[0][i].Y;
                            break;
                        }
                        else if (coRect[1][i].Contains(p) == true &
                            fi.tpTaskbar.MethodsBtnList[0].ImageIndex == 0)
                        {
                            C = i;
                            metod = 1;
                            X -= coRect[1][i].X;
                            Y -= coRect[1][i].Y;
                            break;
                        }
                    }
                }

                if (C == -1 | metod == -1)
                {
                    fi.tpTaskbar.XLabel.Visible = false;
                    fi.tpTaskbar.YLabel.Visible = false;
                    fi.tpTaskbar.ValLabel.Visible = false;
                    return;
                }
                //Calculate value
                int Val = 0;

                FrameCalculator FC = new FrameCalculator();
                int frame = FC.FrameC(fi, C);
                switch (fi.bitsPerPixel)
                {
                    case 8:
                        if (metod == 0)
                        {
                            Val = fi.image8bit[frame][Y][X];
                        }
                        else if (metod == 1)
                        {
                            Val = fi.image8bitFilter[frame][Y][X];
                        }
                        break;
                    case 16:
                        if (metod == 0)
                        {
                            Val = fi.image16bit[frame][Y][X];
                        }
                        else if (metod == 1)
                        {
                            Val = fi.image16bitFilter[frame][Y][X];
                        }
                        break;
                }

                fi.tpTaskbar.XLabel.Text = "X: " + X.ToString();
                fi.tpTaskbar.YLabel.Text = "Y: " + Y.ToString();
                fi.tpTaskbar.ValLabel.Text = "Value: " + Val.ToString();

                fi.tpTaskbar.XLabel.Visible = true;
                fi.tpTaskbar.YLabel.Visible = true;
                fi.tpTaskbar.ValLabel.Visible = true;
            }
            catch
            {
                return;
            }
        }
        public void HideXYVal(object sender, EventArgs e)
        {
            TifFileInfo fi;
            try
            {
                fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;

                if (fi == null) { return; }

                fi.tpTaskbar.XLabel.Visible = false;
                fi.tpTaskbar.YLabel.Visible = false;
                fi.tpTaskbar.ValLabel.Visible = false;
            }
            catch
            {
                return;
            }
        }
        private void GLControl1_MouseClick(object sender, MouseEventArgs e)
        {
            GLControl GLControl1 = sender as GLControl;
            if (GLControl1.Focused == false)
            {
                GLControl1.Focus();
            }

            if (e.Button == MouseButtons.Left)
            {
                TifFileInfo fi = null;
                try
                {
                    fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
                }
                catch { return; }
                if (fi == null) { return; }
                if (IA.RoiMan.DrawNewRoiMode || IA.RoiMan.MoveCurrentRoi || IA.RoiMan.activResizeCurrent) return;
                //if the 3D is enabled - send to imageDrawer_3D
                if (imageDrawer_3D.isImage3D(fi))
                {
                    imageDrawer_3D.GLControl1_MouseClick(GLControl1, fi, e);
                    return;
                }

                double zoom = fi.zoom;
                double X1 = e.X / zoom - valX;
                double Y1 = e.Y / zoom - valY;
                int X = Convert.ToInt32(X1);
                if (X > X1) { X--; }
                int Y = Convert.ToInt32(Y1);
                if (Y > Y1) { Y--; }

                Point p = new Point(X, Y);

                for (int i = 0; i < coRect.Length; i++)
                {
                    for (int j = 0; j < fi.sizeC; j++)
                    {
                        if (colors[j] == true)
                        {
                            if (coRect[i][j].Contains(p) == true)
                            {
                                if (fi.cValue != j)
                                    IA.RoiMan.current = null;

                                if (fi.selectedPictureBoxColumn != i |
                                     fi.cValue != j)
                                {
                                    fi.selectedPictureBoxColumn = i;
                                    fi.cValue = j;
                                    //Reload
                                    IA.ReloadImages(false);
                                    return;
                                }
                                else
                                {
                                    return;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void GLControl1_MouseWheel(object sender, MouseEventArgs e)
        {
            if (IA.RoiMan.DrawNewRoiMode == true) return;
            ToolStripComboBox zoomValue = IA.zoomValue;

            if (Control.ModifierKeys == Keys.Control)
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
            else if (Control.ModifierKeys == Keys.Shift)
            {
                if (this.horizontalScrollBar.Visible == true)
                {
                    TifFileInfo fi;
                    try
                    {
                        fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
                    }
                    catch
                    {
                        return;
                    }
                    if (fi == null) { return; }
                    changeXY = false;
                    int val = 0;
                    if (e.Delta > 0)
                    {
                        val = this.horizontalScrollBar.Value
                            + this.horizontalScrollBar.LargeChange;
                    }
                    else if (e.Delta < 0)
                    {
                        val = this.horizontalScrollBar.Value
                            - this.horizontalScrollBar.LargeChange;
                    }

                    if (val < this.horizontalScrollBar.Minimum)
                    {
                        this.horizontalScrollBar.Value = this.horizontalScrollBar.Minimum;
                    }
                    else if (val > this.horizontalScrollBar.Maximum)
                    {
                        this.horizontalScrollBar.Value = this.horizontalScrollBar.Maximum;
                    }
                    else
                    {
                        this.horizontalScrollBar.Value = val;
                    }

                    if (fi.Xposition != this.horizontalScrollBar.Value / fi.zoom)
                    {
                        fi.Xposition = this.horizontalScrollBar.Value / fi.zoom;
                        IA.ReloadImages(false);
                    }

                    changeXY = true;
                }
            }
            else
            {
                if (this.verticalScrollBar.Visible == true)
                {
                    TifFileInfo fi;
                    try
                    {
                        fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
                    }
                    catch
                    {
                        return;
                    }
                    if (fi == null) { return; }
                    changeXY = false;
                    int val = 0;
                    if (e.Delta < 0)
                    {
                        val = this.verticalScrollBar.Value
                            + this.verticalScrollBar.LargeChange;
                    }
                    else if (e.Delta > 0)
                    {
                        val = this.verticalScrollBar.Value
                            - this.verticalScrollBar.LargeChange;
                    }

                    if (val < this.verticalScrollBar.Minimum)
                    {
                        this.verticalScrollBar.Value = this.verticalScrollBar.Minimum;
                    }
                    else if (val > this.verticalScrollBar.Maximum)
                    {
                        this.verticalScrollBar.Value = this.verticalScrollBar.Maximum;
                    }
                    else
                    {
                        this.verticalScrollBar.Value = val;
                    }

                    if (fi.Yposition != this.verticalScrollBar.Value / fi.zoom)
                    {
                        fi.Yposition = this.verticalScrollBar.Value / fi.zoom;
                        IA.ReloadImages(false);
                    }

                    changeXY = true;
                }
            }
        }
        public void HidePropertiesAndFileBrowser_DoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                TifFileInfo fi = null;
                try
                {
                    fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
                }
                catch { return; }
                if (fi == null) { return; }

                double zoom = fi.zoom;
                double X1 = e.X / zoom;
                double Y1 = e.Y / zoom;
                int X = Convert.ToInt32(X1);
                if (X > X1) { X--; }
                int Y = Convert.ToInt32(Y1);
                if (Y > Y1) { Y--; }

                Point p = new Point(X, Y);

                for (int i = 0; i < coRect.Length; i++)
                {
                    for (int j = 0; j < fi.sizeC; j++)
                    {
                        if (colors[j] == true)
                        {
                            if (coRect[i][j].Contains(p) == true)
                            {
                                return;
                            }
                        }
                    }
                }

                CTFileBrowser FileBrowser = IA.FileBrowser;
                TabPageControl TabPages = IA.TabPages;
                Properties.Settings settings = IA.settings;

                int AccInd = FileBrowser.ActiveAccountIndex;

                if (FileBrowser.DataSourcesPanel.Width != 15 | TabPages.propertiesPanel.Width != 15)
                {
                    FileBrowser.DataSourcesPanel.Width = 15;
                    TabPages.propertiesPanel.Width = 15;
                    TabPages.hidePropAndBrows = true;
                }
                else
                {
                    //data source panel
                    FileBrowser.DataSourcesPanelWidth = int.Parse(settings.DataSourcesPanelValues[AccInd]);

                    if (settings.DataSourcesPanelVisible[AccInd] == "y")
                    {
                        FileBrowser.DataSourcesPanel.Width = int.Parse(settings.DataSourcesPanelValues[AccInd]);
                    }
                    else
                    {
                        FileBrowser.DataSourcesPanel.Width = 15;
                    }
                    //Properties panel
                    if (settings.PropertiesPanelVisible[AccInd] == "n")
                    {
                        TabPages.propertiesPanel.Width = 15;
                    }
                    else
                    {
                        TabPages.propertiesPanel.Width = int.Parse(settings.PropertiesPanelWidth[AccInd]);
                    }
                    TabPages.hidePropAndBrows = false;

                    IA.TabPages.Histograms_Reload();
                }
            }
        }
        #endregion
        #region MouseMoveField event

        private bool fieldMove = false;
        private int oldX = 0;
        private int oldY = 0;
        private void GLControl1_MouseDown(object sender, MouseEventArgs e)
        {
            TifFileInfo fi;
            try
            {
                fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
            }
            catch
            {
                return;
            }
            if (fi == null) { return; }

            //if the 3D is enabled - send to imageDrawer_3D
            if (imageDrawer_3D.isImage3D(fi))
            {
                imageDrawer_3D.GLControl1_MouseDown((GLControl)sender, fi, e);
                return;
            }

            if ((e.Button == MouseButtons.Right & Control.ModifierKeys == Keys.Control)
                | e.Button == MouseButtons.Middle)
            {

                fieldMove = true;
                oldX = e.X;
                oldY = e.Y;

                if (this.horizontalScrollBar.Visible == true &
                   this.verticalScrollBar.Visible == true)
                {
                    ((GLControl)sender).Cursor = Cursors.SizeAll;
                }
                else if (this.horizontalScrollBar.Visible == true)
                {
                    ((GLControl)sender).Cursor = Cursors.SizeWE;
                }
                else if (this.verticalScrollBar.Visible == true)
                {
                    ((GLControl)sender).Cursor = Cursors.SizeNS;
                }
            }
        }
        private void GLControl1_MouseMove(object sender, MouseEventArgs e)
        {
            TifFileInfo fi;
            try
            {
                fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
            }
            catch
            {
                return;
            }
            if (fi == null) { return; }

            //if the 3D is enabled - send to imageDrawer_3D
            if (imageDrawer_3D.isImage3D(fi))
            {
                imageDrawer_3D.GLControl1_MouseMove((GLControl)sender, fi, e);
                return;
            }

            if (fieldMove == true & (Control.ModifierKeys == Keys.Control | e.Button == MouseButtons.Middle))
            {
                int X = oldX - e.X;
                int Y = oldY - e.Y;
                changeXY = false;
                //vertical
                if (this.verticalScrollBar.Visible == true)
                {
                    int val = this.verticalScrollBar.Value + Y;// * IA.GLControl1_VerticalPanel.VerticalScroll.SmallChange;
                    oldY = e.Y;
                    if (val < this.verticalScrollBar.Minimum)
                    {
                        this.verticalScrollBar.Value = this.verticalScrollBar.Minimum;
                    }
                    else if (val > this.verticalScrollBar.Maximum)
                    {
                        this.verticalScrollBar.Value = this.verticalScrollBar.Maximum;
                    }
                    else
                    {
                        this.verticalScrollBar.Value = val;
                    }
                }
                //Horizontal
                if (this.horizontalScrollBar.Visible == true)
                {
                    int val = this.horizontalScrollBar.Value + X;// * IA.GLControl1_HorizontalPanel.HorizontalScroll.SmallChange;
                    oldX = e.X;
                    if (val < this.horizontalScrollBar.Minimum)
                    {
                        this.horizontalScrollBar.Value = this.horizontalScrollBar.Minimum;
                    }
                    else if (val > this.horizontalScrollBar.Maximum)
                    {
                        this.horizontalScrollBar.Value = this.horizontalScrollBar.Maximum;
                    }
                    else
                    {
                        this.horizontalScrollBar.Value = val;
                    }
                }

                if (fi.Yposition != this.verticalScrollBar.Value / fi.zoom |
                    fi.Xposition != this.horizontalScrollBar.Value / fi.zoom)
                {
                    fi.Yposition = this.verticalScrollBar.Value / fi.zoom;
                    fi.Xposition = this.horizontalScrollBar.Value / fi.zoom;
                    IA.ReloadImages(false);
                }
                changeXY = true;
            }
            else
            {
                fieldMove = false;
                IA.RoiMan.GlControl_MouseMoveChangeCursor(sender, e);
            }
        }
        private void GLControl1_MouseUp(object sender, MouseEventArgs e)
        {
            TifFileInfo fi;
            try
            {
                fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
            }
            catch
            {
                return;
            }
            if (fi == null) { return; }
            //if the 3D is enabled - send to imageDrawer_3D
            if (imageDrawer_3D.isImage3D(fi))
            {
                imageDrawer_3D.GLControl1_MouseUp((GLControl)sender, fi, e);
                return;
            }

            if (fieldMove == true)
            {
                fieldMove = false;
                oldX = 0;
                oldY = 0;
                IA.RoiMan.GlControl_MouseMoveChangeCursor(sender, e);
            }

        }
        #endregion MouseMoveField event

        #region Draw ROI
        const float DEG2RAD = (float)(3.14159 / 180.0);
        private void drawEllipse(float x, float y, float xradius, float yradius)
        {
            xradius /= 2;
            yradius /= 2;

            x += xradius;
            y += yradius;

            GL.Begin(PrimitiveType.LineLoop);
            GL.Color4(1f, 1f, 0f, 1f);

            for (int i = 0; i < 360; i++)
            {
                //convert degrees into radians
                float degInRad = i * DEG2RAD;
                double newX = x + Math.Cos(degInRad) * xradius;
                double newY = y + Math.Sin(degInRad) * yradius;
                GL.Vertex2(newX, newY);
            }

            GL.End();
        }
        private void drawEllipse(float x, float y, float xradius, float yradius, Rectangle Rect)
        {
            //Check is it outside
            if (!(x < Rect.X + 0.5 | x + xradius > Rect.Width + 0.5 |
                y < Rect.Y + 0.5 | y + yradius > Rect.Height + 0.5))
            {
                drawEllipse(x, y, xradius, yradius);
                return;
            }

            xradius /= 2;
            yradius /= 2;

            x += xradius;
            y += yradius;

            List<float> Xarr = new List<float>();
            List<float> Yarr = new List<float>();

            for (int i = 0; i <= 360; i++)
            {
                //convert degrees into radians
                float degInRad = i * DEG2RAD;
                double newX = x + Math.Cos(degInRad) * xradius;
                double newY = y + Math.Sin(degInRad) * yradius;
                Xarr.Add((float)newX);
                Yarr.Add((float)newY);
            }
            PolygonalFieldCut(Xarr.ToArray(), Yarr.ToArray(), Rect);
        }
        private void fillRectangle(float x, float y, float w, float h, Rectangle Rect)
        {

            RectangleF RectF = new RectangleF(
               (float)Rect.X,
               (float)Rect.Y,
               (float)(Rect.Width + Rect.X),
               (float)(Rect.Height + Rect.Y));

            x += RectF.X + 0.5f;
            y += RectF.Y + 0.5f;

            if (w + x <= RectF.X + 0.5f |
                h + y <= RectF.Y + 0.5f |
                x > RectF.Width |
                y > RectF.Height)
            {
                return;
            }

            w += x;
            h += y;

            if (x <= RectF.X + 0.5f) x = RectF.X + 0.5f;

            if (y <= RectF.Y + 0.5f) y = RectF.Y + 0.5f;

            if (w > RectF.Width)
                w = RectF.Width;

            if (h > RectF.Height)
                h = RectF.Height;

            //fill rectangle
            GL.Begin(PrimitiveType.TriangleStrip);
            GL.Color4(1f, 1f, 1f, 1f);

            GL.Vertex2(x, y);
            GL.Vertex2(x, h);
            GL.Vertex2(w, y);
            GL.Vertex2(w, h);

            GL.End();
            //draw borders
            GL.Begin(PrimitiveType.LineLoop);
            GL.Color4(0f, 0f, 0f, 1f);

            GL.Vertex2(x, y);
            GL.Vertex2(w, y);
            GL.Vertex2(w, h);
            GL.Vertex2(x, h);

            GL.End();
        }
        private void drawRectangle(float x, float y, float w, float h, Rectangle Rect)
        {
            RectangleF RectF = new RectangleF(
               (float)Rect.X,
               (float)Rect.Y,
               (float)(Rect.Width + Rect.X),
               (float)(Rect.Height + Rect.Y));

            if (!(x < RectF.X + 0.5f |
                y < RectF.Y + 0.5f |
                w + x > RectF.Width |
                h + y > RectF.Height))
            {
                drawRectangle(x, y, w, h);
                return;
            }
            else if (w + x < RectF.X + 0.5f |
                h + y < RectF.Y + 0.5f |
                x > RectF.Width |
                y > RectF.Height)
            {
                return;
            }

            w += x;
            h += y;

            float[] Xarr = new float[] { x, w, w, x };
            float[] Yarr = new float[] { y, y, h, h };
            PolygonalFieldCut(Xarr, Yarr, Rect);

        }
        private void drawRectangle(float x, float y, float w, float h)
        {
            w += x;
            h += y;

            GL.Begin(PrimitiveType.LineLoop);
            GL.Color4(1f, 1f, 0f, 1f);

            GL.Vertex2(x, y);
            GL.Vertex2(w, y);
            GL.Vertex2(w, h);
            GL.Vertex2(x, h);

            GL.End();
        }
        private void drawPolygon(float[] x, float[] y)
        {
            GL.Begin(PrimitiveType.LineLoop);
            GL.Color4(1f, 1f, 0f, 1f);

            for (int i = 0; i < x.Length; i++)
                GL.Vertex2(x[i], y[i]);

            GL.End();
        }
        private void drawUnfinishedPolygon(float[] x, float[] y)
        {
            GL.Begin(PrimitiveType.LineStrip);
            GL.Color4(1f, 1f, 0f, 1f);
            for (int i = 0; i < x.Length; i++)
                GL.Vertex2(x[i], y[i]);

            GL.End();
        }

        private List<PointF> DrawLine(PointF p1, PointF p2)
        {
            //Bresenham's line algorithm
            List<PointF> pxlList = new List<PointF>();
            float deltaX = p2.X - p1.X;
            float deltaY = p2.Y - p1.Y;
            double error = -1;

            if (deltaX == 0)
            {
                // || on X axis
                if (p1.Y < p2.Y)
                {
                    for (float Y = p1.Y; Y <= p2.Y; Y++)
                        pxlList.Add(new PointF(p1.X, Y));
                }
                else
                {
                    for (float Y = p2.Y; Y <= p1.Y; Y++)
                        pxlList.Add(new PointF(p1.X, Y));
                }

            }
            else if (deltaY == 0)
            {
                // || on Y axis
                if (p1.X < p2.X)
                {
                    for (float X = p1.X; X <= p2.X; X++)
                        pxlList.Add(new PointF(X, p1.Y));
                }
                else
                {
                    for (float X = p2.X; X <= p1.X; X++)
                        pxlList.Add(new PointF(X, p1.Y));
                }

            }
            else
            {
                double deltaErr = deltaY / deltaX;
                //find wich case is our line
                int case1 = 0;
                if (deltaErr > 0 & deltaErr <= 1)
                {
                    if (deltaX > 0)
                        case1 = 0;
                    else
                        case1 = 4;
                }
                else if (deltaErr > 1)
                {
                    if (deltaX > 0)
                        case1 = 1;
                    else
                        case1 = 5;
                }
                else if (deltaErr < 0 & deltaErr >= -1)
                {
                    if (deltaX > 0)
                        case1 = 7;
                    else
                        case1 = 3;
                }
                else if (deltaErr < -1)
                {
                    if (deltaX > 0)
                        case1 = 6;
                    else
                        case1 = 2;
                }


                //select case x,y
                float x0;
                float y0;
                float x1;
                float y1;

                switch (case1)
                {
                    //case 0:
                    default:
                        x0 = p1.X;
                        y0 = p1.Y;
                        x1 = p2.X;
                        y1 = p2.Y;
                        break;
                    case 1:
                        x0 = p1.Y;
                        y0 = p1.X;
                        x1 = p2.Y;
                        y1 = p2.X;
                        break;
                    case 2:
                        x0 = p1.Y;
                        y0 = -p1.X;
                        x1 = p2.Y;
                        y1 = -p2.X;
                        break;
                    case 3:
                        x0 = -p1.X;
                        y0 = p1.Y;
                        x1 = -p2.X;
                        y1 = p2.Y;
                        break;
                    case 4:
                        x0 = -p1.X;
                        y0 = -p1.Y;
                        x1 = -p2.X;
                        y1 = -p2.Y;
                        break;
                    case 5:
                        x0 = -p1.Y;
                        y0 = -p1.X;
                        x1 = -p2.Y;
                        y1 = -p2.X;
                        break;
                    case 6:
                        x0 = -p1.Y;
                        y0 = p1.X;
                        x1 = -p2.Y;
                        y1 = p2.X;
                        break;
                    case 7:
                        x0 = p1.X;
                        y0 = -p1.Y;
                        x1 = p2.X;
                        y1 = -p2.Y;
                        break;
                }
                //calculate new values
                deltaX = x1 - x0;
                deltaY = y1 - y0;
                deltaErr = Math.Abs(deltaY / deltaX);

                //Assume deltax != 0 (line is not vertical),
                //note that this division needs to be done in a way that preserves the fractional part
                float y = y0;
                double error1 = -1;
                for (float x = x0; x < x1; x++)
                {
                    switch (case1)
                    {
                        //case 0:
                        default:
                            pxlList.Add(new PointF(x, y));
                            break;
                        case 1:
                            pxlList.Add(new PointF(y, x));
                            break;
                        case 2:
                            pxlList.Add(new PointF(-y, x));
                            break;
                        case 3:
                            pxlList.Add(new PointF(-x, y));
                            break;
                        case 4:
                            pxlList.Add(new PointF(-x, -y));
                            break;
                        case 5:
                            pxlList.Add(new PointF(-y, -x));
                            break;
                        case 6:
                            pxlList.Add(new PointF(y, -x));
                            break;
                        case 7:
                            pxlList.Add(new PointF(x, -y));
                            break;
                    }

                    error1 += deltaErr;
                    if (error1 >= 0.0)
                    {
                        y++;
                        error1 -= 1.0;
                    }
                }
            }

            return pxlList;
        }
        private bool RectFContains(PointF p, RectangleF rectF)
        {
            if (p.X < rectF.X | p.X > rectF.Width |
                p.Y < rectF.Y | p.Y > rectF.Height)
                return false;
            else
                return true;
        }
        private void drawUnfinishedPolygon(List<PointF> points)
        {
            GL.Begin(PrimitiveType.LineStrip);
            GL.Color4(1f, 1f, 0f, 1f);
            for (int i = 0; i < points.Count; i++)
                GL.Vertex2(points[i].X, points[i].Y);

            GL.End();
        }
        private void PolygonalFieldCut(float[] X, float[] Y, Rectangle Rect)
        {
            #region Variables
            //Create actual points rectangleF
            RectangleF RectF = new RectangleF(
                (float)Rect.X,
                (float)Rect.Y,
                (float)(Rect.Width + Rect.X),
                (float)(Rect.Height + Rect.Y));

            List<PointF> resP = new List<PointF>();
            List<PointF> potP;

            PointF p0 = new PointF(X[X.Length - 1], Y[Y.Length - 1]);
            PointF p1;
            bool drawn = false;

            bool visible;
            bool contain;//bool that shows is the point in rectF
            PointF prevP;//the one before the last selected
            PointF lastVisibleP;
            #endregion Variables

            for (int i = 0; i <= X.Length; i++)
            {
                lastVisibleP = PointF.Empty;
                visible = false;
                //set cur point
                if (i < X.Length)
                    p1 = new PointF(X[i], Y[i]);
                else
                    p1 = new PointF(X[0], Y[0]);
                //check is border visible
                if (RectFContains(p0, RectF))
                {
                    resP.Add(p0);
                    visible = true;
                }
                else
                {
                    drawn = true;
                }
                //calculate potPoint
                potP = DrawLine(p0, p1);//Calculates all potential points
                prevP = p0;//the one before the last selected
                foreach (PointF p in potP)
                {
                    //check is point visible
                    contain = RectFContains(p, RectF);

                    if (contain == true)
                        lastVisibleP = p;

                    if (contain != visible)
                    {
                        visible = contain;
                        if (contain == true)
                        {
                            resP.Add(p);
                        }
                        else
                        {
                            resP.Add(prevP);
                            drawUnfinishedPolygon(resP);
                            resP.Clear();
                            drawn = true;
                        }
                    }
                    //set prev point
                    prevP = p;
                }
                //finish the list
                if (RectFContains(p1, RectF))
                    resP.Add(p1);
                else if (lastVisibleP.IsEmpty == false)
                    resP.Add(lastVisibleP);

                drawUnfinishedPolygon(resP);
                //set old point
                p0 = new PointF(p1.X, p1.Y);
                resP.Clear();
            }

            if (drawn == false)
            {
                drawPolygon(X, Y);
            }

        }       
        private void drawCurrentStackRoi(ROI roi, int frame, int addX, int addY, Rectangle rect)
        {
            if (roi.Stack < 1) return;

            if (roi.Shape == 0 | roi.Shape == 1)
            {
                Point p = roi.GetLocation(frame)[0];

                float X = p.X + addX + 0.5f;
                float Y = p.Y + addY + 0.5f;
                float W = roi.Width;
                float H = roi.Height;

                for (int i = 0; i < roi.Stack; i++)
                {
                    X -= roi.D;
                    Y -= roi.D;
                    W += roi.D + roi.D;
                    H += roi.D + roi.D;

                    if (roi.Shape == 0)
                        drawRectangle(X, Y, W, H, rect);
                    else
                        drawEllipse(X, Y, W, H, rect);
                }

                float[] x = new float[2];
                float[] y = new float[2];

                //top
                x[0] = X + (W / 2);
                y[0] = Y;
                x[1] = x[0];
                y[1] = p.Y + addY + 0.5f;

                PolygonalFieldCut(x, y, rect);
                //bot
                x[0] = X + (W / 2);
                y[0] = Y + H;
                x[1] = x[0];
                y[1] = p.Y + roi.Height + addY + 0.5f;

                PolygonalFieldCut(x, y, rect);
                //left
                x[0] = X;
                y[0] = Y + (H / 2);
                x[1] = p.X + addX + 0.5f;
                y[1] = y[0];

                PolygonalFieldCut(x, y, rect);

                //right
                x[0] = X + W;
                y[0] = Y + (H / 2);
                x[1] = p.X + roi.Width + addX + 0.5f;
                y[1] = y[0];

                PolygonalFieldCut(x, y, rect);
            }
            else if (roi.Shape == 2 || roi.Shape == 3 || roi.Shape == 4 || roi.Shape == 5)
            {
                if (IA.RoiMan.DrawNewRoiMode == true && roi == IA.RoiMan.current) return;
                float[] x, y;
                for (int i = 0, D = roi.D; i < roi.Stack; i++, D += roi.D)
                {
                    List<Point> res = RoiMeasure.Polygon_Layers(frame, D, roi, rect);

                    x = new float[res.Count];
                    y = new float[res.Count];

                    for (int i1 = 0; i1 < res.Count; i1++)
                    {
                        Point p = res[i1];
                        x[i1] = p.X + addX + 0.5f;
                        y[i1] = p.Y + addY + 0.5f;
                    }

                    PolygonalFieldCut(x, y, rect);
                }
            }
        }
        private void drawCurrentRoi(TifFileInfo fi)
        {
            if (fi.tpTaskbar.MethodsBtnList[0].ImageIndex == 1) return;
            ROI roi = IA.RoiMan.current;
            if (roi == null) return;

            Rectangle rect = coRect[0][fi.cValue];
            int addX = rect.X;
            int addY = rect.Y;

            FrameCalculator FC = new FrameCalculator();
            int frame = FC.Frame(fi);

            if (fi.frame + 1 < roi.FromT | fi.frame + 1 > roi.ToT) return;
            if (fi.zValue + 1 < roi.FromZ | fi.zValue + 1 > roi.ToZ) return;
            if (roi.Checked == false) return;

            if (roi.Shape == 0)
            {
                Point p = roi.GetLocation(frame)[0];
                drawRectangle(p.X + addX + 0.5f, p.Y + addY + 0.5f, roi.Width, roi.Height, rect);
            }
            else if (roi.Shape == 1)
            {
                Point p = roi.GetLocation(frame)[0];
                drawEllipse(p.X + addX + 0.5f, p.Y + addY + 0.5f, roi.Width, roi.Height, rect);
            }
            else if (roi.Shape == 2)
            {
                Point[] points = roi.GetLocation(frame);
                if (IA.RoiMan.DrawNewRoiMode == true)
                {
                    float[] x = new float[points.Length + 1];
                    float[] y = new float[points.Length + 1];
                    for (int i = 0; i < points.Length; i++)
                    {
                        Point p = points[i];
                        x[i] = p.X + addX + 0.5f;
                        y[i] = p.Y + addY + 0.5f;
                    }
                    x[x.Length - 1] = IA.RoiMan.lastPoint.X + addX + 0.5f;
                    y[y.Length - 1] = IA.RoiMan.lastPoint.Y + addY + 0.5f;
                    drawUnfinishedPolygon(x, y);
                    //draw finish polygon
                    float lineWidth = (float)(3 / fi.zoom);
                    if (lineWidth < 1f) { lineWidth = 1f; }
                    RectangleF rect1 = new RectangleF(
                        (points[0].X - lineWidth + addX),
                        (points[0].Y - lineWidth + addY),
                        (lineWidth + lineWidth),
                        (lineWidth + lineWidth));
                    drawRectangle(rect1.X, rect1.Y, rect1.Width, rect1.Height);
                }
                else
                {
                    float[] x = new float[points.Length];
                    float[] y = new float[points.Length];
                    for (int i = 0; i < points.Length; i++)
                    {
                        Point p = points[i];
                        x[i] = p.X + addX + 0.5f;
                        y[i] = p.Y + addY + 0.5f;
                    }
                    //drawPolygon(x, y);
                    PolygonalFieldCut(x, y, rect);
                }
            }
            else if (roi.Shape == 3)
            {
                Point[] points = roi.GetLocation(frame);
                float[] x = new float[points.Length];
                float[] y = new float[points.Length];
                for (int i = 0; i < points.Length; i++)
                {
                    Point p = points[i];
                    x[i] = p.X + addX + 0.5f;
                    y[i] = p.Y + addY + 0.5f;
                }

                if (IA.RoiMan.DrawNewRoiMode == true)
                {
                    drawUnfinishedPolygon(x, y);
                }
                else
                {
                    //drawPolygon(x, y);
                    PolygonalFieldCut(x, y, rect);
                }
            }
            //draw stack roi
            drawCurrentStackRoi(roi, frame, addX, addY, rect);
            //draw number
            if (IA.RoiMan.roiTV.Nodes.IndexOf(roi) > -1)
                DrawStringToGL(fi, (IA.RoiMan.roiTV.Nodes.IndexOf(roi) + 1).ToString(),
                    roi, frame, rect);
            //draw resize rectangles
            if (IA.RoiMan.DrawNewRoiMode == false)
            {
                IA.RoiMan.PrepareResizeSpotsRectangle(fi, frame);

                if (IA.RoiMan.ResizeSpotsRectangles != null)
                {
                    for (int i = 0; i < IA.RoiMan.ResizeSpotsRectangles.Length; i++)
                    {
                        RectangleF curRect = IA.RoiMan.ResizeSpotsRectangles[i];
                        fillRectangle(curRect.X, curRect.Y, curRect.Width, curRect.Height, rect);
                    }
                }
            }
        }
        private void drawRoi(TifFileInfo fi)
        {
            if (fi.tpTaskbar.MethodsBtnList[0].ImageIndex == 1) return;
            for (int col = 0; col < fi.sizeC; col++)
            {
                if (fi.tpTaskbar.ColorBtnList[col].ImageIndex == 1) continue;

                List<ROI> roiList = fi.roiList[col];
                if (roiList == null) continue;

                foreach (ROI roi in roiList)
                {
                    if (roi == null) continue;
                    if (roi == IA.RoiMan.current) continue;

                    Rectangle rect = coRect[0][col];
                    int addX = rect.X;
                    int addY = rect.Y;

                    FrameCalculator FC = new FrameCalculator();
                    int frame = FC.FrameC(fi, col);

                    if (fi.frame + 1 < roi.FromT | fi.frame + 1 > roi.ToT) continue;
                    if (fi.zValue + 1 < roi.FromZ | fi.zValue + 1 > roi.ToZ) continue;
                    if (roi.Checked == false) continue;

                    if (roi.Shape == 0)
                    {
                        Point p = roi.GetLocation(frame)[0];
                        drawRectangle(p.X + addX + 0.5f, p.Y + addY + 0.5f, roi.Width, roi.Height, rect);
                    }
                    else if (roi.Shape == 1)
                    {
                        Point p = roi.GetLocation(frame)[0];
                        drawEllipse(p.X + addX + 0.5f, p.Y + addY + 0.5f, roi.Width, roi.Height, rect);
                    }
                    else if (roi.Shape == 2)
                    {
                        Point[] points = roi.GetLocation(frame);

                        float[] x = new float[points.Length];
                        float[] y = new float[points.Length];
                        for (int i = 0; i < points.Length; i++)
                        {
                            Point p = points[i];
                            x[i] = p.X + addX + 0.5f;
                            y[i] = p.Y + addY + 0.5f;
                        }
                        //drawPolygon(x, y);
                        PolygonalFieldCut(x, y, rect);
                    }
                    else if (roi.Shape == 3)
                    {
                        Point[] points = roi.GetLocation(frame);
                        float[] x = new float[points.Length];
                        float[] y = new float[points.Length];
                        for (int i = 0; i < points.Length; i++)
                        {
                            Point p = points[i];
                            x[i] = p.X + addX + 0.5f;
                            y[i] = p.Y + addY + 0.5f;
                        }

                        PolygonalFieldCut(x, y, rect);
                    }
                    //draw stack roi
                    drawCurrentStackRoi(roi, frame, addX, addY, rect);
                    DrawStringToGL(fi, (roiList.IndexOf(roi) + 1).ToString(), roi, frame, rect);
                }
            }
        }
        public void tryDrawingWithoutReload()
        {
            if (IA.RoiMan.current == null) return;

            GLControl GLControl1 = IA.GLControl1;
            GLDrawing_Start(GLControl1, false);
            /*
            TifFileInfo fi;
            try
            {
                fi = IA.TabPages.TabCollections[IA.TabPages.SelectedIndex].tifFI;
            }
            catch { return; }
            if (fi == null) return;

            //Activate Control
            GLControl1.MakeCurrent();
            //Start Drawing
           
            // Drawing the saved screen texture back to the screen:
            DrawTexture(fi);

            //Draw rois
            drawRoi(fi);
            drawCurrentRoi(fi);

            IA.RoiMan.fillTextBox(fi);

            GLControl1.SwapBuffers();*/
        }
       
        private void DrawTexture(TifFileInfo fi)
        {
            GLControl GLControl1 = IA.GLControl1;

            Rectangle rectOld = coRect[0][fi.cValue];
            Rectangle rect = new Rectangle(rectOld.X, rectOld.Y,
                rectOld.X + rectOld.Width, rectOld.Y + rectOld.Height);

            //start drawing
            GL.Begin(PrimitiveType.Quads);

            GL.Color4(0f, 0f, 0f, 1f);

            GL.Vertex2(rect.X, rect.Y);
            GL.Vertex2(rect.X, rect.Height);
            GL.Vertex2(rect.Width, rect.Height);
            GL.Vertex2(rect.Width, rect.Y);

            GL.End();

            GL.Enable(EnableCap.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, IA.IDrawer.imagesTextures.GetID(fi.cValue));

            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.One);
            GL.Enable(EnableCap.Blend);
            GL.ShadeModel(ShadingModel.Flat);

            GL.Begin(PrimitiveType.Quads);

            //GL.Color3(fi.LutList[fi.cValue]);
            GL.Color3(Color.White);

            GL.TexCoord2(0, 0);
            GL.Vertex2(rect.X, rect.Y);

            GL.TexCoord2(0, 1);
            GL.Vertex2(rect.X, rect.Height);

            GL.TexCoord2(1, 1);
            GL.Vertex2(rect.Width, rect.Height);

            GL.TexCoord2(1, 0);
            GL.Vertex2(rect.Width, rect.Y);

            GL.End();

            GL.Disable(EnableCap.Texture2D);

            GL.Disable(EnableCap.Blend);
        }
        public void DrawStringToGL(TifFileInfo fi, string str, ROI roi, int imageN, Rectangle Borders)
        {
            if (IA.RoiMan.showLabels == false) return;

            int symb = str.Length;
            float W = 13f / (float)fi.zoom;
            float H = 15f / (float)fi.zoom;
            float lineSpace = 7 / (float)fi.zoom;

            PointF midP = roi.GetMidPoint(imageN);
            float X = Borders.X + midP.X - (lineSpace * (symb / 2)) - 3 / (float)fi.zoom;
            float Y = Borders.Y + midP.Y - (H / 2);

            GLControl GLControl1 = IA.GLControl1;

            GL.Enable(EnableCap.Blend);
            GL.Enable(EnableCap.Texture2D);

            RectangleF rect = new RectangleF(X, Y,
               X + W, Y + H);

            RectangleF BordersF =
                new RectangleF(Borders.X, Borders.Y, Borders.Width, Borders.Height);

            foreach (char val in str)
            {
                if (BordersF.Contains(new PointF(rect.X, rect.Y)) &
                    BordersF.Contains(new PointF(rect.Width, rect.Height)))
                {
                    int code = ImageTexture.NumberID[int.Parse(val.ToString())];

                    GL.BindTexture(TextureTarget.Texture2D, code);

                    GL.Begin(PrimitiveType.Quads);

                    GL.Color3(Color.Transparent);

                    GL.TexCoord2(0, 0);
                    GL.Vertex2(rect.X, rect.Y);

                    GL.TexCoord2(0, 1);
                    GL.Vertex2(rect.X, rect.Height);

                    GL.TexCoord2(1, 1);
                    GL.Vertex2(rect.Width, rect.Height);

                    GL.TexCoord2(1, 0);
                    GL.Vertex2(rect.Width, rect.Y);

                    GL.End();
                }

                rect.X += lineSpace;
                rect.Width = rect.X + W;
            }
            GL.Disable(EnableCap.Texture2D);
            GL.Disable(EnableCap.Blend);
        }

        #endregion Draw ROI
    }
    class ContentPipe
    {
        #region Number Textures
        public int[] NumberID = null;
        public void GenerateNumberTextures()
        {
            NumberID = new int[10];
            for (int i = 0; i < NumberID.Length; i++)
            {
                int code = GL.GenTexture();
                NumberID[i] = code;
                //generate number bitmap
                Bitmap bmp = BitmapFromString(i.ToString());
                //create texture
                LoadNumberTexture(bmp, code);
            }
        }
        private Bitmap BitmapFromString(string str)
        {
            Font font = new Font("Times New Roman", 9, FontStyle.Bold);

            Bitmap bmp = new Bitmap(TextRenderer.MeasureText(str, font).Width,
                TextRenderer.MeasureText(str, font).Height,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            Rectangle rect = new Rectangle(0, 0,
                TextRenderer.MeasureText(str, font).Width,
                TextRenderer.MeasureText(str, font).Height);
            
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.FillRectangle(Brushes.Transparent, rect);
                g.DrawString(str, font, Brushes.Yellow, rect);
                g.Flush();
            }
            return bmp;
        }
        private void LoadNumberTexture(Bitmap bmp, int i)
        {
            //Load texture from file
            Bitmap texture_source = bmp;

            //Link empty texture to texture2d
            GL.BindTexture(TextureTarget.Texture2D, i);

            //Lock pixel data to memory and prepare for pass through
            BitmapData bitmap_data = texture_source.LockBits(
                new Rectangle(0, 0, texture_source.Width,
                texture_source.Height), ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppRgb);

            //Tell gl to write the data from are bitmap image/data to the bound texture
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, texture_source.Width, texture_source.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bitmap_data.Scan0);

            //Release from memory
            texture_source.UnlockBits(bitmap_data);
            //SetUp parametars
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Linear);
        }
        #endregion Number Textures                
        //Generate empty texture           
        private int ChartID;
        public void ReserveTextureID()
        {
            ChartID = GL.GenTexture();
        }
        public int LoadTexture(Bitmap bmp, bool NoAntiAliasing = false)
        {
            //Load texture from file
            Bitmap texture_source = bmp;

            //Link empty texture to texture2d
            GL.BindTexture(TextureTarget.Texture2D, ChartID);

            //Lock pixel data to memory and prepare for pass through
            BitmapData bitmap_data = texture_source.LockBits(
                new Rectangle(0, 0, texture_source.Width,
                texture_source.Height), ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppRgb);

            //Tell gl to write the data from are bitmap image/data to the bound texture
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, texture_source.Width, texture_source.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bitmap_data.Scan0);
            //Release from memory
            texture_source.UnlockBits(bitmap_data);
            //SetUp parametars
            if (NoAntiAliasing)
            {
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Nearest);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Nearest);

            }
            else
            {
                //glTexParameteri(GL_TEXTURE_2D, GL_GENERATE_MIPMAP_SGIS, GL_TRUE);
                //GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.GenerateMipmapSgis, (float)All.True);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Linear);

            }
            return ChartID;
        }      
    }
}
