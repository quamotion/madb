using System;

namespace Managed.Adb.Utilities.Linq
{
    /// <summary>
    /// Implementation of IFuture which retrieves it value from a delegate.
    /// This is primarily used for FromFuture, which will transform another
    /// Future's value on demand.
    /// </summary>
    public class FutureProxy<T> : IFuture<T>
    {
        readonly Func<T> fetcher;

        /// <summary>
        /// Creates a new FutureProxy using the given method
        /// to obtain the value when needed
        /// </summary>
        public FutureProxy(Func<T> fetcher)
        {
            this.fetcher = fetcher;
        }

        /// <summary>
        /// Creates a new FutureProxy from an existing future using
        /// the supplied transformation to obtain the value as needed
        /// </summary>
        public static FutureProxy<T> FromFuture<TSource>(IFuture<TSource> future, Func<TSource, T> projection)
        {
            return new FutureProxy<T>(() => projection(future.Value));
        }
        /// <summary>
        /// Returns the value of the Future
        /// </summary>
        public T Value
        {
            get 
            {
                return fetcher();
            }
        }
    }
}
