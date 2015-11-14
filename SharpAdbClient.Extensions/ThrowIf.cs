// <copyright file="ThrowIf.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion. All rights reserved.
// </copyright>

namespace SharpAdbClient
{
    using System;

    public static partial class ManagedAdbExtenstions
    {
        /// <summary>
        /// Throws an <see cref="ArgumentNullException"/> if <paramref name="argument"/> is
        /// <see langword="null"/>.
        /// </summary>
        /// <typeparam name="T">
        /// The argument type.
        /// </typeparam>
        /// <param name="argument">
        /// The argument value.
        /// </param>
        /// <param name="name">
        /// The name of the argument.
        /// </param>
        public static void ThrowIfNull<T>(this T argument, string name)
            where T : class
        {
            if (argument == null)
            {
                throw new ArgumentNullException(name);
            }
        }

        /// <summary>
        /// Throws an <see cref="ArgumentNullException"/> if a <see cref="string"/> is <see langword="null"/>
        /// or empty.
        /// </summary>
        /// <param name="argument">
        /// The string value.
        /// </param>
        /// <param name="name">
        /// The name of the parameter.
        /// </param>
        public static void ThrowIfNullOrEmpty(this string argument, string name)
        {
            if (string.IsNullOrEmpty(argument))
            {
                throw new ArgumentNullException(name);
            }
        }

        /// <summary>
        /// Throws a <see cref="ArgumentNullException"/> if <paramref name="argument"/>
        /// is <see langword="null"/>, empty or only contains whitespace.
        /// </summary>
        /// <param name="argument">
        /// The <see cref="string"/> to inspect.
        /// </param>
        /// <param name="name">
        /// The name of the parameter.
        /// </param>
        public static void ThrowIfNullOrWhiteSpace(this string argument, string name)
        {
            if (string.IsNullOrEmpty(argument) || string.IsNullOrEmpty(argument.Trim()))
            {
                throw new ArgumentNullException(name);
            }
        }
    }
}
