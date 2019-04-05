using System;

namespace RouteFinder.RouteInspection
{
    /// <summary>
    /// Tracks an edge between psuedonodes.
    ///
    /// Since the Blossom algorithm frequently requires us to combine pseudonodes and collapse edges between them,
    /// edges track both the original nodes they combine and the pseduonodes that those nodes map to.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class PseudoEdge<T>
    {
        private T _originalNode1;
        private T _originalNode2;
        private PseudoNode<T> _currentNode1;
        private PseudoNode<T> _currentNode2;

        public bool InMatching;

        public PseudoEdge(double weight, PseudoNode<T> originalNode1, PseudoNode<T> originalNode2)
        {
            _originalNode1 = originalNode1.OriginalValue;
            _originalNode2 = originalNode2.OriginalValue;
            Weight = weight;
            _currentNode1 = originalNode1;
            _currentNode2 = originalNode2;
            InMatching = false;
        }

        public double Weight { get; }

        public double Slack => Weight - _currentNode1.Ydual - _currentNode2.Ydual;

        public Tuple<PseudoNode<T>, PseudoNode<T>> Nodes => new Tuple<PseudoNode<T>, PseudoNode<T>>(_currentNode1, _currentNode2);

        /// <summary>
        /// Get node that matches other side of n.
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public PseudoNode<T> GetMatchingNode(PseudoNode<T> n)
        {
            if (n == _currentNode1)
            {
                return _currentNode2;
            }
            if (n == _currentNode2)
            {
                return _currentNode1;
            }
            throw new ArgumentException($"{n} is not part of this edge.");
        }

    }
}
