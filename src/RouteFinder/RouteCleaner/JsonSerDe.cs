using RouteCleaner.Model;

namespace RouteCleaner
{
    /// <summary>
    /// Customized serializer deserializer for OSM data.
    /// </summary>
    public static class JsonSerDe
    {
        public static Geometry GetGeometry(string serialized)
        {
            return null;
        }

        public static string Serialize(Geometry geometry)
        {
            return "";
        }
    }
}
