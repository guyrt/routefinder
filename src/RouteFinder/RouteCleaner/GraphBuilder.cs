using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using RouteCleaner.Model;
using RouteFinder;

namespace RouteCleaner
{
    public class GraphBuilder
    {

        private readonly IGraphFilter _filter;

        private readonly IGraphCoster _graphCoster;

        public GraphBuilder(IGraphFilter graphFilter, IGraphCoster graphCoster)
        {
            _filter = graphFilter;
            _graphCoster = graphCoster;
        }

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

            var reducedGraph = _filter.Filter(g);
            return reducedGraph;
        }

        private void AddWay(Graph<Node> graph, Way way, bool mustHit, DirectedEdgeMetadata<Node, Way> originalEdgeWays)
        {
            var nodes = way.Nodes;
            var totalDistance = 0.0;
            for (var i = 0; i < nodes.Count() - 1; i++)
            {
                totalDistance += SimpleDistance.Compute(nodes[i], nodes[i + 1]);
            }

            var id1 = nodes.First();
            var id2 = nodes.Last();
            var weight = _graphCoster.Cost(way, totalDistance);
            if (graph.AddEdge(id1, id2, totalDistance, weight, mustHit))
            {
                originalEdgeWays.Add(id1, id2, way);
            }

            if (mustHit)
            {
                graph.MustHitVertices.Add(id1);
                graph.MustHitVertices.Add(id2);
            }
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
