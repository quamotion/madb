using System;

namespace Managed.Adb.Utilities.Linq
{
    /// <summary>
    /// Simple implementation of IProducerGrouping which proxies to an existing
    /// IDataProducer.
    /// </summary>
    public class ProducerGrouping<TKey,TElement> : IProducerGrouping<TKey, TElement>
    {
        readonly IDataProducer<TElement> source;
        readonly TKey key;
        /// <summary>
        /// Event which is raised when an item of data is produced.
        /// This will not be raised after EndOfData has been raised.
        /// The parameter for the event is the 
        /// </summary>
        /// <seealso cref="Managed.Adb.Utilities.Linq.IDataProducer&lt;T&gt;.DataProduced"/>
        public event Action<TElement> DataProduced
        {
            add { source.DataProduced += value; }
            remove { source.DataProduced -= value; }
        }

        /// <summary>
        /// Event which is raised when the sequence has finished being
        /// produced. This will be raised exactly once, and after all
        /// DataProduced events (if any) have been raised.
        /// </summary>
        /// <seealso cref="Managed.Adb.Utilities.Linq.IDataProducer&lt;T&gt;.EndOfData"/>
        public event Action EndOfData
        {
            add { source.EndOfData += value; }
            remove { source.EndOfData -= value; }
        }

        /// <summary>
        /// The key for this grouping.
        /// </summary>
        public TKey Key
        {
            get { return key; }
        }

        /// <summary>
        /// Constructs a new grouping with the given key
        /// </summary>
        public ProducerGrouping(TKey key, IDataProducer<TElement> source)
        {
            this.key = key;
            this.source = source;
        }
    }
}
