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
            //var allNodes = Parse(input.GetParts());
            var allNodes = NonRecursiveBrokenParse(input.GetParts());
            return allNodes.SelectMany(n => n.MetaData).Sum().ToString();
        }

        public override string Second(string input)
        {
            var allNodes = Parse(input.GetParts());
            //var allNodes = NonRecursiveBrokenParse(input.GetParts());
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
                return string.Format("{0}:meta[{1}]={2}, children[{3}]={4}",
                                        Id<=26 ? ((char)(Id+64)).ToString() : string.Format("[{0}]",Id),
                                        MetaCount,
                                        string.Join(",", MetaData),
                                        ChildCount,
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
                    Id = allNodes.Count + 1,
                    Parent = node           
                };
                allNodes.Add(child);
                node.Children.Add(child);
                ProcessNode(child, dataQueue, allNodes);
            }
            TakeMeta(dataQueue, node);
        }

        private void TakeHeader(Queue<int> dataQueue, Node node)
        {
            node.ChildCount = dataQueue.Dequeue();
            node.MetaCount = dataQueue.Dequeue();
            LogLine("Head: {0}", node);
        }

        private void TakeMeta(Queue<int> dataQueue, Node node)
        {
            for (int i = 0; i < node.MetaCount; i++)
            {
                node.MetaData.Add(dataQueue.Dequeue());
            }
            LogLine("Meta: {0}", node);
        }

        private List<Node> NonRecursiveBrokenParse(string[] data)
        {
            var dataQueue = new Queue<int>(data.Select(d => int.Parse(d)));
            var pendingHeader = new Queue<Node>();
            var pendingMeta = new List<Node>();
            List<Node> allNodes = new List<Node>();

            var root = new Node() { Id = 1 };
            pendingHeader.Enqueue(root);
            allNodes.Add(root);

            do
            {
                LogLine("Pending Headers: [{0}]", pendingHeader.Select(i => i.Id<=26 ? ((char)(i.Id+64)).ToString() : string.Format("[{0}]",i.Id)));
                LogLine("Pending Meta   : [{0}]", pendingMeta.Select(i => i.Id<=26 ? ((char)(i.Id+64)).ToString() : string.Format("[{0}]",i.Id)));
                if (pendingHeader.Any())
                {
                    var node = pendingHeader.Dequeue();
                    TakeHeader(dataQueue, node);
                    if (node.ChildCount > 0)
                    {
                        for (int i = 0; i < node.ChildCount; i++)
                        {
                            var child = new Node()
                            {
                                Id = allNodes.Count + 1,
                                Parent = node
                            };
                            pendingHeader.Enqueue(child);
                            node.Children.Add(child);
                            allNodes.Add(child);
                        }

                        pendingMeta.Insert(0, node);
                        //pendingMeta = node.Children.Union(pendingMeta).ToList();
                    }
                    else
                    {
                        TakeMeta(dataQueue, node);
                    }
                }
                //else???
                else if (pendingMeta.Any())
                {
                    var node = pendingMeta[0];
                    pendingMeta.RemoveAt(0);
                    TakeMeta(dataQueue, node);
                }
            } while (pendingHeader.Any() || pendingMeta.Any());

            if (dataQueue.Any()) throw new InvalidOperationException();
            if (allNodes.Any(n => n.MetaData.Count() != n.MetaCount)) throw new InvalidOperationException();
            if (allNodes.Any(n => n.Children.Count() != n.ChildCount)) throw new InvalidOperationException();

            return allNodes;
        }
    }
}
