using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Managed.Adb
{
    /// <ignore>true</ignore>
    public static partial class ManagedAdbExtenstions
    {
        public static void ThrowIfNegative<T>(this int argument, Expression<Func<T, string>> func )
        {
            if (argument < 0 )
            {
                ThrowException(s => new ArgumentOutOfRangeException(s ), func );
            }
        }

        public static void ThrowIfNonPositive<T>(this int argument, Expression<Func<T, string>> func )
        {
            if (argument <= 0 )
            {
                ThrowException(s => new ArgumentOutOfRangeException(s ), func );
            }
        }

        public static void ThrowIfNull<T>(this T argument, Expression<Func<T, string>> func ) where T : class
        {
            if (argument == null )
            {
                ThrowException(s => new ArgumentNullException(s ), func );
            }
        }

        public static void ThrowIfNullOrEmpty(this string argument, Expression<Func<string, string>> func )
        {
            if (string.IsNullOrEmpty(argument) )
            {
                ThrowException(s => new ArgumentNullException(s ), func );
            }
        }

        public static void ThrowIfNullOrWhiteSpace(this string argument, Expression<Func<string, string>> func )
        {
            if (string.IsNullOrEmpty(argument ) || string.IsNullOrEmpty(argument.Trim()) )
            {
                ThrowException(s => new ArgumentNullException(s ), func );
            }
        }

        public static void ThrowException<T, E>(Func<string, E> e, Expression<Func<T, string>> func ) where E : Exception
        {
            string name = string.Empty;
            Expression body = func.Body;
            if (body is MemberExpression )
                name = ((MemberExpression)body ).Member.Name;
            throw e(name );
        }

        public static void ThrowIfNull<T>(this T argument, string name ) where T : class
        {
            if (argument == null )
            {
                throw new ArgumentNullException(name );
            }
        }

        public static void ThrowIfNullOrEmpty(this string argument, string name )
        {
            if (string.IsNullOrEmpty(argument) )
            {
                throw new ArgumentNullException(name );
            }
        }

        public static void ThrowIfNullOrWhiteSpace(this string argument, string name )
        {
            if (string.IsNullOrEmpty(argument ) || string.IsNullOrEmpty(argument.Trim() ) )
            {
                throw new ArgumentNullException(name );
            }
        }

        public static void ThrowIfNegative(this int argument, string name )
        {
            if (argument < 0 )
            {
                throw new ArgumentOutOfRangeException(name );
            }
        }

        public static void ThrowIfNonPositive(this int argument, string name )
        {
            if (argument <= 0 )
            {
                throw new ArgumentOutOfRangeException(name );
            }
        }
    }
}
