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
    class Wand
    {
        //This class is translated from Fiji ImageJ Java source code
        #region CellTool variables
        private ImageAnalyser IA;
        private bool[,] shablon = null;
        private List<Point> PxlList = new List<Point>();
        private int width, height;
        private int xmin;                   //of selection created

        #endregion CellTool variables

        /** Constructs a Wand object from an ImageProcessor. */
        public Wand(ImageAnalyser IA)
        {
            this.IA = IA;
        }
        /** Traces an object defined by lower and upper threshold values.
          * 'mode' can be FOUR_CONNECTED or EIGHT_CONNECTED.
          * ('LEGACY_MODE' is also supported and may result in selection of
          * interior holes instead of the thresholded area if one clicks left
          * of an interior hole).
          * The start coordinates must be inside the area or left of it.
          * When successful, npoints>0 and the boundary points can be accessed
          * in the public xpoints and ypoints fields. */
       

        /** Traces an object defined by lower and upper threshold values or an
          * interior hole; whatever is found first ('legacy mode').
          * For compatibility with previous versions of ImageJ.
          * The start coordinates must be inside the area or left of it.
          * When successful, npoints>0 and the boundary points can be accessed
          * in the public xpoints and ypoints fields. */
        

        /** This is a variation of legacy autoOutline that uses int threshold arguments. */
     
        /** Traces the boundary of an area of uniform color, where
          * 'startX' and 'startY' are somewhere inside the area.
          * When successful, npoints>0 and the boundary points can be accessed
          * in the public xpoints and ypoints fields.
          * For compatibility with previous versions of ImageJ only; otherwise
          * use the reliable method specifying 4-connected or 8-connected mode
          * and the tolerance. */
       

        /** Traces the boundary of the area with pixel values within
          * 'tolerance' of the value of the pixel at the starting location.
          * 'tolerance' is in uncalibrated units.
          * 'mode' can be FOUR_CONNECTED or EIGHT_CONNECTED.
          * Mode LEGACY_MODE is for compatibility with previous versions of ImageJ;
          * ignored if tolerance > 0.
          * Mode bit THRESHOLDED_MODE for internal use only; it is set by autoOutline
          * with 'upper' and 'lower' arguments.
          * When successful, npoints>0 and the boundary points can be accessed
          * in the public xpoints and ypoints fields. */
        public List<Point> autoOutline(int startX, int startY, bool[,] shablon)
        {
            PxlList.Clear();
            this.shablon = shablon;
            this.width = shablon.GetLength(1);
            this.height = shablon.GetLength(0);

            if (startX < 0 || startX >= width || startY < 0 || startY >= height) return PxlList;
            
            int x = startX;
            int y = startY;
            int seedX = 0;

            // the first inside pixel
            if (inside(x, y))
            {              // find a border when coming from inside
                seedX = x;                // (seedX, startY) is an inside pixel
                do { x++; } while (inside(x, y));
            }
            
            //now, we have a border between (x-1, y) and (x,y)
            bool first = true;
            while (true)
            {                  // loop until we have not traced an inner hole
                bool insideSelected = traceEdge(x,y);
                if (insideSelected)
                {       // not an inner hole
                    if (first) return PxlList;      // started at seed, so we got it (sucessful)
                    if (xmin <= seedX)
                    {      // possibly the correct particle
                           /*
                           Polygon poly = new Polygon(xpoints, ypoints, npoints);
                           if (poly.contains(seedX, startY))
                               return;         // successful, particle contains seed
                               */
                        if (IA.RoiMan.IsPointInPolygon(new Point(seedX, startY), PxlList.ToArray()))
                            return PxlList; // successful, particle contains seed
                    }
                }
                first = false;
                // we have traced an inner hole or the wrong particle
                if (!inside(x, y)) do
                    {
                        x++;                    // traverse the hole
                        if (x > width) return PxlList; //should never happen
                    } while (!inside(x, y));
                do { x++; } while (inside(x, y)); //retry here; maybe no inner hole any more
            }
        }
        /* Trace the outline, starting at a point (startX, startY). 
         * Pixel (startX-1, startY) must be outside, (startX, startY) must be inside,
         * or reverse. Otherwise an endless loop will occur (and eat up all memory).
         * Traces 8-connected inside pixels unless fourConnected is true.
         * Returns whether the selection created encloses an 'inside' area
         * and not an inner hole.
         */
        
        private bool traceEdge(int startX, int startY)
        {
            // Let us name the crossings between 4 pixels vertices, then the
            // vertex (x,y) marked with '+', is between pixels (x-1, y-1) and (x,y):
            //
            //    pixel    x-1    x
            //      y-1        |
            //             ----+----
            //       y         |
            //
            // The four principal directions are numbered such that the direction
            // number * 90 degrees gives the angle in the mathematical sense; and
            // the directions to the adjacent pixels (for inside(x,y,direction) are
            // at (number * 90 - 45) degrees:
            //      walking                     pixel
            //   directions:   1           directions:     2 | 1
            //              2  +  0                      ----+----
            //                 3                           3 | 0
            //
            // Directions, like angles, are cyclic; direction -1 = direction 3, etc.
            //
            // The algorithm: We walk along the border, from one vertex to the next,
            // with the outside pixels always being at the left-hand side.
            // For 8-connected tracing, we always trying to turn left as much as
            // possible, to encompass an area as large as possible.
            // Thus, when walking in direction 1 (up, -y), we start looking
            // at the pixel in direction 2; if it is inside, we proceed in this
            // direction (left); otherwise we try with direction 1 (up); if pixel 1
            // is not inside, we must proceed in direction 0 (right).
            //
            //                     2 | 1                 (i=inside, o=outside)
            //      direction 2 < ---+---- > direction 0
            //                     o | i
            //                       ^ direction 1 = up = starting direction
            //
            // For 4-connected pixels, we try to go right as much as possible:
            // First try with pixel 1; if it is outside we go in direction 0 (right).
            // Otherwise, we examine pixel 2; if it is outside, we go in
            // direction 1 (up); otherwise in direction 2 (left).
            //
            // When moving a closed loop, 'direction' gets incremented or decremented
            // by a total of 360 degrees (i.e., 4) for counterclockwise and clockwise
            // loops respectively. As the inside pixels are at the right side, we have
            // got an outline of inner pixels after a cw loop (direction decremented
            // by 4).
            //
            PxlList.Clear();
            xmin = width;
            int startDirection;
            if (inside(startX, startY))      // inside at left, outside right
                startDirection = 1;         // starting in direction 1 = up
            else {
                startDirection = 3;         // starting in direction 3 = down
                startY++;                   // continue after the boundary that has direction 3
            }

            int x = startX;
            int y = startY;
            int direction = startDirection;

            do
            {
                int newDirection = direction;
                
                    do
                    {
                        if (!inside(x, y, newDirection)) break;
                        newDirection++;
                    } while (newDirection < direction + 2);
                    newDirection--;
                
                if (newDirection != direction)
                    addPoint(x, y);          // a corner point of the outline polygon: add to list
                switch (newDirection & 3)
                { // '& 3' is remainder modulo 4
                    case 0: x++; break;
                    case 1: y--; break;
                    case 2: x--; break;
                    case 3: y++; break;
                }
                direction = newDirection;
            } while (x != startX || y != startY || (direction & 3) != startDirection);
            if (PxlList[0].X != x)            // if the start point = end point is a corner: add to list
                addPoint(x, y);

            return (direction <= 0);        // if we have done a clockwise loop, inside pixels are enclosed
        }

        // add a point x,y to the outline polygon
        private void addPoint(int x, int y)
        {
            PxlList.Add(new Point(x, y));
            if (xmin > x) xmin = x;
        }

        // check pixel at (x,y), whether it is inside traced area
        
        private bool inside(int x, int y)
        {
            if (x < 0 || x >= width || y < 0 || y >= height)
                return false;
            return shablon[y,x];
        }

        // check pixel in a given direction from vertex (x,y)
        private bool inside(int x, int y, int direction)
        {
            switch (direction & 3)
            {         // '& 3' is remainder modulo 4
                case 0: return inside(x, y);
                case 1: return inside(x, y - 1);
                case 2: return inside(x - 1, y - 1);
                case 3: return inside(x - 1, y);
            }
            return false; //will never occur, needed for the compiler
        }
        
        /* Are we tracing a one pixel wide line? Makes Legacy mode 8-connected instead of 4-connected */
        private bool isLine(int xs, int ys)
        {
            int r = 5;
            int xmin = xs;
            int xmax = xs + 2 * r;
            if (xmax >= width) xmax = width - 1;
            int ymin = ys - r;
            if (ymin < 0) ymin = 0;
            int ymax = ys + r;
            if (ymax >= height) ymax = height - 1;
            int area = 0;
            int insideCount = 0;
            for (int x = xmin; (x <= xmax); x++)
                for (int y = ymin; y <= ymax; y++)
                {
                    area++;
                    if (inside(x, y))
                        insideCount++;
                }
            
            return ((double)insideCount) / area < 0.25;
        }
    }
}
