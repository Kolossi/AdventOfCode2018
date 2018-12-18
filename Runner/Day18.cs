using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Runner
{
    class Day18 :  Day
    {
        public override string First(string input)
        {
            LogEnabled = false;
            var area = new CollectionArea(input);
            if (LogEnabled) area.ShowState();
            return area.Solve();
        }

        public override string Second(string input)
        {
            var area = new CollectionArea(input);
            //area.ShowResourceValues(1000);
            long prediction;
            PredictResultBasedOnCycle(area.GetSampleOfResourceValues(1000), 1000000000, out prediction);
            return prediction.ToString(); // not 166860, too low : 169106
            
        }

        public override string SecondTest(string input)
        {
            throw new NotImplementedException("SecondTest");
        }

        ////////////////////////////////////////////////////////

        public enum Item
        {
            Open = 0,
            Trees = 1,
            LumberYard = 2,
        }

        public static Dictionary<Item, char> ItemToChar = new Dictionary<Item, char>()
        {
            { Item.Open,'.'},
            { Item.Trees,'|' },
            { Item.LumberYard,'#'}
        };

        public static Dictionary<char, Item> CharToItem = new Dictionary<char, Item>()
        {
            { '.',Item.Open},
            { '|',Item.Trees },
            { '#',Item.LumberYard}
        };

        public class CollectionArea
        {
            Map<Item> Map = new Map<Item>();
            public CollectionArea(string input)
            {
                var lines = input.GetLines();
                for (int y = 0; y < lines.Length; y++)
                {
                    for (int x = 0; x < lines[0].Length; x++)
                    {
                        Map.Set(x, y, CharToItem[lines[y][x]]);
                    }
                }
            }

            public string Solve()
            {
                for (int i = 0; i < 10; i++)
                {
                    Iterate();
                    ShowState();
                }
                return GetResourceValue().ToString();
            }

            public void ShowResourceValues(int numValues)
            {
                for (int i = 0; i < numValues; i++)
                {
                    Iterate();
                    LogLine("{0:D4}:{1}",i,GetResourceValue());
                }

            }

            public IEnumerable<long> GetSampleOfResourceValues(int numValues)
            {
                List<long> results = new List<long>(numValues);
                for (int i = 0; i < numValues; i++)
                {
                    Iterate();
                    results.Add(GetResourceValue());
                    LogLine("{0:D4}:{1}",i,GetResourceValue());
                }
                return results;
            }

            private long GetResourceValue()
            {
                return (long)Map.GetAllValues().Count(i => i == Item.Trees) * (long)Map.GetAllValues().Count(i => i == Item.LumberYard);
            }

            public void Iterate()
            {
                var newMap = new Map<Item>(Map);
                foreach (var xy in Map.GetAllCoords())
                {
                    Item i;
                    var surroundings = xy.GetSurroundingCoords().Select(sxy => Map.TryGetValue(sxy, out i) ? i : Item.Open);
                    var current = Map.Get(xy);
                    switch (current)
                    {
                        case Item.Open:
                            if (surroundings.Count(s => s == Item.Trees) >= 3) newMap.Set(xy, Item.Trees);
                            break;
                        case Item.Trees:
                            if (surroundings.Count(s => s == Item.LumberYard) >= 3) newMap.Set(xy, Item.LumberYard);
                            break;
                        case Item.LumberYard:
                            if (!surroundings.Any(s => s == Item.LumberYard)|| !surroundings.Any(s => s == Item.Trees)) newMap.Set(xy, Item.Open);
                            break;
                        default:
                            break;
                    }
                }
                Map = newMap;
            }

            public void ShowState()
            {
                LogLine(Map.GetStateString(ItemToChar));
            }
            
        }

    }
}
