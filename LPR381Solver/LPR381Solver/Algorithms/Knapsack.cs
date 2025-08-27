using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Knapsack
{
    // Private nested classes for organizing data
    private class Item
    {
        public int Weight { get; set; }
        public int Value { get; set; }
        public double Density => (double)Value / Weight;
    }

    private class Node
    {
        public int Level { get; set; }
        public int CurrentWeight { get; set; }
        public int CurrentValue { get; set; }
        public double UpperBound { get; set; }
        public List<int> IncludedItems { get; set; } = new List<int>();
    }

    private List<Item> items;
    private int capacity;
    private int bestValue = 0;
    private List<int> bestSolution = new List<int>();
    private StringBuilder outputBuilder;

    public Knapsack(List<int> values, List<int> weights, int capacity)
    {
        this.capacity = capacity;
        this.items = new List<Item>();
        for (int i = 0; i < values.Count; i++)
        {
            this.items.Add(new Item { Value = values[i], Weight = weights[i] });
        }
        this.outputBuilder = new StringBuilder();
    }

    // Public method to start the solving process and return the full output string
    public string Solve()
    {
        outputBuilder.AppendLine("\nStarting Branch & Bound Knapsack Algorithm...");

        // Sort items by density in descending order for better bounding
        items = items.OrderByDescending(item => item.Density).ToList();
        
        // Create the root node
        Node root = new Node { Level = 0, CurrentWeight = 0, CurrentValue = 0 };
        root.UpperBound = Bound(root);
        
        BranchAndBound(root);
        
        outputBuilder.AppendLine($"\nOptimal Solution Found!");
        outputBuilder.AppendLine($"Maximum Value: {bestValue}");
        outputBuilder.AppendLine("Included Items:");
        for (int i = 0; i < bestSolution.Count; i++)
        {
            outputBuilder.AppendLine($"- Item {bestSolution[i] + 1} (Value: {items[bestSolution[i]].Value}, Weight: {items[bestSolution[i]].Weight})");
        }

        return outputBuilder.ToString();
    }

    // Recursive method for the Branch and Bound algorithm
    private void BranchAndBound(Node node)
    {
        if (node.UpperBound <= bestValue)
        {
            outputBuilder.AppendLine($"  Fathoming node at level {node.Level}. Upper bound ({node.UpperBound:F2}) is not better than current best ({bestValue}).");
            return;
        }

        if (node.Level == items.Count)
        {
            if (node.CurrentValue > bestValue)
            {
                bestValue = node.CurrentValue;
                bestSolution = new List<int>(node.IncludedItems);
                outputBuilder.AppendLine($"  New best solution found! Value: {bestValue}");
            }
            return;
        }

        var nextItemIndex = node.Level;
        var nextItem = items[nextItemIndex];
        
        // Branch to include the next item
        if (node.CurrentWeight + nextItem.Weight <= capacity)
        {
            Node includedNode = new Node
            {
                Level = nextItemIndex + 1,
                CurrentWeight = node.CurrentWeight + nextItem.Weight,
                CurrentValue = node.CurrentValue + nextItem.Value,
                IncludedItems = new List<int>(node.IncludedItems)
            };
            includedNode.IncludedItems.Add(nextItemIndex);
            includedNode.UpperBound = Bound(includedNode);
            outputBuilder.AppendLine($"  Branching to include item {nextItemIndex + 1}. Current value: {includedNode.CurrentValue}, upper bound: {includedNode.UpperBound:F2}");
            BranchAndBound(includedNode);
        }

        // Branch to exclude the next item
        Node excludedNode = new Node
        {
            Level = nextItemIndex + 1,
            CurrentWeight = node.CurrentWeight,
            CurrentValue = node.CurrentValue,
            IncludedItems = new List<int>(node.IncludedItems)
        };
        excludedNode.UpperBound = Bound(excludedNode);
        outputBuilder.AppendLine($"  Branching to exclude item {nextItemIndex + 1}. Current value: {excludedNode.CurrentValue}, upper bound: {excludedNode.UpperBound:F2}");
        BranchAndBound(excludedNode);
    }

    // Calculates the upper bound using a greedy approach for the fractional knapsack problem
    private double Bound(Node node)
    {
        double result = node.CurrentValue;
        int j = node.Level;
        int totalWeight = node.CurrentWeight;

        while (j < items.Count && totalWeight + items[j].Weight <= capacity)
        {
            totalWeight += items[j].Weight;
            result += items[j].Value;
            j++;
        }

        if (j < items.Count)
        {
            result += (capacity - totalWeight) * items[j].Density;
        }
        
        return result;
    }
}