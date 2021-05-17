namespace RouteCleaner.Transformers
{
    using RouteFinderDataModel;
    using System.Linq;

    public class DropMunicipalBoundaries 
    {
        public Geometry Transform(Geometry geometry)
        {
            geometry = WayFilterWithNodeCleanup.Transform(geometry, w => !(w.Tags.ContainsKey("boundary") && w.Tags["boundary"] == "administrative"));
            var newRelations = geometry.Relations.Where(w => !(w.Tags.ContainsKey("boundary") && w.Tags["boundary"] == "administrative"));
            return new Geometry(geometry.Nodes, geometry.Ways, newRelations.ToArray());
        }
    }
}
