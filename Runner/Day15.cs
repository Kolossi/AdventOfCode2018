using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Runner
{
    class Day15 :  Day
    {
        public override string First(string input)
        {
            LogEnabled = false;
            var game = GetGame(input);
            return game.FightResult();
        }

        public override string Second(string input)
        {
            LogEnabled = false;
            var game = GetGame(input);
            return game.BoostElves();
        }

        ////////////////////////////////////////////////////////

  

        public Game GetGame(string input)
        {
            var lines = input.GetLines();
            var game = new Game()
            {
                Size = new XY(lines[0].Length, lines.Length)
            };
            int x=0,y=0;
            foreach (var line in lines)
            {
                x = 0;
                foreach (var c in line)
                {
                    switch (c)
                    {
                        case '#':
                            game.Walls.Set(x, y);
                            break;
                        case 'G':
                            game.Goblins.Set(x, y, new Creature()
                            {
                                XY = new XY(x, y),
                                HP = 200,
                                IsElf = false
                            });
                            break;
                        case 'E':
                            game.Elves.Set(x, y, new Creature()
                            {
                                XY = new XY(x, y),
                                HP = 200,
                                IsElf = true
                            });
                            break;
                        case '.':
                        default:
                            break;
                    }
                    x++;
                }
                y++;
            }
            return game;
        }
        
        public class Game
        {
            public XY Size;
            public Map<object> Walls = new Map<object>();
            public Map<Creature> Goblins = new Map<Creature>();
            public Map<Creature> Elves = new Map<Creature>();

            public Game()
            {
            }

            public Game(Game source)
            {
                Size = source.Size;
                Walls = new Map<object>();
                foreach (var ykey in source.Walls.Data.Keys)
                {
                    foreach (var xkey in source.Walls.Data[ykey].Keys)
                    {
                        Walls.Set(xkey, ykey);
                    }
                }
                Goblins = new Map<Creature>();
                foreach (var ykey in source.Goblins.Data.Keys)
                {
                    foreach (var xkey in source.Goblins.Data[ykey].Keys)
                    {
                        var sourceGoblin = source.Goblins.Get(xkey, ykey);
                        Goblins.Set(xkey, ykey, new Creature()
                        {
                            HP = sourceGoblin.HP,
                            XY = new XY(sourceGoblin.XY)
                        });
                    }
                }
                Elves = new Map<Creature>();
                foreach (var ykey in source.Elves.Data.Keys)
                {
                    foreach (var xkey in source.Elves.Data[ykey].Keys)
                    {
                        var sourceElf = source.Elves.Get(xkey, ykey);
                        Elves.Set(xkey, ykey, new Creature()
                        {
                            HP = sourceElf.HP,
                            IsElf = true,
                            XY = new XY(sourceElf.XY)
                        });
                    }
                }
            }

            public IOrderedEnumerable<Creature> GetAllCreatures()
            {
                return Goblins.GetAll()
                    .Union(Elves.GetAll())
                    .OrderBy(i=>i.XY.Y)
                    .ThenBy(i=>i.XY.X);
            }

            public string BoostElves()
            {
                Game boostGame;
                int elfPower = 4;
                int numElves = Elves.Count;
                do
                {
                    boostGame = new Game(this);
                    var result = boostGame.FightResult(elfPower);
                    if (boostGame.Elves.Count == numElves) return result;
                    elfPower++;
                } while (true);
            }

            public string FightResult(int elfPower = 3)
            {
                bool gameOver = false;
                int round = 0;
                do
                {
                    //LogLine("============round:{0}", round);
                    //if (LogEnabled) LogLine(this.ShowState());
                    foreach (var creature in GetAllCreatures())
                    {
                        if (creature.HP==0) continue;
                        var creatureMap = (creature.IsElf) ? Elves : Goblins;
                        var opponentsMap = (creature.IsElf) ? Goblins : Elves;
                        if (opponentsMap.Count == 0)
                        {
                            gameOver = true;
                            break;
                        }

                        var isInRange = DirectionsToTry
                            .Select(d => creature.XY.Move(d))
                            .Any(t => opponentsMap.Has(t));

                        if (!isInRange)
                        {
                            var openSquares = opponentsMap
                            .GetAll()
                            .SelectMany(o => DirectionsToTry.Select(d => (new XY(o.XY)).Move(d)))
                            .Where(s => !Walls.Has(s) && !Goblins.Has(s) && !Elves.Has(s));

                            if (!openSquares.Any(o => o == creature.XY))
                            {
                                //move
                                var targetPath = FindTargetPath(creature.XY, openSquares);
                                if (targetPath == null) continue;
                                creatureMap.Remove(creature.XY);
                                creature.XY = targetPath.Points.First.Next.Value;
                                creatureMap.Set(creature.XY, creature);
                            }
                        }
                        var targetsInRange = DirectionsToTry
                            .Select(d => creature.XY.Move(d))
                            .Where(t => opponentsMap.Has(t))
                            .Select(t => opponentsMap.Get(t))
                            .OrderBy(o => o.HP)
                            .ThenBy(o => o.XY.Y)
                            .ThenBy(o => o.XY.X);

                        var target = targetsInRange.FirstOrDefault();

                        if (target != null)
                        {
                            var newHP = target.HP - (creature.IsElf ? elfPower : 3);
                            target.HP = newHP;
                            if (newHP<=0)
                            {
                                opponentsMap.Remove(target.XY);
                                target.HP = 0;
                            }
                        }
                    }
                    round++;
                } while (!gameOver);
                LogLine("elfPower={0},rounds = {1}, HP=", elfPower, round - 1, GetAllCreatures().Sum(c => c.HP));
                LogLine(ShowState());
                return ((round-1) * GetAllCreatures().Sum(c => c.HP)).ToString();
            }

            private readonly Direction[] DirectionsToTry = new Direction[] {
                Direction.North,
                Direction.West,
                Direction.East,
                Direction.South
            };

            private Path FindTargetPath(XY start, IEnumerable<XY> targetList)
            {
                var targets = new Map<bool>(targetList);
                Map<int> visitDistances = new Map<int>();
                List<Path> paths = new List<Path>();
                Queue<Path> pathsToProcess = new Queue<Path>();
                pathsToProcess.Enqueue((new Path()).Move(start));
                int shortest = int.MaxValue;
                while(pathsToProcess.Any())
                {
                    var path = pathsToProcess.Dequeue();
                    if (path.Length >= shortest) continue;
                    var xy = path.XY;
                    foreach (var direction in DirectionsToTry)
                    {
                        var newXY = xy.Move(direction);
                        if (newXY == start) continue;
                        if (Walls.Has(newXY)) continue;
                        int currentShortestToPos;
                        if (visitDistances.TryGetValue(newXY, out currentShortestToPos) && currentShortestToPos <= path.Length + 1) continue;
                        visitDistances.Set(newXY, path.Length + 1);
                        if (targets.Has(newXY))
                        {
                            var newPath = (new Path(path)).Move(newXY);
                            if (newPath.Length < shortest)
                            {
                                shortest = newPath.Length;
                                paths = new List<Path>();
                            }
                            paths.Add(newPath);
                            continue;
                        }
                        if (Goblins.Has(newXY)) continue;
                        if (Elves.Has(newXY)) continue;
                        pathsToProcess.Enqueue((new Path(path)).Move(newXY));
                    }
                }
                return paths.OrderBy(p => p.Points.First.Next.Value.Y).ThenBy(p => p.Points.First.Next.Value.X).FirstOrDefault();
            }

            public override string ToString()
            {
                return string.Format("Size={0},Goblins={1},Elves={2}", Size, Goblins.GetAll().Count(), Elves.GetAll().Count());
            }

            public string ShowState()
            {
                var sb = new StringBuilder();
                for (int y = 0; y < Size.Y; y++)
                {
                    var creatures = new List<Creature>();
                    for (int x = 0; x < Size.X; x++)
                    {
                        Creature creature;
                        if (Walls.Has(x, y))
                        {
                            sb.Append('#');
                        }
                        else if (Goblins.TryGetValue(x, y, out creature) || Elves.TryGetValue(x, y, out creature))
                        {
                            sb.Append(creature.TypeChar);
                            creatures.Add(creature);
                        }
                        else
                        {
                            sb.Append('.');
                        }
                    }
                    if (creatures.Any())
                    {
                        sb.Append(" ");
                        foreach (var creature in creatures)
                        {
                            sb.Append(creature.TypeChar).Append("(").Append(creature.HP).Append("), ");
                        }
                    }
                    sb.AppendLine();
                }
                return sb.ToString();
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

            public override string ToString()
            {
                return string.Format("({0}):{1}", Length, string.Join(",", Points));
            }
        }

        public class Creature
        {
            public XY XY;
            public int HP = 200;
            public bool IsElf;

            public char TypeChar
            {
                get
                {
                    return IsElf ? 'E' : 'G';
                }
            }

            public override string ToString()
            {
                return string.Format("{0}:{1},HP={2}", TypeChar, XY, HP);
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

            public IEnumerable<T> GetAll()
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
