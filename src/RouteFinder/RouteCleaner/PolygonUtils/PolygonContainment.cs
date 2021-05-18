namespace RouteCleaner.PolygonUtils
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using RouteFinderDataModel;

    public class PolygonContainment
    {
        private readonly Polygon _polygon;
        private readonly LinkedList<Triangle> _triangles;

        public PolygonContainment(Polygon polygon)
        {
            _triangles = new PolygonTriangulation(polygon).Triangulate();
            _polygon = polygon;
        }

        // todo - memoize so you don't need this and can always just call the triangulator.
        public PolygonContainment(Polygon polygon, LinkedList<Triangle> triangles)
        {
            _triangles = triangles;
            _polygon = polygon;
        }

        public bool Contains(Node node)
        {
            var triangleItem = _triangles.First;
            while (triangleItem != null)
            {
                var triangle = triangleItem.Value;
                if (triangleItem.Value.Contains(node))
                {
                    _triangles.Remove(triangleItem);
                    _triangles.AddFirst(triangle);

                    if (triangleItem.Value.LastHitWasOnLine)
                    {
                        Console.WriteLine($"Warning: Node {node.Id} hit on line.");
                    }

                    return true;
                }
                triangleItem = triangleItem.Next;
            }

            return false;
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
                someWayAndAllWaysInPolygon &= contains?.Count == 0; // nothing is outside the polygon.
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
    }
}
