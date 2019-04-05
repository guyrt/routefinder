using System.Collections.Generic;

namespace RouteFinder.RouteInspection
{
    /// <summary>
    /// Tree used in the Blossom Algorithm
    /// </summary>
    internal class Tree<T>
    {
        private readonly HashSet<PseudoEdge<T>> _edges;

        public IReadOnlyCollection<PseudoEdge<T>> Edges => _edges;

        private readonly Dictionary<PseudoNode<T>, List<PseudoEdge<T>>> _adjacency;

        public PseudoNode<T> Root { get; set; }

        public Tree(PseudoNode<T> root)
        {
            _edges = new HashSet<PseudoEdge<T>>();
            _adjacency = new Dictionary<PseudoNode<T>, List<PseudoEdge<T>>>();
            Root = root;
        }

        public void AddEdges(IEnumerable<PseudoEdge<T>> newEdges)
        {
            foreach (var edge in newEdges)
            {
                _edges.Add(edge);
                var n1 = edge.Nodes.Item1;
                if (!_adjacency.ContainsKey(n1))
                {
                    _adjacency.Add(n1, new List<PseudoEdge<T>>{edge});
                }
                var n2 = edge.Nodes.Item2;
                if (!_adjacency.ContainsKey(n2))
                {
                    _adjacency.Add(n2, new List<PseudoEdge<T>> { edge });
                }
            }
        }

        public void SetNodesToFree()
        {
            foreach (var edge in _edges)
            {
                edge.Nodes.Item1.Label = PseudoNode<T>.Labels.Free;
                edge.Nodes.Item2.Label = PseudoNode<T>.Labels.Free;
            }
        }

        /// <summary>
        /// Follow path from startingNode to root, flipping all matching states on path.
        /// </summary>
        /// <param name="startingNode">Typically, startingNode is root of a tree that is being combined.</param>
        public void FlipPath(PseudoNode<T> startingNode)
        {
            InternalFlipPath(startingNode, null);
        }

        private bool InternalFlipPath(PseudoNode<T> node, PseudoNode<T> prev)
        {
            bool onPathToRoot = false;
            foreach (var pseudoEdge in _adjacency[node])
            {
                var matchingNode = pseudoEdge.GetMatchingNode(node);
                if (matchingNode == prev)
                {
                    break;
                }

                onPathToRoot |= matchingNode == Root || InternalFlipPath(matchingNode, node);
                if (onPathToRoot)
                {
                    pseudoEdge.InMatching = !pseudoEdge.InMatching;
                    return true;
                }
            }

            return false;
        }
    }
}
