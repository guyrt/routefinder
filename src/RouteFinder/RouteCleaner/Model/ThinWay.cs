using System.Collections.Generic;
using System.Linq;

namespace RouteCleaner.Model
{
    public class ThinWay
    {

        public string Id { get; set; }

        public Dictionary<string, string> Tags { get; set; }

        public string[] Nodes { get; set; }

        public Way ToThick(Dictionary<string, Node> nodeLookup)
        {
            return new Way(Id, Nodes.Select(n => nodeLookup[n]).ToArray(), Tags);
        }
    }
}
