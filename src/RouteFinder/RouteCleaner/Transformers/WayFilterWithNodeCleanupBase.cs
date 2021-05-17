using RouteFinderDataModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RouteCleaner.Transformers
{
    internal static class WayFilterWithNodeCleanup
    {

        public static Geometry Transform(Geometry geometry, Func<Way, bool> keepWayFunction)
        {
            var keepWays = new List<Way>();
            var dropWays = new List<Way>();
            var dropNodes = new HashSet<Node>();
            var keepNodes = new HashSet<Node>();
            foreach (var way in geometry.Ways)
            {
                if (keepWayFunction(way))
                {
                    keepWays.Add(way);
                    keepNodes.UnionWith(way.Nodes);
                }
                else
                {
                    dropWays.Add(way);
                    dropNodes.UnionWith(way.Nodes);
                }
            }
            dropNodes.ExceptWith(keepNodes);
            return new Geometry(geometry.Nodes.Except(dropNodes).ToArray(), keepWays.ToArray(), geometry.Relations);
        }
    }
}
