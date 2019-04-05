using System.Linq;
using RouteCleaner.Model;

namespace RouteCleaner.Transformers
{
    public class DropWater
    {
        public Geometry Transform(Geometry geometry)
        {
            var ways = geometry.Ways.Where(w => !w.Tags.ContainsKey("waterway"));
            // Assumption: buildings aren't in relations.
            return new Geometry(geometry.Nodes, ways.ToArray(), geometry.Relations);
        }
    }
}
