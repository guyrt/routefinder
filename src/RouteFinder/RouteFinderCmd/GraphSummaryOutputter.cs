using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using RouteCleaner;
using RouteCleaner.Model;
using RouteFinder;

namespace RouteFinderCmd
{
    public class GraphSummaryOutputter
    {
        private readonly string _outputLocation;

        public GraphSummaryOutputter(string outputLocation)
        {
            _outputLocation = outputLocation;
        }

        /// <summary>
        /// Produce an output that records ways that got kept.
        /// </summary>
        public void OutputGraph(Graph<Node> graph, DirectedEdgeMetadata<Node, Way> wayMap, string filename)
        {
            var fullPath = Path.Combine(_outputLocation, filename);

            var converter = new GeoJsonConverter();
            var ways = new Dictionary<Way, int>();

            foreach (var neighbor1 in graph.Neighbors)
            {
                foreach (var neighbor2 in neighbor1.Value.Where(x => x.PrimaryCopy))
                {
                    var way = wayMap[neighbor1.Key, neighbor2.Vertex];
                    if (ways.ContainsKey(way))
                    {
                        ways[way]++;
                    }
                    else
                    {
                        ways.Add(way, 1);
                    }
                }
            }

            var polygonOut = converter.Convert(ways.Keys);
            var serialized = JsonConvert.SerializeObject(polygonOut);
            File.WriteAllLines(fullPath, new[] { serialized });

        }
    }
}
