using System;
using System.Collections.Generic;

namespace Managed.Adb.Utilities.Linq
{
    /// <summary>
    /// Very simple implementation of IDataProducer.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DataProducer<T> : IDataProducer<T>
    {
        /// <summary>
        /// Event which is raised when an item of data is produced.
        /// This will not be raised after EndOfData has been raised.
        /// The parameter for the event is the 
        /// </summary>
        /// <seealso cref="Managed.Adb.Utilities.Linq.IDataProducer&lt;T&gt;.DataProduced"/>
        public event Action<T> DataProduced;
        /// <summary>
        /// Event which is raised when the sequence has finished being
        /// produced. This will be raised exactly once, and after all
        /// DataProduced events (if any) have been raised.
        /// </summary>
        /// <seealso cref="Managed.Adb.Utilities.Linq.IDataProducer&lt;T&gt;.EndOfData"/>
        public event Action EndOfData;
        bool endReached = false;

        /// <summary>
        /// Signals a single item of data.
        /// </summary>
        public void Produce(T item)
        {
            if (endReached)
            {
                throw new InvalidOperationException("Cannot produce after end of data");
            }
            if (DataProduced != null)
            {
                DataProduced(item);
            }
        }

        /// <summary>
        /// Signals multiple items of data, one at a time, then ends.
        /// Note that this method only exists to support the params modifier.
        /// In every other way it's equivalent to the ProduceAndEnd(IEnumerable{T}).
        /// </summary>
        public void ProduceAndEnd(params T[] items)
        {
            ProduceAndEnd((IEnumerable<T>)items);
        }

        /// <summary>
        /// Signals multiple items of data, one at a time, then ends.
        /// </summary>
        public void ProduceAndEnd(IEnumerable<T> items)
        {
            foreach (T item in items)
            {
                Produce(item);
            }
            End();
        }

        /// <summary>
        /// Pumps the specified items into this data producer, yielding results
        /// as they are received. Before an item is pumped, an internal queue is
        /// created. Pumping an item may yield results at the other end of the pipeline
        /// - any such results are buffered in the queue. When the pumping of a particular
        /// item has finished, all results in the queue are yielded. This means that
        /// naturally streaming operations (projection and filtering) require only a single item
        /// buffer. This producer "ends" when all the items have been produced. If the result
        /// pipeline ends before all items have been pumped, the buffered results are yielded
        /// but no more items are pumped.
        /// </summary>
        /// <typeparam name="TResult">Type of element in the result pipeline</typeparam>
        /// <param name="items">Items to insert into the pipeline</param>
        /// <param name="pipeline">The pipeline to subscribe to for items to yield</param>
        /// <returns>A sequence of yielded items.</returns>
        public IEnumerable<TResult> PumpProduceAndEnd<TResult>(IEnumerable<T> items, IDataProducer<TResult> pipeline)
        {
            bool stop = false;
            Queue<TResult> resultBuffer = new Queue<TResult>();
            pipeline.DataProduced += result => resultBuffer.Enqueue(result);
            pipeline.EndOfData += () => stop = true;
            foreach (T item in items)
            {
                Produce(item);
                // Unbuffer as we go
                while (resultBuffer.Count > 0)
                {
                    yield return resultBuffer.Dequeue();
                }
                if (stop)
                {
                    yield break;
                }
            }
            End();
            // Yield any final items which may have been produced due to ending the pipeline
            while (resultBuffer.Count > 0)
            {
                yield return resultBuffer.Dequeue();
            }
        }

        /// <summary>
        /// Signal the end of data. This can only be called once, and
        /// afterwards the Produce method must not be called.
        /// </summary>
        public void End()
        {
            if (endReached)
            {
                throw new InvalidOperationException("Cannot produce end twice");
            }
            endReached = true;
            if (EndOfData != null)
            {
                EndOfData();
            }
        }
    }
}
