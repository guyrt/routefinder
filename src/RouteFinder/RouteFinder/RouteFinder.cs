using System;
using System.Collections.Generic;
using System.Linq;

namespace RouteFinder
{
    public class RouteFinder<T>
    {
        private readonly IEulerianGraphAugmenter<T> _graphAugmenter;

        public RouteFinder(IEulerianGraphAugmenter<T> graphAugmenter)
        {
            _graphAugmenter = graphAugmenter;
        }

        /// <summary>
        /// Use a not very efficient method to find a path.
        /// </summary>
        /// <param name="graph"></param>
        public LinkedList<WeightedAdjacencyNode<T>> GetRoute(Graph<T> graph)
        {
            graph = _graphAugmenter.AugmentGraph(graph);

            TryGetStartingPoint(graph, out var startingPoint);
            var path = MakePath(graph, startingPoint);
            var rawPathLengths = path.Select(x => x.Distance).Sum();

            while (TryGetStartingPoint(graph, path, out var startPointNode))
            {
                var startingPointEdge = startPointNode.Value;
                var localPath = MakePath(graph, startingPointEdge);
                var insertionPoint = startPointNode.Next ?? startPointNode;
                localPath.RemoveFirst();
                rawPathLengths += localPath.Select(x => x.Distance).Sum();
                foreach (var node in localPath)
                {
                    path.AddBefore(insertionPoint, node);
                }
            }

            return path;
        }

        private bool TryGetStartingPoint(Graph<T> graph, LinkedList<WeightedAdjacencyNode<T>> currentPath, 
            out LinkedListNode<WeightedAdjacencyNode<T>> startingPoint)
        {

            for (var recentNode = currentPath.First; recentNode != null; recentNode = recentNode.Next)
            {
                var vertex = recentNode.Value.Vertex;
                foreach (var neighbor in graph.Neighbors[vertex])
                {
                    if (neighbor.Count > 0)
                    {
                        startingPoint = recentNode;
                        return true;
                    }
                }
            }

            startingPoint = default(LinkedListNode<WeightedAdjacencyNode<T>>);
            return false;
        }

        private bool TryGetStartingPoint(Graph<T> graph, out T startingPoint)
        {
            foreach (var kvp in graph.Neighbors)
            {
                foreach (var neighbor in kvp.Value)
                {
                    if (neighbor.Count > 0)
                    {
                        startingPoint = kvp.Key;
                        return true;
                    }
                }
            }

            startingPoint = default(T);
            return false;
        }

        /// <summary>
        /// Follow available paths until one returns to the starting point, using each edge at most as many
        /// times as its cardinality.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="startingPoint"></param>
        /// <returns></returns>
        private LinkedList<WeightedAdjacencyNode<T>> MakePath(Graph<T> graph, T startingPoint)
        {
            var wan = new WeightedAdjacencyNode<T>(startingPoint, 0, 0, true);
            return MakePath(graph, wan);
        }

        private LinkedList<WeightedAdjacencyNode<T>> MakePath(Graph<T> graph, WeightedAdjacencyNode<T> startingPoint)
        {
            var path = new LinkedList<WeightedAdjacencyNode<T>>(new []{startingPoint});
            var currentNode = startingPoint.Vertex;
            while (true)
            {
                var nextVertexAdjancencyNode = GetNextVertex(graph, currentNode);
                if (nextVertexAdjancencyNode == null)
                {
                    break;
                }

                path.AddLast(new WeightedAdjacencyNode<T>(nextVertexAdjancencyNode.Vertex, nextVertexAdjancencyNode.Distance, 0, nextVertexAdjancencyNode.MustHit));
                graph.ReduceEdgeCardinality(nextVertexAdjancencyNode.Vertex, currentNode);
                currentNode = nextVertexAdjancencyNode.Vertex;
            }

            if (!path.First.Value.Vertex.Equals(path.Last.Value.Vertex))
            {
                throw new InvalidOperationException(
                    "Found a path that didn't start/end at same time. This implies that there are vertices with odd edge counts.");
            }

            return path;
        }

        private WeightedAdjacencyNode<T> GetNextVertex(Graph<T> graph, T vertex)
        {
            foreach (var v in graph.Neighbors[vertex])
            {
                if (v.Count > 0)
                {
                    return v;
                }
            }

            return null;
        }
    }
}
