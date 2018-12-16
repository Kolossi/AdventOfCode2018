using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Runner
{
    class Day16 : Day
    {
        public override string First(string input)
        {
            LogEnabled = false;
            IEnumerable<OpTest> opTests = GetOpTests(input);
            return opTests.Count(t => FindPossibleOps(t).Count() >= 3).ToString();
        }

        public override string Second(string input)
        {
            LogEnabled = false;
            var numberOpCodeLookup = GetNumberOpCodeLookup(input);
            var instructions = GetProgram(input);
            long[] regs = Execute(new long[4]{0,0,0,0}, instructions, numberOpCodeLookup);
            return regs[0].ToString();
        }

        public override string FirstTest(string input)
        {
            LogEnabled = false;
            IEnumerable<OpTest> opTests = GetOpTests(input);
            return FindPossibleOps(opTests.First()).Count().ToString();
        }

        public override string SecondTest(string input)
        {
            LogEnabled = false;
            IEnumerable<OpTest> opTests = GetOpTests(input);
            var result = FindPossibleOps(opTests.First());
            LogLine("Possible:{0}", result);
            return string.Join(",",result);
        }

        ////////////////////////////////////////////////////////

        private long[] Execute(long[] regs, IEnumerable<long[]> instructions, Dictionary<long, Op> numberOpCodeLookup)
        {
            foreach (var instruction in instructions)
            {
                regs = ExecuteOp(numberOpCodeLookup[instruction[0]], instruction, regs);
            }
            return regs;
        }

        private Dictionary<long, Op> GetNumberOpCodeLookup(string input)
        {
            var possibleLookups = new Dictionary<long, List<Op>>();
            var tests = GetOpTests(input);
            foreach (var test in tests)
            {
                var number = test.Instruction[0];
                var ops = FindPossibleOps(test);
                possibleLookups[number] = (possibleLookups.ContainsKey(number) ? possibleLookups[number] : Enumerable.Empty<Op>())
                                            .Union(ops)
                                            .Distinct()
                                            .ToList();
            }

            var lookup = GetLookupFromPossibles(possibleLookups);
            return lookup;
        }

        private Dictionary<long, Op> GetLookupFromPossibles(Dictionary<long, List<Op>> possibleLookups)
        {
            var lookup = new Dictionary<long, Op>();
                            if (LogEnabled) ShowState(possibleLookups, lookup);

            foreach (Op opcode in Enum.GetValues(typeof(Op)))
            {
                var numbers = possibleLookups.Where(kv => kv.Value.Contains(opcode)).Select(kv => kv.Key);
                if (numbers.Count()==1)
                {
                    var number = numbers.First();
                    lookup[number] = opcode;
                    possibleLookups.Remove(number);
                    foreach (var k in possibleLookups.Keys.ToArray())
                    {
                        possibleLookups[k] = possibleLookups[k].Where(o => o != opcode).ToList();
                        if (!possibleLookups[k].Any()) throw new InvalidOperationException();
                    }
                }
            }
            do
            {
                if (LogEnabled) ShowState(possibleLookups, lookup);

                var resultKeys = possibleLookups.Where(kv => kv.Value.Count() == 1).Select(kv=>kv.Key);
                foreach (var resultKey in resultKeys.ToList())
                {
                    if (!possibleLookups.ContainsKey(resultKey)) continue;
                    var result = possibleLookups[resultKey];
                    lookup[resultKey] = result.First();
                    foreach (var key in possibleLookups.Keys.ToList())
                    {
                        if (possibleLookups[key].Count() == 1) continue;
                        possibleLookups[key] = possibleLookups[key].Except(result).ToList();
                    }
                    possibleLookups.Remove(resultKey);

                    if (LogEnabled)
                    {
                        LogLine("Removed {0}={1}:", resultKey, result.First());
                        ShowState(possibleLookups, lookup);
                    }
                }
            } while (possibleLookups.Any());

            foreach (Op opcode in Enum.GetValues(typeof(Op)))
            {
                if (!lookup.Values.Contains(opcode)) throw new InvalidOperationException();
            }
            return lookup;
        }

        private static void ShowState(Dictionary<long, List<Op>> possibleLookups, Dictionary<long, Op> lookup)
        {
            foreach (var kv in possibleLookups.OrderBy(kv=>kv.Value.Count()))
            {
                LogLine(string.Format("{0:D2}: {1}", kv.Key, string.Join(", ", kv.Value.Select(v => v.ToString("G")))));
            }
            LogLine("lookup={{{0}}}", string.Join("}, {", lookup.OrderBy(kv=>kv.Key).Select(kv => string.Format("{0}:{1}", kv.Key, kv.Value))));
        }

        private IEnumerable<long[]> GetProgram(string input)
        {
            var lines = input.GetLines().AsEnumerable();
            do
            {
                lines = lines.Skip(3);
            } while (lines.First().StartsWith("Before:"));
            var instructions = lines
                .Select(l => l.GetParts())
                .Select(p => new long[4]{
                long.Parse(p[0]),
                long.Parse(p[1]),
                long.Parse(p[2]),
                long.Parse(p[3])});
            return instructions;
        }

        private IEnumerable<OpTest> GetOpTests(string input)
        {
            var lines = input.GetLines().AsEnumerable();
            var opTests = new List<OpTest>();
            do
            {
                var testLines = lines.Take(3).ToArray();
                lines = lines.Skip(3);
                opTests.Add(new OpTest()
                {
                    Before = testLines[0].GetParts("Before:[],").Select(p => long.Parse(p)).ToArray(),
                    Instruction = testLines[1].GetParts().Select(p => long.Parse(p)).ToArray(),
                    After = testLines[2].GetParts("After:[],").Select(p => long.Parse(p)).ToArray()
                });

            } while (lines.Count() >= 3 && lines.First().StartsWith("Before:"));
            return opTests;
        }

        public List<Op> FindPossibleOps(OpTest test)
        {
            var possible = new List<Op>();
            foreach (Op opcode in Enum.GetValues(typeof(Op)))
            {
                var regs = test.Before.ToArray();
                regs = ExecuteOp(opcode, test.Instruction, regs);
                if (Compare(regs, test.After)) possible.Add(opcode);
            }
            //if (possible.Count() == 0) throw new InvalidOperationException();
            return possible;
        }

        public bool Compare(long[] left, long[] right)
        {
            if (left.Length != right.Length) return false;
            for (int i = 0; i < left.Length; i++)
            {
                if (left[i] != right[i]) return false;
            }
            return true;
        }

        public long[] ExecuteOp(Op opcode, long[] instruction, long[] regs)
        {
            int resultIndex = (int)instruction[3];
            var a = (((int)OpRegLookup[opcode] & 0x10) == 0x10) ? regs[(int)(instruction[1])] : instruction[1];
            var b = (((int)OpRegLookup[opcode] & 0x01) == 0x01) ? regs[(int)(instruction[2])] : instruction[2];
            var func = OpFuncLookup[opcode];
            var result = func(a, b);
            regs[resultIndex] = result;
            return regs;
        }

        public class OpTest
        {
            public long[] Before;
            public long[] After;
            public long[] Instruction;

            public override string ToString()
            {
                return string.Format("Before: [{0}], {1}, After: [{2}]",
                    string.Join(", ", Before),
                    string.Join(" ", Instruction),
                    string.Join(", ", After));
            }
        }

        public enum OpRegs
        {
            ValAValB = 0x00,
            ValARegB = 0x01,
            RegAValB = 0x10,
            RegARegB = 0x11
        }

        public enum Op
        {
            addr,
            addi,
            mulr,
            muli,
            banr,
            bani,
            borr,
            bori,
            setr,
            seti,
            gtir,
            gtri,
            gtrr,
            eqir,
            eqri,
            eqrr
        }

        public Dictionary<Op, OpRegs> OpRegLookup = new Dictionary<Op, OpRegs>()
        {
            {Op.addr,OpRegs.RegARegB},
            {Op.addi,OpRegs.RegAValB},
            {Op.mulr,OpRegs.RegARegB},
            {Op.muli,OpRegs.RegAValB},
            {Op.banr,OpRegs.RegARegB},
            {Op.bani,OpRegs.RegAValB},
            {Op.borr,OpRegs.RegARegB},
            {Op.bori,OpRegs.RegAValB},
            {Op.setr,OpRegs.RegAValB},
            {Op.seti,OpRegs.ValAValB},
            {Op.gtir,OpRegs.ValARegB},
            {Op.gtri,OpRegs.RegAValB},
            {Op.gtrr,OpRegs.RegARegB},
            {Op.eqir,OpRegs.ValARegB},
            {Op.eqri,OpRegs.RegAValB},
            {Op.eqrr,OpRegs.RegARegB}
        };

        public Dictionary<Op, Func<long, long, long>> OpFuncLookup = new Dictionary<Op, Func<long, long, long>>()
        {
            {Op.addr,(a, b) => a+b},
            {Op.addi,(a, b) => a+b},
            {Op.mulr,(a, b) => a*b},
            {Op.muli,(a, b) => a*b},
            {Op.banr,(a, b) => a&b},
            {Op.bani,(a, b) => a&b},
            {Op.borr,(a, b) => a|b},
            {Op.bori,(a, b) => a|b},
            {Op.setr,(a, b) => a},
            {Op.seti,(a, b) => a},
            {Op.gtir,(a, b) => (a>b)?1:0},
            {Op.gtri,(a, b) => (a>b)?1:0},
            {Op.gtrr,(a, b) => (a>b)?1:0},
            {Op.eqir,(a, b) => (a==b)?1:0},
            {Op.eqri,(a, b) => (a==b)?1:0},
            {Op.eqrr,(a, b) => (a==b)?1:0},
        };
    }

    public static class ShowRegs
    {
        public static string ToString(this long[] regs)
        {
            return string.Join(" ", regs);
        }
    }
}
