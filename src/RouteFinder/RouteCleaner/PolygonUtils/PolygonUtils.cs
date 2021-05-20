namespace RouteCleaner.PolygonUtils
{

    using System;
    using System.Collections.Generic;
    using RouteFinderDataModel;

    public static class PolygonUtils
    {
        public static double ComputeDotProduct(List<Node> nodes, int centerIndex, bool normalize = false)
        {
            var nodeLength = nodes.Count;
            var left = nodes[(centerIndex - 1 + nodeLength) % nodeLength];
            var center = nodes[centerIndex];
            var right = nodes[(centerIndex + 1) % nodeLength];

            var leftVector = new []{left.Latitude - center.Latitude, left.Longitude - center.Longitude};
            var rightVector = new[] { right.Latitude - center.Latitude, right.Longitude - center.Longitude };
            var dotProduct = leftVector[0] * rightVector[0] + leftVector[1] * rightVector[1];
            if (normalize)
            {
                dotProduct /= Math.Sqrt(leftVector[0] * leftVector[0] + leftVector[1] * leftVector[1]);
                dotProduct /= Math.Sqrt(rightVector[0] * rightVector[0] + rightVector[1] * rightVector[1]);
            }
            return dotProduct;
        }

        public static double CrossProductZ(List<Node> nodes, int centerIndex)
        {
            var nodeLength = nodes.Count;
            var left = nodes[(centerIndex - 1 + nodeLength) % nodeLength];
            var center = nodes[centerIndex];
            var right = nodes[(centerIndex + 1) % nodeLength];

            var crossProduct = CrossProductZ(left, center, right);
            return crossProduct;
        }

        public static double CrossProductZ(Node left, Node center, Node right)
        {
            var leftVector = new[] { left.Latitude - center.Latitude, left.Longitude - center.Longitude };
            var rightVector = new[] { right.Latitude - center.Latitude, right.Longitude - center.Longitude };

            var leftMagnitude = Math.Sqrt(leftVector[0] * leftVector[0] + leftVector[1] * leftVector[1]);
            var rightMagnitude = Math.Sqrt(rightVector[0] * rightVector[0] + rightVector[1] * rightVector[1]);
            var crossProduct = leftVector[0] * rightVector[1] - leftVector[1] * rightVector[0];
            return crossProduct;
        }

        public static double LineLength(Node n1, Node n2)
        {
            return Math.Sqrt(Math.Pow(n2.Latitude - n1.Latitude, 2) + Math.Pow(n2.Longitude - n1.Longitude, 2));
        }
    }
}
