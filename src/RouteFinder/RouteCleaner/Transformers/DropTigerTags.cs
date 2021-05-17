﻿namespace RouteCleaner.Transformers
{
    using RouteFinderDataModel;
    using System.Linq;

    public class DropTigerTags
    {
        public Geometry Transform(Geometry geometry)
        {
            foreach(var way in geometry.Ways)
            {
                var dropKeys = way.Tags.Keys.Where(k => k.StartsWith("tiger:")).ToArray();
                foreach (var key in dropKeys)
                {
                    way.Tags.Remove(key);
                }
            }
            return geometry;
        }
    }
}