using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LPR381Solver.IO
{
    internal class InputOutput
    {
        public static Tuple<List<int>, List<int>, int> LoadKnapsackModel(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    Console.WriteLine($"Error: The file '{filePath}' was not found.");
                    return null;
                }

                string[] lines = File.ReadAllLines(filePath);

                if (lines.Length < 2)
                {
                    Console.WriteLine("Invalid file format. Please provide at least two lines for the objective function and constraint.");
                    return null;
                }

                var valuesLine = lines[0].Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                var values = valuesLine.Skip(1).Select(v => int.Parse(v.Replace("+", ""))).ToList();

                var weightsLine = lines[1].Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                var capacityPart = weightsLine.Last();
                
                var capacity = int.Parse(capacityPart.Replace("<=", "").Replace(">", "").Replace("=", ""));

                var weights = weightsLine.Take(weightsLine.Length - 1).Select(w => int.Parse(w.Replace("+", ""))).ToList();

                Console.WriteLine("File loaded successfully.");
                return Tuple.Create(values, weights, capacity);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while parsing the file: {ex.Message}");
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