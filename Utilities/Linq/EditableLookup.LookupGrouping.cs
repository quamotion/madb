using System.Collections.Generic;
using System.Linq;

namespace Managed.Adb.Utilities.Linq
{
    partial class EditableLookup<TKey, TElement>
	{
        internal sealed class LookupGrouping : IGrouping<TKey, TElement>
        {
            private readonly TKey key;
            private List<TElement> items = new List<TElement>();
            public TKey Key { get { return key; } }
            public LookupGrouping(TKey key)
            {
                this.key = key;
            }
            public int Count
            {
                get { return items.Count; }
            }
            public void Add(TElement item)
            {
                items.Add(item);
            }
            public bool Contains(TElement item)
            {
                return items.Contains(item);
            }
            public bool Remove(TElement item)
            {
                return items.Remove(item);
            }
            public void TrimExcess()
            {
                items.TrimExcess();
            }

            public IEnumerator<TElement> GetEnumerator()
            {
                return items.GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
	}
}
