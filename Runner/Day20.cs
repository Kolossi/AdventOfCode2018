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
            LogEnabled = false;
            input = input.GetLines("^$")[0].Trim();
            Map<int> map = GetMapRecursive(input);
            Map<int> walkMap = GetWalkDistanceMap(map);
            //LogEnabled = true;
            //LogLine(ShowState(map));
            //LogLine(ShowValues(walkMap));
            int result = GetDistanceValues(walkMap).Max();
            return result.ToString(); // 3633
        }

        public override string Second(string input)
        {
            LogEnabled = false;
            input = input.GetLines("^$")[0].Trim();
            Map<int> map = GetMapRecursive(input);
            Map<int> walkMap = GetWalkDistanceMap(map);
            int result = GetDistanceValues(walkMap).Count(i=>i>=1000);
            return result.ToString();
        }

        public override string SecondTest(string input)
        {
            throw new NotImplementedException("SecondTest");
        }

        ////////////////////////////////////////////////////////

        private Map<int> GetMapRecursive(string input)
        {
            int pos = 0;

            Map<int> map = new Map<int>();
            map.Set(0, 0, 0);
            var startWalk = new LinkedList<Walk>();
            startWalk.AddLast(new Walk() { XY = new XY(0, 0) });
            var allWalkStrings = WalkMapRecursive(map, startWalk, input, ref pos).Select(w=>string.Join("",w));
            return map;
        }

        private LinkedList<Walk> WalkMapRecursive(Map<int> map, LinkedList<Walk> walksOnEntry, string input, ref int pos, int depth = -1)
        {
            depth++;

            LinkedList<Walk> currentWalks = new LinkedList<Walk>(walksOnEntry.Select(w => new Walk(w)));
            LinkedList<Walk> wholeTermWalks = new LinkedList<Walk>();

            var exit = false;
            bool lastWalkEmpty = true;
            while (pos < input.Length && !exit)
            {
                var regexChar = input[pos++];
                switch (regexChar)
                {
                    case ')':
                    case '$':
                        exit = true;
                        break;
                    case '(':
                        var subWalks = WalkMapRecursive(map, currentWalks, input, ref pos, depth);
                        if (subWalks.Any()) currentWalks = NewMergedLL(currentWalks, subWalks);
                        break;
                    case '|':
                        wholeTermWalks = NewMergedLL(wholeTermWalks, currentWalks);
                        currentWalks = new LinkedList<Walk>(walksOnEntry.Select(w => new Walk(w)));
                        lastWalkEmpty = true;
                        break;
                    case 'N':
                    case 'E':
                    case 'S':
                    case 'W':
                        lastWalkEmpty = false;
                        foreach (var walk in currentWalks)
                        {
                            walk.XY = walk.XY.Move(XY.CharToDir[regexChar]);
                            map.Set(walk.XY, (int)Items.Door);
                            walk.XY = walk.XY.Move(XY.CharToDir[regexChar]);
                            walk.Distance++;
                            int currentDistance;
                            if (!map.TryGetValue(walk.XY, out currentDistance) || currentDistance > walk.Distance)
                            {
                                map.Set(walk.XY, walk.Distance);
                            }
                        }
                        break;

                }
            }
            var rv = lastWalkEmpty ? new LinkedList<Walk>() : NewMergedLL(wholeTermWalks, currentWalks);
            return rv;
        }


        private LinkedList<T> NewMergedLL<T>(LinkedList<T> left, IEnumerable<T> right)
        {
            var newList = new LinkedList<T>(left);
            foreach (var i in right)
            {
                newList.AddLast(i);
            }
            return newList;
        }

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

        public class Walk
        {
            public int Distance;
            public XY XY;

            public Walk()
            {
                XY = new XY(0, 0);
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
