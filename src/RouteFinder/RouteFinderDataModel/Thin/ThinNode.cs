using Newtonsoft.Json;

namespace RouteFinderDataModel.Thin
{
    public class ThinNode
    {
        public string Id { get; set; }

        [JsonProperty("Lt")]
        public double Latitude { get; set; }

        [JsonProperty("Ln")]
        public double Longitude { get; set; }

        public Node ToThick()
        {
            return new Node(Id, Latitude, Longitude);
        }
    }
}
