using FluentAssertions;
using Xunit;

namespace RLib.Tsp.UnitTests
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

            //Assert
            for (var i = 0; i < output.Length; i++)
            {
                output[i].Should().Be(expectedOutput[i]);
            }
        }

        [Fact]
        public void TwoOptSwap_AppliesCorrectOrder()
        {
            //Arrange
            var input              = new int[] { 0, 1, 2, 3, 4, 5, 6 };
            var expectedOutput     = new int[] { 0, 1, 5, 4, 3, 2, 6 };

            //Act
            SolverTools.ApplyNodeSwap(input, 2, 5);

            //Assert
            for (var i = 0; i < input.Length; i++)
            {
                input[i].Should().Be(expectedOutput[i]);
            }
        }

    }
}
