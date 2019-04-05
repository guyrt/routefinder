using System;
using System.Collections.Generic;

namespace RouteFinder.RouteInspection
{
    /// <summary>
    /// A PseudoNode is either a singleton node of type T or a odd-sized collection of PseudoNodes.
    /// </summary>
    internal class PseudoNode<T>
    {
        private List<PseudoNode<T>> _childrenNodes;

        private T _originalValue;

        public Labels Label;

        public PseudoNode(T originalValue)
        {
            _childrenNodes = new List<PseudoNode<T>>();
            // at initialization, each node is a tree by itself. It is therefore positive.
            Label = Labels.Positive;
            Ydual = double.MaxValue;
            _originalValue = originalValue;

            TreeId = Guid.NewGuid();
        }

        public double Ydual { get; set; }

        public bool IsBlossom => _childrenNodes.Count > 0;

        public Guid TreeId { get; set; }

        public T OriginalValue
        {
            get
            {
                if (!IsBlossom)
                {
                    return _originalValue;
                }
                throw new InvalidOperationException($"Tried to get original value on a blossom");
            }
        }

        internal enum Labels
        {
            Free = 0,
            Positive = 0,
            Negative = 0
        }
    }
}
