// <copyright file="ThrowIf.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion. All rights reserved.
// </copyright>

namespace Managed.Adb
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Text;

    /// <ignore>true</ignore>
    public static partial class ManagedAdbExtenstions
    {
        public static void ThrowIfNull<T>(this T argument, string name) where T : class
        {
            if (argument == null)
            {
                throw new ArgumentNullException(name);
            }
        }

        public static void ThrowIfNullOrEmpty(this string argument, string name)
        {
            if (string.IsNullOrEmpty(argument))
            {
                throw new ArgumentNullException(name);
            }
        }

        public static void ThrowIfNullOrWhiteSpace(this string argument, string name)
        {
            if (string.IsNullOrEmpty(argument) || string.IsNullOrEmpty(argument.Trim()))
            {
                throw new ArgumentNullException(name);
            }
        }
    }
}
