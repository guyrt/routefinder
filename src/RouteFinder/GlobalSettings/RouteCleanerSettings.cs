namespace GlobalSettings
{
    public class RouteCleanerSettings
    {
        private static RouteCleanerSettings instance = null;

        private RouteCleanerSettings() { }


        public bool PolygonsShouldConsolidateStraightEdges { get; set; } = true;

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
