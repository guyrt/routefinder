using System;
using System.Collections.Generic;
using System.Linq;

namespace RouteFinder
{
    /// <summary>
    /// Compute the shortest path between a T and all Ts in a target set.
    ///
    /// Uses Djikstra's algo with a min heap.
    /// </summary>
    public class AllDestinationShortestPaths<T>
    {
        private readonly HashSet<T> _targets;

        private readonly HashSet<T> _visited;

        private readonly PriorityQueue<(T from, T to)> _nextToVisit;

        private readonly Graph<T> _graph;

        private readonly Dictionary<T, double> _lowestCost;

        public Dictionary<T, T> TraversalPath { get; }  // value for T is previous T in shortest path. Iterating will lead back to starting node.

        private bool _ran;

        /// <summary>
        /// Used to compute distance for a WeightedAdjancencyNode
        /// </summary>
        private readonly Func<T, WeightedAdjacencyNode<T>, double> _getDistance;

        public AllDestinationShortestPaths(T start, IEnumerable<T> targets, Graph<T> graph, Func<T, WeightedAdjacencyNode<T>, double> getDistance)
        {
            _targets = new HashSet<T>(targets);
            _visited = new HashSet<T>{start};
            _graph = graph;
            _nextToVisit = new PriorityQueue<(T from, T to)>();
            _getDistance = getDistance;
            foreach (var adjancencyNode in graph.Neighbors[start])
            {
                _nextToVisit.Add((start, adjancencyNode.Vertex), _getDistance(start, adjancencyNode));
            }
            _lowestCost = new Dictionary<T, double>();
            TraversalPath = new Dictionary<T, T>();
            _lowestCost.Add(start, 0);
            _ran = false;
        }

        public void Run()
        {
            Run(-1, Double.MaxValue);
        }

        public void RunWithNumTargets(int findTargets)
        {
            Run(findTargets, Double.MaxValue);
        }

        public void RunWithDistanceCap(double distanceCap)
        {
            Run(-1, distanceCap);
        }

        private void Run(int findTargets, double distanceCap)
        {
            int localTargetsStopCount = 0;
            if (findTargets > 0)
            {
                localTargetsStopCount = _targets.Count - findTargets - 1;
            }

            var localTargets = new HashSet<T>(_targets);
            if (_ran)
            {
                return;
            }

            while (!_nextToVisit.IsEmpty && localTargets.Count > localTargetsStopCount)
            {
                var nextEdge = _nextToVisit.Dequeue();
                var toT = nextEdge.to;
                var fromT = nextEdge.from;
                if (_visited.Contains(toT))
                {
                    continue;
                }
                TraversalPath.Add(nextEdge.to, nextEdge.from);
                var weight = _lowestCost[fromT] + _getDistance(toT, _graph.GetEdge(toT, fromT));
                if (weight < distanceCap)
                {
                    _lowestCost.Add(toT, weight);
                }
                if (localTargets.Contains(toT))
                {
                    localTargets.Remove(toT);
                }
                foreach (var adjancencyNode in _graph.Neighbors[toT])
                {
                    if (!_visited.Contains(adjancencyNode.Vertex))
                    {
                        var newWeight = weight + _getDistance(toT, adjancencyNode);
                        _nextToVisit.Add((toT, adjancencyNode.Vertex), newWeight);
                    }
                }
                _visited.Add(toT);
            }

            _ran = true;
        }

        /// <summary>
        /// Return list of nodes from ending node back to the start.
        ///
        /// Assumes that the starting node for latest run was start.
        /// </summary>
        /// <returns></returns>
        public List<T> GetPath(T start, T end)
        {
            if (!_ran)
            {
                throw new InvalidOperationException("Need to run first");
            }
            var returnList = new List<T>{end};
            while (!end.Equals(start))
            {
                end = TraversalPath[end];
                returnList.Add(end);
            }

            return returnList;
        }

        public Dictionary<T, double> LowestCost => _ran ? _lowestCost.Where(kvp => _targets.Contains(kvp.Key)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value) : throw new InvalidOperationException("Need to run first");
    }
}
