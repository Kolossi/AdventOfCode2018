using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Runner
{
    class Day01 :  Day
    {
        public override string First(string input)
        {
            return input.GetParts(",").Select(i => int.Parse(i)).Sum().ToString();
        }

        public override string Second(string input)
        {
            return FindDup(input.GetParts(",").Select(i => int.Parse(i)).ToArray());
        }

        ////////////////////////////////////////////////////////
        ///
        public string FindDup(int[] offsets)
        {
            HashSet<int> seen = new HashSet<int>();
            int pos = 0;
            int freq = 0;
            while(true)
            {
                if (seen.Contains(freq)) return freq.ToString();
                seen.Add(freq);
                freq += offsets[pos++];
                if (pos >= offsets.Length) pos = 0;
            }
        }
    }
}
