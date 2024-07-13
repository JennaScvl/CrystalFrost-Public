using System;
using System.Collections.Concurrent;

namespace CrystalFrost.Lib
{
    /// <summary>
    /// Provides an Abstraction around ConcurrentQueue
    /// </summary>s
    /// <typeparam name="T"></typeparam>
    public interface IConcurrentQueue<T>
    {
        /// <summary>
        /// How many <typeparamref name="T"/> are waiting.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Adds a <typeparamref name="T"/> to the queue
        /// </summary>
        /// <param name="item"></param>
        void Enqueue(T item);

        /// <summary>
        /// Gets a <typeparamref name="T"/> from the queue.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        bool TryDequeue(out T item);

        /// <summary>
        /// An event that is fired whenever an item is added to the queue.
        /// </summary>
        event Action<T> ItemEnqueued;

        /// <summary>
        /// An event that is fired whenever an item is removed from the queue.
        /// </summary>
        event Action<T> ItemDequeued;

    }

    public class AbstractedConcurrentQueue<T> : IConcurrentQueue<T>
    {
        private readonly ConcurrentQueue<T> _queue = new();
		// private readonly ConcurrentStack<T> _stack = new();

        public event Action<T> ItemEnqueued;
        public event Action<T> ItemDequeued;

        public void Enqueue(T item)
        {
            _queue.Enqueue(item);
		    //_stack.Push(item);
            ItemEnqueued?.Invoke(item);
        }

        public bool TryDequeue(out T item)
        {
            var result = _queue.TryDequeue(out item);
			//var result = _stack.TryPop(out item);
            if (result && item is not null)
            {
                ItemDequeued?.Invoke(item);
            }
            return result;
        }

		public int Count { get { return _queue.Count; } }
		// public int Count { get { return _stack.Count; } }
    }
}
