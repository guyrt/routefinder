using CosmosDBLayer;
using RouteFinderDataModel.Proto;
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
            var overlappingNodes = GetOverlap(parsedGpx, userId, out var plusCodeRanges);

            // update the raw cache
            foreach (var node in overlappingNodes)
            {
                await this.uploadHandler.Upload(node);
            }

            // upload raw run
            var runDetails = this.ConvertGpx(parsedGpx, userId);
            await this.uploadHandler.Upload(runDetails);

            // update stats
            await this.UpdateUserWayCoverage(overlappingNodes, userId, plusCodeRanges);

            // userSummary
            await this.UpdateUserSummaryAsync(userId);

            // update coverage geoJson squares for display

        }

        private HashSet<UserNodeCoverage> GetOverlap(gpxType parsedGpx, Guid userId, out HashSet<string> plusCodeRanges)
        {
            var runTime = parsedGpx.metadata.time;

            var overlappingNodes = new HashSet<UserNodeCoverage>();

            // process each track
            var watch = Stopwatch.StartNew();
            plusCodeRanges = new HashSet<string>();
            foreach (var track in parsedGpx.trk)
            {
                // get range
                var ranges = GpxParser.ComputeBounds(track);
                plusCodeRanges.UnionWith(ranges);

                var tasks = plusCodeRanges.Select(code => cache.LoadSegment(code)).ToArray();
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
                            if (wayLookup == null)
                            {
                                Console.WriteLine($"Failed to find way {targetableWay} for point {point.Id}");
                                continue;
                            }

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

        private async Task UpdateUserWayCoverage(HashSet<UserNodeCoverage> userNodeCoverages, Guid userId, HashSet<string> plusCodeRanges) 
        {
            // Step 0: Get regions/ways affected
            var uniqueWays = userNodeCoverages.Select(x => x.WayId).Distinct().ToHashSet();

            // Step 1: Get all UserNodeCoverages for the affected areas.
            var allNodeCoverages = (await this.uploadHandler.GetAllUserNodeCoverageByWay(userId, uniqueWays.ToArray()));
            var allWaySummaries = (await this.uploadHandler.GetAllUserWayCoverage(userId, uniqueWays.ToArray())).ToDictionary(k => k.WayId, v => v);
            var nodeCoverage = allNodeCoverages.GroupBy(w => w.WayId).ToDictionary(k => k.Key, v => v.Count());

            // Step 2: Get all ways and regions containing those ways from the cache.
            var allWays = new HashSet<LookupTargetableWay>();
            foreach (var range in plusCodeRanges)
            {
                allWays.UnionWith(cache.WayCache[range].Ways.Where(x => uniqueWays.Contains(x.Id)));
            }

            var wayLookup = allWays.ToDictionary(k => k.Id, v => v);
            var localRegions = allWays.Select(x => x.Relation).Distinct().ToArray();
            var wayCoverageCounts = wayLookup.GroupBy(x => x.Value.Id).ToDictionary(group1 => group1.Key, groupV => groupV.Count());

            // Step 4: Create or Update UserWayCoverages
            foreach ((var wayId, var numNodesRun) in nodeCoverage)
            {
                var totalNodes = wayLookup[wayId].OriginalWays.SelectMany(w => w.NodeIds).Distinct().ToHashSet();

                if (!allWaySummaries.ContainsKey(wayId))
                {
                    allWaySummaries.Add(wayId, new UserWayCoverage
                    {
                        UserId = userId,
                        WayId = wayId,
                        WayName = wayLookup[wayId].WayName,
                        RegionId = wayLookup[wayId].Relation,
                        NodeCompletedCount = nodeCoverage[wayId],
                        NumNodesInWay = numNodesRun,
                    });
                }
                else
                {
                    allWaySummaries[wayId].NodeCompletedCount = nodeCoverage[wayId];
                    allWaySummaries[wayId].NumNodesInWay = numNodesRun;
                }
            }

            // Step 5: updates
            foreach (var wayId in nodeCoverage.Keys)
            {
                await this.uploadHandler.Upload(allWaySummaries[wayId]);
            }
        }

        private async Task UpdateUserSummaryAsync(Guid userId)
        {
            return;
        }
    }
}
