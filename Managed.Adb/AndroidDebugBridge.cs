// <copyright file="AndroidDebugBridge.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion. All rights reserved.
// </copyright>

namespace Managed.Adb
{
    using Exceptions;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Text.RegularExpressions;

    /// <summary>
    /// The android debug bridge
    /// </summary>
    public sealed class AndroidDebugBridge
    {
        /// <summary>
        /// Occurs when the status of the Android Debug Bridge has changed.
        /// </summary>
        /// <ignore>true</ignore>
        public event EventHandler<AndroidDebugBridgeEventArgs> BridgeChanged;

        /// <summary>
        /// Occurs when the status of one of the connected devices has changed.
        /// </summary>
        public event EventHandler<DeviceDataEventArgs> DeviceChanged;

        /// <summary>
        /// Occurs when a device has connected to the Android Debug Bridge.
        /// </summary>
        public event EventHandler<DeviceDataEventArgs> DeviceConnected;

        /// <summary>
        /// Occurs when a device has disconnected from the Android Debug Bridge.
        /// </summary>
        public event EventHandler<DeviceDataEventArgs> DeviceDisconnected;

        /// <summary>
        /// The minum version of adb that is supported.
        /// </summary>
        private static readonly Version RequiredAdbVersion = new Version(1, 0, 20);

        /// <summary>
        /// The regex pattern for getting the adb version
        /// </summary>
        private const string AdbVersionPattern = "^.*(\\d+)\\.(\\d+)\\.(\\d+)$";

        /// <summary>
        /// The tag to use when logging.
        /// </summary>
        public const string Tag = nameof(AndroidDebugBridge);

        /// <summary>
        /// The default ADB bridge port
        /// </summary>
        public const int AdbPort = 5037;

        /// <summary>
        ///
        /// </summary>
        private static AndroidDebugBridge instance;

        /// <summary>
        /// Gets or sets the socket address.
        /// </summary>
        /// <value>The socket address.</value>
        public static IPEndPoint SocketAddress { get; private set; }

        /// <summary>
        /// Initializes static members of the <see cref="AndroidDebugBridge"/> class.
        /// </summary>
        static AndroidDebugBridge()
        {
            // built-in local address/port for ADB.
            SocketAddress = new IPEndPoint(IPAddress.Loopback, AdbPort);
        }

        /// <summary>
        /// Terminates the ddm library. This must be called upon application termination.
        /// </summary>
        public static void Terminate()
        {
            // kill the monitoring services
            if (Instance != null && Instance.DeviceMonitor != null)
            {
                Instance.DeviceMonitor.Stop();
                Instance.DeviceMonitor = null;
            }
        }

        /// <summary>
        /// Gets an instance of <see cref="AndroidDebugBridge"/>.
        /// </summary>
        /// <value>The instance.</value>
        public static AndroidDebugBridge Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = CreateBridge();
                }

                return instance;
            }
        }

        /// <summary>
        /// Creates a {@link AndroidDebugBridge} that is not linked to any particular executable.
        /// This bridge will expect adb to be running. It will not be able to start/stop/restart</summary>
        /// adb.
        /// If a bridge has already been started, it is directly returned with no changes
        /// <returns></returns>
        public static AndroidDebugBridge CreateBridge()
        {
            if (instance != null)
            {
                return instance;
            }

            try
            {
                instance = new AndroidDebugBridge();
                instance.Start();
                instance.OnBridgeChanged(new AndroidDebugBridgeEventArgs(instance));
            }
            catch (ArgumentException)
            {
                instance.OnBridgeChanged(new AndroidDebugBridgeEventArgs(null));
                instance = null;
            }

            return instance;
        }

        /// <summary>
        /// Creates a new debug bridge from the location of the command line tool.
        /// </summary>
        /// <param name="osLocation">the location of the command line tool 'adb'</param>
        /// <param name="forceNewBridge">force creation of a new bridge even if one with the same location
        /// already exists.</param>
        /// <returns>a connected bridge.</returns>
        /// <remarks>Any existing server will be disconnected, unless the location is the same and
        /// <code>forceNewBridge</code> is set to false.
        /// </remarks>
        public static AndroidDebugBridge CreateBridge(string osLocation, bool forceNewBridge)
        {
            if (instance != null)
            {
                if (!string.IsNullOrEmpty(AdbOsLocation) && string.Compare(AdbOsLocation, osLocation, true) == 0 && !forceNewBridge)
                {
                    return instance;
                }
                else
                {
                    // stop the current server
                    Log.i(Tag, "Stopping Current Instance");
                    instance.Stop();
                }
            }

            try
            {
                instance = new AndroidDebugBridge(osLocation);
                instance.Start();
                instance.OnBridgeChanged(new AndroidDebugBridgeEventArgs(instance));
            }
            catch (ArgumentException)
            {
                instance.OnBridgeChanged(new AndroidDebugBridgeEventArgs(null));
                instance = null;
            }

            return instance;
        }

        /// <summary>
        /// Disconnects the current debug bridge, and destroy the object.
        /// </summary>
        /// <remarks>This also stops the current adb host server.</remarks>
        public static void DisconnectBridge()
        {
            if (instance != null)
            {
                instance.Stop();

                instance.OnBridgeChanged(new AndroidDebugBridgeEventArgs(null));
                instance = null;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AndroidDebugBridge"/> class.
        /// </summary>
        /// <param name="osLocation">the location of the command line tool</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="FileNotFoundException"></exception>
        private AndroidDebugBridge(string osLocation)
        {
            if (string.IsNullOrEmpty(osLocation))
            {
                throw new ArgumentException();
            }

            if (!File.Exists(osLocation))
            {
                Log.e(Tag, string.Format("unable to locate adb in the specified location: {0}", osLocation));
                throw new FileNotFoundException("unable to locate adb in the specified location");
            }

            AdbOsLocation = osLocation;

            this.CheckAdbVersion();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AndroidDebugBridge"/> class.
        /// </summary>
        private AndroidDebugBridge()
        {
        }

        /// <summary>
        /// Raises the <see cref="BridgeChanged"/> event.
        /// </summary>
        /// <param name="e">The <see cref="AndroidDebugBridgeEventArgs"/> instance containing the event data.</param>
        internal void OnBridgeChanged(AndroidDebugBridgeEventArgs e)
        {
            if (this.BridgeChanged != null)
            {
                this.BridgeChanged(this, e);
            }
        }

        /// <summary>
        /// Raises the <see cref="DeviceChanged"/> event.
        /// </summary>
        /// <param name="e">The <see cref="DeviceDataEventArgs"/> instance containing the event data.</param>
        internal void OnDeviceChanged(DeviceDataEventArgs e)
        {
            if (this.DeviceChanged != null)
            {
                this.DeviceChanged(this, e);
            }
        }

        /// <summary>
        /// Raises the <see cref="DeviceConnected"/> event.
        /// </summary>
        /// <param name="e">The <see cref="DeviceDataEventArgs"/> instance containing the event data.</param>
        internal void OnDeviceConnected(DeviceDataEventArgs e)
        {
            if (this.DeviceConnected != null)
            {
                this.DeviceConnected(this, e);
            }
        }

        /// <summary>
        /// Raises the <see cref="DeviceDisconnected"/> event.
        /// </summary>
        /// <param name="e">The <see cref="DeviceDataEventArgs"/> instance containing the event data.</param>
        internal void OnDeviceDisconnected(DeviceDataEventArgs e)
        {
            if (this.DeviceDisconnected != null)
            {
                this.DeviceDisconnected(this, e);
            }
        }

        /// <summary>
        /// Starts the debug bridge.
        /// </summary>
        /// <returns><see langword="true"/> if success.</returns>
        public void Start()
        {
            if (string.IsNullOrEmpty(AdbOsLocation) || !this.VersionCheck)
            {
                throw new InvalidOperationException();
            }

            this.StartAdb();

            this.Started = true;

            // now that the bridge is connected, we start the underlying services.
            this.DeviceMonitor = new DeviceMonitor(AdbHelper.SocketFactory.Create(AndroidDebugBridge.SocketAddress));
            this.DeviceMonitor.Start();
        }

        /// <summary>
        /// Kills the debug bridge, and the adb host server.
        /// </summary>
        /// <returns><see langword="true"/> if success.</returns>
        public void Stop()
        {
            // if we haven't started we return false;
            if (!this.Started)
            {
                return;
            }

            // kill the monitoring services
            if (this.DeviceMonitor != null)
            {
                this.DeviceMonitor.Stop();
                this.DeviceMonitor = null;
            }

            this.StopAdb();

            this.Started = false;
        }

        /// <summary>
        /// Restarts adb, but not the services around it.
        /// </summary>
        /// <returns><see langword="true"/> if success.</returns>
        public void Restart()
        {
            if (string.IsNullOrEmpty(AdbOsLocation))
            {
                Log.e(Tag, "Cannot restart adb when AndroidDebugBridge is created without the location of adb.");
                throw new InvalidOperationException("Cannot restart adb when AndroidDebugBridge is created without the location of adb.");
            }

            if (!this.VersionCheck)
            {
                Log.LogAndDisplay(LogLevel.Error, Tag, "Attempting to restart adb, but version check failed!");
                throw new InvalidOperationException("Attempting to restart adb, but version check failed!");
            }

            lock (this)
            {
                this.StopAdb();

                this.StartAdb();

                if (this.DeviceMonitor == null)
                {
                    this.DeviceMonitor = new DeviceMonitor(AdbHelper.SocketFactory.Create(AndroidDebugBridge.SocketAddress));
                    this.DeviceMonitor.Start();
                }

            }
        }

        /// <summary>
        /// Gets or Sets the adb location on the OS.
        /// </summary>
        /// <value>The adb location on the OS.</value>
        public static string AdbOsLocation { get; set; }

        /// <summary>
        /// Gets the devices.
        /// </summary>
        /// <value>The devices.</value>
        public IList<DeviceData> Devices
        {
            get
            {
                return AdbHelper.Instance.GetDevices(AndroidDebugBridge.SocketAddress);
            }
        }

        /// <summary>
        /// Gets the device monitor
        /// </summary>
        public DeviceMonitor DeviceMonitor { get; private set; }

        /// <summary>
        /// Gets if the adb host has started
        /// </summary>
        private bool Started { get; set; }

        /// <summary>
        /// Gets the result of the version check
        /// </summary>
        private bool VersionCheck { get; set; }

        /// <summary>
        /// Queries adb for its version number and checks it against #MIN_VERSION_NUMBER and MAX_VERSION_NUMBER
        /// </summary>
        private void CheckAdbVersion()
        {
            // default is bad check
            this.VersionCheck = false;

            if (string.IsNullOrEmpty(AdbOsLocation))
            {
                Log.w(Tag, "AdbOsLocation is Empty");
                return;
            }

            Log.d(Tag, string.Format("Checking '{0} version'", AdbOsLocation));

            List<string> errorOutput = new List<string>();
            List<string> stdOutput = new List<string>();
            this.RunAdbProcess("version", errorOutput, stdOutput);
            var version = GetAdbVersion(errorOutput, stdOutput);

            // check only the micro version for now.
            if (version < RequiredAdbVersion)
            {
                string message = $"Required minimum version of adb: {RequiredAdbVersion}. Current version is {version}";
                Log.LogAndDisplay(LogLevel.Error, Tag, message);
                throw new AdbException(message);
            }

            this.VersionCheck = true;
        }

        static internal Version GetAdbVersion(List<string> standardOutput, List<string> standardError)
        {
            var outputVersion = GetAdbVersion(standardOutput);

            if (outputVersion != null)
            {
                return outputVersion;
            }

            var errorVersion = GetAdbVersion(standardError);

            if (errorVersion != null)
            {
                return errorVersion;
            }

            throw new AdbException("Failed to retrieve the ADB version number");
        }

        static internal Version GetAdbVersion(List<string> output)
        {
            foreach (var line in output)
            {
                // Skip empty lines
                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }

                Match matcher = Regex.Match(line, AdbVersionPattern);
                if (matcher.Success)
                {
                    int majorVersion = int.Parse(matcher.Groups[1].Value);
                    int minorVersion = int.Parse(matcher.Groups[2].Value);
                    int microVersion = int.Parse(matcher.Groups[3].Value);

                    return new Version(majorVersion, minorVersion, microVersion);
                }
            }

            return null;
        }

        /// <summary>
        /// Starts the adb host side server.
        /// </summary>
        /// <returns>true if success</returns>
        private void StartAdb()
        {
            if (string.IsNullOrEmpty(AdbOsLocation))
            {
                Log.e(Tag, "Cannot start adb when AndroidDebugBridge is created without the location of adb.");
                throw new InvalidOperationException("Cannot start adb when AndroidDebugBridge is created without the location of adb.");
            }

            this.RunAdbProcess("start-server", null, null);
        }

        /// <summary>
        /// Stops the adb host side server.
        /// </summary>
        /// <returns>true if success</returns>
        private void StopAdb()
        {
            if (string.IsNullOrEmpty(AdbOsLocation))
            {
                throw new InvalidOperationException("Cannot stop adb when AndroidDebugBridge is created without the location of adb.");
            }

            this.RunAdbProcess("kill-server", null, null);
        }

        /// <summary>
        /// Get the stderr/stdout outputs of a process and return when the process is done.
        /// Both <b>must</b> be read or the process will block on windows.
        /// </summary>
        /// <param name="errorOutput">The array to store the stderr output. cannot be null.</param>
        /// <param name="stdOutput">The array to store the stdout output. cannot be null.</param>
        /// <returns>the process return code.</returns>
        private void RunAdbProcess(string command, List<string> errorOutput, List<string> stdOutput)
        {
            if (errorOutput == null)
            {
                throw new ArgumentNullException(nameof(errorOutput));
            }

            if (stdOutput == null)
            {
                throw new ArgumentNullException(nameof(stdOutput));
            }

            int status;

            ProcessStartInfo psi = new ProcessStartInfo(AdbOsLocation, command);
            psi.CreateNoWindow = true;
            psi.WindowStyle = ProcessWindowStyle.Hidden;
            psi.UseShellExecute = false;
            psi.RedirectStandardError = true;
            psi.RedirectStandardOutput = true;

            using (Process process = Process.Start(psi))
            {
                var standardErrorString = process.StandardError.ReadToEnd();
                var standardOutputString = process.StandardOutput.ReadToEnd();

                if (errorOutput != null)
                {
                    errorOutput.AddRange(standardErrorString.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries));
                }

                if (stdOutput != null)
                {
                    stdOutput.AddRange(standardOutputString.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries));
                }

                // get the return code from the process
                if (!process.WaitForExit(5000))
                {
                    process.Kill();
                }

                status = process.ExitCode;
            }

            if (status != 0)
            {
                throw new AdbException($"The adb process returned error code {status} when running command {command}");
            }
        }
    }
}
