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
using Microsoft.SolverFoundation.Services;
using NCalc;

namespace Cell_Tool_3
{
    class ResultsExtractor_HalfTimeCalculator
    {
        private Microsoft.SolverFoundation.Solvers.NelderMeadSolver solver;
        private Microsoft.SolverFoundation.Solvers.NelderMeadSolverParams param;
        private Expression e;
        private double half;
        
        public double SolveHalfTime(Expression e, int iterations, double start, double stop,double half, double value)
        {
            try
            {
                this.e = e;
                this.half = half;
                //define constants
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

                solver.AddVariable("t", out constants[0]);
                solver.SetLowerBound(constants[0], start);
                solver.SetUpperBound(constants[0], stop);
                solver.SetValue(constants[0], value);

                // Assign objective function delegate.
                solver.FunctionEvaluator = FunctionValueHalfTime;

                // Solve.
                param.IterationLimit = iterations;

                var solution = solver.Solve(param);

                return solution.GetValue(constants[0]);
            }
            catch
            {
                return value;
            }
        }
        private double FunctionValueHalfTime(INonlinearModel model, int rowVid,
               ValuesByIndex values, bool newValues)
        {
            e.Parameters["t"] = values[model.GetIndexFromKey("t")];

            double val = 0;
            double.TryParse(e.Evaluate().ToString(), out val);

            return Math.Abs(half - val);
        }
    }
}
