using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Runner
{
    class Day25 :  Day
    {
        public static int TestNum = 1;
        public override string First(string input)
        {
            var stars = input.GetLines().Select(l => new XYZT(l));
            var constellations = Constelate(stars);
            //ShowContstellations(constellations);
            return constellations.Count().ToString(); // not 616 too high
        }

        public override string FirstTest(string input)
        {
            var stars = input.GetLines().Select(l => new XYZT(l));
            //if (TestNum++ != 3) return "XXX";
            var constellations = Constelate(stars);
            //LogLine();
            //LogLine("--===--");
            //LogLine();
            //ShowContstellations(constellations);
            //LogLine();
            //LogLine("--===--");
            //LogLine();
            return constellations.Count().ToString(); // not 616 too high
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
 
        private LinkedList<List<XYZT>> Constelate(IEnumerable<XYZT> stars)
        {
            var constellations = new LinkedList<List<XYZT>>(stars.Select(s => new List<XYZT>() { s }));

            bool changed;
            do
            {
                changed = false;
                var newConstellations = new LinkedList<List<XYZT>>();
                var toProcess = new LinkedList<List<XYZT>>(constellations);
                while (toProcess.Any())
                {
                    var left = toProcess.First();
                    toProcess.RemoveFirst();
                    var others = new LinkedList<List<XYZT>>(toProcess.Where(c => c != left));
                    while (others.Any())
                    {
                        var right = others.First();
                        others.RemoveFirst();

                        if (left.Any(ls=>right.Any(rs=>ls.DistanceTo(rs)<=3)))
                        {
                            left = left.Union(right).ToList();
                            others.Remove(right);
                            toProcess.Remove(right);
                            changed = true;
                        }
                    }
                    newConstellations.AddFirst(left);
                }
                constellations = newConstellations;
            } while (changed);
            return constellations;
        }

        private void ShowContstellations(LinkedList<List<XYZT>> constellations)
        {
            foreach (var constellation in constellations)
            {
                LogLine(constellation);
            }
            //var stars = constellations.SelectMany(c => c).ToArray();
            //foreach (var left in stars)
            //{
            //    LogLine("{0} distances:", left);
            //    foreach (var right in stars)
            //    {
            //        LogLine("    {0} - {1}", right, left.DistanceTo(right));
            //    }
            //}
        }

        public class XYZT
        {
            public int X;
            public int Y;
            public int Z;
            public int T;

            public XYZT(string line)
            {
                var parts = line.GetParts(",");
                X = int.Parse(parts[0]);
                Y = int.Parse(parts[1]);
                Z = int.Parse(parts[2]);
                T = int.Parse(parts[3]);
            }

            public int DistanceTo(XYZT right)
            {
                return Math.Abs(X - right.X) + Math.Abs(Y - right.Y) + Math.Abs(Z - right.Z) + Math.Abs(T - right.T);
            }

            public override string ToString()
            {
                return string.Format("{{{0},{1},{2},{3}}}", X, Y, Z, T);
            }
        }
    }
}
