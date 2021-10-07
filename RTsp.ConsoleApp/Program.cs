using System;
using RTsp.Application;

namespace RTsp.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var costMatrix = new int[][]
                             {
                                 new[] {0, 2, 2, 2},
                                 new[] {2, 0, 2, 2},
                                 new[] {2, 2, 0, 2},
                                 new[] {2, 2, 2, 0},
                             };

            var solver = new Solver(solution =>
                                    {
                                        float cost = 0f;
                                        for (var i = 1; i < solution.Length; i++)
                                        {
                                            cost += costMatrix[solution[i - 1]][solution[i]];
                                        }
                                        return cost;
                                    },
                                    (startIndex, endIndex) =>
                                    {
                                        return costMatrix[startIndex][endIndex];
                                    }, 
                                    4, 0, 3);
        }
    }
}
