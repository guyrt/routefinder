using RouteFinderDataModel.Proto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TripProcessor.GpxData;

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

        public TripProcessorHandler()
        {
            pointComparer = new PointComparer(cache);
        }

        public void Process(string gpxFilename)
        {
            // get parsed gpx
            var parsedGpx = GpxParser.Parse(gpxFilename);

            // process each track
            foreach (var track in parsedGpx.trk)
            {
                // get range
                var ranges = GpxParser.ComputeBounds(track);

                var tasks = ranges.Select(code => cache.LoadSegment(code)).ToArray();
                Task.WaitAll(tasks);

                var overlappingNodes = new HashSet<LookupNode>();

                foreach (var seg in track.trkseg) {
                    overlappingNodes.UnionWith(pointComparer.FindOverlapping(seg.trkpt));
                }

                // invert, then look up ways and mark:
                // nodes as done somewhere
                // ways as done


                var uniqueWays = overlappingNodes.SelectMany(x => x.TargetableWays).Distinct();
                
                foreach (var node in uniqueWays)
                {
                    Console.WriteLine($"Found way {node}");
                }
            }
        }
    }
}
