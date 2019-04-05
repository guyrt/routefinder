namespace RouteFinder.RouteInspection.Auxilary
{
    /// <summary>
    /// Node wrapper around a <see cref="Tree{T}"/> that also includes prioritity queues of
    /// Nodes in tree to other nodes.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class TreeNode<T>
    {
        private Tree<T> _tree;

        public TreeNode(Tree<T> tree)
        {
            _tree = tree;
        }
    }
}