using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using RouteCleaner.Model;
using RouteCleaner.PolygonUtils;

namespace RouteCleaner.Transformers
{
    public class LabelWaysInRelation
    {

        private bool _debugOut = false;
        private string _debutOutputLocation = "";

        public List<Way> Transform(Relation target, Geometry geometry)
        {
            var polygons = target.Polygons;
            var p = new PolygonTriangulation(polygons.First());
            var triangles = p.Triangulate();
            DebugOut(triangles, "triangles.json");

            var containment = new PolygonContainment(triangles);
            var ways = new List<Way>();
            foreach (var way in geometry.Ways)
            {
                var (contains, notContains) = containment.Containment(way);
                foreach (var newWay in contains)
                {
                    newWay.Tags.Add("rfInPolygon", newWay.FootTraffic() ? "in" : "notfoot");
                }
                foreach (var newWay in notContains)
                {
                    newWay.Tags.Add("rfInPolygon", "out");
                }

                ways.AddRange(contains);
                ways.AddRange(notContains);
            }

            return ways;
        }

        public void DebugOut(LinkedList<Triangle> triangles, string filename)
        {
            if (!_debugOut)
            {
                return;

            }
            var fullPath = Path.Combine(_debutOutputLocation, filename);
            var converter = new GeoJsonConverter();
            var trianglesOut = converter.Convert(triangles);
            var serialized = JsonConvert.SerializeObject(trianglesOut);
            File.WriteAllLines(fullPath, new[] { serialized });
        }
    }
}
