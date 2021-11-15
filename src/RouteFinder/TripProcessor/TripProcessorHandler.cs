using CosmosDBLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TripProcessor.GpxData;
using UserDataModel;

namespace TripProcessor
{
    /// <summary>
    /// Main driver for a trip processing.
    /// 
    /// This is temporary and should be replaced with DTF orchestrator to processed GPX then fan out.
    /// </summary>
    public class TripProcessorHandler
    {
        private static readonly RunnableWayCache cache = new();

        private PointComparer pointComparer { get; init; }

        private UploadHandler uploadHandler { get; init; }

        public TripProcessorHandler(UploadHandler uploadHandler)
        {
            pointComparer = new PointComparer(cache);
            this.uploadHandler = uploadHandler;
         }

        // this should fire off a bunch of DTF actions.
        public void Process(string gpxFilename, Guid userId)
        {

            var parsedGpx = GpxParser.Parse(gpxFilename);
            
            // get parsed gpx
            var overlappingNodes = GetOverlap(parsedGpx, userId);

            // update the raw cache


            // update stats

        }

        private HashSet<UserNodeCoverage> GetOverlap(gpxType parsedGpx, Guid userId)
        {
            var runTime = parsedGpx.metadata.time;

            var overlappingNodes = new HashSet<UserNodeCoverage>();

            // process each track
            foreach (var track in parsedGpx.trk)
            {
                // get range
                var ranges = GpxParser.ComputeBounds(track);

                var tasks = ranges.Select(code => cache.LoadSegment(code)).ToArray();
                Task.WaitAll(tasks);

                foreach (var seg in track.trkseg)
                {
                    foreach (var point in pointComparer.FindOverlapping(seg.trkpt))
                    {
                        foreach (var targetableWay in point.TargetableWays)
                        {
                            var location = GpxParser.GetLocationCode(point.Latitude, point.Longitude);
                            var ways = cache.WayCache[location].Ways;
                            var region = ways.SingleOrDefault(x => x.Id == targetableWay);
                            overlappingNodes.Add(new UserNodeCoverage
                            {
                                UserId = userId,
                                NodeId = point.Id,
                                RegionId = region.Id,
                                WayId = targetableWay,
                                FirstRan = runTime,
                            });
                        }
                    }
                }

            }

            return overlappingNodes;
        }
    }
}
