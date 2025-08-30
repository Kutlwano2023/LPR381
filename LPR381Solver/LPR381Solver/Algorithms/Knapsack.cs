using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LPR381Solver.Algorithms
{
    public class Knapsack
    {
        private readonly List<double> _values;
        private readonly List<double> _weights;
        private readonly double _capacity;
        private readonly string _objectiveType;
        private readonly List<Item> _items;

        private double _bestValue;
        private List<bool> _bestSolution;
        private StringBuilder _outputBuilder;

        private class Item
        {
            public int Index { get; set; }
            public double Value { get; set; }
            public double Weight { get; set; }
            public double Ratio { get; set; }
        }

        public Knapsack(string objectiveType, List<int> values, List<int> weights, int capacity)
        {
            _objectiveType = objectiveType;
            _capacity = capacity;
            _outputBuilder = new StringBuilder();

            _items = new List<Item>();
            for (int i = 0; i < values.Count; i++)
            {
                _items.Add(new Item
                {
                    Index = i,
                    Value = values[i],
                    Weight = weights[i],
                    Ratio = (double)values[i] / weights[i]
                });
            }

            // Sort items by value-to-weight ratio in descending order for the upper bound calculation
            _items = _items.OrderByDescending(i => i.Ratio).ToList();
        }

        public string Solve()
        {
            _outputBuilder.AppendLine($"Starting Branch & Bound Knapsack Algorithm...");

            _bestValue = (_objectiveType.ToLower() == "max") ? double.MinValue : double.MaxValue;
            _bestSolution = new List<bool>(new bool[_items.Count]);

            Search(0, 0, 0, new List<bool>(new bool[_items.Count]));

            _outputBuilder.AppendLine($"\nOptimal Solution Found!");
            _outputBuilder.AppendLine($"Optimal Value: {_bestValue}");
            _outputBuilder.AppendLine("Included Items:");

            // We need to map the sorted solution back to the original indices
            var originalOrderSolution = new List<bool>(new bool[_items.Count]);
            for(int i = 0; i < _items.Count; i++)
            {
                if (_bestSolution[i])
                {
                    originalOrderSolution[_items[i].Index] = true;
                }
            }

            for (int i = 0; i < originalOrderSolution.Count; i++)
            {
                if (originalOrderSolution[i])
                {
                    _outputBuilder.AppendLine($"- Item {i + 1} (Value: {_items.First(item => item.Index == i).Value}, Weight: {_items.First(item => item.Index == i).Weight})");
                }
            }
            return _outputBuilder.ToString();
        }

        private void Search(int itemIndex, double currentValue, double currentWeight, List<bool> currentSolution)
        {
            // Fathoming: check if the current value is better than the best value found so far
            if (_objectiveType.ToLower() == "max")
            {
                // Prune if the upper bound is not better than the current best integer solution
                if (currentValue + CalculateUpperBound(itemIndex, currentWeight) <= _bestValue)
                {
                    _outputBuilder.AppendLine($"  Fathoming node at level {itemIndex}. Upper bound is not better than current best ({_bestValue}).");
                    return;
                }
            }
            else // Minimize
            {
                // Prune if the current value is already worse than the best
                if (currentValue >= _bestValue)
                {
                    _outputBuilder.AppendLine($"  Fathoming node at level {itemIndex}. Current value ({currentValue}) is not better than current best ({_bestValue}).");
                    return;
                }
            }

            // Check if a complete solution is found
            if (itemIndex == _items.Count)
            {
                // Update best solution based on objective type
                if (_objectiveType.ToLower() == "max")
                {
                    if (currentValue > _bestValue)
                    {
                        _bestValue = currentValue;
                        _bestSolution = new List<bool>(currentSolution);
                        _outputBuilder.AppendLine($"  New best solution found! Value: {_bestValue}");
                    }
                }
                else // Minimize
                {
                    if (currentValue < _bestValue)
                    {
                        _bestValue = currentValue;
                        _bestSolution = new List<bool>(currentSolution);
                        _outputBuilder.AppendLine($"  New best solution found! Value: {_bestValue}");
                    }
                }
                return;
            }

            // Branch 1: Include the current item
            if (currentWeight + _items[itemIndex].Weight <= _capacity)
            {
                currentSolution[itemIndex] = true;
                Search(itemIndex + 1, currentValue + _items[itemIndex].Value, currentWeight + _items[itemIndex].Weight, new List<bool>(currentSolution));
            }

            // Branch 2: Exclude the current item
            currentSolution[itemIndex] = false;
            Search(itemIndex + 1, currentValue, currentWeight, new List<bool>(currentSolution));
        }

        private double CalculateUpperBound(int startItemIndex, double currentWeight)
        {
            double upperBound = 0;
            double remainingCapacity = _capacity - currentWeight;

            for (int i = startItemIndex; i < _items.Count; i++)
            {
                if (_items[i].Weight <= remainingCapacity)
                {
                    upperBound += _items[i].Value;
                    remainingCapacity -= _items[i].Weight;
                }
                else
                {
                    // Add a fraction of the item to fill the remaining capacity
                    upperBound += _items[i].Ratio * remainingCapacity;
                    break;
                }
            }
            return upperBound;
        }
    }
}