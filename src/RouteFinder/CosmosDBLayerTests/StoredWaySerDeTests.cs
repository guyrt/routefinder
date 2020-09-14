namespace CosmosDBLayerTests
{
    using CosmosDBLayer.Serializers;
    using Newtonsoft.Json;
    using RouteFinderDataModel;
    using System.Collections.Generic;
    using Xunit;

    public class StoredWaySerDeTests
    {
        [Fact]
        public void WaySerializedWithPoint()
        {
            var nodes = new[]
            {
                new Node("node1", 0.0, 0.0, new Dictionary<string, string>() {{"firstNode", "yep" } }),
                new Node("node2", 1.0, 1.0)
            };
            var tags = new Dictionary<string, string>()
            {
                {"k1", "v1" },
                {"k2", "v2" }
            };

            var way = new Way("way1", nodes, tags);

            // set up converter
            var serde = new StoredWaySerDe();
            var serialized = JsonConvert.SerializeObject(way, Formatting.Indented, serde);

            // test that Point gets added, but that Lat/Long aren't.
            Assert.Contains("Point", serialized);
            Assert.DoesNotContain("Latitude", serialized);

            var newWay = JsonConvert.DeserializeObject<Way>(serialized, serde);
            Assert.Equal(1.0, newWay.Nodes[1].Latitude);
            Assert.Equal(1.0, newWay.Nodes[1].Longitude);
            Assert.Equal("yep", newWay.Nodes[0].Tags["firstNode"]);
            Assert.Empty(newWay.Nodes[1].Tags);
            Assert.Equal("v1", newWay.Tags["k1"]);
            Assert.Equal("v2", newWay.Tags["k2"]);

            Assert.Equal("way1", newWay.Id);
            Assert.Equal("node1", newWay.Nodes[0].Id);
            Assert.Equal("node2", newWay.Nodes[1].Id);
        }
    }
}
