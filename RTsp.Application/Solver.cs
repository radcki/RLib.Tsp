using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RTsp.Application
{
    public class Solver
    {
        public Solver(Func<int, int, float> getArcCost, string[] nodeIds)
        {
            GetArcCost = getArcCost;
            _nodeIds = nodeIds;
            Configuration = new SolverConfiguration();
        }

        public Solver(Func<int, int, float> getArcCost, string[] nodeIds, SolverConfiguration configuration)
        {
            GetArcCost = getArcCost;
            _nodeIds = nodeIds;
            Configuration = configuration;
        }

        private Func<int[], float> GetSolutionCostExternal { get; set; }
        private Func<int, int, float> GetArcCost { get; set; }
        private int? _startNodeIndex;
        private int? _endNodeIndex;
        private readonly string[] _nodeIds;
        public SolverConfiguration Configuration { get; set; }

        private float GetSolutionCost(int[] solution)
        {
            if (GetSolutionCostExternal != null)
            {
                return GetSolutionCostExternal(solution);
            }

            float cost = 0f;
            for (var i = 1; i < solution.Length; i++)
            {
                cost += GetArcCost(solution[i - 1], solution[i]);
            }

            return cost;
        }
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

        public void SetStartNode(string nodeId)
        {
            _startNodeIndex = Array.IndexOf(_nodeIds, nodeId);
        }

        public void SetEndNode(string nodeId)
        {
            _endNodeIndex = Array.IndexOf(_nodeIds, nodeId);
        }

        public void UseSolutionCostValidation(Func<int[], float> getSolutionCost)
        {
            GetSolutionCostExternal = getSolutionCost;
        }

        private int[] GenerateInitialSolution() => Configuration.FirstSolutionStrategy switch
        {
            eFirstSolutionStrategy.Random => GenerateInitialSolutionRandom(),
            eFirstSolutionStrategy.NearestNeighbor => GenerateInitialSolutionNearestNeighbor(),
            eFirstSolutionStrategy.ConnectCheapestArcs => GenerateInitialSolutionGlobalCheapestArc(),
            _ => throw new ArgumentOutOfRangeException()
        };

        private int[] GenerateInitialSolutionRandom()
        {
            var solution = Enumerable.Repeat(-1, _nodeIds.Length).ToArray();
            solution[0] = _startNodeIndex ?? -1;
            solution[^1] = _endNodeIndex ?? -1;
            var rng = new Random();

            var indexes = Enumerable.Range(0, _nodeIds.Length).Except(solution).OrderBy(a => rng.Next()).ToArray();
            for (var i = 0; i < indexes.Length; i++)
            {
                var solutionIndex = _startNodeIndex.HasValue ? i + 1 : i;
                solution[solutionIndex] = indexes[i];
            }

            return solution;
        }

        private int[] GenerateInitialSolutionNearestNeighbor()
        {
            var solution = Enumerable.Repeat(-1, _nodeIds.Length).ToArray();

            var visitedIndexes = new List<int>(_nodeIds.Length) { };
            if (_endNodeIndex != null)
            {
                visitedIndexes.Add(_endNodeIndex.Value);
                solution[^1] = _endNodeIndex.Value;
            }

            var currentIndex = _startNodeIndex ?? Enumerable.Range(1, _nodeIds.Length - 1).OrderBy(x => GetArcCost(0, x)).First();
            solution[0] = currentIndex;
            var step = 1;
            var steps = _endNodeIndex.HasValue ? _nodeIds.Length - 1 : _nodeIds.Length;

            while (step < steps)
            {
                var minArcCost = float.PositiveInfinity;
                var minArcIndex = -1;
                visitedIndexes.Add(currentIndex);
                for (var i = 0; i < _nodeIds.Length; i++)
                {
                    if (visitedIndexes.Contains(i))
                    {
                        continue;
                    }

                    var arcCost = GetArcCost(currentIndex, i);
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

        private int[] GenerateInitialSolutionGlobalCheapestArc()
        {
            var solution = Enumerable.Repeat(-1, _nodeIds.Length).ToArray();

            if (_startNodeIndex != null)
            {
                solution[0] = _startNodeIndex.Value;
            }

            if (_endNodeIndex != null)
            {
                solution[^1] = _endNodeIndex.Value;
            }

            var expectedLeftover = 1 + (_startNodeIndex.HasValue ? 1 : 0) + (_endNodeIndex.HasValue ? 1 : 0) + (_nodeIds.Length % 2 > 0 ? 1 : 0);
            var availableIndexes = Enumerable.Range(0, _nodeIds.Length).Select(x => new[] {x}).ToList();

            while (availableIndexes.Count > expectedLeftover)
            {
                var createdArcs = new List<int[]>();
                while (availableIndexes.Count > expectedLeftover)
                {
                    var minCost = float.MaxValue;
                    int[] minNode = null;
                    var connectedNodes = new int[2][];
                    for (var i = 0; i < availableIndexes.Count; i++)
                    {
                        var a = availableIndexes[i];
                        for (var j = i + 1; j < availableIndexes.Count; j++)
                        {
                            var b = availableIndexes[j];
                            if (a[0] == solution[0] 
                                || a[^1] == solution[0] 
                                || a[0] == solution[^1]
                                || b[^1] == solution[^1]
                                || b[0] == solution[^1]
                                || b[0] == solution[0])
                            {
                                continue;
                            }

                            var cost = GetArcCost(a[^1], b[0]);
                            if (cost < minCost)
                            {
                                minNode = a.Union(b).ToArray();
                                connectedNodes[0] = a;
                                connectedNodes[1] = b;
                                minCost = cost;
                            }

                            cost = GetArcCost(a[^1], b[^1]);
                            if (cost < minCost)
                            {
                                Array.Reverse(b);
                                minNode = a.Union(b).ToArray();
                                connectedNodes[0] = a;
                                connectedNodes[1] = b;
                                minCost = cost;
                            }
                            cost = GetArcCost(a[0], b[0]);
                            if (cost < minCost)
                            {
                                Array.Reverse(a);
                                minNode = a.Union(b).ToArray();
                                connectedNodes[0] = a;
                                connectedNodes[1] = b;
                                minCost = cost;
                            }
                            cost = GetArcCost(a[^1], b[^1]);
                            if (cost < minCost)
                            {
                                Array.Reverse(a);
                                Array.Reverse(b);
                                minNode = a.Union(b).ToArray();
                                connectedNodes[0] = a;
                                connectedNodes[1] = b;
                                minCost = cost;
                            }

                        }
                    }

                    availableIndexes.Remove(connectedNodes[0]);
                    availableIndexes.Remove(connectedNodes[1]);
                    createdArcs.Add(minNode);
                }

                availableIndexes.ForEach(createdArcs.Add);
                availableIndexes = createdArcs;
            }

            var insertIndex = 0;
            if (solution[0] >= 0)
            {
                availableIndexes.Remove(availableIndexes.First(x => x[0] == solution[0]));
                insertIndex = 1;
            }
            if (solution[^1] >= 0)
            {
                availableIndexes.Remove(availableIndexes.First(x => x[0] == solution[^1]));
            }
            Array.Copy(availableIndexes[0], 0, solution, insertIndex, availableIndexes[0].Length);

            return solution;
        }


        private float GetSwapCostChange(int[] solution, int a, int b)
        {
            return (GetArcCost(solution[a - 1], solution[b])
                    + GetArcCost(solution[a], solution[b + 1]))
                   - (GetArcCost(solution[a], solution[a - 1])
                      + GetArcCost(solution[b], solution[b + 1]));
        }


        public (string[], int[]) FindSolution(int maxIterations)
        {
            var solution = GenerateInitialSolution();
            var solutionCost = GetSolutionCost(solution);
            var size = solution.Length;

            var iteration = 0;
            var iStartIndex = (_startNodeIndex == null ? 0 : 1);
            var jMaxIndex = size - (_endNodeIndex == null ? 0 : 1);
            var rndSeed = 1;
            var rnd = new Random(rndSeed);

            var localSolution = new int[solution.Length];
            var localSolutionCost = solutionCost;
            Array.Copy(solution, 0, localSolution, 0, solution.Length);
            do
            {
                var wasImproved = false;
                for (var i = iStartIndex; i < jMaxIndex - 2; i++)
                {
                    for (var j = i + 2; j < jMaxIndex; j++)
                    {
                        if (GetSolutionCostExternal == null)
                        {
                            var change = GetSwapCostChange(localSolution, i, j);
                            if (change < 0f)
                            {
                                SolverTools.ApplyNodeSwap(localSolution, i, j);
                                localSolutionCost += change;
                                wasImproved = true;
                            }
                        }
                        else
                        {
                            var newRoute = SolverTools.GetRouteByNodeSwap(localSolution, i, j);
                            var newRouteCost = GetSolutionCost(newRoute);
                            if (newRouteCost < localSolutionCost)
                            {
                                localSolutionCost = newRouteCost;
                                localSolution = newRoute;
                                wasImproved = true;
                            }
                        }
                    }
                }

                iteration++;
                if (wasImproved)
                {
                    if (localSolutionCost < solutionCost)
                    {
                        Array.Copy(localSolution, 0, solution, 0, solution.Length);
                        solutionCost = localSolutionCost;
                    }
                }
                else if (Configuration.EnableSolutionMutation)
                {
                    var swapI = rnd.Next(iStartIndex, jMaxIndex - 2);
                    var swapJ = rnd.Next(swapI, jMaxIndex);
                    localSolutionCost += GetSwapCostChange(localSolution, swapI, swapJ);
                    SolverTools.ApplyNodeSwap(localSolution, swapI, swapJ);

                    rndSeed++;
                    rnd = new Random(rndSeed);
                }
                else
                {
                    break;
                }
            }
            while (iteration < maxIterations);

            return (solution.Select(x => _nodeIds[x]).ToArray(), solution);
        }

        public class SolverConfiguration
        {
            public eFirstSolutionStrategy FirstSolutionStrategy { get; set; } = eFirstSolutionStrategy.NearestNeighbor;
            /// <summary>
            /// Make random node swap when no improvement is found in iteration to possibly escape local minimum.
            /// </summary>
            public bool EnableSolutionMutation { get; set; } = false;
        }

        public enum eFirstSolutionStrategy
        {
            /// <summary>
            /// Finds cheapest arc and then add next nodes to route, finding shortest arc each time.
            /// </summary>
            NearestNeighbor,
            /// <summary>
            /// Connect nodes randomly.
            /// </summary>
            Random,
            /// <summary>
            /// Creates cheapest connections between unconnected nodes, then iteratively finds cheapest connections between created arcs.
            /// </summary>
            ConnectCheapestArcs
        }

    }
}