using System.Collections.Generic;

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
                   (Tags.ContainsKey("highway") && (Tags["highway"].Contains("foot") || Tags["highway"].Contains("path")));
        }

        public bool IsParkingLot()
        {
            return Tags.ContainsKey("amenity") && Tags["amenity"] == "parking";
        }

        public bool MustHit => Tags.ContainsKey("rfInPolygon") && Tags["rfInPolygon"] == "in";

        public string Name => Tags.ContainsKey("name") ? Tags["name"] : ToString();

        public override string ToString()
        {
            return $"https://www.openstreetmap.org/way/{Id}";
        }
    }
}
