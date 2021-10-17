namespace RLib.Tsp.Enums
{
    public enum eFirstSolutionStrategy
    {
        /// <summary>
        /// Finds cheapest arc and then add next nodes to route, finding shortest arc each time.
        /// </summary>
        NearestNeighbor,
        /// <summary>
        /// Connect nodes randomly.
        /// </summary>
        Random,
        /// <summary>
        /// Creates cheapest connections between unconnected nodes, then iteratively finds cheapest connections between created arcs.
        /// </summary>
        ConnectCheapestArcs
    }
}