// <copyright file="IAdbCommandLineClient.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion. All rights reserved.
// </copyright>

namespace Managed.Adb
{
    using System;

    /// <summary>
    /// Provides a common interface for any class that provides access to the
    /// <c>adb.exe</c> executable.
    /// </summary>
    public interface IAdbCommandLineClient
    {
        /// <summary>
        /// Queries adb for its version number and checks it against <see cref="AdbServer.RequiredAdbVersion"/>.
        /// </summary>
        Version GetVersion();

        /// <summary>
        /// Starts the adb server by running the <c>adb start-server</c> command.
        /// </summary>
        void StartServer();
    }
}
