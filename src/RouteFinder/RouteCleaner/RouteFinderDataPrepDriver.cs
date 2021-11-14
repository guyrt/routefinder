using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GlobalSettings;
using Newtonsoft.Json;
using RouteCleaner.PolygonUtils;
using RouteFinderDataModel;

namespace RouteCleaner
{
    public class RouteFinderDataPrepDriver
    {
        public void RunChain(string boundariesFilePath, string runnableWaysPath)
        {
            var relationRegion = this.GetRegionGeometry(boundariesFilePath, false, false);

            var thread1 = Task<Dictionary<Relation, Polygon[]>>.Factory.StartNew(() => this.CreateRelationPolygons(relationRegion.Relations));
            var thread2 = Task<Geometry>.Factory.StartNew(() => this.GetRegionGeometry(runnableWaysPath, true, false));

            Task.WaitAll(thread1, thread2);
            var waysRegion = thread2.Result;
            var relationsDict = thread1.Result;

            var nodeStreamer = this.GetNodeStreamer(runnableWaysPath);

            var watch = Stopwatch.StartNew();
            var createTargetableWays = new CreateTargetableWaysWithinRegions(waysRegion.Ways, relationRegion.Relations);
            var time = watch.Elapsed;
            Console.WriteLine($"Done prepping ways in {time}");
            watch.Restart();

            WriteNodesToDoc(createTargetableWays, relationsDict, nodeStreamer, RouteCleanerSettings.GetInstance().TemporaryNodeOutLocation);
            time = watch.Elapsed;
            Console.WriteLine($"Done with NodeContainment in {time} seconds.");

            Console.WriteLine($"Found {createTargetableWays.OutputWays.Count} targetableWays");

            watch.Restart();
            var ways = createTargetableWays.OutputWays;

            var w = ways.Where(w => w.Id == "");


            ways = ConsolidateWays(ways);
            ways = UnconsolidateLargeWays(ways).ToList();  // todo is the problem?
            time = watch.Elapsed;
            Console.WriteLine($"Done with ConsolidatedWays in {time} seconds. Have {ways.Count} ways.");

            this.WriteWays(ways, RouteCleanerSettings.GetInstance().TemporaryTargetableWaysLocation);
        }

        /// <summary>
        /// Write nodes to doc 1 at a time as Json
        /// </summary>
        /// <returns></returns>
        public string WriteNodesToDoc(CreateTargetableWaysWithinRegions createTargetableWays, Dictionary<Relation, Polygon[]> relationRegion, IEnumerable<Node> nodeStreamer, string outPath)
        {
            using (var fs = File.Open(outPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read))
            {
                using (var sr = new StreamWriter(fs, Encoding.UTF8, 65536)) // set a larger buffer
                {
                    foreach (var node in ThreadedNodeContainment(relationRegion, nodeStreamer))
                    {
                        if (createTargetableWays.ProcessNode(node) || node.Relations.Count() > 0)
                        {
                            var nodeStr = JsonConvert.SerializeObject(node);
                            sr.WriteLine(nodeStr);
                        }
                    }
                }
            }
            return outPath;
        }

        public string WriteWays(IEnumerable<TargetableWay> targetableWays, string outPath)
        {
            using (var fs = File.OpenWrite(outPath))
            {
                using (var sr = new StreamWriter(fs))
                {
                    foreach (var way in targetableWays)
                    {
                        sr.WriteLine(JsonConvert.SerializeObject(way));
                    }
                }
            }
            return outPath;
        }

        public string WriteRelations(Dictionary<Relation, Polygon[]> relations, string outPath)
        {
            using (var fs = File.OpenWrite(outPath))
            {
                using (var sr = new StreamWriter(fs))
                {
                    foreach (var kvp in relations)
                    {
                        var outputRelation = new TargetableRelation
                        {
                            Id = kvp.Key.Id,
                            Borders = kvp.Value.Select(v => v.Nodes.Select(n => n.ToThin()).ToArray()).ToArray(),
                            Name = kvp.Key.Name,
                            RelationType = "todo"
                        };
                        sr.WriteLine(JsonConvert.SerializeObject(outputRelation));
                    }
                }
            }
            return outPath;
        }

        /// <summary>
        /// use n + 2 threads. 
        /// Thread 0 will iterate through nodeStreamer and add to n queues in round robin.
        /// Threads 1-n will read a queue and process, writing to a queue.
        /// Thread n+1 will read all output queues and yield one at a time.
        /// </summary>
        /// <param name="relationsDict"></param>
        /// <param name="nodeStreamer"></param>
        /// <returns></returns>

        public IEnumerable<Node> ThreadedNodeContainment(Dictionary<Relation, Polygon[]> relationsDict, IEnumerable<Node> nodeStreamer)
        {
            var allDone = false; // shared - read but only written by the main thread.
            int numThreads = RouteCleanerSettings.GetInstance().NumThreads;
            var requestQueues = Enumerable.Range(0, numThreads).Select(_ => new ConcurrentQueue<Node>()).ToArray();
            var responseQueues = Enumerable.Range(0, numThreads).Select(_ => new ConcurrentQueue<Node>()).ToArray();
            var readThread = Task<int>.Factory.StartNew(() =>
            {
                var numNodes = 0;
                foreach (var node in nodeStreamer)
                {
                    requestQueues[numNodes % numThreads].Enqueue(node);
                    numNodes++;

                    if (numNodes % 10000 == 0)
                    {
                        var depths = requestQueues.Select(q => q.Count);
                        var averageDepth = depths.Average();
                        while (averageDepth > 20000)
                        {
                            //   Console.WriteLine($"Reader thread sleeping to let other threads catch up");
                            Thread.Sleep(RouteCleanerSettings.GetInstance().ReaderThreadSleepInterval); // give it a little time to cool off.

                            depths = requestQueues.Select(q => q.Count);
                            averageDepth = depths.Average();
                        }
                    }
                }

                // when done, push a null to each queue
                Console.WriteLine($"Reader thread done");
                foreach (var q in requestQueues)
                {
                    q.Enqueue(null);
                }

                return numNodes;
            });

            var processedCount = new int[numThreads];
            var processThreads = Enumerable.Range(0, numThreads).Select(processThreadIdx => Task<int>.Factory.StartNew(() =>
            {
                Console.WriteLine($"Thread {processThreadIdx} reporting for duty");
                while (true)
                {
                    Node nodeToProcess = null;
                    while (requestQueues[processThreadIdx].TryDequeue(out nodeToProcess))  // when queue is empty, we want to keep processing. When it has a null in it, we halt. Thus two whiles.
                    {
                        if (nodeToProcess == null)
                        {
                            // pusher will push a null when the queue is done.
                            responseQueues[processThreadIdx].Enqueue(null);
                            Console.WriteLine($"Thread {processThreadIdx} done");
                            return processThreadIdx;
                        }

                        var containingRelations = relationsDict.Where(kvp =>
                        {
                            var target = kvp.Key;
                            var polygons = kvp.Value;

                            foreach (var polygon in polygons)
                            {
                                if (PolygonContainment.Contains(polygon, nodeToProcess))
                                {
                                    return polygon.IsOuter;
                                }
                            }

                            return false;
                        }).Select(x => x.Key.Id);

                        nodeToProcess.Relations.AddRange(containingRelations);
                        responseQueues[processThreadIdx].Enqueue(nodeToProcess);
                        processedCount[processThreadIdx]++;
                    }

                    Console.WriteLine($"Thread {processThreadIdx} failed to dequeue");
                    Thread.Sleep(1000);
                }
            })).ToArray(); // <-- that's important - otherwise these never actually happen!

            // print status thread
            var statusThread = Task.Factory.StartNew(() =>
            {
                while (!allDone)
                {
                    Thread.Sleep(10 * 1000);
                    var queueDepths = requestQueues.Select(q => q.Count).Average();
                    var averageFinished = processedCount.Sum();
                    Console.WriteLine($"Checkin: {queueDepths} average depth with {averageFinished} processed.");
                }
                Console.WriteLine("Checking thread done.");
            });

            // main thread writes
            var deadThreads = Enumerable.Range(0, numThreads).Select(_ => false).ToArray();
            while (true)
            {
                var didWork = false;
                for (var i = 0; i < numThreads; i++)
                {
                    if (!deadThreads[i])
                    {
                        Node processedNode = null;
                        if (responseQueues[i].TryDequeue(out processedNode))
                        {
                            if (processedNode == null)
                            {
                                deadThreads[i] = true;
                            }
                            else
                            {
                                didWork = true;
                                yield return processedNode;
                            }
                        }
                    }
                }
                if (deadThreads.All(x => x))
                {
                    break;
                }
                if (!didWork)
                {
                    Thread.Sleep(1000); // if the queues are all empty, then wait for a little while. No sense having this thread spin.
                    // maybe we should do message passing here?
                }
            }

            allDone = true;
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
                        if (PolygonContainment.Contains(polygon, node))
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
        public List<TargetableWay> ConsolidateWays(IEnumerable<TargetableWay> ways)
        {
            var wayDictionary = new Dictionary<string, Dictionary<string, List<TargetableWay>>>(); // relationId => (wayName => Ways)

            foreach (var way in ways)
            {
                var wayName = way.Name;

                var relation = way.RegionId;
                if (!wayDictionary.ContainsKey(relation))
                {
                    wayDictionary.Add(relation, new Dictionary<string, List<TargetableWay>>());
                }
                if (!wayDictionary[relation].ContainsKey(wayName))
                {
                    wayDictionary[relation][wayName] = new List<TargetableWay>();
                }
                wayDictionary[relation][wayName].Add(way);
            }

            var newWays = new List<TargetableWay>();
            foreach ((var relationId, var waysByName) in wayDictionary)
            {
                foreach ((var wayName, var innerWays) in waysByName)
                {
                    var firstWay = innerWays.First();
                    foreach (var innerWay in innerWays.Skip(1))
                    {
                        firstWay.Merge(innerWay);
                    }
                    newWays.Add(firstWay);
                }
            }

            return newWays;
        }

        /// <summary>
        /// Any TargetableWay that has too many sub ways should be divided.
        /// </summary>
        /// <param name="originalTargetableWays"></param>
        /// <returns></returns>
        private IEnumerable<TargetableWay> UnconsolidateLargeWays(List<TargetableWay> originalTargetableWays)
        {
            var maxWaysInTargetableWay = RouteCleanerSettings.GetInstance().MaxNumberOfWaysToConsolidate;
            foreach (var targetableWay in originalTargetableWays) {
                if (targetableWay.OriginalWays.Count > maxWaysInTargetableWay)
                {
                    var i = 1;
                    foreach (var way in targetableWay.OriginalWays)
                    {
                        // todo - be smarter about this. Build connected components then return those subject to max.
                        yield return new TargetableWay
                        {
                            Id = Guid.NewGuid().ToString(),
                            Name = $"{targetableWay.Name}_{i++}",
                            OriginalWays = new List<TargetableWay.OriginalWay> { way },
                            RegionId = targetableWay.RegionId,
                            RegionName = targetableWay.RegionName,
                        };
                    }
                } 
                else
                {
                    yield return targetableWay;
                }
            }

        }

        private Geometry GetRegionGeometry(string filePath, bool ignoreNodes, bool trimTags)
        {
            var watch = Stopwatch.StartNew();
            var osmDeserializer = new OsmDeserializer(true);
            Geometry relationRegion;
            using (var fs = File.OpenRead(filePath))
            {
                using (var sr = new StreamReader(fs))
                {
                    Console.WriteLine($"Loading regions from {filePath}.");
                    relationRegion = osmDeserializer.ReadFile(sr, ignoreNodes, trimTags);
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

        private Dictionary<Relation, Polygon[]> CreateRelationPolygons(IEnumerable<Relation> relations)
        {
            var watch = Stopwatch.StartNew();
            var retVal = relations.AsParallel().ToDictionary(k => k, v => RelationPolygonMemoizer.BuildPolygons(v));
            var time = watch.Elapsed;
            Console.WriteLine($"Done building polygons in {time}");
            return retVal;
        }
    }
}
