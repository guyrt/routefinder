using System.Collections.Generic;
using Newtonsoft.Json;
using RouteFinderDataModel.Thin;

namespace RouteFinderDataModel
{
    /// <summary>
    /// A runnable way within a single Relation.
    /// </summary>
    public class TargetableWay
    {
        public string Id { get; set; }

        public string Name { get; set; }

        [JsonIgnore]
        public Relation FullRelation { get; set; }

        public string RelationId => FullRelation?.Id;

        public string RelationName => FullRelation?.Name ?? string.Empty;

        public List<OriginalWay> OriginalWays { get; set; }

        public class OriginalWay
        {
            public string Id { get; set; }

            public ThinNode[] Points { get; set; }
        }

        public void Merge(TargetableWay otherWay)
        {
            if (this.Name != otherWay.Name)
            {
                throw new System.Exception($"Cannot merge TargetableWays - names {this.Name} and {otherWay.Name} differ.");
            }

            if (this.FullRelation != null && otherWay.FullRelation != null && this.FullRelation != otherWay.FullRelation)
            {
                throw new System.Exception($"Cannot merge TargetableWays - relations are non null but different: {this.FullRelation} and {otherWay.FullRelation}");
            }

            this.OriginalWays.AddRange(otherWay.OriginalWays);

            this.FullRelation = this.FullRelation ?? otherWay.FullRelation;
        }
    }
}
