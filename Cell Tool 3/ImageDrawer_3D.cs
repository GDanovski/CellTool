using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Diagnostics;
using System.ComponentModel;

namespace Cell_Tool_3
{
    /// <summary>
    /// Includes events for 3D visualization
    /// </summary>
    class ImageDrawer_3D
    {
        private int shearX = 0; // X displacement - the angle of rotation around the Y axis in degrees
        private int shearY = 0; // Y displacement - the angle of rotation around the X axis in degrees
        int backSide = 1; // indicates move direction depending on user mouse direction
        int zLimit = 0; // the minimum Z slice to display for the moving plane
        int downsample_factor = 1; // in case of big images
        int time_frame;


        GPU_Processing gpu; // rotation can be performed on the GPU
        ushort[] image1d_C0, image1d_C1; // the 1d representations of the 2 channel raw images

        private Image3DProjection proj3d; // holds configurations

        private Vector2 MousePosition = new Vector2(-1, -1);

        // Checks is the image 3D
        public bool isImage3D(TifFileInfo fi) { return fi.is3D; }

        // Draw the image      
        public void StartDrawing(GLControl GLcontrol1, TifFileInfo fi)
        {
            if (fi == null) return;

            if (fi.frame != this.time_frame)
            {
                this.time_frame = fi.frame;
                this.image1d_C0 = proj3d.GetImageForGPU(0, 0);
                this.image1d_C1 = fi.sizeC > 1 ? proj3d.GetImageForGPU(0, 1) : null;
                this.gpu.Cleanup();
                this.gpu = new GPU_Processing(image1d_C0, image1d_C1, proj3d.GetSegmentedImage());
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

            GL.Disable(EnableCap.DepthTest);

            GL.Flush();
            GLcontrol1.SwapBuffers();
        }
        private void DrawRotatedView(GLControl GLcontrol1, TifFileInfo fi, int numRectangle)
        {
            GL.Enable(EnableCap.Texture2D);
            
            int leftMargin = 10;
            int topMargin = 10;
            int maxSize = (int)(Math.Sqrt(Math.Pow(proj3d.factor * proj3d.sizeX, 2) +
                Math.Pow(proj3d.factor * proj3d.sizeY, 2) + Math.Pow(proj3d.factor * proj3d.sizeZ, 2)));

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

            if (numRectangle == 0) LoadTexture(RawToBmpComposite(rotation, fi, maxSize));
            else if (numRectangle == 1) LoadTexture(RawToBmpSegmented(rotation, fi));
            else return;

            Cube cube1 = new Cube((float)(proj3d.sizeX * proj3d.factor) / maxSize,
                (float)(proj3d.sizeY * proj3d.factor) / maxSize,
                (float)(proj3d.sizeZ * proj3d.factor) / maxSize);

            double angleX = shearX * Math.PI / 180f;
            double angleY = shearY * Math.PI / 180f;

            double cubeRotationX = angleY;
            double cubeRotationY = angleX;

            Point3d[] points = cube1.rotateCube(cubeRotationX, cubeRotationY, 0);

            Point3d[] current;
            double lastZ = Cube.GetLastZ(points);

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
                    GL.Vertex2(leftMargin + numRectangle * (leftMargin + maxSize) + maxSize * (0.5f + p.X),
                        topMargin + maxSize * (0.5f + p.Y));
                }
                GL.End();
            }

            if (proj3d.drawPlane) drawPlane(zLimit, maxSize, leftMargin, topMargin);
        }

        public void LoadTexture(Bitmap bmp, bool NoAntiAliasing = false)
        {
            //Load texture from file
            Bitmap texture_source = bmp;

            GL.DeleteTexture(0);
            GL.BindTexture(TextureTarget.Texture2D, 0);

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
        private Bitmap RawToBmpSegmented(double[,] rotation, TifFileInfo fi)
        {
            int C = fi.cValue;
            FrameCalculator FC = new FrameCalculator();
            
            ushort[] image;

            if (proj3d.gpu)     image = RotateImage_GPU(rotation, 1, C);
            else                image = RotateImage_CPU(rotation, 1, C);

            int maxSize = (int)(Math.Sqrt(Math.Pow(proj3d.sizeX, 2) + Math.Pow(proj3d.sizeY, 2) + Math.Pow(proj3d.sizeZ, 2)));

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

                for (int j = 0; j < maxSize; j++) {   
                    int val = image[i * maxSize + j];
                    byte val1 = (byte)(fi.newAdjustedLUT[fi.cValue][val]);

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
        
        private Bitmap RawToBmpComposite(double[,] rotation, TifFileInfo fi, int maxSize)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            FrameCalculator FC = new FrameCalculator();
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

            } else {
                image0 = RotateImage_CPU(rotation, 0, 0);
                if (fi.sizeC > 1) image1 = RotateImage_GPU(rotation, 0, 1);  
            }

            Set_RGB(fi, image0, image1, maxSize, bmpData.Stride, rgbValues, drawC0, drawC1);

            // Copy the RGB values back to the bitmap
            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, bytes);
            // Unlock the bits.
            bmp.UnlockBits(bmpData);

            stopwatch.Stop();
            Console.WriteLine(stopwatch.ElapsedMilliseconds);
            //return results
            return bmp;
        }

        private void Set_RGB(TifFileInfo fi, ushort[] image0, ushort[] image1, 
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
            int maxSize = (int)(Math.Sqrt(Math.Pow(proj3d.sizeX, 2) + Math.Pow(proj3d.sizeY, 2) + Math.Pow(proj3d.sizeZ, 2)));
            gpu.RotateImage(rotation, proj3d.sizeX, proj3d.sizeY, proj3d.sizeZ, maxSize, numRectangle, C);    
            return gpu.result;
        }

        private ushort[] RotateImage_CPU(double[,] rotation, int numRectangle, int C)
        {
            int maxSize = (int)(Math.Sqrt(Math.Pow(proj3d.sizeX, 2) + Math.Pow(proj3d.sizeY, 2) + Math.Pow(proj3d.sizeZ, 2)));
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

        public void initProgram(GLControl GLcontrol1, TifFileInfo fi)
        {
            this.time_frame = fi.frame;
            this.proj3d = new Image3DProjection(fi, downsample_factor);
            this.image1d_C0 = proj3d.GetImageForGPU(0, 0);
            this.image1d_C1 = fi.sizeC > 1 ? proj3d.GetImageForGPU(0, 1) : null;

            this.gpu = new GPU_Processing(image1d_C0, image1d_C1, proj3d.GetSegmentedImage());

            GLcontrol1.MakeCurrent();

            //StartAnimation(GLcontrol1, fi);
        }

        // TODO - what do we need to clean?
        public void ClearProgram(GLControl GLcontrol1) { }
        public void Calculate3Dfi(TifFileInfo fi) { }
        public void GLControl1_MouseClick(GLControl GLcontrol1, TifFileInfo fi, MouseEventArgs e) { }
        public void GLControl1_MouseDown(GLControl GLcontrol1, TifFileInfo fi, MouseEventArgs e)
        {
            MousePosition.X = e.X;
            MousePosition.Y = e.Y;
            proj3d.factor = downsample_factor;
        }

        private void StartAnimation(GLControl GLcontrol1, TifFileInfo fi)
        {
            PointF e = new PointF(0f, 30f);
            var bgw = new BackgroundWorker();
            bgw.WorkerReportsProgress = true;

            bgw.DoWork += new DoWorkEventHandler(delegate (Object o, DoWorkEventArgs a)
            {
                while (true)
                {
                    Move(GLcontrol1, fi, new PointF(e.X, e.Y));
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
            Move(GLcontrol1, fi, new PointF(e.X, e.Y));
        }

        private void Move(GLControl GLcontrol1, TifFileInfo fi, PointF e)
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
            
            if (Math.Abs(e.X - MousePosition.X) > Math.Abs(e.Y - MousePosition.Y))
            // Mouse movement in X is bigger, rotate on X
            {
                if (e.X > MousePosition.X) { shearX += backSide * 5; }
                else { shearX -= backSide * 5; }
            }
            else
            {
                // Mouse movement on Y is bigger, rotate on Y
                if (e.Y > MousePosition.Y) { shearY += backSide * 5; }
                else { shearY -= backSide * 5; }
            }

            StartDrawing(GLcontrol1, fi);

            MousePosition.X = e.X;
            MousePosition.Y = e.Y;
        }
        public void GLControl1_MouseUp(GLControl GLcontrol1, TifFileInfo fi, MouseEventArgs e)
        {
            MousePosition = new Vector2(-1, -1);
            proj3d.factor = 1;
            StartDrawing(GLcontrol1, fi);
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
        public Point3d Duplicate()  {  return new Point3d(X, Y, Z); }
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
    }
    
    class Cube
    {
        public float X, Y, Z; // the unit length of the sides, proportional to the image dimensions
        public Point3d[] sides;
        public Cube(float sizeX, float sizeY, float sizeZ) {
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
        public static Point3d[] GetFace(Point3d[] input, CubeFace cubeFace)
        {
            int index = 0;

            Point3d[] output = new Point3d[4];

            switch (cubeFace)
            {
                case CubeFace.Front:    index = 0;  break;
                case CubeFace.Back:     index = 4;  break;
                case CubeFace.Left:     index = 8;  break;
                case CubeFace.Right:    index = 12; break;
                case CubeFace.Top:      index = 16; break;
                case CubeFace.Bottom:   index = 20; break;
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
}