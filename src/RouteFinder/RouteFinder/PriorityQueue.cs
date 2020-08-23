using System;
using System.Collections.Generic;
using System.Linq;


namespace RouteFinder
{
    /// <summary>
    /// Fine for now but tends to create a lot of extra objects.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PriorityQueue<T>
    {

        private readonly List<Tuple<double, int, T>> _heap;

        private int _totalAdditions;

        // if abs(d1 - d2) < _epsilon then they are same.
        private double _epsilon = 1e-10;

        public PriorityQueue()
        {
            _heap = new List<Tuple<double, int, T>>();
            _totalAdditions = 0;
        }

        public void Add(T key, double weight)
        {
            _heap.Add(new Tuple<double, int, T>(weight, _totalAdditions++, key));
            this.HeapifyUp(_heap.Count() - 1);
        }

        /// <summary>
        /// An element was inserted. Promote towards the head.
        /// </summary>
        private void HeapifyUp(int idx)
        {
            if (idx == 0)
            {
                return;
            }
            int parentIdx = (idx - 1) / 2;
            var parent = _heap[parentIdx];
            var local = _heap[idx];
            if (Compare(parent, local) > 0)
            {
                // swap
                Swap(idx, parentIdx);
                HeapifyUp(parentIdx);
            }
        }

        /// <summary>
        /// An element was inserted to head (during pop). Swap with smaller child.
        /// </summary>
        /// <param name="idx"></param>
        private void HeapifyDown(int idx)
        {
            int leftChildIdx = idx * 2 + 1;
            int rightChildIdx = leftChildIdx + 1;
            
            if (leftChildIdx >= _heap.Count)
            {
                return;
            }
            var leftChild = _heap[leftChildIdx];
            var target = _heap[idx];
            bool leftSmaller = Compare(leftChild, target) < 0;
            bool rightSmaller = false;
            if (rightChildIdx < _heap.Count)
            {
                rightSmaller = Compare(_heap[rightChildIdx], target) < 0;
            }
            if (leftSmaller || rightSmaller)
            {
                bool swapRight = rightSmaller && Compare(_heap[rightChildIdx], leftChild) < 0;
                if (swapRight)
                {
                    Swap(idx, rightChildIdx);
                    HeapifyDown(rightChildIdx);
                }
                else
                {
                    Swap(idx, leftChildIdx);
                    HeapifyDown(leftChildIdx);
                }
            }
        }

        // Assume this is called from context where i and j are valid indices.
        private void Swap(int i, int j)
        {
            var tmp = _heap[i];
            _heap[i] = _heap[j];
            _heap[j] = tmp;
        }

        /// <summary>
        /// Return -1 if left is less than right and +1 if right is less than left.
        /// 
        /// Assumes that ties are impossible.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        private int Compare(Tuple<double, int, T> left, Tuple<double, int, T> right)
        {
            double raw = left.Item1 - right.Item1;
            if (raw < -_epsilon)
            {
                return -1;
            }
            if (raw > _epsilon)
            {
                return 1;
            }
            if (left.Item2 < right.Item2)
            {
                return -1;
            }
            return 1;
        }

        /// <summary>
        /// Return current min item, respecting insert order on ties.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Why it's thrown.</exception>
        /// <returns></returns>
        public T Dequeue()
        {
            // n.b. throws IndexOfRangeException
            var currentMinValue = _heap[0].Item3;

            var lastValue = _heap[_heap.Count - 1];
            _heap[0] = lastValue;
            _heap.RemoveAt(_heap.Count - 1); // n.b. this is O(1) for last element.
            if (_heap.Count > 0)
            {
                HeapifyDown(0);
            }

            return currentMinValue;
        }

        public bool IsEmpty => _heap.Count == 0;
    }
}
