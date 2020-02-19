using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Cell_Tool_3
{
    /* 3 types of plot":
     * Ixy - the average intensity for all Z planes vs. X and Y (the so-called 2 1/2 D)
     * Izt - the integrated intensity in a XY ROI, vs. Z and T
     * Motion - displaying the path of a moving object through x, y, z.
     */
    public enum Plots { Ixy, Izt, Motion};

    /* This class extends the CTChart, adding functionality specific for 3D. */
    class CTChart_3D : CTChart
    {
        // rotation angles in degrees of the plot - these are not the same as the image rotations
        public int rotationX = 0, rotationY = 0; 

        // Keep track of the mouse position, for rotation purposes
        private Vector2 MousePosition = new Vector2(-1, -1);
              
        Cube cube1;          // The cube drawn around the plot, i.e. the axes of the plot.
        int maxSize;         // the max possible size when rotating the cube in 3D - equal to the diagonal of the cube
        float scale = 0.75f; // scale the plot

        public CTChart_Properties_3D Properties3D;  // properties of the plot - including the database itself
        public Rectangle rect; // the position of the plot on the GL Control
        public TifFileInfo fi; // the object containing image info
        const double DEG2RAD = Math.PI / 180f;      // when rotating, need to convert to radians

        // Locations of the axes labels.
        Point3d LabelX = new Point3d(0.5, 0.5, -0.5);
        Point3d LabelY = new Point3d(0.5, 0.5, -0.5);
        Point3d LabelZ = new Point3d(0.5, 0.5, 0.5); 

        public CTChart_3D(ImageAnalyser IA, TifFileInfo fi, int maxSize) : base(IA) {
            this.fi = fi;
            this.maxSize = maxSize;
            cube1 = new Cube(scale, scale, scale);

        }
         
        // Called upon mouse motion
        public void Rotate(PointF e)
        {
            if (Math.Abs(e.X - MousePosition.X) > Math.Abs(e.Y - MousePosition.Y))
                // Mouse movement in X is bigger, rotate on X 
                if (e.X > MousePosition.X) rotationX += 5; else rotationX -= 5;

            else
                // Mouse movement on Y is bigger, rotate on Y
                if (e.Y > MousePosition.Y) rotationY += 5; else rotationY -= 5;

            // Keep the angle within the 0-360 range
            if (rotationX > 360) rotationX -= 360;
            if (rotationY > 360) rotationY -= 360;
            if (rotationX < 0) rotationX += 360;
            if (rotationY < 0) rotationY += 360;

            // Update the mouse position to be the current one
            MousePosition.X = e.X;
            MousePosition.Y = e.Y;
        }

        public void CalculateLabelPositions(ref Point3d Px, ref Point3d Py, ref Point3d Pz)
        {
            int flip;           // to make sure labels are always outside the cube

            // Z axis label
            if ((rotationX >= 0 && rotationX <= 90) || (rotationX >= 180 && rotationX <= 270))
                flip = (rotationY > 180) ? -1 : 1;
            else
                flip = (rotationY > 180) ? 1 : -1;

            LabelZ.X = flip * Math.Abs(LabelZ.X);
            Pz = LabelZ.RotateX((Math.PI / 180f) * rotationY).RotateY((Math.PI / 180f) * rotationX);

            // X axis label
            if ((rotationX >= 0 && rotationX <= 90) || (rotationX >= 180 && rotationX <= 270))
                flip = (rotationY > 90 && rotationY < 270) ? 1 : -1;
            else
                flip = (rotationY > 90 && rotationY < 270) ? -1 : 1;

            LabelX.X = flip * Math.Abs(LabelX.X);
            Px = LabelX.RotateX((Math.PI / 180f) * rotationY).RotateY((Math.PI / 180f) * rotationX);

            // Y axis label
            if ((rotationY >= 0 && rotationY <= 90) || (rotationY >= 180 && rotationY <= 270))
                flip = 1;
            else flip = -1;

            LabelY.Y = flip * Math.Abs(LabelY.Y);
            Py = LabelY.RotateX((Math.PI / 180f) * rotationY).RotateY((Math.PI / 180f) * rotationX);
        }

        /* calculate the rotated positions of the axes labels */
        public void renderChartLabels(Rectangle rect, TifFileInfo fi, double minY, double maxY, double maxX,
            Point3d Px, Point3d Py, Point3d Pz)
        {
            // Now convert to bitmap
            if (Properties3D.yAxisLabel.Length < 6)
                BitmapFromString(fi, Properties3D.yAxisLabel, 
                    new PointF(0.5f * (1 - scale) * maxSize + rect.X + maxSize * scale * (float)(Py.X+0.5),
                               0.5f * (1 - scale) * maxSize + rect.Y + maxSize * scale * (float)(Py.Y+0.5)), true,
                    labelBG: new SolidBrush(Color.FromArgb(255, 50, 50, 50)),
                    labelCol: Brushes.White);
            else
                BitmapFromString(fi, Properties3D.yAxisLabel.Substring(0, 6), 
                    new PointF(0.5f * (1 - scale) * maxSize + rect.X + maxSize * scale * (float)(Py.X+0.5),
                               0.5f * (1 - scale) * maxSize + rect.Y + maxSize * scale * (float)(Py.Y+0.5)), true,
                    labelBG: new SolidBrush(Color.FromArgb(255, 50, 50, 50)),
                    labelCol: Brushes.White);

            BitmapFromString(fi, Properties3D.xAxisLabel, 
                new PointF(0.5f * (1 - scale) * maxSize + rect.X + maxSize * scale * (float)(Px.X+0.5),
                           0.5f * (1 - scale) * maxSize + rect.Y + maxSize * scale * (float)(Px.Y+0.5)), true,
                    labelBG: new SolidBrush(Color.FromArgb(255, 50, 50, 50)),
                    labelCol: Brushes.White);

            BitmapFromString(fi, Properties3D.zAxisTB.Text,
                new PointF(0.5f * (1 - scale) * maxSize + rect.X + maxSize * scale * (float)(Pz.X + 0.5),
                           0.5f * (1 - scale) * maxSize + rect.Y + maxSize * scale * (float)(Pz.Y + 0.5)), true,
                    labelBG: new SolidBrush(Color.FromArgb(255, 50, 50, 50)),
                    labelCol: Brushes.White);
        }
        public void Render()
        {

            if (fi == null || rect == null || Properties3D.data3d == null) return;

            Properties3D.Rescale();
            ResetCubeScale();

            if (Properties3D.PlotType == Plots.Motion) RescaleToFitSize(x: true, y: true, z: true);
            else if (Properties3D.PlotType == Plots.Ixy) RescaleToFitSize(x: true, y: true, z: false);

            float radius = 0.005f;
            int maxSize = Properties3D.maxSize;

            GL.LineWidth(1f);

            if (Properties3D.PlotType == Plots.Ixy || Properties3D.PlotType == Plots.Izt)
            {
                foreach (Point3d p in Properties3D.data3d)
                {
                    Point3d rotatedP = p.RotateX(rotationY * DEG2RAD).RotateY(rotationX * DEG2RAD);
                    GL.ClearColor(1.0f, 1.0f, 1.0f, 1.0f);


                    if (p.Z != -0.5)
                        DisplaySphere(p, rotatedP, radius, maxSize);

                }
            }
            GL.LineWidth(1.0f); // reset

            cube1.Show(rotationY * DEG2RAD, rotationX * DEG2RAD, 2, 10, 10, maxSize);

            Point3d Px = null, Py = null, Pz = null; // the location of each axis label
            CalculateLabelPositions(ref Px, ref Py, ref Pz);
            renderChartArea(Rescale: Properties3D.PlotType == Plots.Motion);
            renderChartLabels(rect, fi, 0, 1, 1, Px, Py, Pz);
            
            if (Properties3D.PlotType == Plots.Izt)
            {
                drawPlane(maxSize, 10, 10, axis: 0);
                drawPlane(maxSize, 10, 10, axis: 1);
            }

        }

        private void DisplaySphere(Point3d p, Point3d rotatedP, float radius, int maxSize)
        {
            GL.Begin(PrimitiveType.LineLoop);
            if (Properties3D.xyAxisSelectedIndex == 0)
                GL.Color4(1f, Math.Max(0.001, 0.5f + p.Z), 0f, 1f); // use gradient color
            else if (Properties3D.xyAxisSelectedIndex == 1)
            {
                GL.Color4(1f, 0f, 0f, 1f);

                if (Math.Abs((float)fi.frame / (fi.sizeT - 1) - 0.5 - rotatedP.Y) < 0.02f)
                    GL.Color4(1f, 1f, 0f, 1f);
                if (Math.Abs((float)fi.zValue / (fi.sizeZ - 1) - 0.5 - rotatedP.X) < 0.02f)
                    GL.Color4(1f, 1f, 0f, 1f);
            }

            if (p.X < -0.5 || p.X > 0.5 || p.Y < -0.5 || p.Y > 0.5 || p.Z < -0.5 || p.Z > 0.5) return;

            // Draw the sphere
            for (int i = 0; i < 360; i += 60)
            {
                float degInRad = (float)(i * DEG2RAD);
                double newX = (float)rotatedP.X + Math.Cos(degInRad) * radius;
                double newY = (float)rotatedP.Y + Math.Sin(degInRad) * radius;
                GL.Vertex2(0.5 * (1 - scale) * maxSize + rect.X + rect.Width * scale * (0.5f + newX),
                           0.5 * (1 - scale) * maxSize + rect.Y + rect.Height * scale * (0.5f + newY));
            }
            // inner circle
            for (int i = 0; i < 360; i += 60)
            {
                float degInRad = (float)(i * DEG2RAD);
                double newX = (float)rotatedP.X + Math.Cos(degInRad) * 0.5 * radius;
                double newY = (float)rotatedP.Y + Math.Sin(degInRad) * 0.5 * radius;
                GL.Vertex2(0.5 * (1 - scale) * maxSize + rect.X + rect.Width * scale * (0.5f + newX),
                           0.5 * (1 - scale) * maxSize + rect.Y + rect.Height * scale * (0.5f + newY));
            }

            GL.End();
        }

        private void DisplayLine(Point3d rotatedP, int maxSize, int PointIdx)
        {
            for (int i = 0; i < 2; i++)
            {
                GL.Vertex2(0.5 * (1 - scale) * maxSize + rect.X + rect.Width * scale * (0.5f + rotatedP.X),
                               0.5 * (1 - scale) * maxSize + rect.Y + rect.Height * scale * (0.5f + rotatedP.Y));
                if (Properties3D.MotionPathsStartIndices.Contains(PointIdx)) break;
            }
        }

        private void renderChartArea(bool Rescale)
        {
            int maxSize = Properties3D.maxSize;
            float W = 13f / (float)fi.zoom;
            float H = 15f / (float)fi.zoom;

            W *= 3;
            H *= 2;

            if (W > (float)rect.Width / 3 || H > (float)rect.Height / 3) return;

            RectangleF microRect = new RectangleF(rect.X + W, rect.Y + H, rect.Width - W - H, rect.Height - 2 * H - 0.5f * H);

            // Calculate the number of labels to display
            Point3d LowerLeftCorner = new Point3d(-0.5, 0.5, 0.5).RotateX(DEG2RAD * rotationY).RotateY(DEG2RAD * rotationX);
            Point3d LowerRightCorner = new Point3d(0.5, 0.5, 0.5).RotateX(DEG2RAD * rotationY).RotateY(DEG2RAD * rotationX);
            Point3d UpperLeftCorner = new Point3d(0.5, -0.5, 0.5).RotateX(DEG2RAD * rotationY).RotateY(DEG2RAD * rotationX);
            Point3d FarLeftCorner = new Point3d(-0.5, 0.5, -0.5).RotateX(DEG2RAD * rotationY).RotateY(DEG2RAD * rotationX);

            double stepX = 0.01 * fi.zoom * maxSize * (0.001 + LowerLeftCorner.Distance2DTo(LowerRightCorner));
            double stepY = 0.01 * fi.zoom * maxSize * (0.001 + LowerLeftCorner.Distance2DTo(UpperLeftCorner));
            double stepZ = 0.01 * fi.zoom * maxSize * (0.001 + LowerLeftCorner.Distance2DTo(FarLeftCorner));

            double valX = (Properties3D.maxX_chosen - Properties3D.minX_chosen) / stepX;
            double valY = (Properties3D.maxY_chosen - Properties3D.minY_chosen) / stepY;
            double valZ = (Properties3D.maxZ_chosen - Properties3D.minZ_chosen) / stepZ;

            int flip;
            string label;

            // Display the labels of the X axis
            for (double x = 0, i = Properties3D.minX_chosen; x <= microRect.Width; x += microRect.Width / stepX, i += valX)
            {

                if ((rotationY >= 0 && rotationY <= 90) || (rotationY >= 180 && rotationY <= 270))
                    flip = 1;
                else flip = -1;

                Point3d Px = new Point3d((float)x / maxSize - 0.5f, flip * 0.5f, -0.5f);
                    
                if (Rescale)
                {
                    Px.X *= cube1.X;
                    Px.Y *= cube1.Y;
                    Px.Z *= cube1.Z;
                }
                Px = Px.RotateX(rotationY * DEG2RAD).RotateY(rotationX * DEG2RAD);

                // Round the labels in case they are big
                if (Properties3D.maxX_chosen <= 1) label = i.ToString("0.0");
                else label = ((int)i).ToString();
                BitmapFromString(fi, label, 
                    new PointF(0.5f * (1 - scale) * maxSize + rect.X + maxSize * scale * (float)(Px.X+0.5),
                               0.5f * (1 - scale) * maxSize + rect.Y + maxSize * scale * (float)(Px.Y+0.5)),
                    labelBG: new SolidBrush(Color.FromArgb(255, 50, 50, 50)),
                    labelCol: Brushes.White);
            }

            // Display the labels of the Y axis
            for (double y = 0, i = Properties3D.minY_chosen; y <= microRect.Height; y += microRect.Height / stepY, i += valY)
            {
                Point3d Py;
                if ((rotationX >= 0 && rotationX <= 90) || (rotationX >= 180 && rotationX <= 270))
                    flip = (rotationY > 90 && rotationY < 270) ? 1 : -1;
                else
                    flip = (rotationY > 90 && rotationY < 270) ? -1 : 1;

                Py = new Point3d(flip * 0.5f, (float)y / maxSize - 0.5f, -0.5f);
                if (Rescale)
                {
                    Py.X *= cube1.X;
                    Py.Y *= cube1.Y;
                    Py.Z *= cube1.Z;
                }
                Py = Py.RotateX(rotationY * DEG2RAD).RotateY(rotationX * DEG2RAD);
                
                // Round the labels in case they are big
                if (Properties3D.maxY_chosen <= 1) label = i.ToString("0.0");
                else label = ((int)i).ToString();
                BitmapFromString(fi, label,
                    new PointF(0.5f * (1 - scale) * maxSize + rect.X + maxSize * scale * (float)(Py.X + 0.5),
                               0.5f * (1 - scale) * maxSize + rect.Y + maxSize * scale * (float)(Py.Y + 0.5)),
                    labelBG: new SolidBrush(Color.FromArgb(255, 50, 50, 50)),
                    labelCol: Brushes.White);
            }

            // Display the labels of the Z axis
            for (double z = 0, i = Properties3D.minZ_chosen; z <= microRect.Height; z += microRect.Height / stepZ, i += valZ)
            {
                
                if ((rotationX >= 0 && rotationX <= 90) || (rotationX >= 180 && rotationX <= 270))
                    flip = (rotationY > 180) ? -1 : 1;
                else
                    flip = (rotationY > 180) ? 1 : -1;

                Point3d Pz = new Point3d(flip * 0.5f, 0.5f, (float)z / maxSize - 0.5f);
                if (Rescale)
                {
                    Pz.X *= cube1.X;
                    Pz.Y *= cube1.Y;
                    Pz.Z *= cube1.Z;
                }
                Pz = Pz.RotateX(rotationY * DEG2RAD).RotateY(rotationX * DEG2RAD);
            
                // Round the labels in case they are big
                if (Properties3D.maxZ_chosen <= 1) label = i.ToString("0.0");
                else label = ((int)i).ToString();
                BitmapFromString(fi, label,
                    new PointF(0.5f * (1 - scale) * maxSize + rect.X + maxSize * scale * (float)(Pz.X + 0.5),
                               0.5f * (1 - scale) * maxSize + rect.Y + maxSize * scale * (float)(Pz.Y + 0.5)),
                    labelBG: new SolidBrush(Color.FromArgb(255, 50, 50, 50)),
                    labelCol: Brushes.White);
            }
        }

        public void AdjustOnClick()
        {
            Form OptionForm = new Form();
            OptionForm.FormBorderStyle = FormBorderStyle.FixedDialog;
            OptionForm.Width = 200;
            OptionForm.Height = 300;
            OptionForm.Text = "Adjust the 3D plot";
            OptionForm.StartPosition = FormStartPosition.CenterScreen;
            OptionForm.WindowState = FormWindowState.Normal;
            OptionForm.BackColor = Color.Gray;

            Button OkBtn = new Button();
            OptionForm.Tag = OkBtn;
            OkBtn.Text = "Apply";
            OkBtn.Width = 80;
            OkBtn.BackColor = SystemColors.ButtonFace;
            OkBtn.Location = new Point(50, 215);
            OptionForm.Controls.Add(OkBtn);

            ConfigureScaleTB(OptionForm, new Point(15, 45),  new Point(85, 40),  "X minimum", Properties3D.minX_chosen, OkBtn, 0, true);
            ConfigureScaleTB(OptionForm, new Point(15, 75),  new Point(85, 70),  "X maximum", Properties3D.maxX_chosen, OkBtn, 0, false);
            ConfigureScaleTB(OptionForm, new Point(15, 105), new Point(85, 100), "Y minimum", Properties3D.minY_chosen, OkBtn, 1, true);
            ConfigureScaleTB(OptionForm, new Point(15, 135), new Point(85, 130), "Y maximum", Properties3D.maxY_chosen, OkBtn, 1, false);
            ConfigureScaleTB(OptionForm, new Point(15, 165), new Point(85, 160), "Z minimum", Properties3D.minZ_chosen, OkBtn, 2, true);
            ConfigureScaleTB(OptionForm, new Point(15, 195), new Point(85, 190), "Z maximum", Properties3D.maxZ_chosen, OkBtn, 2, false);

            OptionForm.ShowDialog();
        }

        private void ConfigureScaleTB(Form OptionForm, 
            Point LabelLocation, Point TBLocation, 
            String TBname, float DefaultValue, Button OkBtn, int axis, bool IsMin)
        {
            TextBox tb = new TextBox();
            {
                Label l = new Label();
                l.Text = TBname;
                l.Width = TextRenderer.MeasureText(l.Text, l.Font).Width;
                l.ForeColor = Color.White;
                l.Location = LabelLocation;
                OptionForm.Controls.Add(l);

                tb.Location = TBLocation;
                tb.Width = 75;
                tb.Height = 20;
                tb.Text = DefaultValue.ToString();
                OptionForm.Controls.Add(tb);
                tb.BringToFront();
            }

            OkBtn.Click += new EventHandler(delegate (object o, EventArgs a)
            {
                float value = 0;
                try { value = float.Parse(tb.Text); }
                catch
                {
                    MessageBox.Show("Incorrect minimum value!");
                    tb.Focus();
                    return;
                }
                
                OptionForm.Close();
                AdjustScale(axis: axis, IsMin: IsMin, value: value);
                Properties3D.Rescale();
            });
        }

        private void AdjustScale(int axis, bool IsMin, float value)
        {
            
            switch (axis)
            {
                case 0:
                    if (IsMin) Properties3D.minX_chosen = value;    else Properties3D.maxX_chosen = value;      break;

                case 1:
                    if (IsMin) Properties3D.minY_chosen = value;    else Properties3D.maxY_chosen = value;      break;

                case 2:
                    if (IsMin) Properties3D.minZ_chosen = value;    else Properties3D.maxZ_chosen = value;      break;
            }
            Render();
        }

        private void RescaleToFitSize(bool x, bool y, bool z)
        {
            cube1 = new Cube(x ? (float)fi.sizeX / maxSize : scale,
                             y ? (float)fi.sizeY / maxSize : scale,
                             z ? (float)fi.sizeZ / maxSize : scale);

            if (x & y & z)
            {
                foreach (Point3d p in Properties3D.data3d)
                {
                    p.X = ((float)fi.sizeX / maxSize) * p.X;
                    p.Y = ((float)fi.sizeY / maxSize) * p.Y;
                    p.Z = ((float)fi.sizeZ / maxSize) * p.Z;
                }

                LabelX.X *= (float)fi.sizeX / maxSize;
                LabelX.Y *= (float)fi.sizeY / maxSize;
                LabelX.Z *= (float)fi.sizeZ / maxSize;

                LabelY.X *= (float)fi.sizeX / maxSize;
                LabelY.Y *= (float)fi.sizeY / maxSize;
                LabelY.Z *= (float)fi.sizeZ / maxSize;

                LabelZ.X *= (float)fi.sizeX / maxSize;
                LabelZ.Y *= (float)fi.sizeY / maxSize;
                LabelZ.Z *= (float)fi.sizeZ / maxSize;
            }

        }

        private void ResetCubeScale() {
            cube1 = new Cube(scale, scale, scale);
            LabelX = new Point3d(0.6, 0.4, -0.5);
            LabelY = new Point3d(0.4, 0.5, -0.6);
            LabelZ = new Point3d(0.5, 0.5, 0.4);
        }

        private void drawPlane(int maxSize, int leftMargin, int topMargin, int axis)
        {
            // Find out which T and Z we are at
            float fractionT = (float)fi.frame / (fi.sizeT-1);
            float fractionZ = (float)fi.zValue / (fi.sizeZ-1);

            GL.Color4(1f, 1f, 0f, 0.1f);
            
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.Enable(EnableCap.Blend);
            GL.DepthMask(false);
            GL.Begin(PrimitiveType.Quads);

            Point3d p1 = (axis == 0) ? new Point3d(-0.4, scale * (fractionT - 0.5f), -0.4) :
                new Point3d(scale * (fractionZ - 0.5f), -0.4, -0.4);
            p1 = p1.RotateX(rotationY * Math.PI / 180).RotateY(rotationX * Math.PI / 180f);

            Point3d p2 = (axis == 0) ? new Point3d(-0.4, scale * (fractionT - 0.5f), 0.4) :
                new Point3d(scale * (fractionZ - 0.5f), -0.4, 0.4);
            p2 = p2.RotateX(rotationY * Math.PI / 180f).RotateY(rotationX * Math.PI / 180f);

            Point3d p3 = (axis == 0) ? new Point3d(0.4, scale * (fractionT - 0.5f), 0.4) :
                new Point3d(scale * (fractionZ - 0.5f), 0.4,  0.4);
            p3 = p3.RotateX(rotationY * Math.PI / 180f).RotateY(rotationX * Math.PI / 180f);

            Point3d p4 = (axis == 0) ? new Point3d(0.4, scale * (fractionT - 0.5f), -0.4) :
                 new Point3d(scale * (fractionZ - 0.5f), 0.4, -0.4);
            p4 = p4.RotateX(rotationY * Math.PI / 180f).RotateY(rotationX * Math.PI / 180f);

            GL.Vertex2(2 * maxSize + 3 * leftMargin + maxSize * (p1.X + 0.5f), topMargin + maxSize * (p1.Y + 0.5f));
            GL.Vertex2(2 * maxSize + 3 * leftMargin + maxSize * (p2.X + 0.5f), topMargin + maxSize * (p2.Y + 0.5f));
            GL.Vertex2(2 * maxSize + 3 * leftMargin + maxSize * (p3.X + 0.5f), topMargin + maxSize * (p3.Y + 0.5f));
            GL.Vertex2(2 * maxSize + 3 * leftMargin + maxSize * (p4.X + 0.5f), topMargin + maxSize * (p4.Y + 0.5f));

            GL.End();
            GL.Disable(EnableCap.Blend);
            GL.DepthMask(true);
        }
    }
}
