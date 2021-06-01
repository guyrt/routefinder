namespace RouteCleaner.PolygonUtils
{
    using System.Collections.Generic;
    using System.Linq;
    using RouteFinderDataModel;

    public class PolygonContainment
    {
        private readonly Polygon _polygon;

        public PolygonContainment(Polygon polygon)
        {
            _polygon = polygon;
        }

        // Winding number based implementation.
        public static bool Contains(Polygon polygon, Node node)
        {
            if (PolygonContainment.SimpleNonOverlap(polygon, node))
            {
                return false;
            }

            var windingNumber = 0;
            var startLat = polygon.Nodes[0].Latitude;
            var startLng = polygon.Nodes[0].Longitude;
            for (int i = 0; i < polygon.Nodes.Count; i++)
            {
                var endNode = polygon.Nodes[(i + 1) % polygon.Nodes.Count];
                var endLat = endNode.Latitude;
                var endLng = endNode.Longitude;

                if (startLat <= node.Latitude)
                {
                    if (endLat > node.Latitude)
                    {
                        if (IsLeft(startLat, startLng, endLat, endLng, node.Latitude, node.Longitude) > 0)
                        {
                            windingNumber++; // up intersection
                        }
                    }
                }
                else
                {
                    if (endLat <= node.Latitude)
                    {
                        if (IsLeft(startLat, startLng, endLat, endLng, node.Latitude, node.Longitude) < 0)
                        {
                            windingNumber--; // down intersection
                        }
                    }
                }

                startLat = endLat;
                startLng = endLng;
            }

            return windingNumber != 0;
        }

        private static double IsLeft(double p0Lat, double p0Lng, double p1Lat, double p1Lng, double p2Lat, double p2Lng)
        {
            return ((p1Lng - p0Lng) * (p2Lat - p0Lat) - (p2Lng - p0Lng) * (p1Lat - p0Lat));
        }

        /// <summary>
        /// Compute polygon containment of a <see cref="Way"/>.
        /// 
        /// For now, splits take value of the first node in Way. We could change to "all in" or "one in" as option I suppose.
        /// </summary>
        /// <param name="way"></param>
        /// <returns>Tuple of Way lists. First list is set of ways that are in the polygon. Second is list of ways that are outside the polygon.</returns>
        public (List<Way> contains, List<Way> notContains) SplitWayByContainment(Way way)
        {
            if (SimpleNonOverlap(way))
            {
                return (null, new List<Way> { way });
            }

            var contains = new List<Way>();
            var notContains = new List<Way>();

            var nodeIn = Contains(this._polygon, way.Nodes.First());
            var splitWay = false;
            var nodes = new LinkedList<Node>();
            var i = 0;
            bool newNodeIn = false;
            foreach (var node in way.Nodes)
            {
                newNodeIn = Contains(this._polygon, node);
                nodes.AddLast(node);
                if (nodeIn != newNodeIn)
                {
                    var newWay = new Way(way.Id + "_containsplit_" + i++, nodes.ToArray(), new Dictionary<string, string>(way.Tags));
                    (nodeIn ? contains : notContains).Add(newWay);
                    splitWay = true;
                    nodeIn = newNodeIn;
                    nodes.Clear();
                    nodes.AddFirst(node);
                }
            }

            if (!splitWay)
            {
                (nodeIn ? contains : notContains).Add(way);
            }
            else
            {
                if (nodes.Count > 1)
                {
                    var newWay = new Way(way.Id + "_" + i++, nodes.ToArray(), new Dictionary<string, string>(way.Tags));
                    (newNodeIn ? contains : notContains).Add(newWay);
                }
            }

            return (contains: contains, notContains: notContains);
        }

        public PolygonContainmentRelation ComputePolygonRelation(Polygon polygon)
        {
            var someWayInPolygon = false;
            var someWayAndAllWaysInPolygon = true; // this will be anded together. Only valid if someWayInPolygon also true.
            foreach (var way in polygon.Ways)
            {
                (var contains, var notContains) = this.SplitWayByContainment(way);
                someWayInPolygon |= contains?.Count > 0; // something is inside the polygon.
                someWayAndAllWaysInPolygon &= notContains?.Count == 0; // nothing is outside the polygon.
            }

            return someWayInPolygon ? (someWayAndAllWaysInPolygon ? PolygonContainmentRelation.Contains : PolygonContainmentRelation.Overlap) : PolygonContainmentRelation.NoOverlap;
        }

        // Check based on bounding boxes. This quickly rules out containment for the most common case.
        private bool SimpleNonOverlap(Way w)
        {
            (double minLng, double minLat, double maxLng, double maxLat) = w.Bounds;
            (double polyMinLng, double polyMinLat, double polyMaxLng, double polyMaxLat) = this._polygon.Bounds;

            var nonOverlap = maxLat < polyMinLat|| polyMaxLat < minLat
                || maxLng < polyMinLng || polyMaxLng < minLng;

            return nonOverlap;
        }

        private static bool SimpleNonOverlap(Polygon polygon, Node n)
        {
            (double minLng, double minLat, double maxLng, double maxLat) = polygon.Bounds;

            var nonOverlap = n.Latitude < minLat || maxLat < n.Latitude
                || n.Longitude < minLng || maxLng < n.Longitude;

            return nonOverlap;
        }
    }
}
