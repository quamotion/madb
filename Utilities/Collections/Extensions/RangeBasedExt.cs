
namespace Managed.Adb.Utilities.Collections.Extensions
{
    /// <summary>
    /// Extension methods to do with ranges.
    /// </summary>
    public static class RangeBasedExt
    {
        /// <summary>
        /// Creates an inclusive range between two values. The default comparer is used
        /// to compare values.
        /// </summary>
        /// <typeparam name="T">Type of the values</typeparam>
        /// <param name="start">Start of range.</param>
        /// <param name="end">End of range.</param>
        /// <returns>An inclusive range between the start point and the end point.</returns>
        public static Range<T> To<T>(this T start, T end)
        {
            return new Range<T>(start, end);
        }

        /// <summary>
        /// Returns a RangeIterator over the given range, where the stepping function
        /// is to step by the given number of characters.
        /// </summary>
        /// <param name="range">The range to create an iterator for</param>
        /// <param name="step">How many characters to step each time</param>
        /// <returns>A RangeIterator with a suitable stepping function</returns>
        public static RangeIterator<char> StepChar(this Range<char> range, int step)
        {
            return range.Step(c => (char)(c + step));
        }
    }
}
