using Newtonsoft.Json;

namespace UserDataModel
{
    /// <summary>
    /// Cache of node coverage rolled up to a region/way
    /// </summary>
    public class UserWayCoverage : IPartitionedWithWay
    {
        [JsonProperty("id")]
        public string Id => $"{UserId}_{WayId}";

        public Guid UserId { get; set; }

        public string RegionId { get; set; } = string.Empty;

        public string WayId { get; set; } = string.Empty;

        public string WayName { get; set; } = string.Empty;

        public int NodeCompletedCount { get; set; }

        public int NumNodesInWay { get; set; }

        public string Type => "UserWayCoverage";

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
