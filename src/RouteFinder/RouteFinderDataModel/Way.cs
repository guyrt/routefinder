namespace RouteFinderDataModel
{
    using System.Collections.Generic;

    public class Way : TaggableIdentifiableElement
    {
        public Way(string id, Node[] nodes, Dictionary<string, string> tags = null) : base(id, tags)
        {
            Nodes = nodes;
        }

        public Node[] Nodes { get; }


        public override string ToString()
        {
            return $"https://www.openstreetmap.org/way/{Id}";
        }

    }
}
