using OpenStreetMapEtl.Azure;
using OpenStreetMapEtl.Storage;
using OpenStreetMapEtl.Utils;
using RouteCleaner;
using RouteCleaner.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenStreetMapEtl
{
    public class AreaCacheDownload
    {
        private static AreaCacheDownload _instance;

        private readonly CacheFileLookup _cacheFileLookup;

        public IFileCache FileCache { get; }

        public Geometry GetRegion(double latitude, double longitude, double sizeMeters = 16 * 1000)
        {
            SimpleDistanceCost.ComputeDeltas(latitude, sizeMeters / 2, out var deltaLat, out var deltaLng);
            var box = new BoundingBox
            {
                NorthLatitude = latitude + deltaLat,
                SouthLatitude = latitude - deltaLat,
                WestLongitude = longitude - deltaLng,
                EastLongitude = longitude + deltaLng
            };
            return GetRegion(box);
        }

        public Geometry GetRegion(BoundingBox region)
        {
            var intersectingFiles = _cacheFileLookup.BoxIntersection(region);
            var nodes = new HashSet<Node>();
            var ways = new HashSet<Way>();
            var relations = new HashSet<Relation>();
            foreach (var box in intersectingFiles)
            {
                var geometry = FileCache.GetBox(box);
                nodes.UnionWith(geometry.Nodes);
                ways.UnionWith(geometry.Ways);
                relations.UnionWith(geometry.Relations);
            }
            return new Geometry(nodes.ToArray(), ways.ToArray(), relations.ToArray());
        }

        private AreaCacheDownload(CacheFileLookup cacheFileLookup, IFileCache fileCache)
        {
            _cacheFileLookup = cacheFileLookup;
            FileCache = fileCache;
        }

        public static AreaCacheDownload Create(IFileCache fileCache)
        {
            if (_instance != null)
            {
                if (_instance.FileCache.GetType() != fileCache.GetType())
                {
                    throw new ArgumentException($"Already initialized with fileCache type {fileCache}");
                }
                return _instance;
            }
            var cacheLookup = CacheFileLookup.Create(new AzureFileCache().ListBoxes());

            return new AreaCacheDownload(cacheLookup, fileCache);
        }
    }
}
