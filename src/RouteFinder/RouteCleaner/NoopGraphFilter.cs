namespace RouteCleaner
{
    using RouteFinderDataModel;
    using RouteFinder;

    public class NoopGraphFilter : IGraphFilter
    {
        public Graph<Node> Filter(Graph<Node> graph)
        {
            return graph;
        }
    }
}
