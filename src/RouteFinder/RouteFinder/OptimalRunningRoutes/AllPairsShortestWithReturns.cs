namespace RouteFinder.OptimalRunningRoutes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class AllPairsShortestWithReturns<T>
    {
        private readonly Graph<T> _graph;

        // Place bounds around the outbound distance. Otherwise, routes will tend to 
        private readonly double MinDistanceOutbound = 0.40;
        private readonly double MaxDistanceOutbound = 0.6;


        public AllPairsShortestWithReturns(Graph<T> graph)
        {
            _graph = graph;
        }

        public List<T> GetRoutes(T startingPlace, double maxDistance)
        {
            var potentialRoutes = new List<WeightedAdjacencyNode<T>[]>();

            // step 1: get all destinations shortest path
            // get distance-based shortest paths
            // limit to 0.4 to 0.6 of total distance
            // get weight-based shortest paths to those nodes
            Func<T, WeightedAdjacencyNode<T>, double> basicWeightCostFunction = (_, x) => x.Weight;
            var adspDistance = new AllDestinationShortestPaths<T>(startingPlace, _graph.Neighbors.Keys, _graph, (_, x) => x.Distance);
            adspDistance.RunWithDistanceCap(maxDistance * MaxDistanceOutbound);
            var targets = adspDistance.LowestCost.Where(kvp => kvp.Value > maxDistance * MinDistanceOutbound).Select(k => k.Key);
            var adspWeight = new AllDestinationShortestPaths<T>(startingPlace, targets, _graph, basicWeightCostFunction);
            adspWeight.Run();

            // step 2: for each destination, try to find a return trip. double weight any segment that was previously used.
            var currentMinDistance = double.MaxValue;
            List<T> fullPath = new List<T>();
            foreach (var potentialDestinationKvp in adspWeight.LowestCost)
            {
                // early stop: return trip will always be more expensive, so quit if over half of total best cost.
                if (currentMinDistance * .5 < potentialDestinationKvp.Value)
                {
                    break;
                }

                // double the weights
                var endPosition = potentialDestinationKvp.Key;
                var reversePaths = ReverseWeights(endPosition, adspWeight.TraversalPath);
                // get weight-based shortest path back
                var adspReversed = new AllDestinationShortestPaths<T>(endPosition, new[] { startingPlace }, _graph, GetCostFunction(basicWeightCostFunction, reversePaths));
                adspReversed.Run();
                var reversePathsReturnTrip = ReverseWeights(startingPlace, adspReversed.TraversalPath);

                var totalCost = potentialDestinationKvp.Value + adspReversed.LowestCost.Values.First();
                if (currentMinDistance > totalCost)
                {
                    currentMinDistance = totalCost;
                    fullPath = adspWeight.GetPath(startingPlace, endPosition);
                    fullPath.Reverse();
                    var t = adspReversed.GetPath(endPosition, startingPlace);
                    t.Reverse();
                    fullPath.AddRange(t);
                }
            }

            Console.WriteLine(currentMinDistance);


            return fullPath;
        }

        private Dictionary<T, T> ReverseWeights(T start, Dictionary<T, T> traversalPath)
        {
            var traversalPairs = new Dictionary<T, T>();
            T nextNode;
            while (traversalPath.ContainsKey(start))
            {
                nextNode = traversalPath[start];
                traversalPairs.Add(start, nextNode);
                start = nextNode;
            }
            return traversalPairs;
        }

        private Func<T, WeightedAdjacencyNode<T>, double> GetCostFunction(Func<T, WeightedAdjacencyNode<T>, double> basicCost, Dictionary<T, T> doubleWeightedEdges)
        {
            return (node, weighted) => (doubleWeightedEdges.ContainsKey(node) && doubleWeightedEdges[node].Equals(weighted.Vertex) ? 2.0 : 1.0)
                * basicCost(node, weighted);
        }
    }
}
