using System;
using System.Collections.Generic;
using GlobalSettings;
using RouteCleaner;
using RouteFinderDataModel.Proto;
using TripProcessor.GpxData;

namespace TripProcessor
{
    public class PointComparer
    {
        private RunnableWayCache RunnableWayCache { get; init; }

        public PointComparer(RunnableWayCache cache)
        {
            RunnableWayCache = cache;
        }

        /// <summary>
        /// Find overlapping points between the input point and results.
        /// 
        /// Todo - improve this. It's O(n**2) but could improve a lot by using node sort info.
        /// Next step would be an index in file which could be fun.
        /// </summary>
        /// <param name="point"></param>
        public HashSet<LookupNode> FindOverlapping(wptType[] points)
        {
            var overlappingNodes = new HashSet<LookupNode>();
            
            foreach (var point in points)
            {
                var pointLat = Convert.ToDouble(point.lat);
                var pointLng = Convert.ToDouble(point.lon);
                var lookupKey = GpxParser.GetLocationCode(point);
                var segment = this.RunnableWayCache.Cache[lookupKey];

                foreach (var k in segment.Nodes)
                {
                    if (IsNear(pointLat, pointLng, k))
                    {
                        overlappingNodes.Add(k);
                    }
                }
            }

            return overlappingNodes;
        }

        private static bool IsNear(double lat, double lng, LookupNode point)
        {
            // note the unit conversion: km to m
            return SimpleDistance.Compute(lat, point.Latitude, lng, point.Longitude) * 1000 < TripProcessorSettings.GetInstance().OverlapThresholdMeters;
        }
    }
}
