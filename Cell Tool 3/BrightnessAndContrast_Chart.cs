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
        private Label[] Labels = new Label[5];
        public BrightnessAndContrast_ChartPanel()
        {
            this.SuspendLayout();
            CA = new BrightnessAndContrast_Chart();
            CA.Location = new Point(28, 5);
            CA.Size = new Size(this.Width - 50, this.Height - 30);

            this.Controls.Add(CA);
            //CA.Dock = DockStyle.Fill;
            Panel labPanel = new Panel();
            labPanel.Dock = DockStyle.Bottom;
            labPanel.Height = 25;
            labPanel.Visible = false;
            this.Controls.Add(labPanel);
            CA.labelPanel = labPanel;

            for (int i = 0; i < Labels.Length; i++)
            {
                Label lab = new Label();
                lab.ForeColor = Color.White;
                labPanel.Controls.Add(lab);
                Labels[i] = lab;
            }

            this.BackColorChanged += new EventHandler(delegate (object sender, EventArgs e)
            {
                labPanel.BackColor = this.BackColor;
            });

            this.ForeColorChanged += new EventHandler(delegate (object sender, EventArgs e)
            {
                labPanel.ForeColor = this.ForeColor;
            });

            this.SizeChanged += new EventHandler(delegate (object sender, EventArgs e)
            {
                CA.Size = new Size(this.Width - 50, this.Height - 30);
                CalculateLabelsLocations();
            });

            this.ResumeLayout(true);
        }
        private void CalculateLabelsLocations()
        {
            int step = (int)(CA.Width / 4);

            CA.labelPanel.SuspendLayout();

            for (int i = 0, X = CA.Location.X; i < Labels.Length; i++, X += step)
            {
                Labels[i].Text = CA.Labels[i].ToString();
                Labels[i].Width = TextRenderer.MeasureText(Labels[i].Text, Labels[i].Font).Width;
                Labels[i].Location = new Point(X - Labels[i].Width / 2, 1);
            }

            CA.labelPanel.ResumeLayout(true);

        }
        public void DrawToScreen(TifFileInfo fi)
        {
            CA.DrawToScreen(fi);
            CalculateLabelsLocations();
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
            public Panel labelPanel;
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

                labelPanel.Visible = true;

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
                labelPanel.Visible = false;
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
                    if (ser.Enabled && ser.Points.Count() > 0)
                        foreach (Point p in ser.Points)
                        {
                            if (p.X > MaxX) MaxX = p.X;
                            if (p.Y > MaxY) MaxY = p.Y;
                        }

                MaxY = (int)(1.1 * MaxY) + 10;
                this.MaxX = MaxX;
                this.MaxY = MaxY;

                GL.Ortho(0.0, (double)MaxX, 0.0, (double)MaxY, -1.0, 1.0);

                //Set labels
                double step = MaxX / 5;
                double val = 0;

                for (int i = 1; i < 5; i++)
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
                    if (ser.Enabled && ser.Points.Count() > 0)
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