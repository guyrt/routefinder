using OpenStreetMapEtl.Utils;

using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenStreetMapEtl.Storage
{
    public class CacheFileLookup
    {
        public BoundingBox[] BoundingBoxes { get; }

        private CacheFileLookup(BoundingBox[] sortedBoxes)
        {
            BoundingBoxes = sortedBoxes;
        }

        /// <summary>
        /// todo: improve from n^2 using fact that we have a sorted list.
        /// </summary>
        /// <param name="bbox"></param>
        /// <returns></returns>
        public List<BoundingBox> BoxIntersection(BoundingBox bbox)
        {
            return BoundingBoxes
                .Where(box => box.Overlap(bbox))
                .ToList();
        }

        public static CacheFileLookup Create(BoundingBox[] bboxes)
        {
            Array.Sort(bboxes);
            var newLookup = new CacheFileLookup(bboxes);
            return newLookup;
        }
    }
}
