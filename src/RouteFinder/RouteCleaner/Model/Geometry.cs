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

    }
}
