namespace GlobalSettings
{
    public class TripProcessorSettings
    {
        private static TripProcessorSettings instance = null;

        /// <summary>
        /// Two points that are this close are considered overlapping in our main processor.
        /// </summary>
        public double OverlapThresholdMeters { get; set; } = 1;

        /// <summary>
        /// Not thread safe on create. Suggest forcing create in main.
        /// </summary>
        /// <returns></returns>
        public static TripProcessorSettings GetInstance()
        {
            if (instance == null)
            {
                instance = new TripProcessorSettings();
            }
            return instance;
        }
    }
}
