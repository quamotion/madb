using System;
using System.Collections.Generic;
using System.Linq;

using Managed.Adb.Utilities.Extensions;

namespace Managed.Adb.Utilities.Linq.Extensions
{
    public static partial class DataProducerExt
    {
        /// <summary>
        /// Converts an IDataProducer into an IFuture[IEnumerable]. The results
        /// are buffered in memory (as a list), so be warned that this loses the "streaming"
        /// nature of most of the IDataProducer extension methods. The "future" nature of
        /// the result ensures that all results are produced before the enumeration can take
        /// place.
        /// </summary>
        /// <remarks>This will force all values to be buffered</remarks>
        public static IFuture<IEnumerable<TSource>> AsFutureEnumerable<TSource>(this IDataProducer<TSource> source)
        {
            source.ThrowIfNull("source");

            Future<IEnumerable<TSource>> ret = new Future<IEnumerable<TSource>>();

            List<TSource> list = new List<TSource>();
            source.DataProduced += value => list.Add(value);
            source.EndOfData += () => ret.Value = list;

            return ret;
        }

        /// <summary>
        /// Converts an IDataProducer into an IEnumerable. The results
        /// are buffered in memory (as a list), so be warned that this loses the "streaming"
        /// nature of most of the IDataProducer extension methods. The list is returned
        /// immediately, but further data productions add to it. You must therefore be careful
        /// when the list is used - it is a good idea to only use it after all data has been
        /// produced.
        /// </summary>
        /// <remarks>This will force all values to be buffered</remarks>
        public static IEnumerable<TSource> AsEnumerable<TSource>(this IDataProducer<TSource> source)
        {
            source.ThrowIfNull("source");

            return source.ToList();
        }

        /// <summary>
        /// Converts an IDataProducer into a list. An empty list is returned immediately,
        /// and any results produced are added to it.
        /// </summary>
        /// <remarks>This will force all values to be buffered</remarks>
        public static List<TSource> ToList<TSource>(this IDataProducer<TSource> source)
        {
            source.ThrowIfNull("source");

            List<TSource> list = new List<TSource>();
            source.DataProduced += value => list.Add(value);

            return list;
        }

        /// <summary>
        /// Converts an IDataProducer into a future array.
        /// </summary>
        /// <remarks>This will force all values to be buffered</remarks>
        public static IFuture<TSource[]> ToFutureArray<TSource>(this IDataProducer<TSource> source)
        {
            source.ThrowIfNull("source");

            Future<TSource[]> ret = new Future<TSource[]>();
            List<TSource> list = source.ToList();

            source.EndOfData += () => ret.Value = list.ToArray();

            return ret;
        }

        /// <summary>
        /// Converts an IDataProducer into a lookup.
        /// </summary>
        /// <param name="elementSelector">A transform function to produce a result element value from each element.</param>
        /// <param name="keySelector">A function to extract a key from each element.</param>
        /// <param name="keyComparer">Used to compare keys.</param>
        /// <param name="source">The data source.</param>
        /// <remarks>This will force all values to be buffered</remarks>
        public static ILookup<TKey, TElement> ToLookup<TSource, TKey, TElement>(
            this IDataProducer<TSource> source,
            Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> keyComparer)
        {
            source.ThrowIfNull("source");
            keySelector.ThrowIfNull("keySelector");
            elementSelector.ThrowIfNull("elementSelector");
            keyComparer.ThrowIfNull("keyComparer");

            EditableLookup<TKey, TElement> lookup = new EditableLookup<TKey, TElement>(keyComparer);
            source.DataProduced += t => lookup.Add(keySelector(t), elementSelector(t));
            source.EndOfData += () => lookup.TrimExcess();
            return lookup;
        }

        /// <summary>
        /// Converts an IDataProducer into a lookup.
        /// </summary>
        /// <param name="keySelector">A function to extract a key from each element.</param>
        /// <param name="source">The data source.</param>
        /// <remarks>This will force all values to be buffered</remarks>
        public static ILookup<TKey, TSource> ToLookup<TSource, TKey>(
                this IDataProducer<TSource> source,
                Func<TSource, TKey> keySelector)
        {
            return ToLookup<TSource, TKey, TSource>(source, keySelector, t => t, EqualityComparer<TKey>.Default);
        }
        /// <summary>
        /// Converts an IDataProducer into a lookup.
        /// </summary>
        /// <param name="keySelector">A function to extract a key from each element.</param>
        /// <param name="keyComparer">Used to compare keys.</param>
        /// <param name="source">The data source.</param>
        /// <remarks>This will force all values to be buffered</remarks>
        public static ILookup<TKey, TSource> ToLookup<TSource, TKey>(
                this IDataProducer<TSource> source,
                Func<TSource, TKey> keySelector, IEqualityComparer<TKey> keyComparer)
        {
            return ToLookup<TSource, TKey, TSource>(source, keySelector, t => t, keyComparer);
        }
        /// <summary>
        /// Converts an IDataProducer into a lookup.
        /// </summary>
        /// <param name="elementSelector">A transform function to produce a result element value from each element.</param>
        /// <param name="keySelector">A function to extract a key from each element.</param>
        /// <param name="source">The data source.</param>
        /// <remarks>This will force all values to be buffered</remarks>
        public static ILookup<TKey, TElement> ToLookup<TSource, TKey, TElement>(
                this IDataProducer<TSource> source,
                Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
        {
            return ToLookup<TSource, TKey, TElement>(source, keySelector, elementSelector, EqualityComparer<TKey>.Default);
        }

        /// <summary>
        /// Converts an IDataProducer into a dictionary.
        /// </summary>
        /// <param name="elementSelector">A transform function to produce a result element value from each element.</param>
        /// <param name="keySelector">A function to extract a key from each element.</param>
        /// <param name="keyComparer">Used to compare keys.</param>
        /// <param name="source">The data source.</param>
        /// <remarks>This will force all values to be buffered</remarks>
        public static IDictionary<TKey, TElement> ToDictionary<TSource, TKey, TElement>(
                this IDataProducer<TSource> source,
                Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> keyComparer)
        {
            source.ThrowIfNull("source");
            keySelector.ThrowIfNull("keySelector");
            elementSelector.ThrowIfNull("elementSelector");
            keyComparer.ThrowIfNull("keyComparer");

            Dictionary<TKey, TElement> dict = new Dictionary<TKey, TElement>(keyComparer);
            source.DataProduced += t => dict.Add(keySelector(t), elementSelector(t));
            return dict;
        }
        /// <summary>
        /// Converts an IDataProducer into a dictionary.
        /// </summary>
        /// <param name="keySelector">A function to extract a key from each element.</param>
        /// <param name="source">The data source.</param>
        /// <remarks>This will force all values to be buffered</remarks>
        public static IDictionary<TKey, TSource> ToDictionary<TSource, TKey>(
                this IDataProducer<TSource> source,
                Func<TSource, TKey> keySelector)
        {
            return ToDictionary<TSource, TKey, TSource>(source, keySelector, t => t, EqualityComparer<TKey>.Default);
        }
        /// <summary>
        /// Converts an IDataProducer into a dictionary.
        /// </summary>
        /// <param name="keySelector">A function to extract a key from each element.</param>
        /// <param name="keyComparer">Used to compare keys.</param>
        /// <param name="source">The data source.</param>
        /// <remarks>This will force all values to be buffered</remarks>
        public static IDictionary<TKey, TSource> ToDictionary<TSource, TKey>(
                this IDataProducer<TSource> source,
                Func<TSource, TKey> keySelector, IEqualityComparer<TKey> keyComparer)
        {
            return ToDictionary<TSource, TKey, TSource>(source, keySelector, t => t, keyComparer);
        }
        /// <summary>
        /// Converts an IDataProducer into a dictionary.
        /// </summary>
        /// <param name="elementSelector">A transform function to produce a result element value from each element.</param>
        /// <param name="keySelector">A function to extract a key from each element.</param>
        /// <param name="source">The data source.</param>
        /// <remarks>This will force all values to be buffered</remarks>
        public static IDictionary<TKey, TElement> ToDictionary<TSource, TKey, TElement>(
                this IDataProducer<TSource> source,
                Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
        {
            return ToDictionary<TSource, TKey, TElement>(source, keySelector, elementSelector, EqualityComparer<TKey>.Default);
        }
    }
}
