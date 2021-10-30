namespace GlobalSettings
{
    public class RouteCleanerSettings
    {
        private static RouteCleanerSettings instance = null;

        private RouteCleanerSettings() { }

        public bool PolygonsShouldConsolidateStraightEdges { get; set; } = false;

        public int ReaderThreadSleepInterval { get; set; } = 1 * 1000;

        public int NumThreads { get; set; } = 8;

        public string TemporaryNodeOutLocation { get; set; } = @"/tmp/nodesWithContainment.json";

        public string TemporaryNodeWithContainingWayOutLocation { get; set; } = @"/tmp/nodesWithWayContainment";

        public string TemporaryTargetableWaysLocation { get; set; } = @"/tmp/targetableWays.json";

        /// <summary>
        /// Not thread safe on create. Suggest forcing create in main.
        /// </summary>
        /// <returns></returns>
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
