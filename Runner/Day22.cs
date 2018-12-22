using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using static Runner.Day20;

namespace Runner
{
    class Day22 : Day
    {
        public override string First(string input)
        {
            var parts = input.GetParts("depth:,target");
            var depth = int.Parse(parts[0]);
            var targetXY = new XY(int.Parse(parts[1]), int.Parse(parts[2]));
            var cave = GetCave(depth, targetXY);
            LogLine(cave.TypeMap.GetStateString(TypeCharDict));
            //foreach (var firstType in (Type[])Enum.GetValues(typeof(Type)))
            //{
            //    foreach (var secondType in (Type[])Enum.GetValues(typeof(Type)))
            //    {
            //        foreach (var equip in (Equipment[])Enum.GetValues(typeof(Equipment)))
            //        {
            //            Equipment nextEquip;
            //            bool mustChange = Cave.MustChangeEquipment(firstType, secondType, equip, out nextEquip);
            //            LogLine("{0}->{1} with {2} : {3}", firstType, secondType, equip, mustChange ? string.Format("change to {0}", nextEquip) : string.Format("keep {0}", equip));
            //        }
                    
            //    }
            //}
            return cave.TypeMap.GetAllValues().Sum(t => (long)(t)).ToString();
        }

        public override string Second(string input)
        {
            var parts = input.GetParts("depth:,target");
            var depth = int.Parse(parts[0]);
            var targetXY = new XY(int.Parse(parts[1]), int.Parse(parts[2]));
            var cave = GetCave(depth, targetXY);
            var times = cave.FindTimes();
            return times.Get(targetXY).ToString();
        }

        ////////////////////////////////////////////////////////

        private Cave GetCave(int depth, XY targetXY)
        {
            var cave = new Cave()
            {
                Depth = depth,
                Target = targetXY
            };
            for (int y = 0; y <= targetXY.Y; y++)
            {
                for (int x = 0; x <= targetXY.X; x++)
                {
                    cave.Set(x, y);
                }

            }
            return cave;
        }

        public enum Type
        {
            Rocky = 0,
            Wet = 1,
            Narrow = 2
        }

        public enum Equipment
        {
            Neither = 0,
            Torch = 1,
            Climbing = 2
        }

        Dictionary<Type, char> TypeCharDict = new Dictionary<Type, char>()
        {
            {Type.Rocky,'.' },
            {Type.Wet,'=' },
            {Type.Narrow,'|' }
        };

        public class Walk
        {
            public Path Path;
            public Equipment Equipment;
            public int TimeTaken;
        }

        public class Cave
        {
            public int Depth;
            public XY Target;
            public Map<Type> TypeMap = new Map<Type>();
            public Map<int> IndexMap = new Map<int>();
            public Map<int> ErosionMap = new Map<int>();

            internal Type GetType(XY xy)
            {
                Type type;
                if (!TypeMap.TryGetValue(xy, out type))
                {
                    Set(xy.X, xy.Y);
                    return TypeMap.Get(xy);
                }
                return type;
            }

            internal int GetErosion(int x, int y)
            {
                int erosion;
                if (!ErosionMap.TryGetValue(x, y, out erosion))
                {
                    Set(x, y);
                    return ErosionMap.Get(x, y);
                }
                return erosion;
            }

            internal void Set(int x, int y)
            {
                int geoIndex = (x == 0 && y == 0) ? 0 :
                    (x == Target.X && y == Target.Y) ? 0 :
                    (y == 0) ? x * 16807 :
                    (x == 0) ? y * 48271 :
                    GetErosion(x - 1, y) * GetErosion(x, y - 1);
                int erosion = (geoIndex + Depth) % 20183;
                Type type = (Type)(erosion % 3);
                IndexMap.Set(x, y, geoIndex);
                ErosionMap.Set(x, y, erosion);
                TypeMap.Set(x, y, type);
            }

            public static bool MustChangeEquipment(Type here, Type destination, Equipment current, out Equipment next)
            {
                if (destination == here)
                {
                    next = current;
                    return false;
                }
                next = (Equipment)((3 - (int)here) & (3 - (int)destination));
                return (next != current);
            }

            private readonly Direction[] DirectionsToTryEven = new Direction[] {
                Direction.West,
                Direction.North,
                Direction.East,
                Direction.South
            };

            private readonly Direction[] DirectionsToTryOdd = new Direction[] {
                Direction.North,
                Direction.West,
                Direction.South,
                Direction.East
            };

            public Map<int> FindTimes()
            {
                Map<int> visitTimes = new Map<int>();
                LinkedList<Walk> walksToProcess = new LinkedList<Walk>();
                XY start = new XY(0, 0);
                Walk startWalk = new Walk()
                {
                    Path = (new Path()).Move(start),
                    Equipment = Equipment.Torch
                };
                walksToProcess.AddFirst(startWalk);
                int shortest = (Target.X + Target.Y) * 8;

                long i = 0;
                while (walksToProcess.Any())
                {
                    i++;
                    if ((i % 10000) == 0) LogLine("shortest={0}, pending={1}", shortest, walksToProcess.Count());
                    //if (LogEnabled)
                    //{
                    //    LogLine("shortest={0}, pending={1}", shortest, walksToProcess.Count());
                    //    foreach (var logWalk in walksToProcess.AsEnumerable())
                    //    {
                    //        LogLine(logWalk);
                    //    }
                    //}
                    var walk = walksToProcess.First();
                    walksToProcess.RemoveFirst();

                    var xy = walk.Path.XY;
                    if (xy.X < 0 || xy.Y < 0) continue;
                    if (walk.TimeTaken >= shortest) continue;

                    if (walk.TimeTaken + (Math.Abs(Target.Y - xy.X) + Math.Abs(Target.X - xy.X)) >= shortest) continue; // quickest to target from here still wont beat shortest

                    var directionsToTry = (((xy.X + xy.Y) % 2) == 0) ? DirectionsToTryEven : DirectionsToTryOdd; // prioritise diagonal walk

                    foreach (var direction in directionsToTry)
                    {
                        var newXY = xy.Move(direction);
                        if (newXY.Equals(start)) continue;
                        if (newXY.X < 0 || newXY.Y < 0) continue;
                        if (walk.Path.Visited.Has(newXY)) continue;
                        int timeTaken = walk.TimeTaken + 1;
                        Equipment nextEquipment;
                        if (MustChangeEquipment(this.GetType(xy), this.GetType(newXY), walk.Equipment, out nextEquipment))
                        {
                            timeTaken += 7;
                        }
                        if (timeTaken >= shortest) continue;
                        int currentShortestToPos;
                        if (visitTimes.TryGetValue(newXY, out currentShortestToPos) && currentShortestToPos <= timeTaken) continue;
                        visitTimes.Set(newXY, timeTaken);
                        if (Target.Equals(newXY))
                        {
                            if (timeTaken < shortest) shortest = timeTaken;
                            continue;
                        }

                        if (timeTaken + (Math.Abs(Target.Y - newXY.X) + Math.Abs(Target.X - newXY.X)) >= shortest) continue; // quickest to target from here still wont beat shortest
                        walksToProcess.AddFirst(
                            new Walk()
                            {
                                Path = (new Path(walk.Path)).Move(newXY),
                                Equipment = nextEquipment,
                                TimeTaken = timeTaken
                            });
                    }
                }
                return visitTimes;
            }
        }
    }
}
