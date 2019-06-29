using System.Collections.Generic;
using System.Linq;

namespace RouteCleaner.Model
{
    public class Way : TaggableIdentifiableElement
    {
        public Way(string id, Node[] nodes, Dictionary<string, string> tags = null) : base(id, tags)
        {
            Nodes = nodes;
        }

        public Node[] Nodes { get; }

        public bool FootTraffic()
        {
            return (Tags.ContainsKey("foot") && Tags["foot"] == "designated") ||
                   (Tags.ContainsKey("highway") && (Tags["highway"].Contains("foot") || Tags["highway"].Contains("path") || Tags["highway"].Contains("track")));
        }

        public bool IsParkingLot()
        {
            return Tags.ContainsKey("amenity") && Tags["amenity"] == "parking";
        }

        public bool IsParkingAisle()
        {
            return Tags.ContainsKey("service") && Tags["service"] == "parking_aisle";
        }

        public bool MustHit => Tags.ContainsKey("rfInPolygon") && Tags["rfInPolygon"] == "in";

        public string Name => Tags.ContainsKey("name") ? Tags["name"] : ToString();

        public override string ToString()
        {
            return $"https://www.openstreetmap.org/way/{Id}";
        }

        public ThinWay ToThin()
        {
            return new ThinWay
            {
                Id = Id,
                Nodes = Nodes.Select(n => n.Id).ToArray(),
                Tags = Tags.Count == 0 ? null : Tags
            };
        }
    }
}
