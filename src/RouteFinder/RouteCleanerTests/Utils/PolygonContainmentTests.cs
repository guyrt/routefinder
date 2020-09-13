using RouteFinderDataModel;
using RouteCleaner.PolygonUtils;
using Xunit;

namespace RouteCleanerTests.Utils
{
    public class PolygonContainmentTests
    {
        private Polygon GetSquare(double edgeLength, double center)
        {
            var n = new Node("2", center, center);
            return PolygonFactory.BuildPolygons(new[]
            {
                new Way("1", new[]
                {
                    n,
                    new Node("3", center, center + edgeLength),
                    new Node("4", center + edgeLength, center + edgeLength),
                    new Node("5", center + edgeLength, center),
                    n
                })
            })[0];
        }

        [Theory]
        [InlineData(1, 0)]
        [InlineData(2, -1)]
        public void TotallyContained(double edgeLength, double center)
        {
            var polygon = GetSquare(edgeLength, center);
            var way = new Way("abc", new []
            {
                new Node("20", center, center),
                new Node("21", center + edgeLength / 2, center + edgeLength / 2),
                new Node("22", center + edgeLength, center + edgeLength)
            });
            var polygonContainment = new PolygonContainment(polygon);
            var (contained, uncontained) = polygonContainment.Containment(way);
            Assert.Contains(way, contained);
            Assert.Empty(uncontained);
        }

        [Theory]
        [InlineData(1, 0)]
        [InlineData(2, -1)]
        public void NotContained(double edgeLength, double center)
        {
            var polygon = GetSquare(edgeLength, center);

            var moveOut = 2 * edgeLength;

            var way = new Way("abc", new[]
            {
                new Node("2", center + moveOut, 2 * center + moveOut),
                new Node("2", center + edgeLength / 2 + moveOut, center + edgeLength / 2 + moveOut),
                new Node("2", center + edgeLength + moveOut, center + edgeLength + moveOut)
            });
            var polygonContainment = new PolygonContainment(polygon);
            var (contained, uncontained) = polygonContainment.Containment(way);
            Assert.Contains(way, uncontained);
            Assert.Empty(contained);
        }

        [Fact]
        public void SplitContained()
        {
            var polygon = GetSquare(1, 0);

            var way = new Way("abc", new[]
            {
                new Node("20", -1, 0.5),
                new Node("21", 0, 0.5),
                new Node("22", 0.5, 0.5),
                new Node("23", 1.5, 0.5),
                new Node("24", 0.5, 0.25),
                new Node("25", 0.5, -0.5)
            });
            var polygonContainment = new PolygonContainment(polygon);
            var (contained, uncontained) = polygonContainment.Containment(way);
            Assert.Contains(way.Nodes[0], uncontained[0].Nodes);
            Assert.Contains(way.Nodes[1], contained[0].Nodes);
            Assert.Contains(way.Nodes[2], contained[0].Nodes);
            Assert.Contains(way.Nodes[3], uncontained[1].Nodes);
            Assert.Contains(way.Nodes[4], contained[1].Nodes);
            Assert.Contains(way.Nodes[5], uncontained[2].Nodes);
            Assert.Equal(2, contained.Count);
            Assert.Equal(3, uncontained.Count);
        }
    }
}
