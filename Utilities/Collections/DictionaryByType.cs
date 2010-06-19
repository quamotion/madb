using System;
using System.Collections.Generic;
namespace Managed.Adb.Utilities.Collections
{
    /// <summary>
    /// Map from types to instances of those types, e.g. int to 10 and
    /// string to "hi" within the same dictionary. This cannot be done
    /// without casting (and boxing for value types) as .NET cannot
    /// represent this relationship with generics in their current form.
    /// This class encapsulates the nastiness in a single place.
    /// </summary>
    public class DictionaryByType
    {
        private readonly IDictionary<Type, object> dictionary = new Dictionary<Type, object>();

        /// <summary>
        /// Maps the specified type argument to the given value. If
        /// the type argument already has a value within the dictionary,
        /// ArgumentException is thrown.
        /// </summary>
        public void Add<T>(T value)
        {
            dictionary.Add(typeof(T), value);
        }

        /// <summary>
        /// Maps the specified type argument to the given value. If
        /// the type argument already has a value within the dictionary, it
        /// is overwritten.
        /// </summary>
        public void Put<T>(T value)
        {
            dictionary[typeof(T)] = value;
        }

        /// <summary>
        /// Attempts to fetch a value from the dictionary, throwing a
        /// KeyNotFoundException if the specified type argument has no
        /// entry in the dictionary.
        /// </summary>
        public T Get<T>()
        {
            return (T) dictionary[typeof(T)];
        }

        /// <summary>
        /// Attempts to fetch a value from the dictionary, returning false and
        /// setting the output parameter to the default value for T if it
        /// fails, or returning true and setting the output parameter to the
        /// fetched value if it succeeds.
        /// </summary>
        public bool TryGet<T>(out T value)
        {
            object tmp;
            if (dictionary.TryGetValue(typeof(T), out tmp))
            {
                value = (T) tmp;
                return true;
            }
            value = default(T);
            return false;
        }
    }
}
