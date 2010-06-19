using System;
using System.Collections.Generic;
using Managed.Adb.Utilities.Extensions;
namespace Managed.Adb.Utilities.Linq.Extensions
{
    public static partial class DataProducerExt
    {
        /// <summary>
        /// Groups the elements of a sequence according to a specified key selector function
        /// and creates a result value from each group and its key.
        /// </summary>
        /// <typeparam name="TKey">The return-type of the transform used to group the sequence</typeparam>
        /// <typeparam name="TSource">The values to be yielded by the original data-source</typeparam>
        /// <param name="keySelector">A function to extract the key for each element in hte original sequence.</param>
        /// <param name="source">The data-source to be grouped</param>
        /// <remarks>This will force each unique grouping key to
        /// be buffered, but not the data itself</remarks>
        public static IDataProducer<IProducerGrouping<TKey, TSource>> GroupBy<TSource, TKey>
            (this IDataProducer<TSource> source,
             Func<TSource, TKey> keySelector)
        {
            return source.GroupBy(keySelector,
                                  elt => elt,
                                  (key, elements) => (IProducerGrouping<TKey, TSource>)new ProducerGrouping<TKey, TSource>(key, elements),
                                  EqualityComparer<TKey>.Default);
        }
        /// <summary>
        /// Groups the elements of a sequence according to a specified key selector function
        /// and creates a result value from each group and its key.
        /// </summary>
        /// <typeparam name="TKey">The return-type of the transform used to group the sequence</typeparam>
        /// <typeparam name="TSource">The values to be yielded by the original data-source</typeparam>
        /// <param name="comparer">Used to compare grouping keys</param>
        /// <param name="keySelector">A function to extract the key for each element in hte original sequence.</param>
        /// <param name="source">The data-source to be grouped</param>
        /// <remarks>This will force each unique grouping key to
        /// be buffered, but not the data itself</remarks>
        public static IDataProducer<IProducerGrouping<TKey, TSource>> GroupBy<TSource, TKey>
            (this IDataProducer<TSource> source,
             Func<TSource, TKey> keySelector,
             IEqualityComparer<TKey> comparer)
        {
            return source.GroupBy(keySelector,
                                  elt => elt,
                                  (key, elements) => (IProducerGrouping<TKey, TSource>)new ProducerGrouping<TKey, TSource>(key, elements),
                                  comparer);
        }
        /// <summary>
        /// Groups the elements of a sequence according to a specified key selector function
        /// and creates a result value from each group and its key. The elements of each
        /// group are projected by using a specified function.
        /// </summary>
        /// <typeparam name="TElement">The return-type of the transform used to process the
        /// values within each grouping.</typeparam>
        /// <typeparam name="TKey">The return-type of the transform used to group the sequence</typeparam>
        /// <typeparam name="TSource">The values to be yielded by the original data-source</typeparam>
        /// <param name="elementSelector">A function to map each source element to an element in the appropriate group</param>
        /// <param name="keySelector">A function to extract the key for each element in hte original sequence.</param>
        /// <param name="source">The data-source to be grouped</param>
        /// <remarks>This will force each unique grouping key to
        /// be buffered, but not the data itself</remarks>
        public static IDataProducer<IProducerGrouping<TKey, TElement>> GroupBy<TSource, TKey, TElement>
            (this IDataProducer<TSource> source,
             Func<TSource, TKey> keySelector,
             Func<TSource, TElement> elementSelector)
        {
            return source.GroupBy(keySelector,
                                  elementSelector,
                                  (key, elements) => (IProducerGrouping<TKey, TElement>)new ProducerGrouping<TKey, TElement>(key, elements),
                                  EqualityComparer<TKey>.Default);
        }
        /// <summary>
        /// Groups the elements of a sequence according to a specified key selector function
        /// and creates a result value from each group and its key.
        /// </summary>
        /// <typeparam name="TKey">The return-type of the transform used to group the sequence</typeparam>
        /// <typeparam name="TResult">The final values to be yielded after processing</typeparam>
        /// <typeparam name="TSource">The values to be yielded by the original data-source</typeparam>
        /// <param name="keySelector">A function to extract the key for each element in hte original sequence.</param>
        /// <param name="resultSelector">A function to create a result value from each group.</param>
        /// <param name="source">The data-source to be grouped</param>
        /// <remarks>This will force each unique grouping key to
        /// be buffered, but not the data itself</remarks>
        public static IDataProducer<TResult> GroupBy<TSource, TKey, TResult>
            (this IDataProducer<TSource> source,
             Func<TSource, TKey> keySelector,
             Func<TKey, IDataProducer<TSource>, TResult> resultSelector)
        {
            return source.GroupBy(keySelector,
                                  elt => elt,
                                  resultSelector,
                                  EqualityComparer<TKey>.Default);
        }
        /// <summary>
        /// Groups the elements of a sequence according to a specified key selector function
        /// and creates a result value from each group and its key. The elements of each
        /// group are projected by using a specified function.
        /// </summary>
        /// <typeparam name="TElement">The return-type of the transform used to process the
        /// values within each grouping.</typeparam>
        /// <typeparam name="TKey">The return-type of the transform used to group the sequence</typeparam>
        /// <typeparam name="TSource">The values to be yielded by the original data-source</typeparam>
        /// <param name="comparer">Used to compare grouping keys</param>
        /// <param name="elementSelector">A function to map each source element to an element in the appropriate group</param>
        /// <param name="keySelector">A function to extract the key for each element in hte original sequence.</param>
        /// <param name="source">The data-source to be grouped</param>
        /// <remarks>This will force each unique grouping key to
        /// be buffered, but not the data itself</remarks>
        public static IDataProducer<IProducerGrouping<TKey, TElement>> GroupBy<TSource, TKey, TElement>
            (this IDataProducer<TSource> source,
             Func<TSource, TKey> keySelector,
             Func<TSource, TElement> elementSelector,
             IEqualityComparer<TKey> comparer)
        {
            return source.GroupBy(keySelector,
                                  elementSelector,
                                  (key, elements) => (IProducerGrouping<TKey, TElement>)new ProducerGrouping<TKey, TElement>(key, elements),
                                  comparer);
        }
        /// <summary>
        /// Groups the elements of a sequence according to a specified key selector function
        /// and creates a result value from each group and its key. The elements of each
        /// group are projected by using a specified function.
        /// </summary>
        /// <typeparam name="TElement">The return-type of the transform used to process the
        /// values within each grouping.</typeparam>
        /// <typeparam name="TKey">The return-type of the transform used to group the sequence</typeparam>
        /// <typeparam name="TResult">The final values to be yielded after processing</typeparam>
        /// <typeparam name="TSource">The values to be yielded by the original data-source</typeparam>
        /// <param name="elementSelector">A function to map each source element to an element in the appropriate group</param>
        /// <param name="keySelector">A function to extract the key for each element in hte original sequence.</param>
        /// <param name="resultSelector">A function to create a result value from each group.</param>
        /// <param name="source">The data-source to be grouped</param>
        /// <remarks>This will force each unique grouping key to
        /// be buffered, but not the data itself</remarks>
        public static IDataProducer<TResult> GroupBy<TSource, TKey, TElement, TResult>
            (this IDataProducer<TSource> source,
             Func<TSource, TKey> keySelector,
             Func<TSource, TElement> elementSelector,
             Func<TKey, IDataProducer<TElement>, TResult> resultSelector)
        {
            return source.GroupBy(keySelector, elementSelector, resultSelector, EqualityComparer<TKey>.Default);
        }
        /// <summary>
        /// Groups the elements of a sequence according to a specified key selector function
        /// and creates a result value from each group and its key.
        /// </summary>
        /// <typeparam name="TKey">The return-type of the transform used to group the sequence</typeparam>
        /// <typeparam name="TResult">The final values to be yielded after processing</typeparam>
        /// <typeparam name="TSource">The values to be yielded by the original data-source</typeparam>
        /// <param name="comparer">Used to compare grouping keys</param>
        /// <param name="keySelector">A function to extract the key for each element in hte original sequence.</param>
        /// <param name="resultSelector">A function to create a result value from each group.</param>
        /// <param name="source">The data-source to be grouped</param>
        /// <remarks>This will force each unique grouping key to
        /// be buffered, but not the data itself</remarks>
        public static IDataProducer<TResult> GroupBy<TSource, TKey, TResult>
            (this IDataProducer<TSource> source,
             Func<TSource, TKey> keySelector,
             Func<TKey, IDataProducer<TSource>, TResult> resultSelector,
             IEqualityComparer<TKey> comparer)
        {
            return source.GroupBy(keySelector,
                                  elt => elt,
                                  resultSelector,
                                  comparer);
        }
        /// <summary>
        /// Groups the elements of a sequence according to a specified key selector function
        /// and creates a result value from each group and its key. The elements of each
        /// group are projected by using a specified function.
        /// </summary>
        /// <typeparam name="TElement">The return-type of the transform used to process the
        /// values within each grouping.</typeparam>
        /// <typeparam name="TKey">The return-type of the transform used to group the sequence</typeparam>
        /// <typeparam name="TResult">The final values to be yielded after processing</typeparam>
        /// <typeparam name="TSource">The values to be yielded by the original data-source</typeparam>
        /// <param name="comparer">Used to compare grouping keys</param>
        /// <param name="elementSelector">A function to map each source element to an element in the appropriate group</param>
        /// <param name="keySelector">A function to extract the key for each element in hte original sequence.</param>
        /// <param name="resultSelector">A function to create a result value from each group.</param>
        /// <param name="source">The data-source to be grouped</param>
        /// <remarks>This will force each unique grouping key to
        /// be buffered, but not the data itself</remarks>
        public static IDataProducer<TResult> GroupBy<TSource, TKey, TElement, TResult>
            (this IDataProducer<TSource> source,
             Func<TSource, TKey> keySelector,
             Func<TSource, TElement> elementSelector,
             Func<TKey, IDataProducer<TElement>, TResult> resultSelector,
             IEqualityComparer<TKey> comparer)
        {
            source.ThrowIfNull("source");
            keySelector.ThrowIfNull("keySelector");
            elementSelector.ThrowIfNull("elementSelector");
            resultSelector.ThrowIfNull("resultSelector");
            comparer.ThrowIfNull("comparer");

            DataProducer<TResult> ret = new DataProducer<TResult>();

            Dictionary<TKey, DataProducer<TElement>> dictionary = new Dictionary<TKey, DataProducer<TElement>>(comparer);

            source.DataProduced += value =>
            {
                TKey key = keySelector(value);

                DataProducer<TElement> subProducer;

                if (!dictionary.TryGetValue(key, out subProducer))
                {
                    subProducer = new DataProducer<TElement>();
                    dictionary[key] = subProducer;
                    ret.Produce(resultSelector(key, subProducer));
                }
                subProducer.Produce(elementSelector(value));
            };

            source.EndOfData += () =>
            {
                foreach (DataProducer<TElement> value in dictionary.Values)
                {
                    value.End();
                }
                ret.End();
            };

            return ret;
        }   
    }
}
