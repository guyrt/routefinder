namespace CosmosDBLayer.Serializers
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using RouteFinderDataModel;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Accounts for the custom logic we require to store Nodes and Ways with some memoized information, 
    /// and for CosmosDB-specific storage info.
    /// </summary>
    public class StoredWaySerDe : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Way);
        }

        // Generate a real way from the Json
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var obj = JObject.Load(reader);

            // Then look at the $type property:
            var id = obj["id"]?.Value<string>();
            var nodesRaw = (JArray)obj["Nodes"];
            var nodes = new Node[nodesRaw.Count];
            int i = 0;
            foreach (var nodeRaw in nodesRaw)
            {
                var nodeId = nodeRaw["id"]?.Value<string>();
                var location = (JArray)nodeRaw["location"]["coordinates"];
                var nodeTags = nodeRaw["Tags"]?.ToObject<Dictionary<string, string>>();
                
                var node = new Node(nodeId, location.First.Value<double>(), location.Last.Value<double>(), nodeTags);
                nodes[i++] = node;
            }
            var tags = obj["Tags"]?.ToObject<Dictionary<string, string>>();

            var way = new Way(id, nodes, tags);
            return way;
        }

        /// <summary>
        /// Writing can use default.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="serializer"></param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException("CanWrite should be set to false.");
        }

        public override bool CanWrite => false;
    }
}
