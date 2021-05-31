using RouteFinderDataModel.Thin;

namespace RouteFinderDataModel
{
    /// <summary>
    /// An internal representation of a Relation based on a Polygon
    /// </summary>
    public class TargetableRelation
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string RelationType { get; set; } // e.g. park, city, state, country

        /// <summary>
        /// Store each polygon
        /// </summary>
        public ThinNode[][] Borders { get; set; }
    }
}
