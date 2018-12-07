using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Runner
{
    class Day07 :  Day
    {
        public override string First(string input)
        {
            var instructions = input.GetLines("mustbefinishedbeforestepcanbegin.");
            var deps = GetDeps(instructions);
            return Process(deps);
        }

        public override string Second(string input)
        {
            var instructions = input.GetLines("mustbefinishedbeforestepcanbegin.");
            var deps = GetDeps(instructions);
            return Solve(deps,5,-4);
        }


        public override string SecondTest(string input)
        {
            var instructions = input.GetLines("mustbefinishedbeforestepcanbegin.");
            var deps = GetDeps(instructions);
            return Solve(deps,1,-64);
        }

        ////////////////////////////////////////////////////////

        private class Elf
        {
            public char workingOn;
            public int finishAt;

            public override string ToString()
            {
                return string.Format("{0} until {1}", workingOn, finishAt);
            }
        }

        private class Dep
        {
            public char Step;
            public HashSet<char> Prereqs;

            public override string ToString()
            {
                return string.Format("{0} -> {1}", Step, string.Join("", Prereqs));
            }
        }

        private Dictionary<char,Dep> GetDeps(string[] instructions)
        {
            var deps = new Dictionary<char, Dep>();
            foreach (var instr in instructions)
            {
                var parts = instr.GetParts();
                var prereq = parts[1][0];
                var step = parts[2][0];
                AddPrereq(deps, step, prereq);
            }
            return deps;
        }

        private string Solve(Dictionary<char, Dep> deps, int numElves, int stepDurationAsciiOffset)
        {
            var timeNow = 0;
            var elves = new List<Elf>();
            for (int i = 0; i < numElves+1; i++) //#MeToo!
            {
                elves.Add(new Elf() { workingOn = ' ' });
            }

            while(true)
            {
                FinishWork(deps, timeNow, elves);
                if (!deps.Any()) break;
                AssignWork(deps, stepDurationAsciiOffset, elves, timeNow);
                timeNow++;
            }
            return timeNow.ToString();
        }

        private static void FinishWork(Dictionary<char, Dep> deps, int timeNow, List<Elf> elves)
        {
            var finished = elves.Where(e => e.finishAt > 0 && e.finishAt == timeNow);
            foreach (var elf in finished)
            {
                var step = elf.workingOn;
                CompleteStep(deps, step);
                elf.finishAt = 0;
                elf.workingOn = ' ';
            }
        }

        private static void CompleteStep(Dictionary<char, Dep> deps, char step)
        {
            foreach (var dep in deps.Values.Where(d => d.Prereqs.Contains(step)))
            {
                dep.Prereqs.Remove(step);
            }
            deps.Remove(step);
        }

        private static void AssignWork(Dictionary<char, Dep> deps, int stepDurationAsciiOffset, List<Elf> elves, int timeNow)
        {
            var elvesIdle = elves.Where(e => e.workingOn == ' ');
            if (elvesIdle.Any())
            {
                var ready = new Queue<Dep>(deps.Values
                    .Where(d => !d.Prereqs.Any()
                        && !elves.Any(e=>e.workingOn==d.Step))
                    .OrderBy(d => d.Step));
                foreach (var elf in elvesIdle)
                {
                    if (!ready.Any()) break;
                    var step = ready.Dequeue().Step;
                    elf.workingOn = step;
                    elf.finishAt = timeNow + (int)step + stepDurationAsciiOffset;
                }
            }
        }

        private string Process(Dictionary<char,Dep> deps)
        {
            var sequence = new List<char>();
            while (true)
            {
                var next = deps.Values.Where(d => !d.Prereqs.Any()).OrderBy(d=>d.Step).FirstOrDefault();
                if (next == null) break;
                var step = next.Step;
                sequence.Add(next.Step);
                CompleteStep(deps, step);
            }
            if (deps.Any()) throw new InvalidOperationException();
            return string.Join("",sequence);
        }

        private void AddPrereq(Dictionary<char, Dep> deps, char step, char prereq)
        {
            Dep dep;
            HashSet<char> prereqs;
            if (!deps.TryGetValue(step, out dep))
            {
                prereqs = new HashSet<char>();
                dep = new Dep() { Step = step, Prereqs = prereqs };
                deps[step] = dep;
            }
            else
            {
                prereqs = dep.Prereqs;
            }
            prereqs.Add(prereq);
            if (!deps.ContainsKey(prereq))
            {
                deps[prereq] = new Dep()
                {
                    Step = prereq,
                    Prereqs = new HashSet<char>()
                };
            }
        }
    }
}
