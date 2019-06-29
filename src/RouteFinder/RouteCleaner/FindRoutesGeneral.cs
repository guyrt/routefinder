using RouteCleaner.Model;
using RouteCleaner.Transformers;
using RouteFinder;


namespace RouteCleaner
{
    public class FindRoutesGeneral
    {
        private readonly Graph<Node> _graph;
        private readonly DirectedEdgeMetadata<Node, Way> _originalEdgeWays;

        public FindRoutesGeneral(Geometry geometry)
        {
            var localGeometry = new OnlyTraversable().Transform(geometry);
            var newWays = new SplitBisectedWays().Transform(geometry.Ways);
            var gb = new GraphBuilder(new NoopGraphFilter());
            _graph = gb.BuildGraph(newWays.ToArray(), out var originalEdgeWays);
            _originalEdgeWays = originalEdgeWays;
        }

        /// <summary>
        /// Find a route from start to any one of ends.
        /// 
        /// Returns all suitable routes.
        /// </summary>
        public void FindRoutes(Way startWay, Node start, Node[] ends, double maxLength)
        {
            
        }
    }
}
