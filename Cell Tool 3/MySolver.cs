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
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using Microsoft.SolverFoundation.Services;

namespace Cell_Tool_3
{
   class MySolver
    {
        
        public class FitSettings
        {
            //formula
            private string formula1;
            private string formula2;
            private string formulaIf;
            //parameters
            private Dictionary<string, MySolver.Parameter> parameters;
            private double[] Xvals;
            private double[] Yvals;
            public double StDev = 0;           
            //
            public void Release()
            {
                formula1 = "";
                formula2 = "";
                formulaIf = "";
                parameters = null;
                Xvals = null;
                Yvals = null;
            }
            public FitSettings()
            {
                formula1 = "";
                formula2 = "";
                formulaIf = "";
                parameters = new Dictionary<string, MySolver.Parameter>();
                Xvals = null;
                Yvals = null;
            }
            public FitSettings(string formula1, string formula2, string formulaIf, 
                Dictionary<string, MySolver.Parameter> parameters,
                double[]Xvals,double[]Yvals)
            {
                this.formula1 = formula1;
                this.formula2 = formula2;
                this.formulaIf = formulaIf;
                this.parameters = parameters;
                this.Xvals = Xvals;
                this.Yvals = Yvals;
            }
            public string GetFormula1
            {
                get { return formula1; }
            }
            public string SetFormula1
            {
                set { formula1 = value; }
            }
            public string GetFormula2
            {
                get { return formula2; }
            }
            public string SetFormula2
            {
                set { formula2 = value; }
            }
            public string GetFormulaIF
            {
                get { return formulaIf; }
            }
            public string SetFormulaIF
            {
                set { formulaIf = value; }
            }
            public Dictionary<string, MySolver.Parameter> Parameters
            {
                get { return parameters; }
                set { parameters = value; }
            }
            public double[] XVals
            {
                get { return Xvals; }
                set { Xvals = value; }
            }
            public double[] YVals
            {
                get { return Yvals; }
                set { Yvals = value; }
            }
            public List<string> FormulasForNcalc()
            {
               List <string> l = new List<string>();
                //return if there is no subformulas
                if (this.formula1.IndexOf("{") == -1) return l;

                List<string> l1 = new List<string>();
                List<string> l2 = new List<string>();
                int start = 0;
                int count = 0;
                bool searching = false;
                //search furst formula for subforms
                for(int i = 0; i< this.formula1.Length; i++, count++)
                {
                    if(!searching && this.formula1[i] == '{')
                    {
                        start = i + 1;
                        count = 0;
                        searching = true;
                    }

                    if(searching && this.formula1[i] == '}')
                    {
                        count--;
                        searching = false;
                        l1.Add(this.formula1.Substring(start, count));
                    }
                }
                //search secound formula for subforms
                start = 0;
                count = 0;
                searching = false;
                for (int i = 0; i < this.formula2.Length; i++, count++)
                {
                    if (!searching && this.formula2[i] == '{')
                    {
                        start = i + 1;
                        count = 0;
                        searching = true;
                    }

                    if (searching && this.formula2[i] == '}')
                    {
                        count--;
                        searching = false;
                        l2.Add(this.formula2.Substring(start, count));
                    }
                }
                //Check are the subformulas count correct
                if(l1.Count!= l2.Count)
                {
                    System.Windows.Forms.MessageBox.Show("Error: different numbers of sub formulas!");
                    return l;
                }

                for(int i = 0; i< l1.Count; i++)
                {
                    string val = "";

                    string formulaIf = this.formulaIf;
                    string formula1 = l1[i] ;
                    string formula2 = l2[i];

                    if (formulaIf == "")
                    {
                        val = formula1;
                    }
                    else
                    {
                        val = "if (" + formulaIf + " , ("
                        + formula1 + ") , ("
                        + formula2 + "))";
                    }
                    /*
                    foreach (var kvp in Parameters)
                    {
                        Parameter p = kvp.Value;
                        val = val.Replace(p.Name, p.Value.ToString());
                    }*/
                    
                    val = val.Replace("--", "+");
                    val = val.Replace("+-", "-");
                    val = val.Replace("If", "if ");
                    val = val.Replace("[", "(");
                    val = val.Replace("]", ")");
                    val = val.Replace("(+", "(");
                    val = val.Replace("(-", "(0-");

                    l.Add(val);
                }

                return l;
            }
            public string FormulaForNcalc()
            {
                string val = "";

                string formulaIf = this.formulaIf;
                string formula1 = this.formula1.Replace("{", "").Replace("}", ""); ;
                string formula2 = this.formula2.Replace("{", "").Replace("}", ""); ;

                if(formulaIf == "")
                {
                    val = formula1;
                }
                else
                {
                    val = "if (" + formulaIf + " , ("
                    + formula1 + ") , ("
                    + formula2 + "))";
                }
                /*
                foreach (var kvp in Parameters)
                {
                    Parameter p = kvp.Value;
                    val = val.Replace(p.Name, p.Value.ToString());                   
                }
                */
                val = val.Replace("--", "+");
                val = val.Replace("+-", "-");
                val = val.Replace("If", "if ");
                val = val.Replace("[", "(");
                val = val.Replace("]", ")");
                val = val.Replace("(+", "(");
                val = val.Replace("(-", "(0-");
                return val;
            }

            public void SolverFit()
            {
                Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-GB");

                string val = "";

                string formulaIf = this.formulaIf;
                string formula1 = this.formula1.Replace("{", "").Replace("}", "");
                string formula2 = this.formula2.Replace("{", "").Replace("}", ""); 

                if (formulaIf == "")
                {
                    val = formula1;
                }
                else
                {
                    val = "If[" + formulaIf + ", "
                    + formula1 + ", "
                    + formula2 + "]";
                }
                 
                val = val.Replace("Pow", "Power");

                var newParameters = Solve(this.Xvals, this.Yvals, val, parameters);
                foreach(var par in newParameters)
                {
                    parameters[par.Key] = par.Value;
                }
            }
        }
        public class Parameter
        {
            private string _Name;
            private double _Value;
            private double _Min;
            private double _Max;
            private bool _Variable;

            public Parameter(string Name)
            {
                _Name = Name;
                _Value = 0;
                _Min = double.MinValue;
                _Max = double.MaxValue;
                _Variable = true;
            }
            public Parameter(string Name, double Value, double Min, double Max, bool Variable)
            {
                _Name = Name;
                _Value = Value;
                _Min = Min;
                _Max = Max;
                _Variable = Variable;
            }
            public string Name
            {
                get { return _Name; }
                set { _Name = value; }
            }
            public double Value
            {
                get { return _Value; }
                set { _Value = value; }
            }

            public double Min
            {
                get { return _Min; }
                set { _Min = value; }
            }
            public double Max
            {
                get { return _Max; }
                set { _Max = value; }
            }
            public bool Variable
            {
                get { return _Variable; }
                set { _Variable = value; }
            }
        }
        public static Dictionary<string, MySolver.Parameter> Solve(double[] Xvals, double[] Yvals, 
            string formula, Dictionary<string, MySolver.Parameter> coefficients)
        {
            if (formula.StartsWith("If[FRAP"))//Send to frapa model and return the result
            {
                return FRAPA_Model.AllModels.Solve(formula, Xvals, Yvals, coefficients);
            }

            // create solver model
            SolverContext solver = SolverContext.GetContext();
            solver.ClearModel();
            Model model = solver.CreateModel();
            
            foreach (var parameter in coefficients)
            {
                if (parameter.Value.Variable)//add variable
                {
                    //m.AddDecision(new Decision(Domain.IntegerRange(0, 100), "b"));
                    //Decision des = new Decision(Domain.Real, parameter.Key);
                    Decision des = new Decision(Domain.RealRange(parameter.Value.Min, parameter.Value.Max), parameter.Key);
                    des.SetInitialValue(parameter.Value.Value);
                    model.AddDecision(des);
                }
                else // add scalar
                {
                    Microsoft.SolverFoundation.Services.Parameter par= 
                        new Microsoft.SolverFoundation.Services.Parameter(
                            Domain.Real,parameter.Key);
                    //par.SetBinding(new double[] { parameter.Value.Value }, parameter.Key);
                    par.SetBinding((double)parameter.Value.Value);
                    model.AddParameters(par);                    
                }
            }
            
            //operators: https://msdn.microsoft.com/en-us/library/gg261757(v=vs.93).aspx
            string theTerm = "((" + formula.Replace("t", Xvals[0].ToString()).Replace("Sqr" + Xvals[0].ToString(),"Sqrt") + ") - " + Yvals[0].ToString() + ") ^ 2";

            for (int i = 1; i < Xvals.Length && i < Yvals.Length; i++)
                theTerm += " + ((" + formula.Replace("t", Xvals[i].ToString()).Replace("Sqr" + Xvals[i].ToString(), "Sqrt") + ") - " + Yvals[i].ToString() + ") ^ 2";

            //
            // define optimization type and give objective function SUM(e^2) to be minimized

            model.AddGoal("SumOfSquaredErrors", GoalKind.Minimize, theTerm);
            //
            // solve model and transfer results (optimized decision variables) from 
            // model into a dictionary object which will be returned for the caller
            Solution solution = solver.Solve();
            //return the result
            Dictionary<string, MySolver.Parameter> parameters = new Dictionary<string, MySolver.Parameter>();

            foreach (Decision parameter in model.Decisions)
            {
                MySolver.Parameter p;

                if(!coefficients.TryGetValue(parameter.Name, out p))
                    p = new Parameter(parameter.Name);

                parameters.Add(parameter.Name,new Parameter                
               (parameter.Name, parameter.ToDouble(),p.Min,p.Max,p.Variable));
            }

            return parameters;
        }

    }
}
