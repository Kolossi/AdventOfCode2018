using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Runner
{
    class Day20 :  Day
    {
        public override string First(string input)
        {
            //LogEnabled = false;
            //LogEnabled = true;
            LogEnabled = (input == "^ESSWWN(E|NNENN(EESS(WNSE|)SSS|WWWSSSSE(SW|NNNE)))$");
            input = input.GetLines("^$")[0].Trim();
            //Map<int> map = GetMap(input);
            //Map<int> map = GetMapMultiWalk(input);
            Map<int> map = GetMapRecursive(input);
            Map<int> walkMap = GetWalkDistanceMap(map);
            LogEnabled = true;
            LogLine(ShowState(map));
            LogLine(ShowValues(walkMap));
            int result = GetDistanceValues(walkMap).Max();
            LogLine("{0} ===> {1}", input, result);
            return result.ToString(); // not 3634 too high, 813 too low
        }

        public override string Second(string input)
        {
            throw new NotImplementedException("Second");
        }

        //public override string FirstTest(string input)
        //{
        //    LogEnabled = false;
        //    //return First("^(N|S|E|W)(N|S|E|W)(N|S|E|W)(N|S|E|W)(N|S|E|W)(N|S|E|W)(N|S|E|W)(N|S|E|W)(N|S|E|W)(N|S|E|W)(N|S|E|W)(N|S|E|W)(N|S|E|W)(N|S|E|W)(N|S|E|W)(N|S|E|W)(N|S|E|W)(N|S|E|W)(N|S|E|W)(N|S|E|W)(N|S|E|W)(N|S|E|W)(N|S|E|W)(N|S|E|W)(N|S|E|W)(N|S|E|W)(N|S|E|W)(N|S|E|W)(N|S|E|W)(N|S|E|W)(N|S|E|W)(N|S|E|W)(N|S|E|W)(N|S|E|W)(N|S|E|W)(N|S|E|W)(N|S|E|W)(N|S|E|W)(N|S|E|W)(N|S|E|W)(N|S|E|W)(N|S|E|W)(N|S|E|W)(N|S|E|W)(N|S|E|W)(N|S|E|W)(N|S|E|W)(N|S|E|W)(N|S|E|W)(N|S|E|W)(N|S|E|W)(N|S|E|W)(N|S|E|W)(N|S|E|W)(N|S|E|W)(N|S|E|W)(N|S|E|W)(N|S|E|W)(N|S|E|W)(N|S|E|W)(N|S|E|W)(N|S|E|W)(N|S|E|W)(N|S|E|W)(N|S|E|W)(N|S|E|W)(N|S|E|W)(N|S|E|W)(N|S|E|W)(N|S|E|W)(N|S|E|W)(N|S|E|W)(N|S|E|W)(N|S|E|W)(N|S|E|W)(N|S|E|W)(N|S|E|W)(N|S|E|W)(N|S|E|W)(N|S|E|W)(N|S|E|W)(N|S|E|W)(N|S|E|W)(N|S|E|W)(N|S|E|W)(N|S|E|W)(N|S|E|W)(N|S|E|W)(N|S|E|W)(N|S|E|W)(N|S|E|W)(N|S|E|W)(N|S|E|W)$");
        //    return First("^(N|S|E|W)(N|S|E|W)(N|S|E|W)$");

        //}

        public override string SecondTest(string input)
        {
            throw new NotImplementedException("SecondTest");
        }

        ////////////////////////////////////////////////////////

        #region original

        private Map<int> GetMap(string input)
        {
            Map<int> map = new Map<int>();
            map.Set(0, 0, 0);
            Stack<Walk> branches = new Stack<Walk>();
            var walk = new Walk()
            {
                Distance = 0,
                XY = new XY(0, 0)
            };

            for (int i = 0; i < input.Length; i++)
            {
                var regexChar = input[i];
                switch (regexChar)
                {
                    case 'N':
                    case 'E':
                    case 'S':
                    case 'W':
                        walk.XY = walk.XY.Move(XY.CharToDir[regexChar]);
                        map.Set(walk.XY, (int)Items.Door);
                        walk.XY = walk.XY.Move(XY.CharToDir[regexChar]);
                        walk.Distance++;
                        int currentDistance;
                        if (!map.TryGetValue(walk.XY, out currentDistance) || currentDistance > walk.Distance)
                        {
                            map.Set(walk.XY, walk.Distance);
                        }
                        break;
                    case '(':
                        branches.Push(new Walk(walk));
                        break;
                    case ')':
                        walk = branches.Pop();
                        break;
                    case '|':
                        walk = new Walk(branches.Peek());
                        break;
                    default:
                        break;
                }
                //LogLine(ShowState(map));
            }
            return map;
        }

        #endregion

        private Map<int> GetMapRecursive(string input)
        {
            int pos = 0;
            var allWalkStrings = new List<string>();
            var walksInProgressAtStart = new List<string>() { "" };
            allWalkStrings = WalkMapRecursive(allWalkStrings, walksInProgressAtStart.AsEnumerable(), input, ref pos);
            var map = GetMapFromAllWalks(allWalkStrings);
            return map;
        }

        private Map<int> GetMapFromAllWalks(List<string> allWalkStrings)
        {
            Map<int> map = new Map<int>();
            map.Set(0, 0, 0);
            foreach(var walkString in allWalkStrings)
            {
                var walk = new Walk()
                {
                    Distance = 0,
                    XY = new XY(0, 0)
                };
                foreach (var walkChar in walkString)
                {
                    walk.XY = walk.XY.Move(XY.CharToDir[walkChar]);
                    map.Set(walk.XY, (int)Items.Door);
                    walk.XY = walk.XY.Move(XY.CharToDir[walkChar]);
                    walk.Distance++;
                    int currentDistance;
                    if (!map.TryGetValue(walk.XY, out currentDistance) || currentDistance > walk.Distance)
                    {
                        map.Set(walk.XY, walk.Distance);
                    }
                }
            }
            return map;
        }

        private List<string> WalkMapRecursive(List<string> allWalks, IEnumerable<string> walksInProgressAtStart, string input, ref int pos, int depth = -1)
        {
            depth++;
            var currentWalks = new List<string>(walksInProgressAtStart.Select(w => new string(w.ToArray())));
            var newWalks = new List<string>();
            if (LogEnabled) ShowAllRoutes("Entry", allWalks, newWalks, currentWalks, input, pos, depth);
            var exit = false;
            while (pos < input.Length && !exit)
            {
                var regexChar = input[pos++];
                if (LogEnabled) ShowAllRoutes(string.Format("Before {0}", regexChar), allWalks, newWalks, currentWalks, input, pos - 1, depth);
                switch (regexChar)
                {
                    case ')':
                    case '$':
                        //newWalks = newWalks.Union(currentWalks).ToList();
                        exit = true;
                        break;
                    case '(':
                        //if (LogEnabled) ShowAllRoutes("OpenBracket", allWalks, newWalks, currentWalks, input, pos-1, depth);
                        currentWalks = WalkMapRecursive(allWalks, currentWalks, input, ref pos, depth);
                        break;
                    case '|':
                        //newWalks = newWalks.Union(currentWalks).ToList();
                        //currentWalks = new List<string>(walksInProgressAtStart.Select(w => new string(w.ToArray())));
                        //if (LogEnabled) ShowAllRoutes("pipe", allWalks, newWalks, currentWalks, input, pos-1, depth);
                        var walksAfterPipe = WalkMapRecursive(allWalks, walksInProgressAtStart, input, ref pos, depth);
                        currentWalks = currentWalks.Union(walksAfterPipe).ToList();
                        break;
                    case 'N':
                    case 'E':
                    case 'S':
                    case 'W':
                        currentWalks = currentWalks.Select(w => w + regexChar).ToList();
                        break;

                }
                if (LogEnabled) ShowAllRoutes(string.Format(" After {0}", regexChar), allWalks, newWalks, currentWalks, input, pos - 1, depth);
            }
            newWalks = newWalks.Union(currentWalks).ToList();
            allWalks = allWalks.Union(currentWalks).ToList();
            if (LogEnabled) ShowAllRoutes("Exit", allWalks, newWalks, currentWalks, input, pos-1, depth);
            return newWalks;
        }

        private static void ShowAllRoutes(string message, List<string> allWalks, List<string> newWalks, List<string> currentWalks, string input, int pos, int depth)
        {
            var indent = new string(' ', depth * 4);
            LogLine("{0}{1}", indent, message);
            LogLine("{0}{1}","",input);
            LogLine("{0}{1}^", "",new string(' ', pos));
            LogLine("{0}allWalks={1}", indent,allWalks);
            LogLine("{0}newWalks={1}", indent, newWalks);
            LogLine("{0}currentWalks={1}", indent, currentWalks);
        }

        //private Map<int> GetMapRecursive(string input)
        //{
        //    int pos = 0;
        //    Map<int> map = new Map<int>();
        //    map.Set(0, 0, 0);
        //    var walksInProgressAtStart = new LinkedList<Walk>();
        //    walksInProgressAtStart.AddLast(new Walk()
        //    {
        //        Distance = 0,
        //        XY = new XY(0, 0)
        //    });
        //    WalkMapRecursive(map, walksInProgressAtStart, input, ref pos);

        //    return map;
        //}

        //private void WalkMapRecursive(Map<int> map, LinkedList<Walk> walksInProgressAtStart, string input, ref int pos)
        //{
        //    var currentWalks = new LinkedList<Walk>(walksInProgressAtStart.Select(w => new Walk(w)));

        //    while (pos < input.Length)
        //    {
        //        var regexChar = input[pos++];
        //        switch (regexChar)
        //        {
        //            case ')':
        //            case '$':
        //                return;
        //            case '(':
        //                WalkMapRecursive(map, currentWalks, input, ref pos);
        //                break;
        //            case '|':
        //                WalkMapRecursive(map, walksInProgressAtStart, input, ref pos);
        //                break;
        //            case 'N':
        //            case 'E':
        //            case 'S':
        //            case 'W':
        //                foreach (var walk in currentWalks)
        //                {
        //                    walk.XY = walk.XY.Move(XY.CharToDir[regexChar]);
        //                    map.Set(walk.XY, (int)Items.Door);
        //                    walk.XY = walk.XY.Move(XY.CharToDir[regexChar]);
        //                    walk.Distance++;
        //                    int currentDistance;
        //                    if (!map.TryGetValue(walk.XY, out currentDistance) || currentDistance > walk.Distance)
        //                    {
        //                        map.Set(walk.XY, walk.Distance);
        //                    }
        //                }
        //                break;

        //        }
        //    }

        //}

        //private Map<int> GetMapMultiWalk(string input)
        //{
        //    Map<int> map = new Map<int>();
        //    map.Set(0, 0, 0);
        //    //Stack<MultiWalks> branches = new Stack<MultiWalks>();

        //    var branchRoutes = new LinkedList<Walk>();
        //    var currentRoutes = new LinkedList<Walk>();
        //    var routeStack = new LinkedList<LinkedList<Walk>>();
        //    branchRoutes.AddLast(new Walk()
        //    {
        //        Distance = 0,
        //        XY = new XY(0, 0)
        //    });
        //    currentRoutes.AddLast(new Walk()
        //    {
        //        Distance = 0,
        //        XY = new XY(0, 0)
        //    });

        //    for (int i = 0; i < input.Length; i++)
        //    {
        //        var regexChar = input[i];
        //        LogLine("Char:'{0}'", regexChar);
        //        LogLine(" Before branchRoutes: {0}, currentRoutes: {1}, routeStack: {2}", branchRoutes, currentRoutes, routeStack);
        //        //LogLine("Branches:{0}{1}", Environment.NewLine, string.Join(Environment.NewLine, branches.AsEnumerable()));
        //        switch (regexChar)
        //        {
        //            case 'N':
        //            case 'E':
        //            case 'S':
        //            case 'W':
        //                foreach (var walk in currentRoutes)
        //                {
        //                    walk.XY = walk.XY.Move(XY.CharToDir[regexChar]);
        //                    map.Set(walk.XY, (int)Items.Door);
        //                    walk.XY = walk.XY.Move(XY.CharToDir[regexChar]);
        //                    walk.Distance++;
        //                    int currentDistance;
        //                    if (!map.TryGetValue(walk.XY, out currentDistance) || currentDistance > walk.Distance)
        //                    {
        //                        map.Set(walk.XY, walk.Distance);
        //                    }
        //                }
        //                break;
        //            /// ( : BranchRoutes = clone of currentRoutes

        //            /// | : routestack.push(currentRoutes), currentRoutes=clone of BranchRoutes

        //            /// ) : currentRoutes+=RouteStack.Pop(), BranchRoutes = currentRoutes

        //            case '(':
        //                branchRoutes = new LinkedList<Walk>(currentRoutes.Select(r => new Walk(r)));
        //                LogLine(ShowValues(map));
        //                break;
        //            case ')':
        //                foreach (var newRoute in routeStack.First.Value)
        //                {
        //                    currentRoutes.AddLast(newRoute);
        //                }
        //                routeStack.RemoveFirst();
        //                branchRoutes = new LinkedList<Walk>(currentRoutes.Select(r => new Walk(r)));
        //                LogLine(ShowValues(map));
        //                break;
        //            case '|':
        //                routeStack.AddFirst(new LinkedList<Walk>(currentRoutes.Select(r=>new Walk(r))));
        //                currentRoutes = new LinkedList<Walk>(branchRoutes.Select(r => new Walk(r)));
        //                LogLine(ShowValues(map));
        //                break;
        //            default:
        //                break;
        //        }
        //        //LogLine(ShowState(map));
        //        LogLine("  After branchRoutes: {0}, currentRoutes: {1}, routeStack: {2}", branchRoutes, currentRoutes, routeStack);
        //    }
        //    return map;
        //}

        //private Map<int> GetMapMultiWalkReplacedJan2019(string input)
        //{
        //    Map<int> map = new Map<int>();
        //    map.Set(0, 0, 0);
        //    //Stack<MultiWalks> branches = new Stack<MultiWalks>();

        //    var branchRoutes = new LinkedList<Walk>();
        //    var currentRoutes = new LinkedList<Walk>();
        //    var routeStack = new LinkedList<Walk>();
        //    branchRoutes.AddLast(new Walk()
        //    {
        //        Distance = 0,
        //        XY = new XY(0, 0)
        //    });
        //    currentRoutes.AddLast(new Walk()
        //    {
        //        Distance = 0,
        //        XY = new XY(0, 0)
        //    });

        //    for (int i = 0; i < input.Length; i++)
        //    {
        //        var regexChar = input[i];
        //        LogLine("Before Char:'{0}',  branchRoutes: {1}, currentRoutes: {2}, routeStack: {3}", regexChar,branchRoutes,currentRoutes, routeStack);
        //        //LogLine("Branches:{0}{1}", Environment.NewLine, string.Join(Environment.NewLine, branches.AsEnumerable()));
        //        switch (regexChar)
        //        {
        //            case 'N':
        //            case 'E':
        //            case 'S':
        //            case 'W':
        //                foreach (var walk in currentRoutes)
        //                {
        //                    walk.XY = walk.XY.Move(XY.CharToDir[regexChar]);
        //                    map.Set(walk.XY, (int)Items.Door);
        //                    walk.XY = walk.XY.Move(XY.CharToDir[regexChar]);
        //                    walk.Distance++;
        //                    int currentDistance;
        //                    if (!map.TryGetValue(walk.XY, out currentDistance) || currentDistance > walk.Distance)
        //                    {
        //                        map.Set(walk.XY, walk.Distance);
        //                    }
        //                }
        //                break;
        //            /// ( : BranchRoutes = clone of currentRoutes

        //            /// | : routestack.push(currentRoutes), currentRoutes=clone of BranchRoutes

        //            /// ) : currentRoutes+=RouteStack.Pop(), BranchRoutes = currentRoutes

        //            case '(':
        //                branchRoutes = new LinkedList<Walk>(currentRoutes.Select(r => new Walk(r)));
        //                LogLine(ShowValues(map));
        //                break;
        //            case ')':
        //                currentRoutes.AddLast(routeStack.First.Value);
        //                routeStack.RemoveFirst();
        //                branchRoutes = new LinkedList<Walk>(currentRoutes.Select(r => new Walk(r)));
        //                LogLine(ShowValues(map));
        //                break;
        //            case '|':
        //                foreach (var r in currentRoutes)
        //                {
        //                    routeStack.AddFirst(r);
        //                }
        //                currentRoutes = new LinkedList<Walk>(branchRoutes.Select(r => new Walk(r)));
        //                LogLine(ShowValues(map));
        //                break;
        //            default:
        //                break;
        //        }
        //        //LogLine(ShowState(map));
        //        LogLine(" After Char:'{0}',  branchRoutes: {1}, currentRoutes: {2}, routeStack: {3}", regexChar, branchRoutes, currentRoutes, routeStack);
        //    }
        //    return map;
        //}

        //private Map<int> GetMapMultiWalk_FirstBrokenVersion(string input)
        //{
        //    Map<int> map = new Map<int>();
        //    map.Set(0, 0, 0);
        //    //Stack<MultiWalks> branches = new Stack<MultiWalks>();
        //    var multiWalks = new MultiWalks();
        //    multiWalks.Branches.AddLast(new Walk()
        //    {
        //        Distance = 0,
        //        XY = new XY(0, 0)
        //    });

        //    for (int i = 0; i < input.Length; i++)
        //    {
        //        var regexChar = input[i];
        //        if (LogEnabled)
        //        {
        //            LogLine("Char:'{0}',  multiWalks: {1}", regexChar, multiWalks);
        //            //LogLine("Branches:{0}{1}", Environment.NewLine, string.Join(Environment.NewLine, branches.AsEnumerable()));
        //        }
        //        switch (regexChar)
        //        {
        //            case 'N':
        //            case 'E':
        //            case 'S':
        //            case 'W':
        //                foreach (var startWalk in multiWalks.Branches)
        //                {
        //                    var walk = new Walk(startWalk);
        //                    walk.XY = walk.XY.Move(XY.CharToDir[regexChar]);
        //                    map.Set(walk.XY, (int)Items.Door);
        //                    walk.XY = walk.XY.Move(XY.CharToDir[regexChar]);
        //                    walk.Distance++;
        //                    multiWalks.InProgress.AddLast(walk);
        //                    int currentDistance;
        //                    if (!map.TryGetValue(walk.XY, out currentDistance) || currentDistance > walk.Distance)
        //                    {
        //                        map.Set(walk.XY, walk.Distance);
        //                    }
        //                }
        //                break;
        //            case '(':
        //                //multiWalks = new MultiWalks(multiWalks);
        //                //branches.Push(new MultiWalks(multiWalks));
        //                break;
        //            case ')':
        //                multiWalks.Branches = multiWalks.InProgress;
        //                multiWalks.InProgress = new LinkedList<Walk>();
        //                //multiWalks = branches.Pop();
        //                break;
        //            case '|':
        //                //multiWalks = new MultiWalks(branches.Peek());
        //                break;
        //            default:
        //                break;
        //        }
        //        //LogLine(ShowState(map));
        //    }
        //    return map;
        //}

        //private Map<int> GetMapRecursive(string input)
        //{
        //    Map<int> map = new Map<int>();
        //    map.Set(0, 0, 0);
        //    Stack<Walk> branches = new Stack<Walk>();
        //    var walk = new Walk()
        //    {
        //        Distance = 0,
        //        XY = new XY(0, 0)
        //    };

        //    for (int i = 0; i < input.Length; i++)
        //    {
        //        var regexChar = input[i];
        //        switch (regexChar)
        //        {
        //            case 'N':
        //            case 'E':
        //            case 'S':
        //            case 'W':
        //                walk.XY = walk.XY.Move(XY.CharToDir[regexChar]);
        //                map.Set(walk.XY, (int)Items.Door);
        //                walk.XY = walk.XY.Move(XY.CharToDir[regexChar]);
        //                walk.Distance++;
        //                int currentDistance;
        //                if (!map.TryGetValue(walk.XY, out currentDistance) || currentDistance > walk.Distance)
        //                {
        //                    map.Set(walk.XY, walk.Distance);
        //                }
        //                break;
        //            case '(':
        //                branches.Push(new Walk(walk));
        //                break;
        //            case ')':
        //                walk = branches.Pop();
        //                break;
        //            case '|':
        //                walk = new Walk(branches.Peek());
        //                break;
        //            default:
        //                break;
        //        }
        //        LogLine(ShowState(map));
        //    }
        //    return map;
        //}

        public Map<int> GetWalkDistanceMap(Map<int> originalMap)
        {
            var map = new Map<int>(originalMap);

            foreach (var coord in map.GetAllCoords().ToArray())
            {
                var val = map.Get(coord);
                if (val!=(int)Items.Door && val!=(int)Items.Wall) map.Set(coord, (int)Items.NotVisited);
            }

            var path = new Path();
            path.Move(new XY(0, 0));
            var toProcess = new Queue<Path>();
            toProcess.Enqueue(path);
            while (toProcess.Any())
            {
                path = toProcess.Dequeue();
                var xy = path.XY;
                foreach (var direction in (new List<Direction>((Direction[])Enum.GetValues(typeof(Direction)))).OrderByDescending(d=>d))
                {
                    var newXY = xy.Move(direction);
                    int val;
                    if (!map.TryGetValue(newXY, out val)) continue;
                    if (val != (int)Items.Door) continue;
                    newXY = newXY.Move(direction);
                    if (!map.TryGetValue(newXY, out val)) continue;
                    if (newXY.X == 0 && newXY.Y == 0) continue;
                    if (val <= path.Length + 1) continue;
                    map.Set(newXY, path.Length + 1);
                    toProcess.Enqueue((new Path(path)).Move(newXY));
                }
            }
            return map;

        }

        private IEnumerable<int> GetDistanceValues(Map<int> map)
        {
            var specialValues = new HashSet<int>((int[])Enum.GetValues(typeof(Items)));
            return map.GetAllValues().Where(v => !specialValues.Contains(v));
        }

        public string ShowState(Map<int> map)
        {
            var sb = new StringBuilder();
            for (int y = map.GetMinY()-1; y <= map.GetMaxY()+1; y++)
            {
                for (int x = map.GetMinX()-1; x <= map.GetMaxX()+1; x++)
                {
                    int val;
                    sb.Append(
                            (x == 0 && y == 0) ? "X" :
                            (!map.TryGetValue(x,y, out val) || val==(int)Items.Wall) ? "#" :
                            (val == (int)Items.NotVisited) ? " " :
                            (val==(int)Items.Door) ? "D" :
                            "."
                        );
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }

        public string ShowValues(Map<int> map)
        {
            var sb = new StringBuilder();
            for (int y = map.GetMinY()-1; y <= map.GetMaxY()+1; y++)
            {
                for (int x = map.GetMinX()-1; x <= map.GetMaxX()+1; x++)
                {
                    int val;
                    sb.Append(
                            (x == 0 && y == 0) ? "XXX" :
                            (!map.TryGetValue(x, y, out val) || val == (int)Items.Wall) ? ((x%2==0) ? "###" : "#" ):
                            (val == (int)Items.NotVisited) ? ((x%2==0) ? "   " : " " ) :
                            (val == (int)Items.Door) ? ((x%2==0) ? "---" : "|" ) :
                            string.Format("{0:D3}", val)
                        );
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }

        public enum Items
        {
            Door = int.MaxValue,
            Wall = int.MaxValue-1,
            NotVisited = int.MaxValue-2
        }

        public class MultiWalks
        {
            public LinkedList<Walk> Branches = new LinkedList<Walk>();
            public LinkedList<Walk> InProgress = new LinkedList<Walk>();
            public MultiWalks()
            {

            }

            public MultiWalks(MultiWalks source)
            {
                Branches = new LinkedList<Walk>(source.Branches.Select(w => new Walk(w)));
                InProgress = new LinkedList<Walk>(source.InProgress.Select(w => new Walk(w)));
            }

            public override string ToString()
            {
                return string.Format("Branches:{0} ; InProgress:{1}",
                    string.Join(", ", Branches),
                    string.Join(", ", InProgress));
            }
        }

        public class Walk
        {
            public int Distance;
            public XY XY;

            public Walk()
            {
            }

            public Walk(Walk source)
            {
                Distance = source.Distance;
                XY = source.XY;
            }

            public override string ToString()
            {
                return string.Format("({0}):{1}", Distance, XY);
            }
        }
    }
}
