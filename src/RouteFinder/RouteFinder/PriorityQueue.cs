using System.Collections.Generic;
using System.Linq;

namespace RouteFinder
{
    /// <summary>
    /// Fine for now but tends to create a lot of extra objects.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class PriorityQueue<T>
    {
        private SortedDictionary<double, Queue<T>> _pq;

        public int Count = 0;

        public PriorityQueue()
        {
            _pq = new SortedDictionary<double, Queue<T>>();
        }

        public void Add(T key, double weight)
        {
            Count++;
            if (_pq.TryGetValue(weight, out var q))
            {
                q.Enqueue(key);
            }
            else
            {
                var q2 = new Queue<T>();
                q2.Enqueue(key);
                _pq.Add(weight, q2);
            }
        }

        public T Dequeue()
        {
            Count--;
            var k = _pq.First();
            var q = k.Value;
            var value = q.Dequeue();
            if (q.Count == 0)
            {
                _pq.Remove(k.Key);
            }

            return value;
        }

        public bool IsEmpty => Count == 0;
    }
}
