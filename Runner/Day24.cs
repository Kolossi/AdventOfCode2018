using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Runner
{
    class Day24 :  Day
    {
        public override string First(string input)
        {
            LogEnabled = false;
            var combat = GetCombat(input.GetLines());
            LogLine(combat);
            combat.FightToEnd();
            return (combat.Infection.Sum(i=>i.Units)+combat.Immune.Sum(i=>i.Units)).ToString();
        }

        public override string Second(string input)
        {
            LogEnabled = true;
            var lines = input.GetLines();
            var combat = FindBoostCombat(lines);
            return combat.Immune.Sum(i=>i.Units).ToString(); // not 48
        }

        private Combat FindBoostCombat(string[] lines)
        {
            int boostChange = 1024;
            int boost = boostChange*2;
            Combat combat = null;
            while (Math.Abs(boostChange) >= 1)
            {
                combat = GetCombat(lines, boost);
                try
                {
                    combat.FightToEnd();
                }
                catch (InvalidOperationException)
                {
                    LogLine("boost:{0} - lockedup", boost);
                    boost = boost + boostChange;
                    continue;
                }
                var infectionUnits = combat.Infection.Sum(i => i.Units);
                LogLine("boost:{0} - infection units = {1}", boost, infectionUnits);
                if (boostChange > 1) boostChange /= 2;
                boostChange = Math.Abs(boostChange);
                if (infectionUnits == 0)
                {
                    boostChange = -boostChange;
                }
                boost = boost + boostChange;
            }
            return combat;
        }

        public override string SecondTest(string input)
        {
            var lines = input.GetLines();
            var boost = int.Parse(lines[0]);
            var combat = GetCombat(lines.Skip(1), boost);
            combat.FightToEnd();
            return (combat.Infection.Sum(i=>i.Units)+combat.Immune.Sum(i=>i.Units)).ToString();
        }

        ////////////////////////////////////////////////////////
        
        public class Combat
        {
            public int ImmuneBoost = 0;
            public List<Group> Immune = new List<Group>();
            public List<Group> Infection = new List<Group>();
            public override string ToString()
            {
                var sb = new StringBuilder();
                sb.AppendLine("Immune System:");
                foreach (var group in Immune)
                {
                    sb.AppendLine(group.ToString());
                }
                sb.AppendLine().AppendLine("Infection:");
                foreach (var group in Infection)
                {
                    sb.AppendLine(group.ToString());
                }
                return sb.ToString();
            }

            public void FightToEnd()
            {
                while (Immune.Any(i => i.Units != 0) && Infection.Any(i => i.Units != 0))
                {
                    Fight();
                    //LogLine("---");
                }
            }

            public void Fight()
            {
                GetTargets(Immune, Infection);
                GetTargets(Infection, Immune);

                bool anyKilled = false;
                foreach (var attacker in Immune.Union(Infection).OrderByDescending(i=>i.Initiative))
                {
                    if (attacker.Units == 0) continue;
                    var defender = attacker.Target;
                    if (defender==null) continue;
                    var unitsKilled = attacker.DoAttack();
                    if (unitsKilled >= 1) anyKilled = true;
                    //LogLine("{0} {1}->{2} kills {3}", attacker.IsInfection ? "Infection":"Immune",attacker.Id, defender.Id, unitsKilled);
                    attacker.Target = null;
                    if (defender != null && defender.Units == 0)
                    {
                        (defender.IsInfection ? Infection : Immune).Remove(defender);
                    }
                }
                if (!anyKilled) throw new InvalidOperationException();
            }

            private void GetTargets(List<Group> attackers, List<Group> defenders)
            {
                var availableTargets = new LinkedList<Group>(defenders);

                foreach (var attacker in attackers
                    .OrderByDescending(a=>a.EffectivePower)
                    .ThenByDescending(a=>a.Initiative))
                {
                    var defender = availableTargets
                        .OrderByDescending(d=>attacker.DamageDealt(d))
                        .ThenByDescending(d=>d.EffectivePower)
                        .ThenByDescending(d=>d.Initiative)
                        .FirstOrDefault();
                    attacker.Target = defender;
                    if (defender != null && attacker.DamageDealt(defender)>0)
                    {
                        availableTargets.Remove(defender);
                    }
                }
            }
        }

        public Combat GetCombat(IEnumerable<string> lines, int boost = 0)
        {
            var combat = new Combat();
            combat.ImmuneBoost = boost;
            var list = combat.Immune;
            var isInfection = false;
            foreach (var line in lines.Skip(1))
            {
                if (line.StartsWith("Infection"))
                {
                    isInfection = true;
                    Group.NextId = 1;
                    list = combat.Infection;
                    continue;
                }
                list.Add(new Group(line, isInfection, boost));
            }
            return combat;
        }
            
        // 4485 units each with 2961 hit points (immune to radiation; weak to fire, cold) with an attack that does 12 slashing damage at initiative 4
        public class Group
        {
            public static int NextId = 1;
            public int Id;
            public int Units;
            public int HP;
            public int Initiative;
            public int Damage;
            public string Attack;
            public List<string> Weaknesses = new List<string>();
            public List<string> Immunties = new List<string>();
            public bool IsInfection;
            public Group Target;


            public int EffectivePower
            {
                get
                {
                    return Units * Damage;
                }
            }

            public Group(string line, bool isInfection, int immuneBoost)
            {
                var parts = line.GetParts();
                Id = NextId++;
                IsInfection = isInfection;
                Units = int.Parse(parts[0]);
                HP = int.Parse(parts[4]);
                Initiative = int.Parse(parts[parts.Length - 1]);
                Damage = int.Parse(parts[parts.Length - 6]) + (isInfection ? 0 : immuneBoost);
                Attack = parts[parts.Length - 5].ToLower();
                if (!parts[7].StartsWith("(")) return;
                var startMods = line.IndexOf("(") + 1;
                var modsString = line.Substring(startMods, line.IndexOf(")", startMods) - startMods);
                foreach (var modPart in modsString.Split(";").Select(s => s.Trim().ToLower()))
                {
                    var modList = modPart.StartsWith("weak") ? Weaknesses : Immunties;
                    foreach (var mod in modPart.Split().Skip(2).Select(s => s.Replace(",","")))
                    {
                        modList.Add(mod);
                    }
                }
            }

            public override string ToString()
            {
                var modString = !Weaknesses.Any() && !Immunties.Any() ? "" :
                    string.Format("({0}{1}) ",
                    Weaknesses.Any() ? string.Format("weak to {0}{1}", string.Join(", ", Weaknesses), Immunties.Any() ? "; " : "") : "",
                    Immunties.Any() ? string.Format("immune to {0}", string.Join(", ", Immunties)) : "");
                return string.Format("{0} Group {1}:{2} units each with {3} hit points {4}with an attack that does {5} {6} damage at initiative {7}",
                    IsInfection ? "Infection" : "Immune",
                    Id,
                    Units,
                    HP,
                    modString,
                    Damage,
                    Attack,
                    Initiative);
            }

            internal int DamageDealt(Group defender)
            {
                return EffectivePower
                    * (defender.Immunties.Contains(Attack) ? 0 : 1)
                    * (defender.Weaknesses.Contains(Attack) ? 2 : 1);
            }

            internal int DoAttack()
            {
                if (Target == null) return 0;
                var damage = DamageDealt(Target);
                var damageUnits = (int)(damage / Target.HP);
                var unitsKilled = Math.Min(damageUnits, Target.Units);
                Target.Units -= unitsKilled;
                return unitsKilled;
            }
        }
    }
}
