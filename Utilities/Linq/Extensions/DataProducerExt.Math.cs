using System;
using System.Collections.Generic;
using Managed.Adb.Utilities.Extensions;
namespace Managed.Adb.Utilities.Linq.Extensions
{
    public static partial class DataProducerExt
    {
#if DOTNET35
        /// <summary>
        /// Returns a future to the sum of a sequence of values that are
        /// obtained by taking a transform of the input sequence
        /// </summary>
        /// <remarks>Null values are removed from the sum</remarks>
        public static IFuture<TResult> Sum<TSource, TResult>(this IDataProducer<TSource> source, Func<TSource, TResult> selector)
        {
            source.ThrowIfNull("source");
            selector.ThrowIfNull("selector");

            Future<TResult> ret = new Future<TResult>();
            TResult sum = Operator<TResult>.Zero;
            source.DataProduced += item =>
            {
                Operator.AddIfNotNull(ref sum, selector(item));
            };
            source.EndOfData += () => ret.Value = sum;
            return ret;

        }
        /// <summary>
        /// Returns a future to the sum of a sequence of values
        /// </summary>
        /// <remarks>Null values are removed from the sum</remarks>
        public static IFuture<TSource> Sum<TSource>(this IDataProducer<TSource> source)
        {
            return Sum<TSource, TSource>(source, x => x);
        }
        /// <summary>
        /// Returns a future to the average of a sequence of values that are
        /// obtained by taking a transform of the input sequence
        /// </summary>
        /// <remarks>Null values are removed from the average</remarks>
        public static IFuture<TResult> Average<TSource, TResult>(this IDataProducer<TSource> source, Func<TSource, TResult> selector)
        {
            source.ThrowIfNull("source");
            selector.ThrowIfNull("selector");

            Future<TResult> ret = new Future<TResult>();
            TResult sum = Operator<TResult>.Zero;
            int count = 0; // should this be long? Would demand a Operator.DivideInt64
            source.DataProduced += item =>
            {
                if (Operator.AddIfNotNull(ref sum, selector(item)))
                {
                    count++;
                }
            };
            source.EndOfData += () =>
            {
                if (count == 0)
                {
                    // check if Nullable<T> by seeing if default(T) is
                    // nullable; if so, return null; otherwise, throw
                    sum = default(TResult);
                    if (sum != null)
                    {
                        throw new InvalidOperationException("Cannot perform non-nullable average over an empty series");
                    }
                    ret.Value = sum; // null
                }
                else
                {
                    ret.Value = Operator.DivideInt32(sum, count);
                }
            };
            return ret;
        }

        /// <summary>
        /// Returns a future to the average of a sequence of values
        /// </summary>
        /// <remarks>Null values are removed from the average</remarks>
        public static IFuture<TSource> Average<TSource>(this IDataProducer<TSource> source)
        {
            return Average<TSource, TSource>(source, x => x);
        }

        #region Average special cases (Int32/Int64 to return Double)
        /// <summary>
        /// Returns a future to the average of a sequence of values
        /// </summary>
        public static IFuture<double> Average(this IDataProducer<int> source)
        {
            return Average<int, double>(source, x => x); // silent cast to double
        }
        /// <summary>
        /// Returns a future to the average of a sequence of values
        /// </summary>
        /// <remarks>Null values are removed from the average</remarks>
        public static IFuture<double?> Average(this IDataProducer<int?> source)
        {
            return Average<int?, double?>(source, x => x); // silent cast to double?
        }
        /// <summary>
        /// Returns a future to the average of a sequence of values
        /// </summary>
        public static IFuture<double> Average(this IDataProducer<long> source)
        {
            return Average<long, double>(source, x => x); // silent cast to double
        }
        /// <summary>
        /// Returns a future to the average of a sequence of values
        /// </summary>
        /// <remarks>Null values are removed from the average</remarks>
        public static IFuture<double?> Average(this IDataProducer<long?> source)
        {
            return Average<long?, double?>(source, x => x); // silent cast to double?
        }
        /// <summary>
        /// Returns a future to the average of a sequence of values that are
        /// obtained by taking a transform of the input sequence
        /// </summary>
        public static IFuture<double> Average<TSource>(this IDataProducer<TSource> source, Func<TSource, int> selector)
        {
            selector.ThrowIfNull("selector");
            return Average<TSource, double>(source, x => selector(x)); // silent cast to double
        }
        /// <summary>
        /// Returns a future to the average of a sequence of values that are
        /// obtained by taking a transform of the input sequence
        /// </summary>
        /// <remarks>Null values are removed from the average</remarks>
        public static IFuture<double?> Average<TSource>(this IDataProducer<TSource> source, Func<TSource, int?> selector)
        {
            selector.ThrowIfNull("selector");
            return Average<TSource, double?>(source, x => selector(x)); // silent cast to double?
        }
        /// <summary>
        /// Returns a future to the average of a sequence of values that are
        /// obtained by taking a transform of the input sequence
        /// </summary>
        public static IFuture<double> Average<TSource>(this IDataProducer<TSource> source, Func<TSource, long> selector)
        {
            selector.ThrowIfNull("selector");
            return Average<TSource, double>(source, x => selector(x)); // silent cast to double
        }
        /// <summary>
        /// Returns a future to the average of a sequence of values that are
        /// obtained by taking a transform of the input sequence
        /// </summary>
        /// <remarks>Null values are removed from the average</remarks>
        public static IFuture<double?> Average<TSource>(this IDataProducer<TSource> source, Func<TSource, long?> selector)
        {
            selector.ThrowIfNull("selector");
            return Average<TSource, double?>(source, x => selector(x)); // silent cast to double?
        }
        #endregion

#endif
        /// <summary>
        /// Returns a future to the maximum of a sequence of values that are
        /// obtained by taking a transform of the input sequence, using the default comparer, using the default comparer
        /// </summary>
        /// <remarks>Null values are removed from the maximum</remarks>
        public static IFuture<TResult> Max<TSource, TResult>
            (this IDataProducer<TSource> source,
             Func<TSource, TResult> selector)
        {
            source.ThrowIfNull("source");
            selector.ThrowIfNull("selector");

            return source.Select(selector).Max();
        }
        /// <summary>
        /// Returns a future to the maximum of a sequence of values, using the default comparer
        /// </summary>
        /// <remarks>Null values are removed from the maximum</remarks>
        public static IFuture<TSource> Max<TSource>(this IDataProducer<TSource> source)
        {
            source.ThrowIfNull("source");

            Future<TSource> ret = new Future<TSource>();
            IComparer<TSource> comparer = Comparer<TSource>.Default;

            TSource current = default(TSource);
            bool empty = true, canBeNull = !Operator.HasValue(current);

            source.DataProduced += value =>
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
                else if (comparer.Compare(value, current) > 0)
                {
                    current = value;
                }
            };
            source.EndOfData += () =>
            {
                // Only value types should throw an exception
                if (empty && current != null)
                {
                    throw new InvalidOperationException("Empty sequence");
                }
                ret.Value = current;
            };

            return ret;
        }
        /// <summary>
        /// Returns a future to the minumum of a sequence of values that are
        /// obtained by taking a transform of the input sequence, using the default comparer
        /// </summary>
        /// <remarks>Null values are removed from the minimum</remarks>
        public static IFuture<TResult> Min<TSource, TResult>
           (this IDataProducer<TSource> source,
            Func<TSource, TResult> selector)
        {
            source.ThrowIfNull("source");
            selector.ThrowIfNull("selector");

            return source.Select(selector).Min();
        }
        /// <summary>
        /// Returns a future to the minumum of a sequence of values, using the default comparer
        /// </summary>
        /// <remarks>Null values are removed from the minimum</remarks>
        public static IFuture<TSource> Min<TSource>(this IDataProducer<TSource> source)
        {
            source.ThrowIfNull("source");

            Future<TSource> ret = new Future<TSource>();
            IComparer<TSource> comparer = Comparer<TSource>.Default;

            TSource current = default(TSource);
            bool empty = true, canBeNull = !Operator.HasValue(current);

            source.DataProduced += value =>
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
            };
            source.EndOfData += () =>
            {
                // Only value types should throw an exception
                if (empty && current != null)
                {
                    throw new InvalidOperationException("Empty sequence");
                }
                ret.Value = current;
            };

            return ret;
        }

    }
}
