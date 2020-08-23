using System;
using Xunit;
using RouteFinder;


namespace RouteFinderTests
{
    public class PriorityQueueTests
    {
        [Fact]
        public void InsertInCorrectOrder()
        {
            var pq = new PriorityQueue<int>();
            pq.Add(1, .1);
            pq.Add(2, .2);
            pq.Add(3, .3);
            pq.Add(4, .4);

            Assert.Equal(1, pq.Dequeue());
            Assert.Equal(2, pq.Dequeue());
            Assert.Equal(3, pq.Dequeue());
            Assert.Equal(4, pq.Dequeue());
            Assert.Throws<ArgumentOutOfRangeException>(() => pq.Dequeue());
        }

        [Fact]
        public void InsertInReverseOrder()
        {
            var pq = new PriorityQueue<int>();
            pq.Add(1, .4);
            pq.Add(2, .3);
            pq.Add(3, .2);
            pq.Add(4, .1);

            Assert.Equal(4, pq.Dequeue());
            Assert.Equal(3, pq.Dequeue());
            Assert.Equal(2, pq.Dequeue());
            Assert.Equal(1, pq.Dequeue());
            Assert.Throws<ArgumentOutOfRangeException>(() => pq.Dequeue());
        }

        [Fact]
        public void InsertTies()
        {
            var pq = new PriorityQueue<int>();
            pq.Add(1, .1 + .2);
            pq.Add(2, .2);
            pq.Add(3, .1 + .2);
            pq.Add(4, .1);

            Assert.Equal(4, pq.Dequeue());
            Assert.Equal(2, pq.Dequeue());
            Assert.Equal(1, pq.Dequeue()); // inserted first
            Assert.Equal(3, pq.Dequeue());
            Assert.Throws<ArgumentOutOfRangeException>(() => pq.Dequeue());
        }

        [Fact]
        public void InsertTiesFloatingPointEquality()
        {
            // test for inexact floating point.
            var pq = new PriorityQueue<int>();
            Assert.False(.1 + .2 == .3);
            pq.Add(1, .1 + .2);
            pq.Add(2, .3);
            pq.Add(3, .1 + .2);

            Assert.Equal(1, pq.Dequeue());
            Assert.Equal(2, pq.Dequeue());
            Assert.Equal(3, pq.Dequeue());
            Assert.Throws<ArgumentOutOfRangeException>(() => pq.Dequeue());
        }
    }
}
