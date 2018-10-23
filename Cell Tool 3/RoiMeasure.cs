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


namespace Cell_Tool_3
{
    class RoiMeasure
    {
        
        public static void Measure(ROI roi, TifFileInfo fi, int cVal, ImageAnalyser IA)
        {
            try {
                //return;
                if (fi.loaded == false || fi.roiList == null || fi.roiList[cVal] == null ||
                    (!(fi.roiList[cVal].IndexOf(roi) > -1))) return;
                IA.FileBrowser.StatusLabel.Text = "Measuring ROI...";
                if (roi.Type == 0)
                    switch (roi.Shape)
                    {
                        case 0:
                            GetPointsInRectangleStatic(roi, fi, cVal);
                            break;
                        case 1:
                            GetPointsInOvalStatic(roi, fi, cVal);
                            break;
                        case 2:
                            GetPointsInPolygonStatic(roi, fi, cVal);
                            break;
                        case 3:
                            GetPointsInPolygonStatic(roi, fi, cVal);
                            break;
                        case 4:
                            GetPointsInPolygonStatic(roi, fi, cVal);
                            break;
                        case 5:
                            GetPointsInPolygonStatic(roi, fi, cVal);
                            break;
                    }
                else if (roi.Type == 1)
                    switch (roi.Shape)
                    {
                        case 0:
                            GetPointsInRectangleTracking(roi, fi, cVal);
                            break;
                        case 1:
                            GetPointsInOvalTracking(roi, fi, cVal);
                            break;
                        case 2:
                            GetPointsInPolygonTracking(roi, fi, cVal);
                            break;
                        case 3:
                            GetPointsInPolygonTracking(roi, fi, cVal);
                            break;
                        case 4:
                            GetPointsInPolygonTracking(roi, fi, cVal);
                            break;
                        case 5:
                            GetPointsInPolygonTracking(roi, fi, cVal);
                            break;
                    }
            }
           catch { }

            IA.FileBrowser.StatusLabel.Text = "Ready";
        }
        #region rectangles
        private static void GetPointsInRectangleStatic(ROI roi, TifFileInfo fi, int cVal)
        {
            //get the location of the first value
            Point p = roi.GetLocation(cVal)[0];
            int X = p.X;
            int Y = p.Y;
            int W = roi.Width;
            int H = roi.Height;
            //create shablon for preventing retacking the same value
            bool[,] shablon = new bool[fi.sizeY, fi.sizeX];
            //take the points for the main roi
            List<Point> mainRoi;
            //calculates the stack
            List<Point>[] stackRoi = new List<Point>[roi.Stack];
            int rowSize = 0;

            if (roi.D >= 0)
            {
                mainRoi = CalculateRectangle(shablon, X, Y, W, H);
                rowSize = 4;

                for (int i = 0; i < roi.Stack; i++)
                {
                    X -= roi.D;
                    Y -= roi.D;
                    W += roi.D + roi.D;
                    H += roi.D + roi.D;

                    stackRoi[i] = CalculateRectangle(shablon, X, Y, W, H);
                    rowSize += 16;
                }
            }
            else
            {
                X -= roi.D * roi.Stack;
                Y -= roi.D * roi.Stack;
                W += (roi.D + roi.D) * roi.Stack;
                H += (roi.D + roi.D) * roi.Stack;

                mainRoi = CalculateRectangle(shablon, X, Y, W, H);
                rowSize = 4;

                for (int i = 0; i < roi.Stack; i++)
                {
                    X += roi.D;
                    Y += roi.D;
                    W -= roi.D + roi.D;
                    H -= roi.D + roi.D;

                    stackRoi[i] = CalculateRectangle(shablon, X, Y, W, H);
                    rowSize += 16;
                }

            }

            shablon = null;
            //measure frames
            roi.Results = new double[fi.imageCount][];
            
            Parallel.ForEach(SteppedRange(cVal, fi.imageCount, fi.sizeC), imageN => 
            {
                try {
                    double[] res = new double[rowSize];
                    int position = 0;

                    double[] tempRes = CalculateMainResults(mainRoi, fi, imageN);

                    Array.Copy(tempRes, 0, res, position, tempRes.Length);
                    position += tempRes.Length;

                    Point midP = new Point(p.X + roi.Width / 2, p.Y + roi.Height / 2);

                    for (int i = 0; i < roi.Stack; i++)
                    {
                        tempRes = CalculateStackResults(stackRoi[i], midP, fi, imageN);
                        Array.Copy(tempRes, 0, res, position, tempRes.Length);
                        position += tempRes.Length;
                    }

                    roi.Results[imageN] = res;
                } catch { return; }
            });
        }
        private static void GetPointsInRectangleTracking(ROI roi, TifFileInfo fi, int cVal)
        {
            //get the location of the first value
            Point p = roi.GetLocation(cVal)[0];
            int X = p.X;
            int Y = p.Y;
            int W = roi.Width;
            int H = roi.Height;
            //create shablon for preventing retacking the same value
            bool[,] shablon = new bool[fi.sizeY, fi.sizeX];
            //take the points for the main roi
            List<Point> mainRoi;
            //calculates the stack
            List<Point>[] stackRoi = new List<Point>[roi.Stack];
            int rowSize = 0;

            if (roi.D >= 0)
            {
                mainRoi = CalculateRectangle(shablon, X, Y, W, H);
                rowSize = 4;

                for (int i = 0; i < roi.Stack; i++)
                {
                    X -= roi.D;
                    Y -= roi.D;
                    W += roi.D + roi.D;
                    H += roi.D + roi.D;

                    stackRoi[i] = CalculateRectangle(shablon, X, Y, W, H);
                    rowSize += 16;
                }
            }
            else
            {
                X -= roi.D * roi.Stack;
                Y -= roi.D * roi.Stack;
                W += (roi.D + roi.D) * roi.Stack;
                H += (roi.D + roi.D) * roi.Stack;

                mainRoi = CalculateRectangle(shablon, X, Y, W, H);
                rowSize = 4;

                for (int i = 0; i < roi.Stack; i++)
                {
                    X += roi.D;
                    Y += roi.D;
                    W -= roi.D + roi.D;
                    H -= roi.D + roi.D;

                    stackRoi[i] = CalculateRectangle(shablon, X, Y, W, H);
                    rowSize += 16;
                }

            }

            shablon = null;
            //measure frames
            roi.Results = new double[fi.imageCount][];

            Parallel.ForEach(SteppedRange(cVal, fi.imageCount, fi.sizeC), imageN =>
            {
                try {
                    Point p1 = roi.GetLocation(imageN)[0];
                    double[] res = new double[rowSize];
                    int dX = p.X - p1.X;
                    int dY = p.Y - p1.Y;
                    int position = 0;

                    double[] tempRes = CalculateMainResults(mainRoi, fi, imageN, dX, dY);

                    Array.Copy(tempRes, 0, res, position, tempRes.Length);
                    position += tempRes.Length;

                    Point midP = new Point(p1.X + roi.Width / 2, p1.Y + roi.Height / 2);
                    
                    for (int i = 0; i < roi.Stack; i++)
                    {
                        tempRes = CalculateStackResults(stackRoi[i], midP, fi, imageN, dX, dY);
                        Array.Copy(tempRes, 0, res, position, tempRes.Length);
                        position += tempRes.Length;
                    }

                    roi.Results[imageN] = res;
                }
                catch { return; }
            });
        }
        private static double[] CalculateStackResults(List<Point> pList, Point midP, TifFileInfo fi, int imageN)
        {
            //left - 0
            //right - 1
            // down left = 2
            //down right = 3

            double area0 = 0;
            double mean0 = 0;
            double max0 = 0;
            double min0 = 0;

            double area1 = 0;
            double mean1 = 0;
            double max1 = 0;
            double min1 = 0;

            double area2 = 0;
            double mean2 = 0;
            double max2 = 0;
            double min2 = 0;

            double area3 = 0;
            double mean3 = 0;
            double max3 = 0;
            double min3 = 0;

            switch (fi.bitsPerPixel)
            {
                case 8:
                    byte[][] image = fi.image8bit[imageN];
                    min0 = byte.MaxValue;
                    min1 = byte.MaxValue;
                    min2 = byte.MaxValue;
                    min3 = byte.MaxValue;

                    foreach (Point p in pList)
                    {
                        
                        byte val = image[p.Y][p.X];

                        if (p.Y < midP.Y && p.X <= midP.X)
                        {
                            //image[p.Y][p.X] = 50;
                            area0++;
                            mean0 += val;
                            if (max0 < val) max0 = val;
                            if (min0 > val) min0 = val;
                        }
                        else if (p.Y <= midP.Y && p.X > midP.X)
                        {
                            //image[p.Y][p.X] = 100;
                            area1++;
                            mean1 += val;
                            if (max1 < val) max1 = val;
                            if (min1 > val) min1 = val;
                        }
                        else if (p.Y >= midP.Y && p.X < midP.X)
                        {
                            //image[p.Y][p.X] = 150;
                            area2++;
                            mean2 += val;
                            if (max2 < val) max2 = val;
                            if (min2 > val) min2 = val;
                        }
                        else if (p.Y > midP.Y && p.X >= midP.X)
                        {
                            //image[p.Y][p.X] = 200;
                            area3++;
                            mean3 += val;
                            if (max3 < val) max3 = val;
                            if (min3 > val) min3 = val;
                        }
                    }
                    break;
                case 16:
                    ushort[][] image16 = fi.image16bit[imageN];
                    min0 = ushort.MaxValue;
                    min1 = ushort.MaxValue;
                    min2 = ushort.MaxValue;
                    min3 = ushort.MaxValue;

                    foreach (Point p in pList)
                    {
                        ushort val = image16[p.Y][p.X];

                        if (p.Y < midP.Y && p.X <= midP.X)
                        {
                            //image16[p.Y][p.X] = 50;
                            area0++;
                            mean0 += val;
                            if (max0 < val) max0 = val;
                            if (min0 > val) min0 = val;
                        }
                        else if (p.Y <= midP.Y && p.X > midP.X)
                        {
                            //image16[p.Y][p.X] = 100;
                            area1++;
                            mean1 += val;
                            if (max1 < val) max1 = val;
                            if (min1 > val) min1 = val;
                        }
                        else if (p.Y >= midP.Y && p.X < midP.X)
                        {
                            //image16[p.Y][p.X] = 150;
                            area2++;
                            mean2 += val;
                            if (max2 < val) max2 = val;
                            if (min2 > val) min2 = val;
                        }
                        else if (p.Y > midP.Y && p.X >= midP.X)
                        {
                            //image16[p.Y][p.X] = 200;
                            area3++;
                            mean3 += val;
                            if (max3 < val) max3 = val;
                            if (min3 > val) min3 = val;
                        }
                    }
                    break;
            }

            if (area0 > 0) mean0 /= area0;
            if (area1 > 0) mean1 /= area1;
            if (area2 > 0) mean2 /= area2;
            if (area3 > 0) mean3 /= area3;

            double[] res = new double[] {
            area0, mean0, min0, max0 ,
            area1, mean1, min1, max1 ,
            area2, mean2, min2, max2,
            area3, mean3, min3, max3 };

            return res;

        }
        private static double[] CalculateStackResults(List<Point> pList, Point midP, TifFileInfo fi, int imageN, int dX, int dY)
        {
            //left - 0
            //right - 1
            // down left = 2
            //down right = 3

            double area0 = 0;
            double mean0 = 0;
            double max0 = 0;
            double min0 = 0;

            double area1 = 0;
            double mean1 = 0;
            double max1 = 0;
            double min1 = 0;

            double area2 = 0;
            double mean2 = 0;
            double max2 = 0;
            double min2 = 0;

            double area3 = 0;
            double mean3 = 0;
            double max3 = 0;
            double min3 = 0;

            switch (fi.bitsPerPixel)
            {
                case 8:
                    byte[][] image = fi.image8bit[imageN];
                    min0 = byte.MaxValue;
                    min1 = byte.MaxValue;
                    min2 = byte.MaxValue;
                    min3 = byte.MaxValue;

                    foreach (Point p in pList)
                    {
                        Point p1 = new Point(p.X - dX, p.Y - dY);
                        if (p1.Y >= 0 && p1.Y < fi.sizeY && p1.X >= 0 && p1.X < fi.sizeX)
                        {
                            byte val = image[p1.Y][p1.X];

                            if (p1.Y < midP.Y && p1.X <= midP.X)
                            {
                                //image[p1.Y][p1.X] = 50;
                                area0++;
                                mean0 += val;
                                if (max0 < val) max0 = val;
                                if (min0 > val) min0 = val;
                            }
                            else if (p1.Y <= midP.Y && p1.X > midP.X)
                            {
                                //image[p1.Y][p1.X] = 100;
                                area1++;
                                mean1 += val;
                                if (max1 < val) max1 = val;
                                if (min1 > val) min1 = val;
                            }
                            else if (p1.Y >= midP.Y && p1.X < midP.X)
                            {
                                //image[p1.Y][p1.X] = 150;
                                area2++;
                                mean2 += val;
                                if (max2 < val) max2 = val;
                                if (min2 > val) min2 = val;
                            }
                            else if (p1.Y > midP.Y && p1.X >= midP.X)
                            {
                                //image[p1.Y][p1.X] = 200;
                                area3++;
                                mean3 += val;
                                if (max3 < val) max3 = val;
                                if (min3 > val) min3 = val;
                            }
                        }
                    }
                    break;
                case 16:
                    ushort[][] image16 = fi.image16bit[imageN];
                    min0 = ushort.MaxValue;
                    min1 = ushort.MaxValue;
                    min2 = ushort.MaxValue;
                    min3 = ushort.MaxValue;

                    foreach (Point p in pList)
                    {
                        Point p1 = new Point(p.X - dX, p.Y - dY);

                        if (p1.Y >= 0 && p1.Y < fi.sizeY && p1.X >= 0 && p1.X < fi.sizeX)
                        {
                            ushort val = image16[p1.Y][p1.X];

                            if (p1.Y < midP.Y && p1.X <= midP.X)
                            {
                                //image16[p1.Y][p1.X] = 50;
                                area0++;
                                mean0 += val;
                                if (max0 < val) max0 = val;
                                if (min0 > val) min0 = val;
                            }
                            else if (p1.Y <= midP.Y && p1.X > midP.X)
                            {
                                //image16[p1.Y][p1.X] = 100;
                                area1++;
                                mean1 += val;
                                if (max1 < val) max1 = val;
                                if (min1 > val) min1 = val;
                            }
                            else if (p1.Y >= midP.Y && p1.X < midP.X)
                            {
                                //image16[p1.Y][p1.X] = 150;
                                area2++;
                                mean2 += val;
                                if (max2 < val) max2 = val;
                                if (min2 > val) min2 = val;
                            }
                            else if (p1.Y > midP.Y && p1.X >= midP.X)
                            {
                                //image16[p1.Y][p1.X] = 200;
                                area3++;
                                mean3 += val;
                                if (max3 < val) max3 = val;
                                if (min3 > val) min3 = val;
                            }
                        }
                    }
                    break;
            }

            if (area0 > 0) mean0 /= area0;
            if (area1 > 0) mean1 /= area1;
            if (area2 > 0) mean2 /= area2;
            if (area3 > 0) mean3 /= area3;

            double[] res = new double[] {
            area0, mean0, min0, max0 ,
            area1, mean1, min1, max1 ,
            area2, mean2, min2, max2,
            area3, mean3, min3, max3 };

            return res;

        }
        private static double[] CalculateMainResults(List<Point> pList, TifFileInfo fi, int imageN)
        {
            double area = 0;
            double mean = 0;
            double max = 0;
            double min = 0;

            switch (fi.bitsPerPixel)
            {
                case 8:
                    byte[][] image = fi.image8bit[imageN];
                    min = byte.MaxValue;

                    foreach (Point p in pList)
                    {
                        //image[p.Y][p.X] = 0;
                        byte val = image[p.Y][p.X];

                        area++;
                        mean += val;
                        if (max < val) max = val;
                        if (min > val) min = val;
                    }
                    break;
                case 16:
                    ushort[][] image16 = fi.image16bit[imageN];
                    min = ushort.MaxValue;

                    foreach (Point p in pList)
                    {
                        //image16[p.Y ][p.X ] = 0;
                        ushort val = image16[p.Y][p.X];

                        area++;
                        mean += val;
                        if (max < val) max = val;
                        if (min > val) min = val;
                    }
                    break;
            }

            if (area > 0) mean /= area;
            double[] res = new double[] { area, mean, min, max };

            return res;
        }
        private static double[] CalculateMainResults(List<Point> pList, TifFileInfo fi, int imageN, int dX, int dY)
        {
            //try {
                double area = 0;
                double mean = 0;
                double max = 0;
                double min = 0;

                switch (fi.bitsPerPixel)
                {
                    case 8:
                        byte[][] image = fi.image8bit[imageN];
                        min = byte.MaxValue;

                        foreach (Point p in pList)
                            if (p.Y - dY >= 0 && p.Y - dY < fi.sizeY && p.X - dX >= 0 && p.X - dX < fi.sizeX)
                            {
                                //image[p.Y - dY][p.X - dX] = 0;
                                byte val = image[p.Y - dY][p.X - dX];

                                area++;
                                mean += val;
                                if (max < val) max = val;
                                if (min > val) min = val;
                            }
                        break;
                    case 16:
                        ushort[][] image16 = fi.image16bit[imageN];

                        min = ushort.MaxValue;

                        foreach (Point p in pList)
                            if (p.Y - dY >= 0 && p.Y - dY < fi.sizeY && p.X - dX >= 0 && p.X - dX < fi.sizeX)
                            {
                                //image16[p.Y - dY][p.X - dX] = 0;
                                ushort val = image16[p.Y - dY][p.X - dX];

                                area++;
                                mean += val;
                                if (max < val) max = val;
                                if (min > val) min = val;
                            }
                        break;
                }

                if (area > 0) mean /= area;
                double[] res = new double[] { area, mean, min, max };

                return res;
            //}
            //catch
            {
                //return null;
            }
        }
        public static IEnumerable<int> SteppedRange(int fromInclusive, int toExclusive, int step)
        {
            for (var i = fromInclusive; i < toExclusive; i += step)
            {
                yield return i;
            }
        }
        private static List<Point> CalculateRectangle(bool[,] shablon, int X, int Y, int W, int H)
        {
            List<Point> pList = new List<Point>();

            int X1 = X + W;
            int Y1 = Y + H;

            if (X < 0) X = 0;
            if (Y < 0) Y = 0;
            if (X1 >= shablon.GetLength(1)) X1 = shablon.GetLength(1) - 1;
            if (Y1 >= shablon.GetLength(0)) Y1 = shablon.GetLength(0) - 1;

            for (int curY = Y; curY <= Y1; curY++)
                for (int curX = X; curX <= X1; curX++)
                    if (shablon[curY, curX] == false)
                    {
                        shablon[curY, curX] = true;
                        pList.Add(new Point(curX, curY));
                    }

           return pList;
        }

        #endregion rectangles

        #region oval
        private static void GetPointsInOvalStatic(ROI roi, TifFileInfo fi, int cVal)
        {
            //FillEllipse(roi, fi, 0);
            //get the location of the first value
            Point p = roi.GetLocation(cVal)[0];
            int X = p.X;
            int Y = p.Y;
            int W = roi.Width;
            int H = roi.Height;
            //create shablon for preventing retacking the same value
            bool[,] shablon = new bool[fi.sizeY, fi.sizeX];
            //take the points for the main roi
            List<Point> mainRoi;
            //calculates the stack
            List<Point>[] stackRoi = new List<Point>[roi.Stack];
            int rowSize = 0;

            if (roi.D >= 0)
            {
                mainRoi = CalculateEllipse(shablon, X, Y, W, H);
                rowSize = 4;

                for (int i = 0; i < roi.Stack; i++)
                {
                    X -= roi.D;
                    Y -= roi.D;
                    W += roi.D + roi.D;
                    H += roi.D + roi.D;

                    stackRoi[i] = CalculateEllipse(shablon, X, Y, W, H);
                    rowSize += 16;
                }
            }
            else
            {
                X -= roi.D * roi.Stack;
                Y -= roi.D * roi.Stack;
                W += (roi.D + roi.D) * roi.Stack;
                H += (roi.D + roi.D) * roi.Stack;

                mainRoi = CalculateEllipse(shablon, X, Y, W, H);
                rowSize = 4;

                for (int i = 0; i < roi.Stack; i++)
                {
                    X += roi.D;
                    Y += roi.D;
                    W -= roi.D + roi.D;
                    H -= roi.D + roi.D;

                    stackRoi[i] = CalculateEllipse(shablon, X, Y, W, H);
                    rowSize += 16;
                }

            }

            shablon = null;
            //measure frames
            roi.Results = new double[fi.imageCount][];

            Parallel.ForEach(SteppedRange(cVal, fi.imageCount, fi.sizeC), imageN =>
            {
                try {
                        double[] res = new double[rowSize];
                        int position = 0;

                        double[] tempRes = CalculateMainResults(mainRoi, fi, imageN);

                        Array.Copy(tempRes, 0, res, position, tempRes.Length);
                        position += tempRes.Length;

                        Point midP = new Point(p.X + roi.Width / 2, p.Y + roi.Height / 2);

                        for (int i = 0; i < roi.Stack; i++)
                        {
                            tempRes = CalculateStackResults(stackRoi[i], midP, fi, imageN);
                            Array.Copy(tempRes, 0, res, position, tempRes.Length);
                            position += tempRes.Length;
                        }

                        roi.Results[imageN] = res;
                    
                }
                catch { return; }
            });
        }

        private static void GetPointsInOvalTracking(ROI roi, TifFileInfo fi, int cVal)
        {
            
            //FillEllipse(roi, fi, 0);
            //get the location of the first value
            Point p = roi.GetLocation(cVal)[0];
            int X = p.X;
            int Y = p.Y;
            int W = roi.Width;
            int H = roi.Height;
            //create shablon for preventing retacking the same value
            bool[,] shablon = new bool[fi.sizeY, fi.sizeX];
            //take the points for the main roi
            List<Point> mainRoi;
            //calculates the stack
            List<Point>[] stackRoi = new List<Point>[roi.Stack];
            int rowSize = 0;

            if (roi.D >= 0)
            {
                mainRoi = CalculateEllipse(shablon, X, Y, W, H);
                rowSize = 4;

                for (int i = 0; i < roi.Stack; i++)
                {
                    X -= roi.D;
                    Y -= roi.D;
                    W += roi.D + roi.D;
                    H += roi.D + roi.D;

                    stackRoi[i] = CalculateEllipse(shablon, X, Y, W, H);
                    rowSize += 16;
                }
            }
            else
            {
                X -= roi.D * roi.Stack;
                Y -= roi.D * roi.Stack;
                W += (roi.D + roi.D) * roi.Stack;
                H += (roi.D + roi.D) * roi.Stack;

                mainRoi = CalculateEllipse(shablon, X, Y, W, H);
                rowSize = 4;

                for (int i = 0; i < roi.Stack; i++)
                {
                    X += roi.D;
                    Y += roi.D;
                    W -= roi.D + roi.D;
                    H -= roi.D + roi.D;

                    stackRoi[i] = CalculateEllipse(shablon, X, Y, W, H);
                    rowSize += 16;
                }

            }

            shablon = null;
            //measure frames
            roi.Results = new double[fi.imageCount][];

            Parallel.ForEach(SteppedRange(cVal, fi.imageCount, fi.sizeC), imageN =>
            {
                try {
                        Point p1 = roi.GetLocation(imageN)[0];
                        double[] res = new double[rowSize];
                        int dX = p.X - p1.X;
                        int dY = p.Y - p1.Y;
                        int position = 0;

                        double[] tempRes = CalculateMainResults(mainRoi, fi, imageN, dX, dY);
                        Array.Copy(tempRes, 0, res, position, tempRes.Length);
                        position += tempRes.Length;

                        //System.Windows.Forms.MessageBox.Show(string.Join("\t", tempRes));

                        Point midP = new Point(p1.X + roi.Width / 2, p1.Y + roi.Height / 2);

                        for (int i = 0; i < roi.Stack; i++)
                        {
                            tempRes = CalculateStackResults(stackRoi[i], midP, fi, imageN, dX, dY);
                            Array.Copy(tempRes, 0, res, position, tempRes.Length);
                            position += tempRes.Length;
                        }
                        roi.Results[imageN] = res;
                   
                }
                catch { return; }
            });
        }

        private static List<Point> CalculateEllipse(bool[,] shablon, int Xn, int Yn, int Wn, int Hn)
        {
            List<Point> pList = new List<Point>();
            
            int left = Xn;
            int top = Yn;
            int right = Xn+ Wn;
            int bottom = Yn + Hn;

            int a, b, x, y, temp;
            int old_y;
            int d1, d2;
            int a2, b2, a2b2, a2sqr, b2sqr, a4sqr, b4sqr;
            int a8sqr, b8sqr, a4sqr_b4sqr;
            int fn, fnw, fw;
            int fnn, fnnw, fnwn, fnwnw, fnww, fww, fwnw;

            if (right < left)
            {
                temp = left;
                left = right;
                right = temp;
            }
            if (bottom < top)
            {
                temp = top;
                top = bottom;
                bottom = temp;
            }

            a = (right - left) / 2;
            b = (bottom - top) / 2;

            x = 0;
            y = b;

            a2 = a * a;
            b2 = b * b;
            a2b2 = a2 + b2;
            a2sqr = a2 + a2;
            b2sqr = b2 + b2;
            a4sqr = a2sqr + a2sqr;
            b4sqr = b2sqr + b2sqr;
            a8sqr = a4sqr + a4sqr;
            b8sqr = b4sqr + b4sqr;
            a4sqr_b4sqr = a4sqr + b4sqr;

            fn = a8sqr + a4sqr;
            fnn = a8sqr;
            fnnw = a8sqr;
            fnw = a8sqr + a4sqr - b8sqr * a + b8sqr;
            fnwn = a8sqr;
            fnwnw = a8sqr + b8sqr;
            fnww = b8sqr;
            fwnw = b8sqr;
            fww = b8sqr;
            d1 = b2 - b4sqr * a + a4sqr;

            while ((fnw < a2b2) || (d1 < 0) || ((fnw - fn > b2) && (y > 0)))
            {
                DrawHorizontalOvalLine(left + x, right - x, top + y, shablon, pList); // Replace with your own span filling function. The hard-coded numbers were color values for testing purposes and can be ignored.
                DrawHorizontalOvalLine(left + x, right - x, bottom - y, shablon, pList);

                y--;
                if ((d1 < 0) || (fnw - fn > b2))
                {
                    d1 += fn;
                    fn += fnn;
                    fnw += fnwn;
                }
                else {
                    x++;
                    d1 += fnw;
                    fn += fnnw;
                    fnw += fnwnw;
                }
            }

            fw = fnw - fn + b4sqr;
            d2 = d1 + (fw + fw - fn - fn + a4sqr_b4sqr + a8sqr) / 4;
            fnw += b4sqr - a4sqr;

            old_y = y + 1;

            while (x <= a)
            {
                if (y != old_y) // prevent overdraw
                {
                    DrawHorizontalOvalLine(left + x, right - x, top + y, shablon, pList);
                    DrawHorizontalOvalLine(left + x, right - x, bottom - y, shablon,pList);
                }

                old_y = y;
                x++;
                if (d2 < 0)
                {
                    y--;
                    d2 += fnw;
                    fw += fwnw;
                    fnw += fnwnw;
                }
                else {
                    d2 += fw;
                    fw += fww;
                    fnw += fnww;
                }
            }

            return pList;
        }
        private static void DrawHorizontalOvalLine(int left, int right, int y,bool[,] shablon, List<Point> pList)
        {
            if (left < 0) left = 0;
            if (y < 0) y = 0;
            if (left >= shablon.GetLength(1)) left = shablon.GetLength(1) - 1;
            if (y >= shablon.GetLength(0)) y = shablon.GetLength(0) - 1;

            if (right < 0) right = 0;
            if (right >= shablon.GetLength(1)) right = shablon.GetLength(1) - 1;

            for (int x = left; x <= right; x++)
                if (shablon[y, x] == false)
                {
                    shablon[y, x] = true;
                    pList.Add(new Point(x,y));
                }
        }

        #endregion oval

        #region polygon
        public static List<Point> Polygon_Layers(int imageN, int D, ROI roi, Rectangle rect)
        {
            Point[] points = roi.GetLocation(imageN);
            List<Point> res = new List<Point>();

            double Cx = 0;
            double Cy = 0;
            foreach (Point p in points)
            {
                Cx += p.X;
                Cy += p.Y;
            }

            Cx /= points.Length;
            Cy /= points.Length;
            Cx = (int)Cx;
            Cy = (int)Cy;

            double dY, dX, C, C1, sinA;

            foreach (Point p in points)
            {
                C1 = Math.Sqrt((p.X - Cx) * (p.X - Cx) + (p.Y - Cy) * (p.Y - Cy));
                C = C1 + D;

                sinA = (p.Y - Cy) / C1;
                dY = sinA * C;

                dX = Math.Sqrt(C * C - dY * dY);

                dY += Cy;
                if (p.Y > Cy)
                    dY += 1;

                if (p.X > Cx)
                {
                    dX += Cx + 1;
                }
                else
                {
                    dX = Cx - dX;
                }

                res.Add(new Point((int)dX, (int)dY));
            }

            return res;
        }

        private static void GetPointsInPolygonStatic(ROI roi, TifFileInfo fi, int cVal)
        {
            //Point[][] points = roi.GetLocationAll();
            //GetPolygonPoints(points[0], fi, 0);
            Point[] points = roi.GetLocation(cVal);

            //take the points for the main roi
            List<Point> mainRoi = null;
            List<Point>[] stackRoi = null;
            int rowSize = 4;

            if(roi.Stack < 1)
            {
                mainRoi = GetPolygonPoints(points, fi);
            }
            else
            {
                //create shablon for preventing retacking the same value
                bool[,] shablon = new bool[fi.sizeY, fi.sizeX];
                stackRoi = new List<Point>[roi.Stack];

                if (roi.D >= 0)
                {
                    mainRoi = GetPolygonPoints(points.ToList(), fi, shablon);
                    rowSize = 4;

                    int D = roi.D;
                    for (int i = 0; i < roi.Stack; i++)
                    {
                        stackRoi[i] = GetPolygonPoints(
                            Polygon_Layers(cVal,D,roi,new Rectangle(0,0,fi.sizeX,fi.sizeY)), fi, shablon);

                        rowSize += 16;
                        D += roi.D;
                    }
                }
                else
                {
                    {

                        int D = roi.D*roi.Stack;
                        //inner layer
                        mainRoi = GetPolygonPoints(
                                Polygon_Layers(cVal, D, roi, new Rectangle(0, 0, fi.sizeX, fi.sizeY)), fi, shablon);

                        rowSize = 4;
                        D -= roi.D;
                        //midle layers
                        if (roi.Stack > 1)
                            for (int i = roi.Stack - 1; i > 0; i--)
                            {
                                stackRoi[i] = GetPolygonPoints(
                                    Polygon_Layers(cVal, D, roi, new Rectangle(0, 0, fi.sizeX, fi.sizeY)), fi, shablon);

                                rowSize += 16;
                                D -= roi.D;
                            }
                        //outer layer
                        stackRoi[0] = GetPolygonPoints(points.ToList(), fi, shablon);
                        rowSize += 16;
                    }
                }

                shablon = null;
            }

            //measure frames
            roi.Results = new double[fi.imageCount][];

            Parallel.ForEach(SteppedRange(cVal, fi.imageCount, fi.sizeC), imageN =>
            {
                try
                {
                    double[] res = new double[rowSize];
                    int position = 0;

                    double[] tempRes = CalculateMainResults(mainRoi, fi, imageN);

                    Array.Copy(tempRes, 0, res, position, tempRes.Length);
                    position += tempRes.Length;

                    for (int i = 0; i < roi.Stack; i++)
                    {
                        tempRes = CalculateMainResults(stackRoi[i], fi, imageN);
                        Array.Copy(tempRes, 0, res, position, tempRes.Length);
                        position += 16;
                    }

                    roi.Results[imageN] = res;
                }
                catch { return; }
            });
        }
        private static void GetPointsInPolygonTracking(ROI roi, TifFileInfo fi, int cVal)
        {
            Point[][] points = roi.GetLocationAll();

            int rowSize = 4;

            if (roi.Stack > 0)
                rowSize += roi.Stack * 16;

            //measure frames
            roi.Results = new double[fi.imageCount][];

            Parallel.ForEach(SteppedRange(cVal, fi.imageCount, fi.sizeC), imageN =>
            {
                try
                {

                    double[] res = new double[rowSize];
                    int position = 0;
                    double[] tempRes = null;

                    if (roi.Stack == 0)
                    {
                        tempRes = GetPolygonPoints(points[imageN], fi, imageN);
                        Array.Copy(tempRes, 0, res, position, tempRes.Length);
                    }
                    else
                    {
                        bool[,] shablon = new bool[fi.sizeY, fi.sizeX];

                        if (roi.D >= 0)
                        {
                            tempRes = GetPolygonPoints(points[imageN], fi, imageN, shablon);
                            Array.Copy(tempRes, 0, res, position, tempRes.Length);
                            position += tempRes.Length;

                            int D = roi.D;
                            for (int i = 0; i < roi.Stack; i++)
                            {
                                tempRes = GetPolygonPoints(
                                    Polygon_Layers(imageN, D, roi, new Rectangle(0, 0, fi.sizeX, fi.sizeY)).ToArray(), fi, imageN,shablon);
                                Array.Copy(tempRes, 0, res, position, tempRes.Length);
                                position += 16;
                                D += roi.D;
                            }

                        }
                        else
                        {
                            int D = roi.D * roi.Stack;
                            //inner layer
                            tempRes = GetPolygonPoints(
                                    Polygon_Layers(imageN, D, roi, new Rectangle(0, 0, fi.sizeX, fi.sizeY)).ToArray(), fi, imageN, shablon);
                            Array.Copy(tempRes, 0, res, position, tempRes.Length);
                            position += tempRes.Length;

                            D -= roi.D;
                            //midle layers
                            if (roi.Stack > 1)
                                for (int i = roi.Stack - 1; i > 0; i--)
                                {
                                    tempRes = GetPolygonPoints(
                                    Polygon_Layers(imageN, D, roi, new Rectangle(0, 0, fi.sizeX, fi.sizeY)).ToArray(), fi, imageN, shablon);
                                    Array.Copy(tempRes, 0, res, position, tempRes.Length);
                                    position += 16;
                                    D -= roi.D;
                                }
                            //outer layer
                            tempRes = GetPolygonPoints(points[imageN], fi, imageN, shablon);
                            Array.Copy(tempRes, 0, res, position, tempRes.Length);
                        }

                        shablon = null;
                    }
                    roi.Results[imageN] = res;
                }
                catch { return; }
            });
        }
        private static List<Point> GetPolygonPoints(Point[] points, TifFileInfo fi)
        {
            List<Point> pList = new List<Point>();
            List<int> xList = new List<int>();
            int x, y, swap, i, j, maxY = 0, minY = fi.sizeY - 1;
            //check the size of the polygon
            foreach (Point p in points)
            {
                if (p.Y > maxY && p.Y < fi.sizeY) maxY = p.Y;
                if (p.Y < minY && p.Y >= 0) minY = p.Y;
            }
            //scan lines for Y coords
            for (y = minY; y <= maxY; y++)
            {
                //prepare list for X coords
                xList.Clear();
                j = points.Length - 1;
                //calculate X points via tgA function
                for (i = 0; i < points.Length; i++)
                {
                    if ((points[i].Y < y && points[j].Y >= y) ||
                        (points[j].Y < y && points[i].Y >= y))
                    {
                        // tgA = (y2-y1)/(x2-x1)
                        x = (int)((((y - points[i].Y) * (points[j].X - points[i].X)) /
                            (points[j].Y - points[i].Y)) + points[i].X);

                        xList.Add(x);
                    }

                    j = i;
                }
                //break if there is no points in the line
                if (xList.Count == 0) continue;
                //sort by value via bubble loop
                i = 0;
                while (i < xList.Count - 1)
                {
                    j = i + 1;
                    if (xList[i] > xList[j])
                    {
                        swap = xList[i];
                        xList[i] = xList[j];
                        xList[j] = swap;

                        if (i > 0) i--;
                    }
                    else
                    {
                        i++;
                    }
                }
                //find all points inside the bounds 2 by 2
                for (i = 0; i < xList.Count; i += 2)
                {
                    j = i + 1;
                    if (xList[i] >= fi.sizeX) break;
                    if (xList[j] > 0)
                    {
                        if (xList[i] < 0) xList[i] = 0;
                        if (xList[j] >= fi.sizeX) xList[j] = fi.sizeX - 1;

                        for (x = xList[i]; x <= xList[j]; x++)
                            pList.Add(new Point(x, y));
                    }
                }
            }

            return pList;
        }

        private static List<Point> GetPolygonPoints(List<Point> points, TifFileInfo fi, bool[,] shablon)
        {
            List<Point> pList = new List<Point>();
            List<int> xList = new List<int>();
            int x, y, swap, i, j, maxY = 0, minY = fi.sizeY - 1;
            //check the size of the polygon
            foreach (Point p in points)
            {
                if (p.Y > maxY && p.Y < fi.sizeY) maxY = p.Y;
                if (p.Y < minY && p.Y >= 0) minY = p.Y;
            }
            //scan lines for Y coords
            for (y = minY; y <= maxY; y++)
            {
                //prepare list for X coords
                xList.Clear();
                j = points.Count - 1;
                //calculate X points via tgA function
                for (i = 0; i < points.Count; i++)
                {
                    if ((points[i].Y < y && points[j].Y >= y) ||
                        (points[j].Y < y && points[i].Y >= y))
                    {
                        // tgA = (y2-y1)/(x2-x1)
                        x = (int)((((y - points[i].Y) * (points[j].X - points[i].X)) /
                            (points[j].Y - points[i].Y)) + points[i].X);

                        xList.Add(x);
                    }

                    j = i;
                }
                //break if there is no points in the line
                if (xList.Count == 0) continue;
                //sort by value via bubble loop
                i = 0;
                while (i < xList.Count - 1)
                {
                    j = i + 1;
                    if (xList[i] > xList[j])
                    {
                        swap = xList[i];
                        xList[i] = xList[j];
                        xList[j] = swap;

                        if (i > 0) i--;
                    }
                    else
                    {
                        i++;
                    }
                }
                //find all points inside the bounds 2 by 2
                for (i = 0; i < xList.Count; i += 2)
                {
                    j = i + 1;
                    if (xList[i] >= fi.sizeX) break;
                    if (xList[j] > 0)
                    {
                        if (xList[i] < 0) xList[i] = 0;
                        if (xList[j] >= fi.sizeX) xList[j] = fi.sizeX - 1;

                        for (x = xList[i]; x <= xList[j]; x++)
                            if (shablon[y, x] == false)
                            {
                                pList.Add(new Point(x, y));
                                shablon[y, x] = true;
                            }
                    }
                }
            }

            return pList;
        }
        private static double[] GetPolygonPoints(Point[] points, TifFileInfo fi, int imageN, bool[,] shablon)
        {
            double area = 0;
            double mean = 0;
            double max = 0;
            double min = double.MaxValue;

            List<int> xList = new List<int>();
            int x, y, swap, i, j, maxY = 0, minY = fi.sizeY - 1;
            //check the size of the polygon
            foreach (Point p in points)
            {
                if (p.Y > maxY && p.Y < fi.sizeY) maxY = p.Y;
                if (p.Y < minY && p.Y >= 0) minY = p.Y;
            }
            //scan lines for Y coords
            for (y = minY; y <= maxY; y++)
            {
                //prepare list for X coords
                xList.Clear();
                j = points.Length - 1;
                //calculate X points via tgA function
                for (i = 0; i < points.Length; i++)
                {
                    if ((points[i].Y < y && points[j].Y >= y) ||
                        (points[j].Y < y && points[i].Y >= y))
                    {
                        // tgA = (y2-y1)/(x2-x1)
                        x = (int)((((y - points[i].Y) * (points[j].X - points[i].X)) /
                            (points[j].Y - points[i].Y)) + points[i].X);

                        xList.Add(x);
                    }

                    j = i;
                }
                //break if there is no points in the line
                if (xList.Count == 0) continue;
                //sort by value via bubble loop
                i = 0;
                while (i < xList.Count - 1)
                {
                    j = i + 1;
                    if (xList[i] > xList[j])
                    {
                        swap = xList[i];
                        xList[i] = xList[j];
                        xList[j] = swap;

                        if (i > 0) i--;
                    }
                    else
                    {
                        i++;
                    }
                }
                switch (fi.bitsPerPixel)
                {
                    case 8:
                        byte[][] image = fi.image8bit[imageN];
                        
                        //find all points inside the bounds 2 by 2
                        for (i = 0; i < xList.Count; i += 2)
                        {
                            j = i + 1;
                            if (xList[i] >= fi.sizeX) break;
                            if (xList[j] > 0)
                            {
                                if (xList[i] < 0) xList[i] = 0;
                                if (xList[j] >= fi.sizeX) xList[j] = fi.sizeX - 1;

                                for (x = xList[i]; x <= xList[j]; x++)
                                    if (shablon[y, x] == false)
                                    {
                                        //image[y][x] = 0;
                                        byte val = image[y][x];

                                        area++;
                                        mean += val;
                                        if (max < val) max = val;
                                        if (min > val) min = val;
                                        shablon[y, x] = true;
                                    }
                            }
                        }
                        break;
                    case 16:
                        ushort[][] image16 = fi.image16bit[imageN];
                       
                        //find all points inside the bounds 2 by 2
                        for (i = 0; i < xList.Count; i += 2)
                        {
                            j = i + 1;
                            if (xList[i] >= fi.sizeX) break;
                            if (xList[j] > 0)
                            {
                                if (xList[i] < 0) xList[i] = 0;
                                if (xList[j] >= fi.sizeX) xList[j] = fi.sizeX - 1;

                                for (x = xList[i]; x <= xList[j]; x++)
                                    if (shablon[y, x] == false)
                                    {
                                        //image16[y][x] = 0;
                                        ushort val = image16[y][x];

                                        area++;
                                        mean += val;
                                        if (max < val) max = val;
                                        if (min > val) min = val;
                                        shablon[y, x] = true;
                                    }
                            }
                        }
                        break;
                }

            }

            if (area > 0) mean /= area;
            double[] res = new double[] { area, mean, min, max };

            return res;
        }
        private static double[] GetPolygonPoints(Point[] points, TifFileInfo fi, int imageN)
        {
            double area = 0;
            double mean = 0;
            double max = 0;
            double min = double.MaxValue;

            List<int> xList = new List<int>();
            int x, y, swap, i, j, maxY = 0, minY = fi.sizeY - 1;
            //check the size of the polygon
            foreach (Point p in points)
            {
                if (p.Y > maxY && p.Y < fi.sizeY) maxY = p.Y;
                if (p.Y < minY && p.Y >= 0) minY = p.Y;
            }
            //scan lines for Y coords
            for (y = minY; y <= maxY; y++)
            {
                //prepare list for X coords
                xList.Clear();
                j = points.Length - 1;
                //calculate X points via tgA function
                for (i = 0; i < points.Length; i++)
                {
                    if ((points[i].Y < y && points[j].Y >= y) ||
                        (points[j].Y < y && points[i].Y >= y))
                    {
                        // tgA = (y2-y1)/(x2-x1)
                        x = (int)((((y - points[i].Y) * (points[j].X - points[i].X)) /
                            (points[j].Y - points[i].Y)) + points[i].X);

                        xList.Add(x);
                    }

                    j = i;
                }
                //break if there is no points in the line
                if (xList.Count == 0) continue;
                //sort by value via bubble loop
                i = 0;
                while (i < xList.Count - 1)
                {
                    j = i + 1;
                    if (xList[i] > xList[j])
                    {
                        swap = xList[i];
                        xList[i] = xList[j];
                        xList[j] = swap;

                        if (i > 0) i--;
                    }
                    else
                    {
                        i++;
                    }
                }
                switch (fi.bitsPerPixel)
                {
                    case 8:
                        byte[][] image = fi.image8bit[imageN];
                        
                        //find all points inside the bounds 2 by 2
                        for (i = 0; i < xList.Count; i += 2)
                        {
                            j = i + 1;
                            if (xList[i] >= fi.sizeX) break;
                            if (xList[j] > 0)
                            {
                                if (xList[i] < 0) xList[i] = 0;
                                if (xList[j] >= fi.sizeX) xList[j] = fi.sizeX - 1;

                                for (x = xList[i]; x <= xList[j]; x++)
                                {
                                    //image[y][x] = 0;
                                    byte val = image[y][x];

                                    area++;
                                    mean += val;
                                    if (max < val) max = val;
                                    if (min > val) min = val;
                                }
                            }
                        }
                        break;
                    case 16:
                        ushort[][] image16 = fi.image16bit[imageN];
                        
                        //find all points inside the bounds 2 by 2
                        for (i = 0; i < xList.Count; i += 2)
                        {
                            j = i + 1;
                            if (xList[i] >= fi.sizeX) break;
                            if (xList[j] > 0)
                            {
                                if (xList[i] < 0) xList[i] = 0;
                                if (xList[j] >= fi.sizeX) xList[j] = fi.sizeX - 1;

                                for (x = xList[i]; x <= xList[j]; x++)
                                {
                                    //image16[y][x] = 0;
                                    ushort val = image16[y][x];

                                    area++;
                                    mean += val;
                                    if (max < val) max = val;
                                    if (min > val) min = val;
                                }
                            }
                        }
                        break;
                }

            }

            if (area > 0) mean /= area;
            double[] res = new double[] { area, mean, min, max };

            return res;
        }
        #endregion polygon
        /*
        private static void GetPolygonPoints(Point[] points, TifFileInfo fi, int imageN)
        {
            List<int> xList = new List<int>();
            int x, y, swap, i , j, maxY = 0, minY = fi.sizeY - 1;
            //check the size of the polygon
            foreach(Point p in points)
            {
                if (p.Y > maxY && p.Y < fi.sizeY) maxY = p.Y;
                if (p.Y < minY && p.Y >= 0) minY = p.Y;
            }
            //scan lines for Y coords
            for (y = minY; y <= maxY; y++)
            {
                //prepare list for X coords
                xList.Clear();
                j = points.Length - 1;
                //calculate X points via tgA function
                for (i = 0; i < points.Length; i++)
                {
                    if ((points[i].Y < y && points[j].Y >= y) ||
                        (points[j].Y < y && points[i].Y >= y))
                    {
                        // tgA = (y2-y1)/(x2-x1)
                        x = (int)((((y - points[i].Y) * (points[j].X - points[i].X)) /
                            (points[j].Y - points[i].Y)) + points[i].X);

                        xList.Add(x);
                    }

                    j = i;
                }
                //break if there is no points in the line
                if (xList.Count == 0) continue;
                //sort by value via bubble loop
                i = 0;
                while (i < xList.Count - 1)
                {
                    j = i + 1;
                    if (xList[i] > xList[j])
                    {
                        swap = xList[i];
                        xList[i] = xList[j];
                        xList[j] = swap;

                        if (i > 0) i--;
                    }
                    else
                    {
                        i++;
                    }
                }
                //find all points inside the bounds 2 by 2
                for (i = 0; i < xList.Count; i += 2)
                {
                    j = i + 1;
                    if (xList[i] >= fi.sizeX) break;
                    if (xList[j] > 0)
                    {
                        if (xList[i] < 0) xList[i] = 0;
                        if (xList[j] >= fi.sizeX) xList[j] = fi.sizeX - 1;

                        for (x = xList[i]; x <= xList[j]; x++)
                            fi.image8bit[imageN][y][x] = 0;
                    }
                }
            }
        }
        
        private static void FillEllipse(ROI roi,TifFileInfo fi, int imageN)
        {
            Point[] points = roi.GetLocation(imageN);
            long left = points[0].X;
            long top = points[0].Y;
            long right = points[0].X + (long)roi.Width;
            long bottom = points[0].Y + (long)roi.Height;

            long a, b, x, y, temp;
            long old_y;
            long d1, d2;
            long a2, b2, a2b2, a2sqr, b2sqr, a4sqr, b4sqr;
            long a8sqr, b8sqr, a4sqr_b4sqr;
            long fn, fnw, fw;
            long fnn, fnnw, fnwn, fnwnw, fnww, fww, fwnw;

            if (right < left)
            {
                temp = left;
                left = right;
                right = temp;
            }
            if (bottom < top)
            {
                temp = top;
                top = bottom;
                bottom = temp;
            }

            a = (right - left) / 2;
            b = (bottom - top) / 2;

            x = 0;
            y = b;

            a2 = a * a;
            b2 = b * b;
            a2b2 = a2 + b2;
            a2sqr = a2 + a2;
            b2sqr = b2 + b2;
            a4sqr = a2sqr + a2sqr;
            b4sqr = b2sqr + b2sqr;
            a8sqr = a4sqr + a4sqr;
            b8sqr = b4sqr + b4sqr;
            a4sqr_b4sqr = a4sqr + b4sqr;

            fn = a8sqr + a4sqr;
            fnn = a8sqr;
            fnnw = a8sqr;
            fnw = a8sqr + a4sqr - b8sqr * a + b8sqr;
            fnwn = a8sqr;
            fnwnw = a8sqr + b8sqr;
            fnww = b8sqr;
            fwnw = b8sqr;
            fww = b8sqr;
            d1 = b2 - b4sqr * a + a4sqr;

            while ((fnw < a2b2) || (d1 < 0) || ((fnw - fn > b2) && (y > 0)))
            {
                DrawHorizontalLine(left + x, right - x, top + y,fi, imageN); // Replace with your own span filling function. The hard-coded numbers were color values for testing purposes and can be ignored.
                DrawHorizontalLine(left + x, right - x, bottom - y, fi, imageN);

                y--;
                if ((d1 < 0) || (fnw - fn > b2))
                {
                    d1 += fn;
                    fn += fnn;
                    fnw += fnwn;
                }
                else {
                    x++;
                    d1 += fnw;
                    fn += fnnw;
                    fnw += fnwnw;
                }
            }

            fw = fnw - fn + b4sqr;
            d2 = d1 + (fw + fw - fn - fn + a4sqr_b4sqr + a8sqr) / 4;
            fnw += b4sqr - a4sqr;

            old_y = y + 1;

            while (x <= a)
            {
                if (y != old_y) // prevent overdraw
                {
                    DrawHorizontalLine(left + x, right - x, top + y, fi, imageN);
                    DrawHorizontalLine(left + x, right - x, bottom - y,fi, imageN);
                }

                old_y = y;
                x++;
                if (d2 < 0)
                {
                    y--;
                    d2 += fnw;
                    fw += fwnw;
                    fnw += fnwnw;
                }
                else {
                    d2 += fw;
                    fw += fww;
                    fnw += fnww;
                }
            }
        }
        private static void DrawHorizontalLine(long left, long right, long y, TifFileInfo fi, int imageN)
        {
            for(long x = left; x<= right; x++)
            {
                fi.image8bit[imageN][y][x] = 0;
            }
        }
        */
    }
}