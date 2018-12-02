using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Runner
{
    class Day02 :  Day
    {
        public override string First(string input)
        {
            return Checksum(input.GetLines());
        }

        public override string Second(string input)
        {
            return FindSimilar(input.GetLines());
        }

        public override string FirstTest(string input)
        {
            return Checksum(input.GetParts());
        }

        public override string SecondTest(string input)
        {
            return FindSimilar(input.GetParts());
        }

        ////////////////////////////////////////////////////////

        private string Checksum(string[] ids)
        {
            int twoCount = 0, threeCount = 0;
            foreach (var id in ids)
            {
                bool hasTwo = false, hasThree = false;
                var groups = id.ToArray().GroupBy(c => c);
                foreach (var g in groups)
                {
                    var charCount = g.Count();
                    if (charCount==2)
                    {
                        hasTwo = true;
                    }
                    else if (charCount ==3)
                    {
                        hasThree = true;
                    }
                }
                if (hasTwo) twoCount++;
                if (hasThree) threeCount++;
            }

            return (twoCount * threeCount).ToString();
        }

        private string FindSimilar(string[] ids)
        {
            for (int i = 0; i < ids.Count()-1; i++)
            {
                for (int j = i + 1; j < ids.Count()-1; j++)
                {
                    var pos = SimilarityPos(ids[i], ids[j]);
                    if (pos >= 0)
                    {
                        return RemoveCharAt(ids[i], pos);
                    }
                }
            }
            return string.Empty;
        }

        private int SimilarityPos(string v1, string v2)
        {
            var pos = -1;
            if (v1.Length != v2.Length) return -1;
            for (int i = 0; i < v1.Length; i++)
            {
                if (v1[i] != v2[i])
                {
                    if (pos >= 0) return -1; //two different chars, therefore not similar
                    pos = i;
                }
            }
            return pos;
        }

        private string RemoveCharAt(string value, int pos)
        {
            return value.Substring(0, pos) +
                (pos < (value.Length - 1) ? value.Substring(pos + 1, value.Length - pos - 1) : string.Empty);
        }
    }
}
