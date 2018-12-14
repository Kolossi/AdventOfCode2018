using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Collections;

namespace Runner
{
    class Day14 :  Day
    {
        public override string First(string input)
        {
            LogEnabled = false;
            return Solve1(int.Parse(input));
        }

        public override string Second(string input)
        {
            LogEnabled = false;
            return Solve2(input);
        }
        
        ////////////////////////////////////////////////////////

        public string Solve1(int target)
        {
            var recipes=new byte[target+11];
            recipes[0] = 3;
            recipes[1] = 7;
            var elf1Index = 0;
            var elf2Index = 1;
            int recipeCount = 2;
            do
            {
                if (LogEnabled) ShowState(recipes, recipeCount, elf1Index, elf2Index);
                var elf1Value = recipes[elf1Index];
                var elf2Value = recipes[elf2Index];
                var sum = elf1Value + elf2Value;
                if (sum >= 10) recipes[recipeCount++] = (byte)(sum / 10);
                recipes[recipeCount++] = (byte)(sum % 10);
                elf1Index = (elf1Index + 1 + elf1Value) % recipeCount;
                elf2Index = (elf2Index + 1 + elf2Value) % recipeCount;
                
            } while (recipeCount<target+10);

            var sb = new StringBuilder();
            for (int i = target; i < target+10; i++)
            {
                sb.Append(recipes[i]);
            }
            return sb.ToString();
        }

        public string Solve2(string target)
        {
            var targetWindow = new LinkedList<byte>(target.Select(t => byte.Parse(t.ToString())));
            var currentWindow = new LinkedList<byte>(Enumerable.Repeat((byte)0, target.ToString().Length));
            var recipes = new LinkedList<byte>();
            var elf1Node = recipes.AddLast(3);
            var elf2Node = recipes.AddLast(7);
            do
            {
                if (LogEnabled) ShowState(targetWindow,currentWindow, recipes, elf1Node, elf2Node);
                var sum = elf1Node.Value + elf2Node.Value;
                if (sum >= 10)
                {
                    var val1 = (byte)(sum / 10);
                    recipes.AddLast(val1);
                    currentWindow.RemoveFirst();
                    currentWindow.AddLast(val1);
                    if (Compare(currentWindow, targetWindow)) break;
                }
                var val2 = (byte)(sum % 10);
                recipes.AddLast(val2);
                currentWindow.RemoveFirst();
                currentWindow.AddLast(val2);
                if (Compare(currentWindow, targetWindow)) break;
                elf1Node = Advance(elf1Node, 1 + elf1Node.Value);
                elf2Node = Advance(elf2Node, 1 + elf2Node.Value);
            } while (true);

            return (recipes.Count - targetWindow.Count).ToString();
        }

        private LinkedListNode<byte> Advance(LinkedListNode<byte> node, long count)
        {
            for (int i = 0; i < count; i++)
            {
                node = node.Next ?? node.List.First;
            }
            return node;
        }

        private bool Compare(LinkedList<byte> l, LinkedList<byte> r)
        {
            if (l.Count != r.Count) return false;
            var lVal = l.First;
            var rVal = r.First;
            for (int i = 0; i < l.Count; i++)
            {
                if (lVal.Value != rVal.Value) return false;
                lVal = lVal.Next;
                rVal = rVal.Next;
            }
            return true;
        }

        private void ShowState(LinkedList<byte> targetWindow, LinkedList<byte> currentWindow, LinkedList<byte> recipes, LinkedListNode<byte> elf1Node, LinkedListNode<byte> elf2Node)
        {
            var sb = new StringBuilder();
            Log(currentWindow);
            Log("=");
            Log(targetWindow);
            Log("?:");
            ShowState(recipes, elf1Node, elf2Node);
        }

        private void ShowState(LinkedList<byte> recipes, LinkedListNode<byte> elf1Node, LinkedListNode<byte> elf2Node)
        {
            var sb = new StringBuilder();
            var node = recipes.First;
            do
            {
                sb.Append(node == elf1Node ? "(" : (node == elf2Node ? "[" : " "));
                sb.Append(node.Value);
                sb.Append(node == elf1Node ? ")" : (node == elf2Node ? "]" : " "));
                node = node.Next;
            } while (node!=null);
            LogLine(sb.ToString());
        }

        private void ShowState(LinkedList<byte> targetWindow, LinkedList<byte> currentWindow, byte[] recipes, int recipeCount, int elf1Index, int elf2Index)
        {
            var sb = new StringBuilder();
            Log(currentWindow);
            Log("=");
            Log(targetWindow);
            Log("?:");
            ShowState(recipes, recipeCount, elf1Index, elf2Index);
        }

        private void ShowState(byte[] recipes, int recipeCount, int elf1Index, int elf2Index)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < recipeCount; i++)
            {
                sb.Append(i == elf1Index ? "(" : (i == elf2Index ? "[" : " "));
                sb.Append(recipes[i]);
                sb.Append(i == elf1Index ? ")" : (i == elf2Index ? "]" : " "));
            }
            LogLine(sb.ToString());
        }
    }
}
