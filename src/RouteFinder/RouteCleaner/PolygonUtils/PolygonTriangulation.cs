using System;
using System.Collections.Generic;
using System.Linq;
using RouteCleaner.Model;

namespace RouteCleaner.PolygonUtils
{
    /// <summary>
    /// Uses ear clipping
    /// 
    /// See https://www.geometrictools.com/Documentation/TriangulationByEarClipping.pdf
    /// </summary>
    public class PolygonTriangulation
    {
        private readonly LinkedList<Node> _vertices;

        private readonly HashSet<Node> _convexVertices;

        // nodes whose resultant angle is >180 degrees.
        private readonly HashSet<Node> _reflexVertices;

        private readonly Polygon _polygon;

        public PolygonTriangulation(Polygon polygon)
        {
            _polygon = polygon;
            _vertices = new LinkedList<Node>(polygon.Nodes);
            _convexVertices = new HashSet<Node>();
            _reflexVertices = new HashSet<Node>();

            for (var i = 0; i < _vertices.Count; i++)
            {
                if (PolygonUtils.CrossProduct(polygon.Nodes, i) * polygon.Direction >= 0)
                {
                    _convexVertices.Add(polygon.Nodes[i]);
                }
                else
                {
                    _reflexVertices.Add(polygon.Nodes[i]);
                }
            }

        }

        public LinkedList<Triangle> Triangulate()
        {
            var newPolygons = new List<Triangle>();

            while (true)
            {
                Ear ear;
                try
                {
                    ear = FindEar();
                }
                catch (InvalidOperationException)
                {
                    break;
                }

                var newTriangle = new Triangle(ear.Center.Value, ear.Next, ear.Prev);
                newPolygons.Add(newTriangle);

                if (_vertices.Count == 3)
                {
                    break;
                }

                // modify remaining nodes
                // an optimization to do here is to quit if _reflexVertices is empty. Remaining shape is convex, so we can simplify the interior search.
                // probably to do that, you want to rotate around perimeter finding small triangles to "clip" nonconvexities and avoid bifurcating the polygon.
                var nextNode = ear.Center.Next ?? _vertices.First;
                var previousNode = ear.Center.Previous ?? _vertices.Last;
                if (_reflexVertices.Contains(nextNode?.Value))
                {
                    var nextnextNode = nextNode?.Next?.Value ?? _vertices.First.Value;
                    var cp = PolygonUtils.CrossProduct(previousNode.Value, nextNode?.Value, nextnextNode);
                    if (cp * _polygon.Direction > 0)
                    {
                        _reflexVertices.Remove(nextNode?.Value);
                        _convexVertices.Add(nextNode?.Value);
                    }
                }

                if (_reflexVertices.Contains(previousNode?.Value))
                {
                    var prevprevNode = previousNode?.Previous?.Value ?? _vertices.Last.Value;
                    var cp = PolygonUtils.CrossProduct(prevprevNode, previousNode?.Value, nextNode?.Value);
                    if (cp * _polygon.Direction > 0)
                    {
                        _reflexVertices.Remove(previousNode?.Value);
                        _convexVertices.Add(previousNode?.Value);
                    }
                }

                _vertices.Remove(ear.Center);
            }

            return new LinkedList<Triangle>(newPolygons.OrderByDescending(t => t.Area));
        }

        /// <summary>
        /// An ear is a node that forms a convex angle such that no other nodes in the polygon exist in the triangle formed by
        /// the node and its neighbors.
        /// </summary>
        private Ear FindEar()
        {
            var endNode = _vertices.Last.Value;
            var nodePtr = _vertices.First;
            while (nodePtr.Value != endNode)
            {
                if (_convexVertices.Contains(nodePtr.Value))
                {
                    var next = nodePtr.Next?.Value ?? _vertices.First.Value;
                    var prev = nodePtr.Previous?.Value ?? _vertices.Last.Value;
                    var t = new Triangle(nodePtr.Value, prev, next);
                    var containsReflectiveNode = false;
                    foreach (var rNode in _reflexVertices)
                    {
                        if (next == rNode || prev == rNode)
                        {
                            continue;
                        }
                        if (t.Contains(rNode))
                        {
                            containsReflectiveNode = true;
                            break;
                        }
                    }

                    if (!containsReflectiveNode)
                    {
                        return new Ear(nodePtr, next, prev);
                    }
                }

                nodePtr = nodePtr.Next;
            }

            throw new InvalidOperationException("Ran out of Ears sooner than expected!");
        }

        private class Ear
        {
            public readonly LinkedListNode<Node> Center;
            public readonly Node Next;
            public readonly Node Prev;

            public Ear(LinkedListNode<Node> center, Node next, Node prev)
            {
                Center = center;
                Next = next;
                Prev = prev;
            }
        }
    }
}
