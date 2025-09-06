using System;
using System.Linq;
using System.Text;
using LPR381Solver.IO;

namespace LPR381Solver.Algorithms
{
    internal static class Sensitivity
    {

        public static void Run(double[,] A, double[] b, double[] c, int[] basisIndices, double[,] BInv, string outputPath = "SensitivityResults.txt")
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== Sensitivity Analysis Output ===\n");

            var xB = Multiply(BInv, b);
            sb.AppendLine("Basic solution xB:");
            sb.AppendLine(VectorToString(xB, basisIndices));

            var shadows = ComputeShadowPrices(c, basisIndices, BInv);
            sb.AppendLine("\nShadow prices:");
            for (int i = 0; i < shadows.Length; i++)
                sb.AppendLine($"Constraint {i + 1}: {shadows[i]:F3}");

            sb.AppendLine("\nRHS sensitivity ranges:");
            for (int i = 0; i < b.Length; i++)
            {
                var range = RHSRange(BInv, xB, i);
                sb.AppendLine($"Constraint {i + 1}: Δb ∈ [{range.Item1:F3}, {range.Item2:F3}]");
            }

            sb.AppendLine("\nNon-basic variable coefficient ranges:");
            for (int j = 0; j < c.Length; j++)
            {
                if (!basisIndices.Contains(j))
                {
                    var range = NonBasicObjectiveRange(c, basisIndices, BInv, GetColumn(A, j), c[j]);
                    sb.AppendLine($"x{j + 1}, c={c[j]}: Δc ∈ [{range.Item1:F3}, {range.Item2:F3}]");
                }
            }

            InputOutput.ExportResults(outputPath, sb.ToString());
        }

        // --- Utility functions ---
        public static double[] Multiply(double[,] M, double[] v)
        {
            int r = M.GetLength(0);
            int c = M.GetLength(1);
            double[] res = new double[r];
            for (int i = 0; i < r; i++)
                for (int j = 0; j < c; j++)
                    res[i] += M[i, j] * v[j];
            return res;
        }

        public static double DotProduct(double[] v1, double[] v2)
        {
            double sum = 0;
            for (int i = 0; i < v1.Length; i++) sum += v1[i] * v2[i];
            return sum;
        }

        public static double[] GetColumn(double[,] M, int col)
        {
            int r = M.GetLength(0);
            double[] colArr = new double[r];
            for (int i = 0; i < r; i++) colArr[i] = M[i, col];
            return colArr;
        }

        private static string VectorToString(double[] v, int[] indices = null)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < v.Length; i++)
            {
                int idx = indices != null ? indices[i] : i;
                sb.AppendLine($"x{idx + 1} = {v[i]:F3}");
            }
            return sb.ToString();
        }

        // --- Sensitivity calculations ---
        public static double[] ComputeShadowPrices(double[] c, int[] basisIndices, double[,] BInv)
        {
            double[] cB = basisIndices.Select(i => c[i]).ToArray();
            int m = BInv.GetLength(0);
            double[] y = new double[m];
            for (int j = 0; j < m; j++)
                for (int i = 0; i < m; i++)
                    y[j] += cB[i] * BInv[i, j];
            return y;
        }

        public static Tuple<double, double> RHSRange(double[,] BInv, double[] xB, int k)
        {
            int m = BInv.GetLength(0);
            double minDelta = double.NegativeInfinity;
            double maxDelta = double.PositiveInfinity;
            for (int i = 0; i < m; i++)
            {
                double coeff = BInv[i, k];
                if (Math.Abs(coeff) < 1e-12) continue;
                double bound = -xB[i] / coeff;
                if (coeff > 0) maxDelta = Math.Min(maxDelta, bound);
                else minDelta = Math.Max(minDelta, bound);
            }
            return Tuple.Create(minDelta, maxDelta);
        }

        public static Tuple<double, double> NonBasicObjectiveRange(double[] c, int[] basisIndices, double[,] BInv, double[] a_j, double c_j)
        {
            double[] cB = basisIndices.Select(i => c[i]).ToArray();
            double[] BinvAj = Multiply(BInv, a_j);
            double reducedCost = c_j - DotProduct(cB, BinvAj);

            double minDelta = double.NegativeInfinity;
            double maxDelta = double.PositiveInfinity;
            if (reducedCost < 0) maxDelta = -reducedCost;
            else if (reducedCost > 0) minDelta = -reducedCost;

            return Tuple.Create(minDelta, maxDelta);
        }

        // --- Display & interactive helpers ---
        public static void DisplayNonBasicRanges(double[] c, int[] basis, double[,] BInv, double[,] A)
        {
            Console.WriteLine("\nNon-Basic Variable Ranges:");
            for (int j = 0; j < c.Length; j++)
            {
                if (!basis.Contains(j))
                {
                    var range = NonBasicObjectiveRange(c, basis, BInv, GetColumn(A, j), c[j]);
                    Console.WriteLine($"x{j + 1}: Δc ∈ [{range.Item1:F3}, {range.Item2:F3}]");
                }
            }
        }

        public static void DisplayBasicRanges(int[] basis, double[,] BInv, double[] b)
        {
            Console.WriteLine("\nBasic Variable Ranges:");
            double[] xB = Multiply(BInv, b);
            for (int i = 0; i < basis.Length; i++)
            {
                double minDelta = -xB[i];
                Console.WriteLine($"x{basis[i] + 1}: Δx ∈ [{minDelta:F3}, ∞]");
            }
        }

        public static void DisplayRHSRanges(double[,] BInv, double[] b)
        {
            Console.WriteLine("\nConstraint RHS Ranges:");
            double[] xB = Multiply(BInv, b);
            for (int i = 0; i < b.Length; i++)
            {
                var range = RHSRange(BInv, xB, i);
                Console.WriteLine($"Constraint {i + 1}: Δb ∈ [{range.Item1:F3}, {range.Item2:F3}]");
            }
        }

        public static void DisplayShadowPrices(double[] c, int[] basisIndices, double[,] BInv)
        {
            var shadows = ComputeShadowPrices(c, basisIndices, BInv);
            Console.WriteLine("\nShadow Prices:");
            for (int i = 0; i < shadows.Length; i++)
                Console.WriteLine($"Constraint {i + 1}: {shadows[i]:F3}");
        }

        public static void ApplyNonBasicInteractive(double[] c, int[] basisIndices, double[,] BInv, double[,] A, double[] b)
        {
            Console.Write("Enter Non-Basic Variable index (1-based): ");
            int nbIndex = int.Parse(Console.ReadLine()) - 1;
            var range = NonBasicObjectiveRange(c, basisIndices, BInv, GetColumn(A, nbIndex), c[nbIndex]);
            Console.WriteLine($"Allowed Δc: [{range.Item1:F3}, {range.Item2:F3}]");
            Console.Write("Enter new value: ");
            double newVal = double.Parse(Console.ReadLine());
            double[] xB = Multiply(BInv, b);
            double newObj = DotProduct(basisIndices.Select(i => c[i]).ToArray(), xB) + newVal;
            Console.WriteLine($"New objective: {newObj:F3}");
        }

        public static void ApplyBasicInteractive(int[] basisIndices, double[,] BInv, double[] b, double[] c)
        {
            Console.Write("Enter Basic Variable index (1-based): ");
            int bIndex = int.Parse(Console.ReadLine()) - 1;
            double[] xB = Multiply(BInv, b);
            Console.Write("Enter new value: ");
            double newVal = double.Parse(Console.ReadLine());
            xB[bIndex] = newVal;
            double[] cB = basisIndices.Select(i => c[i]).ToArray();
            double newObj = DotProduct(cB, xB);
            Console.WriteLine($"New objective: {newObj:F3}");
        }

        public static void ApplyRHSInteractive(double[,] BInv, double[] b, double[] c, int[] basisIndices)
        {
            Console.Write("Enter Constraint index (1-based): ");
            int rhsIndex = int.Parse(Console.ReadLine()) - 1;
            Console.Write("Enter new RHS value: ");
            double newVal = double.Parse(Console.ReadLine());
            double delta = newVal - b[rhsIndex];
            b[rhsIndex] = newVal;
            double[] xB = Multiply(BInv, b);
            double[] cB = basisIndices.Select(i => c[i]).ToArray();
            double newObj = DotProduct(cB, xB);
            Console.WriteLine($"New objective: {newObj:F3}");
        }
    }
}
