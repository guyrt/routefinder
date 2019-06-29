using System;
using System.Collections.Generic;
using System.Linq;

namespace RouteCleaner.Model
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

        private Polygon[] _polygons;


        /// <summary>
        /// Uses memoization.
        /// </summary>
        public Polygon[] Polygons => _polygons ?? (_polygons = BuildPolygons());

        private Polygon[] BuildPolygons()
        {
            if (!Tags.TryGetValue("type", out var foo) || foo != "multipolygon")
            {
                return new Polygon[0];
            }
            try
            {
                return Polygon.BuildPolygons(Ways).ToArray();
            }
            catch (InvalidOperationException)
            {
                return new Polygon[0];
            }
        }

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
