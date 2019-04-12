using RouteCleaner.Model;
using RouteFinder;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Text;
using System.IO;
using RouteCleaner;
using Newtonsoft.Json;

namespace RouteFinderCmd
{
    public class RouteDetailOutputter
    {
        private Dictionary<Node, HashSet<Way>> _nodeMap;

        private readonly string _outputLocation;

        private readonly string _filename;

        public RouteDetailOutputter(List<Way> ways, string outputLocation, string graphOutputFilename)
        {
            _outputLocation = outputLocation;
            _filename = graphOutputFilename;
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
            var waySteps = BuildWaySteps(route);

            // make path output graph
            BuildGraph(waySteps);

            // make text output
            var textOutput = BuildTextDirections(waySteps);
            Console.Write(textOutput);

        }

        private void BuildGraph(LinkedList<WayStep> waySteps)
        {
            var fullPath = Path.Combine(_outputLocation, _filename);
            var converter = new GeoJsonConverter();

            var ways = waySteps.GroupBy(x => x.Path).ToDictionary(x => x.Key, x => x.Count());
            foreach (var way in ways)
            {
                way.Key.Tags.Add("EdgeWeight", way.Value.ToString());
            }
            var polygonOut = converter.Convert(ways.Keys);
            var serialized = JsonConvert.SerializeObject(polygonOut);
            File.WriteAllLines(fullPath, new[] { serialized });

        }

        private string BuildTextDirections(LinkedList<WayStep> waySteps)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Start at Node {waySteps.First().Start}");

            WayStep previousStep = null;
            foreach (var step in waySteps)
            {
                if (step.End == previousStep?.Start && previousStep?.End == step.Start)
                {
                    sb.AppendLine($"Turn around and go back to {step.End} on {step.Path.Name} ({step.Weight} miles)");
                }
                else
                {
                    var prefix = "[Stay straight] ";
                    if (previousStep?.Path.Name != step.Path.Name)
                    {
                        prefix = "[Turn] ";
                    }
                    sb.Append($"{prefix} {step.Weight} miles on {step.Path.Name} to ");
                    sb.AppendLine($"intersection of {string.Join(", ", step.IntersectionPoints.Select(x => x.Name).Distinct())}");
                }

                previousStep = step;
            }

            return sb.ToString();
        }

        private LinkedList<WayStep> BuildWaySteps(LinkedList<WeightedAdjacencyNode<Node>> route)
        {
            var waySteps = new LinkedList<WayStep>();
            var start = route.First.Value;
            foreach (var nextNode in route.Skip(1))
            {
                var startWays = _nodeMap[start.Vertex];
                var nextNodeWays = _nodeMap[nextNode.Vertex];

                var intersectingWay = startWays.Intersect(nextNodeWays);
                if (!intersectingWay.Any())
                {
                    throw new ArgumentException($"You can't get to {nextNode.Vertex} from {start.Vertex}");
                }
                else
                {
                    var way = intersectingWay.First();
                    waySteps.AddLast(new WayStep
                    {
                        Start = start.Vertex,
                        End = nextNode.Vertex,
                        Path = way,
                        IntersectionPoints = nextNodeWays.ToArray(),
                        Weight = nextNode.Weight
                    });
                }

                start = nextNode;
            }
            return waySteps;
        }

        private class WayStep
        {
            public Node Start { get; set; }
            public Node End { get; set; }
            public Way Path { get; set; }
            public Way[] IntersectionPoints { get; set; }
            public double Weight;
        }
    }
}
