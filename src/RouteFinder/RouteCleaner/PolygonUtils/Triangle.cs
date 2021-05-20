namespace RouteCleaner.PolygonUtils
{
    using System;
    using System.Diagnostics;
    using System.Linq;

    using RouteFinderDataModel;

    [DebuggerDisplay("Triangle {Nodes[0].Id} {Nodes[1].Id} {Nodes[2].Id}")]
    public class Triangle
    {
        public Triangle(Node n1, Node n2, Node n3)
        {
            Nodes = new[] {n1, n2, n3};
        }

        public Node[] Nodes { get; }

        public double Area
        {
            get
            {
                var lengths = new[]
                {
                    PolygonUtils.LineLength(Nodes[0], Nodes[1]),
                    PolygonUtils.LineLength(Nodes[1], Nodes[2]),
                    PolygonUtils.LineLength(Nodes[2], Nodes[0])
                };
                var s = lengths.Sum() / 2;
                return Math.Sqrt(s * (s - lengths[0]) * (s - lengths[1]) * (s - lengths[2]));
            }
        }

        /// <summary>
        /// Compute containment using three cross products.
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public bool Contains(Node n)
        {
            LastHitWasOnLine = false;
            var cp1Raw = PolygonUtils.CrossProductZ(Nodes[1], Nodes[0], n);
            if (Math.Abs(cp1Raw) < 1e-12)
            {
                LastHitWasOnLine = true;
                return true;
            }
            var cp2Raw = PolygonUtils.CrossProductZ(Nodes[2], Nodes[1], n);
            if (Math.Abs(cp2Raw) < 1e-12)
            {
                LastHitWasOnLine = true;
                return true;
            }
            var cp1 = cp1Raw >= 0 ? 1 : -1;
            var cp2 = cp2Raw >= 0 ? 1 : -1;
            if (cp1 * cp2 < 0)
            {
                return false;
            }
            var cp3Raw = PolygonUtils.CrossProductZ(Nodes[0], Nodes[2], n);
            if (Math.Abs(cp3Raw) < 1e-12)
            {
                LastHitWasOnLine = true;
                return true;
            }
            var cp3 = cp3Raw >= 0 ? 1 : -1;
            return cp1 * cp3 >= 0;
        }

        public bool LastHitWasOnLine = false;
    }
}
