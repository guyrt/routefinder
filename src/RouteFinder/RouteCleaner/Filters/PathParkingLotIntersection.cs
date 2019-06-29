using RouteCleaner.Model;
using System.Collections.Generic;
using System.Linq;

namespace RouteCleaner.Filters
{
    /// <summary>
    /// Identify any trails that intersection parking lots.
    /// </summary>
    public class PathParkingLotIntersection
    {

        public Dictionary<Way, List<Way>> FindParkingLotsWithIntersections(Geometry geometry)
        {
            var sharedNodes = FindWithNodeShared(geometry);
            var overlapInSpace = FindIntersectingInSpace(geometry);
            foreach (var kvp in overlapInSpace)
            {
                if (sharedNodes.ContainsKey(kvp.Key))
                {
                    sharedNodes[kvp.Key].AddRange(kvp.Value);
                } else
                {
                    sharedNodes.Add(kvp.Key, kvp.Value);
                }
            }
            return sharedNodes;
        }

        /// <summary>
        /// Two pass algorithm. First, find any overlapping nodes between paths and parking lot bounding boxes.
        /// 
        /// Then see if there is true intersection.
        /// 
        /// Builds a R* tree along way (one day...)
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        private Dictionary<Way, List<Way>> FindIntersectingInSpace(Geometry geometry)
        {
            var paths = geometry.Ways.Where(w => w.FootTraffic());
            var pathNodes = CreatePathNodeLookup(paths);
            var sortedPathNodes = pathNodes.Keys.OrderBy(x => x.Latitude).ToArray();
            var parkingLots = geometry.Ways.Where(w => w.IsParkingLot());

            var retDict = new Dictionary<Way, List<Way>>();

            foreach (var parkingLot in parkingLots) {
                var ways = Intersection(parkingLot, sortedPathNodes, pathNodes);
                if (ways.Count > 0)
                {
                    retDict.Add(parkingLot, ways.ToList());
                }
            }

            return retDict;
        }

        private HashSet<Way> Intersection(Way parkingLot, Node[] latSortedNodes, Dictionary<Node, List<Way>> pathNodes)
        {
            var retList = new HashSet<Way>();

            var southLat = parkingLot.Nodes.Min(n => n.Latitude);
            var northLat = parkingLot.Nodes.Max(n => n.Latitude);
            var westLng = parkingLot.Nodes.Min(n => n.Longitude);
            var eastLng = parkingLot.Nodes.Max(n => n.Longitude);

            var southLatIndex = BinaryFindNodes(latSortedNodes, southLat);
            if (southLatIndex < 0)
            {
                return retList;
            }
            var northLatIndex = BinaryFindNodes(latSortedNodes, northLat);
            if (northLatIndex >= latSortedNodes.Length) {
                return retList;
            }

            for (var i = southLatIndex; i <= northLatIndex; i++)
            {
                var targetNode = latSortedNodes[i];
                if (westLng <= targetNode.Longitude && targetNode.Longitude <= eastLng)
                {
                    // approximate hit!
                    retList.UnionWith(pathNodes[targetNode]);
                }
            }
            return retList;
        }

        private int BinaryFindNodes(Node[] latSortedNodes, double target, int left = 0)
        {
            if (target < latSortedNodes[0].Latitude)
            {
                return -1;
            }
            if (target > latSortedNodes.Last().Latitude)
            {
                return latSortedNodes.Length + 1;
            }

            left = 0;
            var right = latSortedNodes.Length - 1;
            while(right - left > 1)
            {
                var mid = (right - left) / 2 + left;
                var midValue = latSortedNodes[mid].Latitude;
                if (midValue == target)  // highly unlikely
                {
                    return mid;
                }
                else if (midValue < target)
                {
                    left = mid;
                }
                else
                {
                    right = mid;
                }
            }
            return left;
        }

        private Dictionary<Node, List<Way>> CreatePathNodeLookup(IEnumerable<Way> paths)
        {
            var pathNodeLookup = new Dictionary<Node, List<Way>>();
            foreach (var path in paths)
            {
                foreach (var node in path.Nodes)
                {
                    if (pathNodeLookup.ContainsKey(node))
                    {
                        pathNodeLookup[node].Add(path);
                    }
                    else
                    {
                        pathNodeLookup.Add(node, new List<Way> { path });
                    }
                }
            }
            return pathNodeLookup;
        }

        /// <summary>
        /// Find intersections based on shared nodes.
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        private Dictionary<Way, List<Way>> FindWithNodeShared(Geometry geometry)
        {
            var parkingTrailIntersections = new Dictionary<Way, List<Way>>();

            var parkingLotNodes = geometry.GetParkingLotNodes();

            foreach (var foottraffic in geometry.Ways.Where(w => w.FootTraffic()))
            {
                foreach (var node in new []{ foottraffic.Nodes.First(), foottraffic.Nodes.Last()})
                {
                    if (parkingLotNodes.ContainsKey(node))
                    {
                        var lot = parkingLotNodes[node];
                        if (!parkingTrailIntersections.ContainsKey(lot))
                        {
                            parkingTrailIntersections.Add(lot, new List<Way>());
                        }
                        parkingTrailIntersections[lot].Add(foottraffic);
                        break;
                    }
                }
            }

            return parkingTrailIntersections;
        }
    }
}
