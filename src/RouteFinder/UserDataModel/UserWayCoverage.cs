namespace UserDataModel
{
    /// <summary>
    /// Cache of node coverage rolled up to a region/way
    /// </summary>
    public class UserWayCoverage
    {
        public Guid UserId { get; set; }

        public string RegionId { get; set; } = string.Empty;

        public string WayId { get; set; } = string.Empty;

        public int NodeCompletedCount { get; set; }

        public int NumNodesInWay { get; set; }
    }
}
