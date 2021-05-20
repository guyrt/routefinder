using RouteFinderDataModel;
using RouteCleaner.PolygonUtils;
using Xunit;

namespace RouteCleanerTests.Utils
{
    public class PolygonUtilsTest
    {
        [Fact]
        public void TestAngle()
        {
            var center = new Node("1", 47.5270630, -122.1292480);
            var left = new Node("2", 47.5271130, -122.1431340);
            var right = new Node("3", 47.5288370, -122.1275700);

            var cp = PolygonUtils.CrossProductZ(left, center, right);
            Assert.True(cp > 0);

            var cp2 = PolygonUtils.CrossProductZ(right, center, left);
            Assert.True(cp2 < 0);
        }
    }
}
