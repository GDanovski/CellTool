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
using NCalc;

namespace Cell_Tool_3
{
    class Ncalc_Adapter
    {
        ImageAnalyser IA;
        Expression e;
        List<string[]> parameters = new List<string[]>();
        public Ncalc_Adapter(ImageAnalyser IA)
        {
            this.IA = IA;
        }
        public void LoadFunction(TifFileInfo fi, int Function = -1)
        {
            int ind = fi.yAxisTB - 5;
            if (Function != -1) ind = Function;

            string[] funct = IA.chart.Properties.functions[ind].Split(new string[] { "=" },StringSplitOptions.None);
           
            funct[1] = funct[1].Replace(";", "").Replace("][", "_").Replace("]", "_").Replace("[", "_");
            
            funct[1] = findParameters(funct[1]);
            
            e = new Expression(funct[1]);
        }
        private string findParameters(string f)
        {
            parameters.Clear();
            
            int ind = 0;

            string val = "Mean";

            int begin = f.IndexOf(val);
            int end = begin;
            while (begin > -1)
            {
                string change = f.Substring(begin, FindEnd(f, begin) - begin);
                parameters.Add(change.Split(new string[] { "_" }, StringSplitOptions.None));
                parameters[parameters.Count - 1][0] = "1";
                parameters[parameters.Count - 1][4] = "f" + ind.ToString();

                f =f.Replace(change, "[f" + ind.ToString() + "]");
                
                begin = f.IndexOf(val);
                ind++;
            }

            val = "Area";

            begin = f.IndexOf(val);
            end = begin;
            while (begin > -1)
            {
                string change = f.Substring(begin, FindEnd(f, begin) - begin);
                parameters.Add(change.Split(new string[] { "_" }, StringSplitOptions.None));
                parameters[parameters.Count - 1][0] = "0";
                parameters[parameters.Count - 1][4] = "f" + ind.ToString();

                f = f.Replace(change, "[f" + ind.ToString() + "]");
                
                begin = f.IndexOf(val);
                ind++;
            }

            val = "Min";

            begin = f.IndexOf(val);
            end = begin;
            while (begin > -1)
            {
                string change = f.Substring(begin, FindEnd(f, begin) - begin);
                parameters.Add(change.Split(new string[] { "_" }, StringSplitOptions.None));
                parameters[parameters.Count - 1][0] = "2";
                parameters[parameters.Count - 1][4] = "f" + ind.ToString();

                f = f.Replace(change, "[f" + ind.ToString() + "]");
                
                begin = f.IndexOf(val);
                ind++;
            }

            val = "Max";

            begin = f.IndexOf(val);
            end = begin;
            while (begin > -1)
            {
                string change = f.Substring(begin, FindEnd(f, begin) - begin);
                parameters.Add(change.Split(new string[] { "_" }, StringSplitOptions.None));

                parameters[parameters.Count - 1][0] = "3";
                parameters[parameters.Count - 1][4] = "f" + ind.ToString();

                f = f.Replace(change, "[f" + ind.ToString() + "]");
                
                begin = f.IndexOf(val);
                ind++;
            }
            
            return f;
        }
        private int FindEnd(string f, int begin)
        {
            int count = 0;
            while(count < 4)
            {
                if (f.Substring(begin,1) == "_") count++;
                begin++;
            }
            return begin;
        }
        public string ErrorCheck(int c, ROI roi, List<ROI> roiList, TifFileInfo fi, int position = 0)
        {
            {
                #region parameters
                ROI curRoi;
                int curPosition, curRow;

                for (int i = 0; i < parameters.Count; i++)
                {
                    if (parameters[i][1] != "")
                    {
                        int ind = int.Parse(parameters[i][1]);
                        if (!(roiList.Count > ind))
                        {
                            return "There is no roi " + (ind + 1).ToString();
                        }
                        curRoi = roiList[ind];
                    }
                    else
                        curRoi = roi;

                    if (parameters[i][3] != "")
                    {
                        curRow = int.Parse(parameters[i][3]) * fi.sizeC + c;
                        if (curRoi.Results == null || !(curRoi.Results.Length > curRow))
                        {
                            return "Row " + curRow.ToString()
                                + "is not avaliable in roi:\n" + curRoi.Text;
                        }
                    }
                    else
                        curRow = 0;

                    if (parameters[i][2] != "" && parameters[i][2] != "0")
                    {
                        curPosition = int.Parse(parameters[i][2]) * 4;
                        int boolInd = 1 + (int.Parse(parameters[i][2]) - 1) * 4;
                        
                        if (curRoi.Shape == 0 || curRoi.Shape == 1)
                            for (int i1 = curPosition + int.Parse(parameters[i][0]); i1 < curPosition + 16; i1 += 4)
                            {
                                if (!(curRoi.ChartUseIndex.Length > boolInd))
                                {
                                    return "Missing Layer: " + curRoi.Text;
                                }
                                boolInd++;
                            }
                        
                    }
                    else if (parameters[i][2] != "" && parameters[i][2] == "0")
                    {
                        curPosition = int.Parse(parameters[i][0]);
                        if (!(curPosition < curRoi.Results[curRow].Length))
                        {
                            return "";
                        }
                    }
                    else
                    {
                        curPosition = position + int.Parse(parameters[i][0]);
                        if (!(curPosition < curRoi.Results[curRow].Length))
                        {
                            return "";
                        }
                    }

                }
                #endregion parameters
                return "";
            }

        }
        public double Calculate(int c, int row, ROI roi, List<ROI> roiList, TifFileInfo fi, int position = 0)
        {
            {
                #region parameters
                ROI curRoi;
                int curPosition, curRow;
                
                for (int i = 0; i < parameters.Count; i++)
                {
                    if (parameters[i][1] != "")
                    {
                        int ind = int.Parse(parameters[i][1]);
                        if (!(roiList.Count > ind)) return 0;
                        curRoi = roiList[ind];
                    }
                    else
                        curRoi = roi;

                    if (parameters[i][3] != "")
                    {
                       curRow = int.Parse(parameters[i][3]) * fi.sizeC + c;
                        if (curRoi.Results == null || !(curRoi.Results.Length > curRow)) return 0;
                    }
                    else
                        curRow = row;

                    if (parameters[i][2] != "" && parameters[i][2] != "0")
                    {
                        curPosition = int.Parse(parameters[i][2]) * 4;
                        int boolInd = 1 + (int.Parse(parameters[i][2]) - 1) * 4;

                        double val1 = 0;
                        int fact = 0;

                        if (curRoi.Shape == 0 || curRoi.Shape == 1)
                            for (int i1 = curPosition + int.Parse(parameters[i][0]); i1 < curPosition + 16; i1 += 4)
                            {
                                if (curRoi.ChartUseIndex.Length > boolInd && curRoi.ChartUseIndex[boolInd] == true)
                                {
                                    if (i1 < curRoi.Results[curRow].Length)
                                    {
                                        if (parameters[i][0] == "1" || parameters[i][0] == "0")
                                            val1 += curRoi.Results[curRow][i1];
                                        else if (parameters[i][0] == "3" && val1 < curRoi.Results[curRow][i1])
                                            val1 = curRoi.Results[curRow][i1];
                                        else if (parameters[i][0] == "2" && (val1 == 0 ||
                                            val1 > curRoi.Results[curRow][i1]))
                                            val1 = curRoi.Results[curRow][i1];
                                    }
                                    fact++;
                                }
                                if (!(curRoi.ChartUseIndex.Length > boolInd)) return 0;
                                boolInd++;
                            }

                        if (fact > 0 && parameters[i][0] == "1")
                            val1 /= fact;

                        e.Parameters[parameters[i][4]] = val1;
                    }
                    else if (parameters[i][2] != "" && parameters[i][2] == "0")
                    {
                        curPosition = int.Parse(parameters[i][0]);
                        if (curPosition < curRoi.Results[curRow].Length)
                        {
                            e.Parameters[parameters[i][4]] = curRoi.Results[curRow][curPosition];
                        }
                        else
                        {
                           return 0;
                        }
                    }
                    else
                    {
                        curPosition = position + int.Parse(parameters[i][0]);
                        if (curPosition < curRoi.Results[curRow].Length)
                        {
                            e.Parameters[parameters[i][4]] = curRoi.Results[curRow][curPosition];
                        }
                        else
                        {
                            return 0;
                        }
                    }

                }
                #endregion parameters

                double val = 0;
                //double.TryParse((new Expression("10.01+10").Evaluate()).ToString(),out val);
                double.TryParse((e.Evaluate()).ToString(), out val);
                return val;
            }
            
        }
    }
}
