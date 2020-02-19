using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Cell_Tool_3
{
    class CTChart_Properties_3D
    {
        //controls
        public ImageAnalyser IA;
        private PropertiesPanel_Item PropPanel;
        public Panel panel;
        public ComboBox xyAxisTB, zAxisTB;
        public int xyAxisSelectedIndex = -1, zAxisSelectedIndex = -1;
        public string xAxisLabel = "x", yAxisLabel = "y", zAxisLabel = "MeanZ (x, y)";
        public List<Point3d> data3d, data3d_original;
        private float minX, maxX, minY, maxY, minZ, maxZ;
        public float minX_chosen, maxX_chosen, minY_chosen, maxY_chosen, minZ_chosen, maxZ_chosen;
        public int maxSize;
        private bool bounds_set = false;
        public Plots PlotType;

        public ushort[] ImageProjection_1d;
        float[] Xdata, Ydata;
        public float[,] Zdata;
        public List<int> MotionPathsStartIndices;
        public List<SphereROI> rois;

        public CTChart_Properties_3D (Panel propertiesPanel, Panel PropertiesBody, ImageAnalyser IA, int maxSize,
            ushort[] ImageProjection_1d, float[] Xdata, float[] Ydata, float[,] Zdata) 
        {
            this.IA = IA;
            this.ImageProjection_1d = ImageProjection_1d;
            this.Xdata = Xdata;
            this.Ydata = Ydata;
            this.Zdata = Zdata;
            this.maxSize = maxSize;
            PropPanel = new PropertiesPanel_Item();
            PropPanel_Initialize(propertiesPanel, PropertiesBody);
        }

        private void PropPanel_Initialize(Panel propertiesPanel, Panel PropertiesBody)
        {
            //PropPanel properties
            PropPanel.Initialize(propertiesPanel);
            PropPanel.TitleColor(Color.CornflowerBlue);
            PropPanel.Resizable = false;
            PropPanel.Name.Text = "Chart Properties 3D";
            PropertiesBody.Controls.Add(PropPanel.Panel);

            panel = PropPanel.Panel;

            panel.Visible = true;

            BuildOptions();

            MotionPathsStartIndices = new List<int>();
        }
        public void BuildOptions()
        {
            // Build the X combo box
            Label xAxisLabel = new Label();
            xAxisLabel.Text = "X, Y:";
            xAxisLabel.Location = new Point(5, 35);
            panel.Controls.Add(xAxisLabel);
            xAxisLabel.BringToFront();
            xyAxisTB = new ComboBox();
            xyAxisTB.Location = new Point(50, 32);
            xyAxisTB.Width = 65;
            panel.Controls.Add(xyAxisTB);
            xyAxisTB.BringToFront();
            xyAxisTB.DropDownStyle = ComboBoxStyle.DropDownList;
            xyAxisTB.Items.Clear();
            xyAxisTB.Items.AddRange(new string[] { "x, y", "Z, T" });
            xyAxisTB.SelectedIndex = 0;
            xyAxisTB.AutoSize = false;
            xyAxisTB.SelectedIndexChanged += xAxisTB_ChangeIndex;


            // Build the Z combo box
            Label zAxisLabel = new Label();
            zAxisLabel.Text = "Z axis:";
            zAxisLabel.Location = new Point(5, 59);
            panel.Controls.Add(zAxisLabel);
            zAxisLabel.BringToFront();
            zAxisTB = new ComboBox();
            zAxisTB.Location = new Point(50, 57);
            zAxisTB.Width = 65;
            panel.Controls.Add(zAxisTB);
            zAxisTB.BringToFront();
            zAxisTB.Items.AddRange(new string[] { "Mean", "Max", "Total", "Area", "Z" });
            zAxisTB.DropDownStyle = ComboBoxStyle.DropDownList;
            zAxisTB.SelectedIndex = 0;
            zAxisTB.AutoSize = false;
            zAxisTB.SelectedIndexChanged += zAxisTB_ChangeIndex;
        }

        private void xAxisTB_ChangeIndex(object sender, EventArgs e)
        {
            if (xyAxisTB.Focused == false) return;
            Refresh();
        }
        private void zAxisTB_ChangeIndex(object sender, EventArgs e)
        {
            if (zAxisTB.Focused == false) return;
            Refresh();

        }

        public void Refresh(TifFileInfo fi = null)
        {
            xyAxisSelectedIndex = xyAxisTB.SelectedIndex;
            zAxisSelectedIndex = zAxisTB.SelectedIndex;

            if (xyAxisSelectedIndex == 1)
            {
                bounds_set = false;
                xAxisLabel = "Z";
                yAxisLabel = "T";
                zAxisLabel = "MeanXY (Z, T)";
                PlotType = Plots.Izt;
                Generate3Dplot(update: true);
            }

            if (xyAxisSelectedIndex == 0) {
                
                if (!zAxisTB.Items.Contains("Z")) zAxisTB.Items.Add("Z");
                if (zAxisSelectedIndex == 4)
                {
                    bounds_set = false;
                    xAxisLabel = "x";
                    yAxisLabel = "y";
                    zAxisLabel = "z";
                    PlotType = Plots.Motion;
                    GenerateMotionPlot(fi, update: true);
                }
                else
                {
                    bounds_set = false;
                    xAxisLabel = "x";
                    yAxisLabel = "y";
                    zAxisLabel = "MeanZ (x, y)";
                    PlotType = Plots.Ixy;
                    Generate2D_5(fi, update: true);
                }
            } else
            {
                if (zAxisTB.Items.Contains("Z")) zAxisTB.Items.Remove("Z");
            }
        }
        public void Generate3Dplot(bool update = false)
        {
            if (data3d != null && !update) return;
            if (Zdata == null) return;

            minX = 1000000; maxX = 0; minY = 1000000; maxY = 0; minZ = 1000000; maxZ = 0;
            for (int i = 0; i < Xdata.Length; i++)
            {
                if (Xdata[i] > maxX) maxX = Xdata[i];
                if (Xdata[i] < minX) minX = Xdata[i];
            }
            for (int i = 0; i < Ydata.Length; i++)
            {
                if (Ydata[i] > maxY) maxY = Ydata[i];
                if (Ydata[i] < minY) minY = Ydata[i];
            }
            for (int i = 0; i < Zdata.GetLength(0); i++)
                for (int j = 0; j < Zdata.GetLength(1); j++)
                {
                    if (Zdata[i, j] > maxZ) maxZ = Zdata[i, j];
                    if (Zdata[i, j] < minZ) minZ = Zdata[i, j];
                }
            data3d = new List<Point3d>();
            data3d_original = new List<Point3d>();
            for (int x = 0; x < Xdata.Length; x++)
                for (int y = 0; y < Ydata.Length; y++)
                {
                    data3d_original.Add(new Point3d(Xdata[x], Ydata[y], Zdata[x, y]));
                    data3d.Add(
                        new Point3d(
                            (Xdata[x] - minX) / (maxX - minX) - 0.5,
                            (Ydata[y] - minY) / (maxY - minY) - 0.5,
                            (Zdata[x, y] - minZ) / (maxZ - minZ) - 0.5)
                        );
                }

            minX_chosen = minX; maxX_chosen = maxX;
            minY_chosen = minY; maxY_chosen = maxY;
            minZ_chosen = minZ; maxZ_chosen = maxZ;

        }
        public void GenerateMotionPlot(TifFileInfo fi, bool update = false)
        {
            if (data3d != null && !update) return;
            if (fi == null) return;
            if (rois == null || rois.Count == 0) return;

            minX = -0.5f * fi.sizeX / maxSize; maxX = -minX;
            minY = -0.5f * fi.sizeY / maxSize; maxY = -minY;
            minZ = -0.5f * fi.sizeZ / maxSize; maxZ = -minZ;

            data3d = new List<Point3d>();
            data3d_original = new List<Point3d>();
            
            minX_chosen = 0; maxX_chosen = 2 * maxX;
            minY_chosen = 0; maxY_chosen = 2 * maxY;
            minZ_chosen = 0; maxZ_chosen = 2 * maxZ;
        }
        public void Generate2D_5(TifFileInfo fi, List<SphereROI> rois = null, bool update = false)
        {
            if (data3d != null && !update) return;

            data3d = new List<Point3d>();
            data3d_original = new List<Point3d>();

            if (!bounds_set)
            {
                bounds_set = true;

                minX = 0; maxX = maxSize;
                minY = 0; maxY = maxSize;
                minZ = 1000000; maxZ = 0;

                for (int i = 0; i < ImageProjection_1d.Length; i++)
                {
                    int x = i / maxSize;
                    int y = i % maxSize;
                    /*
                    float X_scaled = (float)x / maxSize - 0.5f;
                    float Y_scaled = (float)y / maxSize - 0.5f;

                    bool displayPoint = true;
                    if (rois != null)
                    {
                        foreach (SphereROI roi in rois)
                        {

                            if (Math.Abs(roi.center.X - X_scaled) > roi.radius ||
                                Math.Abs(roi.center.Y - Y_scaled) > roi.radius) displayPoint = false;
                        }
                    }

                    if (!displayPoint) continue;
                    */
                    if (ImageProjection_1d[i] < minZ) minZ = ImageProjection_1d[i];
                    if (ImageProjection_1d[i] > maxZ) maxZ = ImageProjection_1d[i];
                    //if (x < minX) minX = x; if (x > maxX) maxX = x;
                    //if (y < minY) minY = y; if (y > maxY) maxY = y;

                    minX_chosen = minX; maxX_chosen = maxX;
                    minY_chosen = minY; maxY_chosen = maxY;
                    minZ_chosen = minZ; maxZ_chosen = maxZ;
                }
            }

            for (int i = 0; i < ImageProjection_1d.Length; i++)
            {
                int x = i / maxSize;
                int y = i % maxSize;

                float X_scaled = (x - minX_chosen) / (maxX_chosen - minX_chosen) - 0.5f;
                float Y_scaled = (y - minY_chosen) / (maxY_chosen - minY_chosen) - 0.5f;
                float Z_scaled = (float)((ImageProjection_1d[y * maxSize + x] - minZ) / (maxZ - minZ) - 0.5);

                bool displayPoint = true;
                if (rois != null)
                {
                    foreach (SphereROI roi in rois)
                    {

                        if (Math.Abs(roi.center.X - X_scaled) > roi.radius ||
                            Math.Abs(roi.center.Y - Y_scaled) > roi.radius) displayPoint = false;
                    }
                }

                if (!displayPoint) continue;

                data3d_original.Add(new Point3d(x, y, ImageProjection_1d[y * maxSize + x]));
                data3d.Add(new Point3d(X_scaled, Y_scaled, Z_scaled));
            }


        }
        public void Rescale()
        {
            data3d.Clear();
            foreach (Point3d p in data3d_original)
            {
                data3d.Add(new Point3d(
                            (p.X - minX_chosen) / (maxX_chosen - minX_chosen) - 0.5,
                            (p.Y - minY_chosen) / (maxY_chosen - minY_chosen) - 0.5,
                            (p.Z - minZ_chosen) / (maxZ_chosen - minZ_chosen) - 0.5));

            }
        }

    }


}
