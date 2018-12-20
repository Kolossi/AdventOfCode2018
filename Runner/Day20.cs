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
            LogEnabled = true;
            input = input.GetLines("^$")[0].Trim();
            Map<int> map = GetMap(input);
            Map<int> walkMap = GetWalkDistanceMap(map);
            LogLine(ShowValues(walkMap));
            return GetDistanceValues(walkMap).Max().ToString(); // not 3634 too high, 813 too low
        }

        public override string Second(string input)
        {
            throw new NotImplementedException("Second");
        }

        //public override string FirstTest(string input)
        //{
        //    throw new NotImplementedException("SecondTest");
        //}

        public override string SecondTest(string input)
        {
            throw new NotImplementedException("SecondTest");
        }

        ////////////////////////////////////////////////////////

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
                        walk = branches.Peek();
                        break;
                    default:
                        break;
                }
            }
            return map;
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
            }

            public Walk(Walk source)
            {
                Distance = source.Distance;
                XY = source.XY;
            }
        }
    }
}
