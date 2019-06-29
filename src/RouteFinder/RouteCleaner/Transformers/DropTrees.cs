using RouteCleaner.Model;
using System.Linq;

namespace RouteCleaner.Transformers
{
    public class DropTrees
    {
        public Geometry Transform(Geometry geometry)
        {
            // handle some cases where a user made a tree part of a way.
            var wayNodes = geometry.Ways.SelectMany(w => w.Nodes).Distinct().ToHashSet();
            var nodes = geometry.Nodes.Where(n => wayNodes.Contains(n) || !(n.Tags.ContainsKey("natural") && n.Tags["natural"] == "tree"));
            return new Geometry(nodes.ToArray(), geometry.Ways, geometry.Relations);
        }
    }
}
