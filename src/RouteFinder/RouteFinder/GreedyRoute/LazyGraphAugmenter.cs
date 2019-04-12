using System;
using System.Collections.Generic;
using System.Linq;

namespace RouteFinder.GreedyRoute
{
    /// <summary>
    /// For each node, compute the cost from that node to the nearest 3 nodes.
    /// Iteratively, match nodes where the cost of best match is biggest compared to cost of next two best matches.
    /// </summary>
    public class LazyGraphAugmenter<T> : IEulerianGraphAugmenter<T>
    {
        private int _comparisonSetSize = 3;

        public Graph<T> AugmentGraph(Graph<T> graph)
        {
            var extraWeight = 0.0;
            var oddDegreeNodes = graph.OddDegreeNodes().Intersect(graph.MustHitVertices).ToHashSet();
            var rankedVertices = GetRankedVertices(oddDegreeNodes, graph);

            var s = rankedVertices.Where(x => (string)(x.Value.Start.GetType().GetProperty("Id").GetValue(x.Value.Start)) == "53188016");

            while (oddDegreeNodes.Any())
            {
                var top = rankedVertices.First();
                var start = top.Value.Start;
                var end = top.Value.End;
                if (!oddDegreeNodes.Contains(start))
                {
                    // it was previously matched. Just remove it.
                    rankedVertices.Remove(top.Key);
                    continue;
                }
                if (!oddDegreeNodes.Contains(end))
                {
                    // The target is missing. Drop the current entry, get a new one, and return.
                    rankedVertices.Remove(top.Key);
                    var heapNode = GetHeapNode(start, oddDegreeNodes, graph);
                    InsertIntoSortedDictionary(rankedVertices, heapNode);
                    continue;
                }
                // both start and end can be used. Augment the path and drop both.

                oddDegreeNodes.Remove(start);
                oddDegreeNodes.Remove(end);
                var path = top.Value.Path;
                extraWeight += graph.AddEdgeCardinality(path);
            }

            Console.WriteLine($"LazyGraphAugmenter added {extraWeight}");
            return graph;
        }

        private SortedDictionary<double, HeapNode> GetRankedVertices(HashSet<T> oddDegreeNodes, Graph<T> graph)
        {
            var heap = new SortedDictionary<double, HeapNode>();

            foreach (var oddDegreeNode in oddDegreeNodes)
            {
                var heapNode = GetHeapNode(oddDegreeNode, oddDegreeNodes, graph);
                InsertIntoSortedDictionary(heap, heapNode);
            }

            return heap;
        }

        private void InsertIntoSortedDictionary(SortedDictionary<double, HeapNode> heap, HeapNode heapNode)
        {
            while (!heap.TryAdd(heapNode.Cost, heapNode))
            {
                heapNode.Cost += 1e-12;
            }
        }

        private HeapNode GetHeapNode(T oddDegreeNode, IEnumerable<T> oddDegreeNodes, Graph<T> graph)
        {
            var adsp = new AllDestinationShortestPaths<T>(oddDegreeNode, oddDegreeNodes, graph);
            adsp.Run(_comparisonSetSize);
            var costDict = adsp.LowestCost;
            if (costDict.ContainsKey(oddDegreeNode))
            {
                costDict.Remove(oddDegreeNode);
            }

            var costs = costDict.Values.ToList();
            costs.Sort();
            var firstChoiceSavings = costs.Count > 1 ? costs[0] - costs.Skip(1).Average() : 0;
            var firstChoice = costDict.First(kvp => Math.Abs(kvp.Value - costs.First()) < 1e-12).Key;
            var path = adsp.GetPath(oddDegreeNode, firstChoice);
            var heapNode = new LazyGraphAugmenter<T>.HeapNode
            {
                Cost = firstChoiceSavings,
                End = firstChoice,
                Path = path,
                Start = oddDegreeNode
            };
            return heapNode;
        }

        private class HeapNode
        {
            public T Start { get; set; }
            public T End { get; set; }
            public double Cost { get; set; }
            public List<T> Path { get; set; }

            public override string ToString()
            {
                return Start.ToString();
            }
        }
    }
}
