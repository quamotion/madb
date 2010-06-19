using System;
using System.Collections.Generic;
using Managed.Adb.Utilities.Collections;
using Managed.Adb.Utilities.Collections.Extensions;
using Managed.Adb.Utilities.Extensions;

namespace Managed.Adb.Utilities.Linq.Extensions
{
    public static partial class DataProducerExt
    {

        /// <summary>
        /// Filters a data-producer based on a predicate on each value
        /// </summary>
        /// <param name="source">The data-producer to be filtered</param>
        /// <param name="predicate">The condition to be satisfied</param>
        /// <returns>A filtered data-producer; only matching values will raise the DataProduced event</returns>
        public static IDataProducer<TSource> Where<TSource>(this IDataProducer<TSource> source,
            Func<TSource, bool> predicate)
        {
            predicate.ThrowIfNull("predicate");

            return source.Where((x, index) => predicate(x));
        }
        /// <summary>
        /// Filters a data-producer based on a predicate on each value; the index
        /// in the sequence is used in the predicate
        /// </summary>
        /// <param name="source">The data-producer to be filtered</param>
        /// <param name="predicate">The condition to be satisfied</param>
        /// <returns>A filtered data-producer; only matching values will raise the DataProduced event</returns>
        public static IDataProducer<TSource> Where<TSource>(this IDataProducer<TSource> source,
            Func<TSource, int, bool> predicate)
        {
            source.ThrowIfNull("source");
            predicate.ThrowIfNull("predicate");

            DataProducer<TSource> ret = new DataProducer<TSource>();

            int index = 0;

            source.DataProduced += value =>
            {
                if (predicate(value, index++))
                {
                    ret.Produce(value);
                }
            };
            source.EndOfData += () => ret.End();
            return ret;
        }
        /// <summary>
        /// Returns a data-producer that yeilds the values from the sequence, or which yields the given
        /// singleton value if no data is produced.
        /// </summary>
        /// <param name="defaultValue">The default value to be yielded if no data is produced.</param>
        /// <param name="source">The source data-producer.</param>
        public static IDataProducer<TSource> DefaultIfEmpty<TSource>(this IDataProducer<TSource> source, TSource defaultValue)
        {
            source.ThrowIfNull("source");

            DataProducer<TSource> ret = new DataProducer<TSource>();

            bool empty = true;
            source.DataProduced += value =>
            {
                empty = false;
                ret.Produce(value);
            };
            source.EndOfData += () =>
            {
                if (empty)
                {
                    ret.Produce(defaultValue);
                }
                ret.End();
            };
            return ret;
        }
        /// <summary>
        /// Returns a data-producer that yeilds the values from the sequence, or which yields the default
        /// value for the Type if no data is produced.
        /// </summary>
        /// <param name="source">The source data-producer.</param>
        public static IDataProducer<TSource> DefaultIfEmpty<TSource>(this IDataProducer<TSource> source)
        {
            return source.DefaultIfEmpty(default(TSource));
        }
        /// <summary>
        /// Returns a projection on the data-producer, using a transformation to
        /// map each element into a new form.
        /// </summary>
        /// <typeparam name="TSource">The source type</typeparam>
        /// <typeparam name="TResult">The projected type</typeparam>
        /// <param name="source">The source data-producer</param>
        /// <param name="projection">The transformation to apply to each element.</param>
        public static IDataProducer<TResult> Select<TSource, TResult>(this IDataProducer<TSource> source,
                                                               Func<TSource, TResult> projection)
        {
            projection.ThrowIfNull("projection");
            return source.Select((t, index) => projection(t));
        }
        /// <summary>
        /// Returns a projection on the data-producer, using a transformation
        /// (involving the elements's index in the sequence) to
        /// map each element into a new form.
        /// </summary>
        /// <typeparam name="TSource">The source type</typeparam>
        /// <typeparam name="TResult">The projected type</typeparam>
        /// <param name="source">The source data-producer</param>
        /// <param name="projection">The transformation to apply to each element.</param>
        public static IDataProducer<TResult> Select<TSource, TResult>(this IDataProducer<TSource> source,
                                                                      Func<TSource, int, TResult> projection)
        {
            source.ThrowIfNull("source");
            projection.ThrowIfNull("projection");

            DataProducer<TResult> ret = new DataProducer<TResult>();
            int index = 0;
            source.DataProduced += value => ret.Produce(projection(value, index++));
            source.EndOfData += () => ret.End();
            return ret;
        }

        /// <summary>
        /// Returns a data-producer that will yield a specified number of
        /// contiguous elements from the start of a sequence - i.e.
        /// "the first &lt;x&gt; elements".
        /// </summary>
        /// <param name="source">The source data-producer</param>
        /// <param name="count">The maximum number of elements to return</param>
        public static IDataProducer<TSource> Take<TSource>(this IDataProducer<TSource> source, int count)
        {
            source.ThrowIfNull("source");

            DataProducer<TSource> ret = new DataProducer<TSource>();

            Action completion = () => ret.End();
            Action<TSource> production = null;

            production = value =>
            {
                if (count > 0)
                {
                    ret.Produce(value);
                    count--;
                }
                if (count <= 0)
                {
                    source.EndOfData -= completion;
                    source.DataProduced -= production;
                    ret.End();
                }
            };

            source.DataProduced += production;
            source.EndOfData += completion;
            return ret;
        }
        /// <summary>
        /// Returns a data-producer that will ignore a specified number of
        /// contiguous elements from the start of a sequence, and yield
        /// all elements after this point - i.e. 
        /// "elements from index &lt;x&gt; onwards".
        /// </summary>
        /// <param name="source">The source data-producer</param>
        /// <param name="count">The number of elements to ignore</param>
        public static IDataProducer<TSource> Skip<TSource>(this IDataProducer<TSource> source, int count)
        {
            source.ThrowIfNull("source");

            DataProducer<TSource> ret = new DataProducer<TSource>();
            source.DataProduced += value =>
            {
                if (count > 0)
                {
                    count--;
                }
                else
                {
                    ret.Produce(value);
                }
            };
            source.EndOfData += () => ret.End();
            return ret;
        }
        /// <summary>
        /// Returns a data-producer that will yield
        /// elements a sequence as long as a condition
        /// is satsified; when the condition fails for an element,
        /// that element and all subsequent elements are ignored.
        /// </summary>
        /// <param name="source">The source data-producer</param>
        /// <param name="predicate">The condition to yield elements</param>
        public static IDataProducer<TSource> TakeWhile<TSource>(this IDataProducer<TSource> source, Func<TSource, bool> predicate)
        {
            predicate.ThrowIfNull("predicate");
            return source.TakeWhile((x, index) => predicate(x));
        }
        /// <summary>
        /// Returns a data-producer that will yield
        /// elements a sequence as long as a condition
        /// (involving the element's index in the sequence)
        /// is satsified; when the condition fails for an element,
        /// that element and all subsequent elements are ignored.
        /// </summary>
        /// <param name="source">The source data-producer</param>
        /// <param name="predicate">The condition to yield elements</param>        
        public static IDataProducer<TSource> TakeWhile<TSource>(this IDataProducer<TSource> source, Func<TSource, int, bool> predicate)
        {
            source.ThrowIfNull("source");
            predicate.ThrowIfNull("predicate");

            DataProducer<TSource> ret = new DataProducer<TSource>();
            Action completion = () => ret.End();
            Action<TSource> production = null;

            int index = 0;

            production = value =>
            {
                if (!predicate(value, index++))
                {
                    ret.End();
                    source.DataProduced -= production;
                    source.EndOfData -= completion;
                }
                else
                {
                    ret.Produce(value);
                }
            };

            source.DataProduced += production;
            source.EndOfData += completion;
            return ret;
        }
        /// <summary>
        /// Returns a data-producer that will ignore the
        /// elements from the start of a sequence while a condition
        /// is satsified; when the condition fails for an element,
        /// that element and all subsequent elements are yielded.
        /// </summary>
        /// <param name="source">The source data-producer</param>
        /// <param name="predicate">The condition to skip elements</param>
        public static IDataProducer<TSource> SkipWhile<TSource>(this IDataProducer<TSource> source, Func<TSource, bool> predicate)
        {
            predicate.ThrowIfNull("predicate");

            return source.SkipWhile((t, index) => predicate(t));
        }
        /// <summary>
        /// Returns a data-producer that will ignore the
        /// elements from the start of a sequence while a condition
        /// (involving the elements's index in the sequence)
        /// is satsified; when the condition fails for an element,
        /// that element and all subsequent elements are yielded.
        /// </summary>
        /// <param name="source">The source data-producer</param>
        /// <param name="predicate">The condition to skip elements</param>
        public static IDataProducer<TSource> SkipWhile<TSource>(this IDataProducer<TSource> source, Func<TSource, int, bool> predicate)
        {
            source.ThrowIfNull("source");
            predicate.ThrowIfNull("predicate");

            DataProducer<TSource> ret = new DataProducer<TSource>();
            Action completion = () => ret.End();

            bool skipping = true;
            int index = 0;
            source.DataProduced += value =>
            {
                if (skipping)
                {
                    skipping = predicate(value, index++);
                }
                // Note - not an else clause!
                if (!skipping)
                {
                    ret.Produce(value);
                }
            };
            source.EndOfData += completion;
            return ret;
        }

#if DOTNET35
        /// <summary>
        /// Returns a data-producer that yields the first instance of each unique
        /// value in the sequence; subsequent identical values are ignored.
        /// </summary>
        /// <param name="source">The data-producer</param>
        /// <remarks>This will force the first instance of each unique value to be buffered</remarks>
        public static IDataProducer<TSource> Distinct<TSource>(this IDataProducer<TSource> source)
        {
            return source.Distinct(EqualityComparer<TSource>.Default);
        }
        /// <summary>
        /// Returns a data-producer that yields the first instance of each unique
        /// value in the sequence; subsequent identical values are ignored.
        /// </summary>
        /// <param name="source">The data-producer</param>
        /// <param name="comparer">Used to determine equaility between values</param>
        /// <remarks>This will force the first instance of each unique value to be buffered</remarks>
        public static IDataProducer<TSource> Distinct<TSource>(this IDataProducer<TSource> source, IEqualityComparer<TSource> comparer)
        {
            source.ThrowIfNull("source");
            comparer.ThrowIfNull("comparer");

            DataProducer<TSource> ret = new DataProducer<TSource>();

            HashSet<TSource> set = new HashSet<TSource>(comparer);

            source.DataProduced += value =>
            {
                if (set.Add(value))
                {
                    ret.Produce(value);
                }
            };
            source.EndOfData += () => ret.End();
            return ret;
        }
#endif
        /// <summary>
        /// Reverses the order of a sequence
        /// </summary>
        /// <param name="source">The data-producer</param>
        /// <returns>A data-producer that yields the sequence
        /// in the reverse order</returns>
        /// <remarks>This will force all data to be buffered</remarks>
        public static IDataProducer<TSource> Reverse<TSource>(this IDataProducer<TSource> source)
        {
            source.ThrowIfNull("source");

            DataProducer<TSource> ret = new DataProducer<TSource>();

            // use List (rather than ToList) so we have a List<T> with
            // Reverse immediately available (more efficient, and 2.0 compatible)
            List<TSource> results = new List<TSource>();
            source.DataProduced += item => results.Add(item);
            source.EndOfData += () => {
                List<TSource> items = new List<TSource>(results);
                items.Reverse();
                ret.ProduceAndEnd(items);
            }; 

            return ret;
        }

        /// <summary>
        /// Further orders the values from an ordered data-source by a transform on each term, ascending
        /// (the sort operation is only applied once for the combined ordering)
        /// </summary>
        /// <param name="source">The original data-producer and ordering</param>
        /// <param name="selector">Returns the value (for each term) by which to order the sequence</param>
        /// <returns>A data-producer that yeilds the sequence ordered
        /// by the selected value</returns>
        /// <remarks>This will force all data to be buffered</remarks>
        public static IOrderedDataProducer<TSource> ThenBy<TSource, TKey>(this IOrderedDataProducer<TSource> source, Func<TSource, TKey> selector)
        {
            return ThenBy(source, selector, Comparer<TKey>.Default, false);
        }
        /// <summary>
        /// Further orders the values from an ordered data-source by a transform on each term, ascending
        /// (the sort operation is only applied once for the combined ordering)
        /// </summary>
        /// <param name="source">The original data-producer and ordering</param>
        /// <param name="selector">Returns the value (for each term) by which to order the sequence</param>
        /// <param name="comparer">Comparer to compare the selected values</param>
        /// <returns>A data-producer that yeilds the sequence ordered
        /// by the selected value</returns>
        /// <remarks>This will force all data to be buffered</remarks>
        public static IOrderedDataProducer<TSource> ThenBy<TSource, TKey>(this IOrderedDataProducer<TSource> source, Func<TSource, TKey> selector, IComparer<TKey> comparer)
        {
            return ThenBy(source, selector, comparer, false);
        }
        /// <summary>
        /// Further orders the values from an ordered data-source by a transform on each term, descending
        /// (the sort operation is only applied once for the combined ordering)
        /// </summary>
        /// <param name="source">The original data-producer and ordering</param>
        /// <param name="selector">Returns the value (for each term) by which to order the sequence</param>
        /// <returns>A data-producer that yeilds the sequence ordered
        /// by the selected value</returns>
        /// <remarks>This will force all data to be buffered</remarks>
        public static IOrderedDataProducer<TSource> ThenByDescending<TSource, TKey>(this IOrderedDataProducer<TSource> source, Func<TSource, TKey> selector)
        {
            return ThenBy(source, selector, Comparer<TKey>.Default, true);
        }
        /// <summary>
        /// Further orders the values from an ordered data-source by a transform on each term, descending
        /// (the sort operation is only applied once for the combined ordering)
        /// </summary>
        /// <param name="source">The original data-producer and ordering</param>
        /// <param name="selector">Returns the value (for each term) by which to order the sequence</param>
        /// <param name="comparer">Comparer to compare the selected values</param>
        /// <returns>A data-producer that yeilds the sequence ordered
        /// by the selected value</returns>
        /// <remarks>This will force all data to be buffered</remarks>        
        public static IOrderedDataProducer<TSource> ThenByDescending<TSource, TKey>(this IOrderedDataProducer<TSource> source, Func<TSource, TKey> selector, IComparer<TKey> comparer)
        {
            return ThenBy(source, selector, comparer, true);
        }

        /// <summary>
        /// Orders the values from a data-source by a transform on each term, ascending
        /// </summary>
        /// <param name="source">The original data-producer</param>
        /// <param name="selector">Returns the value (for each term) by which to order the sequence</param>
        /// <returns>A data-producer that yeilds the sequence ordered
        /// by the selected value</returns>
        /// <remarks>This will force all data to be buffered</remarks>
        public static IOrderedDataProducer<TSource> OrderBy<TSource, TKey>(this IDataProducer<TSource> source, Func<TSource, TKey> selector)
        {
            return OrderBy(source, selector, Comparer<TKey>.Default, false);
        }
        /// <summary>
        /// Orders the values from a data-source by a transform on each term, ascending
        /// </summary>
        /// <param name="source">The original data-producer</param>
        /// <param name="selector">Returns the value (for each term) by which to order the sequence</param>
        /// <param name="comparer">Comparer to compare the selected values</param>
        /// <returns>A data-producer that yeilds the sequence ordered
        /// by the selected value</returns>
        /// <remarks>This will force all data to be buffered</remarks>
        public static IOrderedDataProducer<TSource> OrderBy<TSource, TKey>(this IDataProducer<TSource> source, Func<TSource, TKey> selector, IComparer<TKey> comparer)
        {
            return OrderBy(source, selector, comparer, false);
        }
        /// <summary>
        /// Orders the values from a data-source by a transform on each term, descending
        /// </summary>
        /// <param name="source">The original data-producer</param>
        /// <param name="selector">Returns the value (for each term) by which to order the sequence</param>
        /// <returns>A data-producer that yeilds the sequence ordered
        /// by the selected value</returns>
        /// <remarks>This will force all data to be buffered</remarks>
        public static IOrderedDataProducer<TSource> OrderByDescending<TSource, TKey>(this IDataProducer<TSource> source, Func<TSource, TKey> selector)
        {
            return OrderBy(source, selector, Comparer<TKey>.Default, true);
        }
        /// <summary>
        /// Orders the values from a data-source by a transform on each term, descending
        /// </summary>
        /// <param name="source">The original data-producer</param>
        /// <param name="selector">Returns the value (for each term) by which to order the sequence</param>
        /// <param name="comparer">Comparer to compare the selected values</param>
        /// <returns>A data-producer that yeilds the sequence ordered
        /// by the selected value</returns>
        /// <remarks>This will force all data to be buffered</remarks>
        public static IOrderedDataProducer<TSource> OrderByDescending<TSource, TKey>(this IDataProducer<TSource> source, Func<TSource, TKey> selector, IComparer<TKey> comparer)
        {
            return OrderBy(source, selector, comparer, true);
        }

        private static IOrderedDataProducer<TSource> OrderBy<TSource, TKey>(IDataProducer<TSource> source, Func<TSource, TKey> selector, IComparer<TKey> comparer, bool descending)
        {
            source.ThrowIfNull("source");
            comparer.ThrowIfNull("comparer");

            IComparer<TSource> itemComparer = new ProjectionComparer<TSource, TKey>(selector, comparer);
            if (descending) itemComparer = itemComparer.Reverse();

            // first, discard any existing "order by"s by going back to the producer
            IOrderedDataProducer<TSource> orderedProducer;
            bool first = true;
            while ((orderedProducer = source as IOrderedDataProducer<TSource>) != null)
            {
                if(first) {
                    // keep the top-most comparer to enforce a balanced sort
                    itemComparer = new LinkedComparer<TSource>(itemComparer, orderedProducer.Comparer);
                    first = false;
                }
                source = orderedProducer.BaseProducer;
            }            
            return new OrderedDataProducer<TSource>(source, itemComparer);
        }

        private static IOrderedDataProducer<TSource> ThenBy<TSource, TKey>(IOrderedDataProducer<TSource> source, Func<TSource, TKey> selector, IComparer<TKey> comparer, bool descending)
        {
            comparer.ThrowIfNull("comparer");
            IComparer<TSource> itemComparer = new ProjectionComparer<TSource, TKey>(selector, comparer);
            if (descending) itemComparer = itemComparer.Reverse();
            itemComparer = new LinkedComparer<TSource>(source.Comparer, itemComparer);
            return new OrderedDataProducer<TSource>(source, itemComparer);
        }
    }
}
