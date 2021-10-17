using System;
using System.Linq;
using FluentAssertions;
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
                                    TestData.CityNames);

            //Act
            var (cities, indexes) = solver.FindSolution();

            //Assert
            cities.Distinct().Count().Should().Be(TestData.CityNames.Length);
        }


        [Fact]
        public void Solution_ShouldNotRepeatPoints()
        {
            //Arrange
            var solver = new Solver((startIndex, endIndex) => TestData.CostMatrix[startIndex, endIndex],
                                    TestData.CityNames);

            //Act
            var (cities, indexes) = solver.FindSolution();

            //Assert
            cities.Distinct().Count().Should().Be(cities.Length);
        }



        [Fact]
        public void Solution_ShouldShouldRespectStartingPoint()
        {
            //Arrange
            var firstPointIndex = 7;
            var solver = new Solver((startIndex, endIndex) => TestData.CostMatrix[startIndex, endIndex],
                                    TestData.CityNames);
            solver.SetStartNode(TestData.CityNames[firstPointIndex]);

            //Act
            var (cities, indexes) = solver.FindSolution();

            //Assert
            cities[0].Should().Be(TestData.CityNames[firstPointIndex]);
            cities.Distinct().Count().Should().Be(cities.Length);
            cities.Distinct().Count().Should().Be(TestData.CityNames.Length);
        }

        [Fact]
        public void Solution_ShouldShouldRespectEndingPoint()
        {
            //Arrange
            var lastPointIndex = 7;
            var solver = new Solver((startIndex, endIndex) => TestData.CostMatrix[startIndex, endIndex],
                                    TestData.CityNames,
                                    new Solver.SolverConfiguration());
            solver.SetEndNode(TestData.CityNames[lastPointIndex]);

            //Act
            var (cities, indexes) = solver.FindSolution();

            //Assert
            cities[^1].Should().Be(TestData.CityNames[lastPointIndex]);
            cities.Distinct().Count().Should().Be(cities.Length);
            cities.Distinct().Count().Should().Be(TestData.CityNames.Length);
        }

        [Fact]
        public void Solution_ShouldShouldRespectStartingAndEndingPoint()
        {
            //Arrange
            var lastPointIndex = 7;
            var firstPointIndex = 6;
            var solver = new Solver((startIndex, endIndex) => TestData.CostMatrix[startIndex, endIndex],
                                    TestData.CityNames,
                                    new Solver.SolverConfiguration());
            solver.SetStartNode(TestData.CityNames[firstPointIndex]);
            solver.SetEndNode(TestData.CityNames[lastPointIndex]);

            //Act
            var (cities, indexes) = solver.FindSolution();

            //Assert
            cities[0].Should().Be(TestData.CityNames[firstPointIndex]);
            cities[^1].Should().Be(TestData.CityNames[lastPointIndex]);
            cities.Distinct().Count().Should().Be(cities.Length);
            cities.Distinct().Count().Should().Be(TestData.CityNames.Length);
        }


    }
}
