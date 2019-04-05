using System;
using RouteCleaner.Model;

namespace RouteCleaner
{
    public class SimpleDistanceCost
    {
        /// <summary>
        /// Computes Distance in kilometers.
        /// </summary>
        /// <param name="n1"></param>
        /// <param name="n2"></param>
        /// <returns></returns>
        public double Compute(Node n1, Node n2)
        {
            var lat2 = n2.Latitude;
            var lat1 = n1.Latitude;
            var lon2 = n2.Longitude;
            var lon1 = n1.Longitude;
            var R = 6371; // Radius of the earth in km
            var dLat = deg2rad(lat2 - lat1);  // deg2rad below
            var dLon = deg2rad(lon2 - lon1);
            var a =
                    Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(deg2rad(lat1)) * Math.Cos(deg2rad(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2)
                ;
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var d = R * c; // Distance in km
            return d;
            
        }

        private double deg2rad(double deg)
        {
            return deg * (Math.PI / 180);
        }
    }
}
