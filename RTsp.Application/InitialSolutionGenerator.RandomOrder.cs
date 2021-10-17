using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTsp.Application
{
    public partial class InitialSolutionGenerator
    {
        public class RandomOrder
        {
            private readonly int _length;
            private readonly int? _startNodeIndex;
            private readonly int? _endNodeIndex;
            private readonly Random _rng = new Random();

            public RandomOrder(int length, int? startNodeIndex, int? endNodeIndex)
            {
                _length = length;
                _startNodeIndex = startNodeIndex;
                _endNodeIndex = endNodeIndex;
            }

            public int[] Generate()
            {
                var solution = Enumerable.Repeat(-1, _length).ToArray();
                solution[0] = _startNodeIndex ?? -1;
                solution[^1] = _endNodeIndex ?? -1;

                var indexes = Enumerable.Range(0, _length).Except(solution).OrderBy(a => _rng.Next()).ToArray();
                for (var i = 0; i < indexes.Length; i++)
                {
                    var solutionIndex = _startNodeIndex.HasValue ? i + 1 : i;
                    solution[solutionIndex] = indexes[i];
                }

                return solution;
            }
        }
    }
}
