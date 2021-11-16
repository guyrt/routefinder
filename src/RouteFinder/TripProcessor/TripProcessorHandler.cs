using CosmosDBLayer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public async Task Process(string gpxFilename, Guid userId)
        {

            var parsedGpx = GpxParser.Parse(gpxFilename);
            
            // get parsed gpx
            var overlappingNodes = GetOverlap(parsedGpx, userId);

            // update the raw cache
            foreach (var node in overlappingNodes)
            {
                await this.uploadHandler.Upload(node);
            }

            // upload raw run
            var runDetails = this.ConvertGpx(parsedGpx, userId);
            await this.uploadHandler.Upload(runDetails);

            // update stats
            // userWayCoverage
            // userSummary

        }

        private HashSet<UserNodeCoverage> GetOverlap(gpxType parsedGpx, Guid userId)
        {
            var runTime = parsedGpx.metadata.time;

            var overlappingNodes = new HashSet<UserNodeCoverage>();

            // process each track
            var watch = Stopwatch.StartNew();
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
                            var wayLookup = ways.SingleOrDefault(x => x.Id == targetableWay);
                            overlappingNodes.Add(new UserNodeCoverage
                            {
                                UserId = userId,
                                NodeId = point.Id,
                                RegionId = wayLookup.Relation,
                                WayId = targetableWay,
                                FirstRan = runTime,
                            });
                        }
                    }
                }
            }
            var runtime = watch.ElapsedMilliseconds;
            Console.WriteLine($"Completed {overlappingNodes.Count} in {runtime} milliseconds.");

            return overlappingNodes;
        }

        private RunDetails ConvertGpx(gpxType parsedGpx, Guid userId)
        {
            var runTime = parsedGpx.metadata.time;
            var runDetails = new RunDetails
            {
                UserId = userId,
                Name = parsedGpx.trk.First().name,
                Timestamp = parsedGpx.metadata.time,
                Id = $"{userId}_{runTime.Ticks}",
                Route = parsedGpx.trk.SelectMany(t => t.trkseg).Select(s => s.trkpt.Select(p => new RunDetails.RunPoint
                {
                    Latitude = p.lat,
                    Longitude = p.lon,
                    Elevation = p.ele,
                }).ToArray()).ToArray(),
            };
            return runDetails;
        }
    }
}
