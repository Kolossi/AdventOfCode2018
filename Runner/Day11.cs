using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Runner
{
    class Day11 :  Day
    {
        public override string First(string input)
        {
            var serial = int.Parse(input);
            var res = GetMaxPower3(serial);
            return string.Format("{0},{1}", res.X, res.Y);
        }

        public override string Second(string input)
        {
            var serial = int.Parse(input);
            var res = GetMaxPower(serial);
            return string.Format("{0},{1},{2}", res.X, res.Y,res.GridSize);
        }

        public override string FirstTest(string input)
        {
            var parts = input.GetParts("Fuel cell at ,, grid serial number : power level .");
            if (parts.Length > 1)
            {
                var x = int.Parse(parts[0]);
                var y = int.Parse(parts[1]);
                var serial = int.Parse(parts[2]);
                int power = GetPower(x, y, serial);
                return power.ToString();
            }
            else
            {
                return First(input);
            }
        }

        ////////////////////////////////////////////////////////

        private int GetPower(int x, int y, int serial)
        {
            int rackId = x + 10;
            int power = (((int)((((rackId * y) + serial) * rackId) / 100)) % 10) - 5;
            return power;
        }

        private PowerResult GetMaxPower3(int serial)
        {
            var powers = GetPowerGrid(serial);
            var cellPowers = new Dictionary<int, Dictionary<int, int>>();
            for (int cellSize = 0; cellSize < 3; cellSize++)
            {
                IncreaseCellSize(powers, cellPowers, cellSize);
            }
            var gridRes = FindMaxCellPower(cellPowers, 3);
            return gridRes;
        }

        private PowerResult GetMaxPower(int serial)
        {
            PowerResult res = null;
            var powers = GetPowerGrid(serial);
            var cellPowers = new Dictionary<int, Dictionary<int, int>>();
            
            for (int cellSize = 1; cellSize <= 300; cellSize++)
            {
                IncreaseCellSize(powers, cellPowers,cellSize-1);
                var gridRes = FindMaxCellPower(cellPowers, cellSize);
                if (res == null || gridRes.Power > res.Power) res = gridRes;
                
                Log(".");
            }
            
            return res;
        }

        private void IncreaseCellSize(Dictionary<int, Dictionary<int, int>> powers, Dictionary<int, Dictionary<int, int>> cellPowers, int currentCellSize)
        {
            int newCellSize = currentCellSize + 1;
            int maxCoord = 300 - newCellSize + 1;
            for (int y = 1; y <= maxCoord; y++)
            {
                for (int x = 1; x <= maxCoord; x++)
                {
                    for (int py = y; py <=y+currentCellSize-1; py++)
                    {
                        AddScore(cellPowers, x, y, powers[x + currentCellSize][py]);
                    }
                    for (int px = x; px <=x+currentCellSize; px++)//deliberate missing -1  get corner value
                    {
                        AddScore(cellPowers, x, y, powers[px][y + currentCellSize]);
                    }
                }
            }
        }

        private PowerResult FindMaxCellPower(Dictionary<int, Dictionary<int, int>> cellPowers, int cellSize)
        {
            PowerResult res = null;
            for (int y = 1; y <= 300 - cellSize + 1; y++)
            {
                for (int x = 1; x <= 300 - cellSize + 1; x++)
                {
                    var cellPower = cellPowers[x][y];
                    if (res == null || cellPower > res.Power) res = new PowerResult()
                    {
                        X = x,
                        Y = y,
                        Power = cellPower,
                        GridSize = cellSize
                    };
                }
            }

            return res;
        }

        private Dictionary<int, Dictionary<int, int>> GetPowerGrid(int serial)
        {
            var powers = new Dictionary<int, Dictionary<int, int>>();

            for (int y = 1; y <= 300; y++)
            {
                for (int x = 1; x <= 300; x++)
                {
                    var power = GetPower(x, y, serial);
                    AddScore(powers, x, y, power);
                }
            }

            return powers;
        }

        public void AddScore(Dictionary<int, Dictionary<int, int>> dict, int x, int y, int score)
        {
            Dictionary<int,int> currentYs;
            
            if (!dict.TryGetValue(x, out currentYs))
            {
                currentYs = new Dictionary<int, int>();
                dict[x] = currentYs;
            }
            int current;
            if (!currentYs.TryGetValue(y, out current))
            {
                current = 0;
            }
            currentYs[y] = current + score;
        }

        public class PowerResult
        {
            public int X;
            public int Y;
            public int Power;
            public int GridSize;
        }
    }
}
