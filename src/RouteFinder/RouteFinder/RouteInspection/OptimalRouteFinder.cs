namespace RouteFinder.RouteInspection
{
    public class OptimalRouteFinder<T> : IEulerianGraphAugmenter<T>
    {
        public Graph<T> AugmentGraph(Graph<T> graph)
        {
            var surfaceGraph = new SurfaceGraph<T>(graph.Neighbors);
            surfaceGraph.Optimize();

            return graph;
        }
    }
}
