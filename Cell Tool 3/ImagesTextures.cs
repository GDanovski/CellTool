using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;

namespace Cell_Tool_3
{
    class ImagesTextures:IDisposable
    {
        private List<ImageData> Images;
        public int GetID(int index)
        {
            return Images[index].GetID;
        }
        public ImagesTextures()
        {
            this.Images = new List<ImageData>();
        }
        public void Dispose()
        {
            foreach (var image in this.Images)
                image.ClearImage();
        }
        public void PrepareImageList(int index)
        {
            if (Images.Count <= index)
                for (int i = Images.Count; i < index; i++)
                    Images.Add(new ImageData());
            else if (Images.Count > index)
                for (int i = index + 1; i < Images.Count; i++)
                    Images[i].ClearImage();
        }
        public void GenerateImageData(TifFileInfo fi, int index, int C)
        {
            switch (fi.bitsPerPixel)
            {
                case 8:
                    Raw8ToBmp(fi, this.Images[index], C);
                    break;
                case 16:
                    Raw16ToBmp(fi, this.Images[index], C);
                    break;
            }
        }
        public void GenerateFilteredImageData(TifFileInfo fi, int index, int C, int[] SpotDiapason)
        {
           
            switch (fi.bitsPerPixel)
            {
                case 8:
                    Raw8FilteredToBmp(fi, this.Images[index], C,SpotDiapason);
                    break;
                case 16:
                    Raw16FilteredToBmp(fi, this.Images[index], C,SpotDiapason);
                    break;
            }
        }
        public void LoadTextures(TifFileInfo fi, int index)
        {
           LoadImageTexture(Images[index].GetImage(fi.sizeX, fi.sizeY), Images[index].GetID);
        }
        public void DrawTexture(int index, Rectangle rect)
        {
            GL.BindTexture(TextureTarget.Texture2D, this.Images[index].GetID);

            GL.Begin(PrimitiveType.Quads);

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
        }
        private int LoadImageTexture(Bitmap bmp, int textureID)
        {
            //Load texture from file
            Bitmap texture_source = bmp;
            //Link empty texture to texture2d
            GL.BindTexture(TextureTarget.Texture2D, textureID);
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

            return textureID;
        }
        private void Raw16FilteredToBmp(TifFileInfo fi, ImageData imageData, int C, int[] SpotDiapason)
        {
            int Choise = fi.thresholds[C];
            if (fi.SegmentationCBoxIndex[C] == 0) Choise = 0;
            Color lastCol = fi.thresholdColors[C][fi.thresholds[C]];
            float[] LUT = fi.adjustedLUT[C];

            FrameCalculator FC = new FrameCalculator();
            //image array
            Color col = Color.FromArgb(fi.LutList[C].A, fi.LutList[C].R, fi.LutList[C].G, fi.LutList[C].B);
            byte R = col.R;
            byte G = col.G;
            byte B = col.B;
            if (fi.image16bit == null) return;

            if (fi.image16bitFilter == null) fi.image16bitFilter = fi.image16bit;
            ushort[][] image = fi.image16bitFilter[FC.FrameC(fi, C)];

            if (image == null) return;
            imageData.GetImage(fi.sizeX, fi.sizeY);

            byte[] rgbValues = imageData.GetBuffer();
            //take LUT info
            int position = 0;
            foreach (ushort[] row in image)
            {
                foreach (ushort val in row)
                {
                    #region Colors
                    if (fi.SegmentationCBoxIndex[C] != 0 | fi.SelectedSpotThresh[C] != 0)
                    {
                        switch (Choise)
                        {
                            case 0:
                                if (val > SpotDiapason[0] & val < SpotDiapason[1])
                                    col = fi.SpotColor[C];
                                else if (LUT.Length > val)
                                    col = Color.FromArgb((byte)(LUT[val] * 255f),R, G, B);
                                else
                                    col = Color.FromArgb((byte)(LUT[LUT.Length - 1] * 255f),R, G, B );

                                break;
                            default:

                                if (val > SpotDiapason[0] & val < SpotDiapason[1])
                                {
                                    col = fi.SpotColor[C];
                                }
                                else if (val < fi.thresholdValues[C][1])
                                {
                                    col = fi.thresholdColors[C][0];
                                }
                                else if (val < fi.thresholdValues[C][2])
                                {
                                    col = fi.thresholdColors[C][1];
                                }
                                else if (val < fi.thresholdValues[C][3])
                                {
                                    col = fi.thresholdColors[C][2];
                                }
                                else if (val < fi.thresholdValues[C][4])
                                {
                                    col = fi.thresholdColors[C][3];
                                }
                                else
                                {
                                    col = lastCol;
                                }

                                if (col == Color.Transparent)
                                {
                                    if (LUT.Length > val)
                                        col = Color.FromArgb((byte)(LUT[val] * 255f), R, G, B);
                                    else
                                        col = Color.FromArgb((byte)(LUT[LUT.Length - 1] * 255f), R, G, B);
                                }
                                break;
                        }
                    }
                    else
                    {
                        if (LUT.Length > val)
                            col = Color.FromArgb((byte)(LUT[val] * 255f), R, G, B);
                        else
                            col = Color.FromArgb((byte)(LUT[LUT.Length - 1] * 255f), R, G, B);
                    }
                    #endregion Colors

                    rgbValues[position] = col.B;
                    position++;
                    rgbValues[position] = col.G;
                    position++;
                    rgbValues[position] = col.R;
                    position++;
                    rgbValues[position] = col.A;
                    position++;
                }
            }
            imageData.SetBuffer(rgbValues);
        }
        
        private void Raw8FilteredToBmp(TifFileInfo fi, ImageData imageData, int C, int[] SpotDiapason)
        {
            int Choise = fi.thresholds[C];
            if (fi.SegmentationCBoxIndex[C] == 0) Choise = 0;
            Color lastCol = fi.thresholdColors[C][fi.thresholds[C]];
            float[] LUT = fi.adjustedLUT[C];

            FrameCalculator FC = new FrameCalculator();
            //image array
            Color col = Color.FromArgb(fi.LutList[C].A,fi.LutList[C].R, fi.LutList[C].G, fi.LutList[C].B );
            byte R = col.R;
            byte G = col.G;
            byte B = col.B;
            if (fi.image8bit == null) return;

            if (fi.image8bitFilter == null) fi.image8bitFilter = fi.image8bit;
            byte[][] image = fi.image8bitFilter[FC.FrameC(fi, C)];
            if (image == null) return;
            imageData.GetImage(fi.sizeX, fi.sizeY);

            byte[] rgbValues = imageData.GetBuffer();
            //take LUT info
            int position = 0;
            foreach (byte[] row in image)
            {
                foreach (byte val in row)
                {
                    #region Colors
                    if (fi.SegmentationCBoxIndex[C] != 0 | fi.SelectedSpotThresh[C] != 0)
                    {
                        switch (Choise)
                        {
                            case 0:
                                if (val > SpotDiapason[0] & val < SpotDiapason[1])
                                    col = fi.SpotColor[C];
                                else if (LUT.Length > val)
                                    col = Color.FromArgb((byte)(LUT[val] * 255f), R, G, B);
                                else
                                    col = Color.FromArgb((byte)(LUT[LUT.Length - 1] * 255f), R, G, B);

                                break;
                            default:

                                if (val > SpotDiapason[0] & val < SpotDiapason[1])
                                {
                                    col = fi.SpotColor[C];
                                }
                                else if (val < fi.thresholdValues[C][1])
                                {
                                    col = fi.thresholdColors[C][0];
                                }
                                else if (val < fi.thresholdValues[C][2])
                                {
                                    col = fi.thresholdColors[C][1];
                                }
                                else if (val < fi.thresholdValues[C][3])
                                {
                                    col = fi.thresholdColors[C][2];
                                }
                                else if (val < fi.thresholdValues[C][4])
                                {
                                    col = fi.thresholdColors[C][3];
                                }
                                else
                                {
                                    col = lastCol;
                                }

                                if (col == Color.Transparent)
                                {
                                    if (LUT.Length > val)
                                        col = Color.FromArgb((byte)(LUT[val] * 255f), R, G, B);
                                    else
                                        col = Color.FromArgb((byte)(LUT[LUT.Length - 1] * 255f), R, G, B);
                                }
                                break;
                        }
                    }
                    else
                    {
                        if (LUT.Length > val)
                            col = Color.FromArgb((byte)(LUT[val] * 255f), R, G, B);
                        else
                            col = Color.FromArgb((byte)(LUT[LUT.Length - 1] * 255f), R, G, B);
                    }
                    #endregion Colors

                    rgbValues[position] = col.B;
                    position++;
                    rgbValues[position] = col.G;
                    position++;
                    rgbValues[position] = col.R;
                    position++;
                    rgbValues[position] = col.A;
                    position++;
                }
            }
            imageData.SetBuffer(rgbValues);
        }
        private void Raw16ToBmp(TifFileInfo fi,ImageData imageData, int C)
        {
            FrameCalculator FC = new FrameCalculator();
            //image array
            Color col = fi.LutList[C];
            if (fi.image16bit == null) return;
            ushort[][] image = fi.image16bit[FC.FrameC(fi,C)];
            if (image == null) return;
            imageData.GetImage(fi.sizeX, fi.sizeY);

            byte[] rgbValues = imageData.GetBuffer();
            //take LUT info
            int position = 0;
            foreach (ushort[] row in image)
            {
                foreach (ushort val in row)
                {
                    byte val1 = (byte)(fi.newAdjustedLUT[C][val]);

                    rgbValues[position] = col.B;
                    position++;
                    rgbValues[position] = col.G;
                    position++;
                    rgbValues[position] = col.R;
                    position++;
                    rgbValues[position] = val1;
                    position++;
                }
            }
            imageData.SetBuffer(rgbValues);
        }
        private void Raw8ToBmp(TifFileInfo fi, ImageData imageData, int C)
        {
            FrameCalculator FC = new FrameCalculator();
            //image array
            Color col = fi.LutList[C];
            if (fi.image8bit == null) return;
            byte[][] image = fi.image8bit[FC.FrameC(fi, C)];

            if (image == null) return;

            imageData.GetImage(fi.sizeX, fi.sizeY);

            byte[] rgbValues = imageData.GetBuffer();
            //take LUT info
            int position = 0;
            foreach (byte[] row in image)
            {
                foreach (byte val in row)
                {
                    byte val1 = (byte)(fi.newAdjustedLUT[C][val]);

                    rgbValues[position] = col.B;
                    position++;
                    rgbValues[position] = col.G;
                    position++;
                    rgbValues[position] = col.R;
                    position++;
                    rgbValues[position] = val1;
                    position++;
                }
            }
            imageData.SetBuffer(rgbValues);            
        }
        class ImageData : IDisposable
        {
            private Bitmap Bmp;
            private int ID;
            private byte[] Buffer;
            BitmapData bmpData;
            private IntPtr ptr;
            public void Dispose()
            {
                this.Buffer = null;
                if (this.Bmp != null) this.Bmp.Dispose();
                this.Bmp = null;
                this.bmpData = null;
            }
            public void ClearImage()
            {
                this.Buffer = null;
                if(this.Bmp!=null) this.Bmp.Dispose();
                this.Bmp = null;
                this.bmpData = null;
            }
            public ImageData()
            {
                this.ID = GL.GenTexture();
            }
           
            public byte[] GetBuffer()
            {
                // Lock the bitmap's bits.
                Rectangle rect = new Rectangle(0, 0, this.Bmp.Width, this.Bmp.Height);
                this.bmpData = this.Bmp.LockBits(rect, ImageLockMode.ReadWrite, this.Bmp.PixelFormat);
                // Get the address of the first line.
                this.ptr = bmpData.Scan0;
                //store rgb values
                int bytes = Math.Abs(bmpData.Stride) * this.Bmp.Height;

                if (this.Buffer==null || this.Buffer.Length != bytes) this.Buffer = new byte[bytes];

                return this.Buffer;
            }
            public void SetBuffer(byte[] Buffer)
            {
                this.Buffer = Buffer;
                // Copy the RGB values back to the bitmap
                System.Runtime.InteropServices.Marshal.Copy(this.Buffer, 0, this.ptr, this.Buffer.Length);
                // Unlock the bits.
                this.Bmp.UnlockBits(this.bmpData);
            }
            public Bitmap GetImage(int X, int Y)
            {
                if (this.Bmp == null || this.Bmp.Width != X || this.Bmp.Height != Y)
                    this.Bmp = new Bitmap(X, Y, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
                return this.Bmp;
            }
            public int GetID
            {
                get
                {
                    return this.ID;
                }
            }
        }
    }
}
