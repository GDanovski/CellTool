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
            bool isMaskImage = index > fi.sizeC;

            switch (fi.bitsPerPixel)
            {
                case 8:
                    Raw8ToBmp(fi, this.Images[index], C, isMaskImage);
                    break;
                case 16:
                    Raw16ToBmp(fi, this.Images[index], C, isMaskImage);
                    break;
            }
        }
        public void LoadTextures(TifFileInfo fi, int index)
        {
            for(int i = 0; i<index;i++)
                LoadImageTexture(Images[i].GetImage(fi.sizeX, fi.sizeY), Images[i].GetID);
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
        private void Raw16ToBmp(TifFileInfo fi,ImageData imageData, int C, bool isMaskImage)
        {
            FrameCalculator FC = new FrameCalculator();
            //image array
            Color col = fi.LutList[C];

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
                    byte val1 = (byte)(fi.adjustedLUT[C][val] * 255);

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
        private void Raw8ToBmp(TifFileInfo fi, ImageData imageData, int C, bool isMaskImage)
        {
            FrameCalculator FC = new FrameCalculator();
            //image array
            Color col = fi.LutList[C];

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
                    byte val1 = (byte)(fi.adjustedLUT[C][val] * 255);

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
