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
            LogEnabled = false;
            var ground = new Ground(input);
            ground.TraceWater();
            ground.TraceWater();// to fix a screw up in the algorithm somewhere
            ground.ShowState();
            var wetOrWater = ground.WetOrWater();
            return wetOrWater.Count().ToString();  // not 406 too low, 31889 too high
        }

        public override string Second(string input)
        {
            LogEnabled = false;
            var ground = new Ground(input);
            ground.TraceWater();
            ground.TraceWater();// to fix a screw up in the algorithm somewhere
            ground.ShowState();
            var waterOnly = ground.WaterOnly();
            return waterOnly.Count().ToString();  // not 8870 too low
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
                MinPos = new XY(Spring.GetAllCoords().First());
                MaxPos = new XY(Spring.GetAllCoords().First());

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

            public Ground TraceWater()
            {
                var waterPath = new Path();
                waterPath.Move(new XY(500,0));
                var pathsToProcess = new Queue<Path>();
                pathsToProcess.Enqueue(waterPath);

                while (pathsToProcess.Any())
                {
                    IEnumerable<XY> waterCoords = Water.GetAllCoords();
                    //if (waterCoords.Any() && waterCoords.Max(xy => xy.Y) > 1380) LogEnabled = true;

                    var path = pathsToProcess.Dequeue();
                    if (LogEnabled)
                    {
                        LogLine("Will this fall out of map?: {0}", path);
                        ShowState(point: path.XY, startY: 1380, maxLines: 20);
                    }

                    if (!WaterFall(path)) continue;

                    Path trickleWestPath;
                    Path trickleEastPath;
                    bool overFlow = false;
                    bool newFlow = false;

                    while (!overFlow)
                    {
                        if (LogEnabled)
                        {
                            LogLine("Looking for overflow: {0} - {1}", path.XY, path);
                            ShowState(point: path.XY, startY: 1380, maxLines: 20);
                        }
                        if (TrickleWaterToFall(path.XY, Direction.West, out trickleWestPath))
                        {
                            overFlow = true;
                            if (!pathsToProcess.Any(p => p.XY.Equals(trickleWestPath.XY)))
                            {
                                var newPath = new Path().Move(trickleWestPath.XY);
                                pathsToProcess.Enqueue(newPath);
                                LogLine("Enqueued overflow : {0}, ", newPath);
                            }
                        }

                        if (TrickleWaterToFall(path.XY, Direction.East, out trickleEastPath))
                        {
                            overFlow = true;
                            if (!pathsToProcess.Any(p => p.XY.Equals(trickleEastPath.XY)))
                            {
                                var newPath = new Path().Move(trickleEastPath.XY);
                                pathsToProcess.Enqueue(newPath);
                                LogLine("Enqueued overflow : {0}, ", newPath);                            }
                        }

                        IEnumerable<XY> tricklePoints = trickleWestPath.Points.Union(trickleEastPath.Points).Distinct();

                        if (!overFlow && tricklePoints.Any()) newFlow = true;
                        foreach (var trickleXY in tricklePoints)
                        {
                            (overFlow ? Wet : Water).Set(trickleXY);
                        }

                        if (path.Length == 0) break;
                        path.Backup();
                    }
                    if (path.Length == 0) continue;
                    if (newFlow)
                    {
                        var newPath = new Path(path.Backup());
                        pathsToProcess.Enqueue(newPath);
                        if (LogEnabled)
                        {
                            ShowState(point: path.XY, startY: 1380, maxLines: 20);
                            LogLine("Enqueued backup: {0}", path);
                        }
                    }
                }

                return this;
            }

            private bool WaterFall(Path path)
            {
                XY below;
                while (CanFall(path.XY, out below))
                {
                    if (below.Y > MaxPos.Y) return false;
                    path.Move(below);
                    Wet.Set(below);
                };

                return true;
            }

            private bool TrickleWaterToFall(XY xy, Direction direction, out Path trickle)
            {
                trickle = new Path();
                trickle.Move(xy);
                
                XY waterPos = xy.Move(direction);

                while (!Blocked(waterPos))
                {
                    trickle.Move(waterPos);
                    if (CanFall(waterPos)) return true;
                    waterPos = waterPos.Move(direction);
                }
                return false;
            }

            private bool CanFall(XY xy)
            {
                XY below;
                return CanFall(xy, out below);
            }

            private bool CanFall(XY xy, out XY below)
            {
                below = xy.MoveS();
                if (!Blocked(below))
                {
                    return true;
                }
                below = null;
                return false;
            }

            private bool Blocked(XY xy)
            {
                return Clay.Has(xy) || Water.Has(xy);
            }

            public void ShowState(XY point = null, int startY = 0, int maxLines = int.MaxValue)
            {
                int lines = 0;
                XY startPos = new XY(MinPos.X - 1, Math.Max(MinPos.Y - 1, startY));
                XY endPos = new XY(MaxPos.X + 1, MaxPos.Y + 1);
                LogLine("{0}-{1}:", startPos,endPos);
                for (int y = startPos.Y; y <= endPos.Y; y++)
                {
                    for (int x = startPos.X; x <= endPos.X; x++)
                    {
                        Log(point != null && point.X == x && point.Y == y ? "P" :
                            Spring.Has(x, y) ? "+" :
                            Clay.Has(x, y) ? "#" :
                            Water.Has(x, y) ? "~" :
                            Wet.Has(x, y) ? "|" : ".");
                    }
                    LogLine();
                    if (lines++ > maxLines) return;
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

            internal IEnumerable<XY> WetOrWater()
            {
                var minY = Clay.GetAllCoords().Min(clay => clay.Y);
                return Wet.GetAllCoords()
                    .Union(Water.GetAllCoords())
                    .Where(xy=>xy.Y>=minY && xy.Y<=MaxPos.Y)
                    .Distinct();
            }

            internal IEnumerable<XY> WaterOnly()
            {
                var minY = Clay.GetAllCoords().Min(clay => clay.Y);
                return Water.GetAllCoords()
                    .Where(xy=>xy.Y>=minY && xy.Y<=MaxPos.Y)
                    .Distinct();
            }
        }

        public class Path
        {
            public XY XY;
            public LinkedList<XY> Points = new LinkedList<XY>();
            public Map<bool> Visited = new Map<bool>();
            public int Length = -1;

            public Path()
            {
            }

            public Path(Path sourcePath)
            {
                XY = new XY(sourcePath.XY.X, sourcePath.XY.Y);
                Points = new LinkedList<XY>(sourcePath.Points);
                Visited = new Map<bool>(sourcePath.Visited);
                Length = sourcePath.Length;
            }

            public Path Move(XY xy)
            {
                XY = xy;
                Visited.Set(xy);
                Points.AddLast(xy);
                Length++;
                return this;
            }

            public Path Backup()
            {
                if (Length == 0) throw new InvalidOperationException();
                XY = Points.Last.Previous.Value;
                Visited.Remove(XY);
                Points.RemoveLast();
                Length--;
                return this;
            }

            public override string ToString()
            {
                return string.Format("({0}):{1}", Length, string.Join(",", Points));
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

            public IEnumerable<XY> GetAllCoords()
            {
                if (!Data.Any()) return Enumerable.Empty<XY>();
                return Data.Keys.SelectMany(y => Data[y].Keys.Select(x => new XY(x, y)));
            }

            public IEnumerable<T> GetAllValues()
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
