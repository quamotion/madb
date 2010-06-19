using System;

namespace Managed.Adb.Utilities.Extensions.TimeRelated
{
    /// <summary>
    /// Extension methods producing TimeSpan values. Note: Ticks should really
    /// take a long, and the rest should all take doubles. It looks like extension
    /// methods don't quite work properly with implicit numeric conversions :(
    /// </summary>
    public static class TimeSpanBasedExt
    {
        /// <summary>
        /// Returns a TimeSpan representing the specified number of ticks.
        /// </summary>
        public static TimeSpan Ticks(this int ticks)
        {
            return TimeSpan.FromTicks(ticks);
        }

        /// <summary>
        /// Returns a TimeSpan representing the specified number of milliseconds.
        /// </summary>
        public static TimeSpan Milliseconds(this int milliseconds)
        {
            return TimeSpan.FromMilliseconds(milliseconds);
        }

        /// <summary>
        /// Returns a TimeSpan representing the specified number of seconds.
        /// </summary>
        public static TimeSpan Seconds(this int seconds)
        {
            return TimeSpan.FromSeconds(seconds);
        }

        /// <summary>
        /// Returns a TimeSpan representing the specified number of minutes.
        /// </summary>
        public static TimeSpan Minutes(this int minutes)
        {
            return TimeSpan.FromMinutes(minutes);
        }

        /// <summary>
        /// Returns a TimeSpan representing the specified number of hours.
        /// </summary>
        public static TimeSpan Hours(this int hours)
        {
            return TimeSpan.FromHours(hours);
        }

        /// <summary>
        /// Returns a TimeSpan representing the specified number of days.
        /// </summary>
        public static TimeSpan Days(this int days)
        {
            return TimeSpan.FromDays(days);
        }
    }
}
