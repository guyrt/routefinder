using System.Linq;
using RouteCleaner.Model;

namespace RouteCleaner.Transformers
{
    public class DropBuildings
    {
        public Geometry Transform(Geometry geometry)
        {
            var ways = geometry.Ways.Where(w => !w.Tags.ContainsKey("building") || w.Tags["building"] != "yes");
            // Assumption: buildings aren't in relations.
            return new Geometry(geometry.Nodes, ways.ToArray(), geometry.Relations);
        }
    }
}
