using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RouteCleaner.Model;
using System.Collections.Generic;

namespace RouteCleaner
{
    /// <summary>
    /// Customized serializer deserializer for OSM data.
    /// </summary>
    public static class JsonSerDe
    {
        public static Geometry GetGeometry(string serialized)
        {
            var thinGeometry = JsonConvert.DeserializeObject<ThinGeometry>(serialized);
            return thinGeometry.ToThick();
        }

        public static string Serialize(Geometry geometry)
        {
            return JsonConvert.SerializeObject(geometry.ToThin(), 
                Formatting.None,
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                }
            );
        }
    }
}
