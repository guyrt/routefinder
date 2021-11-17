using Newtonsoft.Json;

namespace UserDataModel
{
    /// <summary>
    /// Record of a single user at a single Node. This is the "primary data" for a user.
    /// 
    /// Most other records in our system are simply summaries/caches of the info contained here.
    /// </summary>
    public class UserNodeCoverage : IPartitionedWithWay
    {
        [JsonProperty("id")]
        public string Id => $"{UserId}_{RegionId}_{WayId}_{NodeId}";

        // Partition on user
        public Guid UserId { get; set; }

        public string RegionId { get; set; }

        public string WayId { get; set; }

        public string NodeId { get; set; }

        public DateTime FirstRan { get; set; }

        public string Type => "UserNodeCoverage";

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
