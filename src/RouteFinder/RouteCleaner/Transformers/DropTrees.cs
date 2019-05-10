using RouteCleaner.Model;
using System.Linq;

namespace RouteCleaner.Transformers
{
    public class DropTrees
    {
        public Geometry Transform(Geometry geometry)
        {
            var nodes = geometry.Nodes.Where(n => !(n.Tags.ContainsKey("natural") && n.Tags["natural"] == "tree"));
            return new Geometry(nodes.ToArray(), geometry.Ways, geometry.Relations);
        }
    }
}
