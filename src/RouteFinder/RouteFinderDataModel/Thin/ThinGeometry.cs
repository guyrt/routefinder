namespace RouteFinderDataModel.Thin
{
    using System.Linq;

    public class ThinGeometry
    {
        public ThinWay[] Ways { get; set; }

        public ThinRelation[] Relations { get; set; }

        public ThinNode[] Nodes { get; set; }

        public Geometry ToThick()
        {
            var nodeLookup = Nodes.ToDictionary(n => n.Id, n => n.ToThick());
            var wayLookup = Ways.ToDictionary(w => w.Id, w => w.ToThick(nodeLookup));
            var relations = Relations.Select(r => r.ToThick(wayLookup));
            return new Geometry(nodeLookup.Values.ToArray(), wayLookup.Values.ToArray(), relations.ToArray());
        }
    }
}
