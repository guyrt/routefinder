﻿namespace RouteFinder
{
    /// <summary>
    /// Node in an adjancency list. Tracks at minimum a node and the weight to that node from the source.
    /// </summary>
    public class WeightedAdjacencyNode<T>
    {
        public T Vertex { get; }

        public double Distance { get; }

        public double Weight { get; }

        public int Count { get; set; }

        /// <summary>
        /// Flag that allows iteration to identify unique undirected edges. If an edge connects V1 to V2, it will
        /// show up in both adjancency lists. This flag will be true in only one arbitrarily chosen WeightedAdjacencyNode.
        /// </summary>
        public bool PrimaryCopy { get; set; }

        public bool MustHit { get; }

        public WeightedAdjacencyNode(T vertex, double distance, double weight = 0, bool mustHit = true, bool primaryCopy = true)
        {
            Vertex = vertex;
            Distance = distance;
            Weight = weight;
            PrimaryCopy = primaryCopy;
            MustHit = mustHit;
            Count = mustHit ? 1 : 0;
        }

        public override string ToString()
        {
            return $"WeightedAdjNode: {Vertex} - {Distance} - {Count}";
        }
    }
}
