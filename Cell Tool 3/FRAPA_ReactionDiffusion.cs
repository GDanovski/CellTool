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
using Microsoft.SolverFoundation.Services;

namespace Cell_Tool_3
{
    class FRAPA_ReactionDiffusion
    {
        public class DataSet
        {
            public string XTitle;
            public string YTitle;
            public double[] XVals;
            public double[] YVals;
            public double[] FitYVals;
            public List<Variable> Variables = new List<Variable>();
            public DataSet()
            {
                this.XTitle = "";
                this.YTitle = "";
                this.XVals = null;
                this.YVals = null;
                this.FitYVals = null;
            }
            public DataSet(string Xname, string Yname, double[] Xvals, double[] Yvals)
            {
                this.XTitle = Xname;
                this.YTitle = Yname;
                this.XVals = Xvals;
                this.YVals = Yvals;
                this.FitYVals = new double[this.YVals.Length];
            }
        }
        public class Variable
        {
            #region Variables
            private string _Name;
            private double _Value;
            private double _Min;
            private double _Max;
            #endregion Variables

            #region Properties
            public string ConstName
            {
                get { return this._Name; }
                set
                {
                    this._Name = value;
                    //RefreshValues();
                }
            }
            public double ConstValue
            {
                get { return this._Value; }
                set
                {
                    this._Value = value;
                    //RefreshValues();
                }
            }
            public double ConstMin
            {
                get { return this._Min; }
                set
                {
                    this._Min = value;
                    //RefreshValues();
                }
            }
            public double ConstMax
            {
                get { return this._Max; }
                set
                {
                    this._Max = value;
                    //RefreshValues();
                }
            }
            #endregion Properties

        }
        public class MySolver
        {
            private DataSet data;
            public Microsoft.SolverFoundation.Solvers.NelderMeadSolver solver;
            public Microsoft.SolverFoundation.Solvers.NelderMeadSolverParams param;
            public void Solve(DataSet data, int iterations)
            {
                //define constants

                this.data = data;
                this.param = new Microsoft.SolverFoundation.Solvers.NelderMeadSolverParams();
                int[] constants;
                //Solver
                solver = new Microsoft.SolverFoundation.Solvers.NelderMeadSolver();

                // Objective function.
                int objId;
                solver.AddRow("obj", out objId);
                solver.AddGoal(objId, 0, true);

                // Define variables.
                constants = new int[data.Variables.Count];
                for (int i = 0; i < data.Variables.Count; i++)
                {
                    solver.AddVariable(data.Variables[i].ConstName, out constants[i]);
                    solver.SetLowerBound(constants[i], data.Variables[i].ConstMin);
                    solver.SetUpperBound(constants[i], data.Variables[i].ConstMax);
                    solver.SetValue(constants[i], data.Variables[i].ConstValue);
                }

                // Assign objective function delegate.
                solver.FunctionEvaluator = FunctionValue;

                // Solve.
                param.IterationLimit = iterations;

                var solution = solver.Solve(param);

                for (int i = 0; i < data.Variables.Count; i++)
                {
                    data.Variables[i].ConstValue = solution.GetValue(constants[i]);
                }
                //find half time
               SolveHalfTime(data, iterations);
            }
           
            private double FunctionValue(INonlinearModel model, int rowVid,
                    ValuesByIndex values, bool newValues)
            {
                foreach (Variable c in data.Variables)
                {
                    c.ConstValue = values[model.GetIndexFromKey(c.ConstName)];
                }

                double dev = FindDeviation(data);
                
                return dev;
            }
            private double FindDeviation(DataSet data)
            {

                double Sum = 0;
                double[] YVals = data.YVals;

                SolveEq(data);

                double[] fitYVals = data.FitYVals;

                long counter = 0;


                for (int j = 0; j < YVals.Length; j++)
                {
                    Sum += Math.Pow((fitYVals[j] - YVals[j]), 2);
                    counter++;
                }

                return Math.Sqrt(Sum / (counter - 1));
            }

            private static void SolveEq(DataSet data)
            {
                if (data.XVals.Length == 0 || data.Variables.Count == 0) return;

                double R = data.Variables[0].ConstValue;
                double w = data.Variables[1].ConstValue;
                double Kon = data.Variables[2].ConstValue;
                double Koff = data.Variables[3].ConstValue;
                double Df = data.Variables[4].ConstValue;
                double Ceq = data.Variables[5].ConstValue;
                double Feq = 1 - Ceq;

                data.FitYVals = new double[data.XVals.Length];
                
                Laplace lap = new Laplace();
                lap.InitStehfest(14);

                for (int t = 1; t < data.XVals.Length; t++)
                {
                    //data.FitYVals[0][t] = MathNet.Numerics.SpecialFunctions.BesselK1(data.XVals[t]);
                    //data.YVals[0][t] = MathNet.Numerics.SpecialFunctions.BesselI1(data.XVals[t]);
                    //data.FitYVals[0][t] = Accord.Math.Bessel.I(data.XVals[t]);

                    //data.FitYVals[0][t] = Accord.Math.Bessel.Y(data.XVals[t]); - it is not modified!!! 
                    data.FitYVals[t] = (R) * lap.InverseTransform(R, w, Feq, Ceq, Kon, Koff, Df, data.XVals[t]);
                }
            }

            #region HalfTime
            public void SolveHalfTime(DataSet data, int iterations)
            {
                //define constants

                this.data = data;
                this.param = new Microsoft.SolverFoundation.Solvers.NelderMeadSolverParams();
                int[] constants;
                //Solver
                solver = new Microsoft.SolverFoundation.Solvers.NelderMeadSolver();

                // Objective function.
                int objId;
                solver.AddRow("obj", out objId);
                solver.AddGoal(objId, 0, true);

                // Define variables.
                constants = new int[1];
                for (int i = 0; i < data.Variables.Count; i++)
                {
                    if (data.Variables[i].ConstName == "T1/2")
                    {
                        solver.AddVariable(data.Variables[i].ConstName, out constants[0]);
                        solver.SetLowerBound(constants[0], data.XVals[0]);
                        solver.SetUpperBound(constants[0], data.XVals[data.XVals.Length - 1]);
                        solver.SetValue(constants[0], 0);
                        break;
                    }
                }

                // Assign objective function delegate.
                solver.FunctionEvaluator = FunctionValueHalfTime;

                // Solve.
                param.IterationLimit = iterations;

                var solution = solver.Solve(param);

                for (int i = 0; i < data.Variables.Count; i++)
                    if (data.Variables[i].ConstName == "T1/2")
                    {
                        data.Variables[i].ConstValue = solution.GetValue(constants[0]);
                        break;
                    }

            }
            private double FunctionValueHalfTime(INonlinearModel model, int rowVid,
                   ValuesByIndex values, bool newValues)
            {
                foreach (Variable c in data.Variables)
                    if (c.ConstName == "T1/2")
                    {
                        c.ConstValue = values[model.GetIndexFromKey(c.ConstName)];
                        break;
                    }

                double R = data.Variables[0].ConstValue;
                double w = data.Variables[1].ConstValue;
                double Kon = data.Variables[2].ConstValue;
                double Koff = data.Variables[3].ConstValue;
                double Df = data.Variables[4].ConstValue;
                double Ceq = data.Variables[5].ConstValue;
                double Feq = 1 - Ceq;
                double Tht = data.Variables[6].ConstValue;

                data.FitYVals = new double[data.XVals.Length];

                Laplace lap = new Laplace();
                lap.InitStehfest(14);

                return Math.Abs((R) * lap.InverseTransform(R, w, Feq, Ceq, Kon, Koff, Df, Tht) - 0.5 * R);
            }
            #endregion HalfTime
            class Laplace
            {
                //int DefaultStehfest = 14;

                double[] V; // Stehfest coefficients 
                double ln2; // log of 2

                public void InitStehfest(int N)
                {
                    ln2 = Math.Log(2.0);
                    int N2 = N / 2;
                    int NV = 2 * N2;
                    V = new double[NV];
                    int sign = 1;
                    if ((N2 % 2) != 0)
                        sign = -1;
                    for (int i = 0; i < NV; i++)
                    {
                        int kmin = (i + 2) / 2;
                        int kmax = i + 1;
                        if (kmax > N2)
                            kmax = N2;
                        V[i] = 0;
                        sign = -sign;
                        for (int k = kmin; k <= kmax; k++)
                        {
                            V[i] = V[i] + (Math.Pow(k, N2) / Factorial(k)) * (Factorial(2 * k)
                                 / Factorial(2 * k - i - 1)) / Factorial(N2 - k)
                                 / Factorial(k - 1) / Factorial(i + 1 - k);
                        }
                        V[i] = sign * V[i];
                    }

                }

                public double InverseTransform(double R, double w, double Feq,
                    double Ceq, double Kon, double Koff, double Df, double t)
                {
                    double ln2t = ln2 / t;
                    double p = 0;
                    double y = 0;
                    double q = 0;

                    for (int i = 0; i < V.Length; i++)
                    {
                        p += ln2t;
                        q = Math.Sqrt((p / Df) * (1 + Kon / (p + Koff)));

                        y += V[i] * (
                            (1 / p) - (Feq / p) * (1 - 2 * MathNet.Numerics.SpecialFunctions.BesselK1(q * w) * MathNet.Numerics.SpecialFunctions.BesselI1(q * w)) *
                            (1 + Kon / (p + Koff)) - Ceq / (p + Koff)
                            );
                    }
                    return ln2t * y;
                }

                public double Factorial(int N)
                {
                    double x = 1;
                    if (N > 1)
                    {
                        for (int i = 2; i <= N; i++)
                            x = i * x;
                    }
                    return x;
                }
            }
        }
    }
}
