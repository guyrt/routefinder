using Newtonsoft.Json;

namespace UserDataModel
{
    /// <summary>
    /// Summary object for a region
    /// </summary>
    public class RegionSummary
    {
        [JsonProperty("id")]
        public string Id => RegionId;

        [JsonProperty("pk")]
        public static string Partition => Guid.Empty.ToString();

        public string RegionId { get; set; } = string.Empty;

        public string RegionName { get; set; } = string.Empty;

        public int NumWaysInRegion { get; set; }

        public int NumNodesInRegion { get; set; }

        public string Type => "RegionSummary";
    }
}
