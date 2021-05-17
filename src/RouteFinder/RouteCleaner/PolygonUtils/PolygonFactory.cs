namespace RouteCleaner.PolygonUtils
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using RouteFinderDataModel;

    public static class PolygonFactory
    {

        public static List<Polygon> BuildPolygons(Relation relation)
        {
            return BuildPolygons(relation.Ways);
        }

        /// <summary>
        /// todo:
        /// 
        /// Model ways as directed edges: no Node is used as both a first/last Node of a Way and a middle Node of some other way.
        /// 
        /// Our task is to build a connected path through all nodes that forms a circuit. This is possible if all nodes have even order.
        /// There may be sub-circuits created. In this case, carve them out (smallest first) into separate polygons.
        /// The algo is as follows:
        /// * get map from nodes to ways they reference. validate length of each way list is 0 mod 2.
        /// * create a list of ways by starting with some node. 
        /// ** Find a way that has this node and add to list of ways. if the first node in chain was end, then mark as a reversed way.
        /// ** continue adding to list of ways until you return to the first node you picked. also track the list of nodes you select in order.
        /// * collapse the list of ways to polygons. Any node listed twice in the list implies a circuit.
        /// ** Reading left to right in node array, if you find a repeated node, then create polygon from intermediate ways. remove used nodes (keep one copy of the node that you found repeated).
        /// 
        /// Pick a direction using first, arbitrary way. If subsequent nodes align with way1.end == way2.start then they are same. If backwards: way1.end == way2.end or way1.start == way2.start then mark way2 as reversed.
        /// all reversals are in reference to your first, arbitrary way.
        /// 
        /// May need to repeate if the circuit has fully disconnected components.
        /// 
        /// </summary>
        /// <returns></returns>
        public static List<Polygon> BuildPolygons(Way[] originalWays)
        {
            var polygons = new List<Polygon>();
            if (originalWays.Length == 0)
            {
                return polygons;
            }

            // Step 0: eliminate any closed ways
            var remainingWays = new HashSet<Way>();
            foreach (var way in originalWays)
            {
                if (way.Nodes.First() == way.Nodes.Last())
                {
                    var wayLL = new LinkedList<Way>();
                    wayLL.AddLast(way);
                    polygons.Add(new Polygon(wayLL));
                }
                else
                {
                    remainingWays.Add(way);
                }
            }

            while (remainingWays.Count > 0)
            {

                // Step 1 - build map from node to ways
                var nodeToWayMap = GetNodeToWayMap(remainingWays);
                ValidateNodeToWayMap(nodeToWayMap);

                // Step 2 - build a list of Node to Node links + associated Ways + associated directions.
                var nodes = new LinkedList<Node>();
                var ways = new LinkedList<Way>();  // Way at position i has start (or end if reversed) of nodes[i].
                var reversedWay = new LinkedList<bool>();

                var node = nodeToWayMap.First().Key; // in non-reversed order, starting node is the start of first Way and end of last.
                nodes.AddLast(node);
                while (nodeToWayMap.Count > 0) // can't be infinite b/c we remove something from a value in every iteration.
                {
                    nodeToWayMap = DropWayFromMap(nodeToWayMap, node, out Way localWay);

                    ways.AddLast(localWay);
                    var reversed = localWay.Nodes[0] != node; // if start is node then this is a non-reversed node.
                    reversedWay.AddLast(reversed);
                    node = reversed ? localWay.Nodes[0] : localWay.Nodes.Last();
                    nodes.AddLast(node);
                    nodeToWayMap = DropWayFromMap(nodeToWayMap, node, localWay); // drop return connection.
                    remainingWays.Remove(localWay);
                }

                // Step 3 - find/cull sets of nodes into polygons


            }

            // old

            var startToWay = new Dictionary<Node, Way>();
            foreach (var way in remainingWays)
            {
                var start = way.Nodes.First();
                if (startToWay.ContainsKey(start))
                {
                    Array.Reverse(way.Nodes);
                    start = way.Nodes.First();
                }

                startToWay.Add(start, way);
            }

            var unusedWays = new HashSet<Way>(remainingWays);
            while (unusedWays.Count > 0)
            {
                var polygonWays = new LinkedList<Way>();
                var way = unusedWays.First();
                var firstNode = way.Nodes.First();
                unusedWays.Remove(way);
                polygonWays.AddLast(way);
                while (true)
                {
                    var end = way.Nodes.Last();
                    if (end == firstNode)
                    {
                        break;
                    }

                    if (!startToWay.ContainsKey(end))
                    {
                        throw new InvalidOperationException($"Can't link node {end}");
                    }

                    way = startToWay[end];
                    polygonWays.AddLast(way);
                    unusedWays.Remove(way);
                }
                polygons.Add(new Polygon(polygonWays)); // todo - track reversals.
            }

            return polygons;
        }

        private static Dictionary<Node, LinkedList<Way>> DropWayFromMap(Dictionary<Node, LinkedList<Way>> nodeToWayMap, Node node, out Way way)
        {
            way = nodeToWayMap[node].First();
            nodeToWayMap[node].RemoveFirst();
            if (nodeToWayMap[node].Count == 0)
            {
                nodeToWayMap.Remove(node);
            }

            return nodeToWayMap;
        }

        private static Dictionary<Node, LinkedList<Way>> DropWayFromMap(Dictionary<Node, LinkedList<Way>> nodeToWayMap, Node key, Way way)
        {
            nodeToWayMap[key].Remove(way);
            if (nodeToWayMap[key].Count == 0)
            {
                nodeToWayMap.Remove(key);
            }

            return nodeToWayMap;
        }

        private static Dictionary<Node, LinkedList<Way>> GetNodeToWayMap(IEnumerable<Way> ways)
        {
            var retDict = new Dictionary<Node, LinkedList<Way>>();
            foreach (var way in ways)
            {
                foreach (var node in new[] {way.Nodes.First(), way.Nodes.Last()})
                {
                    if (!retDict.ContainsKey(node))
                    {
                        retDict.Add(node, new LinkedList<Way>());
                    }
                    retDict[node].AddLast(way);
                }
            }

            return retDict;
        }

        private static void ValidateNodeToWayMap(Dictionary<Node, LinkedList<Way>> nodeMap)
        {
            foreach (var kvp in nodeMap)
            {
                if (kvp.Value.Count % 2 != 0)
                {
                    throw new InvalidOperationException($"Node {kvp.Key} has odd number of Ways. Violates assumption of closed polygons.");
                }
            }
        }
    }
}
