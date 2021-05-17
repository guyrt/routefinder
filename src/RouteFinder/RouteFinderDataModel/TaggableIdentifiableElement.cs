namespace RouteFinderDataModel
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public abstract class TaggableIdentifiableElement
    {
        protected TaggableIdentifiableElement(string id, Dictionary<string, string> tags = null)
        {
            if (tags == null)
            {
                tags = new Dictionary<string, string>();
            }
            Tags = tags;
            Id = id;
        }

        [JsonProperty("id")]
        public string Id { get; }

        public Dictionary<string, string> Tags { get; }
    }
}
