﻿namespace RouteFinderDataModel
{
    using Newtonsoft.Json;
    using RouteFinderDataModel.Thin;
    using System.Collections.Generic;
    using System.Linq;

    public class Relation : TaggableIdentifiableElement
    {

        [JsonIgnore]
        public List<Relation> InternalRelations; // list of Relations that are fully contained inside this Relation.

        [JsonIgnore]
        public List<Relation> OverlappingRelations; // overlapping but not proper contained.

        public Relation(string id, Way[] ways, HashSet<string> innerWays, Dictionary<string, string> tags, bool incomplete) : base(id, tags)
        {
            Ways = ways;
            Incomplete = incomplete;
            InternalRelations = new List<Relation>();
            OverlappingRelations = new List<Relation>();
            InnerWays = innerWays;
        }

        public Way[] Ways { get; }

        public HashSet<string> InnerWays { get; }

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

        public string Name
        {
            get
            {
                var defaultName = "Unnamed relation";
                if (this.Tags.TryGetValue("name", out var wayName))
                {
                    return string.IsNullOrEmpty(wayName) ? defaultName : wayName;
                }
                else
                {
                    return defaultName;
                }
            }
        }

        public override string ToString()
        {
            return $"https://www.openstreetmap.org/relation/{Id}";
        }
    }
}
