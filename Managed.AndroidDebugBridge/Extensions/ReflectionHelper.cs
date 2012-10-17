using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Managed.Adb.Extensions {
	public static partial class MadbExtensions {
		/// <summary>
		/// Gets the custom attributes.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="type">The type.</param>
		/// <returns></returns>
		public static IEnumerable<T> GetCustomAttributes<T> ( this Type type ) where T : Attribute {
			return (IEnumerable<T>)Attribute.GetCustomAttributes ( type, typeof ( T ) );
		}

		/// <summary>
		/// Gets the custom attributes.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="module">The module.</param>
		/// <returns></returns>
		public static IEnumerable<T> GetCustomAttributes<T> ( this Module module ) where T : Attribute {
			return (IEnumerable<T>)Attribute.GetCustomAttributes ( module, typeof ( T ) );
		}

		/// <summary>
		/// Gets the custom attributes.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="mi">The mi.</param>
		/// <returns></returns>
		public static IEnumerable<T> GetCustomAttributes<T> ( this MemberInfo mi ) where T : Attribute {
			return (IEnumerable<T>)Attribute.GetCustomAttributes ( mi, typeof ( T ) );
		}

		/// <summary>
		/// Gets the custom attributes.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="mi">The mi.</param>
		/// <returns></returns>
		public static IEnumerable<T> GetCustomAttributes<T> ( this MethodInfo mi ) where T : Attribute {
			return (IEnumerable<T>)Attribute.GetCustomAttributes ( mi, typeof ( T ) );
		}

		/// <summary>
		/// Gets the custom attributes.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="pi">The pi.</param>
		/// <returns></returns>
		public static IEnumerable<T> GetCustomAttributes<T> ( this PropertyInfo pi ) where T : Attribute {
			return (IEnumerable<T>)Attribute.GetCustomAttributes ( pi, typeof ( T ) );
		}

		/// <summary>
		/// Gets the custom attributes.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="pi">The pi.</param>
		/// <returns></returns>
		public static IEnumerable<T> GetCustomAttributes<T> ( this ParameterInfo pi ) where T : Attribute {
			return (IEnumerable<T>)Attribute.GetCustomAttributes ( pi, typeof ( T ) );
		}

		/// <summary>
		/// Gets the custom attributes.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="fi">The fi.</param>
		/// <returns></returns>
		public static IEnumerable<T> GetCustomAttributes<T> ( this FieldInfo fi ) where T : Attribute {
			return (IEnumerable<T>)Attribute.GetCustomAttributes ( fi, typeof ( T ) );
		}
		/// <summary>
		/// Gets the custom attribute.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="type">The type.</param>
		/// <returns></returns>
		public static T GetCustomAttribute<T> ( this Type type ) where T : Attribute {
			return GetCustomAttributes<T> ( type ).FirstOrDefault ( );
		}

		/// <summary>
		/// Gets the custom attribute.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="module">The type.</param>
		/// <returns></returns>
		public static T GetCustomAttribute<T> ( this Module module ) where T : Attribute {
			return GetCustomAttributes<T> ( module ).FirstOrDefault ( );
		}

		/// <summary>
		/// Gets the custom attribute.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="mi">The type.</param>
		/// <returns></returns>
		public static T GetCustomAttribute<T> ( this MemberInfo mi ) where T : Attribute {
			return GetCustomAttributes<T> ( mi ).FirstOrDefault ( );
		}

		/// <summary>
		/// Gets the custom attribute.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="mi">The type.</param>
		/// <returns></returns>
		public static T GetCustomAttribute<T> ( this MethodInfo mi ) where T : Attribute {
			return GetCustomAttributes<T> ( mi ).FirstOrDefault ( );
		}

		/// <summary>
		/// Gets the custom attribute.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="pi">The type.</param>
		/// <returns></returns>
		public static T GetCustomAttribute<T> ( this PropertyInfo pi ) where T : Attribute {
			return GetCustomAttributes<T> ( pi ).FirstOrDefault ( );
		}

		/// <summary>
		/// Gets the custom attribute.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="pi">The type.</param>
		/// <returns></returns>
		public static T GetCustomAttribute<T> ( this ParameterInfo pi ) where T : Attribute {
			return GetCustomAttributes<T> ( pi ).FirstOrDefault ( );
		}

		/// <summary>
		/// Gets the custom attribute.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="fi">The type.</param>
		/// <returns></returns>
		public static T GetCustomAttribute<T> ( this FieldInfo fi ) where T : Attribute {
			return GetCustomAttributes<T> ( fi ).FirstOrDefault ( );
		}

		/// <summary>
		/// Determines whether the specified type is nullable.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <returns>
		///   <c>true</c> if the specified type is nullable; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsNullable ( this Type type ) {
			return ( type.IsGenericType && type.GetGenericTypeDefinition ( ).Equals ( typeof ( Nullable<> ) ) );
		}

		/// <summary>
		/// Gets member info that have the specified attribute
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="members">The members.</param>
		/// <returns></returns>
		public static IEnumerable<MemberInfo> WithAttribute<T> ( this IEnumerable<MemberInfo> members ) where T : Attribute {
			return members.Where ( m => m.GetCustomAttribute<T> ( ) != default ( T ) );
		}

		/// <summary>
		/// Gets field info that have the specified attribute
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="fields">The fields.</param>
		/// <returns></returns>
		public static IEnumerable<FieldInfo> WithAttribute<T> ( this IEnumerable<FieldInfo> fields ) where T : Attribute {
			return fields.Where ( m => m.GetCustomAttribute<T> ( ) != default ( T ) );
		}

		/// <summary>
		/// Gets module info that have the specified attribute
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="modules">The modules.</param>
		/// <returns></returns>
		public static IEnumerable<Module> WithAttribute<T> ( this IEnumerable<Module> modules ) where T : Attribute {
			return modules.Where ( m => m.GetCustomAttribute<T> ( ) != default ( T ) );
		}

		/// <summary>
		/// Gets method info that have the specified attribute
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="methods">The methods.</param>
		/// <returns></returns>
		public static IEnumerable<MethodInfo> WithAttribute<T> ( this IEnumerable<MethodInfo> methods ) where T : Attribute {
			return methods.Where ( m => m.GetCustomAttribute<T> ( ) != default ( T ) );
		}

		/// <summary>
		/// Gets parameter info that have the specified attribute
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="parameters">The parameters.</param>
		/// <returns></returns>
		public static IEnumerable<ParameterInfo> WithAttribute<T> ( this IEnumerable<ParameterInfo> parameters ) where T : Attribute {
			return parameters.Where ( m => m.GetCustomAttribute<T> ( ) != default ( T ) );
		}

		/// <summary>
		/// Gets property info that have the specified attribute
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="properties">The properties.</param>
		/// <returns></returns>
		public static IEnumerable<PropertyInfo> WithAttribute<T> ( this IEnumerable<PropertyInfo> properties ) where T : Attribute {
			return properties.Where ( m => m.GetCustomAttribute<T> ( ) != default ( T ) );
		}

		/// <summary>
		/// Gets types that have the specified attribute
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="types">The types.</param>
		/// <returns></returns>
		public static IEnumerable<Type> WithAttribute<T> ( this IEnumerable<Type> types ) where T : Attribute {
			return types.Where ( m => m.GetCustomAttribute<T> ( ) != default ( T ) );
		}

		/// <summary>
		/// Gets the methods that have the specified return type.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="type">The type.</param>
		/// <returns></returns>
		public static IEnumerable<MethodInfo> GetMethodsOfReturnType<T> ( this Type type ) {
			return GetMethodsOfReturnType<T> ( type, BindingFlags.Instance | BindingFlags.Public );
		}

		/// <summary>
		/// Gets the methods that have the specified return type.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="type">The type.</param>
		/// <param name="bindingFlags">The binding flags.</param>
		/// <returns></returns>
		public static IEnumerable<MethodInfo> GetMethodsOfReturnType<T> ( this Type type, BindingFlags bindingFlags ) {
			return type.GetMethods ( bindingFlags )
				.Where ( m => m.ReturnType.IsAssignableFrom ( typeof ( T ) ) )
				.Select ( m => m );
		}
	}
}
