using RouteCleaner.Model;
using RouteFinder;

namespace RouteCleaner
{
    public interface IGraphFilter
    {
        Graph<Node> Filter(Graph<Node> graph);
    }
}
