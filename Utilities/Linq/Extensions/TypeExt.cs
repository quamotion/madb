#if DOTNET35
using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Managed.Adb.Utilities.Extensions;

namespace Managed.Adb.Utilities.Linq.Extensions
{
    /// <summary>
    /// Provides extension methods to System.Type to provide simple
    /// and efficient access to delegates representing reflection
    /// operations.
    /// </summary>
    public static class TypeExt
    {
        #region Ctor

        private static ConstructorInfo GetConstructor(Type type, params Type[] argumentTypes)
        {
            type.ThrowIfNull("type");
            argumentTypes.ThrowIfNull("argumentTypes");
            
            ConstructorInfo ci = type.GetConstructor(argumentTypes);
            if (ci == null)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(type.Name).Append(" has no ctor(");
                for (int i = 0; i < argumentTypes.Length; i++)
                {
                    if(i > 0) {
                        sb.Append(',');
                    }
                    sb.Append(argumentTypes[i].Name);
                }
                sb.Append(')');
                throw new InvalidOperationException(sb.ToString());
            }
            return ci;
        }
        /// <summary>
        /// Obtains a delegate to invoke a parameterless constructor
        /// </summary>
        /// <typeparam name="TResult">The base/interface type to yield as the
        /// new value; often object except for factory pattern implementations</typeparam>
        /// <param name="type">The Type to be created</param>
        /// <returns>A delegate to the constructor if found, else null</returns>
        public static Func<TResult> Ctor<TResult>(this Type type)
        {
            ConstructorInfo ci = GetConstructor(type, Type.EmptyTypes);
            return Expression.Lambda<Func<TResult>>(
                Expression.New(ci)).Compile();
        }
        /// <summary>
        /// Obtains a delegate to invoke a constructor which takes a parameter
        /// </summary>
        /// <typeparam name="TArg1">The type of the constructor parameter</typeparam>
        /// <typeparam name="TResult">The base/interface type to yield as the
        /// new value; often object except for factory pattern implementations</typeparam>
        /// <param name="type">The Type to be created</param>
        /// <returns>A delegate to the constructor if found, else null</returns>
        public static Func<TArg1, TResult>
            Ctor<TArg1, TResult>(this Type type)
        {
            ConstructorInfo ci = GetConstructor(type, typeof(TArg1));
            ParameterExpression
                param1 = Expression.Parameter(typeof(TArg1), "arg1");

            return Expression.Lambda<Func<TArg1, TResult>>(
                Expression.New(ci, param1), param1).Compile();
        }
        /// <summary>
        /// Obtains a delegate to invoke a constructor with multiple parameters
        /// </summary>
        /// <typeparam name="TArg1">The type of the first constructor parameter</typeparam>
        /// <typeparam name="TArg2">The type of the second constructor parameter</typeparam>
        /// <typeparam name="TResult">The base/interface type to yield as the
        /// new value; often object except for factory pattern implementations</typeparam>
        /// <param name="type">The Type to be created</param>
        /// <returns>A delegate to the constructor if found, else null</returns>
        public static Func<TArg1, TArg2, TResult>
            Ctor<TArg1, TArg2, TResult>(this Type type)
        {
            ConstructorInfo ci = GetConstructor(type, typeof(TArg1), typeof(TArg2));
            ParameterExpression
                param1 = Expression.Parameter(typeof(TArg1), "arg1"),
                param2 = Expression.Parameter(typeof(TArg2), "arg2");

            return Expression.Lambda<Func<TArg1, TArg2, TResult>>(
                Expression.New(ci, param1, param2), param1, param2).Compile();
        }
        /// <summary>
        /// Obtains a delegate to invoke a constructor with multiple parameters
        /// </summary>
        /// <typeparam name="TArg1">The type of the first constructor parameter</typeparam>
        /// <typeparam name="TArg2">The type of the second constructor parameter</typeparam>
        /// <typeparam name="TArg3">The type of the third constructor parameter</typeparam>
        /// <typeparam name="TResult">The base/interface type to yield as the
        /// new value; often object except for factory pattern implementations</typeparam>
        /// <param name="type">The Type to be created</param>
        /// <returns>A delegate to the constructor if found, else null</returns>
        public static Func<TArg1, TArg2, TArg3, TResult>
            Ctor<TArg1, TArg2, TArg3, TResult>(this Type type)
        {
            ConstructorInfo ci = GetConstructor(type, typeof(TArg1), typeof(TArg2), typeof(TArg3));
            ParameterExpression
                param1 = Expression.Parameter(typeof(TArg1), "arg1"),
                param2 = Expression.Parameter(typeof(TArg2), "arg2"),
                param3 = Expression.Parameter(typeof(TArg3), "arg3");

            return Expression.Lambda<Func<TArg1, TArg2, TArg3, TResult>>(
                Expression.New(ci, param1, param2, param3),
                    param1, param2, param3).Compile();
        }
        /// <summary>
        /// Obtains a delegate to invoke a constructor with multiple parameters
        /// </summary>
        /// <typeparam name="TArg1">The type of the first constructor parameter</typeparam>
        /// <typeparam name="TArg2">The type of the second constructor parameter</typeparam>
        /// <typeparam name="TArg3">The type of the third constructor parameter</typeparam>
        /// <typeparam name="TArg4">The type of the fourth constructor parameter</typeparam>
        /// <typeparam name="TResult">The base/interface type to yield as the
        /// new value; often object except for factory pattern implementations</typeparam>
        /// <param name="type">The Type to be created</param>
        /// <returns>A delegate to the constructor if found, else null</returns>
        public static Func<TArg1, TArg2, TArg3, TArg4, TResult>
            Ctor<TArg1, TArg2, TArg3, TArg4, TResult>(this Type type)
        {
            ConstructorInfo ci = GetConstructor(type, typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4));
            ParameterExpression
                param1 = Expression.Parameter(typeof(TArg1), "arg1"),
                param2 = Expression.Parameter(typeof(TArg2), "arg2"),
                param3 = Expression.Parameter(typeof(TArg3), "arg3"),
                param4 = Expression.Parameter(typeof(TArg4), "arg4");

            return Expression.Lambda<Func<TArg1, TArg2, TArg3, TArg4, TResult>>(
                Expression.New(ci, param1, param2, param3, param4),
                    param1, param2, param3, param4).Compile();
        }
        #endregion

    }
}
#endif