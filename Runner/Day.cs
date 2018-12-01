using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Runner
{
    public abstract class Day
    {
        public abstract string First(string input);
        public abstract string Second(string input);

        public string Solve(string set, Func<string,string> solver)
        {
            string input = string.Empty;
            try
            {
                input = GetInput(set);
            }
            catch (FileNotFoundException)
            {
            }

            if (string.IsNullOrWhiteSpace(input)) return "INPUT MISSING";

            try
            {
                Stopwatch sw = Stopwatch.StartNew();
                var result =  solver(input);
                sw.Stop();
                Console.Write("(Took : {0}) ",sw.Elapsed);
                return result;
            }
            catch (NotImplementedException)
            {
                return "NOT IMPLEMENTED";
            }
        }

        public string SolveFirst()
        {
            return Solve("First", this.First);
        }

        public string SolveSecond()
        {
            return Solve("Second", this.Second);
        }

        public virtual string FirstTest(string input)
        {
            return this.First(input);
        }

        public virtual string SecondTest(string input)
        {
            return this.Second(input);
        }

        public bool TestFirst()
        {
            return Test("First", this.FirstTest);
        }

        public bool TestSecond()
        {
            return Test("Second", this.SecondTest);
        }

        public bool Test(string set, Func<string, string> solver)
        {
            bool result = true;
            string input = string.Empty;

            Console.WriteLine(string.Format("    {0} Tests", set));
            try
            {
                input = GetInput(string.Format("{0}Tests", set));
            }
            catch (FileNotFoundException)
            {
            }

            if (string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine("      TEST INPUT MISSING");
                return false;
            }

            var lines = input.Split("\n\r".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            if (lines[lines.Length-1][0]==':')
            {
                lines = new string[1]{
                    string.Join("\r\n",lines.Take(lines.Length-1))+lines[lines.Length-1]
                };
            }

            foreach (var line in lines)
            {

                int colon = line.LastIndexOf(":");
                var parts = line.Split(":");
                var testInput = line.Substring(0,colon);
                var expectedOutput = line.Substring(colon+1,line.Length-colon-1);
                string output = string.Empty;
                try
                {
                    output = solver(testInput);
                    if (output != expectedOutput)
                    {
                        result = false;
                        Console.WriteLine(string.Format("    {0} : FAILED", line));
                        Console.WriteLine(string.Format("      Input : {0}, Expected : {1}, Got : {2}", testInput,
                            expectedOutput, output));
                    }
                    else
                    {
                        Console.WriteLine(string.Format("    {0} : OK", line));
                    }
                }
                catch (NotImplementedException)
                {
                    Console.WriteLine(string.Format("    {0} : NOT IMPLEMENTED", line));
                    result = false;
                }
            }
            return result;
        }

        public string GetInput(string set)
        {
            string filename = string.Format("Inputs/{0}{1}.txt",this.GetType().Name, set);
            if (set == "Second" && !File.Exists(filename)) return GetInput("First");
            string input = System.IO.File.ReadAllText(filename);
            if (set == "Second" && input == "FIRST") return GetInput("First");
            return input;
        }

        public static string[] GetLines(string input, string removeChars = null)
        {
            return input.GetLines(removeChars);
        }

        public static string[] GetParts(string input, string removeChars = null)
        {
            return input.GetParts(removeChars);
        }
    }

    public static class DayUtils
    {
        public static string[] GetParts(this string input, string removeChars=null)
        {
            if (!string.IsNullOrEmpty(removeChars))
            {
                foreach (var c in removeChars)
                {
                    input = input.Replace(c, ' ');
                }
            }
            return input.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
        }

        public static string[] GetLines(this string input, string removeChars = null)
        {
            if (!string.IsNullOrEmpty(removeChars))
            {
                foreach (var c in removeChars)
                {
                    input = input.Replace(c, ' ');
                }
            }
            return input.Split("\n\r".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        }

    }
}
