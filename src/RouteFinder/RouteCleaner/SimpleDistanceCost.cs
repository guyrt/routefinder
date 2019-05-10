using System;
using RouteCleaner.Model;

namespace RouteCleaner
{
    public static class SimpleDistanceCost
    {
        /// <summary>
        /// Computes Distance in kilometers.
        /// </summary>
        /// <param name="n1"></param>
        /// <param name="n2"></param>
        /// <returns></returns>
        public static double Compute(Node n1, Node n2)
        {
            var lat2 = n2.Latitude;
            var lat1 = n1.Latitude;
            var lon2 = n2.Longitude;
            var lon1 = n1.Longitude;
            return Compute(lat1, lat2, lon1, lon2);
        }

        public static double Compute(double lat1, double lat2, double lon1, double lon2)
        {
            var R = 6371; // Radius of the earth in km
            var dLat = Deg2Rad(lat2 - lat1);  // deg2rad below
            var dLon = Deg2Rad(lon2 - lon1);
            var a =
                    Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(Deg2Rad(lat1)) * Math.Cos(Deg2Rad(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2)
                ;
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var d = R * c; // Distance in km
            return d;
        }

        private static double Deg2Rad(double deg) => deg * (Math.PI / 180);
    }
}
