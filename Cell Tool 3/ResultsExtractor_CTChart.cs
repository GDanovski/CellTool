using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using System.Drawing.Imaging;

namespace Cell_Tool_3
{
    class ResultsExtractor_CTChart : GLControl
    {
        private ResultsExtractor.MyForm form1;
        private Color _BackGroundColor = Color.White;
        private List<Series> _Series = new List<Series>();
        private int id;

        public Color BackGroundColor
        {
            get
            {
                return this._BackGroundColor;
            }
            set
            {
                this._BackGroundColor = value;
            }
        }
        public List<Series> ChartSeries
        {
            get
            {
                return this._Series;
            }
            set
            {
                this._Series = value;
            }
        }
        
        public void Build(ResultsExtractor.MyForm form1)
        {
            this.form1 = form1;
            
            //this.MakeCurrent();

            ReserveTextureID();

            this.Paint += GLControl_Paint;
            this.Resize += GLControl_Resize;
            
            GL.ClearColor(BackGroundColor);
        }
       
        #region GLControl_Events
        private void ClearImage()
        {
            //Activate Control
            this.MakeCurrent();
            //Load background
            GL.ClearColor(BackGroundColor);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            //Prepare MatrixMode
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();

            this.SwapBuffers();
        }

        private void GLControl_Resize(object sender, EventArgs e)
        {
            GLDrawing_Start();
        }

        private void GLControl_Paint(object sender, EventArgs e)
        {
            //Global variables
            GLDrawing_Start();
        }

        public void GLDrawing_Start()
        {
            if (this.Visible == false) { this.Visible = true; }

            this.MakeCurrent();
            GL.Disable(EnableCap.Texture2D);
            //Load background
            GL.ClearColor(Color.White);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            //Prepare MatrixMode
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            //Prepare Projection
            GL.Ortho(0.0, (double)this.Width, (double)this.Height, 0.0, -1.0, 1.0);

            GL.MatrixMode(MatrixMode.Modelview);
            //Set viewpoint
            //Set viewpoint
            GL.Viewport(1000, 30 + 30 + 1 + 1, this.Width, this.Height);

            //draw chart            
            try
            {
                Render();
            }
            catch
            {
                ClearImage();
            }
            

            this.SwapBuffers();
        }
        
        #endregion GLControl_Events

        #region Rendering
        private void ReserveTextureID()
        {
            id = GL.GenTexture();
        }
        private void BitmapFromString(string str, PointF p, bool title = false)
        {
            Font font = new Font("Times New Roman", 9, FontStyle.Regular);
            if (title) font = new Font("Times New Roman", 9, FontStyle.Bold);

            Bitmap bmp = new Bitmap(TextRenderer.MeasureText(str, font).Width,
                TextRenderer.MeasureText(str, font).Height,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            RectangleF rect = new Rectangle(0, 0,
                TextRenderer.MeasureText(str, font).Width,
                TextRenderer.MeasureText(str, font).Height);

            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.FillRectangle(Brushes.White, rect);
                g.DrawString(str, font, Brushes.Black, rect);
                g.Flush();
            }

            int ID = LoadTexture(bmp);

            GL.Enable(EnableCap.Texture2D);

            GL.BindTexture(TextureTarget.Texture2D, ID);

            rect = new RectangleF(p.X - rect.Width / 2, p.Y - rect.Height / 2,
                p.X + rect.Width / 2, p.Y + rect.Height / 2);

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

            GL.Disable(EnableCap.Texture2D);
        }
        private int LoadTexture(Bitmap bmp)
        {
            //Load texture from file
            Bitmap texture_source = bmp;

            //Link empty texture to texture2d
            GL.BindTexture(TextureTarget.Texture2D, id);

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

            return id;
        }
        private RectangleF rect;
        private double stepX = 0;
        private double stepY = 0;
        private double MinY = 0;

        private void Render()
        {
           

            Rectangle OldRect = new Rectangle(0, 0, this.Width, this.Height);

            double MaxY = double.MinValue;
            double MaxX = double.MinValue;
            MinY = double.MaxValue;

            {
                PointF p;

                foreach (Series ser in _Series)
                    if (ser.Enabled && ser.Points.Count() > 0)
                        for (int i = 0; i < ser.Points.Count; i++)
                            if (!ser.ErrorBar)
                            {
                                p = ser.Points[i];
                                if (p.X > MaxX) MaxX = p.X;
                                if (p.Y > MaxY) MaxY = p.Y;
                                if (p.Y < MinY) MinY = p.Y;
                            }
                            else
                            {
                                p = ser.Points[i];

                                if (p.X > MaxX) MaxX = p.X;
                                if (p.Y + ser.ErrorVals[i] > MaxY) MaxY = p.Y + ser.ErrorVals[i];
                                if (p.Y - ser.ErrorVals[i] < MinY) MinY = p.Y - ser.ErrorVals[i];
                            }

                p = PointF.Empty;
            }
            rect = renderChartArea(OldRect, MinY, MaxY, MaxX);

            //load series
            stepX = rect.Width / MaxX;
            stepY = rect.Height / (MaxY - MinY);

            double lineW = 1;
            double[] y;
            double[] x;
            foreach (Series ser in _Series)
                if (ser.Enabled && ser.Points.Count() > 0)
                    if (!ser.ErrorBar)
                    {
                        GL.Enable(EnableCap.LineSmooth);

                        x = new double[ser.Points.Count];
                        y = new double[ser.Points.Count];

                        for (int i = 0; i< ser.Points.Count; i++)
                        {
                            x[i] = rect.X + ser.Points[i].X * stepX;
                            y[i] = rect.Y + rect.Height - (ser.Points[i].Y - MinY) * stepY;
                        }
                        
                        for (float cor = -0.5f; cor < 1.0f; cor++)
                        {
                            GL.Begin(PrimitiveType.LineStrip);
                            GL.Color4(ser.Color);
                            if (ser.Points.Count == 1)
                            {
                                GL.Vertex2(x[0] + cor, y[0]);
                                GL.Vertex2(x[0] + cor, rect.Y + rect.Height);
                            }
                            else
                            {
                                for (int i = 0; i < x.Length; i++)
                                    GL.Vertex2(x[i], y[i] + cor);
                            }
                            
                            GL.End();
                        }

                        GL.Disable(EnableCap.LineSmooth);
                    }
                    else
                    {
                        GL.Enable(EnableCap.Blend);
                        GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
                        
                        PointF p;
                        double dev, Ydown, Yup, X;


                        GL.Begin(PrimitiveType.Lines);
                        GL.Color4(ser.Color.R, ser.Color.G , ser.Color.B, ser.Color.A);
                        for (int i = 0; i < ser.Points.Count; i++)
                        {
                            p = ser.Points[i];
                            dev = ser.ErrorVals[i];

                            Ydown = rect.Y + rect.Height - ((p.Y + dev) - MinY) * stepY;
                            Yup = rect.Y + rect.Height - ((p.Y - dev) - MinY) * stepY;
                            X = rect.X + p.X * stepX;

                            GL.Vertex2(X-lineW, Ydown);
                            GL.Vertex2(X + lineW, Ydown);

                            GL.Vertex2(X - lineW, Yup);
                            GL.Vertex2(X + lineW, Yup);

                            GL.Vertex2(X, Ydown);
                            GL.Vertex2(X, Yup);
                        }
                        GL.End();

                        p = PointF.Empty;


                        GL.Disable(EnableCap.Blend);
                    }
            
        }
        private RectangleF renderChartArea(Rectangle rect, double minY, double maxY, double maxX)
        {
            float W = 13f;
            float H = 15f;

            W *= 3;
            H *= 2;

            if (W > (float)rect.Width / 3 || H > (float)rect.Height / 3) return rect;

            GL.Begin(PrimitiveType.LineStrip);
            GL.Color3(Color.Black);

            GL.Vertex2(rect.X + W, rect.Y + H);
            GL.Vertex2(rect.X + W, rect.Y + rect.Height - H - H * 0.5);
            GL.Vertex2(rect.X + rect.Width - H, rect.Y + rect.Height - H - H * 0.5);

            GL.End();

            RectangleF microRect = new RectangleF(rect.X + W, rect.Y + H, rect.Width - W - H, rect.Height - 2 * H - 0.5f * H);

            double stepX = microRect.Width / (40);
            double stepY = microRect.Height / (30);

            double valX = maxX / stepX;
            double valY = (maxY - minY) / stepY;

            for (double x = microRect.X, i = 0; x <= microRect.X + microRect.Width; x += microRect.Width / stepX, i += valX)
            {
                GL.Begin(PrimitiveType.Lines);
                GL.Color3(Color.Black);

                GL.Vertex2(x, rect.Y + rect.Height - H - 0.5f * H);
                GL.Vertex2(x, rect.Y + rect.Height - 0.5f * H - H * 0.8);

                GL.End();

                double i1 = Math.Round(i, 1);

                if ((i1).ToString().Length > 6)
                    BitmapFromString((i1).ToString("0.0E0"), new PointF((float)x, (rect.Y + rect.Height - 0.5f * H - H / 2)));
                else
                    BitmapFromString((i1).ToString(), new PointF((float)x, (rect.Y + rect.Height - 0.5f * H - H / 2)));
            }

            for (double y = microRect.Y + microRect.Height, i = minY; y >= microRect.Y; y -= microRect.Height / stepY, i += valY)
            {
                GL.Begin(PrimitiveType.Lines);
                GL.Color3(Color.Black);

                GL.Vertex2(rect.X + W, y);
                GL.Vertex2(rect.X + W - H * 0.2, y);

                GL.End();

                double i1 = Math.Round(i, 1);

                if ((i1).ToString().Length > 5)
                    BitmapFromString((i1).ToString("0.0E0"), new PointF(rect.X + W / 2, (float)y));
                else
                    BitmapFromString((i1).ToString(), new PointF(rect.X + W / 2, (float)y));
            }
            if (form1 != null)
            {
                if (form1.dataTV.YaxisTitle.Length < 6)
                    BitmapFromString(form1.dataTV.YaxisTitle, new PointF(rect.X + W / 2 + 1, rect.Y + H / 2), true);
                else
                    BitmapFromString(form1.dataTV.YaxisTitle.Substring(0, 6), new PointF(rect.X + W / 2 + 1, rect.Y + H / 2), true);

                BitmapFromString(form1.dataTV.XaxisTitle, new PointF(rect.X + rect.Width / 2, (rect.Y + rect.Height - H / 2)), true);
            }
            return microRect;

        }
       

        private void DrawSeries(Series ser)
        {
            if (ser.Points.Count() == 0) return;

            Color col = ser.Color;

            GL.Begin(PrimitiveType.LineStrip);

            GL.Color3(col);

            foreach (PointF p in ser.Points)
            {
                GL.Vertex2(p.X, p.Y);
            }

            GL.End();
        }

        #endregion Rendering
        public class ChartAreaSettings
        {
            private List<double> _Xvals = new List<double>();
            private List<double> _Yvals = new List<double>();
            public List<double> Xvals
            {
                get
                {
                    return this._Xvals;
                }
                set
                {
                    this._Xvals = value;
                }
            }
            public List<double> Yvals
            {
                get
                {
                    return this._Yvals;
                }
                set
                {
                    this._Yvals = value;
                }
            }

        }
        public class Series
        {
            private bool _Enabled = true;
            private bool _ErrorBar = false;
            private Color _Color = Color.Black;
            private points _Series = new points();
            private List<double> _ErrorVals = new List<double>();

            public bool Enabled
            {
                get
                {
                    return this._Enabled;
                }
                set
                {
                    this._Enabled = value;
                }
            }
            public bool ErrorBar
            {
                get
                {
                    return this._ErrorBar;
                }
                set
                {
                    this._ErrorBar = value;
                }
            }
            public Color Color
            {
                get
                {
                    return this._Color;
                }
                set
                {
                    this._Color = value;
                }
            }
            public points Points
            {
                get
                {
                    return this._Series;
                }
                set
                {
                    this._Series = value;
                }
            }
            public List<double> ErrorVals
            {
                get
                {
                    return this._ErrorVals;
                }
                set
                {
                    this._ErrorVals = value;
                }
            }

            public class points : List<PointF>
            {
                public void AddXY(double X, double Y)
                {
                    this.Add(new PointF((float)X, (float)Y));
                }
            }
            
        }
    }
}

