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

            // Call the PrimalSimplex solver (returns string)
            // If you really need to work with a string representation
            var simplexResult = LPR_Project.LinearProgramming.PrimalSimplex.Solve(lpModel);
            string resultString = simplexResult.ToString();

            // Create your result object
            var result = new LPR381Solver.Core.SimplexResult();

            // Check string content (less reliable than checking properties)
            if (resultString.Contains("Error") || resultString.Contains("Unbounded"))
            {
                result.IsFeasible = false;
                result.OptimalValue = double.NaN;
                result.Solution = new double[model.NumVariables];
                return result;
            }
            // Check if the solution contains an error
            if (resultString.Contains("Error") || resultString.Contains("Unbounded"))
            {
                result.IsFeasible = false;
                result.ObjectiveValue = double.NaN;
                result.Solution = new double[model.NumVariables];
                return result;
            }

            // Parse the string output to extract values
            try
            {
                // Extract objective value
                var objectiveMatch = Regex.Match(resultString, @"Objective Value: ([-+]?[0-9]*\.?[0-9]+)");
                if (objectiveMatch.Success)
                {
                    result.ObjectiveValue = double.Parse(objectiveMatch.Groups[1].Value, CultureInfo.InvariantCulture);
                }

                // Extract variable values
                result.Solution = new double[model.NumVariables];
                var variableMatches = Regex.Matches(resultString, @"x(\d+) = ([-+]?[0-9]*\.?[0-9]+)");
                foreach (Match match in variableMatches)
                {
                    int index = int.Parse(match.Groups[1].Value) - 1;
                    if (index < model.NumVariables)
                    {
                        result.Solution[index] = double.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
                    }
                }

                // Check if optimal
                result.IsFeasible = !resultString.Contains("Not Optimal") && !resultString.Contains("WARNING");
            }
            catch
            {
                // If parsing fails, return an infeasible result
                result.IsFeasible = false;
                result.ObjectiveValue = double.NaN;
                result.Solution = new double[model.NumVariables];
            }

            return result;
        }
    }
}
