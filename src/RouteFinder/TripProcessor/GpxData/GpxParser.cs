using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

using Google.OpenLocationCode;


namespace TripProcessor.GpxData
{
    public class GpxParser
    {
        public static gpxType Parse(string gpxFilename)
        {
            using var reader = XmlReader.Create(gpxFilename);
            var serializer = new XmlSerializer(typeof(gpxType));
            var gpxTrace = (gpxType)serializer.Deserialize(reader);
            return gpxTrace;
        }

        /// <summary>
        /// Return set of all unique 6-digit plus codes that intersect a track.
        /// </summary>
        /// <param name="track"></param>
        /// <returns></returns>
        public static HashSet<string> ComputeBounds(trkType track)
        {
            var codes = new HashSet<string>();
            foreach (var seg in track.trkseg) 
            {
                foreach (var point in seg.trkpt)
                {
                    codes.Add(GetLocationCode(point));
                }
            }
            return codes;

        }

        /// <summary>
        /// todo - both of these fxns should return lists and should jitter the point by a fixed amount determined by the accuracy to account for points that are right on an edge.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static string GetLocationCode(wptType point)
        {
            return GetLocationCode(Convert.ToDouble(point.lat), Convert.ToDouble(point.lon));
        }

        public static string GetLocationCode(double latitude, double longitude)
        {
            return OpenLocationCode.Encode(latitude, longitude, codeLength: 6);
        }
    }
}
