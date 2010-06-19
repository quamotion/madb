using System;
using System.Collections.Generic;
using Managed.Adb.Utilities.Collections;
using Managed.Adb.Utilities.Collections.Extensions;
namespace Managed.Adb.Utilities.Linq.Extensions
{
    /// <summary>
    /// Provides extension methods to List&lt;T&gt;
    /// </summary>
    public static class ListExt
    {
        /// <summary>
        /// Sorts the elements in the entire System.Collections.Generic.List{T} using
        /// a projection.
        /// </summary>
        /// <param name="source">Data source</param>
        /// <param name="selector">The projection to use to obtain values for comparison</param>
        /// <param name="comparer">The comparer to use to compare projected values (on null to use the default comparer)</param>
        /// <param name="descending">Should the list be sorted ascending or descending?</param>
        public static void Sort<T, TValue>(this List<T> source, Func<T, TValue> selector, IComparer<TValue> comparer, bool descending)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (comparer == null) comparer = Comparer<TValue>.Default;
            IComparer<T> itemComparer = new ProjectionComparer<T, TValue>(selector, comparer);
            if(descending) itemComparer = itemComparer.Reverse();
            source.Sort(itemComparer);
        }

        /// <summary>
        /// Sorts the elements in the entire System.Collections.Generic.List{T} using
        /// a projection.
        /// </summary>
        /// <param name="source">Data source</param>
        /// <param name="selector">The projection to use to obtain values for comparison</param>
        public static void Sort<T, TValue>(this List<T> source, Func<T, TValue> selector)
        {
            Sort<T, TValue>(source, selector, null, false);
        }
    }
}
