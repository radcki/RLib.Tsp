using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RTsp.Application.Enums;

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
        private bool CompareFullSolutionCost { get; set; }
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

        public void SetStartNode(string nodeId)
        {
            _startNodeIndex = Array.IndexOf(_nodeIds, nodeId);
        }

        public void SetEndNode(string nodeId)
        {
            _endNodeIndex = Array.IndexOf(_nodeIds, nodeId);
        }

        public void UseFullSolutionCostValidation(Func<int[], float> getSolutionCost)
        {
            GetSolutionCostExternal = getSolutionCost;
            CompareFullSolutionCost = true;
        }

        private int[] GenerateInitialSolution() => Configuration.FirstSolutionStrategy switch
        {
            eFirstSolutionStrategy.Random => new InitialSolutionGenerator.RandomOrder(_nodeIds.Length, _startNodeIndex, _endNodeIndex).Generate(),
            eFirstSolutionStrategy.NearestNeighbor => new InitialSolutionGenerator.NearestNeighbor(_nodeIds.Length, _startNodeIndex, _endNodeIndex).Generate(GetArcCost),
            eFirstSolutionStrategy.ConnectCheapestArcs => new InitialSolutionGenerator.GlobalCheapestArc(_nodeIds.Length, _startNodeIndex, _endNodeIndex).Generate(GetArcCost),
            _ => throw new ArgumentOutOfRangeException()
        };


        private float NodeSwapCostChange(int[] solution, int nodeA, int nodeB)
        {
            return (nodeA > 0 ? GetArcCost(solution[nodeA - 1], solution[nodeB]) : 0)
                   + (nodeB < (solution.Length - 1) ? GetArcCost(solution[nodeA], solution[nodeB + 1]) : 0)
                   - ((nodeA > 0 ? GetArcCost(solution[nodeA], solution[nodeA - 1]) : 0)
                      + (nodeB < (solution.Length - 1) ? GetArcCost(solution[nodeB], solution[nodeB + 1]) : 0));
        }


        public (string[], int[]) FindSolution()
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
            var mutationCount = 0;
            Array.Copy(solution, 0, localSolution, 0, solution.Length);
            do
            {
                var wasImproved = false;
                for (var i = iStartIndex; i < jMaxIndex - 2; i++)
                {
                    for (var j = i + 2; j < jMaxIndex; j++)
                    {
                        if (!CompareFullSolutionCost)
                        {
                            var costChange = NodeSwapCostChange(localSolution, i, j);
                            if (costChange < 0f)
                            {
                                SolverTools.ApplyNodeSwap(localSolution, i, j);
                                localSolutionCost += costChange;
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

                if (wasImproved)
                {
                    if (localSolutionCost < solutionCost)
                    {
                        Array.Copy(localSolution, 0, solution, 0, solution.Length);
                        solutionCost = localSolutionCost;
                    }
                }
                else if (Configuration.EnableSolutionMutation && mutationCount < Configuration.MaxMutations)
                {
                    var swapI = rnd.Next(iStartIndex, jMaxIndex - 2);
                    var swapJ = rnd.Next(swapI, jMaxIndex);
                    localSolutionCost += NodeSwapCostChange(localSolution, swapI, swapJ);
                    SolverTools.ApplyNodeSwap(localSolution, swapI, swapJ);

                    mutationCount++;
                    rndSeed++;
                    rnd = new Random(rndSeed);
                }
                else
                {
                    break;
                }

                iteration++;
            }
            while (iteration < Configuration.MaxIterations);

            return (solution.Select(x => _nodeIds[x]).ToArray(), solution);
        }


        public class SolverConfiguration
        {
            public eFirstSolutionStrategy FirstSolutionStrategy { get; set; } = eFirstSolutionStrategy.NearestNeighbor;

            /// <summary>
            /// Make random node swap when no improvement is found in iteration to possibly escape local minimum.
            /// </summary>
            public bool EnableSolutionMutation { get; set; } = false;

            public int MaxIterations { get; set; } = 5000;
            public int MaxMutations { get; set; } = 100;
        }
    }
}