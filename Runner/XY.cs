using System;
using System.Collections.Generic;
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
    }

}
