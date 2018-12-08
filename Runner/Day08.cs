using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Runner
{
    class Day08 :  Day
    {
        public override string First(string input)
        {
            var allNodes = Parse(input.GetParts());
            return allNodes.SelectMany(n => n.MetaData).Sum().ToString();
        }

        public override string Second(string input)
        {
            var allNodes = Parse(input.GetParts());
            var root = allNodes[0];
            return root.GetValue().ToString();
        }

        ////////////////////////////////////////////////////////
    
        public class Node
        {
            public int Id;
            public int ChildCount;
            public List<Node> Children = new List<Node>();
            public Node Parent;
            public int MetaCount;
            public List<int> MetaData = new List<int>();

            public int GetValue()
            {
                if (ChildCount==0)
                {
                    return MetaData.Sum();
                }
                else
                {
                    return MetaData.Sum(m => (m == 0 || m > ChildCount) ? 0 : Children[m - 1].GetValue());
                }
            }


            public override string ToString()
            {
                return string.Format("{0}:meta[{1}]={2}, children={3}]",
                                        (char)(Id+64),
                                        MetaCount,
                                        string.Join(",", MetaData),
                                        string.Join(",", Children.Select(c => c.Id))
                                    );
            }
        }


        private List<Node> Parse(string[] data)
        {
            var dataQueue = new Queue<int>(data.Select(d=>int.Parse(d)));
            List<Node> allNodes = new List<Node>();

            var root = new Node() { Id = 1 };
            allNodes.Add(root);
            
            ProcessNode(root, dataQueue, allNodes);

            return allNodes;
        }

        private void ProcessNode(Node node, Queue<int> dataQueue,List<Node> allNodes)
        {
            TakeHeader(dataQueue, node);
            for (int i = 0; i < node.ChildCount; i++)
            {
                var child = new Node()
                {
                    Parent = node,
                    Id = (char)(allNodes.Count)
                };
                allNodes.Add(child);
                node.Children.Add(child);
                ProcessNode(child, dataQueue, allNodes);
            }
            TakeMeta(dataQueue, node);
        }

        private static void TakeHeader(Queue<int> dataQueue, Node node)
        {
            node.ChildCount = dataQueue.Dequeue();
            node.MetaCount = dataQueue.Dequeue();
        }

        private static void TakeMeta(Queue<int> dataQueue, Node node)
        {
            for (int i = 0; i < node.MetaCount; i++)
            {
                node.MetaData.Add(dataQueue.Dequeue());
            }
        }

        //private string NonRecursiveBrokenParse(string[] data)
        //{
        //    var dataQueue = new Queue<int>(data.Select(d=>int.Parse(d)));
        //    var pendingHeader = new Queue<Node>();
        //    var pendingMeta = new List<Node>();
        //    List<Node> allNodes = new List<Node>();

        //    var root = new Node() { Id = 1 };
        //    pendingHeader.Enqueue(root);
        //    allNodes.Add(root);

        //    do
        //    {
        //        if (pendingHeader.Any())
        //        {
        //            var node = pendingHeader.Dequeue();
        //            TakeHeader(dataQueue, node);
        //            if (node.ChildCount > 0)
        //            {
        //                for (int i = 0; i < node.ChildCount; i++)
        //                {
        //                    var child = new Node()
        //                    {
        //                        Id = allNodes.Count,
        //                        Parent = node
        //                    };
        //                    pendingHeader.Enqueue(child);
        //                    node.Children.Add(child);
        //                    allNodes.Add(child);
        //                }

        //                pendingMeta.Insert(0, node);
        //                //pendingMeta = node.Children.Union(pendingMeta).ToList();
        //            }
        //            else
        //            {
        //                TakeMeta(dataQueue, node);
        //            }
        //        }
        //        //else???
        //        if (pendingMeta.Any())
        //        {
        //            var node = pendingMeta[0];
        //            pendingMeta.RemoveAt(0);
        //            TakeMeta(dataQueue, node);
        //        }
        //    } while (pendingHeader.Any() || pendingMeta.Any());

        //    if (dataQueue.Any()) throw new InvalidOperationException();
        //    if (allNodes.Any(n => n.MetaData.Count() != n.MetaCount)) throw new InvalidOperationException();
        //    if (allNodes.Any(n => n.Children.Count() != n.ChildCount)) throw new InvalidOperationException();

        //    return allNodes.SelectMany(n => n.MetaData).Sum().ToString();
        //}
    }
}
