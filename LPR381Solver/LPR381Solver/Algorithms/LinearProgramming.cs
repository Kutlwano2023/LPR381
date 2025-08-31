using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LPR_Project
{
    public class LinearProgramming
    {
        public string ObjectiveType { get; set; }
        public List<double> Coefficients { get; set; } = new List<double>();
        public List<double> Constraints { get; set; } = new List<double>();
        public List<string> Relations { get; set; } = new List<string>();
        public List<double> RHS { get; set; } = new List<double>();

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
            var parts = line.Split(':')[1].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            model.ObjectiveType = parts[0].ToLower();
            model.Coefficients = parts.Skip(1).Select(double.Parse).ToList();
        }

        private static void ParseConstraint(string line, LinearProgramming model)
        {
            string[] parts;
            string relation;

            if (line.Contains("<="))
            {
                parts = line.Split(new string[] { "<=" }, StringSplitOptions.None);
                relation = "<=";
            }
            else if (line.Contains(">="))
            {
                parts = line.Split(new string[] { ">=" }, StringSplitOptions.None);
                relation = ">=";
            }
            else if (line.Contains("="))
            {
                parts = line.Split(new string[] { "=" }, StringSplitOptions.None);
                relation = "=";
            }
            else
            {
                throw new FormatException("Invalid constraint format: relation not found.");
            }

            string coefficientsString = parts[0].Split(':')[1].Trim();
            string rhsString = parts[1].Trim();

            var coefficients = coefficientsString.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(double.Parse).ToList();
            var rhs = double.Parse(rhsString);

            model.Constraints.AddRange(coefficients);
            model.Relations.Add(relation);
            model.RHS.Add(rhs);
        }
    }
}