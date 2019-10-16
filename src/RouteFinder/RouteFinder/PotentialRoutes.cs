using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RouteFinder
{
    /// <summary>
    /// FIFO space explorer that will produce routes.
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

        private List<WeightedAdjacencyNode<T>[]> _paths;
        private Stack<WeightedAdjacencyNode<T>> _currentStack;
        private Dictionary<WeightedAdjacencyNode<T>, int> _nodeCount; // prevents pathological paths.
        private Dictionary<T, int> _vertextCounts; 
        private int MaxTripsForSegment = 3;
        private int MaxTripsForVertex = 4;
        private int MaxTurnsPerKilometer = 4;
        private double MaxDuplicatedDistance = .55; // > .5 is meaningless.

        public List<WeightedAdjacencyNode<T>[]> GetRouteGreedy(T startingPlace, double maxDistance)
        {
            _paths = new List<WeightedAdjacencyNode<T>[]>();
            _currentStack = new Stack<WeightedAdjacencyNode<T>>();
            _currentStack.Push(new WeightedAdjacencyNode<T>(startingPlace, 0));
            _nodeCount = new Dictionary<WeightedAdjacencyNode<T>, int>();
            _vertextCounts = new Dictionary<T, int>();

            foreach (var nextNode in _graph.Neighbors[startingPlace])
            {
                Recurse(startingPlace, nextNode, maxDistance, 0.0);
            }
            return _paths;
        }

        // return is a reject depth
        private int Recurse(T startingPlace, WeightedAdjacencyNode<T> currentNode, double maxDistance, double currentDistance)
        {
            // Console.WriteLine($"{new string(' ', _currentStack.Count)}{currentNode.Vertex}");
            var rejectDepth = 0;
            if (currentDistance > maxDistance)
            {
                return rejectDepth; // base case: too far away so log a path and don't recurse.
            }
            if (_simpleDistanceCostCompute(startingPlace, currentNode.Vertex) + currentDistance > maxDistance)
            {
                _paths.Add(CreateOutAndBack(currentNode)); // one day, need to force in a node to a random turnaround.
                return rejectDepth;
            }
            if (maxDistance * MaxTurnsPerKilometer < _currentStack.Count())
            {
                return rejectDepth;
            }
            if (_nodeCount.Where(kvp => kvp.Value > 2).Select(kvp => kvp.Key.Weight * kvp.Value).Sum() > maxDistance * MaxDuplicatedDistance) {
                return rejectDepth;
            }

            PushNode(currentNode);
            foreach (var node in _graph.Neighbors[currentNode.Vertex])
            {
                if (node.Vertex.Equals(startingPlace))
                {
                    rejectDepth = AddPath(CreateLoop(currentNode)); // base case: hit start so log a path and don't recurse.
                } 
                else
                {
                    if (!TightLoop(node))
                    {
                        rejectDepth = Recurse(startingPlace, node, maxDistance, currentDistance + node.Weight);
                    }
                }
                if (rejectDepth > 0)
                {
                    break;
                }
            }
            PopNode(currentNode);
            return rejectDepth--;
        }

        // decide if this is a good path. If not, send back a >1.
        private int AddPath(WeightedAdjacencyNode<T>[] path)
        {
            _paths.Add(path);
            if (path.Any(p => p.Vertex.ToString().EndsWith("6511")))
            {
                var i = 0;
            }
            return 0;
        }

        private bool TightLoop(WeightedAdjacencyNode<T> node)
        {
            return _nodeCount.ContainsKey(node) && _nodeCount[node] + 1 > MaxTripsForSegment
                && _vertextCounts.ContainsKey(node.Vertex) && _vertextCounts[node.Vertex] + 1 > MaxTripsForVertex ;
        }

        private void PushNode(WeightedAdjacencyNode<T> currentNode)
        {
            _currentStack.Push(currentNode);
            if (!_nodeCount.ContainsKey(currentNode))
            {
                _nodeCount.Add(currentNode, 0);
            }
            _nodeCount[currentNode]++;
            if (!_vertextCounts.ContainsKey(currentNode.Vertex))
            {
                _vertextCounts.Add(currentNode.Vertex, 0);
            }
            _vertextCounts[currentNode.Vertex]++;
        }

        private void PopNode(WeightedAdjacencyNode<T> currentNode)
        {
            _currentStack.Pop();
            _nodeCount[currentNode]--;
            _vertextCounts[currentNode.Vertex]--;
        }

        private WeightedAdjacencyNode<T>[] CreateLoop(WeightedAdjacencyNode<T> lastLink)
        {
            var outPath = _currentStack.ToArray();
            return outPath;
        }

        /// <summary>
        /// Create a path by simply doing the current path then reversing it.
        /// </summary>
        /// <returns></returns>
        private WeightedAdjacencyNode<T>[] CreateOutAndBack(WeightedAdjacencyNode<T> turnaround)
        {
            var outPath = _currentStack.ToList();
            outPath.Add(turnaround);
            outPath.AddRange(_currentStack.Reverse());
            return outPath.ToArray();

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
                if (node.Vertex.Equals(startingPlace))
                {
                    // ended up back at the start.
                    var path = GetPathToStart(currentPath, startingPlace, currentNode);
                    path.AddFirst(node);
                    yield return path.ToArray();
                }
                else if (currentPath.ContainsKey(node.Vertex))
                {
                    // ended up intersecting a node previously visited.
                    var path = GetPathToStart(currentPath, startingPlace, currentNode);
                    var pathForwardToStart = GetPathToStart(currentPath, startingPlace, node.Vertex);
                    foreach (var n in pathForwardToStart)
                    {
                        path.AddLast(n);
                    }
                    currentPath[node.Vertex] = new WeightedAdjacencyNode<T>(currentNode, node.Weight);
                    yield return path.ToArray();
                }
                else
                {
                    currentPath.Add(node.Vertex, new WeightedAdjacencyNode<T>(currentNode, node.Weight));

                    foreach (var nextNode in _graph.Neighbors[node.Vertex])
                    {
                        var naiveDistanceToStart = _simpleDistanceCostCompute(nextNode.Vertex, startingPlace);
                        if (naiveDistanceToStart < maxDistance * 0.6)
                        {
                            exploreNext.Enqueue(nextNode);
                        }
                    }
                }
            }

            Console.WriteLine(i);
        }

        private LinkedList<WeightedAdjacencyNode<T>> GetPathToStart(Dictionary<T, WeightedAdjacencyNode<T>> paths, T start, T end)
        {
            var path = new LinkedList<WeightedAdjacencyNode<T>>();
            var currentNode = end;
            while (!currentNode.Equals(start))
            {
                var currentEdge = paths[currentNode];
                path.AddFirst(currentEdge);
                currentNode = currentEdge.Vertex;                
            }
            return path;
        }
    }
}
