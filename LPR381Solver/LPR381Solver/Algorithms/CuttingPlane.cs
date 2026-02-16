using LPR381Solver.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace LPR381Solver.Algorithms
{
    public class CuttingPlane
    {
        private readonly LPModel _initialModel;
        private double[] _bestIntegerSolution;
        private double _bestObjectiveValue;
        private readonly StringBuilder _outputBuilder;
        private const double EPSILON = 0.0001; // Tolerance for detecting fractional values

        public CuttingPlane(LPModel model)
        {
            _initialModel = model;
            _outputBuilder = new StringBuilder();
            _bestObjectiveValue = double.MinValue; // For maximization problems
        }

        public string Solve()
        {
            _outputBuilder.AppendLine("Starting Cutting Plane Algorithm...");

            LPModel currentModel = _initialModel.Clone();
            int iteration = 0;

            while (true)
            {
                iteration++;
                _outputBuilder.AppendLine($"\nIteration {iteration}:");

                // Solve the current LP relaxation using Simplex
                var solver = new Simplex(currentModel);
                var result = solver.Solve();

                if (!result.IsFeasible)
                {
                    _outputBuilder.AppendLine("  LP is infeasible. Terminating.");
                    break;
                }

                double[] solution = result.Solution;
                double objectiveValue = result.ObjectiveValue;

                _outputBuilder.AppendLine($"  LP Relaxation Solution: Objective = {objectiveValue:F3}");
                _outputBuilder.AppendLine($"  Solution values: {string.Join(", ", solution.Select(val => val.ToString("F3")))}");

                // Check if the solution is integer
                int fractionalVarIndex = FindFirstFractionalVariable(solution);
                if (fractionalVarIndex == -1)
                {
                    // All variables are integer
                    _bestIntegerSolution = solution;
                    _bestObjectiveValue = objectiveValue;
                    _outputBuilder.AppendLine("  All variables are integer. Optimal integer solution found!");
                    break;
                }

                // Update best integer solution if applicable
                if (objectiveValue > _bestObjectiveValue)
                {
                    _bestObjectiveValue = objectiveValue;
                    _outputBuilder.AppendLine($"  Updated best objective value: {_bestObjectiveValue:F3}");
                }

                // Generate a Gomory cut
                if (!GenerateGomoryCut(currentModel, solver.GetTableau(), fractionalVarIndex))
                {
                    _outputBuilder.AppendLine("  Failed to generate a valid Gomory cut. Terminating.");
                    break;
                }
            }

            // Output the final result
            if (_bestIntegerSolution != null)
            {
                _outputBuilder.AppendLine("\nOptimal Integer Solution Found!");
                _outputBuilder.AppendLine($"Objective Value: {_bestObjectiveValue:F3}");
                _outputBuilder.AppendLine("Variable values:");
                for (int i = 0; i < _bestIntegerSolution.Length; i++)
                {
                    _outputBuilder.AppendLine($"x{i + 1} = {_bestIntegerSolution[i]:F3}");
                }
            }
            else
            {
                _outputBuilder.AppendLine("\nNo integer solution was found.");
            }

            return _outputBuilder.ToString();
        }

        private int FindFirstFractionalVariable(double[] solution)
        {
            for (int i = 0; i < solution.Length; i++)
            {
                if (Math.Abs(solution[i] - Math.Round(solution[i])) > EPSILON)
                {
                    return i;
                }
            }
            return -1;
        }

        private bool GenerateGomoryCut(LPModel model, double[,] tableau, int fractionalVarIndex)
        {
            _outputBuilder.AppendLine($"  Generating Gomory cut for variable x{fractionalVarIndex + 1}...");

            // Find the row in the tableau corresponding to the fractional variable
            int rowIndex = -1;
            for (int i = 0; i < tableau.GetLength(0); i++)
            {
                if (Math.Abs(tableau[i, fractionalVarIndex] - 1.0) < EPSILON &&
                    Enumerable.Range(0, tableau.GetLength(1))
                              .Where(j => j != fractionalVarIndex)
                              .All(j => Math.Abs(tableau[i, j]) < EPSILON))
                {
                    rowIndex = i;
                    break;
                }
            }

            if (rowIndex == -1)
            {
                _outputBuilder.AppendLine("  Error: Could not find the tableau row for the fractional variable.");
                return false;
            }

            // Get the fractional part of the right-hand side
            double rhs = tableau[rowIndex, tableau.GetLength(1) - 1];
            double fractionalPart = rhs - Math.Floor(rhs);

            if (fractionalPart < EPSILON || fractionalPart > 1 - EPSILON)
            {
                _outputBuilder.AppendLine("  Fractional part is too small or too large to generate a valid cut.");
                return false;
            }

            // Create the Gomory cut: sum(a_ij * x_j) >= fractionalPart for basic variables
            double[] cutCoefficients = new double[model.NumVariables];
            for (int j = 0; j < model.NumVariables; j++)
            {
                double coef = tableau[rowIndex, j];
                cutCoefficients[j] = -(coef - Math.Floor(coef)); // Negative of fractional part
            }

            // Add the cut as a new constraint
            var cutConstraint = new Constraint(cutCoefficients, ConstraintType.GreaterThanOrEqual, -fractionalPart, model.NumVariables);
            model.AddConstraint(cutConstraint);

            _outputBuilder.AppendLine($"  Added Gomory cut: {string.Join(" + ", cutCoefficients.Select((c, i) => $"{c:F3}x{i + 1}"))} >= {-fractionalPart:F3}");
            return true;
        }
    }
}