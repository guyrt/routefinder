using Newtonsoft.Json;

namespace UserDataModel
{
    /// <summary>
    /// Todo - track completed and advanced ways
    /// </summary>
    public class RunDetails : IPartitionedDataModel
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        public Guid UserId { get; set; }

        public string Name { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; }

        // tracks can have more than one section
        public RunPoint[][] Route { get; set; } = Array.Empty<RunPoint[]>();

        public class RunPoint
        {
            public decimal Latitude { get; set; }

            public decimal Longitude { get; set; }

            public decimal Elevation { get; set; }
        }

        public string Type => "RunDetails";
    }
}
