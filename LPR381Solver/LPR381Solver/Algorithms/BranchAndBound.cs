using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LPR381Solver.Core;
using LPR381Solver.Algorithms;

namespace LPR381Solver.Algorithms
{
    public class BranchAndBoundSimplex
    {
        private class Node
        {
            public LPModel Model { get; }
            public double ObjectiveValue { get; set; }
            public double[] Solution { get; set; }

            public Node(LPModel model)
            {
                Model = model;
            }
        }

        private double bestObjectiveValue;
        private double[] bestIntegerSolution;
        private readonly StringBuilder outputBuilder = new StringBuilder();

        public string Solve(LPModel initialModel)
        {
            outputBuilder.AppendLine("Starting Branch and Bound Simplex Algorithm...");

            // Initialize best solution value based on objective type
            bestObjectiveValue = initialModel.ObjectiveType == "max" ? double.MinValue : double.MaxValue;
            bestIntegerSolution = null;

            // Use a priority queue for a best-first search
            var nodeQueue = new PriorityQueue<Node, double>();
            var initialNode = new Node(initialModel);
            nodeQueue.Enqueue(initialNode, GetPriority(initialNode, initialModel.ObjectiveType));

            while (nodeQueue.Count > 0)
            {
                var currentNode = nodeQueue.Dequeue();

                outputBuilder.AppendLine($"\n--- Exploring Node ---");

                // Check for infeasibility
                var result = SimplexAdapter.Solve(currentNode.Model);

                // Fathoming by infeasibility
                if (!result.IsFeasible)
                {
                    outputBuilder.AppendLine("  Fathomed: LP is infeasible.");
                    continue;
                }

                currentNode.ObjectiveValue = result.ObjectiveValue;
                currentNode.Solution = result.Solution;
                
                // Fathoming by bounding
                if ((initialModel.ObjectiveType == "max" && currentNode.ObjectiveValue <= bestObjectiveValue) ||
                    (initialModel.ObjectiveType == "min" && currentNode.ObjectiveValue >= bestObjectiveValue))
                {
                    outputBuilder.AppendLine($"  Fathomed: Objective value ({currentNode.ObjectiveValue:F3}) is not better than the current best integer solution ({bestObjectiveValue:F3}).");
                    continue;
                }

                // Check if solution is all integers
                int fractionalVarIndex = FindBestFractionalVariable(currentNode.Solution);
                if (fractionalVarIndex == -1)
                {
                    // Fathoming by integrality: found a new incumbent solution
                    bestObjectiveValue = currentNode.ObjectiveValue;
                    bestIntegerSolution = currentNode.Solution;
                    outputBuilder.AppendLine($"  Fathomed: Found a new integer solution with value {bestObjectiveValue:F3}.");
                    continue;
                }

                // Branching step
                double fractionalValue = currentNode.Solution[fractionalVarIndex];
                int floorValue = (int)Math.Floor(fractionalValue);
                int ceilValue = (int)Math.Ceiling(fractionalValue);

                outputBuilder.AppendLine($"  Branching on variable x{fractionalVarIndex + 1} with fractional value {fractionalValue:F3}.");

                // Branch 1: x_i <= floor(x_i)
                var leftModel = currentNode.Model.Clone();
                leftModel.AddConstraint(new Constraint(fractionalVarIndex, ConstraintType.LessThanOrEqual, floorValue, initialModel.NumVariables));
                var leftNode = new Node(leftModel);
                nodeQueue.Enqueue(leftNode, GetPriority(leftNode, initialModel.ObjectiveType));
                outputBuilder.AppendLine($"    Added constraint: x{fractionalVarIndex + 1} <= {floorValue}");

                // Branch 2: x_i >= ceil(x_i)
                var rightModel = currentNode.Model.Clone();
                rightModel.AddConstraint(new Constraint(fractionalVarIndex, ConstraintType.GreaterThanOrEqual, ceilValue, initialModel.NumVariables));
                var rightNode = new Node(rightModel);
                nodeQueue.Enqueue(rightNode, GetPriority(rightNode, initialModel.ObjectiveType));
                outputBuilder.AppendLine($"    Added constraint: x{fractionalVarIndex + 1} >= {ceilValue}");
            }

            return FormatFinalResult();
        }

        // Uses the value of the relaxed solution as priority for a best-first search
        private double GetPriority(Node node, string objectiveType)
        {
            var result = SimplexAdapter.Solve(node.Model);
            if (!result.IsFeasible) return objectiveType == "max" ? double.MinValue : double.MaxValue;
            return objectiveType == "max" ? -result.ObjectiveValue : result.ObjectiveValue;
        }

        // Implemented a better branching heuristic: closest to 0.5
        private int FindBestFractionalVariable(double[] solution)
        {
            int bestIndex = -1;
            double maxFractional = 0;

            for (int i = 0; i < solution.Length; i++)
            {
                double fractionalPart = Math.Abs(solution[i] - Math.Round(solution[i]));
                if (fractionalPart > 0.001)
                {
                    if (fractionalPart > maxFractional)
                    {
                        maxFractional = fractionalPart;
                        bestIndex = i;
                    }
                }
            }
            return bestIndex;
        }

        private string FormatFinalResult()
        {
            if (bestIntegerSolution != null)
            {
                outputBuilder.AppendLine("\n=== Optimal Integer Solution Found! ===");
                outputBuilder.AppendLine($"Objective Value: {bestObjectiveValue:F4}");
                outputBuilder.AppendLine("Variable values:");
                for (int i = 0; i < bestIntegerSolution.Length; i++)
                {
                    outputBuilder.AppendLine($"x{i + 1} = {bestIntegerSolution[i]:F4}");
                }
            }
            else
            {
                outputBuilder.AppendLine("\nNo integer solution was found.");
            }
            return outputBuilder.ToString();
        }
    }
}