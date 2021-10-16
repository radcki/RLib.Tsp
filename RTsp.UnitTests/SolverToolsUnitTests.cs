using System;
using System.Diagnostics;
using System.Linq;
using FluentAssertions;
using RTsp.Application;
using Xunit;

namespace RTsp.UnitTests
{
    public class SolverToolsUnitTests
    {
        [Fact]
        public void TwoOptSwap_ReturnsCorrectOrder()
        {
            //Arrange
            var input              = new int[] {0, 1, 2, 3, 4, 5, 6};
            var expectedOutput     = new int[] {0, 1, 5, 4, 3, 2, 6};

            //Act
            var output = SolverTools.GetRouteByNodeSwap(input, 2, 5);
            var outputReversed = SolverTools.GetRouteByNodeSwap(output, 2, 5);

            //Assert
            for (var i = 0; i < output.Length; i++)
            {
                output[i].Should().Be(expectedOutput[i]);
                outputReversed[i].Should().Be(input[i]);
            }
        }

        [Fact]
        public void TwoOptSwap_AppliesCorrectOrder()
        {
            //Arrange
            var input = new int[] { 0, 1, 2, 3, 4, 5, 6 };
            var expectedOutput = new int[] { 0, 1, 5, 4, 3, 2, 6 };

            //Act
            SolverTools.ApplyNodeSwap(input, 2, 5);

            //Assert
            for (var i = 0; i < input.Length; i++)
            {
                input[i].Should().Be(expectedOutput[i]);
            }
        }

        [Fact]
        public void TwoOptSwap_Speed()
        {
            //Arrange
            var input = Enumerable.Range(0, 100000).ToArray();
            var expectedOutput = new int[] { 0, 1, 5, 4, 3, 2, 6 };
            var rnd = new Random();
            var testIterations = 100000;

            //Act
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < testIterations; i++)
            {
                var randomSwapI = rnd.Next(0, input.Length - 2);
                var randomSwapJ = rnd.Next(randomSwapI, input.Length);
                SolverTools.ApplyNodeSwap(input, randomSwapI, randomSwapJ);
            }
            var r1 = sw.Elapsed;
            sw.Restart();
            for (int i = 0; i < testIterations; i++)
            {
                var randomSwapI = rnd.Next(0, input.Length - 2);
                var randomSwapJ = rnd.Next(randomSwapI, input.Length);
                input = SolverTools.GetRouteByNodeSwap(input, randomSwapI, randomSwapJ);
            }

            var r2 = sw.Elapsed;

            //Assert
            r1.Should().BeLessThan(r2);
        }
    }
}
