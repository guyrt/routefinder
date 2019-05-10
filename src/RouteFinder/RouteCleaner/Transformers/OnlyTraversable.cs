using System;
using RouteCleaner.Model;

namespace RouteCleaner.Transformers
{
    public class OnlyTraversable
    {
        private readonly bool _reverse;

        public OnlyTraversable(bool reverse = false)
        {
            _reverse = reverse;
        }

        public Geometry Transform(Geometry geometry)
        {
            Func<Way, bool> f = w => (w.FootTraffic() || w.Tags.ContainsKey("highway") || w.IsParkingLot()) && !(w.Tags.ContainsKey("service") && w.Tags["service"] == "parking_aisle") || w.Id == "42108700";
            var g = _reverse ? w => !f(w) : f;
            return WayFilterWithNodeCleanup.Transform(geometry, g);
        }
    }
}
