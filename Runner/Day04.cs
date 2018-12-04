using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Globalization;

namespace Runner
{
    class Day04 :  Day
    {
        public override string First(string input)
        {
            var lines = input.GetLines("Guard#beginsht[]");
            var instructions = GetInstructions(lines);
            var data = GetAsleepMinuteData(instructions);
            return FindStrategy1(data);
        }

        public override string Second(string input)
        {
            var lines = input.GetLines("Guard#beginsht[]");
            var instructions = GetInstructions(lines);
            var data = GetAsleepMinuteData(instructions);
            return FindStrategy2(data);
        }

        ////////////////////////////////////////////////////////
        
        private class Instr
        {
            public DateTime Time;
            public string Action;

            public override string ToString()
            {
                return string.Format("{0} {1} : {2}", Time.ToLongDateString(), Time.ToShortTimeString(), Action);
            }
        }

        private IEnumerable<Instr> GetInstructions(string[] lines)
        {
            var instructions = new List<Instr>();
            foreach (var line in lines)
            {
                var parts = line.GetParts();
                var timestr = parts[0]+" "+parts[1];
                var action = parts[2];
                var time = DateTime.ParseExact(timestr, "yyyy-MM-dd HH:mm", CultureInfo.CurrentCulture);
                instructions.Add(new Instr() { Time = time, Action = action });
            }

            return instructions.OrderBy(i=>i.Time);
        }

        private class MinuteAsleepData
        {
            public DateTime Day;
            public int Minute;
            public int Guard;
        }

        private IEnumerable<MinuteAsleepData> GetAsleepMinuteData(IEnumerable<Instr> instructions)
        {
            var data = new List<MinuteAsleepData>();
            int guard = -1;
            bool asleep = false;
            int sleepMinute = -1;
            DateTime day = DateTime.MinValue;
            foreach (var i in instructions)
            {
                if (i.Time.Date != day)
                {
                    if (asleep) AddData(data, guard, day, sleepMinute, 60);
                    day = i.Time.Date;
                }
                if (i.Action == "f")
                {
                    asleep = true;
                    sleepMinute = i.Time.Minute;
                }
                else if (i.Action == "w")
                {
                    if (!asleep) throw new InvalidOperationException();
                    AddData(data, guard, day, sleepMinute, i.Time.Minute);
                    asleep = false;
                    sleepMinute = -1;
                }
                else
                {
                    guard = int.Parse(i.Action);
                    if (asleep) AddData(data, guard, day, sleepMinute, 60);
                    day = i.Time.Date;
                    asleep = false;
                    sleepMinute = -1;
                }
            }
            if (asleep) AddData(data, guard, day, sleepMinute, 60);
            return data;
        }

        private void AddData(List<MinuteAsleepData> data, int guard, DateTime day, int asleepMinute, int wakeMinute)
        {
            if (guard == -1 || asleepMinute == -1) throw new InvalidOperationException();
            for (int i = asleepMinute; i < wakeMinute; i++)
            {
                data.Add(new MinuteAsleepData()
                {
                    Day = day.Date,
                    Guard = guard,
                    Minute = i
                });
            }
        }

        private int FindMinute(IEnumerable<MinuteAsleepData> data, int guard)
        {
            var guardData = data.Where(d => d.Guard == guard);
            var answer = guardData.GroupBy(i => i.Minute).OrderByDescending(g => g.Count()).First().Key;
            return answer;
        }

        private int FindSleepiest(IEnumerable<MinuteAsleepData> data)
        {
            var guardData = data.GroupBy(i=>i.Guard);
            var guard = guardData.OrderByDescending(g => g.Count()).First().Key;
            return guard;
        }

        private string FindStrategy1(IEnumerable<MinuteAsleepData> data)
        {
            var guard = FindSleepiest(data);
            var minute = FindMinute(data, guard);
            return (guard * minute).ToString();
        }

        private string FindStrategy2(IEnumerable<MinuteAsleepData> data)
        {
            var answer= data.GroupBy(g => new { g.Guard, g.Minute }).OrderByDescending(g => g.Count()).First().Key;
            return (answer.Guard * answer.Minute).ToString();
        }
    }
}
