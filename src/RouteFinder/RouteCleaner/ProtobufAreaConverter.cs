using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using GlobalSettings;
using Google.OpenLocationCode;
using Newtonsoft.Json;
using RouteFinderDataModel;
using RouteFinderDataModel.Proto;

namespace RouteCleaner
{
    public class ProtobufAreaConverter
    {
        public static IEnumerable<(string, List<LookupNode>)> CreateLookupNodeProtobufs()
        {
            var folder = RouteCleanerSettings.GetInstance().TemporaryNodeWithContainingWayOutLocation;
            var allFiles = Directory.GetFiles(folder);
            foreach (var file in allFiles)
            {
                Console.WriteLine($"Working on {file}");
                var outputs = new Dictionary<string, List<LookupNode>>();
                string line;
                var sr = new StreamReader(file);
                while ((line = sr.ReadLine()) != null)
                {
                    var node = JsonConvert.DeserializeObject<Node>(line);
                    var location = new OpenLocationCode(node.Latitude, node.Longitude, codeLength: 6);
                    if (!outputs.ContainsKey(location.Code))
                    {
                        outputs.Add(location.Code, new List<LookupNode>());
                    }

                    var lNode = new LookupNode
                    {
                        Id = node.Id,
                        Latitude = node.Latitude,
                        Longitude = node.Longitude
                    };
                    lNode.Relations.AddRange(node.Relations);
                    lNode.TargetableWays.AddRange(node.ContainingWays);
                    outputs[location.Code].Add(lNode);
                }

                foreach (var kvp in outputs)
                {
                    kvp.Value.Sort(SortNodesByLatLong);
                    yield return (kvp.Key, kvp.Value);
                }
            }
        }

        public static IEnumerable<(string, List<LookupTargetableWay>)> CreateWayProtobufs()
        {
            var wayContents = File.ReadAllText(RouteCleanerSettings.GetInstance().TemporaryTargetableWaysLocation);
            var outputs = new Dictionary<string, List<LookupTargetableWay>>();

            var targetableWays = JsonConvert.DeserializeObject<List<TargetableWay>>(wayContents);

            foreach (var way in targetableWays)
            {
                var keys = way.OriginalWays.SelectMany(x => x.Points).Select(point => OpenLocationCode.Encode(point.Latitude, point.Longitude, codeLength: 6)).Distinct();

                var lookupTargetableWay = new LookupTargetableWay
                {
                    Id = way.Id,
                    Relation = way.RelationId,
                    RelationName = way.RelationName
                };
                lookupTargetableWay.OriginalWays.AddRange(way.OriginalWays.Select(x => {
                    var l = new LookupOriginalWay
                    {
                        Id = x.Id
                    };
                    l.NodeIds.AddRange(x.Points.Select(xx => xx.Id));
                    l.NodeLatitudes.AddRange(x.Points.Select(xx => xx.Latitude));
                    l.NodeLongitudes.AddRange(x.Points.Select(xx => xx.Longitude));
                    return l;
                }));

                foreach (var key in keys)
                {
                    if (!outputs.ContainsKey(key))
                    {
                        outputs.Add(key, new List<LookupTargetableWay>());
                    }

                    outputs[key].Add(lookupTargetableWay);
                }
            }

            foreach (var kvp in outputs)
            {
                yield return (kvp.Key, kvp.Value);
            }
        }

        private static int SortNodesByLatLong(LookupNode l, LookupNode r)
        {
            if (l.Latitude < r.Latitude)
            {
                return -1;
            }
            if (l.Latitude > r.Latitude)
            {
                return 1;
            }
            if (l.Longitude < r.Longitude)
            {
                return -1;
            }
            if (l.Longitude > r.Longitude)
            {
                return 1;
            }
            return 0;
        }
    }
}
