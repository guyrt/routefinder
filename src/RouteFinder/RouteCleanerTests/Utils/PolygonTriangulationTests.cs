using System;
using RouteFinderDataModel;
using RouteCleaner.PolygonUtils;
using Xunit;

namespace RouteCleanerTests.Utils
{
    public class PolygonTriangulationTests
    {
        [Fact]
        public void TriangulateSquare()
        {
            var n1 = new Node("1", 0, 0);
            var n2 = new Node("2", 0, 1);
            var n3 = new Node("3", 1, 1);
            var n4 = new Node("4", 1, 0);
            var ll = new []
            {
                new Way("5", new[] {n1, n2}),
                new Way("6", new[] {n2, n3}),
                new Way("7", new[] {n3, n4}),
                new Way("8", new[] {n4, n1})
            };
            var poly = PolygonFactory.BuildPolygons(ll)[0];
            var triangulator = new PolygonTriangulation(poly);
            var triangles = triangulator.Triangulate();
            Assert.Equal(2, triangles.Count);
            Assert.Equal(n1, triangles.First.Value.Nodes[0]);
            Assert.Equal(n2, triangles.First.Value.Nodes[1]);
            Assert.Equal(n4, triangles.First.Value.Nodes[2]);
            Assert.Equal(n2, triangles.Last.Value.Nodes[0]);
            Assert.Equal(n3, triangles.Last.Value.Nodes[1]);
            Assert.Equal(n4, triangles.Last.Value.Nodes[2]);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void TriangulateStrangeShape(bool reverse)
        {
            var n1 = new Node("1", 0, 0);
            var n2 = new Node("2", 1, 1);
            var n3 = new Node("3", 0, 2);
            var n4 = new Node("4", 4, 2);
            var n5 = new Node("5", 3, 1);
            var n6 = new Node("6", 4, 0);
            var way = new Way("5", new[] {n1, n2, n3, n4, n5, n6, n1});
            if (reverse)
            {
                Array.Reverse(way.Nodes);
            }
            var ll = new[]
            {
                way
            };
            var poly = PolygonFactory.BuildPolygons(ll)[0];
            var triangulator = new PolygonTriangulation(poly);
            var triangles = triangulator.Triangulate();
            Assert.Equal(4, triangles.Count);
        }
    }
}
