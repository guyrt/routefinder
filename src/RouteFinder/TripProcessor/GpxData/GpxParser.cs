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

        public static string GetLocationCode(wptType point)
        {
            return OpenLocationCode.Encode(Convert.ToDouble(point.lat), Convert.ToDouble(point.lon), codeLength: 6);
        }
    }
}
