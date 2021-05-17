using System;
using System.Collections.Generic;
using System.Linq;

namespace RouteFinder.GreedyRoute
{
    /// <summary>
    /// Route finder using greedy operation: push closest pairs of items together.
    /// </summary>
    /// <remarks>
    /// Uses a potentially naive method. First, find nearest neighbor for each node. Order nodes in descending order
    /// by maximum gap to a near neighbor.
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    public class GreedyGraphAugmenter<T> : IEulerianGraphAugmenter<T>
    {

        public Graph<T> AugmentGraph(Graph<T> graph)
        {
            var extraWeight = 0.0;
            while (true)
            {
                if (!graph.OddDegreeNodes().Any())
                {
                    break;
                }
                var oddDegreeNodes = graph.OddDegreeNodes();
                var startingNode = oddDegreeNodes.First();
                var adsp = new AllDestinationShortestPaths<T>(startingNode, graph.OddDegreeNodes().Intersect(graph.MustHitVertices), graph, (_, x) => x.Distance);
                adsp.Run();
                var paths = adsp.TraversalPath;
                var nodeOrder = adsp.LowestCost.OrderByDescending(kvp => kvp.Value).Select(kvp => kvp.Key);

                // find pairs of nodes on the shortest path tree
                var oddNodeLookup = new HashSet<T>(graph.OddDegreeNodes());
                var usedNodes = new HashSet<T>();
                foreach (var node in nodeOrder)
                {
                    if (node.Equals(startingNode) || usedNodes.Contains(node))
                    {
                        continue;
                    }
                    var endNode = paths[node];
                    var path = new List<T>(new[] { node, endNode });
                    while (!oddNodeLookup.Contains(endNode) && paths.ContainsKey(endNode))
                    {
                        endNode = paths[endNode];
                        path.Add(endNode);
                    }

                    if (!usedNodes.Contains(node) && !usedNodes.Contains(endNode))
                    {
                        // at this stage, node and endNode are a short path between odd nodes.
                        usedNodes.Add(endNode);
                        usedNodes.Add(node);
                        extraWeight += graph.AddEdgeCardinality(path);
                    }
                }
            }

            Console.WriteLine($"GreedyGraphAugmenter added {extraWeight}");
            return graph;
        }

    }
}
