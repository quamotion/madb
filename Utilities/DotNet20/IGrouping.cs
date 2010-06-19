#if !DOTNET35
using System.Collections;
using System.Collections.Generic;

namespace System.Linq
{
    /// <summary>
    /// LINQ interface used to represent groupings - each grouping has a key,
    /// and represents a sequence of elements.
    /// </summary>
    public interface IGrouping<TKey, TElement> : IEnumerable<TElement>, IEnumerable
    {
        /// <summary>
        /// Key for this group
        /// </summary>
        TKey Key { get; }
    }
}
#endif