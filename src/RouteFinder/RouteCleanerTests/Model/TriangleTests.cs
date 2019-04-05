using RouteCleaner.Model;
using Xunit;

namespace RouteCleanerTests.Model
{

    public class TriangleTests
    {
        [Fact]
        public void Contains()
        {
            var triangle = new Triangle(new Node("1", 0, 0), new Node("2", 1, 0), new Node("3", 0, 1));
            Assert.False(triangle.Contains(new Node("4", 1, 1)));
            Assert.True(triangle.Contains(new Node("4", 0.5, 0.5))); // note: this one is right on line!
            Assert.True(triangle.Contains(new Node("4", 0.0, 0.5))); // note: this one is right on line!
            Assert.True(triangle.Contains(new Node("4", 0.5, 0.0))); // note: this one is right on line!
            Assert.True(triangle.Contains(new Node("4", 0.5, 0.25)));
            Assert.True(triangle.Contains(new Node("4", 0, 0)));
            Assert.False(triangle.Contains(new Node("4", -1, 1)));
            Assert.False(triangle.Contains(new Node("4", 1, -1)));
            Assert.False(triangle.Contains(new Node("4", -1, -1)));
        }

        [Fact]
        public void Area()
        {
            var triangle = new Triangle(new Node("1", 0, 0), new Node("2", 1, 0), new Node("3", 0, 1));
            Assert.InRange(triangle.Area, .5 - 1e-8, .5 + 1e-8);
        }
    }
}
