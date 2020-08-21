using OpenStreetMapEtl.Azure;
using OpenStreetMapEtl.Utils;
using RouteCleaner;
using RouteCleaner.Filters;
using RouteCleaner.Model;
using System;
using System.Linq;
using System.Threading;

namespace OpenStreetMapEtl
{
    /// <summary>
    /// Orchestrate the download and cleaning process.
    /// </summary>
    public class DownloaderGeneral
    {
        private readonly double _kmSize = 16;

        private readonly IRangeDownloader _downloader = new OsmDownloader();

        private readonly DetailedDebugOutputter _debugger = new DetailedDebugOutputter(@"\Users\riguy\Documents\Github\routefinder\src\RouteViewerWeb\data");

        public DownloaderGeneral(IRangeDownloader downloader)
        {
            _downloader = downloader;
        }

        public void Run(BoundingBox bbox)
        {
            var boxes = new BuildBoundingBoxes(_kmSize).GetBoundingBoxes(bbox).ToList();
            var first = true;
            foreach (var box in boxes)
            {
                if (!first)
                {
                    // do one pull every four minutes.
                    Thread.Sleep(1000 * 60 * 4);
                }
                first = false;
                var geometry = RunSingleSquare(box);
                SaveGeometry(geometry, box);
            }
        }

        public void SaveGeometry(Geometry geometry, BoundingBox box)
        {
            var uploader = new BlobUpload();
            var serialized = JsonSerDe.Serialize(geometry);
            uploader.Upload(serialized, box);
           // _debugger.DumpString(serialized, "fullOutput.json");
        }

        public Geometry RunSingleSquare(BoundingBox box)
        {
            using (var downloadedFile = _downloader.GetRange(box))
            {
                var cleaner = new ParseAndCleanOsm();
                var geometry = cleaner.ReadAndClean(downloadedFile.TmpFile);
                //var parkingLots = new PathParkingLotIntersection().FindParkingLotsWithIntersections(geometry);
                
                //_debugger.OutputWays(parkingLots.Keys, "parkingLots.json");
                return geometry;
            }
        }
    }
}
