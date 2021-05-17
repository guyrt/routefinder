using System.Collections.Generic;
using System.Linq;

namespace RouteFinderDataModel.Thin
{
    public class ThinRelation
    {
        public string[] Ways { get; set; }

        public string Id { get; set; }

        public Dictionary<string, string> Tags { get; set; }

        public Relation ToThick(Dictionary<string, Way> wayLookup)
        {
            return new Relation(Id, Ways.Where(w => wayLookup.ContainsKey(w)).Select(w => wayLookup[w]).ToArray(), Tags, false);
        }
    }
}