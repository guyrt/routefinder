using System.Linq;

namespace RouteCleaner.Model
{
    public class Geometry
    {

        public Geometry(Node[] nodes, Way[] ways, Relation[] relations)
        {
            Nodes = nodes;
            Ways = ways;
            Relations = relations;
        }

        public Way[] Ways { get; }

        public Node[] Nodes { get; }

        public Relation[] Relations { get; }

        public ThinGeometry ToThin()
        {
            return new ThinGeometry
            {
                Nodes = Nodes.Select(n => n.ToThin()).ToArray(),
                Ways = Ways.Select(w => w.ToThin()).ToArray(),
                Relations = Relations.Select(r => r.ToThin()).ToArray()
            };
        }
    }
}
