#if !DOTNET35
using System.Collections;
using System.Collections.Generic;

namespace System.Linq
{
    /// <summary>
    /// LINQ interface representing a lookup. This is like a dictionary, but
    /// each key maps to a sequence of values.
    /// </summary>
    public interface ILookup<TKey, TElement> : IEnumerable<IGrouping<TKey, TElement>>,
        IEnumerable
    {
        /// <summary>
        /// Returns whether or not the lookup contains the specified key
        /// </summary>
        bool Contains(TKey key);
        /// <summary>
        /// Returns the number of keys in this lookup
        /// </summary>
        int Count { get; }
        /// <summary>
        /// Returns the sequence of elements associated with the given key
        /// </summary>
        IEnumerable<TElement> this[TKey key] { get; }
    }
}
#endif