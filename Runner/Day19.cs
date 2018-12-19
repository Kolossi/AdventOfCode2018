using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using static Runner.Day16;

namespace Runner
{
    class Day19 : Day
    {
        public override string First(string input)
        {
            LogEnabled = false;
            var program = GetProgram(input, out int ipreg);
            return Execute(new long[] { 0, 0, 0, 0, 0, 0 }, program, ipreg)[0].ToString();
        }

        public override string Second(string input)
        {
            //LogEnabled = true;
            var factorProgram = GetProgram(input, out int ipreg);
            if (LogEnabled) LogLine(Dissemble(factorProgram, ipreg));
            int[] values = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 1000, 10000 };
            //int[] values = new int[] { 2 };
            foreach (var value in values)
            {
                factorProgram = ChangeTarget(factorProgram, value);
                Console.WriteLine("bugfix {0}={1}",value,Execute(new long[] { 1, 0, 0, 0, 0, 0 }, factorProgram, ipreg)[0]);
            }
            return @"Part 2 answer is 18869760.  This is the sum of the factors of 10551398 - see .\AdventOfCode2018\Runner\ManualAnalysis\Day19Code.txt for details";

            // ChangeTarget:
            //bugfix 0=0
            //bugfix 1=1
            //bugfix 2=3
            //bugfix 3=4
            //bugfix 4=7
            //bugfix 5=6
            //bugfix 6=12
            //bugfix 7=8
            //bugfix 8=15
            //bugfix 9=13
            //bugfix 10=18
            //bugfix 1000=2340
            //bugfix 10000=24211
        }


        ////////////////////////////////////////////////////////

        private IEnumerable<Instruction> ChangeTarget(IEnumerable<Instruction> program, int value)
        {
            if (program.Count() < 34) return program;
            var progArray = program.ToArray();
            progArray[33].OpCode = Op.seti;
            progArray[33].A = value;
            return progArray;
            
        }
        private IEnumerable<Instruction> GetProgram(string input, out int ipreg)
        {
            var lines = input.GetLines().AsEnumerable();

            ipreg = -1;
            var firstLine = lines.First();
            if (firstLine.StartsWith("#ip"))
            {
                ipreg = int.Parse(firstLine.GetParts()[1]);
                lines = lines.Skip(1);
            }
            var instructions = lines
                .Select(l => l.GetParts())
                .Select(p => new Instruction{
                OpCode = (Op)Enum.Parse(typeof(Op),p[0]),
                A = long.Parse(p[1]),
                B = long.Parse(p[2]),
                C = long.Parse(p[3])});
            return instructions;
        }

        
        private long[] Execute(long[] regs, IEnumerable<Instruction> instructions, int ipreg)
        {
            var program = instructions.ToArray();
            long ptr = 0;
            while (ptr>=0 && ptr < program.Length)
            {
                var instruction = program[ptr];
                if (ipreg >= 0) regs[ipreg] = ptr;
                if (LogEnabled) LogLine("PTR = {0:D2} : {1} : {2}", 
                    ptr,
                    DissembleInstruction(instruction, ipreg).PadRight(35), 
                    regs.Select((r,i)=>string.Format("r{0}={1,-8}",i,r)));
                regs = ExecuteOp(instruction.OpCode, new long[] { 0, instruction.A, instruction.B, instruction.C }, regs);
                if (ipreg >= 0) ptr = regs[ipreg];
                ptr++;
            }
            return regs;
        }

        public static string Dissemble(IEnumerable<Instruction> instructions, int ipreg)
        {
            var sb = new StringBuilder();
            var i = 0;
            foreach (var instruction in instructions)
            {
                sb.AppendFormat("{0}: {1}", i++, DissembleInstruction(instruction, ipreg)).AppendLine();
            }
            return sb.ToString();
        }

        public static string DissembleInstruction(Instruction instruction, int ipreg)
        {
            var opcode = instruction.OpCode;
            int resultIndex = (int)instruction.C;
            var a = (((int)OpRegLookup[opcode] & 0x10) == 0x10) ? 
                            ipreg>=0 && instruction.A==ipreg ? "PTR"
                                : "r" + instruction.A.ToString()
                            : instruction.A.ToString();
            var b = (((int)OpRegLookup[opcode] & 0x01) == 0x01) ?
                            ipreg>=0 && instruction.B==ipreg ? "PTR"
                                : "r" + instruction.B.ToString()
                            : instruction.B.ToString();
            var c = ipreg >= 0 && instruction.C == ipreg ? "PTR"
                                : "r" + instruction.C.ToString();
            var formatString = DissemblyLookup[opcode];
            return string.Format(formatString, a, b, c);
        }

        public class Instruction
        {
            public Op OpCode;
            public long A;
            public long B;
            public long C;

        }

        public static Dictionary<Op, string> DissemblyLookup = new Dictionary<Op, string>()
        {
            {Op.addr,"{2} = {0} + {1}" },
            {Op.addi,"{2} = {0} + {1}" },
            {Op.mulr,"{2} = {0} * {1}"},
            {Op.muli,"{2} = {0} * {1}"},
            {Op.banr,"{2} = {0} & {1}"},
            {Op.bani,"{2} = {0} & {1}"},
            {Op.borr,"{2} = {0} | {1}"},
            {Op.bori,"{2} = {0} | {1}"},
            {Op.setr,"{2} = {0}"},
            {Op.seti,"{2} = {0}"},
            {Op.gtir,"if ({1} < {0}) {2} = 1 else {2} = 0"},
            {Op.gtri,"if ({0} > {1}) {2} = 1 else {2} = 0"},
            {Op.gtrr,"if ({0} > {1}) {2} = 1 else {2} = 0"},
            {Op.eqir,"if ({1} == {0}) {2} = 1 else {2} = 0"},
            {Op.eqri,"if ({0} == {1}) {2} = 1 else {2} = 0"},
            {Op.eqrr,"if ({0} == {1}) {2} = 1 else {2} = 0"},
        };
    }
}
