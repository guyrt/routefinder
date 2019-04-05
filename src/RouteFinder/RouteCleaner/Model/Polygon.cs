using System;
using System.Collections.Generic;
using System.Linq;

namespace RouteCleaner.Model
{
    /// <summary>
    /// A polygon is a series of nodes that form 1 or more ways that connect into a circuit.
    /// </summary>
    public class Polygon
    {

        public const double FlattenThreshold = -1 + 1e-2;
        public List<Node> EliminatedNodes;

        private Polygon(LinkedList<Way> ways)
        {
            Ways = ways;
            Nodes = BuildNodes(ways);
        }

        private List<Node> BuildNodes(LinkedList<Way> ways)
        {
            var originalNodes = new List<Node>();
            foreach (var way in ways)
            {
                var lastNode = way.Nodes.Last();
                originalNodes.Add(way.Nodes.First());
                foreach (var node in way.Nodes.Skip(1))
                {
                    if (node != lastNode)
                    {
                        originalNodes.Add(node);
                    }
                }
            }

            var dotProducts = new List<double>();
            for (var i = 0; i < originalNodes.Count; i++)
            {
                dotProducts.Add(PolygonUtils.PolygonUtils.ComputeDotProduct(originalNodes, i, true));
            }

            var newNodes = new List<Node>();
            EliminatedNodes = new List<Node>();
            // Keep nodes where the angle is not close enough to 180 degrees (based on threshold)
            for (var i = 0; i < dotProducts.Count; i++)
            {
                if (dotProducts[i] > FlattenThreshold)
                {
                    newNodes.Add(originalNodes[i]);
                }
                else
                {
                    EliminatedNodes.Add(originalNodes[i]);
                }
            }

            return newNodes;
        }

        /// <summary>
        /// Ordered set of Ways where the end of node n_i is the start of node n_j
        /// </summary>
        public LinkedList<Way> Ways { get; }

        public List<Node> Nodes { get; }

        private bool? _isConvex;

        public bool IsConvex => (_isConvex ?? (_isConvex = SetConvexity())).Value;

        private bool SetConvexity()
        {
            if (Nodes.Count == 3)
            {
                return true;
            }

            var originalNumber = PolygonUtils.PolygonUtils.CrossProduct(Nodes, 0) > 0 ? 1 : -1;
            for (var i = 1; i < Nodes.Count; i++)
            {
                var newCp = PolygonUtils.PolygonUtils.CrossProduct(Nodes, i);
                if (newCp * originalNumber < 0)
                {
                    return false;
                }
            }

            return true;
        }

        private int? _direction;

        /// <summary>
        /// Direction is either +1 or -1 and corresponds to the sign of the cross product of convex angles.
        ///
        /// A negative value means that a cross-product that is negative denotes a convex angle. This also
        /// corresponds to a counter-clockwise direction of travel around the parimeter of the polygon.
        /// </summary>
        public int Direction => (_direction ?? (_direction = SetDirection())).Value;

        /// <summary>
        /// Use shoelace algorithm.
        /// </summary>
        /// <returns></returns>
        private int SetDirection()
        {
            var shoelaceProductSum = 0.0;
            var nodeCount = Nodes.Count;

            for (var i = 1; i < nodeCount; i++)
            {
                var n1 = Nodes[i];
                var n2 = Nodes[(i + 1) % nodeCount];
                shoelaceProductSum += (n2.Latitude - n1.Latitude) * (n2.Longitude + n1.Longitude);
            }

            return shoelaceProductSum < 0 ? -1 : 1;
        }

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
