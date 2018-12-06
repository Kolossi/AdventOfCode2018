using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Runner
{
    class Day06 : Day
    {
        public override string First(string input)
        {
            var coords = GetCoords(input);
            return SolveDangerous(coords);
        }

        public override string Second(string input)
        {
            var coords = GetCoords(input);
            return SolveSafe(coords, 10000);
        }

        public override string SecondTest(string input)
        {
            var coords = GetCoords(input);
            return SolveSafe(coords, 32);
        }

        ////////////////////////////////////////////////////////

        private static IEnumerable<XY> GetCoords(string input)
        {
            return input.GetLines(",")
                .Select(l => GetParts(l))
                .Select(p => new XY(int.Parse(p[0]), int.Parse(p[1])));
        }

        private string SolveDangerous(IEnumerable<XY> coords)
        {
            var minX = coords.Min(c => c.X);
            var minY = coords.Min(c => c.Y);
            var maxX = coords.Max(c => c.X);
            var maxY = coords.Max(c => c.Y);
            var noninfinite = coords.Where(c => c.X > minX && c.X < maxX && c.Y > minY && c.Y < maxY);
            var areas = new Dictionary<XY, int>();
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    var xy = new XY(x, y);
                    Dictionary<XY, int> dists = GetDists(coords, xy);
                    var lowest = dists.Values.Min();
                    var mins = dists.Keys.Where(k => dists[k] == lowest);
                    var clash = mins.Count() > 1;
                    if (!clash)
                    {
                        var winner = mins.First();
                        if (noninfinite.Contains(winner))
                        {
                            AddScore(areas, winner, 1);
                        }
                    }
                }
            }
            var max = areas.Values.Max();
            return max.ToString();
        }

        private string SolveSafe(IEnumerable<XY> coords, int threshold)
        {
            var minX = coords.Min(c => c.X);
            var minY = coords.Min(c => c.Y);
            var maxX = coords.Max(c => c.X);
            var maxY = coords.Max(c => c.Y);
            var areasize = 0;
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    var xy = new XY(x, y);
                    Dictionary<XY, int> dists = GetDists(coords, xy);
                    var score = dists.Values.Sum();
                    if (score < threshold)
                    {
                        areasize++;
                    }
                }
            }
            return areasize.ToString();
        }

        private Dictionary<XY, int> GetDists(IEnumerable<XY> coords, XY xy)
        {
            var dists = new Dictionary<XY, int>();
            foreach (var c in coords)
            {
                var dist = Dist(c, xy);
                dists[c] = dist;
            }

            return dists;
        }

        private int Dist(XY a, XY b)
        {
            return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
        }

        public void AddScore(Dictionary<XY, int> dict, XY xy, int score)
        {
            int current;
            if (!dict.TryGetValue(xy, out current))
            {
                current = 0;
            }
            dict[xy] = current + score;
        }


    }
}
