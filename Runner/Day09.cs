using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Runner
{
    class Day09 : Day
    {
        public override string First(string input)
        {
            var parts = input.GetParts("players;lastmarbleisworthpoints");
            var numPlayers = int.Parse(parts[0]);
            var maxMarble = int.Parse(parts[1]);
            return MaxScore(numPlayers, maxMarble);

        }

        public override string Second(string input)
        {
            var parts = input.GetParts("players;lastmarbleisworthpoints");
            var numPlayers = int.Parse(parts[0]);
            var maxMarble = int.Parse(parts[1]);
            return MaxScore(numPlayers, maxMarble * 100);
        }

        public override string SecondTest(string input)
        {
            throw new NotImplementedException("SecondTest");
        }

        ////////////////////////////////////////////////////////

        public class Marble
        {
            public Marble Prev;
            public Marble Next;
            public long Value;

            public Marble Insert(Marble newMarble)
            {
                newMarble.Next = this;
                newMarble.Prev = this.Prev;
                this.Prev.Next = newMarble;
                this.Prev = newMarble;
                return newMarble;
            }

            public void Remove()
            {
                this.Prev.Next = this.Next;
                this.Next.Prev = this.Prev;
            }
        }

        // 0.01 secs for first inc tests
        private string MaxScore(int numPlayers, int maxMarble)
        {
            var scores = Enumerable.Repeat((long)0, numPlayers).ToArray();
            var root = new Marble() { Value = 0 };
            root.Next = root;
            root.Prev = root;
            int playerPos = 0;
            Marble currentMarble = root;
            for (long marbleValue = 1; marbleValue <= maxMarble; marbleValue++)
            {
                if (marbleValue % 23 == 0)
                {
                    var score = marbleValue;
                    var removeMarble = currentMarble.Prev.Prev.Prev.Prev.Prev.Prev.Prev;
                    score += removeMarble.Value;
                    currentMarble = removeMarble.Next;
                    removeMarble.Remove();
                    scores[playerPos] += score;
                }
                else
                {
                    var newMarble = new Marble() { Value = marbleValue };
                    currentMarble = currentMarble.Next.Next.Insert(newMarble);
                }
                //ShowState(currentMarble,playerPos);
                //if (marbleValue % 2000 == 0) Console.Write(".");

                playerPos = (playerPos + 1) % numPlayers;

            }

            return scores.Max(s => s).ToString();
        }

        private void ShowState(Marble current, int playerPos)
        {
            var marble = current;
            while (marble.Value != 0) marble = marble.Next;
            Console.Write(string.Format("[{0}]", playerPos + 1));
            do
            {
                Console.Write(string.Format(
                    (marble.Value == current.Value ? "({0})" : " {0} "), marble.Value));
                marble = marble.Next;
            } while (marble.Value != 0);
            Console.WriteLine();
        }


        //// NOT SCALABLE, way way slower (x100) than list, even with pre-allocated array
        //// 2.6 seconds for first inc tests
        //private string MaxScoreArray(int numPlayers, int maxMarble)
        //{
        //    var scores = Enumerable.Repeat((long)0, numPlayers).ToArray();
        //    var marbles = new long[maxMarble];
        //    int marbleCount = 1;
        //    int playerPos = 0;
        //    int currentPos = 0;
        //    for (long marble = 1; marble <= maxMarble; marble++)
        //    {
        //        int numMarbles = marbleCount;
        //        if (marble % 23 == 0)
        //        {
        //            var score = marble;
        //            var removePos = (currentPos - 7 + numMarbles) % numMarbles;
        //            score += marbles[removePos];
        //            for (int i = removePos; i < marbleCount; i++)
        //            {
        //                marbles[i] = marbles[i+1];
        //            }
        //            marbleCount--;
        //            scores[playerPos] += score;
        //            currentPos = removePos % (numMarbles - 1);
        //        }
        //        else
        //        {
        //            int insertPos = ((currentPos + 1) % numMarbles) + 1;
        //            if (insertPos == numMarbles)
        //            {
        //                marbles[marbleCount++] = marble;
        //            }
        //            else
        //            {
        //                for (int i = marbleCount; i > insertPos; i--)
        //                {
        //                    marbles[i] = marbles[i-1];
        //                }
        //                marbles[insertPos] = marble;
        //                marbleCount++;
        //            }
        //            currentPos = insertPos;
        //        }
        //        //ShowState(marbles.ToList().Take(marbleCount), playerPos, currentPos);
        //        if (marble % 2000 == 0) Console.Write(".");

        //        playerPos = (playerPos + 1) % numPlayers;

        //    }

        //    return scores.Max(s => s).ToString();
        //}

        //// NOT SCALABLE
        //// 0.3 seconds for first inc tests
        //private string MaxScoreList(int numPlayers, int maxMarble)
        //{
        //    var scores = Enumerable.Repeat((long)0, numPlayers).ToArray();
        //    List<long> marbles = new List<long>(maxMarble);
        //    int playerPos = 0;
        //    int currentPos = 0;
        //    marbles.Add(0);
        //    for (long marble = 1; marble <= maxMarble; marble++)
        //    {
        //        int numMarbles = marbles.Count;
        //        if (marble % 23 == 0)
        //        {
        //            var score = marble;
        //            var removePos = (currentPos - 7 + numMarbles) % numMarbles;
        //            score += marbles[removePos];
        //            marbles.RemoveAt(removePos);
        //            scores[playerPos] += score;
        //            currentPos = removePos % (numMarbles - 1);
        //        }
        //        else
        //        {
        //            int insertPos = ((currentPos + 1) % numMarbles) + 1;
        //            if (insertPos == numMarbles)
        //            {
        //                marbles.Add(marble);
        //            }
        //            else
        //            {
        //                marbles.Insert(insertPos, marble);
        //            }
        //            currentPos = insertPos;
        //        }
        //        //ShowState(marbles, playerPos, currentPos);
        //        if (marble % 2000 == 0) Console.Write(".");

        //        playerPos = (playerPos + 1) % numPlayers;

        //    }

        //    return scores.Max(s => s).ToString();
        //}

        //private static void ShowState(IEnumerable<int> marbles, int playerPos, int currentPos)
        //{
        //    Console.WriteLine(string.Format("[{0}] {1}",
        //        playerPos + 1,
        //        string.Join("", marbles.Select((m,i) => string.Format(
        //            (i == currentPos ? "({0})":" {0} ")
        //            , m).PadLeft(4)))));
        //}
    }
}
