using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Runner
{
    class Day05 :  Day
    {
        public override string First(string input)
        {
            return React(input).Length.ToString();
        }

        public override string Second(string input)
        {
            return Solve(input);
        }

        ////////////////////////////////////////////////////////

        public class Attempt
        {
            public char c;
            public int units;
        }

        public string Solve(string polymer)
        {
            List<Attempt> attempts = new List<Attempt>();
            foreach (var c in polymer.ToLower().ToArray().Distinct())
            {
                attempts.Add(new Attempt()
                {
                    c = c,
                    units = React(Reduce(polymer, c)).Length
                });
            }
            var result = attempts.OrderBy(a => a.units).First();
            return string.Format("{0}{1}", result.c, result.units);
        }
        
        public string Reduce(string polymer, char c)
        {
            var C = ToUpper(c);
            return (polymer.Replace(c, ' ').Replace(C, ' ').Replace(" ", ""));
        }

        public string React(string polymer)
        {
            int pos = 0;
            var units = new List<char>(polymer.ToArray());
            while (true)
            {
                if (pos >= (units.Count()-1)) break;
                if (pos < 0) pos = 0;
                if (WillReact(units[pos], units[pos + 1]))
                {
                    units.RemoveAt(pos);
                    units.RemoveAt(pos);
                    pos -= 2;
                }
                pos += 1;
            }

            return string.Join("", units.ToArray());
        }

        public char ToUpper(char c)
        {
            return c.ToString().ToUpper()[0];
        }

        public bool WillReact(char x, char y)
        {
            var X = ToUpper(x);
            var Y = ToUpper(y);
            var xUpper = (x == X);
            var yUpper = (y == Y);
            return ((xUpper ^ yUpper) && X == Y);
        }
    }
}
