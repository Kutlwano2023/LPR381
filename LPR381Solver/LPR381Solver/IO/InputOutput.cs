using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LPR381Solver.Core;

namespace LPR381Solver.IO
{
    internal class InputOutput
    {
        public static LPModel LoadLPModel(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    Console.WriteLine($"Error: The file '{filePath}' was not found.");
                    return null;
                }

                string[] lines = File.ReadAllLines(filePath);
                var model = new LPModel();
                
                // Parse Objective Function
                var objParts = lines[0].Trim().Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                model.ObjectiveType = objParts[0];
                model.ObjectiveCoefficients = objParts.Skip(1)
                                                      .Select(part => double.Parse(part.TrimStart('+')))
                                                      .ToList();

                // Parse Constraints
                for (int i = 1; i < lines.Length - 1; i++)
                {
                    var line = lines[i].Trim();
                    string relation = "";
                    if (line.Contains("<=")) relation = "<=";
                    else if (line.Contains(">=")) relation = ">=";
                    else if (line.Contains("=")) relation = "=";

                    if (string.IsNullOrEmpty(relation))
                    {
                        throw new FormatException("Constraint line is missing a valid relation (<=, >=, =).");
                    }

                    var parts = line.Split(new[] { relation }, StringSplitOptions.RemoveEmptyEntries);
                    
                    var coefficients = parts[0].Trim().Split((char[])null, StringSplitOptions.RemoveEmptyEntries)
                                               .Select(part => double.Parse(part.TrimStart('+')))
                                               .ToList();
                    
                    double rhs = double.Parse(parts[1].Trim());

                    ConstraintType type = ConstraintType.Equal;
                    if (relation == "<=") type = ConstraintType.LessThanOrEqual;
                    if (relation == ">=") type = ConstraintType.GreaterThanOrEqual;

                    model.Constraints.Add(new Constraint(coefficients, type, rhs));
                }

                // Parse Sign Restrictions (the last line)
                model.SignRestrictions = lines.Last().Trim().Split((char[])null, StringSplitOptions.RemoveEmptyEntries).ToList();

                return model;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while parsing the LP Model file: {ex.Message}");
                return null;
            }
        }

        public static Tuple<string, List<int>, List<int>, int> LoadKnapsackModel(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    Console.WriteLine($"Error: The file '{filePath}' was not found.");
                    return null;
                }

                string[] lines = File.ReadAllLines(filePath);
                
                var valuesLine = lines[0].Trim().Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                string objectiveType = valuesLine[0];
                var values = valuesLine.Skip(1).Select(v => int.Parse(v.TrimStart('+'))).ToList();

                var weightsLine = lines[1].Trim();
                var relationIndex = weightsLine.IndexOfAny(new char[] { '<', '>', '=' });
                var weightsPart = weightsLine.Substring(0, relationIndex).Trim();
                var capacityPart = weightsLine.Substring(relationIndex + 2).Trim();

                var weights = weightsPart.Split((char[])null, StringSplitOptions.RemoveEmptyEntries)
                                         .Select(w => int.Parse(w.TrimStart('+'))).ToList();
                int capacity = int.Parse(capacityPart);

                return Tuple.Create(objectiveType, values, weights, capacity);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while parsing the Knapsack file: {ex.Message}");
                return null;
            }
        }

        public static void ExportResults(string filePath, string results)
        {
            try
            {
                File.WriteAllText(filePath, results);
                Console.WriteLine($"Results successfully exported to: {filePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while exporting the file: {ex.Message}");
            }
        }
    }
}