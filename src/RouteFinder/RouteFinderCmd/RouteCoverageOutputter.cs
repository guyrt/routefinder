using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using RouteCleaner;
using RouteCleaner.Model;
using RouteFinder;

namespace RouteFinderCmd
{
    public class RouteCoverageOutputter
    {
        private readonly string _outputLocation;

        public RouteCoverageOutputter(string outputLocation)
        {
            _outputLocation = outputLocation;
        }

        /// <summary>
        /// Produce an output that records ways that got kept.
        /// </summary>
        public void OutputGraph(LinkedList<WeightedAdjacencyNode<Node>> route, DirectedEdgeMetadata<Node, Way> wayMap,
            string filename)
        {
            var fullPath = Path.Combine(_outputLocation, filename);
            var converter = new GeoJsonConverter();

//            var prevIt = route.First;
//            for (var it = prevIt.Next; it != null;)
//            {
//                if (prevIt.Value.Vertex != it.Value.Vertex)
//                {
//                    if (wayMap.ContainsKey(prevIt.Value.Vertex, it.Value.Vertex))
//                    {
//                        var way = wayMap[prevIt.Value.Vertex, it.Value.Vertex];
//                        Console.WriteLine(way.Tags["name"]);
//                    }
//                    else
//                    {
//                        Console.WriteLine("Missing");
//                    }
//                }
//
//                it = it.Next;
//            }

            var polygonOut = converter.Convert(route.Select(x => x.Vertex));
            var serialized = JsonConvert.SerializeObject(polygonOut);
            File.WriteAllLines(fullPath, new[] {serialized});
        }
    }
}
