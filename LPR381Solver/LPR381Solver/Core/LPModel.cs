using System;
using System.Collections.Generic;
using System.Linq;

namespace LPR381Solver.Core
{
    public enum ConstraintType { LessThanOrEqual, GreaterThanOrEqual, Equal }

    public class Constraint
    {
        public List<double> Coefficients { get; private set; }
        public ConstraintType Type { get; private set; }
        public double RightHandSide { get; private set; }

        public Constraint(List<double> coeffs, ConstraintType type, double rhs)
        {
            Coefficients = new List<double>(coeffs);
            Type = type;
            RightHandSide = rhs;
        }

        public Constraint(int variableIndex, ConstraintType type, double rhs, int numVariables)
        {
            Coefficients = new List<double>(new double[numVariables]);
            Coefficients[variableIndex] = 1.0;
            Type = type;
            RightHandSide = rhs;
        }
    }

    public class LPModel
    {
        public string ObjectiveType { get; set; }
        public List<double> ObjectiveCoefficients { get; set; }
        public List<Constraint> Constraints { get; set; }
        public List<string> SignRestrictions { get; set; }

        public LPModel()
        {
            ObjectiveCoefficients = new List<double>();
            Constraints = new List<Constraint>();
            SignRestrictions = new List<string>();
        }

        public int NumVariables => ObjectiveCoefficients.Count;
        public int NumConstraints => Constraints.Count;

        public LPModel Clone()
        {
            var clonedModel = new LPModel
            {
                ObjectiveType = this.ObjectiveType,
                ObjectiveCoefficients = new List<double>(this.ObjectiveCoefficients),
                SignRestrictions = new List<string>(this.SignRestrictions)
            };

            foreach (var constraint in this.Constraints)
            {
                var clonedCoeffs = new List<double>(constraint.Coefficients);
                clonedModel.Constraints.Add(new Constraint(clonedCoeffs, constraint.Type, constraint.RightHandSide));
            }
            return clonedModel;
        }

        public void AddConstraint(Constraint newConstraint)
        {
            Constraints.Add(newConstraint);
        }
    }

    public class SimplexResult
    {
        public double[] Solution { get; set; }
        public double ObjectiveValue { get; set; }
        public bool IsFeasible { get; set; }
    }
}