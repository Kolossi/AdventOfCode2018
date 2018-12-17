using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Runner
{
    class Day17 :  Day
    {
        public override string First(string input)
        {
            var ground = new Ground(input);
            return "X";
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

        public class Ground
        {
            public XY MinPos;
            public XY MaxPos;
            public Map<object> Clay = new Map<object>();
            public Map<object> Water = new Map<object>();
            public Map<object> Wet = new Map<object>();
            public Map<object> Spring = new Map<object>();

            public Ground(string input)
            {
                Spring.Set(500, 0);
                MinPos = new XY(int.MaxValue, int.MaxValue);
                MaxPos = new XY(int.MinValue, int.MinValue);

                foreach (var line in input.GetLines("=,"))
                {
                    var parts = line.GetParts("xy");
                    if (line.StartsWith("x"))
                    {
                        var x = int.Parse(parts[0]);
                        var yParts = parts[1].Split("..");
                        var minY = int.Parse(yParts[0]);
                        var maxY = int.Parse(yParts[1]);

                        for (int y = minY; y <= maxY; y++)
                        {
                            Clay.Set(x, y);
                            UpdateMinMax(x, y);
                        }
                    }
                    else if (line.StartsWith("y"))
                    {
                        var y = int.Parse(parts[0]);
                        var xParts = parts[1].Split("..");
                        var minx = int.Parse(xParts[0]);
                        var mayx = int.Parse(xParts[1]);

                        for (int x = minx; x <= mayx; x++)
                        {
                            Clay.Set(x, y);
                            UpdateMinMax(x, y);
                        }
                    }
                    else throw new InvalidOperationException();
                }

                if (LogEnabled) ShowState();
            }

            private void ShowState()
            {
                LogLine("{0}:", MinPos.MoveN().MoveW());
                for (int y = MinPos.Y - 1; y <= MaxPos.Y + 1; y++)
                {
                    for (int x = MinPos.X - 1; x <= MaxPos.X + 1; x++)
                    {
                        Log(Spring.Has(x,y) ? "+" :
                            Clay.Has(x, y) ? "#" :
                            Water.Has(x, y) ? "~" :
                            Wet.Has(x, y) ? "|" : ".");
                    }
                    LogLine();
                }
            }

            private void UpdateMinMax(int x, int y)
            {
                var minX = Math.Min(MinPos.X, x);
                var minY = Math.Min(MinPos.Y, y);
                var maxX = Math.Max(MaxPos.X, x);
                var maxY = Math.Max(MaxPos.Y, y);
                MinPos = new XY(minX, minY);
                MaxPos = new XY(maxX, maxY);
            }
        }

        public class Map<T>
        {
            public Dictionary<int, Dictionary<int, T>> Data;
            public int Count;

            public Map()
            {
                Count = 0;
                Data = new Dictionary<int, Dictionary<int, T>>();
            }

            public Map(IEnumerable<XY> coords)
            {
                Count = 0;
                Data = new Dictionary<int, Dictionary<int, T>>();
                foreach (var coord in coords)
                {
                    Set(coord);
                }
            }

            public Map(Map<T> sourceMap)
            {
                Count = sourceMap.Count;
                Data = new Dictionary<int, Dictionary<int, T>>();
                foreach (var key in sourceMap.Data.Keys)
                {
                    Data[key] = new Dictionary<int, T>(sourceMap.Data[key]);
                }
            }

            public IEnumerable<T> GetAll()
            {
                return Data.SelectMany(i => i.Value).Select(i => i.Value);
            }

            public bool Has(XY xy)
            {
                return Has(xy.X, xy.Y);
            }

            public bool Has(int x, int y)
            {
                Dictionary<int,T> yDict;
                if (!Data.TryGetValue(y, out yDict)) return false;
                return yDict.ContainsKey(x);
            }

            public T Get(XY xy)
            {
                return Get(xy.X, xy.Y);
            }

            public T Get(int x, int y)
            {
                Dictionary<int,T> yDict;
                if (!Data.TryGetValue(y, out yDict)) return default(T);
                return yDict[x];
            }

            public bool TryGetValue(XY xy, out T value)
            {
                return TryGetValue(xy.X, xy.Y, out value);
            }

            public bool TryGetValue(int x, int y, out T value)
            {
                Dictionary<int,T> yDict;
                if (!Data.TryGetValue(y, out yDict))
                {
                    value = default(T);
                    return false;
                }
                return yDict.TryGetValue(x, out value);
            }

            public bool Remove(XY xy)
            {
                Dictionary<int,T> yDict;
                if (!Data.TryGetValue(xy.Y, out yDict))
                {
                    return false;
                }
                if (yDict.Remove(xy.X))
                {
                    Count--;
                    return true;
                }
                return false;
            }

            public void Set(XY xy)
            {
                Set(xy.X, xy.Y);
            }

            public void Set(int x, int y)
            {
                Set(x, y, default(T));
            }

            public void Set(XY xy, T value)
            {
                Set(xy.X, xy.Y, value);
            }

            public void Set(int x, int y, T value)
            {
                Dictionary<int,T> yDict;
                if (!Data.TryGetValue(y, out yDict))
                {
                    yDict = new Dictionary<int, T>();
                    Data[y] = yDict;
                }
                if (!yDict.ContainsKey(x)) Count++;
                yDict[x] = value;
            }
        }
    }
}
