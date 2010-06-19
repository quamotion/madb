using System;

namespace Managed.Adb.Utilities.Linq
{
    /// <summary>
    /// Poor-man's version of a Future. This wraps a result which *will* be
    /// available in the future. It's up to the caller/provider to make sure
    /// that the value has been specified by the time it's requested.
    /// </summary>
    public class Future<T> : IFuture<T>
    {
        T value;
        bool valueSet = false;
        /// <summary>
        /// Returns the value of the future, once it has been set
        /// </summary>
        /// <exception cref="InvalidOperationException">If the value is not yet available</exception>
        public T Value
        {
            get
            {
                if (!valueSet)
                {
                    throw new InvalidOperationException("No value has been set yet");
                }
                return value;
            }
            set
            {
                if (valueSet)
                {
                    throw new InvalidOperationException("Value has already been set");
                }
                valueSet = true;
                this.value = value;
            }
        }
        /// <summary>
        /// Returns a string representation of the value if available, null otherwise
        /// </summary>
        /// <returns>A string representation of the value if available, null otherwise</returns>
        public override string ToString()
        {
            return valueSet ? Convert.ToString(value) : null;
        }
    }
}
