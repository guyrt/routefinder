using RouteCleaner.Model;
using RouteFinder;
using System.Collections.Generic;
using System.Linq;
using System;

namespace RouteFinderCmd
{
    public class RouteDetailOutputter
    {
        private Dictionary<Node, HashSet<Way>> _nodeMap;

        public RouteDetailOutputter(List<Way> ways)
        {
            _nodeMap = new Dictionary<Node, HashSet<Way>>();
            foreach (var way in ways)
            {
                foreach (var node in way.Nodes)
                {
                    if (!_nodeMap.ContainsKey(node))
                    {
                        _nodeMap.Add(node, new HashSet<Way> { way });
                    }
                    else
                    {
                        _nodeMap[node].Add(way);
                    }
                }
            }
        }

        public void DescribeRoutesAsWays(LinkedList<WeightedAdjacencyNode<Node>> route)
        {
            var start = route.First.Value;
            Console.WriteLine($"Start at {start.Vertex}");
            foreach (var nextNode in route.Skip(1))
            {
                var startWays = _nodeMap[start.Vertex];
                var nextNodeWays = _nodeMap[nextNode.Vertex];

                var intersectingWay = startWays.Intersect(nextNodeWays);
                if (!intersectingWay.Any())
                {
                    throw new ArgumentException($"You can get to {nextNode.Vertex} from {start.Vertex}");
                }
                else
                {
                    var way = intersectingWay.First();
                    Console.WriteLine($"Travel {nextNode.Weight} miles on {way.Name} to {nextNode.Vertex}");
                }

                start = nextNode;
            }
        }
    }
}
