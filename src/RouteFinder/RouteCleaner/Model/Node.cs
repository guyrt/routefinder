using System;
using System.Collections.Generic;

namespace RouteCleaner.Model
{
    /// <summary>
    /// A single point in space.
    /// </summary>
    public class Node : TaggableIdentifiableElement
    {
        public Node(string id, double latitude, double longitude, Dictionary<string, string> tags = null) : base(id, tags)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        public double Latitude { get; }


        public double Longitude { get; }

        public static Comparer<Node> NodeComparer = Comparer<Node>.Create((n1, n2) => string.Compare(n1.Id, n2.Id, StringComparison.Ordinal));

        public override string ToString()
        {
            return $"https://www.openstreetmap.org/node/{Id}";
        }
    }
}
