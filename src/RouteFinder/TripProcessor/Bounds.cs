namespace TripProcessor
{
    public readonly struct Bounds
    {
        public Bounds(double minLat, double minLng, double maxLat, double maxLng)
        {
            MinLat = minLat;
            MinLng = minLng;
            MaxLat = maxLat;
            MaxLng = maxLng;
        }
        
        public double MinLat { get; init; }

        public double MinLng { get; init; }

        public double MaxLat { get; init; }
        
        public double MaxLng { get; init; }
    }
}
