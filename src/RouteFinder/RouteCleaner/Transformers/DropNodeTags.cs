using RouteFinderDataModel;
using System.Linq;

namespace RouteCleaner.Transformers
{
    public class DropNodeTags
    {
        public Geometry Transform(Geometry geometry)
        {
            foreach (var node in geometry.Nodes)
            {
                var dropKeys = node.Tags.Keys.Where(k => k == "power" || k == "source").ToArray();
                foreach (var key in dropKeys)
                {
                    node.Tags.Remove(key);
                }
            }
            return geometry;
        }
    }
}
