using System.Collections.Generic;
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

        public Relation Relation { get; set; }

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

            if (this.Relation != null && otherWay.Relation != null && this.Relation != otherWay.Relation)
            {
                throw new System.Exception($"Cannot merge TargetableWays - relations are non null but different: {this.Relation} and {otherWay.Relation}");
            }

            this.OriginalWays.AddRange(otherWay.OriginalWays);

            this.Relation = this.Relation ?? otherWay.Relation;
        }
    }
}
