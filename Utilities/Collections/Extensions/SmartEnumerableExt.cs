using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Managed.Adb.Utilities.Collections.Extensions
{
    /// <summary>
    /// Wrapper methods for SmartEnumerable[T].
    /// </summary>
    public static class SmartEnumerableExt
    {
        /// <summary>
        /// Extension method to make life easier.
        /// </summary>
        /// <typeparam name="T">Type of enumerable</typeparam>
        /// <param name="source">Source enumerable</param>
        /// <returns>A new SmartEnumerable of the appropriate type</returns>
        public static SmartEnumerable<T> AsSmartEnumerable<T>(this IEnumerable<T> source)
        {
            return new SmartEnumerable<T>(source);
        }
    }

}
