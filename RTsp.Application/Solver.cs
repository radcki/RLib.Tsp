using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTsp.Application
{
    public class Solver
    {
        public Solver(Func<int[], float> getSolutionCost, Func<int, int, float> getArcCost, string[] nodeIds, int startNodeIndex, int endNodeIndex)
        {
            GetSolutionCost = getSolutionCost;
            GetArcCost = getArcCost;
            _nodeIds = nodeIds;
            _startNodeIndex = startNodeIndex;
            _endNodeIndex = endNodeIndex;

            var nodeCount = nodeIds.Length;
            //NodesCostMatrix = Enumerable.Range(0, nodeCount).Select(x => new float[nodeCount]).ToArray();

            //FillNodeCostMatrix();
        }

        private Func<int[], float> GetSolutionCost { get; set; }
        private Func<int, int, float> GetArcCost { get; set; }
        private readonly int _startNodeIndex;
        private readonly int _endNodeIndex;
        private readonly string[] _nodeIds;

        //public float[][] NodesCostMatrix { get; set; }

        //private void FillNodeCostMatrix()
        //{
        //    for (int i = 0; i < NodesCostMatrix.Length; i++)
        //    {
        //        for (int j = 0; j < NodesCostMatrix.Length; j++)
        //        {
        //            NodesCostMatrix[i][j] = CalculateNodeCost(i, j);
        //        }
        //    }
        //}

        private int[] GenerateInitialSolution()
        {
            var visitedIndexes = new List<int>(_nodeIds.Length){_endNodeIndex};
            var solution = new int[_nodeIds.Length];
            solution[0] = _startNodeIndex;
            solution[^1] = _endNodeIndex;

            var currentIndex = _startNodeIndex;
            var step = 1;
            while (step < (_nodeIds.Length-1))
            {
                var minArcCost = float.PositiveInfinity;
                var minArcIndex = -1;
                visitedIndexes.Add(currentIndex);
                for (var i = 0; i < _nodeIds.Length; i++)
                {
                    if (visitedIndexes.Contains(i)) { continue; }

                    var arcCost = GetArcCost(currentIndex,i);
                    if (arcCost < minArcCost)
                    {
                        minArcCost = arcCost;
                        minArcIndex = i;
                    }
                }

                if (minArcIndex < 0)
                {
                    throw new Exception("No solution found");
                }

                solution[step] = minArcIndex;
                visitedIndexes.Add(minArcIndex);
                step++;
            }

            return solution;
        }

        private int[] TwoOptSwap(int[] route, int aIndex, int bIndex)
        {
            var newRoute = new int[route.Length];
            for (int i = 0; i < aIndex; i++)
            {
                newRoute[i] = route[i];
            }

            int j = 0;
            for (int i = aIndex; i <= bIndex; i++)
            {
                newRoute[i] = route[bIndex - j];
                j++;
            }

            for (int i = bIndex + 1; i < route.Length; i++)
            {
                newRoute[i] = route[i];
            }

            return newRoute;
        }

        public string[] FindSolution()
        {
            var initialSolution = GenerateInitialSolution();
            var initialSolutionCost = GetSolutionCost(initialSolution);
        }
    }
}