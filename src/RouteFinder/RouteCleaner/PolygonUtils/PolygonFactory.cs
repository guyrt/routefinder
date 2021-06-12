namespace RouteCleaner.PolygonUtils
{
    using System.Collections.Generic;
    using System.Linq;
    using RouteFinderDataModel;

    public static class PolygonFactory
    {

        public static List<Polygon> BuildPolygons(Relation relation)
        {
            return BuildPolygons(relation.Ways, relation.InnerWays);
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
        /// May need to repeat if the circuit has fully disconnected components.
        /// 
        /// </summary>
        /// <returns></returns>
        public static List<Polygon> BuildPolygons(Way[] originalWays, HashSet<string> innerWays)
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
                    var newWay = new Way(way.Id, way.Nodes.Skip(1).ToArray(), way.Tags, way.ContainedIn, way.IsComposite);
                    wayLL.AddLast(newWay);
                    polygons.Add(new Polygon(wayLL, new List<bool> { false }, !innerWays.Contains(way.Id)));
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

                // Step 2 - build a list of Node to Node links + associated Ways + associated directions.
                var nodes = new List<Node>();
                var ways = new List<Way>();  // Way at position i has start (or end if reversed) of nodes[i].
                var reversedWay = new List<bool>();

                var node = nodeToWayMap.First().Key; // in non-reversed order, starting node is the start of first Way and end of last.
                var firstNode = node;
                nodes.Add(node);
                while (nodeToWayMap.Count > 0) // can't be infinite b/c we remove something from a value in every iteration.
                {
                    nodeToWayMap = DropWayFromMap(nodeToWayMap, node, out Way localWay);

                    ways.Add(localWay);
                    var reversed = localWay.Nodes[0] != node; // if start is node then this is a non-reversed node.
                    reversedWay.Add(reversed);
                    node = reversed ? localWay.Nodes[0] : localWay.Nodes.Last();
                    nodes.Add(node);
                    nodeToWayMap = DropWayFromMap(nodeToWayMap, node, localWay); // drop return connection.
                    remainingWays.Remove(localWay);

                    if (firstNode == node)
                    {
                        break;
                    }
                }

                // Step 3 - find/cull sets of nodes into polygons
                polygons.AddRange(FindPolygons(nodes, ways, reversedWay, innerWays));
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

        private static List<Polygon> FindPolygons(List<Node> nodes, List<Way> ways, List<bool> reversed, HashSet<string> innerWays)
        {
            var polygons = new List<Polygon>();
            var firstSeenIndex = new Dictionary<Node, int>();
            var idx = 0;
            var alreadyUsed = new bool[nodes.Count]; // if true then omit this from subsequent polygons because a closed circuit has been removed.

            foreach (var node in nodes)
            {
                if (firstSeenIndex.ContainsKey(node))
                {
                    var polygonWays = new LinkedList<Way>();
                    var reversals = new List<bool>();
                    // found a polygon from range firstSeenIndex to idx.
                    for (var j = firstSeenIndex[node]; j < idx; j++)
                    {
                        if (!alreadyUsed[j])
                        {
                            polygonWays.AddLast(ways[j]);
                            reversals.Add(reversed[j]);
                            alreadyUsed[j] = true;
                        }
                    }

                    var isOuter = false;
                    var isInner = false;
                    foreach (var way in polygonWays)
                    {
                        isOuter |= !innerWays.Contains(way.Id);
                        isInner |= innerWays.Contains(way.Id);
                    }

/*                    todo - inner/outer is unclear right now so we default to outer. See relation 140781 for example where a way is unclear (but is actually inner)
 *                    if (isOuter == isInner)
                    {
                        throw new Exception($"Created polygon with way {polygonWays.First()} that is both inner and outer.");
                    }*/  

                    polygons.Add(new Polygon(polygonWays, reversals, isOuter));
                    firstSeenIndex.Remove(node); // not strictly necessary, but this allows us to avoid recycling over some elements.
                }
                else
                {
                    firstSeenIndex.Add(node, idx);
                }

                idx++;
            }
            return polygons;
        }


    }
}
