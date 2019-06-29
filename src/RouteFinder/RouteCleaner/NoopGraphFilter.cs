using RouteCleaner.Model;
using RouteFinder;

namespace RouteCleaner
{
    public class NoopGraphFilter : IGraphFilter
    {
        public Graph<Node> Filter(Graph<Node> graph)
        {
            return graph;
        }
    }
}
