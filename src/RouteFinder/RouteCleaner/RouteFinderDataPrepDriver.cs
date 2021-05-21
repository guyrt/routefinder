using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using RouteFinderDataModel;
using RouteCleaner.PolygonUtils;
using System.Diagnostics;

namespace RouteCleaner
{
    public class RouteFinderDataPrepDriver
    {
        public Geometry RunChain(Geometry relationRegion, Geometry waysRegion)
        {
            var watch = Stopwatch.StartNew();
            var nodes = NodeContainment(relationRegion, waysRegion);
            var time = watch.Elapsed;
            Console.WriteLine($"Done with NodeContainment in {time.TotalSeconds} seconds.");
            
            watch.Restart();
            var ways = GeneratedWaysPerRegion(nodes, waysRegion.Ways);
            time = watch.Elapsed;
            Console.WriteLine($"Done with GeneratedWaysPerRegion in {time.TotalSeconds} seconds. Have {ways.Count} ways.");

            watch.Restart();
            ways = ConsolidateWays(ways);
            time = watch.Elapsed;
            Console.WriteLine($"Done with ConsolidatedWays in {time.TotalSeconds} seconds. Have {ways.Count} ways.");

            watch.Restart();
            AddWaysToNodes(ways);
            time = watch.Elapsed;
            Console.WriteLine($"Done with AddWaystonodes in {time.TotalSeconds} seconds. Have {ways.Count} ways.");
            return new Geometry(nodes.Values.ToArray(), ways.ToArray(), Array.Empty<Relation>());
        }

        public Dictionary<string, Node> NodeContainment(Geometry relationRegion, Geometry waysRegion)
        {
            var relations = relationRegion.Relations;//.Where(x => x.Id == "237356").ToArray();// kirkland: 237356. nike park 11556206
            var ways = waysRegion.Ways;

            var nodes = new Dictionary<string, Node>();
            foreach (var w in ways)
            {
                foreach (var n in w.Nodes)
                {
                    if (!nodes.ContainsKey(n.Id))
                    {
                        nodes.Add(n.Id, n);
                    }
                }
            }

            var rnd = new Random();
            var randomRelations = relations.OrderBy(x => rnd.Next()); // randomize order to reduce contention on Nodes.

            Parallel.ForEach(randomRelations, target =>
            {
                var cnt = 0;
                var polygons = RelationPolygonMemoizer.BuildPolygons(target);
                if (polygons.Count() == 0)
                {
                    NonBlockingConsole.WriteLine($"Skipping {target.Id}");
                    return;  // typically this implies a non-closed relation.
                }

                cnt = 0;
                // todo - do the in/out part
                foreach (var polygon in polygons)
                {
                    var containment = new PolygonContainment(polygon);
                    
                    foreach (var node in nodes.Values)
                    {
                        if (containment.Contains(node))
                        {
                            node.Relations.Add(target);
                            cnt++;
                        }
                    }

                    foreach (var node in polygon.Nodes)
                    {
                        if (nodes.ContainsKey(node.Id) && !nodes[node.Id].Relations.Contains(target))
                        {
                            nodes[node.Id].Relations.Add(target);
                        }
                    }
                }

                NonBlockingConsole.WriteLine($"Process {target} Found {cnt} nodes in and {nodes.Count - cnt} out.");
            });

            Console.WriteLine($"Found {nodes.Values.Select(x => x.Relations.Count).Sum()} contains with max of {nodes.Values.Select(x => x.Relations.Count).Max()}");
            return nodes;
        }
    
        /// <summary>
        /// Construct a unique Way for each region based on node containments.
        /// 
        /// Track number of relations that contain the node. 
        /// </summary>
        public List<Way> GeneratedWaysPerRegion(Dictionary<string, Node> nodeDict, IEnumerable<Way> incomingWays)
        {
            var ways = new List<Way>();

            foreach (var way in incomingWays)
            {
                var activeRegions = new Dictionary<Relation, List<Node>>();

                foreach (var node in way.Nodes)
                {
                    foreach (var region in node.Relations)
                    {
                        if (!activeRegions.ContainsKey(region))
                        {
                            activeRegions[region] = new List<Node>();
                        }
                        activeRegions[region].Add(node);
                    }

                    foreach (var region in activeRegions.Keys)
                    {
                        if (!node.Relations.Contains(region)) // this is a very short set.
                        {
                            // the end of a way segment. Create a new Way
                            var newId = $"{way.Id}_in_{region.Id}";
                            var newWay = new Way(newId, activeRegions[region].ToArray(), way.Tags, region);
                            ways.Add(newWay);
                            activeRegions.Remove(region);
                        }
                    }
                }

                foreach (var region in activeRegions.Keys)
                {
                    // the end of a way segment. Create a new Way
                    var newId = $"{way.Id}_in_{region.Id}";
                    var newWay = new Way(newId, activeRegions[region].ToArray(), way.Tags, region);
                    ways.Add(newWay);
                }
            }

            return ways;
        }

        /// <summary>
        /// Make at most one Way per region with same name.
        /// 
        /// If name is blank, then make one up.
        /// 
        /// We want to track "done" by looking at the whole street done AND node done rate. But for counties ect this will be very hard for streets. Lot of "1st st" in WA state. Eh. Run em all.
        /// </summary>
        /// <returns></returns>
        public List<Way> ConsolidateWays(IEnumerable<Way> ways)
        {
            var wayDictionary = new Dictionary<Relation, Dictionary<string, List<Way>>>(); // relationId => (wayName => Ways)

            foreach (var way in ways)
            {
                var wayName = way.Name;

                var relation = way.ContainedIn;
                if (!wayDictionary.ContainsKey(relation))
                {
                    wayDictionary.Add(relation, new Dictionary<string, List<Way>>());
                }
                if (!wayDictionary[relation].ContainsKey(wayName))
                {
                    wayDictionary[relation][wayName] = new List<Way>();
                }
                wayDictionary[relation][wayName].Add(way);
            }

            var newWays = new List<Way>();
            foreach ((var relation, var waysByName) in wayDictionary)
            {
                foreach ((var wayName, var innerWays) in waysByName)
                {
                    var nodes = innerWays.SelectMany(x => x.Nodes).Distinct().ToArray(); // order is obliterated.
                    var tags = new Dictionary<string, string>();
                    // take tags somewhat arbitrarily.
                    foreach (var innerWay in innerWays)
                    {
                        foreach (var kvp in innerWay.Tags)
                        {
                            if (!tags.ContainsKey(kvp.Key))
                            {
                                tags.Add(kvp.Key, kvp.Value);
                            }
                        }
                    }
                    var way = new Way(innerWays.First().Id, nodes, tags, relation, true);
                    newWays.Add(way);
                }
            }

            return newWays;
        }

        public void AddWaysToNodes(IEnumerable<Way> ways)
        {
            foreach (var way in ways)
            {
                foreach (var node in way.Nodes)
                {
                    node.ContainingWays.Add(way);
                }
            }
        }
    }
}
