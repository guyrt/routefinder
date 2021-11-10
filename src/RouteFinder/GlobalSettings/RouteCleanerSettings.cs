namespace GlobalSettings
{
    public class RouteCleanerSettings
    {
        private static RouteCleanerSettings instance = null;

        private RouteCleanerSettings() { }

        public bool PolygonsShouldConsolidateStraightEdges { get; set; } = false;

        public int ReaderThreadSleepInterval { get; set; } = 1 * 1000;

        public int NumThreads { get; set; } = 4;

        public string TemporaryNodeOutLocation { get; set; } = @"/tmp/nodesWithContainment.json";

        public string TemporaryNodeWithContainingWayOutLocation { get; set; } = @"/tmp/nodesWithWayContainment";

        public string TemporaryTargetableWaysLocation { get; set; } = @"/tmp/targetableWays.json";

        public bool ShouldUploadRawTargetableWays { get; set; } = false;

        /// <summary>
        /// Max number of ways to consolidate.
        /// </summary>
        public int MaxNumberOfWaysToConsolidate { get; set; } = 100;

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
