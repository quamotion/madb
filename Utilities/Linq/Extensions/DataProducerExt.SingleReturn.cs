using System;
using System.Collections.Generic;
using Managed.Adb.Utilities.Extensions;

namespace Managed.Adb.Utilities.Linq.Extensions
{
    /// <summary>
    /// Extensions on IDataProducer
    /// </summary>
    public static partial class DataProducerExt
    {
        /// <summary>
        /// Returns the number of elements in a sequence, as a future value.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <param name="source">A sequence that contains elements to be counted.</param>
        /// <returns>The number of elements in the input sequence, as a future value.
        /// The actual count can only be retrieved after the source has indicated the end
        /// of its data.
        /// </returns>
        public static IFuture<int> Count<TSource>(this IDataProducer<TSource> source)
        {
            source.ThrowIfNull("source");
            Future<int> ret = new Future<int>();
            int count=0;

            source.DataProduced += t => count++;
            source.EndOfData += () => ret.Value = count;

            return ret;
        }

        /// <summary>
        /// Returns the number of elements in the specified sequence satisfy a condition,
        /// as a future value.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <param name="source">A sequence that contains elements to be tested and counted.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <returns>A number that represents how many elements in the sequence satisfy 
        /// the condition in the predicate function, as a future value.
        /// The actual count can only be retrieved after the source has indicated the end
        /// of its data.
        /// </returns>
        public static IFuture<int> Count<TSource>(this IDataProducer<TSource> source, Func<TSource,bool> predicate)
        {
            source.ThrowIfNull("source");
            predicate.ThrowIfNull("predicate");
            Future<int> ret = new Future<int>();
            int count = 0;

            source.DataProduced += t =>
            {
                if (predicate(t))
                {
                    count++;
                }
            };
            source.EndOfData += () => ret.Value = count;

            return ret;
        }

        /// <summary>
        /// Returns the number of elements in a sequence, as a future value.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <param name="source">A sequence that contains elements to be counted.</param>
        /// <returns>The number of elements in the input sequence, as a future value.
        /// The actual count can only be retrieved after the source has indicated the end
        /// of its data.
        /// </returns>
        public static IFuture<long> LongCount<TSource>(this IDataProducer<TSource> source)
        {
            source.ThrowIfNull("source");

            Future<long> ret = new Future<long>();
            int count = 0;

            source.DataProduced += t => count++;
            source.EndOfData += () => ret.Value = count;

            return ret;
        }

        /// <summary>
        /// Returns the number of elements in the specified sequence satisfy a condition,
        /// as a future value.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <param name="source">A sequence that contains elements to be tested and counted.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <returns>A number that represents how many elements in the sequence satisfy 
        /// the condition in the predicate function, as a future value.
        /// The actual count can only be retrieved after the source has indicated the end
        /// of its data.
        /// </returns>
        public static IFuture<long> LongCount<TSource>(this IDataProducer<TSource> source, Func<TSource, bool> predicate)
        {
            source.ThrowIfNull("source");
            predicate.ThrowIfNull("predicate");

            Future<long> ret = new Future<long>();
            int count = 0;

            source.DataProduced += t =>
            {
                if (predicate(t))
                {
                    count++;
                }
            };
            source.EndOfData += () => ret.Value = count;

            return ret;
        }

        /// <summary>
        /// Returns the first element of a sequence, as a future value.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <param name="source">The sequence to return the first element of.</param>
        /// <returns>The first element in the specified sequence, as a future value.
        /// The actual value can only be retrieved after the source has indicated the end
        /// of its data.
        /// </returns>
        public static IFuture<TSource> First<TSource>(this IDataProducer<TSource> source)
        {
            return source.First(x => true);
        }

        /// <summary>
        /// Returns the first element in a sequence that satisfies a specified condition, as a future value.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <param name="source">The sequence to an element from.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <returns>The first element in the specified sequence that passes the test in 
        /// the specified predicate function, as a future value.
        /// The actual value can only be retrieved after the source has indicated the end
        /// of its data.
        /// </returns>
        public static IFuture<TSource> First<TSource>(this IDataProducer<TSource> source, Func<TSource, bool> predicate)
        {
            source.ThrowIfNull("source");
            predicate.ThrowIfNull("predicate");

            Future<TSource> ret = new Future<TSource>();

            Action completion = () => { throw new InvalidOperationException("Sequence is empty"); };
            Action<TSource> production = null;
            production = t =>
            {
                if (predicate(t))
                {
                    ret.Value = t;
                    source.EndOfData -= completion;
                    source.DataProduced -= production;
                }
            };
            source.DataProduced += production;
            source.EndOfData += completion;

            return ret;
        }

        /// <summary>
        /// Returns the last element of a sequence, as a future value.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <param name="source">The sequence to return the last element of.</param>
        /// <returns>The last element in the specified sequence, as a future value.
        /// The actual value can only be retrieved after the source has indicated the end
        /// of its data.
        /// </returns>
        public static IFuture<TSource> Last<TSource>(this IDataProducer<TSource> source)
        {
            return source.Last(x => true);
        }

        /// <summary>
        /// Returns the last element in a sequence that satisfies a specified condition, as a future value.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <param name="source">The sequence to an element from.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <returns>The last element in the specified sequence that passes the test in 
        /// the specified predicate function, as a future value.
        /// The actual value can only be retrieved after the source has indicated the end
        /// of its data.
        /// </returns>
        public static IFuture<TSource> Last<TSource>(this IDataProducer<TSource> source, Func<TSource,bool> predicate)
        {
            source.ThrowIfNull("source");
            predicate.ThrowIfNull("predicate");

            Future<TSource> ret = new Future<TSource>();

            bool gotData = false;
            TSource prev = default(TSource);

            source.DataProduced += value => 
            { 
                if (predicate(value)) 
                { 
                    prev = value;
                    gotData = true; 
                } 
            };
            source.EndOfData += () => 
            {
                if (!gotData)
                {
                    throw new InvalidOperationException("Sequence is empty");
                }
                ret.Value = prev;
            };

            return ret;
        }
        /// <summary>
        /// Returns a future to the first value from a sequence, or the default for that type
        /// if no value is produced.
        /// </summary>
        /// <param name="source">The source data-producer.</param>
        public static IFuture<TSource> FirstOrDefault<TSource>(this IDataProducer<TSource> source)
        {
            return source.FirstOrDefault(x => true);
        }
        /// <summary>
        /// Returns a future to the first value from a sequence that matches the given condition, or the default
        /// for that type if no matching value is produced.
        /// </summary>
        /// <param name="source">The source data-producer.</param>
        /// <param name="predicate">The condition to be satisfied.</param>
        public static IFuture<TSource> FirstOrDefault<TSource>(this IDataProducer<TSource> source, Func<TSource, bool> predicate)
        {
            source.ThrowIfNull("source");
            predicate.ThrowIfNull("predicate");

            Future<TSource> ret = new Future<TSource>();

            Action completion = () => ret.Value=default(TSource);
            Action<TSource> production = null;
            production = t =>
            {
                if (predicate(t))
                {
                    ret.Value = t;
                    source.EndOfData -= completion;
                    source.DataProduced -= production;
                }
            };
            source.DataProduced += production;
            source.EndOfData += completion;

            return ret;
        }
        /// <summary>
        /// Returns a future to the last value from a sequence, or the default for that type
        /// if no value is produced.
        /// </summary>
        /// <param name="source">The source data-producer.</param>
        public static IFuture<TSource> LastOrDefault<TSource>(this IDataProducer<TSource> source)
        {
            return source.LastOrDefault(x => true);
        }
        /// <summary>
        /// Returns the last value from a sequence that matches the given condition, or the default
        /// for that type if no matching value is produced.
        /// </summary>
        /// <param name="source">The source data-producer.</param>
        /// <param name="predicate">The condition to be satisfied.</param>
        public static IFuture<TSource> LastOrDefault<TSource>(this IDataProducer<TSource> source, Func<TSource,bool> predicate)
        {
            source.ThrowIfNull("source");
            predicate.ThrowIfNull("predicate");

            Future<TSource> ret = new Future<TSource>();

            TSource prev = default(TSource);

            source.DataProduced += value =>
            {
                if (predicate(value))
                {
                    prev = value;
                }
            };
            source.EndOfData += () =>
            {
                ret.Value = prev;
            };

            return ret;
        }
        /// <summary>
        /// Returns a future to a single value from a data-source; an exception
        /// is thrown if no values, or multiple values, are encountered.
        /// </summary>
        /// <param name="source">The source data-producer.</param>
        /// <exception cref="InvalidOperationException">Zero or multiple terms are encountered.</exception>
        public static IFuture<TSource> Single<TSource>(this IDataProducer<TSource> source)
        {
            return source.Single(x => true);
        }

        /// <summary>
        /// Returns a future to a single value from a data-source that matches the
        /// specified condition; an exception
        /// is thrown if no matching values, or multiple matching values, are encountered.
        /// </summary>
        /// <param name="source">The source data-producer.</param>
        /// <param name="predicate">The condition to be satisfied.</param>
        /// <exception cref="InvalidOperationException">Zero or multiple matching terms are encountered.</exception>
        public static IFuture<TSource> Single<TSource>(this IDataProducer<TSource> source, Func<TSource,bool> predicate)
        {
            source.ThrowIfNull("source");
            predicate.ThrowIfNull("predicate");

            Future<TSource> ret = new Future<TSource>();

            TSource output = default(TSource);
            bool gotValue = false;

            source.DataProduced += value =>
            {
                if (predicate(value))
                {
                    if (gotValue)
                    {
                        throw new InvalidOperationException("More than one element in source data");
                    }
                    output = value;
                    gotValue = true;
                }
            };

            source.EndOfData += () =>
            {
                if (!gotValue)
                {
                    throw new InvalidOperationException("No elements in source data");
                }
                ret.Value = output;
            };

            return ret;
        }
        /// <summary>
        /// Returns a future to a single value from a data-source or the default value if no values
        /// are encountered. An exception
        /// is thrown if multiple values are encountered.
        /// </summary>
        /// <param name="source">The source data-producer.</param>
        /// <exception cref="InvalidOperationException">Multiple terms are encountered.</exception>
        public static IFuture<TSource> SingleOrDefault<TSource>(this IDataProducer<TSource> source)
        {
            return source.SingleOrDefault(x => true);
        }
        /// <summary>
        /// Returns a future to a single value from a data-source that matches the
        /// specified condition, or the default value if no matching values
        /// are encountered. An exception
        /// is thrown if multiple matching values are encountered.
        /// </summary>
        /// <param name="source">The source data-producer.</param>
        /// <param name="predicate">The condition to be satisfied.</param>
        /// <exception cref="InvalidOperationException">Multiple matching terms are encountered.</exception>
        public static IFuture<TSource> SingleOrDefault<TSource>(this IDataProducer<TSource> source, Func<TSource, bool> predicate)
        {
            source.ThrowIfNull("source");
            predicate.ThrowIfNull("predicate");

            Future<TSource> ret = new Future<TSource>();

            TSource output = default(TSource);
            bool gotValue = false;

            source.DataProduced += value =>
            {
                if (predicate(value))
                {
                    if (gotValue)
                    {
                        throw new InvalidOperationException("More than one element in source data");
                    }
                    output = value;
                    gotValue = true;
                }
            };

            source.EndOfData += () =>
            {
                ret.Value = output;
            };

            return ret;
        }
        /// <summary>
        /// Returns a future to the element at the given position in the sequence
        /// </summary>
        /// <param name="source">The data-producer</param>
        /// <param name="index">The index of the desired trem in the sequence</param>
        /// <exception cref="ArgumentOutOfRangeException">If the specified index is negative
        /// or is never reached</exception>
        public static IFuture<TSource> ElementAt<TSource>(this IDataProducer<TSource> source, int index)
        {
            source.ThrowIfNull("source");
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            Future<TSource> ret = new Future<TSource>();

            Action completion = () => { throw new ArgumentOutOfRangeException("Specified index never reached"); };
            Action<TSource> production = null;
            production = value =>
            {
                if (index == 0)
                {
                    ret.Value = value;
                    source.DataProduced -= production;
                    source.EndOfData -= completion;
                }
                else
                {
                    index--;
                }
            };

            source.DataProduced += production;
            source.EndOfData += completion;

            return ret;
        }
        /// <summary>
        /// Returns a future to the element at the given position in the sequence,
        /// or the default-value if the specified index is never reached
        /// </summary>
        /// <param name="source">The data-producer</param>
        /// <param name="index">The index of the desired trem in the sequence</param>
        /// <exception cref="ArgumentOutOfRangeException">If the specified index is negative</exception>
        public static IFuture<TSource> ElementAtOrDefault<TSource>(this IDataProducer<TSource> source, int index)
        {
            source.ThrowIfNull("source");
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            Future<TSource> ret = new Future<TSource>();

            Action completion = () => ret.Value = default(TSource);
            Action<TSource> production = null;
            production = value =>
            {
                if (index == 0)
                {
                    ret.Value = value;
                    source.DataProduced -= production;
                    source.EndOfData -= completion;
                }
                else
                {
                    index--;
                }
            };

            source.DataProduced += production;
            source.EndOfData += completion;

            return ret;
        }

        /// <summary>
        /// Returns a future that indicates whether all values
        /// yielded by the data-producer satisfy a given condition.
        /// The future will return true for an empty sequence or
        /// where all values satisfy the condition, else false
        /// (if any value value fails to satisfy the condition).
        /// </summary>
        /// <param name="source">The data-producer to be monitored.</param>
        /// <param name="predicate">The condition that must be satisfied by all terms.</param>
        public static IFuture<bool> All<TSource>(this IDataProducer<TSource> source, Func<TSource, bool> predicate)
        {
            predicate.ThrowIfNull("predicate");

            return FutureProxy<bool>.FromFuture(source.Any(value => !predicate(value)), value => !value);
        }
        /// <summary>
        /// Returns a future that indicates whether any values are
        /// yielded by the data-producer. The future will return false
        /// for an empty sequence, or true for a sequence with values.
        /// </summary>
        /// <param name="source">The data-producer to be monitored.</param>
        public static IFuture<bool> Any<TSource>(this IDataProducer<TSource> source)
        {
            source.ThrowIfNull("source");

            Future<bool> ret = new Future<bool>();

            Action<TSource> production = null;
            Action completion = () => ret.Value = false;
            production = value =>
            { 
                ret.Value = true;
                source.DataProduced -= production; 
                source.EndOfData -= completion;
            };

            source.DataProduced += production;
            source.EndOfData += completion;

            return ret;
        }
        /// <summary>
        /// Returns a future that indicates whether any suitable values are
        /// yielded by the data-producer. The future will return false
        /// for an empty sequence or one with no matching values, or true for a sequence with matching values.
        /// </summary>
        /// <param name="source">The data-producer to be monitored.</param>
        /// <param name="predicate">The condition that must be satisfied.</param>
        public static IFuture<bool> Any<TSource>(this IDataProducer<TSource> source, Func<TSource, bool> predicate)
        {
            source.ThrowIfNull("source");
            predicate.ThrowIfNull("predicate");

            Future<bool> ret = new Future<bool>();

            Action<TSource> production = null;
            Action completion = () => ret.Value = false;
            production = value =>
            {
                if (predicate(value))
                {
                    ret.Value = true;
                    source.DataProduced -= production;
                    source.EndOfData -= completion;
                }
            };

            source.DataProduced += production;
            source.EndOfData += completion;

            return ret;
        }
        /// <summary>
        /// Returns a future to indicate whether the specified value
        /// is yielded by the data-source.
        /// </summary>
        /// <typeparam name="TSource">The type of data to be yielded</typeparam>
        /// <param name="source">The data-source</param>
        /// <param name="value">The value to detect from the data-source, checked with the default comparer</param>
        public static IFuture<bool> Contains<TSource>(this IDataProducer<TSource> source, TSource value)
        {
            return source.Contains(value, EqualityComparer<TSource>.Default);
        }
        /// <summary>
        /// Returns a future to indicate whether the specified value
        /// is yielded by the data-source.
        /// </summary>
        /// <typeparam name="TSource">The type of data to be yielded</typeparam>
        /// <param name="source">The data-source</param>
        /// <param name="value">The value to detect from the data-source</param>
        /// <param name="comparer">The comparer to use to determine equality</param>
        public static IFuture<bool> Contains<TSource>(this IDataProducer<TSource> source, TSource value, IEqualityComparer<TSource> comparer)
        {
            source.ThrowIfNull("source");
            comparer.ThrowIfNull("comparer");

            return source.Any(element => comparer.Equals(value, element));
        }
        /// <summary>
        /// Applies an accumulator function over the values yielded from
        /// a data-producer. The first value in the seqnence
        /// is used as the initial accumulator value, and the specified function is used
        /// to select the result value. If the sequence is empty then
        /// the default value for TSource is returned.
        /// </summary>
        /// <typeparam name="TSource">The type of data yielded by the data-source</typeparam>
        /// <param name="func">Accumulator function to be applied to each term in the sequence</param>
        /// <param name="source">The data-source for the values</param>
        public static IFuture<TSource> Aggregate<TSource>
            (this IDataProducer<TSource> source,
             Func<TSource, TSource, TSource> func)
        {
            source.ThrowIfNull("source");
            func.ThrowIfNull("func");

            Future<TSource> ret = new Future<TSource>();
            bool first = true;

            TSource current = default(TSource);

            source.DataProduced += value =>
            {
                if (first)
                {
                    first = false;
                    current = value;
                }
                else
                {
                    current = func(current, value);
                }
            };
            source.EndOfData += () => ret.Value = current;

            return ret;
        }
        /// <summary>
        /// Applies an accumulator function over the values yielded from
        /// a data-producer. The specified seed value
        /// is used as the initial accumulator value, and the specified function is used
        /// to select the result value
        /// </summary>
        /// <typeparam name="TSource">The type of data yielded by the data-source</typeparam>
        /// <typeparam name="TAccumulate">The type to be used for the accumulator</typeparam>
        /// <param name="func">Accumulator function to be applied to each term in the sequence</param>
        /// <param name="seed">The initial value for the accumulator</param>
        /// <param name="source">The data-source for the values</param>
        public static IFuture<TAccumulate> Aggregate<TSource, TAccumulate>
            (this IDataProducer<TSource> source,
	         TAccumulate seed,
	         Func<TAccumulate, TSource, TAccumulate> func)
        {
            return source.Aggregate(seed, func, x => x);
        }
        /// <summary>
        /// Applies an accumulator function over the values yielded from
        /// a data-producer, performing a transformation on the final
        /// accululated value. The specified seed value
        /// is used as the initial accumulator value, and the specified function is used
        /// to select the result value
        /// </summary>
        /// <typeparam name="TSource">The type of data yielded by the data-source</typeparam>
        /// <typeparam name="TResult">The final result type (after the accumulator has been transformed)</typeparam>
        /// <typeparam name="TAccumulate">The type to be used for the accumulator</typeparam>
        /// <param name="func">Accumulator function to be applied to each term in the sequence</param>
        /// <param name="resultSelector">Transformation to apply to the final
        /// accumulated value to produce the result</param>
        /// <param name="seed">The initial value for the accumulator</param>
        /// <param name="source">The data-source for the values</param>
        public static IFuture<TResult> Aggregate<TSource, TAccumulate, TResult>
            (this IDataProducer<TSource> source,
             TAccumulate seed,
             Func<TAccumulate, TSource, TAccumulate> func,
             Func<TAccumulate, TResult> resultSelector)
        {
            source.ThrowIfNull("source");
            func.ThrowIfNull("func");
            resultSelector.ThrowIfNull("resultSelector");

            Future<TResult> result = new Future<TResult>();

            TAccumulate current = seed;

            source.DataProduced += value => current = func(current, value);
            source.EndOfData += () => result.Value = resultSelector(current);

            return result;
        }
    }
}
