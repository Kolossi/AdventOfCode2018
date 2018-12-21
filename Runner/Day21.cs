using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using static Runner.Day16;
using static Runner.Day19;

namespace Runner
{
    class Day21 :  Day
    {
        public override string First(string input)
        {
            LogEnabled = false;
            var program = GetProgram(input, out int ipreg);
            //LogLine(Dissemble(program, ipreg));
            return SimulatedFindTargets(new long[] { 0, 0, 0, 0, 0, 0 }, program, ipreg, firstOnly:true).First.Value.ToString(); // 16128384
            //return FindTargets(new long[] { 0, 0, 0, 0, 0, 0 }, program, ipreg, firstOnly:true).First.Value.ToString(); // 16128384
        }

        public override string Second(string input)
        {
            LogEnabled = false;
            var program = GetProgram(input, out int ipreg);
            //LogLine(Dissemble(program, ipreg));
            return SimulatedFindTargets(new long[] { 0, 0, 0, 0, 0, 0 }, program, ipreg).Last.Value.ToString(); // 7705368
            //return FindTargets(new long[] { 0, 0, 0, 0, 0, 0 }, program, ipreg).Last.Value.ToString(); // 7705368
        }

        public override string SecondTest(string input)
        {
            throw new NotImplementedException("SecondTest");
        }

        ////////////////////////////////////////////////////////

        private LinkedList<long> FindTargets(long[] regs, IEnumerable<Instruction> instructions, int ipreg, bool firstOnly = false)
        {
            var seen = new HashSet<long>();
            var targets = new LinkedList<long>();
            var targetsSeen = new HashSet<long>();
            var program = instructions.ToArray();
            long ptr = 0;
            while (ptr>=0 && ptr < program.Length)
            {
                if (ptr == 6)
                {
                    // it's in a loop, so we are all done
                    if (seen.Contains(regs[4])) return targets;
                    seen.Add(regs[4]);
                }
                else if (ptr == 28)
                {
                    // collect possible exit target values
                    var target = regs[4];
                    if (!targetsSeen.Contains(target))
                    {
                        targetsSeen.Add(target);
                        targets.AddLast(target);
                    }
                    if (firstOnly) return targets;
                }

                var instruction = program[ptr];
                if (ipreg >= 0) regs[ipreg] = ptr;
                //if (LogEnabled 
                //    //&& (ptr==8 || ptr==16)
                //    )
                //{
                //    LogLine("PTR = {0:D2} : {1} : {2}",
                //    ptr,
                //    DissembleInstruction(instruction, ipreg).PadRight(35),
                //    regs.Select((r, i) => string.Format("r{0}={1,-8}", i, r)));
                //}
                regs = ExecuteOp(instruction, regs);
                if (ipreg >= 0) ptr = regs[ipreg];
                ptr++;
            }
            return targets;
        }

        public static long[] ExecuteOp(Instruction instruction, long[] regs)
        {
            int resultIndex = (int)instruction.C;
            var a = (((int)OpRegLookup[instruction.OpCode] & 0x10) == 0x10) ? regs[(int)(instruction.A)] : instruction.A;
            var b = (((int)OpRegLookup[instruction.OpCode] & 0x01) == 0x01) ? regs[(int)(instruction.B)] : instruction.B;
            var func = OpFuncLookup[instruction.OpCode];
            var result = func(a, b);
            regs[resultIndex] = result;
            return regs;
        }

        private LinkedList<long> SimulatedFindTargets(long[] regs, IEnumerable<Instruction> instructions, int ipreg, bool firstOnly = false)
        {
            var target = regs[0];
            var ptr = 8;
            var innerLoop = regs[2];
            var innerTarget = regs[3];
            var value = regs[4];
            var counter = regs[5];
            var targets = new LinkedList<long>();
            var targetsSeen = new HashSet<long>();
            HashSet<long> seenValues = new HashSet<long>();

            value = 0;
            do {
                if (seenValues.Contains(value))
                {
                    return targets;
                }
                seenValues.Add(value);
                counter = 0;
                innerTarget = value | 65536;
                value = 3730679;
                do {
                    counter = innerTarget & 255;
                    value = (((value + counter) & 16777215) * 65899) & 16777215;
                    if (innerTarget < 256)
                    {
                        if (!targetsSeen.Contains(value))
                        {
                            targetsSeen.Add(value);
                            targets.AddLast(value);
                        }
                        if (firstOnly) return targets;
                        break;
                    }
                    counter = (int)(innerTarget / 256);
                    innerTarget = counter;
                } while (true);
            } while (true);
        }

    }
}
