using System.Collections.Generic;
using RouteFinderDataModel.Thin;

namespace RouteFinderDataModel
{
    /// <summary>
    /// A runnable way within a single regional Relation.
    /// 
    /// There should be only one possible TargetableWay per Region/Way
    /// </summary>
    public class TargetableWay
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string RegionId { get; set; }

        public string RegionName { get; set; }

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

            if (this.RegionId != null && otherWay.RegionId != null && this.RegionId != otherWay.RegionId)
            {
                throw new System.Exception($"Cannot merge TargetableWays - relations are non null but different: {this.RegionId} and {otherWay.RegionId}");
            }

            this.OriginalWays.AddRange(otherWay.OriginalWays);

            this.RegionId = this.RegionId ?? otherWay.RegionId;
            this.RegionName = this.RegionName ?? otherWay.RegionName;
        }
    }
}
