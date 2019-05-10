using RouteCleaner.Model;

namespace RouteCleaner.Transformers
{
    public class DropUnderground
    {
        public Geometry Transform(Geometry geometry)
        {
            return WayFilterWithNodeCleanup.Transform(geometry, w => !(w.Tags.ContainsKey("location") && w.Tags["location"] == "underground"));
        }
    }
}
