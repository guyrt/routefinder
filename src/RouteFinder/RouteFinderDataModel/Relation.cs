using RouteFinderDataModel.Thin;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RouteFinderDataModel
{
    public class Relation : TaggableIdentifiableElement
    {

        public Relation(string id, Way[] ways, Dictionary<string, string> tags, bool incomplete) : base(id, tags)
        {
            Ways = ways;
            Incomplete = incomplete;
        }

        public Way[] Ways { get; }

        public bool Incomplete { get; }

        public ThinRelation ToThin()
        {
            return new ThinRelation
            {
                Id = Id,
                Ways = Ways.Select(w => w.Id).ToArray(),
                Tags = Tags.Count == 0 ? null : Tags
            };
        }
    }
}
