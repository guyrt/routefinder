using System;
using System.Collections.Generic;
using System.Linq;
using RouteCleaner;
using RouteCleaner.Model;
using RouteFinder;

namespace RouteFinderCmd
{
    public class GraphBuilder
    {
        public Graph<Node> BuildGraph(Way[] ways, out DirectedEdgeMetadata<Node, Way> originalEdgeWays)
        {
            originalEdgeWays = new DirectedEdgeMetadata<Node, Way>(Node.NodeComparer);

            var finalWays = CreateExtraEdges(ways);
            var nodes = new HashSet<Node>(finalWays.SelectMany(x => x.Nodes));

            var g = new Graph<Node>(nodes, Node.NodeComparer);
            foreach (var way in finalWays)
            {
                AddWay(g, way, way.MustHit, originalEdgeWays);
            }

            var reducedGraph = ReduceToRequiredNodes(g);
            return reducedGraph;
        }

        private static void AddWay(Graph<Node> graph, Way way, bool mustHit, DirectedEdgeMetadata<Node, Way> originalEdgeWays)
        {
            var nodes = way.Nodes;
            var totalCost = 0.0;
            for (var i = 0; i < nodes.Count() - 1; i++)
            {
                totalCost += SimpleDistanceCost.Compute(nodes[i], nodes[i + 1]);
            }

            var id1 = nodes.First();
            var id2 = nodes.Last();
            if (graph.AddEdge(id1, id2, totalCost, mustHit))
            {
                originalEdgeWays.Add(id1, id2, way);
            }

            if (mustHit)
            {
                graph.MustHitVertices.Add(id1);
                graph.MustHitVertices.Add(id2);
            }
        }

        private static Graph<Node> ReduceToRequiredNodes(Graph<Node> g)
        {
            var reducedNodes = GetPathsBetweenRequiredNodes(g);
            var reducedGraph = g.ReduceToVertexSet(reducedNodes);
            return reducedGraph;
        }

        /// <summary>
        /// Construct a set of vertices that are on the shortest path between any two nodes in the required node set.
        /// These may include non-required nodes.
        /// </summary>
        /// <param name="g"></param>
        /// <returns></returns>
        private static HashSet<Node> GetPathsBetweenRequiredNodes(Graph<Node> g)
        {
            var fullVertexSet = new HashSet<Node>(g.MustHitVertices);
            foreach (var node in g.MustHitVertices)
            {
                var adsp = new AllDestinationShortestPaths<Node>(node, g.MustHitVertices, g);
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

        private static List<Way> CreateExtraEdges(IEnumerable<Way> ways)
        {
            // identify nodes that will enter the graph
            var nodes = new HashSet<Node>();
            var finalWays = new List<Way>();

            var edgeTracker = new DirectedEdgeMetadata<Node, bool>(Node.NodeComparer);  // todo it would be good to get rid of this thing.
            foreach (var way in ways)
            {
                var start = way.Nodes.First();
                var end = way.Nodes.Last();
                if (start == end)
                {
                    // this is a loop!
                    nodes.Add(start);
                    if (way.Nodes.Length < 2)
                    {
                        throw new InvalidOperationException($"Way {way.Id} has too few nodes");
                    }

                    var copyNode = way.Nodes[1];
                    var newNode = new Node(copyNode.Id + "_copy_0", copyNode.Latitude, copyNode.Longitude);
                    var newWay1 = new Way(way.Id + "_gb_short", new[] { start, newNode }, new Dictionary<string, string>(way.Tags));
                    var newWay2 = new Way(way.Id + "_gb_short", way.Nodes.Skip(1).ToArray(), new Dictionary<string, string>(way.Tags));
                    var newWay3 = new Way(way.Id + "_gb_fake", new[] { copyNode, newNode }, new Dictionary<string, string>(way.Tags));
                    finalWays.Add(newWay1);
                    finalWays.Add(newWay2);
                    finalWays.Add(newWay3);
                    nodes.Add(newNode);
                    nodes.Add(copyNode);

                    edgeTracker.Add(newWay1.Nodes.First(), newWay1.Nodes.Last(), true);
                    edgeTracker.Add(newWay2.Nodes.First(), newWay2.Nodes.Last(), true);
                    edgeTracker.Add(newWay3.Nodes.First(), newWay3.Nodes.Last(), true);
                }
                else if (edgeTracker.ContainsKey(start, end))
                {
                    // two ways exist between the same two points. For instance, two 1-way lanes.
                    // we want to ignore these for now. We could eventually either say "run both!" or 
                    // take the shorter one?
                    // NOTE! this does drop a few sidewalks and could drop trails (none in Cougar when I checked)
                    // so maybe consider something smarter for paths/sidewalks and keep the exclusion only for roads.
                    //Console.WriteLine($"Graphbuilder is ignoring way {way.Id}");
                }
                else
                {
                    finalWays.Add(way);
                    nodes.Add(start);
                    nodes.Add(end);
                    edgeTracker.Add(start, end, true);
                }
            }

            return finalWays;
        }
    }
}
