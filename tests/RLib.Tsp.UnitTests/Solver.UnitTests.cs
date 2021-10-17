using System;
using System.Linq;
using FluentAssertions;
using RLib.Tsp.Enums;
using Xunit;

namespace RLib.Tsp.UnitTests
{
    public class SolverUnitTests
    {
        [Fact]
        public void Solution_ShouldContainAllPoints()
        {
            //Arrange
            var solver = new Solver((startIndex, endIndex) => TestData.CostMatrix[startIndex, endIndex],
                                    TestData.CityNames.Length);

            //Act
            var indexes = solver.FindSolution();

            //Assert
            indexes.Distinct().Count().Should().Be(TestData.CityNames.Length);
        }

        [Fact]
        public void Solution_ShouldContainAllPoints_2()
        {
            //Arrange
            var solver = new Solver((startIndex, endIndex) => TestData.CostMatrix[startIndex, endIndex],
                                    TestData.CityNames.Length,
                                    new Solver.SolverConfiguration()
                                    {
                                        FirstSolutionStrategy = eFirstSolutionStrategy.ConnectCheapestArcs
                                    });

            //Act
            var indexes = solver.FindSolution();

            //Assert
            indexes.Distinct().Count().Should().Be(TestData.CityNames.Length);
        }


        [Fact]
        public void Solution_ShouldNotRepeatPoints()
        {
            //Arrange
            var solver = new Solver((startIndex, endIndex) => TestData.CostMatrix[startIndex, endIndex],
                                    TestData.CityNames.Length);

            //Act
            var indexes = solver.FindSolution();

            //Assert
            indexes.Distinct().Count().Should().Be(indexes.Length);
        }


        [Fact]
        public void Solution_ShouldShouldRespectStartingPoint()
        {
            //Arrange
            var firstPointIndex = 7;
            var solver = new Solver((startIndex, endIndex) => TestData.CostMatrix[startIndex, endIndex],
                                    TestData.CityNames.Length);
            solver.SetStartNode(firstPointIndex);

            //Act
            var indexes = solver.FindSolution();

            //Assert
            indexes[0].Should().Be(firstPointIndex);
            indexes.Distinct().Count().Should().Be(indexes.Length);
            indexes.Distinct().Count().Should().Be(TestData.CityNames.Length);
        }

        [Fact]
        public void Solution_ShouldShouldRespectEndingPoint()
        {
            //Arrange
            var lastPointIndex = 7;
            var solver = new Solver((startIndex, endIndex) => TestData.CostMatrix[startIndex, endIndex],
                                    TestData.CityNames.Length,
                                    new Solver.SolverConfiguration());
            solver.SetEndNode(lastPointIndex);

            //Act
            var indexes = solver.FindSolution();

            //Assert
            indexes[^1].Should().Be(lastPointIndex);
            indexes.Distinct().Count().Should().Be(indexes.Length);
            indexes.Distinct().Count().Should().Be(TestData.CityNames.Length);
        }

        [Fact]
        public void Solution_ShouldShouldRespectStartingAndEndingPoint()
        {
            //Arrange
            var lastPointIndex = 7;
            var firstPointIndex = 6;
            var solver = new Solver((startIndex, endIndex) => TestData.CostMatrix[startIndex, endIndex],
                                    TestData.CityNames.Length,
                                    new Solver.SolverConfiguration());
            solver.SetStartNode(firstPointIndex);
            solver.SetEndNode(lastPointIndex);

            //Act
            var indexes = solver.FindSolution();

            //Assert
            indexes[0].Should().Be(firstPointIndex);
            indexes[^1].Should().Be(lastPointIndex);
            indexes.Distinct().Count().Should().Be(indexes.Length);
            indexes.Distinct().Count().Should().Be(TestData.CityNames.Length);
        }
    }
}