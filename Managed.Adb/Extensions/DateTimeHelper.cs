// <copyright file="DateTimeHelper.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion. All rights reserved.
// </copyright>

namespace Managed.Adb
{
    using System;

    /// <ignore>true</ignore>
    public static class DateTimeHelper
    {
        /// <summary>
        /// Gets EPOCH time
        /// </summary>
        public static DateTime Epoch
        { get; } = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// Gets epoch time.
        /// </summary>
        /// <param name="dt">The dt.</param>
        /// <returns></returns>
        public static DateTime GetEpoch(this DateTime dt)
        {
            return Epoch;
        }

        /// <summary>
        /// Converts a DateTime to Unix Epoch
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns></returns>
        public static long ToUnixEpoch(this DateTime date)
        {
            TimeSpan t = date.ToUniversalTime() - Epoch;
            long epoch = (long)t.TotalSeconds;
            return epoch;
        }
    }
}
