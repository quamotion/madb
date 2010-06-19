using System;
using System.Collections.Generic;

using Managed.Adb.Utilities.Extensions;

namespace Managed.Adb.Utilities.Collections
{
    /// <summary>
    /// Represents a range of values. An IComparer{T} is used to compare specific
    /// values with a start and end point. A range may be include or exclude each end
    /// individually.
    /// 
    /// A range which is half-open but has the same start and end point is deemed to be empty,
    /// e.g. [3,3) doesn't include 3. To create a range with a single value, use an inclusive
    /// range, e.g. [3,3].
    /// 
    /// Ranges are always immutable - calls such as IncludeEnd() and ExcludeEnd() return a new
    /// range without modifying this one.
    /// </summary>
    public sealed class Range<T>
    {
        readonly T start;
        /// <summary>
        /// The start of the range.
        /// </summary>
        public T Start
        {
            get { return start; }
        }

        readonly T end;
        /// <summary>
        /// The end of the range.
        /// </summary>
        public T End
        {
            get { return end; }
        }

        readonly IComparer<T> comparer;
        /// <summary>
        /// Comparer to use for comparisons
        /// </summary>
        public IComparer<T> Comparer
        {
            get { return comparer; }
        }

        readonly bool includesStart;
        /// <summary>
        /// Whether or not this range includes the start point
        /// </summary>
        public bool IncludesStart
        {
            get { return includesStart; }
        }

        readonly bool includesEnd;
        /// <summary>
        /// Whether or not this range includes the end point
        /// </summary>
        public bool IncludesEnd
        {
            get { return includesEnd; }
        }

        /// <summary>
        /// Constructs a new inclusive range using the default comparer
        /// </summary>
        public Range(T start, T end)
            : this(start, end, Comparer<T>.Default, true, true)
        {
        }

        /// <summary>
        /// Constructs a new range including both ends using the specified comparer
        /// </summary>
        public Range(T start, T end, IComparer<T> comparer)
            : this(start, end, comparer, true, true)
        {
        }

        /// <summary>
        /// Constructs a new range, including or excluding each end as specified,
        /// with the given comparer.
        /// </summary>
        public Range(T start, T end, IComparer<T> comparer, bool includeStart, bool includeEnd)
        {
            if (comparer.Compare(start, end) > 0)
            {
                throw new ArgumentOutOfRangeException("end", "start must be lower than end according to comparer");
            }

            this.start = start;
            this.end = end;
            this.comparer = comparer;
            this.includesStart = includeStart;
            this.includesEnd = includeEnd;
        }

        /// <summary>
        /// Returns a range with the same boundaries as this, but excluding the end point.
        /// When called on a range already excluding the end point, the original range is returned.
        /// </summary>
        public Range<T> ExcludeEnd()
        {
            if (!includesEnd)
            {
                return this;
            }
            return new Range<T>(start, end, comparer, includesStart, false);
        }

        /// <summary>
        /// Returns a range with the same boundaries as this, but excluding the start point.
        /// When called on a range already excluding the start point, the original range is returned.
        /// </summary>
        public Range<T> ExcludeStart()
        {
            if (!includesStart)
            {
                return this;
            }
            return new Range<T>(start, end, comparer, false, includesEnd);
        }

        /// <summary>
        /// Returns a range with the same boundaries as this, but including the end point.
        /// When called on a range already including the end point, the original range is returned.
        /// </summary>
        public Range<T> IncludeEnd()
        {
            if (includesEnd)
            {
                return this;
            }
            return new Range<T>(start, end, comparer, includesStart, true);
        }

        /// <summary>
        /// Returns a range with the same boundaries as this, but including the start point.
        /// When called on a range already including the start point, the original range is returned.
        /// </summary>
        public Range<T> IncludeStart()
        {
            if (includesStart)
            {
                return this;
            }
            return new Range<T>(start, end, comparer, true, includesEnd);
        }

        /// <summary>
        /// Returns whether or not the range contains the given value
        /// </summary>
        public bool Contains(T value)
        {
            int lowerBound = comparer.Compare(value, start);
            if (lowerBound < 0 || (lowerBound == 0 && !includesStart))
            {
                return false;
            }
            int upperBound = comparer.Compare(value, end);
            return upperBound < 0 || (upperBound == 0 && includesEnd);
        }

        /// <summary>
        /// Returns an iterator which begins at the start of this range,
        /// applying the given step delegate on each iteration until the 
        /// end is reached or passed. The start and end points are included
        /// or excluded according to this range.
        /// </summary>
        /// <param name="step">Delegate to apply to the "current value" on each iteration</param>
        public RangeIterator<T> FromStart(Func<T, T> step)
        {
            return new RangeIterator<T>(this, step);
        }

        /// <summary>
        /// Returns an iterator which begins at the end of this range,
        /// applying the given step delegate on each iteration until the 
        /// start is reached or passed. The start and end points are included
        /// or excluded according to this range.
        /// </summary>
        /// <param name="step">Delegate to apply to the "current value" on each iteration</param>
        public RangeIterator<T> FromEnd(Func<T, T> step)
        {
            return new RangeIterator<T>(this, step, false);
        }

#if DOTNET35
        /// <summary>
        /// Returns an iterator which begins at the start of this range,
        /// adding the given step amount to the current value each iteration until the 
        /// end is reached or passed. The start and end points are included
        /// or excluded according to this range. This method does not check for
        /// the availability of an addition operator at compile-time; if you use it
        /// on a range where there is no such operator, it will fail at execution time.
        /// </summary>
        /// <param name="stepAmount">Amount to add on each iteration</param>
        public RangeIterator<T> UpBy(T stepAmount)
        {
            return new RangeIterator<T>(this, t => Operator.Add(t, stepAmount));
        }

        /// <summary>
        /// Returns an iterator which begins at the end of this range,
        /// subtracting the given step amount to the current value each iteration until the 
        /// start is reached or passed. The start and end points are included
        /// or excluded according to this range. This method does not check for
        /// the availability of a subtraction operator at compile-time; if you use it
        /// on a range where there is no such operator, it will fail at execution time.
        /// </summary>
        /// <param name="stepAmount">Amount to subtract on each iteration. Note that
        /// this is subtracted, so in a range [0,10] you would pass +2 as this parameter
        /// to obtain the sequence (10, 8, 6, 4, 2, 0).
        /// </param>
        public RangeIterator<T> DownBy(T stepAmount)
        {
            return new RangeIterator<T>(this, t => Operator.Subtract(t, stepAmount), false);
        }

        /// <summary>
        /// Returns an iterator which begins at the start of this range,
        /// adding the given step amount to the current value each iteration until the 
        /// end is reached or passed. The start and end points are included
        /// or excluded according to this range. This method does not check for
        /// the availability of an addition operator at compile-time; if you use it
        /// on a range where there is no such operator, it will fail at execution time.
        /// </summary>
        /// <param name="stepAmount">Amount to add on each iteration</param>
        public RangeIterator<T> UpBy<TAmount>(TAmount stepAmount)
        {
            return new RangeIterator<T>(this, t => Operator.AddAlternative(t, stepAmount));
        }

        /// <summary>
        /// Returns an iterator which begins at the end of this range,
        /// subtracting the given step amount to the current value each iteration until the 
        /// start is reached or passed. The start and end points are included
        /// or excluded according to this range. This method does not check for
        /// the availability of a subtraction operator at compile-time; if you use it
        /// on a range where there is no such operator, it will fail at execution time.
        /// </summary>
        /// <param name="stepAmount">Amount to subtract on each iteration. Note that
        /// this is subtracted, so in a range [0,10] you would pass +2 as this parameter
        /// to obtain the sequence (10, 8, 6, 4, 2, 0).
        /// </param>
        public RangeIterator<T> DownBy<TAmount>(TAmount stepAmount)
        {
            return new RangeIterator<T>(this, t => Operator.SubtractAlternative(t, stepAmount), false);
        }
#endif

        /// <summary>
        /// Returns an iterator which steps through the range, applying the specified
        /// step delegate on each iteration. The method determines whether to begin 
        /// at the start or end of the range based on whether the step delegate appears to go
        /// "up" or "down". The step delegate is applied to the start point. If the result is 
        /// more than the start point, the returned iterator begins at the start point; otherwise
        /// it begins at the end point.
        /// </summary>
        /// <param name="step">Delegate to apply to the "current value" on each iteration</param>
        public RangeIterator<T> Step(Func<T, T> step)
        {
            step.ThrowIfNull("step");
            bool ascending = (comparer.Compare(start, step(start)) < 0);

            return ascending ? FromStart(step) : FromEnd(step);
        }

#if DOTNET35
        /// <summary>
        /// Returns an iterator which steps through the range, adding the specified amount
        /// on each iteration. If the step amount is logically negative, the returned iterator
        /// begins at the start point; otherwise it begins at the end point.
        /// This method does not check for
        /// the availability of an addition operator at compile-time; if you use it
        /// on a range where there is no such operator, it will fail at execution time.
        /// </summary>
        /// <param name="stepAmount">The amount to add on each iteration</param>
        public RangeIterator<T> Step(T stepAmount)
        {
            return Step(t => Operator.Add(t, stepAmount));
        }

        /// <summary>
        /// Returns an iterator which steps through the range, adding the specified amount
        /// on each iteration. If the step amount is logically negative, the returned iterator
        /// begins at the end point; otherwise it begins at the start point. This method
        /// is equivalent to Step(T stepAmount), but allows an alternative type to be used.
        /// The most common example of this is likely to be stepping a range of DateTimes
        /// by a TimeSpan.
        /// This method does not check for
        /// the availability of a suitable addition operator at compile-time; if you use it
        /// on a range where there is no such operator, it will fail at execution time.
        /// </summary>
        /// <param name="stepAmount">The amount to add on each iteration</param>
        public RangeIterator<T> Step<TAmount>(TAmount stepAmount)
        {
            return Step(t => Operator.AddAlternative(t, stepAmount));
        }
#endif
    }
}
