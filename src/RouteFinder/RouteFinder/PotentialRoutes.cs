using System;
using System.Collections.Generic;

namespace RouteFinder
{
    /// <summary>
    /// LIFO space explorer that will produce routes.
    /// 
    /// Stores unique loops.
    /// </summary>
    public class PotentialRoutes<T>
    {
        private readonly Graph<T> _graph;
        private readonly Func<T, T, double> _simpleDistanceCostCompute;

        public PotentialRoutes(Graph<T> graph, Func<T, T, double> simpleDistanceCostCompute)
        {
            _graph = graph;
            _simpleDistanceCostCompute = simpleDistanceCostCompute;
        }

        public IEnumerable<WeightedAdjacencyNode<T>[]> GetRoutes(T startingPlace, double maxDistance)
        {
            var exploreNext = new Queue<WeightedAdjacencyNode<T>>();
            foreach (var nextNode in _graph.Neighbors[startingPlace])
            {
                exploreNext.Enqueue(nextNode);
            }
            var currentPath = new Dictionary<T, WeightedAdjacencyNode<T>>();
            var loops = new List<List<T>>();

            var currentNode = startingPlace;
            var i = 0;
            while (exploreNext.Count > 0)
            {
                var node = exploreNext.Dequeue();
                
                if (currentPath.ContainsKey(node.Vertex))
                {
                    currentPath[node.Vertex] = new WeightedAdjacencyNode<T>(currentNode, node.Weight);
                }
                else
                {
                    currentPath.Add(node.Vertex, new WeightedAdjacencyNode<T>(currentNode, node.Weight));
                }

                if (node.Vertex.Equals(startingPlace))
                {
                    i++;
                    if (i % 10000 == 0)
                    {
                        Console.WriteLine(i);
                    }
                    // trace path, returning nodes.
                    yield return null;
                }
                else
                {
                    foreach (var nextNode in _graph.Neighbors[node.Vertex])
                    {
                        if (_simpleDistanceCostCompute(nextNode.Vertex, node.Vertex) < maxDistance)
                        {
                            exploreNext.Enqueue(nextNode);
                        }
                    }
                }
            }

            Console.WriteLine(i);
        }
    }
}
