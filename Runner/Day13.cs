using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Runner
{
    class Day13 : Day
    {
        public override string First(string input)
        {
            LogEnabled = false;
            var map = input.Replace(">", "-").Replace("<", "-").Replace("^", "|").Replace("v", "|").GetLines();
            List<Truck> trucks = GetTrucks(input);
            return GetCollision(map, trucks);
        }

        public override string Second(string input)
        {
            LogEnabled = false;
            var map = input.Replace(">", "-").Replace("<", "-").Replace("^", "|").Replace("v", "|").GetLines();
            List<Truck> trucks = GetTrucks(input);
            return GetLastTruck(map, trucks); //not 9,6 not 9,7
        }

        ////////////////////////////////////////////////////////

        private string GetCollision(string[] map, List<Truck> trucks)
        {
            return Process(map, trucks, true);
        }

        private string GetLastTruck(string[] map, List<Truck> trucks)
        {
            return Process(map, trucks, false);
        }

        private string Process(string[] map, IEnumerable<Truck> trucks, bool first)
        {
            LogLine("START");
            int iter = 0;
            while (true)
            {
                iter++;
                LogLine("---{0}---", iter);
                var trucksToProcess = trucks.ToArray().AsEnumerable();
                while (trucksToProcess.Any())
                {
                    var truck = trucksToProcess.OrderBy(t => t.XY.Y).ThenBy(t => t.XY.Y).First();
                    trucksToProcess = trucksToProcess.Where(t => t.Id != truck.Id);
                    try
                    {
                        truck.XY = truck.XY.Move(truck.Direction);
                        var matchingTrucks = trucks.Where(t => t.Id != truck.Id && t.XY.X == truck.XY.X && t.XY.Y == truck.XY.Y);
                        if (matchingTrucks.Count() > 1) throw new InvalidOperationException();
                        var matchingTruck = matchingTrucks.FirstOrDefault();
                        if (matchingTruck != null)
                        {
                            if (first)
                            {
                                LogLine("END");
                                return string.Format("{0},{1}", truck.XY.X, truck.XY.Y);
                            }
                            else
                            {
                                trucksToProcess = trucksToProcess.Where(t => t.Id != truck.Id);
                                trucksToProcess = trucksToProcess.Where(t => t.Id != matchingTruck.Id);
                                truck.Dead = true;
                                matchingTruck.Dead = true;
                                if (trucks.Where(t=>!t.Dead).Count() == 0) throw new InvalidOperationException();
                            }

                        }
                        TurnTruck(truck, map);
                    }
                    catch
                    {
                        Console.WriteLine(ShowState(map, trucksToProcess));
                        throw;
                    }
                }
                trucks = trucks.Where(t => !t.Dead).ToArray().AsEnumerable();
                if (!first && trucks.Count() == 1)
                {
                    if (LogEnabled) LogLine(ShowState(map, trucks));
                    LogLine("End");
                    Console.WriteLine(string.Format("LastIter:{0}", iter));
                    return string.Format("{0},{1}", trucks.First().XY.X, trucks.First().XY.Y);
                }
            }
        }

        private void TurnTruck(Truck truck, string[] map)
        {
            var mapChar = map[truck.XY.Y][truck.XY.X];
            switch (mapChar)
            {
                case '|':
                case '-':
                    return;
                case ' ':
                    throw new InvalidOperationException();
                case '\\':
                    switch (truck.Direction)
                    {
                        case Direction.North:
                            truck.Direction = Direction.West;
                            break;
                        case Direction.East:
                            truck.Direction = Direction.South;
                            break;
                        case Direction.South:
                            truck.Direction = Direction.East;
                            break;
                        case Direction.West:
                            truck.Direction = Direction.North;
                            break;

                        default:
                            throw new InvalidOperationException();
                    }
                    break;
                case '/':
                    switch (truck.Direction)
                    {
                        case Direction.North:
                            truck.Direction = Direction.East;
                            break;
                        case Direction.West:
                            truck.Direction = Direction.South;
                            break;
                        case Direction.East:
                            truck.Direction = Direction.North;
                            break;
                        case Direction.South:
                            truck.Direction = Direction.West;
                            break;

                        default:
                            throw new InvalidOperationException();
                    }
                    break;
                case '+':
                    switch (truck.NextChoice)
                    {
                        case Choice.Left:
                            truck.Direction = (Direction)(((int)truck.Direction - 1 + 4) % 4);
                            break;
                        case Choice.Straight:
                            break;
                        case Choice.Right:
                            truck.Direction = (Direction)(((int)truck.Direction + 1) % 4);
                            break;
                        default:
                            throw new InvalidOperationException();

                    }
                    truck.NextChoice = (Choice)(((int)truck.NextChoice + 1) % 3);
                    break;


                default:
                    throw new InvalidOperationException();
            }
        }

        private string ShowState(string[] map, IEnumerable<Truck> trucks)
        {
            var sb = new StringBuilder();
            for (int y = 0; y < map.Length; y++)
            {
                for (int x = 0; x < map[0].Length; x++)
                {
                    var truck = trucks.FirstOrDefault(t => t.XY.X == x && t.XY.Y == y);
                    if (truck!=null)
                    {
                        sb.Append(XY.DirToChar[truck.Direction]);
                    }
                    else
                    {
                        sb.Append(map[y][x]);
                    }
                }
                sb.AppendLine();
            }
            sb.AppendLine();
            return sb.ToString();
        }

        private string ShowTruck(string[] map, Truck truck, int size)
        {
            if (truck == null) return string.Empty;
            var sb = new StringBuilder();
            sb.AppendLine(truck.ToString());
            for (int y = truck.XY.Y-size; y <= truck.XY.Y+size; y++)
            {
                for (int x = truck.XY.X-size; x <= truck.XY.X+size; x++)
                {
                    if (truck.XY.X == x && truck.XY.Y==y)
                    {
                        sb.Append(XY.DirToChar[truck.Direction]);
                    }
                    else if (y<0 || y>=map.Length||x<0 || x>=map[0].Length)
                    {
                        sb.Append(" ");
                    }
                    else
                    {
                        sb.Append(map[y][x]);
                    }
                }
                sb.AppendLine();
            }
            sb.AppendLine();
            return sb.ToString();
        }

        private List<Truck> GetTrucks(string input)
        {
            List<Truck> trucks = new List<Truck>();
            var lines = input.GetLines();
            var maxY = lines.Length;
            var maxX = lines.Max(l=>l.Length);
            lines = lines.Select(l => l.PadRight(maxX)).ToArray();
            for (int y = 0; y < maxY; y++)
            {
                for (int x = 0; x < maxX; x++)
                {
                    switch (lines[y][x])
                    {
                        case '^':
                            trucks.Add(new Truck()
                            {
                                XY = new XY(x, y),
                                Direction = Direction.North
                            });
                            break;
                        case '>':
                            trucks.Add(new Truck()
                            {
                                XY = new XY(x, y),
                                Direction = Direction.East
                            });
                            break;
                        case 'v':
                            trucks.Add(new Truck()
                            {
                                XY = new XY(x, y),
                                Direction = Direction.South
                            });
                            break;
                        case '<':
                            trucks.Add(new Truck()
                            {
                                XY = new XY(x, y),
                                Direction = Direction.West
                            });
                            break;

                        default:
                            break;
                    }
                }
            }

            return trucks;
        }

        public class Truck 
        {
            public static int NextId = 1;
            public int Id;
            public XY XY;
            public Direction Direction;
            public Choice NextChoice;
            public bool Dead;

            public Truck()
            {
                Id = NextId++;
            }

            public override string ToString()
            {
                return string.Format("{0}:{1}{2}:Next={3}", Id, XY, XY.DirToChar[Direction], NextChoice);
            }

        }

        public enum Choice
        {
            Left = 0,
            Straight = 1,
            Right = 2
        }
    }
}
