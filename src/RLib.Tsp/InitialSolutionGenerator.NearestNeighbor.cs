using System;
using System.Collections.Generic;
using System.Linq;

namespace RLib.Tsp
{
    public partial class InitialSolutionGenerator
    {
        public class NearestNeighbor
        {
            private readonly int _length;
            private readonly int? _startNodeIndex;
            private readonly int? _endNodeIndex;

            public NearestNeighbor(int length, int? startNodeIndex, int? endNodeIndex)
            {
                _length = length;
                _startNodeIndex = startNodeIndex;
                _endNodeIndex = endNodeIndex;
            }

            public int[] Generate(Func<int, int, float> arcCost)
            {
                var solution = Enumerable.Repeat(-1, _length).ToArray();

                var visitedIndexes = new List<int>(_length) { };
                if (_endNodeIndex != null)
                {
                    visitedIndexes.Add(_endNodeIndex.Value);
                    solution[^1] = _endNodeIndex.Value;
                }

                var currentIndex = _startNodeIndex ?? Enumerable.Range(1, _length - 1).OrderBy(x => arcCost(0, x)).First();
                solution[0] = currentIndex;
                var step = 1;
                var steps = _endNodeIndex.HasValue ? _length - 1 : _length;

                while (step < steps)
                {
                    var minArcCost = float.PositiveInfinity;
                    var minArcIndex = -1;
                    visitedIndexes.Add(currentIndex);
                    for (var i = 0; i < _length; i++)
                    {
                        if (visitedIndexes.Contains(i))
                        {
                            continue;
                        }

                        var cost = arcCost(currentIndex, i);
                        if (cost < minArcCost)
                        {
                            minArcCost = cost;
                            minArcIndex = i;
                        }
                    }

                    solution[step] = minArcIndex;
                    visitedIndexes.Add(minArcIndex);
                    step++;
                }

                return solution;
            }
        }
    }
}
