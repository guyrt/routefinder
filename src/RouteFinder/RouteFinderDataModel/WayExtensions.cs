namespace RouteFinderDataModel
{
    using System.Linq;
    using RouteFinderDataModel.Thin;

    public static class WayExtensions
    {
        public static bool FootTraffic(this Way way)
        {
            return (way.Tags.ContainsKey("foot") && way.Tags["foot"] == "designated") ||
                   (way.Tags.ContainsKey("highway") && (way.Tags["highway"].Contains("foot") || way.Tags["highway"].Contains("path") || way.Tags["highway"].Contains("track")));
        }

        public static bool IsParkingLot(this Way way)
        {
            return way.Tags.ContainsKey("amenity") && way.Tags["amenity"] == "parking";
        }

        public static bool IsParkingAisle(this Way way)
        {
            return way.Tags.ContainsKey("service") && way.Tags["service"] == "parking_aisle";
        }

        public static bool MustHit(this Way way) {
            return way.Tags.ContainsKey("rfInPolygon") && way.Tags["rfInPolygon"] == "in";
        }

        public static string Name(this Way way) {
            return way.Tags.ContainsKey("name") ? way.Tags["name"] : way.ToString();
        }

        public static ThinWay ToThin(this Way way)
        {
            return new ThinWay
            {
                Id = way.Id,
                Nodes = way.Nodes.Select(n => n.Id).ToArray(),
                Tags = way.Tags.Count == 0 ? null : way.Tags
            };
        }

    }
}
