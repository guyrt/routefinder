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

        public string RegionId { get; set; } = string.Empty;

        public string WayId { get; set; } = string.Empty;

        public string NodeId { get; set; } = string.Empty;

        public string Type => "UserNodeCoverage";

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            UserNodeCoverage other = (UserNodeCoverage)obj;
            return this.Id == other.Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
