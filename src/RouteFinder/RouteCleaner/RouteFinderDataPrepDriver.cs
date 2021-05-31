using System;
using System.Linq;
using System.Collections.Generic;
using RouteFinderDataModel;
using RouteCleaner.PolygonUtils;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace RouteCleaner
{
    public class RouteFinderDataPrepDriver
    {
        public void RunChain(string boundariesFilePath, string runnableWaysPath)
        {
            var relationRegion = this.GetRegionGeometry(boundariesFilePath, false);

            var thread1 = Task<Dictionary<Relation, Polygon[]>>.Factory.StartNew(() => this.CreateRelationPolygons(relationRegion.Relations));
            var thread2 = Task<Geometry>.Factory.StartNew(() => this.GetRegionGeometry(runnableWaysPath, true));

            Task.WaitAll(thread1, thread2);
            var waysRegion = thread2.Result;
            var relationsDict = thread1.Result;

            var nodeStreamer = this.GetNodeStreamer(runnableWaysPath);

            var createTargetableWays = new CreateTargetableWaysWithinRegions(waysRegion.Ways, relationRegion.Relations);

            var watch = Stopwatch.StartNew();
            WriteNodesToDoc(createTargetableWays, relationsDict, nodeStreamer, @"C:\Users\riguy\code\routefinder\data\nodesWithContainment.json");
            var time = watch.Elapsed;
            Console.WriteLine($"Done with NodeContainment in {time.TotalSeconds} seconds.");

            Console.WriteLine($"Found {createTargetableWays.OutputWays.Count} targetableWays");
            
           /* watch.Restart();

            watch.Restart();
            ways = ConsolidateWays(ways);
            time = watch.Elapsed;
            Console.WriteLine($"Done with ConsolidatedWays in {time.TotalSeconds} seconds. Have {ways.Count} ways.");

            watch.Restart();
            AddWaysToNodes(ways);
            time = watch.Elapsed;
            Console.WriteLine($"Done with AddWaystonodes in {time.TotalSeconds} seconds. Have {ways.Count} ways."); */
            
        }

        /// <summary>
        /// Write nodes to doc 1 at a time as Json
        /// </summary>
        /// <returns></returns>
        public string WriteNodesToDoc(CreateTargetableWaysWithinRegions createTargetableWays, Dictionary<Relation, Polygon[]> relationRegion, IEnumerable<Node> nodeStreamer, string outPath)
        {
            using (var fs = File.OpenWrite(outPath))
            {
                using (var sr = new StreamWriter(fs))
                {
                    foreach (var node in NodeContainment(relationRegion, nodeStreamer))
                    {
                        var nodeStr = JsonConvert.SerializeObject(node);
                        sr.WriteLine(nodeStr);

                        createTargetableWays.ProcessNode(node);
                    }
                }
            }
            return outPath;
        }

        private Dictionary<Relation, Polygon[]> CreateRelationPolygons(IEnumerable<Relation> relations)
        {
            var watch = Stopwatch.StartNew();
            var retVal = relations.AsParallel().ToDictionary(k => k, v => RelationPolygonMemoizer.BuildPolygons(v));
            var time = watch.Elapsed;
            Console.WriteLine($"Done building polygons in {time}");
            return retVal;
        }

        public IEnumerable<Node> NodeContainment(Dictionary<Relation, Polygon[]> relationsDict, IEnumerable<Node> nodeStreamer)
        {
            // prebuild polygons to reduce contention
            foreach (var node in nodeStreamer)
            {
                var containingRelations = relationsDict.AsParallel().Where(kvp => 
                {
                    var target = kvp.Key;
                    var polygons = kvp.Value;

                    foreach (var polygon in polygons)
                    {
                        var containment = new PolygonContainment(polygon);

                        if (containment.Contains(node))
                        {
                            return true;
                        }

                        foreach (var polyNodes in polygon.Nodes)
                        {
                            if (polyNodes.Id == node.Id)
                            {
                                return true;
                            }
                        }
                    }
                    return false;
                }).Select(x => x.Key.Id);

                node.Relations.AddRange(containingRelations);
                yield return node;
            }

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
            var wayDictionary = new Dictionary<string, Dictionary<string, List<Way>>>(); // relationId => (wayName => Ways)

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
            foreach ((var relationId, var waysByName) in wayDictionary)
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
                    var way = new Way(innerWays.First().Id, nodes, tags, relationId, true);
                    newWays.Add(way);
                }
            }

            return newWays;
        }

       /* todo - this but streaming
        * public void AddWaysToNodes(IEnumerable<Way> ways)
        {
            foreach (var way in ways)
            {
                foreach (var node in way.Nodes)
                {
                    node.ContainingWays.Add(way.Id);
                }
            }
        }*/

        private Geometry GetRegionGeometry(string filePath, bool ignoreNodes)
        {
            var watch = Stopwatch.StartNew();
            var osmDeserializer = new OsmDeserializer(true);
            Geometry relationRegion;
            using (var fs = File.OpenRead(filePath))
            {
                using (var sr = new StreamReader(fs))
                {
                    Console.WriteLine($"Loading regions from {filePath}.");
                    relationRegion = osmDeserializer.ReadFile(sr, ignoreNodes);
                }
            }
            var time = watch.Elapsed;
            Console.WriteLine($"Done loading {filePath} in {time}");
            return relationRegion;
        }

        private IEnumerable<Node> GetNodeStreamer(string filePath)
        {
            var osmDeserializer = new OsmDeserializer();
            return osmDeserializer.StreamNode(filePath);
        }
    }
}
