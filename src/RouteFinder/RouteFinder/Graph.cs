using System.Collections.Generic;
using System.Linq;


namespace RouteFinder
{
    /// <summary>
    /// A graph contains a Node -> LinkedList of neighbors. All graphs are weighted, and 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Graph<T>
    {
        private readonly IComparer<T> _comparer;

        public HashSet<T> MustHitVertices { get; }

        public Dictionary<T, LinkedList<WeightedAdjacencyNode<T>>> Neighbors { get; }

        public Graph(IEnumerable<T> nodes, IComparer<T> comparer)
        {
            _comparer = comparer;

            MustHitVertices = new HashSet<T>();
            Neighbors = nodes.ToDictionary(n => n, _ => new LinkedList<WeightedAdjacencyNode<T>>());
        }

        /// <summary>
        /// Produce a graph from a subset of nodes, maintaining all edges between nodes in the set.
        /// </summary>
        /// <param name="reducedNodes"></param>
        /// <returns></returns>
        public Graph<T> ReduceToVertexSet(HashSet<T> reducedNodes)
        {
            var g = new Graph<T>(reducedNodes, _comparer);
            foreach (var vertex in Neighbors.Where(kvp => reducedNodes.Contains(kvp.Key)))
            {

                foreach (var vertex2 in vertex.Value.Where(v => reducedNodes.Contains(v.Vertex)))
                {
                    g.Neighbors[vertex.Key].AddLast(vertex2);
                }
            }

            g.MustHitVertices.UnionWith(reducedNodes);
            return g;
        }

        private bool EdgeExists(T n1, T n2)
        {
            if (!Neighbors.ContainsKey(n1) || !Neighbors.ContainsKey(n2))
            {
                return false;
            }

            return Neighbors[n1].Any(x => x.Vertex.Equals(n2));
        }

        /// <summary>
        /// Unsafe check for weight between two nodes.
        /// </summary>
        /// <param name="n1"></param>
        /// <param name="n2"></param>
        /// <returns></returns>
        public WeightedAdjacencyNode<T> GetEdge(T n1, T n2)
        {
            var weightList = Neighbors[n1];
            return weightList.First(x => x.Vertex.Equals(n2));
        }

        public bool AddEdge(T n1, T n2, double distance, double weight, bool mustHit)
        {
            if (!EdgeExists(n1, n2))
            {
                Neighbors[n1].AddLast(new WeightedAdjacencyNode<T>(n2, distance, weight, mustHit));
                Neighbors[n2].AddLast(new WeightedAdjacencyNode<T>(n1, distance, weight, mustHit, false));
                return true;
            }

            return false;
        }

        public IEnumerable<T> OddDegreeNodes()
        {
            return Neighbors.Where(kvp => kvp.Value.Select(v => v.Count).Sum() % 2 == 1).Select(kvp => kvp.Key);
        }

        public double RequiredEdgeCost()
        {
            return Neighbors.Sum(kvp => kvp.Value.Where(x => x.MustHit && x.PrimaryCopy).Select(x => x.Distance).Sum());
        }

        /// <summary>
        /// Given list of nodes with edges between them, increase the cardinality of the edge by 1.
        /// </summary>
        /// <param name="pathBetweenNodes"></param>
        public double AddEdgeCardinality(List<T> pathBetweenNodes)
        {
            var extraCost = 0.0;
            for (var i = 0; i < pathBetweenNodes.Count() - 1; i++)
            {
                extraCost += AddEdgeCardinality(pathBetweenNodes[i], pathBetweenNodes[i + 1]);
            }

            return extraCost;
        }

        private double AddEdgeCardinality(T start, T end, int delta = 1)
        {
            var n1 = Neighbors[start].First(n => n.Vertex.Equals(end));
            n1.Count += delta;
            if (n1.Count == 0)
            {
                Neighbors[start].Remove(n1);
            }

            var n2 = Neighbors[end].First(n => n.Vertex.Equals(start));
            n2.Count += delta;
            if (n2.Count == 0)
            {
                Neighbors[end].Remove(n2);
            }
            return n2.Distance;

        }

        public void ReduceEdgeCardinality(T vertex, T previousVertex)
        {
            AddEdgeCardinality(vertex, previousVertex, -1);
        }
    }
}
