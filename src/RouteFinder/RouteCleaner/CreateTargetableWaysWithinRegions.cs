using System;
using System.Collections.Generic;
using System.Linq;
using RouteFinderDataModel;
using RouteFinderDataModel.Thin;

namespace RouteCleaner
{
    /// <summary>
    /// Processes a stream of Nodes to identify when a Way has all Nodes known. Once it does, create one or more TargetableWays for that Way.
    /// </summary>
    public class CreateTargetableWaysWithinRegions
    {

        public List<TargetableWay> OutputWays;
        
        private Dictionary<string, List<Way>> wayMap; // NodeId => Ways that contain the node.

        private Dictionary<string, Relation> relationMap;

        private int counter;

        public CreateTargetableWaysWithinRegions(IEnumerable<Way> ways, IEnumerable<Relation> relations)
        {
            Initialize(ways, relations);
        }

        public void ProcessNode(Node node)
        {
            if (!this.wayMap.ContainsKey(node.Id))
            {
                return;
            }

            var remainingWays = new List<Way>();

            foreach (var way in this.wayMap[node.Id])
            {
                if (way.AddNode(node))
                {
                    // way is complete
                    this.ConvertToOutputWays(way);
                } 
                else
                {
                    remainingWays.Add(way);
                }
            }

            if (remainingWays.Count > 0)
            {
                this.wayMap[node.Id] = remainingWays;
            } 
            else
            {
                this.wayMap.Remove(node.Id);
            }

            this.counter++;

            if (this.counter % 10000 == 0)
            {
                Console.WriteLine($"After Node {this.counter}, have {this.wayMap.Count} nodes remaining and {this.wayMap.Select(w => w.Value.Count).Sum()} total way nodes to map. {this.OutputWays.Count()} ways created.");
            }
        }

        private void ConvertToOutputWays(Way way)
        {
            var activeRegions = new Dictionary<string, List<Node>>();

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
                        this.OutputWays.Add(this.MakeSingleWay(way, region, activeRegions[region]));
                        activeRegions.Remove(region);
                    }
                }
            }

            foreach (var region in activeRegions.Keys)
            {
                // the end of a way segment. Create a new Way
                this.OutputWays.Add(this.MakeSingleWay(way, region, activeRegions[region]));
            }
        }

        private TargetableWay MakeSingleWay(Way way, string region, IEnumerable<Node> regionNodes)
        {
            var newId = Guid.NewGuid().ToString();
            var newWay = new TargetableWay
            {
                Id = newId,
                Name = way.Name,
                Relation = this.relationMap[region],
                OriginalWays = new List<TargetableWay.OriginalWay>
                {
                    new TargetableWay.OriginalWay
                    {
                        Id = way.Id,
                        Points = regionNodes.Select(r => new ThinNode
                        {
                            Id = r.Id,
                            Latitude = r.Latitude,
                            Longitude = r.Longitude
                        }).ToArray()
                    }
                }
            };
            return newWay;
        }

        private void Initialize(IEnumerable<Way> ways, IEnumerable<Relation> relations)
        {
            this.counter = 0;
            this.wayMap = new Dictionary<string, List<Way>>();
            foreach (var way in ways)
            {
                foreach (var nodeId in way.NodeIds)
                {
                    if (!wayMap.ContainsKey(nodeId))
                    {
                        wayMap.Add(nodeId, new List<Way>());
                    }
                    wayMap[nodeId].Add(way);
                }
            }

            this.relationMap = relations.ToDictionary(r1 => r1.Id, r2 => r2);

            this.OutputWays = new List<TargetableWay>();
        }
    }
}
