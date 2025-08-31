using LPR_Project;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;

namespace LPR381Solver.Algorithms
{
    public class SimplexAdapter
    {
        public static LPR381Solver.Core.SimplexResult Solve(LPR381Solver.Core.LPModel model)
        {
            // Convert LPModel to LinearProgramming
            var lpModel = new LPR_Project.LinearProgramming
            {
                ObjectiveType = model.ObjectiveType,
                Coefficients = model.ObjectiveCoefficients,
                Constraints = model.Constraints.SelectMany(c => c.Coefficients).ToList(),
                Relations = model.Constraints.Select(c =>
                {
                    switch (c.Type)
                    {
                        case LPR381Solver.Core.ConstraintType.LessThanOrEqual:
                            return "<=";
                        case LPR381Solver.Core.ConstraintType.GreaterThanOrEqual:
                            return ">=";
                        case LPR381Solver.Core.ConstraintType.Equal:
                            return "=";
                        default:
                            return "";
                    }
                }).ToList(),
                RHS = model.Constraints.Select(c => c.RightHandSide).ToList()
            };

            // Call the PrimalSimplex solver
            var simplexResult = PrimalSimplex.Solve(lpModel);

            // Create your result object and populate it directly
            var result = new LPR381Solver.Core.SimplexResult();

            if (simplexResult.IsFeasible)
            {
                result.IsFeasible = true;
                result.ObjectiveValue = simplexResult.ObjectiveValue;
                result.Solution = simplexResult.Variables;
            }
            else
            {
                result.IsFeasible = false;
                result.ObjectiveValue = double.NaN;
                result.Solution = new double[model.NumVariables];
            }

            return result;
        }
    }
}