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
        public bool Contains(Node node)
        {
            if (SimpleNonOverlap(node))
            {
                return false;
            }

            var windingNumber = 0;
            var startLat = _polygon.Nodes[0].Latitude;
            var startLng = _polygon.Nodes[0].Longitude;
            for (int i = 0; i < _polygon.Nodes.Count; i++)
            {
                var endNode = _polygon.Nodes[(i + 1) % _polygon.Nodes.Count];
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

        private double IsLeft(double p0Lat, double p0Lng, double p1Lat, double p1Lng, double p2Lat, double p2Lng)
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

            var nodeIn = Contains(way.Nodes.First());
            var splitWay = false;
            var nodes = new LinkedList<Node>();
            var i = 0;
            bool newNodeIn = false;
            foreach (var node in way.Nodes)
            {
                newNodeIn = Contains(node);
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
            (var wayBoundsMin, var wayBoundsMax) = w.Bounds;
            (var polyBoundsMin, var polyBoundsMax) = this._polygon.Bounds;

            var nonOverlap = wayBoundsMax.Position.Latitude < polyBoundsMin.Position.Latitude || polyBoundsMax.Position.Latitude < wayBoundsMin.Position.Latitude
                || wayBoundsMax.Position.Longitude < polyBoundsMin.Position.Longitude || polyBoundsMax.Position.Longitude < wayBoundsMin.Position.Longitude;

            return nonOverlap;
        }

        private bool SimpleNonOverlap(Node n)
        {
            (var polyBoundsMin, var polyBoundsMax) = this._polygon.Bounds;

            var nonOverlap = n.Latitude < polyBoundsMin.Position.Latitude || polyBoundsMax.Position.Latitude < n.Latitude
                || n.Longitude < polyBoundsMin.Position.Longitude || polyBoundsMax.Position.Longitude < n.Longitude;

            return nonOverlap;
        }
    }
}
