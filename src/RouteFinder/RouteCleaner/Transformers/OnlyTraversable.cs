using System.Linq;
using RouteCleaner.Model;

namespace RouteCleaner.Transformers
{
    public class OnlyTraversable
    {
        public Geometry Transform(Geometry geometry)
        {
            var ways = geometry.Ways.Where(w => w.FootTraffic() || w.Tags.ContainsKey("highway") || w.IsParkingLot());
            ways = ways.Where(w => !(w.Tags.ContainsKey("service") && w.Tags["service"] == "parking_aisle") || w.Id == "42108700");
            // Assumption: buildings aren't in relations.
            return new Geometry(geometry.Nodes, ways.ToArray(), geometry.Relations);
        }
    }
}
