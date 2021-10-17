using System;
using System.Collections.Generic;
using System.Linq;

namespace RLib.Tsp
{
    public partial class InitialSolutionGenerator
    {
        public class GlobalCheapestArc
        {
            private readonly int _length;
            private readonly int? _startNodeIndex;
            private readonly int? _endNodeIndex;

            public GlobalCheapestArc(int length, int? startNodeIndex, int? endNodeIndex)
            {
                _length = length;
                _startNodeIndex = startNodeIndex;
                _endNodeIndex = endNodeIndex;
            }

            public int[] Generate(Func<int, int, float> arcCost)
            {
                var solution = Enumerable.Repeat(-1, _length).ToArray();

                if (_startNodeIndex != null)
                {
                    solution[0] = _startNodeIndex.Value;
                }

                if (_endNodeIndex != null)
                {
                    solution[^1] = _endNodeIndex.Value;
                }

                var expectedLeftover = 1 + (_startNodeIndex.HasValue ? 1 : 0) + (_endNodeIndex.HasValue ? 1 : 0) ;
                var arcsToConnect = Enumerable.Range(0, _length).Select(x => new[] { x }).ToList();

                while (arcsToConnect.Count > expectedLeftover)
                {
                    var createdArcs = new List<int[]>();
                    while (arcsToConnect.Count > expectedLeftover)
                    {
                        var minCost = float.MaxValue;
                        var connectedArcs = new int[2][];
                        for (var i = 0; i < arcsToConnect.Count; i++)
                        {
                            var a = arcsToConnect[i];
                            for (var j = i + 1; j < arcsToConnect.Count; j++)
                            {
                                var b = arcsToConnect[j];
                                if (a[0] == solution[0]
                                    || a[^1] == solution[0]
                                    || a[0] == solution[^1]
                                    || b[^1] == solution[^1]
                                    || b[0] == solution[^1]
                                    || b[0] == solution[0])
                                {
                                    continue;
                                }

                                var cost = arcCost(a[^1], b[0]);
                                if (cost < minCost)
                                {
                                    connectedArcs[0] = a;
                                    connectedArcs[1] = b;
                                    minCost = cost;
                                }

                                cost = arcCost(a[^1], b[^1]);
                                if (cost < minCost)
                                {
                                    Array.Reverse(b);
                                    connectedArcs[0] = a;
                                    connectedArcs[1] = b;
                                    minCost = cost;
                                }
                                cost = arcCost(a[0], b[0]);
                                if (cost < minCost)
                                {
                                    Array.Reverse(a);
                                    connectedArcs[0] = a;
                                    connectedArcs[1] = b;
                                    minCost = cost;
                                }
                                cost = arcCost(a[^1], b[^1]);
                                if (cost < minCost)
                                {
                                    Array.Reverse(a);
                                    Array.Reverse(b);
                                    connectedArcs[0] = a;
                                    connectedArcs[1] = b;
                                    minCost = cost;
                                }

                            }
                        }

                        arcsToConnect.Remove(connectedArcs[0]);
                        arcsToConnect.Remove(connectedArcs[1]);
                        createdArcs.Add(connectedArcs.SelectMany(x=>x).ToArray());
                    }

                    arcsToConnect.ForEach(createdArcs.Add);
                    arcsToConnect = createdArcs;
                }

                var insertIndex = 0;
                if (solution[0] >= 0)
                {
                    arcsToConnect.Remove(arcsToConnect.First(x => x[0] == solution[0]));
                    insertIndex = 1;
                }
                if (solution[^1] >= 0)
                {
                    arcsToConnect.Remove(arcsToConnect.First(x => x[0] == solution[^1]));
                }
                Array.Copy(arcsToConnect[0], 0, solution, insertIndex, arcsToConnect[0].Length);

                return solution;
            }
        }
    }
}
