namespace GlobalSettings
{
    public class RouteCleanerSettings
    {
        private static RouteCleanerSettings instance = null;

        private RouteCleanerSettings() { }


        public bool PolygonsShouldConsolidateStraightEdges { get; set; } = false;

        public int ReaderThreadSleepInterval { get; set; } = 10 * 1000;

        public int NumThreads { get; set; } = 8;

        public static RouteCleanerSettings GetInstance()
        {
            if (instance == null)
            {
                instance = new RouteCleanerSettings();
            }
            return instance;
        }
    }
}
