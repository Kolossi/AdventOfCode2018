using System;

namespace Percentiler
{
    class Program
    {
        ////////////////////////////////////////////////
        // paste from http://adventofcode.com/2018/leaderboard/self
        ////////////////////////////////////////////////
        /// 
        /// 
        private static string PersonalStats = @"Day       Time  Rank  Score       Time  Rank  Score
 10   02:09:04  2047      0   02:12:46  2057      0
  9   03:00:53  2260      0   04:57:20  2184      0
  8   03:46:12  2907      0   03:59:15  2656      0
  7   01:09:43  1719      0   01:51:56  1258      0
  6   01:58:11  2009      0   02:18:09  1940      0
  5   01:56:58  3889      0   02:09:24  3540      0
  4   02:00:48  2549      0   02:25:05  2599      0
  3   01:11:01  2728      0   02:01:46  3160      0
  2   01:55:37  3990      0   02:20:35  3581      0
  1   02:04:40  3265      0   02:13:06  2521      0";



        ////////////////////////////////////////////////
        // paste from http://adventofcode.com/2018/stats
        ////////////////////////////////////////////////
        /// 
        /// 
        private static string GlobalStats = @"25      0      0  
24      0      0  
23      0      0  
22      0      0  
21      0      0  
20      0      0  
19      0      0  
18      0      0  
17      0      0  
16      0      0  
15      0      0  
14      0      0  
13      0      0  
12      0      0  
11      0      0  
10   2126     32  ***
 9   7260    940  ******
 8   9576    541  ********
 7  10577   2602  *********
 6  13881    639  **********
 5  20725    797  ***************
 4  20857    730  ***************
 3  29222   1593  *********************
 2  38988   4394  ****************************
 1  49619  12824  *****************************************";

        private static Tuple<int, int>[] PersonalPlacings;

        private static Tuple<int, int>[] GlobalTotals;

        static void Main(string[] args)
        {
            PersonalPlacings = new Tuple<int, int>[25];
            GlobalTotals = new Tuple<int, int>[25];

            foreach (var personalStat in PersonalStats.Split("\n\r".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
            {
                var parts = personalStat.Trim().Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                int day;
                if (!int.TryParse(parts[0], out day)) continue;
                PersonalPlacings[day-1] = new Tuple<int, int>(int.Parse(parts[2]), int.Parse(parts[5]));
            }

            foreach (var globalStat in GlobalStats.Split("\n\r".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
            {
                var parts = globalStat.Trim().Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                int day;
                if (!int.TryParse(parts[0], out day) || day==0) continue;
                GlobalTotals[day-1] = new Tuple<int, int>(int.Parse(parts[1]) + int.Parse(parts[2]), int.Parse(parts[1]));
            }

            var day1Part1 = GlobalTotals[0].Item1;

            for (int i = 24; i >= 0; i--)
            {
                if (PersonalPlacings[i] == null || GlobalTotals[i] == null) continue;
                Console.WriteLine("{0,5}  {1,5}/{2,5} = {3:f0}% of day,   {4:f0}% of all       {5,5}/{6,5} = {7:f0}% of day,   {4:f0}% of all", i+1,
                    PersonalPlacings[i].Item1, GlobalTotals[i].Item1,
                    100.0F * (1.0F - (float)PersonalPlacings[i].Item1 / (float) GlobalTotals[i].Item1),
                    100.0F * (1.0F - (float)PersonalPlacings[i].Item1 / (float) day1Part1),
                    PersonalPlacings[i].Item2, GlobalTotals[i].Item2,
                    100.0F * (1.0F - (float)PersonalPlacings[i].Item2 / (float) GlobalTotals[i].Item2),
                    100.0F * (1.0F - (float)PersonalPlacings[i].Item2 / (float) day1Part1));
            }

            Console.WriteLine();
            Console.WriteLine("Press enter to exit");
            Console.ReadLine();
        }
    }
}
