// <copyright file="AdbSocketFactory.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion. All rights reserved.
// </copyright>

namespace SharpAdbClient
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Text;

    /// <summary>
    /// Provides factory methods used by the various SharpAdbClient classes.
    /// </summary>
    public static class Factories
    {
        /// <summary>
        /// Creates a new instance of the <see cref="AdbSocket"/> class.
        /// </summary>
        /// <returns>
        /// A new instance of the <see cref="AdbSocket"/> class.
        /// </returns>
        public static Func<IPEndPoint, IAdbSocket> AdbSocketFactory
        { get; set; } = (endPoint) => new AdbSocket(endPoint);

        /// <summary>
        /// Creates a new instance of the <see cref="AdbClient"/> class.
        /// </summary>
        /// <returns>
        /// A new instance of the <see cref="AdbClient"/> class.
        /// </returns>
        public static Func<IPEndPoint, IAdbClient> AdbClientFactory
        { get; set; } = (endPoint) => new AdbClient(endPoint);
    }
}
