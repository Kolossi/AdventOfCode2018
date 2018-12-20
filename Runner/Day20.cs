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
            input = input.GetLines("^$")[0];
            Map<long> map = GetMap(input);
            return map.GetAllValues().Max().ToString();
        }

        public override string Second(string input)
        {
            throw new NotImplementedException("Second");
        }

        public override string SecondTest(string input)
        {
            throw new NotImplementedException("SecondTest");
        }

        ////////////////////////////////////////////////////////
    
         private Map<long> GetMap(string input)
        {
            Map<long> map = new Map<long>();
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
                        walk.Move(XY.CharToDir[regexChar]);
                        long currentDistance;
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

            public Walk Move(Direction dir)
            {
                XY = XY.Move(dir);
                Distance++;
                return this;
            }
        }
    }
}
