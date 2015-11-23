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

        /// <summary>
        /// Gets or sets a function that returns a new instance of a class that implements the
        /// <see cref="IAdbCommandLineClient"/> interface, that can be used to interact with the
        /// <c>adb.exe</c> command line client.
        /// </summary>
        public static Func<string, IAdbCommandLineClient> AdbCommandLineClientFactory
        { get; set; } = (path) => new AdbCommandLineClient(path);

        /// <summary>
        /// Gets or sets a function that returns a new instance of a class that implements the
        /// <see cref="ISyncService"/> interface, that can be used to transfer files to and from
        /// a given device.
        /// </summary>
        public static Func<DeviceData, ISyncService> SyncServiceFactory
        { get; set; } = (device) => new SyncService(device);
    }
}
