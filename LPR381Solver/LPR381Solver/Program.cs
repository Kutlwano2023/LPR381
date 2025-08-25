using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Instrumentation;
using System.Text;
using System.Threading.Tasks;

namespace LPR_Project
{
    internal class Program
    {
        //menu for application
        static void Main(string[] args)
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
            Console.WriteLine("==============================================");
            Console.WriteLine("======Pick an option======");
            int choice = int.Parse(Console.ReadLine());

            switch (choice)
            {
                case 1:
                     LinearProgramming.LoadModel(); 
                    break;
                case 2:
                    
                        break;
                case 3:

                    break;
                case 4:

                    break;

                case 5:

                    break;

                case 6:

                    break;
                case 7:

                    break;
                case 8:

                    break;
                case 9:
                    return;
                default:
                    Console.WriteLine("Pick a valid option");
                    Console.ReadLine();
                    break;




            }

        }
    }
}
