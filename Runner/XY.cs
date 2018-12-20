using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Runner
{
    public class XY : IEquatable<XY>
    {
        public int X { get; set; }
        public int Y { get; set; }
        public XY(int x, int y)
        {
            X = x;
            Y = y;
        }

        public XY(XY xy) : this(xy.X, xy.Y)
        {
        }

        public override string ToString()
        {
            return string.Format("[{0},{1}]", X, Y);
        }

        public override bool Equals(object obj)
        {
            if (obj as XY == null) return base.Equals(obj);
            XY objXY = (XY)obj;
            return Equals(objXY);
        }

        public bool Equals(XY objXY)
        {
            return (objXY.X == X && objXY.Y == Y);
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode();
        }

        //       +1
        //        |
        // -1 ----0---- +1
        //        |
        //       -1
        public XY MoveN()
        {
            return new XY(X, Y - 1);
        }

        public XY MoveS()
        {
            return new XY(X, Y + 1);
        }

        public XY MoveE()
        {
            return new XY(X + 1, Y);
        }

        public XY MoveW()
        {
            return new XY(X - 1, Y);
        }

        public XY Move(Direction dir)
        {
            switch (dir)
            {
                case Direction.North:
                    return MoveN();
                case Direction.East:
                    return MoveE();
                case Direction.South:
                    return MoveS();
                case Direction.West:
                    return MoveW();
                default:
                    throw new ArgumentOutOfRangeException("Direction");
            }
        }

        public IEnumerable<XY> GetSurroundingCoords()
        {
            var xys = new List<XY>();
            for (int sy = -1; sy <= 1; sy++)
            {
                for (int sx = -1; sx <= 1; sx++)
                {
                    if (sx == 0 && sy == 0) continue;
                    xys.Add(new XY(X + sx, Y + sy));
                }
            }
            return xys;
        }

        public static Dictionary<Direction, char> DirToChar = new Dictionary<Direction, char>()
        {
            { Direction.North,'^'},
            { Direction.East,'>' },
            { Direction.South,'v'},
            { Direction.West,'<' }
        };

        public static Dictionary<char, Direction> CharToDir = new Dictionary<char, Direction>()
        {
            { '^',Direction.North},
            { '>',Direction.East},
            { 'v',Direction.South},
            { '<',Direction.West },
            { 'N',Direction.North},
            { 'E',Direction.East},
            { 'S',Direction.South},
            { 'W',Direction.West },
            { 'n',Direction.North},
            { 'e',Direction.East},
            { 's',Direction.South},
            { 'w',Direction.West }
        };
    }

    public enum Direction
    {
        North = 0,
        East = 1,
        South = 2,
        West = 3
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

        public int GetMinX()
        {
            return Data.Keys.SelectMany(y => Data[y].Keys).Min();
        }

        public int GetMaxX()
        {
            return Data.Keys.SelectMany(y => Data[y].Keys).Max();
        }

        public int GetMinY()
        {
            return Data.Keys.Min();
        }

        public int GetMaxY()
        {
            return Data.Keys.Max();
        }

        public XY GetMinPos()
        {
            return new XY(GetMinX(), GetMinY());
        }

        public XY GetMaxPos()
        {
            return new XY(GetMaxX(), GetMaxY());
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
            Dictionary<int, T> yDict;
            if (!Data.TryGetValue(y, out yDict)) return false;
            return yDict.ContainsKey(x);
        }

        public T Get(XY xy)
        {
            return Get(xy.X, xy.Y);
        }

        public T Get(int x, int y)
        {
            Dictionary<int, T> yDict;
            if (!Data.TryGetValue(y, out yDict)) return default(T);
            return yDict[x];
        }

        public bool TryGetValue(XY xy, out T value)
        {
            return TryGetValue(xy.X, xy.Y, out value);
        }

        public bool TryGetValue(int x, int y, out T value)
        {
            Dictionary<int, T> yDict;
            if (!Data.TryGetValue(y, out yDict))
            {
                value = default(T);
                return false;
            }
            return yDict.TryGetValue(x, out value);
        }

        public bool Remove(XY xy)
        {
            Dictionary<int, T> yDict;
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
            Dictionary<int, T> yDict;
            if (!Data.TryGetValue(y, out yDict))
            {
                yDict = new Dictionary<int, T>();
                Data[y] = yDict;
            }
            if (!yDict.ContainsKey(x)) Count++;
            yDict[x] = value;
        }

        public string GetStateString(Dictionary<T,char> valueMap)
            {
                var sb = new StringBuilder();
                var minPos = GetMinPos();
                var maxPos = GetMaxPos();
                sb.AppendFormat("{0}->{1}", minPos, maxPos).AppendLine();
                for (int y = minPos.Y; y <= maxPos.Y; y++)
                {
                    for (int x = minPos.X; x <= maxPos.X; x++)
                    {
                        sb.Append(valueMap[Get(x,y)]);
                    }
                    sb.AppendLine();
                }
                return sb.ToString();
            }

    }
}
