using RouteFinderDataModel;

namespace RouteCleaner.Transformers
{
    public class DropParkingAisle
    {
        public Geometry Transform(Geometry geometry)
        {
            return WayFilterWithNodeCleanup.Transform(geometry, w => !w.IsParkingAisle());
        }

    }
}