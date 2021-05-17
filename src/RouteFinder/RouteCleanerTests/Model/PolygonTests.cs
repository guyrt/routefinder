using System;
using RouteFinderDataModel;
using RouteCleaner.PolygonUtils;
using Xunit;

namespace RouteCleanerTests.Model
{
    public class PolygonTests
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void BuildPolygon(bool reverse)
        {
            var n1 = new Node("1", 0, 0);
            var n2 = new Node("2", 1, 1);
            var n3 = new Node("3", 0, 2);
            var n4 = new Node("4", 4, 2);
            var n5 = new Node("5", 3, 1);
            var n6 = new Node("6", 4, 0);
            var way = new Way("5", new[] { n1, n2, n3, n4, n5, n6, n1 });
            if (reverse)
            {
                Array.Reverse(way.Nodes);
            }
            var ll = new[]
            {
                way
            };
            var poly = PolygonFactory.BuildPolygons(ll);
            Assert.Single(poly);
            Assert.Contains(n1, poly[0].Nodes);
            Assert.Contains(n2, poly[0].Nodes);
            Assert.Contains(n3, poly[0].Nodes);
            Assert.Contains(n4, poly[0].Nodes);
            Assert.Contains(n5, poly[0].Nodes);
            Assert.Contains(n6, poly[0].Nodes);
        }
    }
}
