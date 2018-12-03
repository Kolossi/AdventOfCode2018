using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Runner
{
    class Day03 :  Day
    {
        public override string First(string input)
        {
            var data = input.GetLines("#@,;x");
            return FindOverlaps(data);
            
        }

        public override string Second(string input)
        {
            var data = input.GetLines("#@,;x");
            return FindNonOverlapId(data);
        }


        ////////////////////////////////////////////////////////

        private Dictionary<XY,List<int>> GetMap(string[] data)
        {
            int id;
            List<int> idList;
            var map = new Dictionary<XY,List<int>>();
            foreach (var line in data)
            {
                var coords = GetCoords(line, out id);
                foreach (var coord in coords)
                {
                    if (!map.TryGetValue(coord, out idList))
                    {
                        idList = new List<int>();
                        map[coord] = idList;
                    }
                    idList.Add(id);
                }
            }
            return map;
        }

        private string FindNonOverlapId(string[] data)
        {
            var map = GetMap(data);

            int id;
            foreach (var line in data)
            {
                var coords = GetCoords(line, out id);
                var ids = coords.Select(c => map[c]);
                if (ids.All(ii => ii.Count == 1)) return id.ToString();
            }
            throw new InvalidOperationException("Not Found");
        }

        private string FindOverlaps(string[] data)
        {
            int id;
            var used = new HashSet<XY>();
            var overlaps = new HashSet<XY>();
            foreach (var line in data)
            {
                var coords = GetCoords(line, out id);
                var newOverlap = used.Intersect(coords);
                overlaps.UnionWith(newOverlap);
                used.UnionWith(coords);
            }
            return overlaps.Count.ToString();
        }

        private List<XY> GetCoords(string line, out int id)
        {
            var parts = line.GetParts();
            id = int.Parse(parts[0]);
            int x = int.Parse(parts[1]);
            int y = int.Parse(parts[2]);
            int w = int.Parse(parts[3]);
            int h = int.Parse(parts[4]);

            var coords = new List<XY>();

            for (int thisx = x; thisx < x+w; thisx++)
            {
                for (int thisy = y; thisy < y+h; thisy++)
                {
                    coords.Add(new XY(thisx, thisy));
                }
            }
            return coords;
        }
    }
}
