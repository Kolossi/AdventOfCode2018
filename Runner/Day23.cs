using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Runner
{
    class Day23 :  Day
    {
        public override string First(string input)
        {
            var bots = GetNanoBots(input);
            var strongest = bots.Values.OrderByDescending(b => b.R).First();
            return bots.Values.Count(b => b.DistanceTo(strongest) <= strongest.R).ToString();
        }

        public override string Second(string input)
        {
            LogEnabled = false;
            var bots = GetNanoBots(input);
            var cavern = new Cavern(bots);
            cavern.ShowBots();
            //cavern.ShowDistances();
            //cavern.ShowInRange();
            cavern.ShowIntersects();
            var result = cavern.SolveAll();
            //LogEnabled = true;
            LogLine(result);
            LogLine(cavern.NewCoords(result.XYZ, 1).Select(c => new Attempt() { XYZ = c, Score = cavern.BotsInRange(c) }));
            //result = cavern.HeadToOrigin(result);
            return result.XYZ.DistanceTo(XYZ.Origin).ToString(); // not 107680769, 107296598 too high
        }

        ////////////////////////////////////////////////////////


        public class XYZ
        {
            public long X;
            public long Y;
            public long Z;

            public static XYZ Origin = new XYZ() { X = 0, Y = 0, Z = 0 };

            public long DistanceTo(XYZ xyz)
            {
                return (Math.Abs(X - xyz.X) + Math.Abs(Y - xyz.Y) + Math.Abs(Z - xyz.Z));
            }

            public override string ToString()
            {
                return string.Format("[{0},{1},{2}]", X, Y, Z);
            }
        }

        public class Attempt
        {
            public XYZ XYZ;
            public long Score;

            public override string ToString()
            {
                return string.Format("xyz={0}, score={1}", XYZ, Score);
            }
        }

        public class Cavern
        {
            public Dictionary<int, NanoBot> Bots;
            public Dictionary<int, Dictionary<int, long>> Distances = new Dictionary<int, Dictionary<int, long>>();
            public Dictionary<int, List<int>> InRange = new Dictionary<int, List<int>>();
            public Dictionary<int, List<int>> IntersectsWith = new Dictionary<int, List<int>>();



            public int Count
            {
                get
                {
                    return Bots.Count;
                }
            }

            public Cavern(Dictionary<int, NanoBot> bots)
            {
                Bots = bots;
                SetDistances();
                FindInRange();
            }

            public XYZ FindCog()
            {
                return new XYZ()
                {
                    X = (long)Bots.Values.Average(b => b.XYZ.X),
                    Y = (long)Bots.Values.Average(b => b.XYZ.Y),
                    Z = (long)Bots.Values.Average(b => b.XYZ.Z)
                };
            }

            internal Attempt HeadToOrigin(Attempt result)
            {
                var attempt = new Attempt()
                {
                    XYZ = result.XYZ,
                    Score = result.Score
                };
                long scale = Math.Max(attempt.XYZ.X / 2,1);
                while (true)
                {
                    if (attempt.XYZ.X == XYZ.Origin.X && attempt.XYZ.Y == XYZ.Origin.Y && attempt.XYZ.Z == XYZ.Origin.Z) break;
                    LogLine("Pos={0}, Score={1}, scale={2}", attempt.XYZ, attempt.Score, scale);
                    var possibleAttempts = NewCoords(attempt.XYZ, scale).Select(c => new Attempt() { XYZ = c, Score = c.DistanceTo(XYZ.Origin) }).OrderBy(a => a.Score);
                    LogLine(possibleAttempts);
                    var firstAttempt = possibleAttempts.First();
                    if (BotsInRange(firstAttempt.XYZ) < result.Score)
                    {
                        if (scale == 1) break;
                        scale/=2;
                        continue;
                    }
                    attempt = firstAttempt;
                }
                LogLine("NEAR Origin ANSWER: Pos={0}, Score={1}, scale={2}", attempt.XYZ, attempt.Score, scale);
                return attempt;
            }

            public Attempt SolveAll()
            {
                // change to do a spread of values around origin using newcoords, and only up to (value too high distance)
                var distance = Bots[InRange.OrderBy(kv => kv.Value.Count).First().Key].XYZ.DistanceTo(XYZ.Origin);
                var iterations = Math.Max(Math.Min(100, (int)distance / 3),1);
                var results = Enumerable.Range(1, iterations).Select(i => Solve(2*i / (float)iterations));
                var maxScore = results.Max(r => r.Score);
                var topResults = results.Where(r => r.Score == maxScore);
                topResults = topResults.Select(r => HeadToOrigin(r)).OrderBy(r => r.Score);
                LogLine(topResults);
                return topResults.First();

            }

            public Attempt Solve(float startScale)
            {
                var startPoint = Bots[InRange.OrderBy(kv => kv.Value.Count).First().Key].XYZ;
                var attempt = new Attempt()
                {
                    //XYZ = FindCog()
                    XYZ = new XYZ() {
                        X = (long)(startPoint.X * startScale),
                        Y = (long)(startPoint.Y * startScale),
                        Z = (long)(startPoint.Z * startScale)
                    }
                };
                attempt.Score = BotsInRange(attempt.XYZ);
                long scale = Math.Max(attempt.XYZ.X / 2, 1);
                while (true)
                {
                    LogLine("Pos={0}, Score={1}, scale={2}", attempt.XYZ, attempt.Score, scale);
                    var possibleAttempts = NewCoords(attempt.XYZ, scale).Select(c => new Attempt() { XYZ = c, Score = BotsInRange(c) }).OrderByDescending(a => a.Score);
                    LogLine(possibleAttempts);
                    if (possibleAttempts.All(a => a.Score <= attempt.Score))
                    {
                        if (scale == 1) break;
                        scale/=2;
                        continue;
                    }
                    attempt = possibleAttempts.First();
                }
                LogLine("ANSWER: Pos={0}, Score={1}, scale={2}", attempt.XYZ, attempt.Score, scale);
                return attempt;
            }

            public LinkedList<XYZ> NewCoords(XYZ xyz, long scale)
            {
                var coords = new LinkedList<XYZ>();
                for (long dx = -scale; dx <= scale; dx+=scale)
                {
                    for (long dy = -scale; dy <= scale; dy+=scale)
                    {
                        for (long dz = -scale; dz <= scale; dz+=scale)
                        {
                            if (dx == 0 && dy == 0 && dz == 0) continue;
                            coords.AddLast(new XYZ()
                            {
                                X = xyz.X + dx,
                                Y = xyz.Y + dy,
                                Z = xyz.Z + dz
                            });
                        }
                    }
                }

                return coords;
            }

            public Cavern SetDistances()
            {
                for (int leftId = 0; leftId < this.Count; leftId++)
                {
                    SetDistance(leftId, leftId, 0);
                    var left = Bots[leftId];
                    for (int rightId = leftId + 1; rightId < this.Count; rightId++)
                    {
                        var right = Bots[rightId];
                        var distance = left.DistanceTo(right);
                        SetDistance(leftId, rightId, distance);
                        SetDistance(rightId, leftId, distance);
                    }
                }

                return this;
            }

            public int BotsInRange(XYZ xyz)
            {
                return Bots.Values.Count(b => b.DistanceTo(xyz) <= b.R);
            }

            public Cavern FindInRange()
            {
                for (int id = 0; id < this.Count; id++)
                {
                    var bot = Bots[id];
                    InRange[id] = Distances[id].Where(kv => kv.Value <= bot.R).Select(kv => kv.Key).ToList();
                    IntersectsWith[id] = Distances[id].Where(kv => kv.Value <= (bot.R + Bots[kv.Key].R)).Select(kv => kv.Key).ToList();
                }

                return this;
            }

            private void SetDistance(int leftId, int rightId, long distance)
            {
                Dictionary<int, long> distanceDict;
                if (!Distances.TryGetValue(leftId, out distanceDict))
                {
                    distanceDict = new Dictionary<int, long>();
                    Distances[leftId] = distanceDict;
                }
                distanceDict[rightId] = distance;
            }

            public void ShowDistances()
            {
                for (int leftId = 0; leftId < this.Count; leftId++)
                {
                    for (int rightId = 0; rightId < this.Count; rightId++)
                    {
                        LogLine("{0}->{1}={2}", leftId, rightId, Distances[leftId][rightId]);
                    }
                }
            }

            public void ShowIntersects()
            {
                for (int id = 0; id < this.Count; id++)
                {
                    LogLine("{0} : Intersects with {1}", id, IntersectsWith[id].Count());
                }

            }

            public void ShowInRange()
            {
                for (int id = 0; id < this.Count; id++)
                {
                    LogLine("{0} : In Range {1}, Intersects with {2}", id, InRange[id], IntersectsWith[id]);
                }
            }

            public void ShowBots()
            {
                for (int id = 0; id < this.Count; id++)
                {
                    LogLine(Bots[id]);
                }
            }
        }

        public class NanoBot
        {
            public int Id;
            public XYZ XYZ;
            public long R;

            public long DistanceTo(XYZ xyz)
            {
                return (Math.Abs(XYZ.X - xyz.X) + Math.Abs(XYZ.Y - xyz.Y) + Math.Abs(XYZ.Z - xyz.Z));
            }

            public long DistanceTo(NanoBot right)
            {
                return (Math.Abs(XYZ.X - right.XYZ.X) + Math.Abs(XYZ.Y - right.XYZ.Y) + Math.Abs(XYZ.Z - right.XYZ.Z));
            }

            public override string ToString()
            {
                return string.Format("{0} : {1}, ({2})", Id, XYZ, R);
            }
        }

        Dictionary<int,NanoBot> GetNanoBots(string input)
        {
            int i = 0;
            var bots = new Dictionary<int,NanoBot>();
            foreach (var line in input.GetLines("pos=<>,r="))
            {
                var parts = line.GetParts();
                int id = i++;

                bots[id] = new NanoBot()
                {
                    Id = id,
                    XYZ = new XYZ()
                    {
                        X = long.Parse(parts[0]),
                        Y = long.Parse(parts[1]),
                        Z = long.Parse(parts[2])
                    },
                    R = long.Parse(parts[3]),
                };
            }
            return bots;
        }
    }
}
