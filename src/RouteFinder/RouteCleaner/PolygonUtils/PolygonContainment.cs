namespace RouteCleaner.PolygonUtils
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using RouteFinderDataModel;

    public class PolygonContainment
    {
        private readonly LinkedList<Triangle> _triangles;

        public PolygonContainment(Polygon polygon)
        {
            _triangles = new PolygonTriangulation(polygon).Triangulate();
        }

        public PolygonContainment(LinkedList<Triangle> triangles)
        {
            _triangles = triangles;
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
        /// </summary>
        /// <param name="way"></param>
        /// <returns>Tuple of Way lists. First list is </returns>
        public (List<Way> contains, List<Way> notContains) Containment(Way way)
        {
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

    }
}
