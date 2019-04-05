using System;
using System.Collections.Generic;
using System.Linq;

namespace RouteFinder.RouteInspection
{
    /// <summary>
    /// Updatable graph
    /// </summary>
    internal class SurfaceGraph<T>
    {
        private readonly Dictionary<PseudoNode<T>, LinkedList<PseudoEdge<T>>> _adjacencyLists;

        // Map original nodes to the psuedonode in which they currently exist.
        private Dictionary<T, PseudoNode<T>> _originalNodes;

        private readonly Forest<T> _forest;

        public SurfaceGraph(Dictionary<T, LinkedList<WeightedAdjacencyNode<T>>> adjancencyList)
        {
            var nodes = adjancencyList.Keys;
            _originalNodes = nodes.ToDictionary(n => n, n => new PseudoNode<T>(n));
            _adjacencyLists = new Dictionary<PseudoNode<T>, LinkedList<PseudoEdge<T>>>();
            _adjacencyLists = nodes.ToDictionary(n => _originalNodes[n], _ => new LinkedList<PseudoEdge<T>>());
            // handle duplicates.
            foreach (var nodeKvp in adjancencyList)
            {
                foreach (var adjacentNode in nodeKvp.Value.Where(x => x.PrimaryCopy))
                {
                    var v1 = _originalNodes[nodeKvp.Key];
                    var v2 = _originalNodes[adjacentNode.Vertex];
                    var weight = adjacentNode.Weight;
                    var newEdge = new PseudoEdge<T>(weight, v1, v2);
                    _adjacencyLists[v1].AddLast(newEdge);
                    _adjacencyLists[v2].AddLast(newEdge);

                    // Start each dual variable to half of the min weight between the nodes.
                    v1.Ydual = Math.Min(v1.Ydual, weight / 2);
                    v2.Ydual = Math.Min(v2.Ydual, weight / 2);
                }
            }

            _forest = new Forest<T>(_originalNodes.Values);

            GreedyInitialize();
        }

        /// <summary>
        /// Finish initialization by greedily increasing dual variables until an edge loses slack.
        ///
        /// Don't combine nodes more than once.
        /// </summary>
        private void GreedyInitialize()
        {
            foreach (var kvp in _adjacencyLists)
            {
                var pNode = kvp.Key;
                var edges = kvp.Value;
                pNode.Ydual = edges.Select(e => e.Weight).Min() / 2;
            }

            var combinedEdges = new HashSet<PseudoNode<T>>();
            foreach (var kvp in _adjacencyLists)
            {
                var pNode = kvp.Key;
                var edges = kvp.Value;
                var minDual = double.MaxValue;
                PseudoEdge<T> argminDual = null;
                foreach (var e in edges)
                {
                    var localDual = e.Weight - e.GetMatchingNode(pNode).Ydual;
                    if (minDual > localDual)
                    {
                        minDual = localDual;
                        argminDual = e;
                    }
                }

                if (minDual < 1e-8 && !combinedEdges.Contains(argminDual.Nodes.Item1) && !combinedEdges.Contains(argminDual.Nodes.Item2))
                {
                    // combine tight edge
                    _forest.CombineTrees(argminDual);
                    combinedEdges.Add(argminDual.Nodes.Item1);
                    combinedEdges.Add(argminDual.Nodes.Item2);
                }

            }

        }

        /// <summary>
        /// Perform primal/dual updates.
        /// </summary>
        public void Optimize()
        {
            throw new NotImplementedException();
        }

        private bool PrimalUpdate()
        {

            return true;
        }
    }
}
