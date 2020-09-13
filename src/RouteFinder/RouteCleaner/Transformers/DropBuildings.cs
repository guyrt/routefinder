using RouteFinderDataModel;

namespace RouteCleaner.Transformers
{
    public class DropBuildings
    {
        public Geometry Transform(Geometry geometry)
        {
            return WayFilterWithNodeCleanup.Transform(geometry, w => !(w.Tags.ContainsKey("building") || (w.Tags.ContainsKey("amenity") && w.Tags["amenity"] == "school")));
        }
    }
}
