using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Managed.Adb.Reflection {
	/// <summary>
	/// Reflection helper class
	/// </summary>
	internal static class ReflectionHelper {

		/// <summary>
		/// Gets the custom attribute.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="type">The type.</param>
		/// <returns></returns>
		public static T GetCustomAttribute<T> ( Type type ) where T : Attribute {
			object[] attr = type.GetCustomAttributes ( typeof ( T ), true ) as Attribute[];
			if ( attr != null && attr.Length > 0 ) {
				return (T)attr[ 0 ];
			} else
				return default ( T );
		}

		/// <summary>
		/// Gets the custom attribute.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="mi">The mi.</param>
		/// <returns></returns>
		public static T GetCustomAttribute<T> ( MemberInfo mi ) where T : Attribute {
			Attribute attrib = Attribute.GetCustomAttribute ( mi, typeof ( T ) );
			if ( attrib != null ) {
				return (T)attrib;
			}
			object[] attr = mi.GetCustomAttributes ( typeof ( T ), true ) as Attribute[];
			if ( attr != null && attr.Length > 0 ) {
				return (T)attr[ 0 ];
			} else
				return default ( T );
		}


	}
}
