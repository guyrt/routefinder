namespace RouteFinderDataModel
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using Microsoft.Azure.Cosmos.Spatial;
    using Newtonsoft.Json;
    using RouteFinderDataModel.Thin;

    /// <summary>
    /// A single point in space.
    /// </summary>
    public class Node : TaggableIdentifiableElement
    {
        public Node(string id, double latitude, double longitude, Dictionary<string, string> tags = null) : base(id, tags)
        {
            Location = new Point(longitude, latitude);
            Relations = new ConcurrentBag<Relation>();
            ContainingWays = new List<Way>();
        }

        public ConcurrentBag<Relation> Relations { get; set; }

        public List<Way> ContainingWays { get; set; }


        /// <summary>
        /// We store lat/long in a CosmosDB point to avoid copying objects on read. However,
        /// users of the class should refer only to Latitude and Longitude.
        /// most 
        /// </summary>
        [JsonProperty("location")]
        private Point Location { get; }

        [JsonIgnore]
        public double Latitude { get
            {
                return Location.Position.Latitude;
            }
        }

        [JsonIgnore]
        public double Longitude { get
            {
                return Location.Position.Longitude;
            }
        }

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
                Tags = Tags.Count == 0 ? null : Tags,
                Latitude = Latitude,
                Longitude = Longitude
            };
        }
    }
}
