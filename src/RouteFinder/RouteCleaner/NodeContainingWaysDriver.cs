using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using GlobalSettings;
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
            var watch = Stopwatch.StartNew();
            var nodeWayMap = BuildWayMap();
            var time = watch.Elapsed;
            Console.WriteLine($"Finished building way map in {time}");
            watch.Restart();
            WriteStreamedNodes(nodeWayMap);

        }

        private void WriteStreamedNodes(Dictionary<string, HashSet<string>> wayMap)
        {
            using (var fs = File.Open(RouteCleanerSettings.GetInstance().TemporaryNodeWithContainingWayOutLocation, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read))
            {
                using (var sw = new StreamWriter(fs, Encoding.UTF8, 65536)) // set a larger buffer
                {
                    StreamNodes(wayMap, sw);
                }
            }
        }

        private void StreamNodes(Dictionary<string, HashSet<string>> wayMap, StreamWriter sw)
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
                            sw.WriteLine(JsonConvert.SerializeObject(node));
                        } 
                        catch(JsonSerializationException e)
                        {
                            Console.WriteLine($"Could not deserialize {content}: {e.Message}");
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
