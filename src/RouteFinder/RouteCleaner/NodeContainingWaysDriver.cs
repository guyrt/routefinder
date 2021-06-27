using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using GlobalSettings;
using Google.OpenLocationCode;
using Newtonsoft.Json;
using RouteFinderDataModel;

namespace RouteCleaner
{
    /// <summary>
    /// Logic to read nodes with relations and add containing ways.
    /// </summary>
    public class NodeContainingWaysDriver
    {
        public void ProcessNodes()
        {
            Console.WriteLine($"Starting NodeContainingWaysDriver");
            var watch = Stopwatch.StartNew();
            var nodeWayMap = BuildWayMap();
            var time = watch.Elapsed;
            Console.WriteLine($"Finished building way map in {time}");
            watch.Restart();
            WriteStreamedNodes(nodeWayMap);

        }

        private void WriteStreamedNodes(Dictionary<string, HashSet<string>> wayMap)
        {
            using (var disposableDict = new DisposableDictionary<string, StreamWriter>())
            {
                StreamNodes(wayMap, disposableDict);
            }
        }

        private StreamWriter GetStreamWriter(string key)
        {
            System.IO.Directory.CreateDirectory(RouteCleanerSettings.GetInstance().TemporaryNodeWithContainingWayOutLocation);
            var fullPath = Path.Combine(RouteCleanerSettings.GetInstance().TemporaryNodeWithContainingWayOutLocation, key + ".json");
            Console.WriteLine($"Opening path {fullPath}");
            var fs = File.Open(fullPath, FileMode.Append, FileAccess.Write, FileShare.Read);
            return new StreamWriter(fs, Encoding.UTF8, 65536);
        }

        private void StreamNodes(Dictionary<string, HashSet<string>> wayMap, DisposableDictionary<string, StreamWriter> streamWriters)
        {
            var lineCntr = 0;
            using (var fs = File.Open(RouteCleanerSettings.GetInstance().TemporaryNodeOutLocation, FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read))
            {
                using (var sr = new StreamReader(fs))
                {
                    while(sr.Peek() >= 0)
                    {
                        var content = sr.ReadLine();
                        try
                        {
                            var node = JsonConvert.DeserializeObject<Node>(content);
                            if (wayMap.TryGetValue(node.Id, out var ways))
                            {
                                node.ContainingWays = ways.ToList();
                            }

                            var code = new OpenLocationCode(node.Latitude, node.Longitude, codeLength: 2);
                            if (!streamWriters.ContainsKey(code.Code))
                            {
                                streamWriters.Add(code.Code, GetStreamWriter(code.Code));
                            }
                            streamWriters[code.Code].WriteLine(JsonConvert.SerializeObject(node));
                        } 
                        catch(JsonReaderException e)
                        {
                            Console.WriteLine($"Could not deserialize line {lineCntr} {content}: {e.Message}");
                        }
                        catch (JsonSerializationException e)
                        {
                            Console.WriteLine($"Could not deserialize line {lineCntr} {content}: {e.Message}");
                        }
                        lineCntr++;
                    }
                }
            }
        }

        private Dictionary<string, HashSet<string>> BuildWayMap()
        {
            var lineCnt = 0;
            var nodeMap = new Dictionary<string, HashSet<string>>(); // node.id => {way.id}
            using (var fs = File.Open(RouteCleanerSettings.GetInstance().TemporaryTargetableWaysLocation, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (var sr = new StreamReader(fs))
                {
                    while (sr.Peek() >= 0)
                    {
                        var content = sr.ReadLine();
                        var targetableWay = JsonConvert.DeserializeObject<TargetableWay>(content);
                        foreach (var originalWay in targetableWay.OriginalWays)
                        {
                            foreach (var node in originalWay.Points)
                            {
                                if (!nodeMap.ContainsKey(node.Id))
                                {
                                    nodeMap.Add(node.Id, new HashSet<string>());
                                }
                                if (!nodeMap[node.Id].Contains(targetableWay.Id))
                                {
                                    nodeMap[node.Id].Add(targetableWay.Id);
                                }
                            }
                        }
                        lineCnt++;
                    }
                }
            }
            return nodeMap;
        }
    }
}
