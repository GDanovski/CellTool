using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using System.Drawing;
using System.Windows.Forms;

namespace Cell_Tool_3
{
    class BrightnessAndContrast_ChartPanel : Panel
    {
        public BrightnessAndContrast_Chart CA;
        public BrightnessAndContrast_ChartPanel()
        {
            CA = new BrightnessAndContrast_Chart();
            this.Controls.Add(CA);
            CA.Dock = DockStyle.Fill;
        }
        public void DrawToScreen(TifFileInfo fi)
        {
            CA.DrawToScreen(fi);
        }
        public List<BrightnessAndContrast_Chart.Series> Series
        {
            get
            {
                return this.CA._Series;
            }
            set
            {
                this.CA._Series = value;
            }
        }
        public class BrightnessAndContrast_Chart : GLControl
        {
            public Color BackGroundColor = Color.Black;
            public List<Series> _Series = new List<Series>();
            public int[] Labels = new int[5];

            public bool ShowMinAndMax = false;
            private int Min;
            private int Max;

            private float[] LUT;
            private float[] LUTColor = new float[3];
            private int MaxX;
            private int MaxY;

            public BrightnessAndContrast_Chart()
            {
                this.MakeCurrent();
                GL.ClearColor(BackGroundColor);

                this.Paint += GLControl_Paint;
                this.Resize += GLControl_Resize;
                //this.MouseClick += GLControl_MouseClick;
            }
            public int PixelPositionToValue(int X)
            {
                return (int)((((double)X) / ((double)this.Width)) * ((double)MaxX));
            }
            public int ValueToPixelPosition(int X)
            {
                return (int)((((double)X) / ((double)MaxX)) * ((double)this.Width));
            }
            public void DrawToScreen(TifFileInfo fi)
            {
                if (fi == null)
                {
                    ClearImage();
                    
                    return;
                }

                LUT = fi.adjustedLUT[fi.cValue];
                Min = fi.MinBrightness[fi.cValue];
                Max = fi.MaxBrightness[fi.cValue];

                LUTColor[0] = (float)(fi.LutList[fi.cValue].R / 255f);
                LUTColor[1] = (float)(fi.LutList[fi.cValue].G / 255f);
                LUTColor[2] = (float)(fi.LutList[fi.cValue].B / 255f);

                GLDrawing_Start();
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

            private void GLDrawing_Start()
            {
                if (this.Visible == false) { this.Visible = true; }
                //Activate Control
                this.MakeCurrent();
                GL.Disable(EnableCap.Texture2D);
                //Load background
                GL.ClearColor(BackGroundColor);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                //Prepare MatrixMode
                GL.MatrixMode(MatrixMode.Projection);
                GL.LoadIdentity();
                //Prepare Projection                
                SetProjection();

                GL.MatrixMode(MatrixMode.Modelview);
                //Set viewpoint
                GL.Viewport(0, 0, this.Width, this.Height);
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
            private void SetProjection()
            {
                //GL.Ortho(0.0, (double)this.Width, (double)this.Height, 0.0, -1.0, 1.0);

                int MaxX = 0;
                int MaxY = 0;

                foreach (Series ser in _Series)
                    foreach (Point p in ser.Points)
                    {
                        if (p.X > MaxX) MaxX = p.X;
                        if (p.Y > MaxY) MaxY = p.Y;
                    }
                this.MaxX = MaxX;
                this.MaxY = MaxY;

                MaxY = (int)(1.1 * MaxY)+10;                
               
                GL.Ortho(0.0, (double)MaxX, 0.0, (double)MaxY, -1.0, 1.0);

                //Set labels
                double step = MaxX / 5;
                double val = 0;

                for(int i =1; i < 5; i++)
                {
                    val += step;
                    Labels[i] = (int)val;
                }
            }
            #endregion GLControl_Events

            #region Rendering
            private void Render()
            {
                //GL.ShadeModel(ShadingModel.Flat);
                foreach (Series ser in _Series)
                    if (ser.Enabled)
                    {
                        
                        DrawSeriesSingleColor(ser, Color.Black);
                        GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.One);
                        if (!ser.UseGradientStyle)
                            DrawSeriesSingleColor(ser, ser.Color);
                        else
                        {
                            GL.Enable(EnableCap.Blend);
                            DrawSeriesGradientColor(ser);
                            GL.Disable(EnableCap.Blend);
                        }
                        DrawSeriesBorder(ser);
                    }

                if (ShowMinAndMax)
                {
                    DrawLine(Min);
                    DrawLine(Max);
                }
            }
            private void DrawLine(int X)
            {
                GL.Begin(PrimitiveType.Lines);

                GL.Color3(Color.White);

                GL.Vertex2(X, MaxY);
                GL.Vertex2(X, 0);

                GL.End();
            }

            private void DrawSeriesSingleColor(Series ser, Color col)
            {
                if (ser.Points.Count() == 0) return;
                GL.Begin(PrimitiveType.TriangleStrip);

                GL.Color3(col);

                foreach (Point p in ser.Points)
                {
                    GL.Vertex2(p.X, p.Y);
                    GL.Vertex2(p.X, 0);
                }

                GL.End();
            }
            private void DrawSeriesGradientColor(Series ser)
            {
                if (ser.Points.Count() == 0) return;
                GL.Begin(PrimitiveType.TriangleStrip);
                
                foreach (Point p in ser.Points)
                {
                    if (LUT.Length > p.X)
                        GL.Color4(LUTColor[0], LUTColor[1], LUTColor[2], LUT[p.X]);
                    else
                        GL.Color4(LUTColor[0], LUTColor[1], LUTColor[2], LUT[LUT.Length - 1]);

                    GL.Vertex2(p.X, p.Y);
                    GL.Vertex2(p.X, 0);

                }

                GL.End();
            }
            private void DrawSeriesBorder(Series ser)
            {
                if (ser.Points.Count() == 0) return;

                GL.Begin(PrimitiveType.LineStrip);

                GL.Color3(ser.BorderColor);

                GL.Vertex2(ser.Points[0].X, 0);

                foreach (Point p in ser.Points)
                {
                    GL.Vertex2(p.X, p.Y);
                }

                GL.Vertex2(ser.Points[ser.Points.Count - 1].X, 0);

                GL.End();
            }
            #endregion Rendering
            public class Series
            {
                public bool Enabled = true;
                public bool UseGradientStyle = false;

                public Color Color = Color.Black;
                public Color BackSecondaryColor = Color.White;
                public Color BorderColor = Color.White;

                public points Points = new points();

                public class points : List<Point>
                {
                    public void AddXY(int X, int Y)
                    {
                        this.Add(new Point(X, Y));
                    }
                }
            }
        }
        
    }
}
