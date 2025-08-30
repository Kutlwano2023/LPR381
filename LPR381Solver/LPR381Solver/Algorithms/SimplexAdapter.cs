using LPR_Project;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

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

            // Call the original, unchanged PrimalSimplex solver
            // This line is the cause of the error. It needs to be corrected.
            // Old line: string resultString = LPR_Project.LinearProgramming.PrimalSimplex.Solve(lpModel); ❌
            // Corrected line:
            var primalSimplexResult = LPR_Project.LinearProgramming.PrimalSimplex.Solve(lpModel); 
            var result = new LPR381Solver.Core.SimplexResult();

            // The PrimalSimplex.Solve method returns a SimplexResult, not a string.
            // We now have the result as an object and can access its properties.

            // Check if the solution is an error
            if (!primalSimplexResult.IsFeasible || !primalSimplexResult.IsOptimal)
            {
                result.IsFeasible = false;
                result.ObjectiveValue = double.NaN;
                result.Solution = new double[model.NumVariables];
                // Handle case where PrimalSimplex returns an error
                return result;
            }

            // Map the properties from the PrimalSimplex's SimplexResult to the Core's SimplexResult
            result.ObjectiveValue = primalSimplexResult.ObjectiveValue;
            result.Solution = primalSimplexResult.Variables;
            result.IsFeasible = primalSimplexResult.IsFeasible;

            return result;
        }
    }
}