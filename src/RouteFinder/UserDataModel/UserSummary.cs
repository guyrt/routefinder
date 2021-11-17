using Newtonsoft.Json;

namespace UserDataModel
{
    /// <summary>
    /// Summary of a user that can be used to retrieve basic run stats.
    /// </summary>
    public class UserSummary : IPartitionedDataModel
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        public Guid UserId { get; set; }

        /// <summary>
        /// Summary objects for every region this user has started.
        /// </summary>
        public List<RegionSummary>? RegionSummaries { get; set; }

        public string Type => "UserSummary";

        public class RegionSummary
        {
            public string RegionId { get; set; }

            public string Name { get; set; } = string.Empty;

            /// <summary>
            /// Pointer to the most recent page of region summary.
            /// </summary>
            public int LatestRegionDetailsPage { get; set; } = 0;

            // Track coverage stats
            public int CompletedStreets { get; set; }
            public int StartedStreets { get; set; }

            public double TotalStreets { get; set; }

            public int CompletedNodes { get; set; }

            public int TotalNodes { get; set; }

            /// <summary>
            /// If true, this is a priority region for the user.
            /// </summary>
            public bool IsStarred { get; set; }

            public DateTime FirstCoverage { get; set; }

            public DateTime LastCoverage { get; set; }
        }
    }
}