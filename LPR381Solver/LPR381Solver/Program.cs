using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LPR381Solver.Core;
using LPR381Solver.Algorithms;
using LPR381Solver.IO;
using LPR_Project; // Added this using directive for PrimalSimplex

namespace LPR381Solver.Main
{
    internal class Program
    {
        private static LPModel loadedModel;
        private static string modelObjectiveType;
        private static List<int> modelValues;
        private static List<int> modelWeights;
        private static int modelCapacity;
        private static string solutionResults = "";

        static void Main(string[] args)
        {
            bool exitProgram = false;

            while (!exitProgram)
            {
                Console.WriteLine("==========Linear Programming Solver==========");
                Console.WriteLine("");
                Console.WriteLine("1. Load Model from Input File");
                Console.WriteLine("2. Solve with Primal Simplex");
                Console.WriteLine("3. Revised Primal Simplex");
                Console.WriteLine("4 Branch & Bound Simplex Algorithm");
                Console.WriteLine("5. Branch & Bound Knapsack Algorithm");
                Console.WriteLine("6. Cutting Plane Algorithm");
                Console.WriteLine("7. Sensitivity Analysis");
                Console.WriteLine("8. Export result to output file");
                Console.WriteLine("9. Exit");
                Console.WriteLine("==============================================");
                Console.WriteLine("======Pick an option======");

                if (!int.TryParse(Console.ReadLine(), out int choice))
                {
                    Console.WriteLine("Invalid input. Please enter a number.");
                    continue;
                }

                switch (choice)
                {
                    case 1:
                        Console.WriteLine("Enter the full path of the input file:");
                        string filePath = Console.ReadLine();
                        
                        // Try to load as an LPModel first (for options 2, 4)
                        loadedModel = InputOutput.LoadLPModel(filePath);
                        
                        // If that fails, try to load as a Knapsack model (for option 5)
                        if (loadedModel == null)
                        {
                            var knapsackData = InputOutput.LoadKnapsackModel(filePath);
                            if (knapsackData != null)
                            {
                                modelObjectiveType = knapsackData.Item1;
                                modelValues = knapsackData.Item2;
                                modelWeights = knapsackData.Item3;
                                modelCapacity = knapsackData.Item4;
                                Console.WriteLine("Knapsack model data stored. You can now choose an algorithm to solve.");
                            }
                            else
                            {
                                Console.WriteLine("Failed to load model from file. Check the file format and path.");
                            }
                        }
                        else
                        {
                            Console.WriteLine("LP Model data stored. You can now choose an algorithm to solve.");
                            
                            // Extra step to check if the LP Model is also a Knapsack problem (i.e., has binary variables)
                            if (loadedModel.SignRestrictions != null && loadedModel.SignRestrictions.All(sr => sr == "bin"))
                            {
                                var knapsackData = InputOutput.LoadKnapsackModel(filePath);
                                if (knapsackData != null)
                                {
                                    modelObjectiveType = knapsackData.Item1;
                                    modelValues = knapsackData.Item2;
                                    modelWeights = knapsackData.Item3;
                                    modelCapacity = knapsackData.Item4;
                                }
                            }
                        }
                        break;
                    case 2:
                        // ADDED PRIMAL SIMPLEX LOGIC HERE
                        if (loadedModel == null)
                        {
                            Console.WriteLine("Error: Please load an LP model first (Option 1).");
                            break;
                        }

                        // Convert LPModel to the PrimalSimplex's expected LinearProgramming format
                        var simplexModel = new LPR_Project.LinearProgramming
                        {
                            ObjectiveType = loadedModel.ObjectiveType,
                            Coefficients = loadedModel.ObjectiveCoefficients,
                            Constraints = loadedModel.Constraints.SelectMany(c => c.Coefficients).ToList(),
                            Relations = loadedModel.Constraints.Select(c =>
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
                            RHS = loadedModel.Constraints.Select(c => c.RightHandSide).ToList()
                        };

                        // Solve using the PrimalSimplex algorithm, which returns a SimplexResult object.
                        var primalSimplexResult = LPR_Project.LinearProgramming.PrimalSimplex.Solve(simplexModel);

                        // Now, format the result object into a string for display.
                        string newPrimalSimplexResults;
                        if (!primalSimplexResult.IsFeasible)
                        {
                             newPrimalSimplexResults = "The problem is infeasible or unbounded.";
                        }
                        else
                        {
                            var sb = new StringBuilder();
                            sb.AppendLine($"Optimal Objective Value: {primalSimplexResult.ObjectiveValue:F4}");
                            sb.AppendLine("Variable values:");
                            for (int i = 0; i < primalSimplexResult.Variables.Length; i++)
                            {
                                sb.AppendLine($"x{i + 1} = {primalSimplexResult.Variables[i]:F4}");
                            }
                            newPrimalSimplexResults = sb.ToString();
                        }

                        string formattedPrimalSimplexResults = "\n=============================================\n" +
                                                         "== Primal Simplex Results ==\n" +
                                                         "=============================================\n" +
                                                         newPrimalSimplexResults;
                        
                        solutionResults += formattedPrimalSimplexResults;
                        Console.WriteLine(formattedPrimalSimplexResults);
                        break;
                    case 3:
                        Console.WriteLine("Revised Primal Simplex logic to be implemented.");
                        break;
                    case 4:
                        if (loadedModel == null)
                        {
                            Console.WriteLine("Error: Please load an LP model first (Option 1).");
                            break;
                        }
                        var branchAndBoundSolver = new BranchAndBoundSimplex();
                        string newBranchAndBoundResults = "\n=============================================\n" +
                                                          "== Branch and Bound Simplex Results ==\n" +
                                                          "=============================================\n" + 
                                                          branchAndBoundSolver.Solve(loadedModel);
                        solutionResults += newBranchAndBoundResults;
                        Console.WriteLine(newBranchAndBoundResults);
                        break;
                    case 5:
                        if (modelValues == null || modelWeights == null)
                        {
                            Console.WriteLine("Error: Please load a model first (Option 1).");
                            break;
                        }
                        var knapsack = new Knapsack(modelObjectiveType, modelValues, modelWeights, modelCapacity);
                        string newKnapsackResults = "\n=============================================\n" +
                                                    "== Branch and Bound Knapsack Results ==\n" +
                                                    "=============================================\n" + 
                                                    knapsack.Solve();
                        solutionResults += newKnapsackResults;
                        Console.WriteLine(newKnapsackResults);
                        break;
                    case 6:
                        Console.WriteLine("Cutting Plane Algorithm logic to be implemented.");
                        break;
                    case 7:
                        Console.WriteLine("Sensitivity Analysis logic to be implemented.");
                        break;
                    case 8:
                        if (solutionResults != null)
                        {
                            Console.WriteLine("Enter the path for the output file (e.g., C:\\output.txt):");
                            string outputPath = Console.ReadLine();
                            InputOutput.ExportResults(outputPath, solutionResults);
                        }
                        else
                        {
                            Console.WriteLine("Error: Please solve a model first before exporting results.");
                        }
                        break;
                    case 9:
                        exitProgram = true;
                        break;
                    default:
                        Console.WriteLine("Pick a valid option.");
                        break;
                }
                
                if (!exitProgram)
                {
                    Console.WriteLine("\nPress Enter to continue...");
                    Console.ReadLine();
                    Console.Clear();
                }
            }
        }
    }
}