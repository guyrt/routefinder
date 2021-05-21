using System.Collections.Generic;
using System.Linq;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using RouteCleaner.PolygonUtils;
using RouteFinderDataModel;
using Polygon = RouteCleaner.PolygonUtils.Polygon;

namespace RouteCleaner
{
    public class GeoJsonConverter
    {
        public Point ConvertSimple(Node n)
        {
            return new Point(new Position(n.Latitude, n.Longitude));
        }

        public Feature Convert(Way w)
        {
            var ls = ConvertSimple(w);
            Dictionary<string, object> d = w.Tags.ToDictionary(k => k.Key, v => (object) v.Value);
            if (d.ContainsKey("name") && !d.ContainsKey("title"))
            {
                d.Add("title", d["name"]);
            }
            var f = new Feature(ls, d, w.Id);
            return f;
        }

        public IGeometryObject ConvertSimple(Way w)
        {
            if (w.Nodes.Length < 2)
            {
                return ConvertSimple(w.Nodes[0]);
            }
            return ConvertSimple(w.Nodes);
        }

        public Feature Convert(IEnumerable<Node> nodes)
        {
            return new Feature(ConvertSimple(nodes));
        }

        public LineString ConvertSimple(IEnumerable<Node> nodes, bool asPolygon = false)
        {
            IEnumerable<double[]> coords = nodes.Select(n => new[] {n.Longitude, n.Latitude});
            if (asPolygon)
            {
                coords = coords.Append(coords.First());
            }
            return new LineString(coords.ToArray());
        }

        public FeatureCollection Convert(Relation r)
        {
            return ConvertSimple(r);
        }

        public FeatureCollection ConvertSimple(Relation r)
        {
            return new FeatureCollection(r.Ways.Select(Convert).ToList());
        }

        public Feature Convert(Polygon p)
        {
             return new Feature(ConvertSimple(p));
        }

        public GeoJSON.Net.Geometry.Polygon ConvertSimple(Polygon p)
        {
            var lineString = ConvertSimple(p.Nodes, true);
            return new GeoJSON.Net.Geometry.Polygon(new []{lineString});
        }

        public FeatureCollection Convert(IEnumerable<Way> ways)
        {
            return new FeatureCollection(ways.Select(Convert).ToList());
        }
    }
}
