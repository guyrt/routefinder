using RouteFinderDataModel;
using RouteFinder;

namespace RouteCleaner
{
    public interface IGraphFilter
    {
        Graph<Node> Filter(Graph<Node> graph);
    }
}
