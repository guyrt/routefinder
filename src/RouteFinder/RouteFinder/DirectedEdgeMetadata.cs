using System;
using System.Collections;
using System.Collections.Generic;

namespace RouteFinder
{
    /// <summary>
    /// Collection of edges in undirected graph with a weight.
    ///
    /// Allows specification of node type and weight type.
    /// </summary>
    public class DirectedEdgeMetadata<TKey, TValue> : IEnumerable<KeyValuePair<Tuple<TKey, TKey>, TValue>>
    {
        private readonly Dictionary<Tuple<TKey, TKey>, TValue> _edgeWeights;

        /// <summary>
        /// Ranker for vertex class allows for a normalized storage format for an edge.
        /// </summary>
        private readonly IComparer<TKey> _vertexRanker;

        public DirectedEdgeMetadata(IComparer<TKey> vertexRanker)
        {
            _vertexRanker = vertexRanker;
            _edgeWeights = new Dictionary<Tuple<TKey, TKey>, TValue>();
        }

        public bool ContainsKey(TKey n1, TKey n2)
        {
            var k = NormalizedKey(n1, n2);
            return _edgeWeights.ContainsKey(k);
        }

        public void Add(TKey n1, TKey n2, TValue value)
        {
            var k = NormalizedKey(n1, n2);
            if (_edgeWeights.TryGetValue(k, out var oldValue))
            {
                if (!oldValue.Equals(value))
                {
                    throw new InvalidOperationException(
                        $"Tried to re-add edge with different value: {n1} {n2} {value} {oldValue}");
                }
            }
            else
            {
                _edgeWeights.Add(k, value);
            }
        }

        public TValue this[TKey n1, TKey n2]
        {
            get
            {
                var k = NormalizedKey(n1, n2);
                return _edgeWeights[k];
            }

            set
            {
                var k = NormalizedKey(n1, n2);
                _edgeWeights[k] = value;
            }
        }

        protected Tuple<TKey, TKey> NormalizedKey(TKey n1, TKey n2)
        {
            if (_vertexRanker.Compare(n1, n2) > 0)
            {
                return new Tuple<TKey, TKey>(n2, n1);
            }
            return new Tuple<TKey, TKey>(n1, n2);
        }

        public IEnumerator<KeyValuePair<Tuple<TKey, TKey>, TValue>> GetEnumerator()
        {
            return _edgeWeights.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
