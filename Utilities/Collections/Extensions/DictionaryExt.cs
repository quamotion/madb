using System;
using System.Collections.Generic;

namespace Managed.Adb.Utilities.Collections.Extensions
{
    /// <summary>
    /// Extensions to IDictionary
    /// </summary>
    public static class DictionaryExt
    {
        /// <summary>
        /// Returns the value associated with the specified key if there
        /// already is one, or inserts a new value for the specified key and
        /// returns that.
        /// </summary>
        /// <typeparam name="TKey">Type of key</typeparam>
        /// <typeparam name="TValue">Type of value, which must either have
        /// a public parameterless constructor or be a value type</typeparam>
        /// <param name="dictionary">Dictionary to access</param>
        /// <param name="key">Key to lookup</param>
        /// <returns>Existing value in the dictionary, or new one inserted</returns>
        public static TValue GetOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary,
                                                       TKey key)
            where TValue : new()
        {
            TValue ret;
            if (!dictionary.TryGetValue(key, out ret))
            {
                ret = new TValue();
                dictionary[key] = ret;
            }
            return ret;
        }

        /// <summary>
        /// Returns the value associated with the specified key if there already
        /// is one, or calls the specified delegate to create a new value which is
        /// stored and returned.
        /// </summary>
        /// <typeparam name="TKey">Type of key</typeparam>
        /// <typeparam name="TValue">Type of value</typeparam>
        /// <param name="dictionary">Dictionary to access</param>
        /// <param name="key">Key to lookup</param>
        /// <param name="valueProvider">Delegate to provide new value if required</param>
        /// <returns>Existing value in the dictionary, or new one inserted</returns>
        public static TValue GetOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary,
                                                       TKey key,
                                                       Func<TValue> valueProvider)
        {
            TValue ret;
            if (!dictionary.TryGetValue(key, out ret))
            {
                ret = valueProvider();
                dictionary[key] = ret;
            }
            return ret;
        }

        /// <summary>
        /// Returns the value associated with the specified key if there
        /// already is one, or inserts the specified value and returns it.
        /// </summary>
        /// <typeparam name="TKey">Type of key</typeparam>
        /// <typeparam name="TValue">Type of value</typeparam>
        /// <param name="dictionary">Dictionary to access</param>
        /// <param name="key">Key to lookup</param>
        /// <param name="missingValue">Value to use when key is missing</param>
        /// <returns>Existing value in the dictionary, or new one inserted</returns>
        public static TValue GetOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary,
                                                       TKey key,
                                                       TValue missingValue)
        {
            TValue ret;
            if (!dictionary.TryGetValue(key, out ret))
            {
                ret = missingValue;
                dictionary[key] = ret;
            }
            return ret;
        }
    }
}
