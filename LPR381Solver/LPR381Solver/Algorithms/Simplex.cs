using System;
using System.Collections.Generic;
using System.IO; // Added this for File.ReadAllLines()
using System.Linq;

namespace LPR_Project
{
    public class LinearProgramming
    {
        // Public properties to define the LP model
        public string ObjectiveType { get; set; } //max or min
        public List<double> Coefficients { get; set; } = new List<double>();
        public List<double> Constraints { get; set; } = new List<double>();
        public List<string> Relations { get; set; } = new List<string>();
        public List<double> RHS { get; set; } = new List<double>();
        public List<string> SignRestictions { get; set; } = new List<string>();

        public static LinearProgramming Parse(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                throw new FileNotFoundException("File not found.");

            var model = new LinearProgramming();
            var lines = File.ReadAllLines(path);

            foreach (var line in lines)
            {
                if (line.StartsWith("Objective:"))
                    ParseObjective(line, model);
                else if (line.StartsWith("Constraint:"))
                    ParseConstraint(line, model);
            }

            return model;
        }

        private static void ParseObjective(string line, LinearProgramming model)
        {
            var parts = line.Split(':')[1].Split(' ');
            model.ObjectiveType = parts[0].ToLower();
            model.Coefficients = parts.Skip(1).Select(double.Parse).ToList();
        }

        private static void ParseConstraint(string line, LinearProgramming model)
        {
            var parts = line.Split(':')[1].Split(' ');
            var coefficients = parts.Take(parts.Length - 2).Select(double.Parse).ToList();
            var relation = parts[parts.Length - 2];
            var rhs = double.Parse(parts[parts.Length - 1]);

            model.Constraints.AddRange(coefficients);
            model.Relations.Add(relation);
            model.RHS.Add(rhs);
       } 


        // Primal simplex solver model
        public class PrimalSimplex
        {
            public static SimplexResult Solve(LinearProgramming model)
            {
                try
                {
                    // Convert to standard form
                    var tableau = ConvertToTableau(model);

                    // Solve using simplex
                    var result = SolveSimplex(tableau, model);

                    return result;
                }
                catch (Exception ex)
                {
                    return new SimplexResult { IsOptimal = false, IsFeasible = false, ErrorMessage = ex.Message };
                }
            }

            private static double[,] ConvertToTableau(LinearProgramming model)
            {
                int numVars = model.Coefficients.Count;
                int numConstraints = model.RHS.Count;
                int totalVars = numVars + numConstraints;

                double[,] tableau = new double[numConstraints + 1, totalVars + 1];

                // Objective row
                for (int j = 0; j < numVars; j++)
                {
                    tableau[0, j] = model.ObjectiveType == "max" ? -model.Coefficients[j] : model.Coefficients[j];
                }

                // Constraint rows
                for (int i = 0; i < numConstraints; i++)
                {
                    for (int j = 0; j < numVars; j++)
                    {
                        tableau[i + 1, j] = model.Constraints[i * numVars + j];
                    }

                    // Add slack variables
                    tableau[i + 1, numVars + i] = model.Relations[i] == "<=" ? 1 : -1;
                    tableau[i + 1, totalVars] = model.RHS[i];
                }

                return tableau;
            }

            private static SimplexResult SolveSimplex(double[,] tableau, LinearProgramming model)
            {
                int numConstraints = model.RHS.Count;
                int numVars = model.Coefficients.Count;
                int totalVars = numVars + numConstraints;

                while (true)
                {
                    // Find entering variable (most negative in objective row)
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

                    // Find leaving variable
                    int pivotRow = -1;
                    double minRatio = double.MaxValue;
                    for (int i = 1; i <= numConstraints; i++)
                    {
                        if (tableau[i, pivotCol] > 0)
                        {
                            double ratio = tableau[i, totalVars] / tableau[i, pivotCol];
                            if (ratio < minRatio)
                            {
                                minRatio = ratio;
                                pivotRow = i;
                            }
                        }
                    }

                    if (pivotRow == -1) throw new Exception("Unbounded solution");

                    // Pivot
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

                return ExtractSolution(tableau, model);
            }

            private static SimplexResult ExtractSolution(double[,] tableau, LinearProgramming model)
            {
                int numVars = model.Coefficients.Count;
                int numConstraints = model.RHS.Count;
                var result = new SimplexResult();

                result.ObjectiveValue = model.ObjectiveType == "max" ? -tableau[0, numVars + numConstraints] : tableau[0, numVars + numConstraints];
                result.Variables = new double[numVars];

                for (int j = 0; j < numVars; j++)
                {
                    // Check if this variable is basic
                    int basicRow = -1;
                    for (int i = 1; i <= numConstraints; i++)
                    {
                        if (tableau[i, j] == 1)
                        {
                            if (basicRow == -1) basicRow = i;
                            else { basicRow = -1; break; } // Not basic if multiple 1's
                        }
                        else if (tableau[i, j] != 0)
                        {
                            basicRow = -1;
                            break;
                        }
                    }

                    if (basicRow != -1)
                    {
                        result.Variables[j] = tableau[basicRow, numVars + numConstraints];
                    }
                }

                result.IsOptimal = true;
                result.IsFeasible = true;
                return result;
            }

            private static string FormatResult(SimplexResult result, LinearProgramming model)
            {
                var sb = new System.Text.StringBuilder();
                sb.AppendLine("=== PRIMAL SIMPLEX SOLUTION ===");
                sb.AppendLine($"Objective Value: {result.ObjectiveValue:F4}");

                sb.AppendLine("Variable Values:");
                for (int i = 0; i < result.Variables.Length; i++)
                {
                    sb.AppendLine($"x{i + 1} = {result.Variables[i]:F4}");
                }

                sb.AppendLine($"Status: {(result.IsOptimal ? "Optimal" : "Not Optimal")}");
                return sb.ToString();
            }
        }

        public class RevisedSimplex
        {
            public static string Solve(LinearProgramming model)
            {
                return "Revised Simplex implementation would go here";
            }
        }
    }

    public class SimplexResult
    {
        public double ObjectiveValue { get; set; }
        public double[] Variables { get; set; }
        public bool IsOptimal { get; set; }
        public bool IsFeasible { get; set; }
        public string ErrorMessage { get; set; } 
    }

        }
    


