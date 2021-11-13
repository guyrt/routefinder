using System;
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

        public async Task ProcessAsync(string gpxFilename)
        {
            // get parsed gpx
            var parsedGpx = GpxParser.Parse(gpxFilename);

            // process each track
            foreach (var track in parsedGpx.trk)
            {
                // get range
                var ranges = GpxParser.ComputeBounds(track);
                
                foreach (var code in ranges)
                {
                    cache.LoadSegment(code);
                }

                foreach (var seg in track.trkseg) {
                    var overlappingNodes = pointComparer.FindOverlapping(seg.trkpt);
                    foreach (var node in overlappingNodes)
                    {
                        Console.WriteLine($"Found point {node.Id}: {node.TargetableWays}");
                    }
                }
            }
        }
    }
}
