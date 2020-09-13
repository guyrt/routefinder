namespace RouteCleaner.Transformers
{
    using RouteFinderDataModel;
    public class DropUnderground
    {
        public Geometry Transform(Geometry geometry)
        {
            return WayFilterWithNodeCleanup.Transform(geometry, w => !(w.Tags.ContainsKey("location") && w.Tags["location"] == "underground"));
        }
    }
}
