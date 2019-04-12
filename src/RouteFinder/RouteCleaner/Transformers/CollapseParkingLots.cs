using RouteCleaner.Model;
using System.Collections.Generic;
using System.Linq;

namespace RouteCleaner.Transformers
{
    /// <summary>
    /// Since you can walk across parking lots, any path that connects directly to a parking lot as an end point 
    /// should imply that you can walk through that lot.
    /// 
    /// This class removes the lot entirely and replaces it with a node. Updates all ways that start/end at the lot.
    /// </summary>
    public class CollapseParkingLots
    {
        public Geometry Transform(Geometry geometry)
        {
            var parkingLotReplacementNodes = FindParkingLots(geometry);
            var nodes = geometry.Nodes.ToList();
            nodes.AddRange(parkingLotReplacementNodes.Keys);
            var ways = new List<Way>();
            foreach (var way in geometry.Ways)
            {
                if (way.IsParkingLot())
                {
                    ways.Add(way);
                    continue;
                }

                var newNodes = way.Nodes;
                var replaced = false;
                for (int i = 0; i < newNodes.Count(); i++)
                {
                    if (parkingLotReplacementNodes.ContainsKey(newNodes[i]))
                    {
                        replaced = true;
                        newNodes[i] = parkingLotReplacementNodes[newNodes[i]];
                    }
                }
                if (replaced)
                {
                    ways.Add(new Way(way.Id, newNodes, way.Tags));
                }
                else
                {
                    ways.Add(way);
                }
            }

            // caution: I'm not updating relations. Need to fix.
            return new Geometry(nodes.ToArray(), ways.ToArray(), geometry.Relations);
        }

        /// <summary>
        /// Find parking lots
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        private Dictionary<Node, Node> FindParkingLots(Geometry geometry)
        {
            var parkingLots = geometry.Ways.Where(w => w.IsParkingLot());
            var outputNodes = new Dictionary<Node, Node>();
            foreach (var parkingLot in parkingLots)
            {
                var latMean = parkingLot.Nodes.Select(x => x.Latitude).Average();
                var lngMean = parkingLot.Nodes.Select(x => x.Longitude).Average();
                var newNode = new Node(parkingLot.Id + "_node", latMean, lngMean, parkingLot.Tags);
                foreach (var n in parkingLot.Nodes)
                {
                    outputNodes.TryAdd(n, newNode);
                }
            }
            return outputNodes;
        }
    }
}
