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
    class ConvexHull
    {
        public static Point[] g_MinMaxCorners;
        public static Rectangle g_MinMaxBox;
        public static Point[] g_NonCulledPoints;
        public static List<Point> MakeConvexHull(List<Point> points)
        {
            // Find the remaining point with the smallest Y value.
            // if (there's a tie, take the one with the smaller X value.
            Point best_pt = points[0];
            foreach (Point pt in points)
            {
                if ((pt.Y < best_pt.Y) ||
                   ((pt.Y == best_pt.Y) && (pt.X < best_pt.X)))
                {
                    best_pt = pt;
                }
            }
           
            // Move this point to the convex hull.
            List<Point> hull = new List<Point>();
            hull.Add(best_pt);
            points.Remove(best_pt);

            // Start wrapping up the other points.
            float sweep_angle = 0;
            for (;;)
            {
                // Find the point with smallest AngleValue
                // from the last point.
                int X = hull[hull.Count - 1].X;
                int Y = hull[hull.Count - 1].Y;
                best_pt = points[0];
                float best_angle = 3600;

                // Search the rest of the points.
                foreach (Point pt in points)
                {
                    float test_angle = AngleValue(X, Y, pt.X, pt.Y);
                    if ((test_angle >= sweep_angle) &&
                        (best_angle > test_angle))
                    {
                        best_angle = test_angle;
                        best_pt = pt;
                    }
                }

                // See if the first point is better.
                // If so, we are done.
                float first_angle = AngleValue(X, Y, hull[0].X, hull[0].Y);
                if ((first_angle >= sweep_angle) &&
                    (best_angle >= first_angle))
                {
                    // The first point is better. We're done.
                    break;
                }

                // Add the best point to the convex hull.
                hull.Add(best_pt);
                points.Remove(best_pt);

                sweep_angle = best_angle;

                // If all of the points are on the hull, we're done.
                if (points.Count == 0) break;
            }

            return hull;
        }
        private static float AngleValue(int x1, int y1, int x2, int y2)
        {
            float dx, dy, ax, ay, t;

            dx = x2 - x1;
            ax = Math.Abs(dx);
            dy = y2 - y1;
            ay = Math.Abs(dy);
            if (ax + ay == 0)
            {
                // if (the two points are the same, return 360.
                t = 360f / 9f;
            }
            else
            {
                t = dy / (ax + ay);
            }
            if (dx < 0)
            {
                t = 2 - t;
            }
            else if (dy < 0)
            {
                t = 4 + t;
            }
            return t * 90;
        }

    }
}

