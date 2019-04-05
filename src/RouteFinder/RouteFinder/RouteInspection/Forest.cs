using System;
using System.Collections.Generic;
using System.Linq;

namespace RouteFinder.RouteInspection
{
    internal class Forest<T>
    {
        private HashSet<Tree<T>> _trees;
        private readonly Dictionary<PseudoNode<T>, Tree<T>> _nodeToTreeMap;

        public Forest(IEnumerable<PseudoNode<T>> pNodes)
        {
            _trees = new HashSet<Tree<T>>(pNodes.Select(x => new Tree<T>(x)));
            _nodeToTreeMap = _trees.ToDictionary(t => t.Root, t => t);
        }

        /// <summary>
        /// Combine two trees into one, flip matching status of edges along path between roots, and set all nodes to free
        /// </summary>
        /// <param name="newEdge"></param>
        /// <returns></returns>
        public Tree<T> CombineTrees(PseudoEdge<T> newEdge)
        {
            if (newEdge.Slack > 1e-6)
            {
                throw new InvalidOperationException($"Tried to combine trees from {newEdge} it's got slack of {newEdge.Slack}");
            }
            var nodes = newEdge.Nodes;
            var tree1 = _nodeToTreeMap[nodes.Item1];
            var tree2 = _nodeToTreeMap[nodes.Item2];
            if (tree1 == tree2)
            {
                throw new InvalidOperationException($"Tried to combine trees from {newEdge} but it's in {tree1} already.");
            }

            // combine two trees
            tree1.AddEdges(new []{newEdge});
            tree1.AddEdges(tree2.Edges);
            foreach (var edge in tree2.Edges)
            {
                _nodeToTreeMap[edge.Nodes.Item1] = tree1;
                _nodeToTreeMap[edge.Nodes.Item2] = tree2;
            }
            _trees.Remove(tree2);

            tree1.FlipPath(tree2.Root);

            tree1.SetNodesToFree();

            return tree1;
        }
    }
}
