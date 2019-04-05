using System.Collections.Generic;

namespace RouteFinder.RouteInspection.Auxilary
{
    /// <summary>
    /// Auxilary graph of trees
    /// </summary>
    internal class AuxilaryGraph<T>
    {
        private Dictionary<TreeNode<T>, LinkedList<TreeEdge<T>>> _adjacencyLists;

    }

}
