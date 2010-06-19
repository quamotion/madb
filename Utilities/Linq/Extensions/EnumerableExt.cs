#if DOTNET35
using System;
using System.Collections.Generic;
using System.Linq;
using Managed.Adb.Utilities.Extensions;

namespace Managed.Adb.Utilities.Linq.Extensions
{
    /// <summary>
    /// Further extensions to IEnumerable{T}.
    /// </summary>
    public static class EnumerableExt
    {
        /// <summary>
        /// Groups and executes a pipeline for a single result per group
        /// </summary>
        public static IEnumerable<KeyValueTuple<TKey, TResult>> GroupWithPipeline<TElement, TKey, TResult>
            (this IEnumerable<TElement> source,
             Func<TElement, TKey> keySelector,
             Func<IDataProducer<TElement>, IFuture<TResult>> pipeline)
        {
            return source.GroupWithPipeline(keySelector, EqualityComparer<TKey>.Default, pipeline);
        }
        
        /// <summary>
        /// Groups and executes a pipeline for a single result per group
        /// </summary>
        public static IEnumerable<KeyValueTuple<TKey, TResult>> GroupWithPipeline<TElement, TKey, TResult>
            (this IEnumerable<TElement> source,
             Func<TElement, TKey> keySelector,
             IEqualityComparer<TKey> comparer,
             Func<IDataProducer<TElement>, IFuture<TResult>> pipeline)
        {
            var keyMap = new Dictionary<TKey, DataProducer<TElement>>(comparer);
            var results = new List<KeyValueTuple<TKey, IFuture<TResult>>>();

            foreach (TElement element in source)
            {
                TKey key = keySelector(element);
                DataProducer<TElement> producer;
                if (!keyMap.TryGetValue(key, out producer))
                {
                    producer = new DataProducer<TElement>();
                    keyMap[key] = producer;
                    results.Add (new KeyValueTuple<TKey,IFuture<TResult>>(key, pipeline(producer)));
                }
                producer.Produce(element);
            }

            foreach (var producer in keyMap.Values)
            {
                producer.End();
            }

            foreach (var result in results)
            {
                yield return new KeyValueTuple<TKey, TResult>(result.Key, result.Value.Value);
            }
        }

        /// <summary>
        /// Groups and executes a pipeline for two results per group
        /// </summary>
        public static IEnumerable<KeyValueTuple<TKey, TResult1, TResult2>> GroupWithPipeline<TElement, TKey, TResult1, TResult2>
            (this IEnumerable<TElement> source,
             Func<TElement, TKey> keySelector,
             Func<IDataProducer<TElement>, IFuture<TResult1>> pipeline1,
             Func<IDataProducer<TElement>, IFuture<TResult2>> pipeline2)
        {
            return source.GroupWithPipeline(keySelector, EqualityComparer<TKey>.Default, pipeline1, pipeline2);
        }

        /// <summary>
        /// Groups and executes a pipeline for two results per group
        /// </summary>
        public static IEnumerable<KeyValueTuple<TKey,TResult1,TResult2>> GroupWithPipeline<TElement,TKey,TResult1,TResult2>
            (this IEnumerable<TElement> source,
             Func<TElement, TKey> keySelector,
             IEqualityComparer<TKey> comparer,
             Func<IDataProducer<TElement>, IFuture<TResult1>> pipeline1,
             Func<IDataProducer<TElement>, IFuture<TResult2>> pipeline2)
        {
            var keyMap = new Dictionary<TKey, DataProducer<TElement>>(comparer);
            var results = new List<KeyValueTuple<TKey, IFuture<TResult1>, IFuture<TResult2>>>();

            foreach (TElement element in source)
            {
                TKey key = keySelector(element);
                DataProducer<TElement> producer;
                if (!keyMap.TryGetValue(key, out producer))
                {
                    producer = new DataProducer<TElement>();
                    keyMap[key] = producer;
                    results.Add(new KeyValueTuple<TKey, IFuture<TResult1>, IFuture<TResult2>>(key, pipeline1(producer), pipeline2(producer)));
                }
                producer.Produce(element);
            }

            foreach (var producer in keyMap.Values)
            {
                producer.End();
            }

            foreach (var result in results)
            {
                yield return new KeyValueTuple<TKey, TResult1, TResult2>(result.Key, result.Value1.Value, result.Value2.Value);
            }
        }

        /// <summary>
        /// Groups and executes a pipeline for three results per group
        /// </summary>
        public static IEnumerable<KeyValueTuple<TKey,TResult1,TResult2,TResult3>> GroupWithPipeline<TElement, TKey, TResult1, TResult2, TResult3>
            (this IEnumerable<TElement> source,
             Func<TElement, TKey> keySelector,
             Func<IDataProducer<TElement>, IFuture<TResult1>> pipeline1,
             Func<IDataProducer<TElement>, IFuture<TResult2>> pipeline2,
             Func<IDataProducer<TElement>, IFuture<TResult3>> pipeline3)
        {
            return source.GroupWithPipeline(keySelector, EqualityComparer<TKey>.Default, pipeline1, pipeline2, pipeline3);
        }


        /// <summary>
        /// Groups and executes a pipeline for three results per group
        /// </summary>
        public static IEnumerable<KeyValueTuple<TKey, TResult1, TResult2, TResult3>> GroupWithPipeline<TElement, TKey, TResult1, TResult2, TResult3>
            (this IEnumerable<TElement> source,
             Func<TElement, TKey> keySelector,
             IEqualityComparer<TKey> comparer,
             Func<IDataProducer<TElement>, IFuture<TResult1>> pipeline1,
             Func<IDataProducer<TElement>, IFuture<TResult2>> pipeline2,
             Func<IDataProducer<TElement>, IFuture<TResult3>> pipeline3)
        {
            var keyMap = new Dictionary<TKey, DataProducer<TElement>>(comparer);
            var results = new List<KeyValueTuple<TKey, IFuture<TResult1>, IFuture<TResult2>, IFuture<TResult3>>>();

            foreach (TElement element in source)
            {
                TKey key = keySelector(element);
                DataProducer<TElement> producer;
                if (!keyMap.TryGetValue(key, out producer))
                {
                    producer = new DataProducer<TElement>();
                    keyMap[key] = producer;
                    results.Add(new KeyValueTuple<TKey, IFuture<TResult1>, IFuture<TResult2>, IFuture<TResult3>>(key, pipeline1(producer), pipeline2(producer), pipeline3(producer)));
                }
                producer.Produce(element);
            }

            foreach (var producer in keyMap.Values)
            {
                producer.End();
            }

            foreach (var result in results)
            {
                yield return new KeyValueTuple<TKey,TResult1,TResult2,TResult3>(result.Key, result.Value1.Value, result.Value2.Value, result.Value3.Value);
            }
        }

        /// <summary>
        /// Groups and executes a pipeline for four results per group
        /// </summary>
        public static IEnumerable<KeyValueTuple<TKey, TResult1, TResult2, TResult3, TResult4>> GroupWithPipeline<TElement, TKey, TResult1, TResult2, TResult3, TResult4>
            (this IEnumerable<TElement> source,
             Func<TElement, TKey> keySelector,
             Func<IDataProducer<TElement>, IFuture<TResult1>> pipeline1,
             Func<IDataProducer<TElement>, IFuture<TResult2>> pipeline2,
             Func<IDataProducer<TElement>, IFuture<TResult3>> pipeline3,
             Func<IDataProducer<TElement>, IFuture<TResult4>> pipeline4)
        {
            return source.GroupWithPipeline(keySelector, EqualityComparer<TKey>.Default, pipeline1, pipeline2, pipeline3, pipeline4);
        }

        /// <summary>
        /// Groups and executes a pipeline for four results per group
        /// </summary>
        public static IEnumerable<KeyValueTuple<TKey, TResult1, TResult2, TResult3, TResult4>> GroupWithPipeline<TElement, TKey, TResult1, TResult2, TResult3, TResult4>
            (this IEnumerable<TElement> source,
             Func<TElement, TKey> keySelector,
             IEqualityComparer<TKey> comparer,
             Func<IDataProducer<TElement>, IFuture<TResult1>> pipeline1,
             Func<IDataProducer<TElement>, IFuture<TResult2>> pipeline2,
             Func<IDataProducer<TElement>, IFuture<TResult3>> pipeline3,
             Func<IDataProducer<TElement>, IFuture<TResult4>> pipeline4)
        {
            var keyMap = new Dictionary<TKey, DataProducer<TElement>>(comparer);
            var results = new List<KeyValueTuple<TKey, IFuture<TResult1>, IFuture<TResult2>, IFuture<TResult3>, IFuture<TResult4>>>();

            foreach (TElement element in source)
            {
                TKey key = keySelector(element);
                DataProducer<TElement> producer;
                if (!keyMap.TryGetValue(key, out producer))
                {
                    producer = new DataProducer<TElement>();
                    keyMap[key] = producer;
                    results.Add(new KeyValueTuple<TKey, IFuture<TResult1>, IFuture<TResult2>, IFuture<TResult3>, IFuture<TResult4>>(key, pipeline1(producer), pipeline2(producer), pipeline3(producer), pipeline4(producer)));
                }
                producer.Produce(element);
            }

            foreach (var producer in keyMap.Values)
            {
                producer.End();
            }

            foreach (var result in results)
            {
                yield return new KeyValueTuple<TKey, TResult1, TResult2, TResult3, TResult4>(result.Key, result.Value1.Value, result.Value2.Value, result.Value3.Value, result.Value4.Value);
            }
        }
        /// <summary>
        /// Computes the sum of a sequence of values
        /// </summary>
        /// <remarks>The values in the sequence must support the Add operator</remarks>
        public static TSource Sum<TSource>(this IEnumerable<TSource> source)
        {
            return Sum(source, x => x);
        }
        /// <summary>
        /// Computes the sum of the sequence of values that are
        /// obtained by invoking a transform function on each element of the input sequence.
        /// </summary>
        /// <remarks>The values returned by the transform function must support the Add operator</remarks>
        public static TValue Sum<TSource, TValue>(this IEnumerable<TSource> source, Func<TSource,TValue> selector)
        {
            source.ThrowIfNull("source");
            selector.ThrowIfNull("selector");
            TValue sum = Operator<TValue>.Zero; // not the same as default(T); think "int?"
            foreach (TSource item in source)
            {
                Operator.AddIfNotNull(ref sum, selector(item));
            }
            return sum;
        }

        /// <summary>
        /// Computes the mean average of a sequence of values
        /// </summary>
        /// <remarks>The values in the sequence must support the Add and Divide(Int32) operators</remarks>
        public static TSource Average<TSource>(this IEnumerable<TSource> source)
        {
            return Average(source, x => x);
        }
        /// <summary>
        /// Computes the mean average of the sequence of values that are
        /// obtained by invoking a transform function on each element of the input sequence.
        /// </summary>
        /// <remarks>The values returned by the transform function must support the Add and Divide(Int32) operators</remarks>
        public static TValue Average<TSource, TValue>(this IEnumerable<TSource> source, Func<TSource, TValue> selector)
        {
            source.ThrowIfNull("source");
            selector.ThrowIfNull("selector");
            int count = 0;
            TValue sum = Operator<TValue>.Zero; // not the same as default(T); think "int?"
            foreach (TSource item in source)
            {
                if (Operator.AddIfNotNull(ref sum, selector(item)))
                {
                    count++;
                }
            }
            if (count == 0)
            {
                sum = default(TValue);
                if (sum != null)
                {
                    throw new InvalidOperationException("Cannot perform non-nullable average over an empty series");
                }
                return sum;
            }
            else
            {
                return Operator.DivideInt32(sum, count);
            }
        }
        /// <summary>
        /// Computes the maximum (using a custom comparer) of a sequence of values.
        /// </summary>
        public static TSource Max<TSource>(this IEnumerable<TSource> source, IComparer<TSource> comparer)
        {
            TSource current = default(TSource);
            bool empty = true, canBeNull = !Operator.HasValue(current);

            foreach(TSource value in source) 
            {
                if (canBeNull && !Operator.HasValue(value))
                {
                    // NOP
                } 
                else if(empty)
                {
                    current = value;
                    empty = false;
                } 
                else if(comparer.Compare(value, current) > 0) 
                {
                    current = value;
                }
            }
            if (empty && current != null)
            {
                throw new InvalidOperationException("Empty sequence");
            }
            return current;
        }
        /// <summary>
        /// Computes the maximum (using a custom comparer) of the sequence of values that are
        /// obtained by invoking a transform function on each element of the input sequence.
        /// </summary>
        public static TValue Max<TSource, TValue>(this IEnumerable<TSource> source, Func<TSource, TValue> selector, IComparer<TValue> comparer)
        {
            return source.Select(selector).Max(comparer);
        }
        /// <summary>
        /// Computes the minimum (using a custom comparer) of a sequence of values.
        /// </summary>
        public static TSource Min<TSource>(this IEnumerable<TSource> source, IComparer<TSource> comparer)
        {
            TSource current = default(TSource);
            bool empty = true, canBeNull = !Operator.HasValue(current);

            foreach (TSource value in source)
            {
                if (canBeNull && !Operator.HasValue(value))
                {
                    // NOP
                } 
                else if (empty)
                {
                    current = value;
                    empty = false;
                }
                else if (comparer.Compare(value, current) < 0)
                {
                    current = value;
                }
            }
            if (empty && current != null)
            {
                throw new InvalidOperationException("Empty sequence");
            }
            return current;
        }
        /// <summary>
        /// Computes the minimum (using a custom comparer) of the sequence of values that are
        /// obtained by invoking a transform function on each element of the input sequence.
        /// </summary>
        public static TValue Min<TSource, TValue>(this IEnumerable<TSource> source, Func<TSource, TValue> selector, IComparer<TValue> comparer)
        {
            return source.Select(selector).Min(comparer);
        }
    }
}
#endif