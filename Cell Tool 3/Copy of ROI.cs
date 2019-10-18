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
using System.Windows.Forms;
using System.Drawing;

namespace Cell_Tool_3
{
    class ROI : TreeNode
    {
        public string Comment = "";

        private int ID;
        #region Variables
        private int shape = 0;
        /*shape
        0 - rectangle 
        1 - oval
        2 - polygon
        3 - freehand 
        4 - magic wand       
        */
        private int type = 0;
        /*type
        0 - static
        1 - tracking
        */

        #region Coord controls
        private Point[] TrackingLocation; //use for oval and rectangle - tracking
        private Point[] StaticLocation;//use for oval and rectangle - static
        private Point[][] TrackingBorderPoints; //use for polygon and freehand - tracking
        private Point[][] StaticBorderPoints; //use for polygon and freehand - static
        #endregion Coord controls

        private int W = 0;
        private int H = 0;
        private int BiggestW = 0;
        private int BiggestH = 0;
        private bool ReturnBiggest = false;

        public bool turnOnStackRoi;
        private int stack = 0;
        private int d = 0;

        private int firstTFrame = 0;
        private int lastTFrame = 0;
        private int firstZFrame = 0;
        private int lastZFrame = 0;

        public double[][] Results;

        public Color[] colors;
        public bool[] ChartUseIndex;
        public bool[] expanded;

        #endregion Variables
        public ROI()
        {
            this.Name = "None";
            this.Checked = true;
        }
        public void Delete()
        {
            TrackingLocation = null; //use for oval and rectangle - tracking
            StaticLocation = null;//use for oval and rectangle - static
            TrackingBorderPoints = null; //use for polygon and freehand - tracking
            StaticBorderPoints = null; //use for polygon and freehand - static
            colors = null;
            ChartUseIndex = null;
            expanded = null;
        }
        public ROI(int ID, int imageCount, int shape, int type, bool turnOnStackRoi)
        {
            this.ID = ID;
            this.Name = "None";
            this.Checked = true;
            this.shape = shape;
            this.type = type;
            this.SelectedImageIndex = shape;
            this.ImageIndex = shape;
            this.turnOnStackRoi = turnOnStackRoi;
            //Prepare coord control
            if (shape == 0 | shape == 1)//rectangle and oval
            {
                if (type == 0)//static
                    StaticLocation = new Point[1];
                else if (type == 1)//tracking
                    TrackingLocation = new Point[imageCount];
            }
            else if (shape == 2 | shape == 3)//polygon and freehand
            {
                if (type == 0)//static
                {
                    StaticBorderPoints = new Point[1][];
                    StaticBorderPoints[0] = new Point[0];
                }
                if (type == 1)//tracking
                {
                    TrackingBorderPoints = new Point[imageCount][];
                    for (int i = 0; i < imageCount; i++)
                        TrackingBorderPoints[i] = new Point[0];
                }
            }
        }
        #region Properties
        public int getID
        {
            get
            {
                return this.ID;
            }
            set
            {
                this.ID = value;
            }
        }
        public int biggestW
        {
            get
            {
                return this.BiggestW;
            }
            set
            {
                this.BiggestW = value;
            }
        }
        public bool returnBiggest
        {
            get
            {
                return this.ReturnBiggest;
            }
            set
            {
                this.ReturnBiggest = value;
            }
        }
        public int biggestH
        {
            get
            {
                return this.BiggestH;
            }
            set
            {
                this.BiggestH = value;
            }
        }
        public int Shape
        {
            get
            {
                return this.shape;
            }
        }
        public int Type
        {
            get
            {
                return this.type;
            }
        }
        public int Width
        {
            get
            {
                return this.W;
            }
            set
            {
                this.W = value;
            }
        }
        public int Height
        {
            get
            {
                return this.H;
            }
            set
            {
                this.H = value;
            }
        }
        public int Stack
        {
            get
            {
                return this.stack;
            }
            set
            {
                this.stack = value;
            }
        }
        public int D
        {
            get
            {
                return this.d;
            }
            set
            {
                this.d = value;
            }
        }
        public int FromT
        {
            get
            {
                return this.firstTFrame;
            }
            set
            {
                this.firstTFrame = value;
            }
        }
        public int ToT
        {
            get
            {
                return this.lastTFrame;
            }
            set
            {
                this.lastTFrame = value;
            }
        }
        public int FromZ
        {
            get
            {
                return this.firstZFrame;
            }
            set
            {
                this.firstZFrame = value;
            }
        }
        public int ToZ
        {
            get
            {
                return this.lastZFrame;
            }
            set
            {
                this.lastZFrame = value;
            }
        }
        public Point[] GetLocation(int ImageN)
        {
            Point[] points = null;

            if (shape == 0 | shape == 1)//rectangle and oval
            {
                if (type == 0)//static
                    points = StaticLocation;
                else if (type == 1)//tracking
                    points = new Point[] { TrackingLocation[ImageN] };
            }
            else if (shape == 2 | shape == 3)//polygon and freehand
            {
                if (type == 0)//static
                    points = StaticBorderPoints[0];
                else if (type == 1)//tracking
                    points = TrackingBorderPoints[ImageN];
            }
            
            return points;
        }
        public void SetLocation(int ImageN, Point[] points)
        {
            if (shape == 0 | shape == 1)//rectangle and oval
            {
                if (type == 0)//static
                    StaticLocation = points;
                else if (type == 1)//tracking
                    TrackingLocation[ImageN] = points[0];
            }
            else if (shape == 2 | shape == 3)//polygon and freehand
            {
                if (type == 0)//static
                {
                    StaticBorderPoints[0] = points;
                }
                else if (type == 1)//tracking
                    TrackingBorderPoints[ImageN] = points;
            }
        }
        
        public void SetLocationAll(Point[][] points)
        {
            if (shape == 0 | shape == 1)//rectangle and oval
            {
                if (type == 0)//static
                    StaticLocation = points[0];
                else if (type == 1)//tracking
                    TrackingLocation = points[0];
            }
            else if (shape == 2 | shape == 3)//polygon and freehand
            {
                if (type == 0)//static
                {
                    if (StaticBorderPoints == null) StaticBorderPoints = new Point[1][];
                    StaticBorderPoints[0] = points[0];
                }
                else if (type == 1)//tracking
                {
                    TrackingBorderPoints = points;
                }
            }
        }
        public Point[][] GetLocationAll()
        {
            Point[][] points = null;
            if (shape == 0 | shape == 1)//rectangle and oval
            {
                points = new Point[1][];
                if (type == 0)//static
                {
                     points[0]  = DuplicatePointArray(StaticLocation);
                }
                else if (type == 1)//tracking
                {
                    points[0] = DuplicatePointArray(TrackingLocation);
                }
            }
            else if (shape == 2 | shape == 3)//polygon and freehand
            {
                if (type == 0)//static
                {
                    points = new Point[1][];
                    points[0] = DuplicatePointArray(StaticBorderPoints[0]);
                }
                else if (type == 1)//tracking
                {
                    points = new Point[TrackingBorderPoints.Length][];
                    for(int i = 0; i< TrackingBorderPoints.Length;i++)
                        points[i] = DuplicatePointArray(TrackingBorderPoints[i]);
                }
            }
            return points;
        }
        private Point[] DuplicatePointArray(Point[] source)
        {
            if (source == null || source.Length == 0) return null;

            Point[] result = new Point[source.Length];
            for(int i = 0; i< source.Length; i++)
            {
                result[i].X = source[i].X;
                result[i].Y = source[i].Y;
            }
            return result;
        }
        public PointF GetMidPoint(int ImageN)
        {
            PointF p = new PointF();
            if (shape == 0 | shape == 1)//rectangle and oval
            {
                if (type == 0)//static
                {
                    p.X = StaticLocation[0].X + W/2;
                    p.Y = StaticLocation[0].Y + H/2;
                }
                else if (type == 1)//tracking
                {
                    p.X = TrackingLocation[ImageN].X + W / 2;
                    p.Y = TrackingLocation[ImageN].Y + H / 2;
                }
            }
            else if (shape == 2 | shape == 3)//polygon and freehand
            {
                if (type == 0)//static
                {
                    p.X = 0;
                    p.Y = 0;
                    foreach (Point p1 in StaticBorderPoints[0])
                    {
                        p.X += p1.X;
                        p.Y += p1.Y;
                    }
                    p.X /= StaticBorderPoints[0].Length;
                    p.Y /= StaticBorderPoints[0].Length;
                }
                else if (type == 1)//tracking
                {
                    p.X = 0;
                    p.Y = 0;
                    foreach (Point p1 in TrackingBorderPoints[ImageN])
                    {
                        p.X += p1.X;
                        p.Y += p1.Y;
                    }
                    p.X /= TrackingBorderPoints[ImageN].Length;
                    p.Y /= TrackingBorderPoints[ImageN].Length;
                }
            }
            return p;
        }
        #endregion Properties

        #region Dupliate
        public ROI Duplicate()
        {
            ROI newRoi = new ROI(ID, 1, shape, type, turnOnStackRoi);

            newRoi.Comment = Comment;

            newRoi.W = W;
            newRoi.H = H;
            
            newRoi.Stack = stack;
            newRoi.D = d;

            newRoi.firstTFrame = firstTFrame;
            newRoi.lastTFrame = lastTFrame;
            newRoi.firstZFrame = firstZFrame;
            newRoi.lastZFrame = lastZFrame;

            newRoi.SetLocationAll(this.GetLocationAll());
            
            return newRoi;
        }
        #endregion Duplicate

        #region History
        public string getRoiResizeToHistory(int C, int imageN)
        {
            //roi.resize(chanelN,roiID,imageN,W,H,Location)
            return "roi.resize(" + C.ToString() + "," +
                this.ID.ToString() + "," +
                imageN.ToString() + "," +
                W.ToString() + "," +
                H.ToString() + "," +
                locationToHist(imageN) + ")";
        }
        public string getStatus(string stat, int imageN = 0)
        {
            string val = stat + "=";

            switch (stat)
            {
                case "Comment":
                    val += this.Comment;
                    break;
                case "Check":
                    val += this.Checked.ToString();
                    break;
                case "W":
                    val+=this.W;
                    break;
                case "H":
                    val += this.H;
                    break;
                case "Stack":
                    val += this.stack;
                    break;
                case "D":
                    val += this.d;
                    break;
                case "fromT":
                    val += this.FromT;
                    break;
                case "toT":
                    val += this.ToT;
                    break;
                case "fromZ":
                    val += this.FromZ;
                    break;
                case "toZ":
                    val += this.ToZ;
                    break;
                case "Location":
                    val += locationToHist(imageN);
                    break;
                default:
                    MessageBox.Show("Error!");
                    break;
            }

            return val;
        }
        private string locationToHist(int imageN)
        {
           string val = "";
           foreach (Point p in this.GetLocation(imageN))
                val += p.X.ToString() + "\t" + p.Y.ToString() + "\t";

           return val;
        }
        public void setStatus(string val, int imageN = 0)
        {
            string[] vals = val.Split(new string[] { "=" }, StringSplitOptions.None);
            switch (vals[0])
            {
                case "Comment":
                    this.Comment = vals[1];
                    break;
                case "Check":
                    this.Checked = bool.Parse(vals[1]);
                    break;
                case "W":
                    this.W = int.Parse(vals[1]);
                    break;
                case "H":
                    this.H = int.Parse(vals[1]);
                    break;
                case "Stack":
                    this.stack = int.Parse(vals[1]);
                    break;
                case "D":
                    this.d = int.Parse(vals[1]);
                    break;
                case "fromT":
                    this.FromT = int.Parse(vals[1]);
                    break;
                case "toT":
                    this.ToT = int.Parse(vals[1]);
                    break;
                case "fromZ":
                    this.FromZ = int.Parse(vals[1]);
                    break;
                case "toZ":
                    this.ToZ = int.Parse(vals[1]);
                    break;
                case "Location":
                    locationFromHist(vals[1],imageN);
                    break;
                default:
                    MessageBox.Show("Error!");
                    break;
            }
        }
        public void locationFromHist(string val, int imageN)
        {
            string[] row = val.Split(new string[] { "\t" }, StringSplitOptions.None);
            List<Point> rowFinal = new List<Point>();
            if (row.Length > 1)
                for (int x = 0, y = 1; y < row.Length; x += 2, y += 2)
                    rowFinal.Add(new Point(int.Parse(row[x]), int.Parse(row[y])));
            
            SetLocation(imageN, rowFinal.ToArray());
        }
        public string roi_getAllInfo()
        {
            ROI roi = this;
            
            string val = "{";
            val += roi.Shape.ToString() + "\n";
            val += roi.Type.ToString() + "\n";
            val += roi.Width.ToString() + "\n";
            val += roi.Height.ToString() + "\n";
            val += roi.Stack.ToString() + "\n";
            val += roi.D.ToString() + "\n";
            val += roi.FromT.ToString() + "\n";
            val += roi.ToT.ToString() + "\n";
            val += roi.FromZ.ToString() + "\n";
            val += roi.ToZ.ToString() + "\n";

            val += BiggestW.ToString() + "\n";
            val += BiggestH.ToString() + "\n";
            val += ReturnBiggest.ToString() + "\n";
            val += turnOnStackRoi.ToString() + "\n";

            val += GetColors() + "\n";
            val += GetChartUseIndex() + "\n";
            val += "Comment=" + Comment + "\n";

            val += roi_getAllLocation();

            val += "}";
            return val;
        }
        private string GetColors()
        {
            List<string> vals = new List<string>();
                vals.Add("colors");
            if(colors != null)
                foreach(Color col in colors)
                {
                    vals.Add(ColorTranslator.ToHtml(col));
                }

            return string.Join("\t", vals);
        }
        private bool SetColors(string str)
        {
            string[] vals = str.Split(new string[] { "\t" }, StringSplitOptions.None);

            if (vals[0] != "colors") return false;

            colors = new Color[vals.Length - 1];

            for (int i = 1; i < vals.Length; i++)
                colors[i - 1] = ColorTranslator.FromHtml(vals[i]);

            return true;
        }
        private string GetChartUseIndex()
        {
            List<string> vals = new List<string>();
            vals.Add("ChartUseIndex");
            if (ChartUseIndex != null)
                foreach (bool val in ChartUseIndex)
                {
                    vals.Add(val.ToString());
                }

            return string.Join("\t", vals);
        }
        private bool SetChartUseIndex(string str)
        {
            string[] vals = str.Split(new string[] { "\t" }, StringSplitOptions.None);

            if (vals[0] != "ChartUseIndex") return false;

            ChartUseIndex = new bool[vals.Length - 1];

            for (int i = 1; i < vals.Length; i++)
                ChartUseIndex[i - 1] = bool.Parse(vals[i]);

            return true;
        }
        private string roi_getAllLocation()
        {
            ROI roi = this;
            Point[][] loc = roi.GetLocationAll();
            string[] vals = new string[loc.Length];
            //foreach (Point[] pList in loc)
            Parallel.For(0, loc.Length, i =>
            {
                Point[] pList = loc[i];
                if (pList != null)
                    foreach (Point p in pList)
                    {
                        vals[i] += p.X.ToString() + "\t" + p.Y.ToString() + "\t";
                    }
            });

            return string.Join("\n",vals);
        }
        public void CreateFromHistory(int roiID, string val)
        {
            val = val.Remove(val.Length - 1, 1).Remove(0, 1);
            string[] vals = val.Split(new string[] { "\n" }, StringSplitOptions.None);
            
            this.ID = roiID;
            shape = int.Parse(vals[0]);
            type = int.Parse(vals[1]);
            W = int.Parse(vals[2]);
            H = int.Parse(vals[3]);
            stack = int.Parse(vals[4]);
            d = int.Parse(vals[5]);
            FromT = int.Parse(vals[6]);
            ToT= int.Parse(vals[7]);
            FromZ = int.Parse(vals[8]);
            ToZ = int.Parse(vals[9]);
            BiggestW = int.Parse(vals[10]);
            BiggestH = int.Parse(vals[11]);
            ReturnBiggest = bool.Parse(vals[12]);
            turnOnStackRoi = bool.Parse(vals[13]);
            int i = 14;
           
            if (vals.Length > 15)
                try
                {
                    if (SetColors(vals[14])) i++;
                    if (SetChartUseIndex(vals[15])) i++;
                }
                catch { }

            if(vals.Length > 16)
            {
                if (vals[i].StartsWith("Comment="))
                {
                    Comment = vals[i].Replace("Comment=", "");
                    i++;
                }
            }

            setLocationFromHistory(vals, i);
        }
        private void setLocationFromHistory(string[] vals, int n)
        {
            Point[][] points = new Point[vals.Length - n][];
            for (int i = n, frame = 0; i < vals.Length; i++, frame++)
            {
                string[] row = vals[i].Split(new string[] { "\t" }, StringSplitOptions.None);
                List<Point> rowFinal = new List<Point>();
                if (row.Length > 1)
                    for (int x = 0, y = 1; y < row.Length; x += 2, y += 2)
                        rowFinal.Add(new Point(int.Parse(row[x]), int.Parse(row[y])));

                points[frame] = rowFinal.ToArray();
            }
            SetLocationAll(points);
        }
        #endregion History
    }
}
