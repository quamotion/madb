// <copyright file="AdbServer.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion. All rights reserved.
// </copyright>

namespace SharpAdbClient
{
    using Mono.Unix;
    using SharpAdbClient.Exceptions;
    using System;
    using System.Net;
    using System.Net.Sockets;

    /// <summary>
    /// <para>
    /// Provides methods for interacting with the adb server. The adb server must be running for
    /// the rest of the <c>Managed.Adb</c> library to work.
    /// </para>
    /// <para>
    /// The adb server is a background process
    /// that runs on the host machine. Its purpose if to sense the USB ports to know when devices are
    /// attached/removed, as well as when emulator instances start/stop. The ADB server is really one
    /// giant multiplexing loop whose purpose is to orchestrate the exchange of data
    /// between clients and devices.
    /// </para>
    /// </summary>
    public static class AdbServer
    {
        /// <summary>
        /// The port at which the Android Debug Bridge server listens by default.
        /// </summary>
        public const int AdbServerPort = 5037;

        /// <summary>
        /// The minum version of <c>adb.exe</c> that is supported by this library.
        /// </summary>
        public static readonly Version RequiredAdbVersion = new Version(1, 0, 20);

        /// <summary>
        /// The error code that is returned by the <see cref="SocketException"/> when the connection is refused.
        /// </summary>
        /// <remarks>
        /// No connection could be made because the target computer actively refused it.This usually
        /// results from trying to connect to a service that is inactive on the foreign host—that is,
        ///  one with no server application running.
        /// </remarks>
        /// <seealso href="https://msdn.microsoft.com/en-us/library/ms740668.aspx"/>
        internal const int ConnectionRefused = 10061;

        /// <summary>
        /// The tag to use when logging.
        /// </summary>
        private const string Tag = nameof(AdbServer);

        static AdbServer()
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32NT:
                    EndPoint = new IPEndPoint(IPAddress.Loopback, AdbServerPort);
                    break;

                case PlatformID.MacOSX:
                case PlatformID.Unix:
                    EndPoint = new UnixEndPoint($"/tmp/{AdbServerPort}");
                    break;

                default:
                    throw new InvalidOperationException("Only Windows, Linux and Mac OS X are supported");
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="IPEndPoint"/> at which the Android Debug Bridge server is listening..
        /// </summary>
        public static EndPoint EndPoint { get; private set; }

        /// <summary>
        /// Starts the adb server if it was not previously running.
        /// </summary>
        /// <param name="adbPath">
        /// The path to the <c>adb.exe</c> executable that can be used to start the adb server.
        /// If this path is not provided, this method will throw an exception if the server
        /// is not running or is not up to date.
        /// </param>
        /// <param name="restartServerIfNewer">
        /// <see langword="true"/> to restart the adb server if the version of the <c>adb.exe</c>
        /// executable at <paramref name="adbPath"/> is newer than the version that is currently
        /// running; <see langword="false"/> to keep a previous version of the server running.
        /// </param>
        /// <returns>
        /// <list type="ordered">
        /// <item>
        ///     <see cref="StartServerResult.AlreadyRunning"/> if the adb server was already
        ///     running and the version of the adb server was at least <see cref="RequiredAdbVersion"/>.
        /// </item>
        /// <item>
        ///     <see cref="StartServerResult.RestartedOutdatedDaemon"/> if the adb server
        ///     was already running, but the version was less than <see cref="RequiredAdbVersion"/>
        ///     or less than the version of the adb client at <paramref name="adbPath"/> and the
        ///     <paramref name="restartServerIfNewer"/> flag was set.
        /// </item>
        /// <item>
        /// </item>
        ///     <see cref="StartServerResult.Started"/> if the adb server was not running,
        ///     and the server was started.
        /// </list>
        /// </returns>
        /// <exception cref="AdbException">
        /// The server was not running, or an outdated version of the server was running,
        /// and the <paramref name="adbPath"/> parameter was not specified.
        /// </exception>
        public static StartServerResult StartServer(string adbPath, bool restartServerIfNewer)
        {
            var serverStatus = GetStatus();
            Version commandLineVersion = null;

            var commandLineClient = Factories.AdbCommandLineClientFactory(adbPath);

            if (adbPath != null)
            {
                commandLineVersion = commandLineClient.GetVersion();
            }

            // If the server is running, and no adb path is provided, check if we have the minimum
            // version
            if (adbPath == null)
            {
                if (!serverStatus.IsRunning)
                {
                    throw new AdbException("The adb server is not running, but no valid path to the adb.exe executable was provided. The adb server cannot be started.");
                }

                if (serverStatus.Version >= RequiredAdbVersion)
                {
                    return StartServerResult.AlreadyRunning;
                }
                else
                {
                    throw new AdbException($"The adb deamon is running an outdated version ${commandLineVersion}, but not valid path to the adb.exe executable was provided. A more recent version of the adb server cannot be started.");
                }
            }

            if (serverStatus.IsRunning
                && ((serverStatus.Version < RequiredAdbVersion)
                     || ((serverStatus.Version < commandLineVersion) && restartServerIfNewer)))
            {
                if (adbPath == null)
                {
                    throw new ArgumentNullException(nameof(adbPath));
                }

                AdbClient.Instance.KillAdb();
                serverStatus.IsRunning = false;
                serverStatus.Version = null;

                commandLineClient.StartServer();
                return StartServerResult.RestartedOutdatedDaemon;
            }
            else if (!serverStatus.IsRunning)
            {
                if (adbPath == null)
                {
                    throw new ArgumentNullException(nameof(adbPath));
                }

                commandLineClient.StartServer();
                return StartServerResult.Started;
            }
            else
            {
                return StartServerResult.AlreadyRunning;
            }
        }

        /// <summary>
        /// Gets the status of the adb server.
        /// </summary>
        /// <returns>
        /// A <see cref="AdbServerStatus"/> object that describes the status of the
        /// adb server.
        /// </returns>
        public static AdbServerStatus GetStatus()
        {
            // Try to connect to a running instance of the adb server
            try
            {
                int versionCode = AdbClient.Instance.GetAdbVersion();

                return new AdbServerStatus()
                {
                    IsRunning = true,
                    Version = new Version(1, 0, versionCode)
                };
            }
            catch (SocketException ex)
            {
                if (ex.ErrorCode == ConnectionRefused)
                {
                    return new AdbServerStatus()
                    {
                        IsRunning = false,
                        Version = null
                    };
                }
                else
                {
                    // An unexpected exception occurred; re-throw the exception
                    throw;
                }
            }
        }
    }
}
