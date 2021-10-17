# RLib.Tsp
Traveling Salesman problem solver written in C#. Solver is using 2-Opt algorithm implementation.

## Usage
Arc cost calculating delegate must be defined in solver constructor. Solver does not cache arc distances, performance of provided delegate can drastically change solution search time.


```csharp
var arcCost = new Func<int, int, float>((startIndex, endIndex) => costMatrix[startIndex, endIndex]);

var solver = new Solver(arcCost, cityNames);

var (solutionCities, solutionIndexes) = solver.FindSolution();
```

By default during possible node swap evaluation, only cost of affected nodes is calculated. In specific applications it is possible that travel cost between points depends also on other route segments. To cover this usecase, solver can be configure to evaluate swaps using full solution cost calculation delegate (in most cases this will increase solution search execution time).

```csharp
var arcCost = new Func<int, int, float>((startIndex, endIndex) => costMatrix[startIndex, endIndex]);

var solutionCost = new Func<int[], float>((solution) =>
                                                    {
                                                        float cost = 0f;
                                                        for (var i = 1; i < solution.Length; i++)
                                                        {
                                                            cost += arcCost(solution[i - 1], solution[i]) * 1+(0.1f*i);
                                                        }
                                                        return cost;
                                                    });
var solver = new Solver(arcCost, cityNames);
solver.UseFullSolutionCostValidation(solutionCost);

var (solutionCities, solutionIndexes) = solver.FindSolution();
```
Full solution cost validation is not used during initial solution creation.