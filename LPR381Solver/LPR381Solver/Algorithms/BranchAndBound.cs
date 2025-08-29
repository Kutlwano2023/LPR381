using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LPR381Solver.Core;

namespace LPR381Solver.Algorithms
{
    public class BranchAndBoundSimplex
    {
        private class Node
        {
            public LPModel Model { get; private set; }
            public double[] Solution { get; set; }
            public double ObjectiveValue { get; set; }

            public Node(LPModel model)
            {
                this.Model = model;
            }
        }

        private double bestIntegerSolutionValue = double.MinValue;
        private double[] bestIntegerSolution = null;
        private StringBuilder outputBuilder = new StringBuilder();

        public string Solve(LPModel initialModel)
        {
            outputBuilder.AppendLine("Starting Branch and Bound Simplex Algorithm...");
            var nodeStack = new Stack<Node>();
            nodeStack.Push(new Node(initialModel));

            while (nodeStack.Count > 0)
            {
                var currentNode = nodeStack.Pop();
                
                outputBuilder.AppendLine($"\nExploring a new node...");

                var solver = new Simplex(currentNode.Model);
                var result = solver.Solve();

                currentNode.Solution = result.Solution;
                currentNode.ObjectiveValue = result.ObjectiveValue;
                
                outputBuilder.AppendLine($"Relaxed LP Solution: Objective = {currentNode.ObjectiveValue:F3}");
                outputBuilder.AppendLine($"Solution values: {string.Join(", ", currentNode.Solution.Select(val => val.ToString("F3")))}");

                if (!result.IsFeasible)
                {
                    outputBuilder.AppendLine("  Fathomed: LP is infeasible.");
                    continue; // Move to the next node in the stack
                }

                if (currentNode.ObjectiveValue <= bestIntegerSolutionValue)
                {
                    outputBuilder.AppendLine($"  Fathomed: Objective value ({currentNode.ObjectiveValue:F3}) is not better than the current best integer solution ({bestIntegerSolutionValue:F3}).");
                    continue; // Move to the next node in the stack
                }

                int fractionalVarIndex = FindFirstFractionalVariable(currentNode.Solution);
                if (fractionalVarIndex == -1)
                {
                    bestIntegerSolutionValue = currentNode.ObjectiveValue;
                    bestIntegerSolution = currentNode.Solution;
                    outputBuilder.AppendLine($"  Fathomed: Found a new integer solution with value {bestIntegerSolutionValue:F3}.");
                    continue; // Move to the next node in the stack
                }

                double fractionalValue = currentNode.Solution[fractionalVarIndex];
                double floorValue = Math.Floor(fractionalValue);
                double ceilValue = Math.Ceiling(fractionalValue);

                outputBuilder.AppendLine($"  Branching on variable x{fractionalVarIndex + 1} with fractional value {fractionalValue:F3}.");

                // Branching step, adding new nodes to the stack
                var rightModel = currentNode.Model.Clone();
                rightModel.AddConstraint(new Constraint(fractionalVarIndex, ConstraintType.GreaterThanOrEqual, ceilValue, rightModel.NumVariables));
                nodeStack.Push(new Node(rightModel));

                var leftModel = currentNode.Model.Clone();
                leftModel.AddConstraint(new Constraint(fractionalVarIndex, ConstraintType.LessThanOrEqual, floorValue, leftModel.NumVariables));
                nodeStack.Push(new Node(leftModel));
            }

            if (bestIntegerSolution != null)
            {
                outputBuilder.AppendLine("\nOptimal Integer Solution Found!");
                outputBuilder.AppendLine($"Objective Value: {bestIntegerSolutionValue:F3}");
                outputBuilder.AppendLine("Variable values:");
                for (int i = 0; i < bestIntegerSolution.Length; i++)
                {
                    outputBuilder.AppendLine($"x{i + 1} = {bestIntegerSolution[i]:F3}");
                }
            }
            else
            {
                outputBuilder.AppendLine("\nNo integer solution was found.");
            }
            return outputBuilder.ToString();
        }

        private int FindFirstFractionalVariable(double[] solution)
        {
            for (int i = 0; i < solution.Length; i++)
            {
                if (Math.Abs(solution[i] - Math.Round(solution[i])) > 0.001)
                {
                    return i;
                }
            }
            return -1;
        }
    }
}