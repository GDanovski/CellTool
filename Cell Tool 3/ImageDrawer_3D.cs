using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Diagnostics;
using System.ComponentModel;
using System.Collections.Generic;

namespace Cell_Tool_3
{
    /// <summary>
    /// Includes events for 3D visualization
    /// </summary>
    class ImageDrawer_3D
    {
        private int shearX = 0; // X displacement - the angle of rotation around the Y axis in degrees
        private int shearY = 0; // Y displacement - the angle of rotation around the X axis in degrees
        int zLimit = 0; // the minimum Z slice to display for the moving plane
        int downsample_factor = 1; // in case of big images
        int time_frame;
        int leftMargin = 10;
        int topMargin = 10;
        public ImageDrawer IDrawer;
        int maxSize;
        Cube cube1;
        SphereROI CurrentSphereRoi = null;
        List<SphereROI> ROIs;
        bool DisplayROIs;
        bool initialized; // don't call Init more than once TODO

        ushort[] rotated_image;


        GPU_Processing gpu; // rotation can be performed on the GPU
        ushort[] image1d_C0, image1d_C1; // the 1d representations of the 2 channel raw images

        // For 3D plots
        CTChart_3D plt;
        TifFileInfo fi;

        // For the 3D plot options
        public PropertiesPanel_Item PropPanel;


        private Image3DProjection proj3d; // holds configurations

        private Vector2 MousePosition = new Vector2(-1, -1);


        public ImageDrawer_3D(ImageDrawer IDRawer) { this.IDrawer = IDRawer; }
        // Checks is the image 3D
        public bool isImage3D(TifFileInfo fi) { return fi.is3D; }

        // Draw the image      
        public void StartDrawing(GLControl GLcontrol1, TifFileInfo incoming_fi)
        {
            if (incoming_fi == null) return;
            if (this.fi == null) this.fi = incoming_fi; // proj3d.CopyFi(incoming_fi);
            if (!initialized) initProgram(GLcontrol1, incoming_fi);

            if (fi.frame != this.time_frame)
            {
                this.time_frame = fi.frame;
                this.image1d_C0 = proj3d.GetImage1D(0, 0, fi.frame);
                this.image1d_C1 = fi.sizeC > 1 ? proj3d.GetImage1D(0, 1, fi.frame) : null;
                this.gpu.Cleanup();
                this.gpu = new GPU_Processing(image1d_C0, image1d_C1, proj3d.GetSegmentedImage());

                // Update the 3D plot
                double[,] identity = new double[3, 3] { { 1, 0, 0 }, { 0, 1, 0 }, { 0, 0, 1 } };
                plt.Properties3D.ImageProjection_1d = RotateImage_CPU(identity, 1, 0);
                plt.Properties3D.Refresh(fi);

                // if (CurrentSphereRoi != null) CurrentSphereRoi.old_center = CurrentSphereRoi.center;
            }

            // Adjust the sizes in case of downsampling
            if (proj3d.factor == 1)
            {
                proj3d.sizeX = fi.sizeX;
                proj3d.sizeY = fi.sizeY;
                proj3d.sizeZ = fi.sizeZ;
            }
            else
            {
                proj3d.sizeX = fi.sizeX / proj3d.factor;
                proj3d.sizeY = fi.sizeY / proj3d.factor;
                proj3d.sizeZ = fi.sizeZ / proj3d.factor;
            }

            //Activate Control

            GLcontrol1.MakeCurrent();
            GL.ClearColor(0.2f, 0.2f, 0.2f, 0.2f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            UpdateRotationAngles(ref shearX);
            UpdateRotationAngles(ref shearY);


            // Draw the raw image for all selected channels
            if (fi.tpTaskbar.MethodsBtnList[0].ImageIndex == 0) DrawRotatedView(GLcontrol1, fi, 0);

            // Draw the filtered image for the currently selected channel
            if (fi.tpTaskbar.MethodsBtnList[1].ImageIndex == 0) DrawRotatedView(GLcontrol1, fi, 1);

            // Draw the plot
            if (fi.tpTaskbar.MethodsBtnList[2].ImageIndex == 0) DrawRotatedView(GLcontrol1, fi, 2);

            if (CurrentSphereRoi != null && DisplayROIs == true)
            {
                CurrentSphereRoi.Display(shearX, shearY, leftMargin, topMargin, maxSize, -1);
                /* Implementation in proress
                
                ushort[][] image = new ushort[maxSize][];
                int idx = 0;

                for (int y = 0; y < maxSize; y++)
                {
                    image[y] = new ushort[maxSize];
                    Array.Copy(rotated_image, idx, image[y], 0, maxSize);
                    idx += maxSize;
                }
                MagicWandRoi(image, fi.thresholdColors[0][1]);
                */

            }

            GL.Disable(EnableCap.DepthTest);


            GL.Flush();
            GLcontrol1.SwapBuffers();

        }
        private void DrawRotatedView(GLControl GLcontrol1, TifFileInfo incoming_fi, int numRectangle)
        {
            // Create the combined rotation matrix from the X and Y angles
            float X_rad = (float)(shearX * Math.PI / 180f);
            float Y_rad = (float)(shearY * Math.PI / 180f);

            float sin = (float)Math.Sin(Y_rad);
            float cos = (float)Math.Cos(Y_rad);

            double[,] rotationX = new double[3, 3] { { 1, 0, 0 }, { 0, cos, sin }, { 0, -sin, cos } };

            sin = (float)Math.Sin(X_rad);
            cos = (float)Math.Cos(X_rad);

            double[,] rotationY = new double[3, 3] { { cos, 0, -sin }, { 0, 1, 0 }, { sin, 0, cos } };

            double[,] rotation = Accord.Math.Matrix.Multiply(rotationX, rotationY);

            if (numRectangle == 0) LoadTexture(RawToBmpComposite(rotation, maxSize));
            else if (numRectangle == 1) LoadTexture(RawToBmpSegmented(rotation));
            else if (numRectangle == 2)
            {
                UpdateRotationAngles(ref plt.rotationX);
                UpdateRotationAngles(ref plt.rotationY);

                GL.DeleteTexture(0);

                plt.XaxisData = new double[fi.sizeT];
                for (int t = 0; t < fi.sizeT; t++)
                {
                    plt.XaxisData[t] = t;
                }

                plt.Properties3D.rois = ROIs;
                if (CurrentSphereRoi == null || CurrentSphereRoi.ROI2d == null) return;

                //plt.Properties3D.Generate2D_5(update: false);
                plt.Properties3D.Refresh(fi);
                plt.Render();
                return;
            }
            else return;

            double angleX = shearX * Math.PI / 180f;
            double angleY = shearY * Math.PI / 180f;

            double cubeRotationX = angleY;
            double cubeRotationY = angleX;


            GL.Enable(EnableCap.Texture2D);
            GL.Begin(PrimitiveType.Quads);

            GL.TexCoord2(0, 0);
            GL.Vertex2(leftMargin + numRectangle * (leftMargin + maxSize), topMargin);

            GL.TexCoord2(0, 1);
            GL.Vertex2(leftMargin + numRectangle * (leftMargin + maxSize), topMargin + maxSize);

            GL.TexCoord2(1, 1);
            GL.Vertex2((numRectangle + 1) * (leftMargin + maxSize), topMargin + maxSize);

            GL.TexCoord2(1, 0);
            GL.Vertex2((numRectangle + 1) * (leftMargin + maxSize), topMargin);

            GL.End();

            GL.Disable(EnableCap.Texture2D);

            cube1.Show(cubeRotationX, cubeRotationY, numRectangle, leftMargin, topMargin, maxSize);

            GL.ClearColor(0.2f, 0.2f, 0.2f, 0.2f);

            UpdateRoiList();
            if (DisplayROIs == true && numRectangle == 0)
            {
                LoadExistingRois();
                DisplayAllRoiPaths();
            }

            if (proj3d.drawPlane) drawPlane(zLimit, maxSize, leftMargin, topMargin);
        }

        private void DisplayAllRoiPaths()
        {
            PathColor Colors = new PathColor();
            foreach (SphereROI roi in ROIs)
            {
                if (roi != null)
                {
                    roi.Display(shearX, shearY, leftMargin, topMargin, maxSize, fi.frame);

                    // If a motion plot is chosen, display the ROI paths in the 3D plot regions;
                    // else, display them on top of the raw image itself.
                    if (plt.Properties3D.PlotType == Plots.Motion)
                    {

                        GL.Color3(Colors.NextColor());
                        roi.DisplayPath(plt.rotationX, plt.rotationY,
                            3 * leftMargin + 2 * maxSize, topMargin, maxSize, fi.frame);
                    }
                    else
                        roi.DisplayPath(shearX, shearY, leftMargin, topMargin, maxSize, fi.frame);
                }
            }
        }
        public void LoadTexture(Bitmap bmp, bool NoAntiAliasing = false)
        {
            //Load texture from file
            Bitmap texture_source = bmp;

            GL.DeleteTexture(0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.Color3(Color.White);

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
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Nearest);
        }
        private Bitmap RawToBmpSegmented(double[,] rotation)
        {
            int C = fi.cValue;

            ushort[] image;

            if (proj3d.gpu) image = RotateImage_GPU(rotation, 1, C);
            else image = RotateImage_CPU(rotation, 1, C);

            this.rotated_image = image;

            //new bitmap
            Bitmap bmp = new Bitmap(maxSize, maxSize,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            // Lock the bitmap's bits.
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadWrite, bmp.PixelFormat);
            // Get the address of the first line.
            IntPtr ptr = bmpData.Scan0;
            //store rgb values
            int bytes = Math.Abs(bmpData.Stride) * bmp.Height;
            byte[] rgbValues = new byte[bytes];
            // Copy the RGB values into the array
            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);

            //take LUT info
            Parallel.For(0, maxSize, i =>
            {
                int position = i * bmpData.Stride;

                for (int j = 0; j < maxSize; j++)
                {
                    int val = image[i * maxSize + j];
                    byte val1 = (byte)(fi.adjustedLUT[fi.cValue][val] * 255);

                    if (val < fi.thresholdValues[C][1])
                    {
                        rgbValues[position] = 0;
                        position++;
                        rgbValues[position] = 0;
                        position++;
                        rgbValues[position] = 0;
                        position++;
                        rgbValues[position] = 255;
                        position++;
                    }
                    else if (fi.thresholdValues[C][2] == 0 || val < fi.thresholdValues[C][2])
                    {

                        rgbValues[position] = (byte)(fi.thresholdColors[C][1].B);
                        position++;
                        rgbValues[position] = (byte)(fi.thresholdColors[C][1].G);
                        position++;
                        rgbValues[position] = (byte)(fi.thresholdColors[C][1].R);
                        position++;
                        rgbValues[position] = 255;
                        position++;
                    }
                    else if (fi.thresholdValues[C][3] == 0 || val < fi.thresholdValues[C][3])
                    {

                        rgbValues[position] = (byte)(fi.thresholdColors[C][2].B);
                        position++;
                        rgbValues[position] = (byte)(fi.thresholdColors[C][2].G);
                        position++;
                        rgbValues[position] = (byte)(fi.thresholdColors[C][2].R);
                        position++;
                        rgbValues[position] = 255;
                        position++;
                    }
                    else if (fi.thresholdValues[C][4] == 0 || val < fi.thresholdValues[C][4])
                    {

                        rgbValues[position] = (byte)(fi.thresholdColors[C][3].B);
                        position++;
                        rgbValues[position] = (byte)(fi.thresholdColors[C][3].G);
                        position++;
                        rgbValues[position] = (byte)(fi.thresholdColors[C][3].R);
                        position++;
                        rgbValues[position] = 255;
                        position++;
                    }
                    else
                    {

                        rgbValues[position] = (byte)(fi.thresholdColors[C][2].B);
                        position++;
                        rgbValues[position] = (byte)(fi.thresholdColors[C][2].G);
                        position++;
                        rgbValues[position] = (byte)(fi.thresholdColors[C][2].R);
                        position++;
                        rgbValues[position] = 255;
                        position++;
                    }
                }
            });
            // Copy the RGB values back to the bitmap
            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, bytes);
            // Unlock the bits.
            bmp.UnlockBits(bmpData);
            //return results
            return bmp;
        }
        private void drawPlane(int z, int maxSize, int leftMargin, int topMargin)
        {

            GL.Color3(Color.White);
            GL.Begin(PrimitiveType.LineLoop);

            Point3d p1 = new Point3d(-0.4, -0.4, (float)z / proj3d.sizeZ - (0.5 * (float)proj3d.sizeZ / maxSize))
                .RotateX(shearY * Math.PI / 180).RotateY(shearX * Math.PI / 180f);

            Point3d p2 = new Point3d(-0.4, 0.4, (float)z / proj3d.sizeZ - (0.5 * (float)proj3d.sizeZ / maxSize))
                .RotateX(shearY * Math.PI / 180f).RotateY(shearX * Math.PI / 180f);

            Point3d p3 = new Point3d(0.4, 0.4, (float)z / proj3d.sizeZ - (0.5 * (float)proj3d.sizeZ / maxSize))
                .RotateX(shearY * Math.PI / 180f).RotateY(shearX * Math.PI / 180f);

            Point3d p4 = new Point3d(0.4, -0.4, (float)z / proj3d.sizeZ - (0.5 * (float)proj3d.sizeZ / maxSize))
                .RotateX(shearY * Math.PI / 180f).RotateY(shearX * Math.PI / 180f);

            GL.Vertex2(leftMargin + maxSize * (p1.X + 0.5f), topMargin + maxSize * (p1.Y + 0.5f));
            GL.Vertex2(leftMargin + maxSize * (p2.X + 0.5f), topMargin + maxSize * (p2.Y + 0.5f));
            GL.Vertex2(leftMargin + maxSize * (p3.X + 0.5f), topMargin + maxSize * (p3.Y + 0.5f));
            GL.Vertex2(leftMargin + maxSize * (p4.X + 0.5f), topMargin + maxSize * (p4.Y + 0.5f));

            GL.End();
        }
        private Bitmap RawToBmpComposite(double[,] rotation, int maxSize)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            //image array
            ushort[] image0, image1 = null;

            //new bitmap
            Bitmap bmp = new Bitmap(maxSize, maxSize,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            // Lock the bitmap's bits.
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadWrite, bmp.PixelFormat);

            // Get the address of the first line.
            IntPtr ptr = bmpData.Scan0;

            //store rgb values
            int bytes = Math.Abs(bmpData.Stride) * bmp.Height;
            byte[] rgbValues = new byte[bytes];

            bool drawC0 = (fi.tpTaskbar.ColorBtnList[0].ImageIndex == 0);
            bool drawC1 = (fi.sizeC > 1 && fi.tpTaskbar.ColorBtnList[1].ImageIndex == 0);

            if (proj3d.gpu)
            {
                image0 = RotateImage_GPU(rotation, 0, 0);
                if (fi.sizeC > 1) image1 = RotateImage_GPU(rotation, 0, 1);

            }
            else
            {
                image0 = RotateImage_CPU(rotation, 0, 0);
                if (fi.sizeC > 1) image1 = RotateImage_GPU(rotation, 0, 1);
            }

            Set_RGB(image0, image1, maxSize, bmpData.Stride, rgbValues, drawC0, drawC1);

            // Copy the RGB values back to the bitmap
            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, bytes);
            // Unlock the bits.
            bmp.UnlockBits(bmpData);

            stopwatch.Stop();
            Console.WriteLine(stopwatch.ElapsedMilliseconds);
            //return results
            return bmp;
        }
        private void Set_RGB(ushort[] image0, ushort[] image1,
                                    int maxSize, int bmp_stride, byte[] rgbValues,
                                    bool drawC0, bool drawC1)
        {
            Color c0 = fi.LutList[0];
            Color c1 = new Color();
            if (drawC1) c1 = fi.LutList[1];
            Parallel.For(0, maxSize * maxSize, p =>
            {
                int i = p / maxSize;
                int j = p % maxSize;
                int position = i * bmp_stride + 4 * j;

                float val0_adj = 0, val1_adj = 0;

                if (drawC0)
                {
                    ushort val0 = image0[p];
                    val0_adj = fi.adjustedLUT[0][val0];
                }

                if (drawC1)
                {
                    ushort val1 = image1[p];
                    val1_adj = fi.adjustedLUT[1][val1];
                }

                rgbValues[position] = (byte)(val0_adj * c0.B + val1_adj * c1.B);
                position++;
                rgbValues[position] = (byte)(val0_adj * c0.G + val1_adj * c1.G);
                position++;
                rgbValues[position] = (byte)(val0_adj * c0.R + val1_adj * c1.R);
                position++;
                rgbValues[position] = 255;
                position++;
            });
        }

        private void UpdateRotationAngles(ref int shearDirection)
        {
            if (shearDirection >= 360) shearDirection = 0;
        }
        private ushort[] RotateImage_GPU(double[,] rotation, int numRectangle, int C)
        {
            gpu.RotateImage(rotation, proj3d.sizeX, proj3d.sizeY, proj3d.sizeZ, maxSize, numRectangle, C);
            return gpu.result;
        }
        private ushort[] RotateImage_CPU(double[,] rotation, int numRectangle, int C)
        {
            ushort[][] sheared_image = new ushort[maxSize][];

            ushort[] sheared1d = new ushort[maxSize * maxSize];

            ushort[] image1d;
            if (C == 0) image1d = this.image1d_C0;
            else image1d = this.image1d_C1;

            for (int y = 0; y < sheared_image.Length; y++)
                sheared_image[y] = new ushort[maxSize];

            float halfX = proj3d.sizeX / 2;
            float halfY = proj3d.sizeY / 2;
            float halfZ = proj3d.sizeZ / 2;
            float halfMax = maxSize / 2;

            Parallel.For(0, proj3d.sizeY, y =>
            {
                double[] p = new double[3];
                p[1] = y - halfY;
                for (int x = 0; x < proj3d.sizeX; x++)
                {
                    p[0] = x - halfX;
                    for (int z = zLimit; z < proj3d.sizeZ; z += 1)
                    {
                        p[2] = z - halfZ;

                        int NewImgIndex = x + proj3d.sizeX * (y + proj3d.sizeY * z);
                        int OriginalIndex = maxSize *
                                        ((int)(p[0] * rotation[0, 1] + p[1] * rotation[1, 1] + p[2] * rotation[2, 1] + halfMax)) +
                                         (int)(p[0] * rotation[0, 0] + p[1] * rotation[1, 0] + p[2] * rotation[2, 0] + halfMax);

                        if (image1d[NewImgIndex] > sheared1d[OriginalIndex])
                            sheared1d[OriginalIndex] = image1d[NewImgIndex];
                    }
                }
            });

            return sheared1d;
        }
        public void initProgram(GLControl GLcontrol1, TifFileInfo incoming_fi)
        {

            if (initialized) return;
            this.initialized = true;


            this.proj3d = new Image3DProjection(incoming_fi, downsample_factor);
            this.fi = incoming_fi; // proj3d.CopyFi(incoming_fi);

            //proj3d.ProjectionEvent(incoming_fi);

            this.time_frame = fi.frame;

            this.maxSize = (int)(Math.Sqrt(Math.Pow(proj3d.factor * proj3d.sizeX, 2) +
                Math.Pow(proj3d.factor * proj3d.sizeY, 2) + Math.Pow(proj3d.factor * proj3d.sizeZ, 2)));
            this.image1d_C0 = proj3d.GetImage1D(0, 0, fi.frame);
            this.image1d_C1 = fi.sizeC > 1 ? proj3d.GetImage1D(0, 1, fi.frame) : null;

            this.cube1 = new Cube((float)(proj3d.sizeX * proj3d.factor) / maxSize,
                (float)(proj3d.sizeY * proj3d.factor) / maxSize,
                (float)(proj3d.sizeZ * proj3d.factor) / maxSize);

            this.gpu = new GPU_Processing(image1d_C0, image1d_C1, proj3d.GetSegmentedImage());
            ROIs = new List<SphereROI>();

            LoadExistingRois();

            GLcontrol1.MakeCurrent();

            //StartAnimation(GLcontrol1, fi);

            // initialize the 3D plot and the related properties
            Rectangle chartRegion = new Rectangle(
                    3 * leftMargin + 2 * maxSize, topMargin, maxSize, maxSize);

            plt = new CTChart_3D(IDrawer.IA, fi, maxSize);
            plt.rect = chartRegion;

            float[] Xdata = new float[fi.sizeZ];
            float[] Ydata = new float[fi.sizeT];
            for (int z = 0; z < fi.sizeZ; z++) Xdata[z] = z;
            for (int t = 0; t < fi.sizeT; t++) Ydata[t] = t;

            float[,] Zdata = null; // CurrentSphereRoi.Measure(fi);

            double[,] identity = new double[3, 3] { { 1, 0, 0 }, { 0, 1, 0 }, { 0, 0, 1 } };

            if (PropPanel == null) PropPanel = new PropertiesPanel_Item();
            plt.Properties3D = new CTChart_Properties_3D(IDrawer.IA.TabPages.propertiesPanel,
                IDrawer.IA.TabPages.PropertiesBody, IDrawer.IA, maxSize,
                RotateImage_CPU(identity, 1, 0), Xdata, Ydata, Zdata, PropPanel);


        }




        public void ClearProgram(GLControl GLcontrol1)
        {
            initialized = false;
            proj3d = null;
            shearX = 0; shearY = 0;
            zLimit = 0;
            downsample_factor = 1;
            time_frame = 0;
            leftMargin = 10;
            topMargin = 10;
            maxSize = 0;
            cube1 = null;
            CurrentSphereRoi = null;
            ROIs = null;
            DisplayROIs = false;
            rotated_image = null;
            gpu = null;
            image1d_C0 = null; image1d_C1 = null;
            //plt = null; Reuse the 3D plots
            fi = null;
        }

        public void GLControl1_DoubleClick(object sender, MouseEventArgs e)
        {
            Point newP = IsPointInPlot(e);
            if (newP.X == -10000) return;

            //plt.FindClosestPointToClick(newP);
        }
        public void GLControl1_MouseClick(GLControl GLcontrol1, TifFileInfo original_fi, MouseEventArgs e)
        {
            if (!initialized) initProgram(GLcontrol1, fi);
            DisplayROIs = false;
            if (e.Button == MouseButtons.Right) { plt.AdjustOnClick(); return; }

            Point newP = IsPointInImage(e);

            int currentI = (newP.X) + maxSize * (newP.Y);

            if (currentI < 0) return;
            if (newP.X == -10000) { plt.Properties3D.Refresh(fi); return; }

            DrawRotatedView(GLcontrol1, fi, 0);
            AddNewSphereRoi(currentI);


            // Tracking

            if (IDrawer.IA.RoiMan.RoiType != 1) return;

            //Check is filter image open
            if (fi.tpTaskbar.MethodsBtnList[1].ImageIndex == 1) return;
            //if (fi.selectedPictureBoxColumn != 1) return;



        }
        public void GLControl1_MouseDown(GLControl GLcontrol1, TifFileInfo fi, MouseEventArgs e)
        {
            if (!initialized) initProgram(GLcontrol1, fi);
            MousePosition.X = e.X;
            MousePosition.Y = e.Y;
            proj3d.factor = downsample_factor;
        }

        private void StartAnimation(GLControl GLcontrol1)
        {
            PointF e = new PointF(0f, 30f);
            var bgw = new BackgroundWorker();
            bgw.WorkerReportsProgress = true;

            bgw.DoWork += new DoWorkEventHandler(delegate (Object o, DoWorkEventArgs a)
            {
                while (true)
                {
                    Move(GLcontrol1, new PointF(e.X, e.Y));
                    e.X += 2;
                    if (e.X >= 360)
                    {
                        e.X = 0;
                        MousePosition.X = 0;
                    }
                    MousePosition.X += e.X;

                    DateTime start = DateTime.Now;
                    DateTime finish = start.AddMilliseconds(5); // or whatever the delay is to be
                    do { } while (DateTime.Now < finish);

                    ((BackgroundWorker)o).ReportProgress(0);
                }
            });

            bgw.RunWorkerAsync();
        }
        public void GLControl1_MouseMove(GLControl GLcontrol1, TifFileInfo fi, MouseEventArgs e)
        {
            if (!initialized) initProgram(GLcontrol1, fi);
            Move(GLcontrol1, new PointF(e.X, e.Y));
        }

        private void Move(GLControl GLcontrol1, PointF e)
        {
            if (MousePosition.X == -1) return;

            if (MousePosition.X < 20 && proj3d.drawPlane)
            {

                if (e.Y > MousePosition.Y)
                    zLimit += 1;
                else
                    zLimit -= 1;

                if (zLimit < 0) zLimit = 0;
                if (zLimit > proj3d.sizeZ - 1) zLimit = proj3d.sizeZ - 1;

                StartDrawing(GLcontrol1, fi);
                return;
            }

            // If point is on the graph, rotate it
            if (MousePosition.X > 2 * maxSize * fi.zoom)
            {
                plt.Rotate(e);
                StartDrawing(GLcontrol1, fi);
                return;
            }

            if (Math.Abs(e.X - MousePosition.X) > Math.Abs(e.Y - MousePosition.Y))
                // Mouse movement in X is bigger, rotate on X 
                if (e.X > MousePosition.X) shearX += 5; else shearX -= 5;

            else
                // Mouse movement on Y is bigger, rotate on Y
                if (e.Y > MousePosition.Y) shearY += 5; else shearY -= 5;

            DisplayROIs = false;
            StartDrawing(GLcontrol1, fi);

            MousePosition.X = e.X;
            MousePosition.Y = e.Y;
        }
        public void GLControl1_MouseUp(GLControl GLcontrol1, TifFileInfo fi, MouseEventArgs e)
        {
            if (!initialized) initProgram(GLcontrol1, fi);
            proj3d.factor = 1;
            DisplayROIs = true;

            StartDrawing(GLcontrol1, fi);
            MousePosition = new Vector2(-1, -1);
        }

        private Point IsPointInImage(MouseEventArgs e)
        {
            Point newP = new Point((int)((float)e.X / fi.zoom - maxSize - leftMargin * 2),
                (int)((double)e.Y / fi.zoom - topMargin));

            if (fi.tpTaskbar.MethodsBtnList[1].ImageIndex == 0
                && newP.X > 0 && newP.Y > 0 && newP.X < maxSize && newP.Y < maxSize)
            {
                return newP;
            }

            else
            {
                return new Point(-10000, -10000);
            }
        }

        private Point IsPointInPlot(MouseEventArgs e)
        {
            Point newP = new Point((int)((float)e.X / fi.zoom - 2 * maxSize - leftMargin * 3),
                (int)((double)e.Y / fi.zoom - topMargin));

            if (fi.tpTaskbar.MethodsBtnList[2].ImageIndex == 0
                && newP.X > 0 && newP.Y > 0 && newP.X < maxSize && newP.Y < maxSize)
            {
                return newP;
            }

            else
            {
                return new Point(-10000, -10000);
            }
        }

        private void AddNewSphereRoi(int currentI)
        {

            // Reconstruct the X, Y and Z from the 1D representation maxValueIndex
            // knowing that maxValueIndex = x + sizeX * (y + sizeY * z);

            int originalZ = gpu.maxValueArray[currentI] / (fi.sizeX * fi.sizeY);
            int originalY = (gpu.maxValueArray[currentI] % (fi.sizeX * fi.sizeY)) / fi.sizeX;
            int originalX = ((gpu.maxValueArray[currentI] % (fi.sizeX * fi.sizeY)) % fi.sizeX);

            Point3d center = new Point3d((float)originalX / maxSize,
                (float)originalY / maxSize,
                (float)originalZ / maxSize);
            CurrentSphereRoi = new SphereROI(fi, maxSize, center, 0.02f, shearX, shearY, cube1);

            // Create new current ROI
            ROI current = new ROI(fi.ROICounter, fi.imageCount,
                IDrawer.IA.RoiMan.RoiShape, IDrawer.IA.RoiMan.RoiType,
                IDrawer.IA.RoiMan.turnOnStackRoi);
            current.Width = (int)(CurrentSphereRoi.radius * fi.sizeX);
            current.Height = (int)(CurrentSphereRoi.radius * fi.sizeY);

            fi.ROICounter++;

            // Maap the 3D ROI to the 2D ROI
            CurrentSphereRoi.ROI2d = current;

            current.FromT = 1;
            current.FromZ = 1;
            current.ToT = fi.sizeT;
            current.ToZ = fi.sizeZ;

            if (IDrawer.IA.RoiMan.turnOnStackRoi == true)
            {
                current.Stack = 1;
                current.D = 3;
            }

            current.SetLocation(fi.frame, new Point[1] {
                new Point(  (int)(maxSize * center.X - current.Width/2),
                            (int)(maxSize * center.Y - current.Height/2)) });

            IDrawer.IA.RoiMan.current = current;
            //Clear selected roi list
            IDrawer.IA.RoiMan.SelectedROIsList.Clear();

            ROIs.Add(CurrentSphereRoi);

            // Finally, add the ROI data to the 3D plot
            plt.Properties3D.Zdata = CurrentSphereRoi.Measure(IDrawer.IA, fi);





        }

        private SphereROI SphereRoiExists(ROI roi)
        {
            foreach (SphereROI roi3d in ROIs) if (roi3d != null && roi3d.ROI2d == roi) return roi3d;
            return null;
        }


        // Remove the SphereRois which have no 2D counterpart
        private void UpdateRoiList()
        {
            if (fi.roiList == null || fi.roiList[fi.cValue] == null)
                ROIs.Clear();

            ROIs.RemoveAll(roi3d => !fi.roiList[fi.cValue].Contains(roi3d.ROI2d));
        }

        // Each 2D ROI has its mirror 3D ROI. This function updates the 3D list
        private void LoadExistingRois()
        {
            if (fi.roiList == null || fi.roiList[fi.cValue] == null) return;

            foreach (ROI roi in fi.roiList[fi.cValue])
            {
                PointF P = roi.GetMidPoint(fi.frame);
                Point3d center = new Point3d(P.X / maxSize, P.Y / maxSize, 0);
                float radius = (float)roi.Width / fi.sizeX;

                // If a SphereRoi already exists for this, just update its X and Y
                SphereROI existingRoi = SphereRoiExists(roi);


                if (existingRoi != null)
                {
                    existingRoi.radius = radius;
                    //existingRoi.UpdateLocation(center, false, fi.frame);
                }
                else if (CurrentSphereRoi != null && !ROIs.Contains(CurrentSphereRoi))
                {
                    CurrentSphereRoi.ROI2d = roi;
                    CurrentSphereRoi.radius = roi.Width / fi.sizeX;
                    ROIs.Add(CurrentSphereRoi);
                }
                else
                {
                    // add a new 3D ROI
                    SphereROI mirrorRoi = new SphereROI(fi, maxSize, center, radius, shearX, shearY, cube1);
                    mirrorRoi.ROI2d = roi;
                    ROIs.Add(mirrorRoi);
                }
            }
        }

        private void InitializeTrackingShablon(Color PointColor)
        {
            // Send this FI to the magin wand ROI function
            IDrawer.IA.Tracking.shablon = new bool[maxSize, maxSize];
            for (int y = 0; y < maxSize; y++)
            {
                for (int x = 0; x < maxSize; x++)
                {
                    ushort val = rotated_image[y * maxSize + x];
                    Color col;
                    int C = 0;

                    if (val < fi.thresholdValues[C][1])
                        col = fi.thresholdColors[C][0];
                    else if (val < fi.thresholdValues[C][2])
                        col = fi.thresholdColors[C][1];
                    else if (val < fi.thresholdValues[C][3])
                        col = fi.thresholdColors[C][2];
                    else if (val < fi.thresholdValues[C][4])
                        col = fi.thresholdColors[C][3];
                    else
                        col = fi.thresholdColors[C][fi.thresholds[C]];

                    if (col == PointColor) IDrawer.IA.Tracking.shablon[y, x] = true;
                }
            }
        }
        private void MagicWandRoi(ushort[][] image, Color PointColor)
        {
            if (MousePosition.X == -1) return;

            Point newP = new Point((int)((float)MousePosition.X / fi.zoom - maxSize - leftMargin * 2),
                (int)((double)MousePosition.Y / fi.zoom - topMargin));


            FrameCalculator FC = new FrameCalculator();

            InitializeTrackingShablon(PointColor);

            List<Point> MagicRoi = (List<Point>)(IDrawer.IA.Tracking.BordersOfObjectExactPolygon(
                                                            fi,
                                                            fi.cValue,
                                                            FC.Frame(fi),
                                                            image,
                                                            newP,
                                                            PointColor,
                                                            scipShablon: true));
            GL.Begin(PrimitiveType.LineLoop);
            GL.Color4(1f, 1f, 0f, 1f);

            foreach (Point P in MagicRoi) GL.Vertex2(leftMargin + P.X, topMargin + P.Y);

            GL.End();



        }
    }

    class Point3d
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public Point3d(double x, double y, double z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }
        public Point3d Duplicate() { return new Point3d(X, Y, Z); }
        public Point3d RotateX(double angle)
        {
            PointF point = new PointF((float)Y, (float)Z);
            point = Rotate(point, angle);
            return new Point3d(X, point.X, point.Y);
        }
        public Point3d RotateY(double angle)
        {
            PointF point = new PointF((float)Z, (float)X);
            point = Rotate(point, angle);
            return new Point3d(point.Y, Y, point.X);
        }
        public Point3d RotateZ(double angle)
        {
            PointF point = new PointF((float)X, (float)Y);
            point = Rotate(point, angle);
            return new Point3d(point.X, point.Y, Z);
        }
        private PointF Rotate(PointF point, double angle)
        {
            double sin = Math.Sin(angle);
            double cos = Math.Cos(angle);

            return new PointF(
               (float)(point.X * cos - point.Y * sin),
                (float)(point.X * sin + point.Y * cos)
                );
        }

        public double Distance2DTo(Point3d other)
        {
            return Math.Sqrt(Math.Pow(X - other.X, 2) + Math.Pow(Y - other.Y, 2));
        }
    }

    class Cube
    {
        public float X, Y, Z; // the unit length of the sides, proportional to the image dimensions
        public Point3d[] sides;
        public Cube(float sizeX, float sizeY, float sizeZ)
        {
            X = sizeX; Y = sizeY; Z = sizeZ;
            sides = new Point3d[]
               {
                new Point3d(    -X/2, -Y/2,  -Z/2   ),
                new Point3d(     X/2, -Y/2,  -Z/2   ),
                new Point3d(     X/2,  Y/2,  -Z/2   ),
                new Point3d(    -X/2,  Y/2,  -Z/2   ),
                new Point3d(    -X/2, -Y/2,   Z/2   ),
                new Point3d(     X/2, -Y/2,   Z/2   ),
                new Point3d(     X/2,  Y/2,   Z/2   ),
                new Point3d(    -X/2,  Y/2,   Z/2   )
               };
        }

        private static int[] faces = new int[]
        {
            0,1,2,3, /*Back*/  3,2,6,7, /*Left*/ 5,4,7,6, /*Front*/
            1,0,4,5, /*Right*/ 4,0,3,7, /*Top*/  5,1,2,6  /*Bottom*/
        };
        public enum CubeFace { Back, Left, Front, Right, Top, Bottom };

        public void Show(double cubeRotationX, double cubeRotationY,
            int numRectangle, int leftMargin, int topMargin, int maxSize, float scale = 1f)
        {
            Point3d[] points = rotateCube(cubeRotationX, cubeRotationY, 0);

            Point3d[] current;
            double lastZ = Cube.GetLastZ(points);
            for (int i = 0; i < 6; i++)
            {
                Color lineC = Color.White;
                current = Cube.GetFace(points, (Cube.CubeFace)i);
                if (!Cube.toDrawPlane(current, lastZ))
                {
                    lineC = Color.White; // Color.Pink;
                }
                //double light = (0.4d + GetArea(current.ToList())) / 1.4d;
                GL.Color3(1, 1, 1);
                //DrawTexture(current);
                GL.Begin(PrimitiveType.LineLoop);

                GL.Color3(lineC);

                foreach (var p in current)
                {
                    GL.Vertex2(leftMargin + numRectangle * (leftMargin + maxSize) + maxSize * scale * (0.5f + p.X),
                        topMargin + maxSize * scale * (0.5f + p.Y));
                }
                GL.End();



            }
        }
        public static Point3d[] GetFace(Point3d[] input, CubeFace cubeFace)
        {
            int index = 0;

            Point3d[] output = new Point3d[4];

            switch (cubeFace)
            {
                case CubeFace.Front: index = 0; break;
                case CubeFace.Back: index = 4; break;
                case CubeFace.Left: index = 8; break;
                case CubeFace.Right: index = 12; break;
                case CubeFace.Top: index = 16; break;
                case CubeFace.Bottom: index = 20; break;
            }

            for (int i = 0; i < output.Length; i++, index++)
                output[i] = input[faces[index]];

            return output;
        }
        public static double GetLastZ(Point3d[] input)
        {
            double output = double.MaxValue;
            foreach (var p in input)
                if (output > p.Z)
                    output = p.Z;
            return output;
        }
        public static bool toDrawPlane(Point3d[] input, double lastZ)
        {
            foreach (var p in input)
                if (lastZ == p.Z)
                    return false;
            return true;
        }
        public Point3d[] rotateCube(double angleX, double angleY, double angleZ)
        {
            Point3d[] current = this.Duplicate();

            for (int i = 0; i < current.Length; i++)
                current[i] = current[i]
                    .RotateX(angleX)
                    .RotateY(angleY)
                    .RotateZ(angleZ);

            return current;
        }

        private Point3d[] Duplicate()
        {
            Point3d[] output = new Point3d[sides.Length];
            for (int i = 0; i < sides.Length; i++)
                output[i] = sides[i].Duplicate();
            return output;
        }
    }

    class PathColor
    {
        Color[] ColorCollection = new Color[6] {
            Color.Red, Color.Green, Color.Blue, Color.Yellow, Color.Orange, Color.Pink };
        int ChosenIndex = 0;

        public Color NextColor()
        {
            ChosenIndex++;
            if (ChosenIndex >= ColorCollection.Length) ChosenIndex = 0;
            return ColorCollection[ChosenIndex];
        }
    }
    // Represents a 3D ROI
    class SphereROI
    {
        public Point3d center, old_center; // keeping track of the old center to display the path
        public float radius;
        public Cube cube1;
        public ROI ROI2d;
        public Point3d[] center_in_time;
        int originalT;
        int maxSize;

        public SphereROI(TifFileInfo fi, int maxSize,
            Point3d center, float radius, float shearX, float shearY, Cube cube1)
        {
            this.cube1 = cube1;
            this.center = center;
            this.old_center = center;
            this.radius = radius;

            originalT = fi.frame;
            this.maxSize = maxSize;
            Track(fi);
        }

        public void Track(TifFileInfo fi)
        {
            if (center_in_time != null) return; // already tracked

            center_in_time = new Point3d[fi.sizeT];
            center_in_time[originalT] = center;

            for (int frame = originalT + 1; frame < fi.sizeT; frame++)
            {
                center_in_time[frame] = FindMaxPixelInRegion(fi, frame, 1, center_in_time[frame - 1]);
            }
            for (int frame = originalT - 1; frame >= 0; frame--)
            {
                center_in_time[frame] = FindMaxPixelInRegion(fi, frame, 1, center_in_time[frame + 1]);
            }
        }


        // For now, return the mean signal in 2D (x, y), dependent on Z and T
        public float[,] MeasureMaxPixel(TifFileInfo fi)
        {
            float[,] roiData = new float[fi.sizeZ, fi.sizeT];

            for (int z = 0; z < fi.sizeZ; z++)
                for (int t = 0; t < fi.sizeT; t++)
                {
                    Point3d centerT = center_in_time[t];
                    roiData[z, t] = fi.image16bitFilter[z][(int)(maxSize * centerT.Y)][(int)(maxSize * centerT.X)];
                }
            return roiData;
        }

        // Generate a new static ROI for each Z and T. Measure those with the RoiMeasure tools
        public float[,] Measure(ImageAnalyser IA, TifFileInfo fi)
        {
            float[,] roiData = new float[fi.sizeZ, fi.sizeT];

            ROI newROI = new ROI(0, fi.imageCount, 1, 1, false);

            newROI.Width = 5;
            newROI.Height = 5;

            for (int t = 0, imageN = fi.cValue; t < fi.sizeT; t++)
            {
                for (int z = 0; z < fi.sizeZ && imageN < fi.imageCount; z++, imageN += fi.sizeC)
                {
                    // Create a new static ROI on this Z
                    newROI.SetLocation(imageN, new Point[] { new Point(
                        (int)center_in_time[t].X - newROI.Width/2,
                        (int)center_in_time[t].Y - newROI.Height/2)});
                }

            }

            if (fi.roiList == null) fi.roiList = new List<ROI>[fi.sizeC];
            if (fi.roiList[fi.cValue] == null) fi.roiList[fi.cValue] = new List<ROI>();

            fi.roiList[fi.cValue].Add(newROI);
            RoiMeasure.Measure(newROI, fi, 0, IA);
            fi.roiList[fi.cValue].Remove(newROI);

            for (int t = 0, imageN = fi.cValue; t < fi.sizeT; t++)
            {
                for (int z = 0; z < fi.sizeZ && imageN < fi.imageCount; z++, imageN += fi.sizeC)
                {
                    roiData[z, t] = (float)newROI.Results[imageN][1];

                }
            }
            return roiData;

        }

        private Point3d FindMaxPixelInRegion(TifFileInfo fi, int frame, int C, Point3d original_center,
            int xy_width = 6, int z_width = 2)
        {
            Point3d center = new Point3d(
                original_center.X * maxSize,
                original_center.Y * maxSize,
                original_center.Z * maxSize);

            Point3d maxLocation = new Point3d(center.X, center.Y, center.Z);
            ushort maxValue = 0;
            int ZCF = fi.sizeZ * fi.sizeC * frame + C;

            for (int z = Math.Max(0, (int)center.Z - z_width);
                z < Math.Min(fi.sizeZ - 1, (int)center.Z + z_width); z++)
            {
                int Z_new = ZCF + z * fi.sizeC;
                for (int y = Math.Max(0, (int)center.Y - xy_width);
                    y < Math.Min(fi.sizeY - 1, (int)center.Y + xy_width); y++)
                {
                    for (int x = Math.Max(0, (int)center.X - xy_width);
                        x < Math.Min(fi.sizeX - 1, (int)center.X + xy_width); x++)
                    {
                        if (fi.image16bitFilter[Z_new][y][x] > maxValue)
                        {
                            maxValue = fi.image16bitFilter[Z_new][y][x];
                            maxLocation.X = (double)x / maxSize;
                            maxLocation.Y = (double)y / maxSize;
                            maxLocation.Z = (double)z / maxSize;
                        }
                    }
                }
            }


            return maxLocation;
        }


        public void Display(float angleX, float angleY, int leftMargin, int topMargin, int maxSize, int frame = -1)
        {
            // Rotate the center point according to the current angles
            double DEG2RAD = Math.PI / 180f;

            Point3d corner = cube1.rotateCube(0, 0, 0)[0];

            Point3d current_center = center;
            // is this a tracking ROI?
            if (frame != -1) current_center = center_in_time[frame];
            if (frame != -1 && ROI2d != null && frame < ROI2d.FromT - 1) return;
            if (frame != -1 && ROI2d != null && frame >= ROI2d.ToT) return;

            Point3d newP = new Point3d(current_center.X + corner.X,
                current_center.Y + corner.Y, current_center.Z + corner.Z);

            Point3d rotatedP = newP.RotateX((angleY) * DEG2RAD)
                .RotateY((angleX) * DEG2RAD);
            //rotatedP.Z = 0;
            GL.Begin(PrimitiveType.LineLoop);
            GL.Color4(1f, 0f, 0f, 1f);

            // Draw the sphere
            for (int i = 0; i < 360; i += 10)
            {
                float degInRad = (float)(i * DEG2RAD);
                double newX = rotatedP.X + Math.Cos(degInRad) * radius;
                double newY = rotatedP.Y + Math.Sin(degInRad) * radius;
                GL.Vertex2(leftMargin + maxSize * (0.5f + newX),
                        topMargin + maxSize * (0.5f + newY));
            }
            GL.End();
            GL.Color3(Color.White);

        }

        public void DisplayPath(float angleX, float angleY, int leftMargin, int topMargin, int maxSize, int current_frame)
        {
            double DEG2RAD = Math.PI / 180f;
            Point3d corner = cube1.rotateCube(0, 0, 0)[0];

            GL.Begin(PrimitiveType.LineStrip);

            for (int frame = ROI2d.FromT + 1; frame < Math.Min(ROI2d.ToT, current_frame); frame++)
            {
                Point3d newCenter = center_in_time[frame];

                Point3d newP = new Point3d(newCenter.X + corner.X, newCenter.Y + corner.Y, newCenter.Z + corner.Z);
                Point3d rotatedP_new = newP.RotateX((angleY) * DEG2RAD)
                    .RotateY((angleX) * DEG2RAD);

                GL.Vertex2(leftMargin + maxSize * (0.5f + rotatedP_new.X),
                            topMargin + maxSize * (0.5f + rotatedP_new.Y));
            }
            GL.End();
        }
    }
}