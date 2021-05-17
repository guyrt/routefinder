using OpenStreetMapEtl.Utils;
using System.IO;

namespace OpenStreetMapEtl.Azure
{
    static class BoundingBoxFilenameConverter
    {
        public static string CreateFileName(BoundingBox box, string suffix)
        {
            var fileName = $"{box.SouthLatitude}_{box.NorthLatitude}__{box.WestLongitude}_{box.EastLongitude}.{suffix}";
            return $"{(int)box.SouthLatitude}/{(int)box.WestLongitude}/{fileName}";
        }

        public static BoundingBox ParseFileName(string rawFilename)
        {
            rawFilename = Path.GetFileNameWithoutExtension(rawFilename);
            var latLng = rawFilename.Split("__");
            var lat = latLng[0].Split("_");
            var lng = latLng[1].Split("_");

            return new BoundingBox
            {
                NorthLatitude = double.Parse(lat[1]),
                SouthLatitude = double.Parse(lat[0]),
                EastLongitude = double.Parse(lng[1]),
                WestLongitude = double.Parse(lng[0])
            };
        }
    }
}
