namespace RouteCleanerTests.Utils
{
    using System;
    using System.Linq;
    using RouteCleaner.PolygonUtils;
    using RouteFinderDataModel;
    using Xunit;

    //todo
    // test a set of linked diamonds - play with ordering in there too so you track reversals. You should be able to start with every node/way and get same answer modulo reversals and way ordering. 
    // test disjoint sets (say to sets of linked diamons).

    public class PolygonFactoryTests
    {
        [Fact]
        public void SinglePolygonFromDiamond()
        {
            var nodes = (new[] { 1, 2, 3, 4 }).Select(x => new Node($"{x}", 0.0, 0.0)).ToArray();
            var dummyNodes = (new[] { 1, 2, 3, 4 }).Select(x => new Node($"dummy{x}", 0.0, 0.0)).ToArray(); // sit in middle of wyas
            var ways = new Way[]
            {
                new Way("w1", new[] {nodes[0], dummyNodes[0], nodes[1]}),
                new Way("w2", new[] {nodes[1], dummyNodes[1], nodes[2]}),
                new Way("w3", new[] {nodes[2], dummyNodes[2], nodes[3]}),
                new Way("w4", new[] {nodes[3], dummyNodes[3], nodes[0]}),
            };

            var polygons = PolygonFactory.BuildPolygons(ways);
         //   Assert.Single(polygons);
            // todo = check polygon somehow

            Array.Reverse(ways[1].Nodes);
            Array.Reverse(ways[3].Nodes);

            polygons = PolygonFactory.BuildPolygons(ways);
            Assert.Single(polygons);
        }
    }
}
