using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using RouteFinderDataModel;
using RouteCleaner.PolygonUtils;

namespace RouteCleaner
{
    public class RouteFinderDataPrepDriver
    {
        public Geometry RunChain(Geometry relationRegion, Geometry waysRegion)
        {
            var nodes = NodeContainment(relationRegion, waysRegion);
            Console.WriteLine("Done with NodeContainment");
            var ways = GeneratedWaysPerRegion(nodes, waysRegion);
            Console.WriteLine("Done with GeneratedWaysPerRegion");
            AddNodesToWays(ways);
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

            //for (var i = 0; i < relations.Length; i++)
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
        public List<Way> GeneratedWaysPerRegion(Dictionary<string, Node> nodeDict, Geometry waysRegion)
        {
            var ways = new List<Way>();

            foreach (var way in waysRegion.Ways)
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
                        if (!node.Relations.Contains(region))  // todo - not very performant.
                        {
                            // the end of a way segment. Create a new Way
                            var newId = $"{way.Id}_in_{region.Id}";
                            var newWay = new Way(newId, activeRegions[region].ToArray(), way.Tags, region);
                            ways.Add(newWay);
                            activeRegions.Remove(region);
                        }
                    }
                }
            }

            return ways;
        }

        /// <summary>
        /// Make at most one Way per region with same name.
        /// 
        /// If name is blank, then make one up.
        /// </summary>
        /// <returns></returns>
        public List<Way> ConsolidateWays(IEnumerable<Way> ways)
        {
            var defaultName = "Unnamed path";
            var wayDictionary = new Dictionary<string, Dictionary<string, List<Way>>>(); // relationId => (wayName => Ways)

            foreach (var way in ways)
            {
                if (way.Tags.TryGetValue("name", out var wayName))
                {
                    wayName = string.IsNullOrEmpty(wayName) ? defaultName : wayName;
                }
                else
                {
                    wayName = defaultName;
                }

                var relationId = way.ContainedIn.Id;
                if (!wayDictionary.ContainsKey(relationId))
                {
                    wayDictionary.Add(relationId, new Dictionary<string, List<Way>>());
                }
                if (!wayDictionary[relationId].ContainsKey(wayName))
                {
                    wayDictionary[relationId][wayName] = new List<Way>();
                }
                wayDictionary[relationId][wayName].Add(way);
            }

            var newWays = new List<Way>();
            foreach ((var relation, var waysByName) in wayDictionary)
            {
                foreach ((var wayName, var innerWays) in waysByName)
                {
                    var nodes = innerWays.SelectMany(x => x.Nodes).Distinct().ToArray();

                }
            }

            return newWays;
        }

        public void AddNodesToWays(IEnumerable<Way> ways)
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
