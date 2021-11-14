namespace UserDataModel
{
    /// <summary>
    /// Record of a single user at a single Node. This is the "primary data" for a user.
    /// 
    /// Most other records in our system are simply summaries/caches of the info contained here.
    /// </summary>
    public class UserNodeCoverage
    {
        // Partition on user
        public Guid UserId { get; set; }

        public string RegionId { get; set; }

        public string WayId { get; set; }

        public string NodeId { get; set; }

        public DateTime FirstRan { get; set; }
    }
}
