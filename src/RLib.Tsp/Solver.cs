using System;
using System.Linq;
using RLib.Tsp.Enums;

namespace RLib.Tsp
{
    public class Solver
    {
        public Solver(Func<int, int, float> getArcCost, int nodeCount)
        {
            GetArcCost = getArcCost;
            _nodeCount = nodeCount;
            Configuration = new SolverConfiguration();
        }

        public Solver(Func<int, int, float> getArcCost, int nodeCount, SolverConfiguration configuration)
        {
            GetArcCost = getArcCost;
            _nodeCount = nodeCount;
            Configuration = configuration;
        }

        private Func<int[], float> GetSolutionCostExternal { get; set; }
        private bool CompareFullSolutionCost { get; set; }
        private Func<int, int, float> GetArcCost { get; set; }
        private int? _startNodeIndex;
        private int? _endNodeIndex;
        private readonly int _nodeCount;
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

        public void SetStartNode(int nodeIndex)
        {
            if (nodeIndex < 0 || nodeIndex > _nodeCount - 1)
            {
                throw new ArgumentException($"Start node index must be in range between 0 and {_nodeCount-1}");
            }
            _startNodeIndex = nodeIndex;
        }

        public void SetEndNode(int nodeIndex)
        {
            if (nodeIndex < 0 || nodeIndex > _nodeCount - 1)
            {
                throw new ArgumentException($"End node index must be in range between 0 and {_nodeCount - 1}");
            }
            _endNodeIndex = nodeIndex;
        }

        public void UseFullSolutionCostValidation(Func<int[], float> getSolutionCost)
        {
            GetSolutionCostExternal = getSolutionCost;
            CompareFullSolutionCost = true;
        }

        private int[] GenerateInitialSolution()
        {
            switch (Configuration.FirstSolutionStrategy)
            {
                case eFirstSolutionStrategy.Random: return new InitialSolutionGenerator.RandomOrder(_nodeCount, _startNodeIndex, _endNodeIndex).Generate();
                case eFirstSolutionStrategy.NearestNeighbor: return new InitialSolutionGenerator.NearestNeighbor(_nodeCount, _startNodeIndex, _endNodeIndex).Generate(GetArcCost);
                case eFirstSolutionStrategy.ConnectCheapestArcs: return new InitialSolutionGenerator.GlobalCheapestArc(_nodeCount, _startNodeIndex, _endNodeIndex).Generate(GetArcCost);

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


        private float NodeSwapCostChange(int[] solution, int nodeA, int nodeB)
        {
            return (nodeA > 0 ? GetArcCost(solution[nodeA - 1], solution[nodeB]) : 0)
                   + (nodeB < (solution.Length - 1) ? GetArcCost(solution[nodeA], solution[nodeB + 1]) : 0)
                   - ((nodeA > 0 ? GetArcCost(solution[nodeA], solution[nodeA - 1]) : 0)
                      + (nodeB < (solution.Length - 1) ? GetArcCost(solution[nodeB], solution[nodeB + 1]) : 0));
        }


        public int[] FindSolution()
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
                        mutationCount = 0;
                    }
                }
                else if (Configuration.EnableSolutionMutation && mutationCount < Configuration.MaxSolutionMutations)
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

            return solution;
        }

        
        public class SolverConfiguration
        {
            public eFirstSolutionStrategy FirstSolutionStrategy { get; set; } = eFirstSolutionStrategy.NearestNeighbor;
            public int MaxIterations { get; set; } = 5000;

            /// <summary>
            /// Make random node swap when no improvement is found in iteration to possibly escape local minimum.
            /// </summary>
            public bool EnableSolutionMutation { get; set; } = true;

            /// <summary>
            /// Mutation counter is reset if better solution was found.
            /// If after specified number of attempts no better solution is found, solution search is stopped.
            /// Increasing this number can result in finding better solution but increases computation time.
            /// </summary>
            public int MaxSolutionMutations { get; set; } = 50;
        }
    }
}