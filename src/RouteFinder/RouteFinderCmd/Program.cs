﻿namespace RouteFinderCmd
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Newtonsoft.Json;
    using RouteCleaner;
    using RouteFinderDataModel;
    using RouteCleaner.Transformers;
    using RouteCleaner.PolygonUtils;
    using RouteFinder;
    using RouteFinder.GreedyRoute;
    using RouteFinder.OptimalRunningRoutes;

    public class Program
    {
        private static readonly string outputLocation = @"C:\Users\riguy\Documents\GitHub\routefinder\src\RouteViewerWeb\data\";

        public static void Main()
        {
            /*var acd = AreaCacheDownload.Create(new AzureFileCache());
            var region = acd.GetRegion(47.627773, -122.208002, 5);
            region = new OnlyTraversable().Transform(region);
            region = new CollapseParkingLots().Transform(region);
            var ways = new SplitBisectedWays().Transform(region.Ways);
            var graph = new GraphBuilder(new NoopGraphFilter(), new ReasonablyEnjoyableRunningCost()).BuildGraph(ways.ToArray(), out var originalEdgeWays);
            //var routes = new PotentialRoutes<Node>(graph, SimpleDistance.Compute);
            //var routeList = routes.GetRouteGreedy(region.Nodes.First(x => x.Id == "4521863210"), 20).ToList();
            var routes = new AllPairsShortestWithReturns<Node>(graph);
            var routeList = routes.GetRoutes(region.Nodes.First(x => x.Id == "29937652"), 20);
            for (var i = 0; i < routeList.Count - 1; i++)
            {
                if (!routeList[i].Equals(routeList[i + 1]))
                {
                    var way = originalEdgeWays[routeList[i], routeList[i + 1]];
                    Console.WriteLine(way);
                }
            }*/
        }

        /// <summary>
        /// Find optimal running route through cougar
        /// </summary>
        /// <param name="geometry"></param>
        private static void RunCougarFinder(Geometry geometry) {
            geometry = new DropBuildings().Transform(geometry);
            geometry = new DropWater().Transform(geometry);

            OutputPolygons("275765", geometry);

            geometry = new OnlyTraversable().Transform(geometry);
            geometry = new CollapseParkingLots().Transform(geometry);
            var cougar = geometry.Relations.First(x => x.Id == "275765");
            var ways = new LabelWaysInRelation().Transform(cougar, geometry);
            ways = new SplitBisectedWays().Transform(ways);
            DebugOut(ways, "labeledWays.json");

            var targetNodes = new HashSet<Node>();
            foreach (var way in ways)
            {
                if (way.Tags["rfInPolygon"] == "in")
                {
                    targetNodes.Add(way.Nodes.First());
                    targetNodes.Add(way.Nodes.Last());
                }
            }

            var graph = new GraphBuilder(new RequiredEdgeGraphFilter(), new ReasonablyEnjoyableRunningCost()).BuildGraph(ways.ToArray(), out var originalEdgeWays);

            new GraphSummaryOutputter(outputLocation).OutputGraph(graph, originalEdgeWays, "reducedGraph.json");

            var requiredCost = graph.RequiredEdgeCost();

            // lazy route
            var lazyRouteFinder = new RouteFinder<Node>(new LazyGraphAugmenter<Node>());
            var lazyRoute = lazyRouteFinder.GetRoute(graph);
            var lazyRouteCost = lazyRoute.Select(x => x.Distance).Sum();

            // do regular route (rebuilds graph)
            graph = new GraphBuilder(new RequiredEdgeGraphFilter(), new ReasonablyEnjoyableRunningCost()).BuildGraph(ways.ToArray(), out originalEdgeWays);
            var greedRouteFinder = new RouteFinder<Node>(new GreedyGraphAugmenter<Node>());
            var route = greedRouteFinder.GetRoute(graph);

            var routeCost = route.Select(x => x.Distance).Sum();
            //new RouteCoverageOutputter(outputLocation).OutputGraph(route, originalEdgeWays, "greedyRouteCoverage.json");

            new RouteDetailOutputter(ways, outputLocation, "lazyRouteCoverage.json", "instructions.txt").DescribeRoutesAsWays(lazyRoute);

            Console.WriteLine($"Required running: {requiredCost}");
            Console.WriteLine($"Lazy Route: {lazyRouteCost}");
            Console.WriteLine($"Greedy Route: {routeCost}");
            Console.Read();
        }

        private static void OutputPolygons(string polygonId, Geometry geometry)
        {
            var cougar = geometry.Relations.First(x => x.Id == polygonId);
            var polygons = RelationPolygonMemoizer.Instance.GetPolygons(cougar);
            DebugOut(polygons.First(), "cleanpolygon.json");
        }

        public static void DebugOut(List<Way> ways, string filename)
        {
            var fullPath = Path.Combine(outputLocation, filename);
            var converter = new GeoJsonConverter();
            var polygonOut = converter.Convert(ways);
            var serialized = JsonConvert.SerializeObject(polygonOut);
            File.WriteAllLines(fullPath, new[] { serialized });
        }

        public static void DebugOut(Polygon p, string filename)
        {
            var fullPath = Path.Combine(outputLocation, filename);
            var converter = new GeoJsonConverter();
            var polygonOut = converter.Convert(p);
            var serialized = JsonConvert.SerializeObject(polygonOut);
            File.WriteAllLines(fullPath, new []{serialized});
        }

        public static void DebugOut(IEnumerable<Node> nodes, string filename)
        {
            var fullPath = Path.Combine(outputLocation, filename);
            var converter = new GeoJsonConverter();
            var polygonOut = converter.Convert(nodes);
            var serialized = JsonConvert.SerializeObject(polygonOut);
            File.WriteAllLines(fullPath, new[] { serialized });
        }

    }
}
