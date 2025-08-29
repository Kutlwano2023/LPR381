using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LPR381Solver.Algorithms
{
    public class Knapsack
    {
        private readonly List<int> _values;
        private readonly List<int> _weights;
        private readonly int _capacity;
        private readonly string _objectiveType;

        private int _bestValue;
        private List<bool> _bestSolution;
        private StringBuilder _outputBuilder;

        public Knapsack(string objectiveType, List<int> values, List<int> weights, int capacity)
        {
            _objectiveType = objectiveType;
            _values = values;
            _weights = weights;
            _capacity = capacity;
            _outputBuilder = new StringBuilder();
        }

        public string Solve()
        {
            _outputBuilder.AppendLine($"Starting Branch & Bound Knapsack Algorithm...");

            _bestValue = (_objectiveType.ToLower() == "max") ? -1 : int.MaxValue;
            _bestSolution = new List<bool>(new bool[_values.Count]);

            Search(0, 0, 0, new List<bool>(new bool[_values.Count]));

            _outputBuilder.AppendLine($"\nOptimal Solution Found!");
            _outputBuilder.AppendLine($"Optimal Value: {_bestValue}");
            _outputBuilder.AppendLine("Included Items:");

            for (int i = 0; i < _bestSolution.Count; i++)
            {
                if (_bestSolution[i])
                {
                    _outputBuilder.AppendLine($"- Item {i + 1} (Value: {_values[i]}, Weight: {_weights[i]})");
                }
            }
            return _outputBuilder.ToString();
        }

        private void Search(int itemIndex, int currentValue, int currentWeight, List<bool> currentSolution)
        {
            // Pruning/Fathoming based on objective type
            if (_objectiveType.ToLower() == "max")
            {
                if (currentValue + CalculateUpperBound(itemIndex) <= _bestValue)
                {
                    _outputBuilder.AppendLine($"  Fathoming node at level {itemIndex}. Upper bound ({currentValue + CalculateUpperBound(itemIndex):F2}) is not better than current best ({_bestValue}).");
                    return;
                }
            }
            else // Minimize
            {
                // This logic is simplified but effective for positive values.
                // If the current value already exceeds the best value, no need to continue.
                if (currentValue >= _bestValue)
                {
                    _outputBuilder.AppendLine($"  Fathoming node at level {itemIndex}. Current value ({currentValue}) is not better than current best ({_bestValue}).");
                    return;
                }
            }

            // Check if a complete solution is found
            if (itemIndex == _values.Count)
            {
                // Update best solution based on objective type
                if (_objectiveType.ToLower() == "max")
                {
                    if (currentValue > _bestValue)
                    {
                        _bestValue = currentValue;
                        _bestSolution = new List<bool>(currentSolution);
                        _outputBuilder.AppendLine($"  New best solution found! Value: {_bestValue}");
                    }
                }
                else // Minimize
                {
                    if (currentValue < _bestValue)
                    {
                        _bestValue = currentValue;
                        _bestSolution = new List<bool>(currentSolution);
                        _outputBuilder.AppendLine($"  New best solution found! Value: {_bestValue}");
                    }
                }
                return;
            }

            // Branch 1: Include the current item
            if (currentWeight + _weights[itemIndex] <= _capacity)
            {
                _outputBuilder.AppendLine($"  Branching to include item {itemIndex + 1}. Current value: {currentValue + _values[itemIndex]}, upper bound: {currentValue + _values[itemIndex] + CalculateUpperBound(itemIndex + 1):F2}");
                currentSolution[itemIndex] = true;
                Search(itemIndex + 1, currentValue + _values[itemIndex], currentWeight + _weights[itemIndex], currentSolution);
            }

            // Branch 2: Exclude the current item
            _outputBuilder.AppendLine($"  Branching to exclude item {itemIndex + 1}. Current value: {currentValue}, upper bound: {currentValue + CalculateUpperBound(itemIndex + 1):F2}");
            currentSolution[itemIndex] = false;
            Search(itemIndex + 1, currentValue, currentWeight, currentSolution);
        }

        private double CalculateUpperBound(int startItemIndex)
        {
            double upperBound = 0;
            double remainingCapacity = _capacity;

            // Simple upper bound calculation for the rest of the items.
            for (int i = startItemIndex; i < _values.Count; i++)
            {
                upperBound += _values[i];
            }
            return upperBound;
        }
    }
}