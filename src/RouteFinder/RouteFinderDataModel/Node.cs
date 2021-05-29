namespace RouteFinderDataModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;
    using RouteFinderDataModel.Thin;

    /// <summary>
    /// A single point in space.
    /// </summary>
    public class Node
    {
        public Node(string id, double latitude, double longitude)
        {
            Id = id;
            Relations = new List<string>();
            ContainingWays = new List<string>();
            Latitude = latitude;
            Longitude = longitude;
        }

        [JsonProperty("id")]
        public string Id { get; }

        public List<string> Relations { get; set; }

        public List<string> ContainingWays { get; set; }

        public double Latitude { get; }

        public double Longitude { get; }

        public static Comparer<Node> NodeComparer = Comparer<Node>.Create((n1, n2) => string.Compare(n1.Id, n2.Id, StringComparison.Ordinal));

        public override string ToString()
        {
            return $"https://www.openstreetmap.org/node/{Id}";
        }

        public ThinNode ToThin()
        {
            return new ThinNode
            {
                Id = Id,
                Latitude = Latitude,
                Longitude = Longitude
            };
        }
    }
}
