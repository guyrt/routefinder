namespace RouteFinder
{
    /// <summary>
    /// Accept a graph with vertices that have an odd number of edges. Return a graph with no such edges.
    /// </summary>
    /// <remarks>
    /// A graph with no vertices with an odd number of edges has an Eulerian Circuit.
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    public interface IEulerianGraphAugmenter<T>
    {
        Graph<T> AugmentGraph(Graph<T> graph);
    }
}
