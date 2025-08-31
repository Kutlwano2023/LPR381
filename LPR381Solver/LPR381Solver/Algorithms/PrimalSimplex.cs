using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LPR_Project
{
    public class PrimalSimplex
    {
        public static SimplexResult Solve(LinearProgramming model)
        {
            try
            {
                // PHASE 1: Find a basic feasible solution
                double[,] phase1Tableau = ConvertToPhase1Tableau(model);
                var phase1Result = SolveSimplex(phase1Tableau);

                if (Math.Abs(phase1Result.ObjectiveValue) > 1e-9)
                {
                    return new SimplexResult { IsOptimal = false, IsFeasible = false, ErrorMessage = "No feasible solution found." };
                }

                // PHASE 2: Solve the original problem
                double[,] phase2Tableau = ConvertToPhase2Tableau(phase1Tableau, model);
                var finalResult = SolveSimplex(phase2Tableau);
                
                return ExtractSolution(finalResult.Tableau, model);
            }
            catch (Exception ex)
            {
                return new SimplexResult { IsOptimal = false, IsFeasible = false, ErrorMessage = ex.Message };
            }
        }

        private static double[,] ConvertToPhase1Tableau(LinearProgramming model)
        {
            int numVars = model.Coefficients.Count;
            int numConstraints = model.RHS.Count;
            int numSlack = model.Relations.Count(r => r == "<=");
            int numSurplus = model.Relations.Count(r => r == ">=");
            int numArtificial = model.Relations.Count(r => r == ">=" || r == "=");

            int totalVars = numVars + numSlack + numSurplus + numArtificial;
            double[,] tableau = new double[numConstraints + 1, totalVars + 1];

            int slackIndex = numVars;
            int surplusIndex = numVars + numSlack;
            int artificialIndex = numVars + numSlack + numSurplus;

            // Set up constraint rows
            for (int i = 0; i < numConstraints; i++)
            {
                for (int j = 0; j < numVars; j++)
                {
                    tableau[i + 1, j] = model.Constraints[i * numVars + j];
                }

                if (model.Relations[i] == "<=")
                {
                    tableau[i + 1, slackIndex++] = 1;
                }
                else if (model.Relations[i] == ">=")
                {
                    tableau[i + 1, surplusIndex++] = -1;
                    tableau[i + 1, artificialIndex++] = 1;
                }
                else // "="
                {
                    tableau[i + 1, artificialIndex++] = 1;
                }

                tableau[i + 1, totalVars] = model.RHS[i];
            }

            // Phase 1 objective row
            for (int i = 1; i <= numConstraints; i++)
            {
                if (model.Relations[i - 1] == ">=" || model.Relations[i - 1] == "=")
                {
                    for (int j = 0; j <= totalVars; j++)
                    {
                        tableau[0, j] -= tableau[i, j];
                    }
                }
            }
            return tableau;
        }

        private static double[,] ConvertToPhase2Tableau(double[,] phase1Tableau, LinearProgramming model)
        {
            int numVars = model.Coefficients.Count;
            int numConstraints = model.RHS.Count;
            int numSlack = model.Relations.Count(r => r == "<=");
            int numSurplus = model.Relations.Count(r => r == ">=");

            int phase2TotalVars = numVars + numSlack + numSurplus;
            double[,] phase2Tableau = new double[numConstraints + 1, phase2TotalVars + 1];

            // Copy constraint rows from Phase 1, excluding artificial variables
            for (int i = 1; i <= numConstraints; i++)
            {
                for (int j = 0; j < phase2TotalVars; j++)
                {
                    phase2Tableau[i, j] = phase1Tableau[i, j];
                }
                phase2Tableau[i, phase2Tableau.GetLength(1) - 1] = phase1Tableau[i, phase1Tableau.GetLength(1) - 1];
            }

            // Set up original objective row for Phase 2
            for (int j = 0; j < numVars; j++)
            {
                phase2Tableau[0, j] = model.ObjectiveType.ToLower() == "max" ? -model.Coefficients[j] : model.Coefficients[j];
            }

            // Ensure the objective row is in canonical form for basic variables
            for (int i = 1; i <= numConstraints; i++)
            {
                for (int j = 0; j < phase2TotalVars; j++)
                {
                    if (Math.Abs(phase2Tableau[i, j] - 1.0) < 1e-9)
                    {
                        if (phase2Tableau[0, j] != 0)
                        {
                            double factor = phase2Tableau[0, j];
                            for (int k = 0; k <= phase2TotalVars; k++)
                            {
                                phase2Tableau[0, k] -= factor * phase2Tableau[i, k];
                            }
                        }
                    }
                }
            }
            return phase2Tableau;
        }

        private static SimplexResult SolveSimplex(double[,] tableau)
        {
            int numConstraints = tableau.GetLength(0) - 1;
            int totalVars = tableau.GetLength(1) - 1;

            while (true)
            {
                int pivotCol = -1;
                double minVal = 0;
                for (int j = 0; j < totalVars; j++)
                {
                    if (tableau[0, j] < minVal)
                    {
                        minVal = tableau[0, j];
                        pivotCol = j;
                    }
                }

                if (pivotCol == -1) break; // Optimal solution found

                int pivotRow = -1;
                double minRatio = double.MaxValue;
                for (int i = 1; i <= numConstraints; i++)
                {
                    if (tableau[i, pivotCol] > 1e-9)
                    {
                        double ratio = tableau[i, totalVars] / tableau[i, pivotCol];
                        if (ratio < minRatio)
                        {
                            minRatio = ratio;
                            pivotRow = i;
                        }
                    }
                }

                if (pivotRow == -1) throw new Exception("Unbounded solution.");

                double pivotValue = tableau[pivotRow, pivotCol];
                for (int j = 0; j <= totalVars; j++)
                {
                    tableau[pivotRow, j] /= pivotValue;
                }

                for (int i = 0; i <= numConstraints; i++)
                {
                    if (i != pivotRow)
                    {
                        double factor = tableau[i, pivotCol];
                        for (int j = 0; j <= totalVars; j++)
                        {
                            tableau[i, j] -= factor * tableau[pivotRow, j];
                        }
                    }
                }
            }
            return new SimplexResult { Tableau = tableau, ObjectiveValue = tableau[0, totalVars] };
        }

        private static SimplexResult ExtractSolution(double[,] tableau, LinearProgramming model)
        {
            int numVars = model.Coefficients.Count;
            int numConstraints = model.RHS.Count;
            var result = new SimplexResult();

            // The objective value in the tableau for a maximization problem is negative. 
            // We take the absolute value to get the correct result.
            double finalObjectiveValue = tableau[0, tableau.GetLength(1) - 1];

            if (model.ObjectiveType.ToLower() == "max")
            {
                // Key change here: Use Math.Abs to guarantee a positive result.
                result.ObjectiveValue = Math.Abs(finalObjectiveValue);
            }
            else
            {
                result.ObjectiveValue = finalObjectiveValue;
            }

            result.Variables = new double[numVars];
            for (int j = 0; j < numVars; j++)
            {
                int basicRow = -1;
                bool isBasic = true;
                for (int i = 1; i <= numConstraints; i++)
                {
                    if (Math.Abs(tableau[i, j] - 1) < 1e-9)
                    {
                        if (basicRow != -1)
                        {
                            isBasic = false;
                            break;
                        }
                        basicRow = i;
                    }
                    else if (Math.Abs(tableau[i, j]) > 1e-9)
                    {
                        isBasic = false;
                        break;
                    }
                }

                if (isBasic && basicRow != -1)
                {
                    result.Variables[j] = tableau[basicRow, tableau.GetLength(1) - 1];
                }
                else
                {
                    result.Variables[j] = 0;
                }
            }

            result.IsOptimal = true;
            result.IsFeasible = true;
            return result;
        }

        public static string FormatResult(SimplexResult result, LinearProgramming model)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=============================================");
            sb.AppendLine("== Primal Simplex Results ==");
            sb.AppendLine("=============================================");

            if (result.IsFeasible)
            {
                sb.AppendLine($"Optimal Objective Value: {result.ObjectiveValue:F4}");
                sb.AppendLine("Variable values:");
                for (int i = 0; i < result.Variables.Length; i++)
                {
                    sb.AppendLine($"x{i + 1} = {result.Variables[i]:F4}");
                }
            }
            else
            {
                sb.AppendLine("Status: Infeasible");
                sb.AppendLine($"Error Message: {result.ErrorMessage}");
            }
            sb.AppendLine("=============================================");

            return sb.ToString();
        }
    }

    public class SimplexResult
    {
        public double ObjectiveValue { get; set; }
        public double[] Variables { get; set; }
        public bool IsOptimal { get; set; }
        public bool IsFeasible { get; set; }
        public string ErrorMessage { get; set; }
        public double[,] Tableau { get; set; }
    }
}