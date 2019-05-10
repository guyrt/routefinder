using RouteCleaner.Model;
using System;

namespace RouteCleaner.Transformers
{
    public class DropDriveways
    {
        public Geometry Transform(Geometry geometry) {
            Func<Way, bool> f = w => !(w.Tags.ContainsKey("service") && w.Tags["service"] == "driveway" && w.Tags.ContainsKey("access") && w.Tags["access"] == "private");
            return WayFilterWithNodeCleanup.Transform(geometry, f);
        }

    }
}
