using RouteFinderDataModel;
using System;
using System.Linq;
using System.Collections.Generic;

namespace RouteFinderCmd
{
    /// <summary>
    /// Count how often nodes are reused.
    /// </summary>
    internal static class TagReuse {

        public static void Summarize(Geometry geometry) {
            var tagCounter = geometry.Nodes.ToDictionary(n => n, _ => 0);
            foreach (var way in geometry.Ways)
            {
                foreach (var node in way.Nodes) {
                    tagCounter[node]+=1;
                }
            }

            var histogram = new Dictionary<int, int>();
            foreach (var kvp in tagCounter)
            {
                if (!histogram.ContainsKey(kvp.Value)) {
                    histogram.Add(kvp.Value, 0);
                }
                histogram[kvp.Value]++;
            }

            foreach(var kvp in histogram) {
                Console.WriteLine($"{kvp.Key}\t{kvp.Value}");
            }
        }
    }
}