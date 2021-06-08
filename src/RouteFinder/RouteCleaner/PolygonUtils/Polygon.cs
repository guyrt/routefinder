namespace RouteCleaner.PolygonUtils
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Azure.Cosmos.Spatial;
    using RouteFinderDataModel;
    using RouteFinderDataModel.Tools;
    using GlobalSettings;
    using System;

    /// <summary>
    /// A polygon is a series of nodes that form 1 or more ways that connect into a circuit.
    /// </summary>
    public class Polygon
    {
        public const double FlattenThreshold = -1 + 1e-2;
        public List<Node> EliminatedNodes;

        // If true, this polygon marks an outer boundary. If false, then it's inner and being inside the node means the relation doesn't
        // contain the point.
        public bool IsOuter { get; }

        private readonly NodeArrayBounds nodeArrayBounds;

        internal Polygon(LinkedList<Way> ways, List<bool> reversals, bool outer)
        {
            IsOuter = outer;
            Ways = ways;
            Reversals = reversals;
            Nodes = BuildNodes(ways, reversals);

            nodeArrayBounds = new NodeArrayBounds(Nodes);
        }

        private List<Node> BuildNodes(LinkedList<Way> ways, List<bool> reversals)
        {
            var originalNodes = new List<Node>();
            var idx = 0;
            foreach (var way in ways)
            {
                var nodes = way.Nodes.ToArray();
                if (reversals[idx])
                {
                    nodes = this.Reverse(nodes);
                }
                var lastNode = nodes.Last();
                originalNodes.Add(nodes[0]);
                foreach (var node in nodes.Skip(1))
                {
                    if (node != lastNode)
                    {
                        originalNodes.Add(node);
                    }
                }

                idx++;
            }

            if (!RouteCleanerSettings.GetInstance().PolygonsShouldConsolidateStraightEdges)
            {
                return originalNodes;
            }

            var dotProducts = new List<double>();
            for (var i = 0; i < originalNodes.Count; i++)
            {
                dotProducts.Add(PolygonUtils.ComputeDotProduct(originalNodes, i, true));
            }

            var newNodes = new List<Node>();
            EliminatedNodes = new List<Node>();
            // Keep nodes where the angle is not close enough to 180 degrees (based on threshold)
            for (var i = 0; i < dotProducts.Count; i++)
            {
                if (Math.Abs(dotProducts[i]) > FlattenThreshold)
                {
                    newNodes.Add(originalNodes[i]);
                }
                else
                {
                    EliminatedNodes.Add(originalNodes[i]);
                }
            }

            if (newNodes[0] == newNodes[newNodes.Count - 1])
            {
                throw new System.Exception($"Node {newNodes[0]} duplicate at start and end.");
            }

            return newNodes;
        }

        private Node[] Reverse(Node[] nodes)
        {
            var nodeLength = nodes.Length;
            var newArr = new Node[nodes.Length];
            for (var i = 0; i < nodeLength; i++)
            {
                newArr[i] = nodes[nodeLength - 1 - i];
            }
            return newArr;
        }

        /// <summary>
        /// Ordered set of Ways where the end of node n_i is the start/end of node n_j depending on reversals.
        /// </summary>
        public LinkedList<Way> Ways { get; }

        public List<Node> Nodes { get; }

        /// <summary>
        /// List of bools corresponding to the list of Ways. If all values are false, then the end of Ways[n] is start of Wans[n+1].
        /// If Reversals[n] == true, then the Nodes in Way[n] must be reversed for the quality Ways[n].End == Ways[n+1].Start. 
        /// </summary>
        public List<bool> Reversals { get; }

        public (double minLng, double minLat, double maxLng, double maxLat) Bounds => this.nodeArrayBounds.Bounds;

        private bool? _isConvex;

        public bool IsConvex => (_isConvex ?? (_isConvex = SetConvexity())).Value;

        private bool SetConvexity()
        {
            if (Nodes.Count == 3)
            {
                return true;
            }

            var originalNumber = PolygonUtils.CrossProductZ(Nodes, 0) > 0 ? 1 : -1;
            for (var i = 1; i < Nodes.Count; i++)
            {
                var newCp = PolygonUtils.CrossProductZ(Nodes, i);
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
        /// A negative value means that negative cross-product that implies a convex angle. This also
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

    }
}
