namespace RouteCleaner.PolygonUtils
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using RouteFinderDataModel;

    public static class PolygonFactory
    {
        /// <summary>
        /// Note: could be optimized.
        /// </summary>
        /// <returns></returns>
        public static List<Polygon> BuildPolygons(Way[] ways)
        {
            var polygons = new List<Polygon>();
            if (ways.Length == 0)
            {
                return polygons;
            }

            var startToWay = new Dictionary<Node, Way>();
            foreach (var way in ways)
            {
                var start = way.Nodes.First();
                if (startToWay.ContainsKey(start))
                {
                    throw new InvalidOperationException($"Node {start} is start for 2 ways in region. This violates single path assumption.");
                }

                startToWay.Add(start, way);
            }

            var unusedWays = new HashSet<Way>(ways);
            while (unusedWays.Count > 0)
            {
                var polygonWays = new LinkedList<Way>();
                var way = unusedWays.First();
                var firstNode = way.Nodes.First();
                unusedWays.Remove(way);
                polygonWays.AddLast(way);
                while (true)
                {
                    var end = way.Nodes.Last();
                    if (end == firstNode)
                    {
                        break;
                    }

                    if (!startToWay.ContainsKey(end))
                    {
                        throw new InvalidOperationException($"Can't link node {end}");
                    }

                    way = startToWay[end];
                    polygonWays.AddLast(way);
                    unusedWays.Remove(way);
                }
                polygons.Add(new Polygon(polygonWays));
            }

            return polygons;
        }
    }
}
