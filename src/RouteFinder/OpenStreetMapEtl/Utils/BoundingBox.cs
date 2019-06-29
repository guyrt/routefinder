using System;

namespace OpenStreetMapEtl.Utils
{
    public class BoundingBox : IComparable
    {
        public double EastLongitude { get; set; }

        public double WestLongitude { get; set; }

        public double NorthLatitude { get; set; }

        public double SouthLatitude { get; set; }

        public int CompareTo(object obj)
        {
            if (obj == null) { return 1; }

            BoundingBox otherBox = obj as BoundingBox;
            if (otherBox != null)
            {
                var comparer = SouthLatitude.CompareTo(otherBox.SouthLatitude);
                if (comparer == 0)
                {
                    comparer = WestLongitude.CompareTo(otherBox.WestLongitude);
                }
                return comparer;
            }
            else
            {
                throw new ArgumentException("Object is not a BoundingBox");
            }
        }

        public bool Overlap(BoundingBox other)
        {
            var box = this;
            var bbox = other;



            var lat = (box.SouthLatitude < bbox.SouthLatitude && bbox.SouthLatitude < box.NorthLatitude) || (box.SouthLatitude < bbox.NorthLatitude && bbox.NorthLatitude < box.NorthLatitude);
            return lat && ((box.WestLongitude < bbox.WestLongitude && bbox.WestLongitude < box.EastLongitude) || (box.WestLongitude < bbox.EastLongitude && bbox.EastLongitude < box.EastLongitude));
        }

        public override string ToString()
        {
            return $"{WestLongitude},{SouthLatitude};{EastLongitude},{NorthLatitude}";
        }
    }
}
