using System;
using System.Diagnostics;
using System.Linq;
using Google.OrTools.ConstraintSolver;
using RLib.Tsp.Enums;

namespace RLib.Tsp.Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            var costMatrix = InputData.CostMatrix;

            var calculateSolutionCost = new Func<int[], float>((solution) =>
                                                               {
                                                                   float cost = 0f;
                                                                   for (var i = 1; i < solution.Length; i++)
                                                                   {
                                                                       cost += costMatrix[solution[i - 1], solution[i]];
                                                                   }
                                                                   return cost;
                                                               });
            var nodeIds = InputData.Names;

            var sw = Stopwatch.StartNew();
            var solver = new Solver((startIndex, endIndex) => costMatrix[startIndex, endIndex],
                                    nodeIds,
                                    new Solver.SolverConfiguration()
                                    {
                                        EnableSolutionMutation = true,
                                        FirstSolutionStrategy = eFirstSolutionStrategy.ConnectCheapestArcs
                                    });
            solver.SetStartNode(nodeIds[0]);
            solver.SetEndNode(nodeIds[^1]);

            var (solution, solutionIndexes) = solver.FindSolution();
            sw.Stop();
            var solutionCost = calculateSolutionCost(solutionIndexes);

            Console.WriteLine("RTsp Solution:");
            Console.WriteLine(string.Join("->", solution));
            Console.WriteLine("-------");
            Console.WriteLine($"RTsp Cost: {solutionCost}, time: {sw.Elapsed.TotalMilliseconds}ms");
            Console.WriteLine("");
            Console.WriteLine("");

            sw.Restart();
            var orToolsSolution = GetOrToolsSolution(nodeIds, costMatrix);
            sw.Stop();
            var orToolsSolutionCost = calculateSolutionCost(orToolsSolution);

            Console.WriteLine("OrTools Solution:");
            Console.WriteLine(string.Join("->", orToolsSolution.Select(x=>nodeIds[x])));
                Console.WriteLine("-------");
            Console.WriteLine($"OrTools Cost: {orToolsSolutionCost}, elapsed: {Math.Round(sw.Elapsed.TotalMilliseconds, 2)}ms");

            Console.ReadKey();
        }

        private static int[] GetOrToolsSolution(string[] nodeIds, float[,] costMatrix)
        {
            RoutingIndexManager manager = new RoutingIndexManager(nodeIds.Length, 1, new[] { 0 }, new[] { nodeIds.Length - 1 });
            RoutingModel routing = new RoutingModel(manager);
            int transitCallbackIndex = routing.RegisterTransitCallback((long fromIndex, long toIndex) =>
                                                                       {
                                                                           var fromNode = manager.IndexToNode(fromIndex);
                                                                           var toNode = manager.IndexToNode(toIndex);
                                                                       return (long)costMatrix[fromNode, toNode];
                                                                       });

            routing.SetArcCostEvaluatorOfAllVehicles(transitCallbackIndex);
            RoutingSearchParameters searchParameters = operations_research_constraint_solver.DefaultRoutingSearchParameters();

            var orToolsSolution = new int[nodeIds.Length];
            Assignment sol = routing.SolveWithParameters(searchParameters);
            var index = routing.Start(0);
            var step = 0;
            while (routing.IsEnd(index) == false)
            {
                orToolsSolution[step] = manager.IndexToNode((int)index);
                step++;
                index = sol.Value(routing.NextVar(index));
            }

            orToolsSolution[step] = manager.IndexToNode((int)index);
            return orToolsSolution;
        }
    }
}