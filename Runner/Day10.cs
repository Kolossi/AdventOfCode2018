using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Runner
{
    class Day10 :  Day
    {
        public override string First(string input)
        {
            var data = GetData(input.GetLines("position=,<>velocity"));
            data = FindMinY(data);
            return Environment.NewLine+data.Render();
        }

        public override string Second(string input)
        {
            var data = GetData(input.GetLines("position=,<>velocity"));
            data = FindMinY(data);
            return data.Time.ToString();
        }

        ////////////////////////////////////////////////////////
    
        public class Data
        {
            public List<XY> Coords = new List<XY>();
            public List<XY> Velocity = new List<XY>();
            public int Time = 0;

            public string Render()
            {
                var drawXMin = Coords.Min(c => c.X);
                var drawXMax = Coords.Max(c => c.X);
                var drawYMin = Coords.Min(c => c.Y);
                var drawYMax = Coords.Max(c => c.Y);

                var sb = new StringBuilder();

                for (int y = drawYMin; y <= drawYMax; y++)
                {
                    for (int x = drawXMin; x <= drawXMax; x++)
                    {
                        sb.Append(Coords.Any(c => (c.X == x && c.Y == y)) ? "#" : ".");
                    }
                    sb.AppendLine();
                }

                return sb.ToString();
            }

            
        }

        public Data GetData(string[] lines)
        {
            var data = new Data();

            foreach (var line in lines)
            {
                var parts = line.GetParts();
                data.Coords.Add(new XY(int.Parse(parts[0]), int.Parse(parts[1])));
                data.Velocity.Add(new XY(int.Parse(parts[2]), int.Parse(parts[3])));
            }

            return data;
        }

        public Data FindMinY(Data data)
        {

            var height = data.Coords.Max(c => c.Y) - data.Coords.Min(c => c.Y);
            var lastHeight = int.MaxValue;
            List<XY> lastCoords = data.Coords;
            while(height<lastHeight)
            {
                lastHeight = height;
                lastCoords = data.Coords.Select(c => new XY(c.X, c.Y)).ToList();
                for (int i = 0; i < data.Coords.Count; i++)
                {
                    data.Coords[i].X += data.Velocity[i].X;
                    data.Coords[i].Y += data.Velocity[i].Y;
                }
                height = data.Coords.Max(c => c.Y) - data.Coords.Min(c => c.Y);
                data.Time++;
            };

            data.Time--;
            data.Coords = lastCoords;
            return data;
        }
    }
}
