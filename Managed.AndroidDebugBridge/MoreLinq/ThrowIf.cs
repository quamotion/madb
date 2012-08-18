using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Managed.Adb.MoreLinq {
	public partial class MoreEnumerable {
		internal static void ThrowIfNegative<T> ( this int argument, Expression<Func<T, string>> func ) {
			if ( argument < 0 ) {
				ThrowException ( s => new ArgumentOutOfRangeException ( s ), func );
			}
		}

		internal static void ThrowIfNonPositive<T> ( this int argument, Expression<Func<T, string>> func ) {
			if ( argument <= 0 ) {
				ThrowException ( s => new ArgumentOutOfRangeException ( s ), func );
			}
		}

		public static void ThrowIfNull<T> ( this T argument, Expression<Func<T, string>> func ) where T : class {
			if ( argument == null ) {
				ThrowException ( s => new ArgumentNullException ( s ), func );
			}
		}

		private static void ThrowException<T, E> ( Func<string, E> e, Expression<Func<T, string>> func ) where E : Exception {
			string name = string.Empty;
			Expression body = func.Body;
			if ( body is MemberExpression )
				name = ( (MemberExpression)body ).Member.Name;
			throw e ( name );
		}

		internal static void ThrowIfNull<T> ( this T argument, string name ) where T : class {
			if ( argument == null ) {
				throw new ArgumentNullException ( name );
			}
		}

		internal static void ThrowIfNegative ( this int argument, string name ) {
			if ( argument < 0 ) {
				throw new ArgumentOutOfRangeException ( name );
			}
		}

		internal static void ThrowIfNonPositive ( this int argument, string name ) {
			if ( argument <= 0 ) {
				throw new ArgumentOutOfRangeException ( name );
			}
		}
	}
}
