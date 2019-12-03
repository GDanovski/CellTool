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
using OpenTK.Graphics.OpenGL;
using OpenTK;
using System.Windows.Forms;

namespace Cell_Tool_3
{
    class CTChart
    {
        private ImageAnalyser IA;
        public CTChart_Properties Properties;
        public CTChart_Series Series;

        private List<double[]> data = new List<double[]>();
        private List<Color> SeriesColors = new List<Color>();
        private double[] XaxisData = null;
        public CTChart(ImageAnalyser IA)
        {
            this.IA = IA;
        }
        public void LoadFI(TifFileInfo fi)
        {
            if (fi.tpTaskbar.MethodsBtnList[2].ImageIndex != 0) return;
            if (fi.loaded == false) return;
            if (fi.roiList == null) return;

            #region Calculate Original Data Set

            int c, ind, row, fromT, toT, fromZ, toZ,
                t, z, position, stack, boolStart;
            double[] mainRoi;
            ROI roi;

            List<int> factorsT = new List<int>();
            List<int> factorsZ = new List<int>();

            if (fi.yAxisTB > 4)
                IA.chart.Properties.Ncalc.LoadFunction(fi);

            if (data == null) data = new List<double[]>();

            for (c = 0; c < fi.sizeC; c++)
                if (fi.tpTaskbar.ColorBtnList[c].ImageIndex == 0 && fi.roiList[c] != null)
                {
                    data.Clear();
                    SeriesColors.Clear();

                    for (ind = 0; ind < fi.roiList[c].Count; ind++)
                    {
                        roi = fi.roiList[c][ind];
                        if (roi.Results == null || roi.Checked == false) continue;

                        fromT = roi.FromT;
                        toT = roi.ToT;
                        fromZ = roi.FromZ;
                        toZ = roi.ToZ;
                        //main roi part

                        t = 1;
                        z = 1;


                        if (roi.ChartUseIndex[0] == true)
                        {
                            /*
                            if (fi.yAxisTB > 4 &&
                                IA.chart.Properties.Ncalc.ErrorCheck(c, roi, fi.roiList[c], fi))
                                continue;
                            */
                            mainRoi = new double[roi.Results.Length];

                            for (row = c; row < roi.Results.Length; row += fi.sizeC)
                            {
                                if (roi.Results[row] != null &&
                                    t >= fromT && t <= toT && z >= fromZ && z <= toZ)
                                    if (fi.yAxisTB == 4)
                                        mainRoi[row] = roi.Results[row][0] * roi.Results[row][1];
                                    else if (fi.yAxisTB > 4)
                                        mainRoi[row] = IA.chart.Properties.Ncalc.Calculate(c, row, roi, fi.roiList[c], fi);
                                    else
                                        mainRoi[row] = roi.Results[row][fi.yAxisTB];
                                //apply change t and z

                                z++;
                                if (z > fi.sizeZ)
                                {
                                    z = 1;
                                    t++;
                                    if (t > fi.sizeT) t = 1;
                                }
                            }
                            factorsT.Add(toT - fromT + 1);
                            factorsZ.Add(toZ - fromZ + 1);
                            SeriesColors.Add(roi.colors[0]);
                            data.Add(mainRoi);
                        }

                        //layers
                        if (roi.Stack == 0) continue;
                        if (fi.yAxisTB > 4) continue;
                        position = 4;

                        for (stack = 0; stack < roi.Stack; stack++)
                        {
                            t = 1;
                            z = 1;

                            mainRoi = new double[roi.Results.Length];

                            int factor = 0;
                            if (roi.Shape == 0 || roi.Shape == 1)
                            {
                                for (boolStart = 1 + stack * 4; boolStart < 5 + stack * 4; boolStart++, position += 4)
                                    if (roi.ChartUseIndex[boolStart] == true)
                                    {
                                        for (row = c; row < roi.Results.Length; row += fi.sizeC)
                                        {
                                            if (roi.Results[row] != null &&
                                                t >= fromT && t <= toT && z >= fromZ && z <= toZ)
                                                if (fi.yAxisTB == 4)
                                                    mainRoi[row] += roi.Results[row][position] * roi.Results[row][position + 1];
                                                else if (fi.yAxisTB < 2)
                                                    mainRoi[row] += roi.Results[row][position + fi.yAxisTB];
                                                else if (fi.yAxisTB == 2 && (mainRoi[row] == 0 || mainRoi[row] > roi.Results[row][position + fi.yAxisTB]))
                                                    mainRoi[row] = roi.Results[row][position + fi.yAxisTB];
                                                else if (fi.yAxisTB == 3 && mainRoi[row] < roi.Results[row][position + fi.yAxisTB])
                                                    mainRoi[row] = roi.Results[row][position + fi.yAxisTB];
                                            //apply change t and z

                                            z++;
                                            if (z > fi.sizeZ)
                                            {
                                                z = 1;
                                                t++;
                                                if (t > fi.sizeT) t = 1;
                                            }
                                        }

                                        factor++;
                                    }
                            }
                            else
                            {
                                boolStart = 1 + stack * 4;

                                if (roi.ChartUseIndex[boolStart] == true)
                                {
                                    for (row = c; row < roi.Results.Length; row += fi.sizeC)
                                    {
                                        if (roi.Results[row] != null &&
                                            t >= fromT && t <= toT && z >= fromZ && z <= toZ)
                                            if (fi.yAxisTB == 4)
                                                mainRoi[row] = roi.Results[row][position] * roi.Results[row][position + 1];
                                            else if (fi.yAxisTB < 2)
                                                mainRoi[row] = roi.Results[row][position + fi.yAxisTB];
                                            else if (fi.yAxisTB == 2)
                                                mainRoi[row] = roi.Results[row][position + fi.yAxisTB];
                                            else if (fi.yAxisTB == 3)
                                                mainRoi[row] = roi.Results[row][position + fi.yAxisTB];
                                        //apply change t and z

                                        z++;
                                        if (z > fi.sizeZ)
                                        {
                                            z = 1;
                                            t++;
                                            if (t > fi.sizeT) t = 1;
                                        }
                                    }

                                    factor++;
                                }

                                position += 16;
                            }

                            if (factor == 0) continue;

                            if (fi.yAxisTB == 1)
                                for (int i = 0; i < mainRoi.Length; i++)
                                    if (mainRoi[i] != 0) mainRoi[i] /= factor;

                            factorsT.Add(toT - fromT + 1);
                            factorsZ.Add(toZ - fromZ + 1);
                            SeriesColors.Add(roi.colors[1 + stack]);
                            data.Add(mainRoi);
                        }
                    }


                    XaxisData = new double[fi.imageCount];
                    t = 1;
                    z = 1;

                    double time = 0;
                    int timeIndex = 0;
                    double timeT = fi.TimeSteps[timeIndex];

                    for (row = c; row < fi.imageCount; row += fi.sizeC)
                    {
                        switch (fi.xAxisTB)
                        {
                            case 0:
                                //T slice
                                XaxisData[row] = t;
                                break;
                            case 1:
                                //T sec
                                XaxisData[row] = time;
                                break;
                            case 2:
                                //Z slice
                                XaxisData[row] = z;
                                break;
                            case 3:
                                //T sec
                                XaxisData[row] = time/60;
                                break;
                            case 4:
                                //T sec
                                XaxisData[row] = time / 3600;
                                break;
                        }
                        //apply change t and z

                        z++;
                        if (z > fi.sizeZ)
                        {
                            z = 1;
                            t++;
                            if (t > fi.sizeT) t = 1;

                            if (t <= timeT)
                            {
                                time += fi.TimeSteps[timeIndex + 1];
                            }
                            else
                            {
                                timeIndex += 2;

                                if (timeIndex < fi.TimeSteps.Count)
                                    timeT += fi.TimeSteps[timeIndex];
                                else
                                {
                                    timeIndex -= 2;
                                    timeT += fi.imageCount;
                                }

                                time += fi.TimeSteps[timeIndex + 1];
                            }
                        }
                    }
                    #endregion Calculate Original Data Set

                    #region recalculate original data set
                    if (fi.xAxisTB < 2 || fi.xAxisTB == 3 || fi.xAxisTB == 4)
                    {
                        double[] res;
                        int counter;

                        for (ind = 0; ind < data.Count; ind++)
                        {
                            res = new double[fi.sizeT];
                            counter = 0;

                            t = 1;
                            z = 1;

                            for (row = c; row < fi.imageCount; row += fi.sizeC)
                            {
                                z++;
                                if (fi.yAxisTB < 2)
                                    res[counter] += data[ind][row];
                                else if (fi.yAxisTB == 2 && (res[counter] > data[ind][row] || res[counter] == 0))
                                    res[counter] = data[ind][row];
                                else if (fi.yAxisTB == 3 && res[counter] < data[ind][row])
                                    res[counter] = data[ind][row];
                                else
                                    res[counter] += data[ind][row];

                                if (z > fi.sizeZ)
                                {
                                    z = 1;
                                    t++;
                                    if (fi.yAxisTB == 1)
                                        res[counter] /= factorsZ[ind];
                                    counter++;
                                }
                            }

                            data[ind] = res;
                        }
                        //x axis
                        res = new double[fi.sizeT];
                        counter = 0;
                        t = 1;
                        z = 1;

                        for (row = c; row < fi.imageCount; row += fi.sizeC)
                        {
                            z++;
                            if (z > fi.sizeZ)
                            {
                                z = 1;
                                t++;
                                res[counter] = XaxisData[row];
                                counter++;
                            }
                        }

                        XaxisData = res;
                    }
                    else if (fi.xAxisTB == 2)
                    {
                        double[] res;
                        int counter;

                        for (ind = 0; ind < data.Count; ind++)
                        {
                            res = new double[fi.sizeZ];
                            counter = 0;

                            t = 1;
                            z = 1;

                            for (row = c; row < fi.imageCount; row += fi.sizeC)
                            {
                                z++;

                                if (fi.yAxisTB < 2)
                                    res[counter] += data[ind][row];
                                else if (fi.yAxisTB == 2 && (res[counter] > data[ind][row] || res[counter] == 0))
                                    res[counter] = data[ind][row];
                                else if (fi.yAxisTB == 3 && res[counter] < data[ind][row])
                                    res[counter] = data[ind][row];
                                else
                                    res[counter] += data[ind][row];

                                counter++;

                                if (z > fi.sizeZ)
                                {
                                    z = 1;
                                    counter = 0;
                                    t++;
                                }
                            }
                            //popravi tuk s faktor list w gornata chast
                            if (fi.yAxisTB == 1)
                                for (int i = 0; i < res.Length; i++)
                                    res[i] /= factorsT[ind];

                            data[ind] = res;
                        }

                        //x axis
                        res = new double[fi.sizeZ];
                        counter = 0;
                        t = 1;
                        z = 1;

                        for (row = c; row < fi.imageCount; row += fi.sizeC)
                        {
                            res[counter] = XaxisData[row];
                            counter++;

                            z++;
                            if (z > fi.sizeZ) break;
                        }

                        XaxisData = res;
                    }
                    #endregion recalculate original data set
                    //System.IO.File.WriteAllText(fi.Dir.Replace(".tif", "_c" + c + ".text"),MeargeResult());
                    #region render Chart

                    Rectangle rect = IA.IDrawer.coRect[2][c];
                    Render(rect, fi);
                    #endregion region render chart
                }
        }

        private void Render(Rectangle OldRect, TifFileInfo fi)
        {

            double MaxY = 0;
            double MinY = 0;
            foreach (double[] dList in data)
                foreach (double val in dList)
                    if (val > MaxY) MaxY = val;
                    else if (val < MinY) MinY = val;

            double MaxX = 0;
            foreach (double val in XaxisData)
                if (val > MaxX) MaxX = val;


            RectangleF rect = renderChartArea(OldRect, fi, MinY, MaxY, MaxX);

            //Load tracer
            float curX =
                (float)((rect.Width / XaxisData[XaxisData.Length - 1]) *
                (XaxisData[fi.frame]));
            /*
             
            if (fi.xAxisTB == 0 || fi.xAxisTB == 1)
                curX = (float)(rect.Width / fi.sizeT) * (fi.frame + 1);
            else if (fi.xAxisTB == 2)
                curX = (float)(rect.Width / fi.sizeZ) * (fi.zValue + 1);
            */

            GL.Begin(PrimitiveType.Lines);
            GL.Color3(Color.Gray);

            GL.Vertex2(rect.X + curX, rect.Y);
            GL.Vertex2(rect.X + curX, rect.Y + rect.Height);

            GL.End();

            //load series
            double stepX = rect.Width / MaxX;
            double stepY = rect.Height / (MaxY - MinY);
            GL.Enable(EnableCap.LineSmooth);
            for (int i = 0; i < data.Count; i++)
            {
                GL.Begin(PrimitiveType.LineStrip);
                GL.Color3(SeriesColors[i]);

                for (int ind = 0; ind < XaxisData.Length; ind++)
                    GL.Vertex2(rect.X + XaxisData[ind] * stepX, rect.Y + rect.Height - (data[i][ind] - MinY) * stepY);

                if (XaxisData.Length == 1)
                    GL.Vertex2(rect.X + XaxisData[0] * stepX, rect.Y + rect.Height);

                GL.End();
            }
            GL.Disable(EnableCap.LineSmooth);
        }
        public RectangleF renderChartArea(Rectangle rect, TifFileInfo fi, double minY, double maxY, double maxX)
        {
            float W = 13f / (float)fi.zoom;
            float H = 15f / (float)fi.zoom;

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

            double stepX = microRect.Width / (30 / (float)fi.zoom);
            double stepY = microRect.Height / (20 / (float)fi.zoom);

            double valX = maxX / stepX;
            double valY = (maxY - minY) / stepY;

            for (double x = microRect.X, i = 0; x <= microRect.X + microRect.Width; x += microRect.Width / stepX, i += valX)
            {
                GL.Begin(PrimitiveType.Lines);
                GL.Color3(Color.Black);

                GL.Vertex2(x, rect.Y + rect.Height - H - 0.5f * H);
                GL.Vertex2(x, rect.Y + rect.Height - 0.5f * H - H * 0.8);

                GL.End();

                if (((int)i).ToString().Length > 5)
                    BitmapFromString(fi, ((int)i).ToString("0.0E0"), new PointF((float)x, (rect.Y + rect.Height - 0.5f * H - H / 2)));
                else
                    BitmapFromString(fi, ((int)i).ToString(), new PointF((float)x, (rect.Y + rect.Height - 0.5f * H - H / 2)));
            }

            for (double y = microRect.Y + microRect.Height, i = minY; y >= microRect.Y; y -= microRect.Height / stepY, i += valY)
            {
                GL.Begin(PrimitiveType.Lines);
                GL.Color3(Color.Black);

                GL.Vertex2(rect.X + W, y);
                GL.Vertex2(rect.X + W - H * 0.2, y);

                GL.End();

                if (((int)i).ToString().Length > 5)
                    BitmapFromString(fi, ((int)i).ToString("0.0E0"), new PointF(rect.X + W / 2, (float)y));
                else
                    BitmapFromString(fi, ((int)i).ToString(), new PointF(rect.X + W / 2, (float)y));
            }

            if (Properties.yAxisTB.Text.Length < 6)
                BitmapFromString(fi, Properties.yAxisTB.Text, new PointF(rect.X + W / 2 + 1, rect.Y + H / 2), true);
            else
                BitmapFromString(fi, Properties.yAxisTB.Text.Substring(0, 6), new PointF(rect.X + W / 2 + 1, rect.Y + H / 2), true);

            BitmapFromString(fi, Properties.xAxisTB.Text, new PointF(rect.X + rect.Width / 2, (rect.Y + rect.Height - H / 2)), true);

            return microRect;

        }

        private void BitmapFromString(TifFileInfo fi, string str, PointF p, bool title = false)
        {
            Font font = new Font("Times New Roman", 9, FontStyle.Regular);
            if (title) font = new Font("Times New Roman", 9, FontStyle.Bold);

            Bitmap bmp = new Bitmap(TextRenderer.MeasureText(str, font).Width,
                TextRenderer.MeasureText(str, font).Height,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            RectangleF rect = new Rectangle(0, 0,
                TextRenderer.MeasureText(str, font).Width,
                TextRenderer.MeasureText(str, font).Height);

            //MessageBox.Show(rect.Width.ToString() + "\n" +                rect.Height.ToString());
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.FillRectangle(Brushes.White, rect);
                g.DrawString(str, font, Brushes.Black, rect);
                g.Flush();
            }

            int ID = IA.IDrawer.ImageTexture.LoadTexture(bmp);

            GLControl GLControl1 = IA.GLControl1;

            GL.Enable(EnableCap.Texture2D);

            GL.BindTexture(TextureTarget.Texture2D, ID);

            rect.Width /= (float)fi.zoom;
            rect.Height /= (float)fi.zoom;

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
        #region SvaeFile
        public string GetResults(TifFileInfo fi, int c)
        {
            string val = "";
            if (fi.tpTaskbar.MethodsBtnList[2].ImageIndex != 0) return val;
            if (fi.loaded == false) return val;
            if (fi.roiList == null) return val;

            #region Calculate Original Data Set

            int ind, row, fromT, toT, fromZ, toZ,
                t, z, position, stack, boolStart;
            double[] mainRoi;
            List<string> RoiNames = new List<string>() { IA.chart.Properties.xAxisTB.Text };
            List<string> Comments = new List<string>() { "Comments" };

            ROI roi;

            List<int> factorsT = new List<int>();
            List<int> factorsZ = new List<int>();

            if (fi.yAxisTB > 4)
                IA.chart.Properties.Ncalc.LoadFunction(fi);

            if (data == null) data = new List<double[]>();

            if (fi.tpTaskbar.ColorBtnList[c].ImageIndex == 0 && fi.roiList[c] != null)
            {
                data.Clear();
                SeriesColors.Clear();

                for (ind = 0; ind < fi.roiList[c].Count; ind++)
                {
                    roi = fi.roiList[c][ind];
                    if (roi.Results == null || roi.Checked == false) continue;

                    fromT = roi.FromT;
                    toT = roi.ToT;
                    fromZ = roi.FromZ;
                    toZ = roi.ToZ;
                    //main roi part

                    t = 1;
                    z = 1;

                    if (roi.ChartUseIndex[0] == true)
                    {
                        mainRoi = new double[roi.Results.Length];

                        for (row = c; row < roi.Results.Length; row += fi.sizeC)
                        {
                            if (roi.Results[row] != null &&
                                t >= fromT && t <= toT && z >= fromZ && z <= toZ)
                                if (fi.yAxisTB == 4)
                                    mainRoi[row] = roi.Results[row][0] * roi.Results[row][1];
                                else if (fi.yAxisTB > 4)
                                    mainRoi[row] = IA.chart.Properties.Ncalc.Calculate(c, row, roi, fi.roiList[c], fi);
                                else
                                    mainRoi[row] = roi.Results[row][fi.yAxisTB];
                            //apply change t and z

                            z++;
                            if (z > fi.sizeZ)
                            {
                                z = 1;
                                t++;
                                if (t > fi.sizeT) t = 1;
                            }
                        }
                        factorsT.Add(toT - fromT + 1);
                        factorsZ.Add(toZ - fromZ + 1);
                        SeriesColors.Add(roi.colors[0]);
                        data.Add(mainRoi);
                        RoiNames.Add(IA.chart.Properties.yAxisTB.Text + "_ROI" + (ind + 1).ToString());
                        Comments.Add(roi.Comment);
                    }

                    //layers
                    if (roi.Stack == 0) continue;

                    if (fi.yAxisTB > 4) continue;
                    position = 4;

                    for (stack = 0; stack < roi.Stack; stack++)
                    {
                        t = 1;
                        z = 1;

                        mainRoi = new double[roi.Results.Length];

                        int factor = 0;
                        if (roi.Shape == 0 || roi.Shape == 1)
                        {
                            for (boolStart = 1 + stack * 4; boolStart < 5 + stack * 4; boolStart++, position += 4)
                                if (roi.ChartUseIndex[boolStart] == true)
                                {
                                    for (row = c; row < roi.Results.Length; row += fi.sizeC)
                                    {
                                        if (roi.Results[row] != null &&
                                            t >= fromT && t <= toT && z >= fromZ && z <= toZ)
                                            if (fi.yAxisTB == 4)
                                                mainRoi[row] += roi.Results[row][position] * roi.Results[row][position + 1];
                                            else if (fi.yAxisTB < 2)
                                                mainRoi[row] += roi.Results[row][position + fi.yAxisTB];
                                            else if (fi.yAxisTB == 2 && (mainRoi[row] == 0 || mainRoi[row] > roi.Results[row][position + fi.yAxisTB]))
                                                mainRoi[row] = roi.Results[row][position + fi.yAxisTB];
                                            else if (fi.yAxisTB == 3 && mainRoi[row] < roi.Results[row][position + fi.yAxisTB])
                                                mainRoi[row] = roi.Results[row][position + fi.yAxisTB];
                                        //apply change t and z

                                        z++;
                                        if (z > fi.sizeZ)
                                        {
                                            z = 1;
                                            t++;
                                            if (t > fi.sizeT) t = 1;
                                        }
                                    }

                                    factor++;
                                }
                        }
                        else
                        {
                            boolStart =1+ stack * 4;

                            if (roi.ChartUseIndex[boolStart] == true)
                            {
                                for (row = c; row < roi.Results.Length; row += fi.sizeC)
                                {
                                    if (roi.Results[row] != null &&
                                        t >= fromT && t <= toT && z >= fromZ && z <= toZ)
                                        if (fi.yAxisTB == 4)
                                            mainRoi[row] = roi.Results[row][position] * roi.Results[row][position + 1];
                                        else if (fi.yAxisTB < 2)
                                            mainRoi[row] = roi.Results[row][position + fi.yAxisTB];
                                        else if (fi.yAxisTB == 2)
                                            mainRoi[row] = roi.Results[row][position + fi.yAxisTB];
                                        else if (fi.yAxisTB == 3)
                                            mainRoi[row] = roi.Results[row][position + fi.yAxisTB];
                                    //apply change t and z

                                    z++;
                                    if (z > fi.sizeZ)
                                    {
                                        z = 1;
                                        t++;
                                        if (t > fi.sizeT) t = 1;
                                    }
                                }

                                factor++;
                            }

                            position += 16;
                        }
                        
                        if (fi.yAxisTB == 1)
                            for (int i = 0; i < mainRoi.Length; i++)
                                if (mainRoi[i] != 0) mainRoi[i] /= factor;

                        factorsT.Add(toT - fromT + 1);
                        factorsZ.Add(toZ - fromZ + 1);
                        SeriesColors.Add(roi.colors[1 + stack]);
                        data.Add(mainRoi);
                        RoiNames.Add(IA.chart.Properties.yAxisTB.Text + "_ROI" + (ind + 1).ToString() + ".Layer" + (stack + 1).ToString());
                        Comments.Add(roi.Comment);
                    }
                }


                XaxisData = new double[fi.imageCount];
                t = 1;
                z = 1;

                double time = 0;
                int timeIndex = 0;
                double timeT = fi.TimeSteps[timeIndex];

                for (row = c; row < fi.imageCount; row += fi.sizeC)
                {
                    switch (fi.xAxisTB)
                    {
                        case 0:
                            //T slice
                            XaxisData[row] = t;
                            break;
                        case 1:
                            //T sec
                            XaxisData[row] = time;
                            break;
                        case 2:
                            //Z slice
                            XaxisData[row] = z;
                            break;
                        case 3:
                            //T sec
                            XaxisData[row] = time / 60;
                            break;
                        case 4:
                            //T sec
                            XaxisData[row] = time / 3600;
                            break;
                    }
                    //apply change t and z

                    z++;
                    if (z > fi.sizeZ)
                    {
                        z = 1;
                        t++;
                        if (t > fi.sizeT) t = 1;

                        if (t <= timeT)
                        {
                            time += fi.TimeSteps[timeIndex + 1];
                        }
                        else
                        {
                            timeIndex += 2;

                            if (timeIndex < fi.TimeSteps.Count)
                                timeT += fi.TimeSteps[timeIndex];
                            else
                            {
                                timeIndex -= 2;
                                timeT += fi.imageCount;
                            }

                            time += fi.TimeSteps[timeIndex + 1];
                        }
                    }
                }
                #endregion Calculate Original Data Set

                #region recalculate original data set
                if (fi.xAxisTB < 2 || fi.xAxisTB == 3 || fi.xAxisTB == 4)
                {
                    double[] res;
                    int counter;

                    for (ind = 0; ind < data.Count; ind++)
                    {
                        res = new double[fi.sizeT];
                        counter = 0;

                        t = 1;
                        z = 1;

                        for (row = c; row < fi.imageCount; row += fi.sizeC)
                        {
                            z++;
                            if (fi.yAxisTB < 2)
                                res[counter] += data[ind][row];
                            else if (fi.yAxisTB == 2 && (res[counter] > data[ind][row] || res[counter] == 0))
                                res[counter] = data[ind][row];
                            else if (fi.yAxisTB == 3 && res[counter] < data[ind][row])
                                res[counter] = data[ind][row];
                            else
                                res[counter] += data[ind][row];

                            if (z > fi.sizeZ)
                            {
                                z = 1;
                                t++;
                                if (fi.yAxisTB == 1)
                                    res[counter] /= factorsZ[ind];
                                counter++;
                            }
                        }

                        data[ind] = res;
                    }
                    //x axis
                    res = new double[fi.sizeT];
                    counter = 0;
                    t = 1;
                    z = 1;

                    for (row = c; row < fi.imageCount; row += fi.sizeC)
                    {
                        z++;
                        if (z > fi.sizeZ)
                        {
                            z = 1;
                            t++;
                            res[counter] = XaxisData[row];
                            counter++;
                        }
                    }

                    XaxisData = res;
                }
                else if (fi.xAxisTB == 2)
                {
                    double[] res;
                    int counter;

                    for (ind = 0; ind < data.Count; ind++)
                    {
                        res = new double[fi.sizeZ];
                        counter = 0;

                        t = 1;
                        z = 1;

                        for (row = c; row < fi.imageCount; row += fi.sizeC)
                        {
                            z++;

                            if (fi.yAxisTB < 2)
                                res[counter] += data[ind][row];
                            else if (fi.yAxisTB == 2 && (res[counter] > data[ind][row] || res[counter] == 0))
                                res[counter] = data[ind][row];
                            else if (fi.yAxisTB == 3 && res[counter] < data[ind][row])
                                res[counter] = data[ind][row];
                            else
                                res[counter] += data[ind][row];

                            counter++;

                            if (z > fi.sizeZ)
                            {
                                z = 1;
                                counter = 0;
                                t++;
                            }
                        }
                        //popravi tuk s faktor list w gornata chast
                        if (fi.yAxisTB == 1)
                            for (int i = 0; i < res.Length; i++)
                                res[i] /= factorsT[ind];

                        data[ind] = res;
                    }

                    //x axis
                    res = new double[fi.sizeZ];
                    counter = 0;
                    t = 1;
                    z = 1;

                    for (row = c; row < fi.imageCount; row += fi.sizeC)
                    {
                        res[counter] = XaxisData[row];
                        counter++;

                        z++;
                        if (z > fi.sizeZ) break;
                    }

                    XaxisData = res;
                }
                #endregion recalculate original data set
                //System.IO.File.WriteAllText(fi.Dir.Replace(".tif", "_c" + c + ".text"),MeargeResult());
                #region Prepare string
                string[] resList = new string[XaxisData.Length + 3];
                //system description row
                string[] temp = new string[RoiNames.Count];
                temp[0] = "CTResults: " + IA.chart.Properties.yAxisTB.Text;
                temp[1] = fi.Dir;
                resList[0] = string.Join("\t", temp);
                temp = null;
                //comments
                resList[1] = string.Join("\t", Comments);
                Comments = null;
                //titles
                resList[2] = string.Join("\t", RoiNames);
                RoiNames = null;
                for (int i = 0; i < XaxisData.Length; i++)
                {
                    val = XaxisData[i].ToString();
                    foreach (double[] dList in data)
                    {
                        val += "\t" + dList[i];
                    }
                    resList[i + 3] = val;
                }
                XaxisData = null;
                data = null;

                val = string.Join("\n", resList);
                resList = null;
                #endregion Prepare string
            }

            return val;
        }
        #endregion SaveFile
    }
}
