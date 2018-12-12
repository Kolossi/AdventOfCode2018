using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Runner
{
    class Day12 :  Day
    {
        public override string First(string input)
        {
            var lines = input.GetLines("initial state:=>");
            HashSet<int> plants = GetPlants(lines[0].Trim());
            Dictionary<string, bool> rules = GetRules(lines.Skip(1));
            HashSet<int> finalPlants = PlantsAfter(plants, rules, 20);
            return finalPlants.Sum().ToString();
        }

        public override string Second(string input)
        {
            LogEnabled = false;
            var lines = input.GetLines("initial state:=>");
            HashSet<int> plants = GetPlants(lines[0].Trim());
            Dictionary<string, bool> rules = GetRules(lines.Skip(1));
            HashSet<int> finalPlants = PlantsAfter(plants, rules, 100000); //10 seconds
            //            HashSet<int> finalPlants=PlantsAfter(plants, rules, 50000000000);
            return finalPlants.Sum().ToString();
        }

        //public override string Second(string input)
        //{
        //    //method by looking at part 1 iters and spotting the pattern:
        //    // it's not a cheat method as 50bn iters is never going to be realistic
        //    //UInt128? answer = 5145 + (50000000 - 89) * 50;
        //}
        // 89=5145:...................#..#..#..#..#..#..#....#..#....#....#..#....#..#..#..#..#..#..#....#....#..#..#..#.. ...
        // 90=5195:....................#..#..#..#..#..#..#....#..#....#....#..#....#..#..#..#..#..#..#....#....#..#..#..#. ...
        // 91=5245:.....................#..#..#..#..#..#..#....#..#....#....#..#....#..#..#..#..#..#..#....#....#..#..#..# ...
        //  etc

        public override string SecondTest(string input)
        {
            throw new NotImplementedException("SecondTest");
        }

        ////////////////////////////////////////////////////////

        #region first pass solution

        private HashSet<int> GetPlants(string state)
        {
            var plants = new HashSet<int>();
            for (int p = 0; p < state.Length; p++)
            {
                if (state[p] == '#') plants.Add(p);
            }
            return plants;
        }

        private Dictionary<string, bool> GetRules(IEnumerable<string> lines)
        {
            var rules = new Dictionary<string, bool>();
            foreach (var line in lines)
            {
                var parts = line.GetParts();
                rules[parts[0]] = parts[1] == "#" ? true : false;
            }
            return rules;
        }

        private HashSet<int> PlantsAfter(HashSet<int> plants, Dictionary<string, bool> rules, int numGens)
        {
            HashSet<int> newPlants = null;
            if (LogEnabled) LogLine(ShowState(plants, -2, 32));
            for (int gen = 1; gen <= numGens; gen++)
            {
                newPlants = new HashSet<int>();
                for (int plant = plants.Min() - 2; plant <= plants.Max() + 2; plant++)
                {
                    var pattern = GetPattern(plants, plant);
                    bool isPlant;
                    if (rules.TryGetValue(pattern, out isPlant) && isPlant) newPlants.Add(plant);
                }
                plants = newPlants;
                if (LogEnabled) LogLine(string.Format("{0}={1}:{2}", gen, plants.Sum(), ShowState(plants, -2, 100)));
            }
            return newPlants;
        }

        private string ShowState(HashSet<int> plants, int startPos, int endPos)
        {
            var sb = new StringBuilder();
            for (int p = startPos; p <= endPos; p++)
            {
                sb.Append(plants.Contains(p) ? "#" : ".");
            }
            return sb.ToString();
        }

        private string GetPattern(HashSet<int> plants, int plant)
        {
            string pattern = (plants.Contains(plant - 2) ? "#" : ".")
                + (plants.Contains(plant - 1) ? "#" : ".")
                + (plants.Contains(plant) ? "#" : ".")
                + (plants.Contains(plant + 1) ? "#" : ".")
                + (plants.Contains(plant + 2) ? "#" : ".");
            return pattern;
        }
        #endregion


        #region custom collections
        // having fun trying to best optimise
        // turns out (5x faster)

        //public override string First(string input)
        //{
        //    // insanely optimised
        //    var lines = input.GetLines("initial state:=>");
        //    var plants = GetPlantsList(lines[0].Trim());
        //    RuleTree rules = GetRulesTree(lines.Skip(1));
        //    long result = FindResultFromTree(plants, rules, 20);
        //    return result.ToString();
        //}

        //public override string Second(string input)
        //{
        //    // insanely optimised
        //    LogEnabled = false;
        //    var lines = input.GetLines("initial state:=>");
        //    var plants = GetPlantsList(lines[0].Trim());
        //    RuleTree rules = GetRulesTree(lines.Skip(1));
        //    long result = FindResultFromTree(plants, rules, 100000); // 2 seconds
        //    return result.ToString();
        //}

        //private PlantList GetPlantsList(string state)
        //{
        //    var plants = new List<bool>();
        //    for (int p = 0; p < state.Length; p++)
        //    {
        //        plants.Add(state[p] == '#');
        //    }
        //    return new PlantList(plants, 0);
        //}

        //private RuleTree GetRulesTree(IEnumerable<string> lines)
        //{
        //    var tree = new RuleTree();
        //    foreach (var line in lines)
        //    {
        //        var parts = line.GetParts();
        //        tree.AddRule(parts[0].Select(b => b == '#' ? true : false), parts[1] == "#" ? true : false);
        //    }

        //    return tree;
        //}

        //private long FindResultFromTree(PlantList plants, RuleTree rules, int generations)
        //{
        //    for (int g = 0; g < generations; g++)
        //    {
        //        plants.Shrink();
        //        if (LogEnabled) LogLine(plants.ToString());


        //        var window = new Window(5);

        //        ValueNode plantNode = plants.First;
        //        for (int i = 0; i < 3; i++)
        //        {
        //            window.AddAtEnd(plantNode.Value);
        //            plantNode = plantNode.Right;
        //        }
        //        plantNode = plantNode.Left.Left;

        //        PlantList newPlants = new PlantList(plants.StartIndex);

        //        for (long i = plants.StartIndex; i <= plants.Length+plants.StartIndex-2; i++)
        //        {
        //            newPlants.Append(rules.MatchWindow(window));
        //            plantNode = plantNode.Right;
        //            window.AddAtEnd((plantNode==null || plantNode.Right==null) ? false :plantNode.Right.Value);
        //        }

        //        plants = newPlants;
        //    }
        //    LogLine("Final:");
        //    if (LogEnabled) LogLine(plants.ToString());
        //    return plants.Score();
        //}

        //public class Window
        //{
        //    public ValueNode First;
        //    public ValueNode Last;
        //    public long Length;

        //    public Window(int capacity)
        //    {
        //        First = new ValueNode();
        //        var prev = First;
        //        for (int i = 1; i < capacity; i++)
        //        {
        //            var node = new ValueNode();
        //            node.Left = prev;
        //            prev.Right = node;
        //            prev = node;
        //        }
        //        Last = prev;
        //        Length = capacity;
        //    }

        //    public ValueNode AddAtEnd(bool v)
        //    {
        //        var newNode = new ValueNode() { Value = v };
        //        newNode.Left = Last;
        //        Last.Right = newNode;
        //        Last = newNode;
        //        First = First.Right;
        //        First.Left.Right = null;
        //        First.Left = null;
        //        return newNode;
        //    }

        //    public override string ToString()
        //    {
        //        var sb = new StringBuilder();
        //        var node = First;
        //        for (int i = 0; i < Length; i++)
        //        {
        //            sb.Append(node.Value ? "#" : ".");
        //            node = node.Right;
        //        }

        //        return sb.ToString();
        //    }
        //}

        //public class PlantList
        //{
        //    public ValueNode First;
        //    public ValueNode Last;
        //    public long StartIndex = 0;
        //    public long Length;

        //    public PlantList(IEnumerable<bool> vals, long startIndex)
        //    {
        //        StartIndex = startIndex;
        //        ValueNode prev = null;
        //        foreach (var val in vals)
        //        {
        //            var node = new ValueNode() { Value = val };
        //            if (First == null)
        //            {
        //                First = node;
        //            }
        //            else
        //            {
        //                prev.Right = node;
        //                node.Left = prev;
        //            }
        //            prev = node;
        //            Last = node;
        //            Length++;
        //        }
        //    }

        //    public PlantList(long startIndex)
        //    {
        //        StartIndex = startIndex;
        //        Length = 0;
        //    }

        //    public long Score()
        //    {
        //        var node = First;
        //        long score = 0;
        //        for (int i = 0; i < Length; i++)
        //        {
        //            if (node.Value) score = score + i + StartIndex;
        //            node = node.Right;
        //        }

        //        return score;
        //    }
        //    public PlantList Shrink()
        //    {
        //        while (First.Value == false)
        //        {
        //            First = First.Right;
        //            First.Left.Right = null;
        //            First.Left = null;
        //            StartIndex++;
        //            Length--;
        //        }

        //        while (Last.Value == false)
        //        {
        //            Last = Last.Left;
        //            Last.Right.Left = null;
        //            Last.Right = null;
        //            Length--;
        //        }

        //        Prepend(false);
        //        Prepend(false);
        //        Append(false);
        //        Append(false);

        //        return this;
        //    }

        //    public PlantList Prepend(bool value)
        //    {
        //        var node = new ValueNode()
        //        {
        //            Value = value,
        //            Right = First
        //        };

        //        if (First != null) First.Left = node;

        //        First = node;

        //        if (Last == null) Last = node;

        //        Length += 1;
        //        StartIndex -= 1;
        //        return this;
        //    }

        //    public PlantList Append(bool value)
        //    {
        //        var node = new ValueNode()
        //        {
        //            Value = value,
        //            Left = Last
        //        };

        //        if (Last != null) Last.Right = node;

        //        Last = node;

        //        if (First == null) First = node;

        //        Length += 1;
        //        return this;
        //    }

        //    public override string ToString()
        //    {
        //        var sb = new StringBuilder();
        //        sb.AppendFormat("{0}:", StartIndex);
        //        var node = First;
        //        do
        //        {
        //            sb.Append(node.Value ? "#" : ".");
        //            node = node.Right;
        //        } while (node != null);
        //        return sb.ToString();
        //    }

        //}
        //public class RuleTree
        //{
        //    public ValueNode Root = new ValueNode();

        //    public bool MatchWindow(Window window)
        //    {
        //        var valueNode = window.First;
        //        var ruleNode = Root;
        //        do
        //        {
        //            ruleNode = valueNode.Value ? ruleNode.Right : ruleNode.Left;
        //            if (ruleNode == null) return false;
        //            if (ruleNode.Value) return true;
        //            valueNode = valueNode.Right;

        //        } while (valueNode != null);
        //        return false;
        //    }

        //    public ValueNode AddRule(IEnumerable<bool> ruleVals, bool value)
        //    {
        //        ValueNode node = GetNode(ruleVals);
        //        node.Value = value;
        //        return node;
        //    }

        //    public ValueNode GetNode(IEnumerable<bool> ruleVals)
        //    {
        //        var node = Root;
        //        foreach (var ruleVal in ruleVals)
        //        {
        //            var nextNode = ruleVal ? node.Right : node.Left;
        //            if (nextNode == null)
        //            {
        //                nextNode = new ValueNode();
        //                if (ruleVal) node.Right = nextNode;
        //                else node.Left = nextNode;
        //            }
        //            node = nextNode;
        //        }

        //        return node;
        //    }

        //    public override string ToString()
        //    {
        //        var sb = new StringBuilder();
        //        var node = Root;
        //        AppendNodeValue(sb, "", node);
        //        return sb.ToString();
        //    }

        //    private void AppendNodeValue(StringBuilder sb, string prefix, ValueNode node)
        //    {
        //        if (node.Left == null && node.Right == null) {
        //            sb.Append(prefix).Append(" ==> ").AppendLine(node.Value ? "#" : ".");
        //            return;
        //        }
        //        if (node.Left != null)
        //        {
        //            AppendNodeValue(sb, prefix+".", node.Left);
        //        }
        //        if (node.Right != null)
        //        {
        //            AppendNodeValue(sb, prefix+"#", node.Right);
        //        }

        //    }
        //}

        //public class ValueNode
        //{
        //    public bool Value;
        //    public ValueNode Left;
        //    public ValueNode Right;
        //}

        #endregion
    }
}
