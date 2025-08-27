using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LPR381Solver.IO; // Make sure this using directive is correct

namespace LPR_Project
{
    internal class Program
    {
        private static List<int> modelValues;
        private static List<int> modelWeights;
        private static int modelCapacity;
        private static string solutionResults;

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
                Console.WriteLine("9. Exit"); // Added a new exit option
                Console.WriteLine("==============================================");
                Console.WriteLine("======Pick an option======");

                if (!int.TryParse(Console.ReadLine(), out int choice))
                {
                    Console.WriteLine("Invalid input. Please enter a number.");
                    continue; // Skip the rest of the loop and show the menu again
                }

                switch (choice)
                {
                    case 1:
                        Console.WriteLine("Enter the full path of the input file:");
                        string filePath = Console.ReadLine();

                        var modelData = InputOutput.LoadKnapsackModel(filePath);

                        if (modelData != null)
                        {
                            modelValues = modelData.Item1;
                            modelWeights = modelData.Item2;
                            modelCapacity = modelData.Item3;
                            Console.WriteLine("Model data stored. You can now choose an algorithm to solve.");
                        }
                        break;
                    case 2:
                        // Add your Primal Simplex logic here
                        break;
                    case 3:
                        // Add your Revised Primal Simplex logic here
                        break;
                    case 4:
                        // Add your Branch & Bound Simplex logic here
                        break;
                    case 5:
                        if (modelValues == null || modelWeights == null)
                            {
                                Console.WriteLine("Error: Please load a model first (Option 1).");
                                break;
                            }
                        var knapsack = new Knapsack(modelValues, modelWeights, modelCapacity);
                        solutionResults = knapsack.Solve(); // Assign the result to your new variable
                        Console.WriteLine(solutionResults); 
                        break;
                    case 6:
                        // Add your Cutting Plane logic here
                        break;
                    case 7:
                        // Add your Sensitivity Analysis logic here
                        break;
                    case 8:
                        if (solutionResults != null) // Check if a solution has been found
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

                // Pause the program before showing the menu again to let the user read the output
                if (!exitProgram)
                {
                    Console.WriteLine("\nPress Enter to continue...");
                    Console.ReadLine();
                    Console.Clear(); // Clears the console for a fresh menu
                }
            }
        }
    }
}