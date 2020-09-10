using RouteCleaner.Model;
using RouteFinder;
using System.Collections.Generic;

namespace RouteCleaner
{
    public class RequiredEdgeGraphFilter : IGraphFilter
    {

        public Graph<Node> Filter(Graph<Node> graph)
        {
            var reducedNodes = GetPathsBetweenRequiredNodes(graph);
            var reducedGraph = graph.ReduceToVertexSet(reducedNodes);
            return reducedGraph;
        }

        /// <summary>
        /// Construct a set of vertices that are on the shortest path between any two nodes in the required node set.
        /// These may include non-required nodes.
        /// </summary>
        /// <param name="g"></param>
        /// <returns></returns>
        private HashSet<Node> GetPathsBetweenRequiredNodes(Graph<Node> g)
        {
            var fullVertexSet = new HashSet<Node>(g.MustHitVertices);
            foreach (var node in g.MustHitVertices)
            {
                var adsp = new AllDestinationShortestPaths<Node>(node, g.MustHitVertices, g, (_, x) => x.Distance);
                adsp.Run();
                var traversalPaths = adsp.TraversalPath;
                foreach (var endNode in g.MustHitVertices)
                {
                    var localNode = endNode;
                    while (traversalPaths.ContainsKey(localNode) && localNode != node)
                    {
                        fullVertexSet.Add(localNode);
                        localNode = traversalPaths[localNode];
                    }
                }
            }

            return fullVertexSet;
        }
    }
}
