// <copyright file="Device.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion. All rights reserved.
// </copyright>

namespace SharpAdbClient
{
    using DeviceCommands;
    using SharpAdbClient.Exceptions;
    using SharpAdbClient.Logs;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Text.RegularExpressions;
    using System.Threading;
    /// <summary>
    /// Represents an Android device.
    /// </summary>
    public sealed class Device : IDevice
    {
        /// <summary>
        /// Occurs when [state changed].
        /// </summary>
        public event EventHandler<EventArgs> StateChanged;

        /// <summary>
        /// Occurs when [build info changed].
        /// </summary>
        public event EventHandler<EventArgs> BuildInfoChanged;

        /// <summary>
        /// Occurs when [client list changed].
        /// </summary>
        public event EventHandler<EventArgs> ClientListChanged;

        /// <summary>
        /// The name of the device property that holds the Android build version.
        /// </summary>
        public const string PROP_BUILD_VERSION = "ro.build.version.release";

        /// <summary>
        /// The name of the device property that holds the Android API level.
        /// </summary>
        public const string PROP_BUILD_API_LEVEL = "ro.build.version.sdk";

        /// <summary>
        /// The name of the device property that holds the code name for the Android API level.
        /// </summary>
        public const string PROP_BUILD_CODENAME = "ro.build.version.codename";

        /// <summary>
        /// The name of the device property that indicates whether the device is debuggable.
        /// </summary>
        public const string PROP_DEBUGGABLE = "ro.debuggable";

        /// <summary>
        /// Serial number of the first connected emulator.
        /// </summary>
        public const string FIRST_EMULATOR_SN = "emulator-5554";

        /// <summary>
        /// The name of the device property that indicates the Android API level.
        /// </summary>
        [Obsolete("Use PROP_BUILD_API_LEVEL")]
        public const string PROP_BUILD_VERSION_NUMBER = PROP_BUILD_API_LEVEL;

        /// <summary>
        ///  Emulator Serial Number regexp.
        /// </summary>
        private const string RE_EMULATOR_SN = @"emulator-(\d+)";

        /// <summary>
        /// The tag to use when logging messages.
        /// </summary>
        private const string LOG_TAG = nameof(Device);

        /// <summary>
        /// The time-out when receiving battery information. The default is two seconds.
        /// </summary>
        private const int BATTERY_TIMEOUT = 2 * 1000;

        /// <summary>
        /// The time-out when receiving device properties. The default is two seconds.
        /// </summary>
        private const int GETPROP_TIMEOUT = 2 * 1000;

        /// <summary>
        /// The time-out when installing applications. The default is two seconds.
        /// </summary>
        private const int INSTALL_TIMEOUT = 2 * 60 * 1000;

        /// <summary>
        /// The name of the Android Virtual Device (emulator).
        /// </summary>
        private string avdName;

        /// <summary>
        /// Indicates whether the user can obtain su (root) privileges.
        /// </summary>
        private bool canSU = false;

        /// <summary>
        /// The latest battery information.
        /// </summary>
        private BatteryInfo lastBatteryInfo = null;

        /// <summary>
        /// The time at which the battery information was last obtained.
        /// </summary>
        private DateTime lastBatteryCheckTime = DateTime.MinValue;

        public Device(DeviceData data)
            : this(data.Serial, data.State, data.Model, data.Product, data.Name)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Device"/> class.
        /// </summary>
        /// <param name="serial">The serial.</param>
        /// <param name="state">The state.</param>
        /// <param name="model">The model.</param>
        /// <param name="product">The product.</param>
        /// <param name="device">The device.</param>
        public Device(string serial, DeviceState state, string model, string product, string device)
        {
            this.SerialNumber = serial;
            this.State = state;
            this.MountPoints = new Dictionary<string, MountPoint>();
            this.Properties = new Dictionary<string, string>();
            this.EnvironmentVariables = new Dictionary<string, string>();

            this.Model = model;
            this.Product = product;
            this.DeviceProperty = device;

            this.RetrieveDeviceInfo();
        }

        /// <summary>
        /// Retrieves the device info.
        /// </summary>
        private void RetrieveDeviceInfo()
        {
            this.RefreshMountPoints();
            this.RefreshEnvironmentVariables();
            this.RefreshProperties();
        }

        /// <summary>
        /// Determines whether this instance can use the SU shell.
        /// </summary>
        /// <returns>
        ///   <see langword="true"/> if this instance can use the SU shell; otherwise, <see langword="false"/>.
        /// </returns>
        public bool CanSU()
        {
            if (this.canSU)
            {
                return this.canSU;
            }

            try
            {
                // workitem: 16822
                // this now checks if permission was denied and accounts for that.
                // The nulloutput receiver is fine here because it doesn't need to send the output anywhere,
                // the execute command can still handle the output with the null output receiver.
                this.ExecuteRootShellCommand("echo \\\"I can haz root\\\"", null);
                this.canSU = true;
            }
            catch (PermissionDeniedException)
            {
                this.canSU = false;
            }
            catch (FileNotFoundException)
            {
                this.canSU = false;
            }

            return this.canSU;
        }

        /// <summary>
        /// Gets or sets the client monitoring socket.
        /// </summary>
        /// <value>
        /// The client monitoring socket.
        /// </value>
        public Socket ClientMonitoringSocket { get; set; }

        /// <summary>
        /// Gets the device serial number
        /// </summary>
        public string SerialNumber { get; private set; }

        /// <summary>
        /// Gets the TCP endpoint defined when the transport is TCP.
        /// </summary>
        /// <value>
        /// The endpoint.
        /// </value>
        public IPEndPoint Endpoint { get; private set; }

        /// <summary>
        /// Indicates how the device is connected to the Android Debug Bridge.
        /// </summary>
        public TransportType TransportType { get; private set; }

        /// <summary>
        /// Gets or sets the Avd name.
        /// </summary>
        public string AvdName
        {
            get
            {
                return this.avdName;
            }

            set
            {
                if (!this.IsEmulator)
                {
                    throw new ArgumentException("Cannot set the AVD name of the device is not an emulator");
                }

                this.avdName = value;
            }
        }

        /// <summary>
        /// Gets the product.
        /// </summary>
        /// <value>
        /// The product.
        /// </value>
        public string Product { get; private set; }

        /// <summary>
        /// Gets the model.
        /// </summary>
        /// <value>
        /// The model.
        /// </value>
        public string Model { get; private set; }

        /// <summary>
        /// Gets the device.
        /// </summary>
        /// <value>
        /// The device identifier.
        /// </value>
        public string DeviceProperty { get; private set; }

        /// <summary>
        /// Gets the device state
        /// </summary>
        public DeviceState State { get; internal set; }

        /// <summary>
        /// Gets the device mount points.
        /// </summary>
        public Dictionary<string, MountPoint> MountPoints { get; set; }

        /// <summary>
        /// Returns the device properties. It contains the whole output of 'getprop'
        /// </summary>
        /// <value>The properties.</value>
        public Dictionary<string, string> Properties { get; private set; }

        /// <summary>
        /// Gets the environment variables.
        /// </summary>
        /// <value>The environment variables.</value>
        public Dictionary<string, string> EnvironmentVariables { get; private set; }

        /// <summary>
        /// Gets the property value.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <returns>
        /// the value or <see langword="null"/> if the property does not exist.
        /// </returns>
        public string GetProperty(string name)
        {
            return this.GetProperty(new string[] { name });
        }

        /// <summary>
        /// Gets the first property that exists in the array of property names.
        /// </summary>
        /// <param name="name">The array of property names.</param>
        /// <returns></returns>
        public string GetProperty(params string[] name)
        {
            foreach (var item in name)
            {
                if (this.Properties.ContainsKey(item))
                {
                    return this.Properties[item];
                }
            }

            return null;
        }

        /// <summary>
        /// Gets a value indicating whether the device is online.
        /// </summary>
        /// <value><see langword="true"/> if the device is online; otherwise, <see langword="false"/>.</value>
        public bool IsOnline
        {
            get
            {
                return this.State == DeviceState.Online;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this device is emulator.
        /// </summary>
        /// <value><see langword="true"/> if this device is emulator; otherwise, <see langword="false"/>.</value>
        public bool IsEmulator
        {
            get
            {
                return Regex.Match(this.SerialNumber, RE_EMULATOR_SN).Success;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this device is offline.
        /// </summary>
        /// <value><see langword="true"/> if this device is offline; otherwise, <see langword="false"/>.</value>
        public bool IsOffline
        {
            get
            {
                return this.State == DeviceState.Offline;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this device is in boot loader mode.
        /// </summary>
        /// <value>
        /// 	<see langword="true"/> if this device is in boot loader mode; otherwise, <see langword="false"/>.
        /// </value>
        public bool IsBootLoader
        {
            get
            {
                return this.State == DeviceState.BootLoader;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is recovery.
        /// </summary>
        /// <value>
        /// 	<see langword="true"/> if this instance is recovery; otherwise, <see langword="false"/>.
        /// </value>
        public bool IsRecovery
        {
            get { return this.State == DeviceState.Recovery; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is unauthorized.
        /// </summary>
        /// <value>
        /// 	<see langword="true"/> if this instance is unauthorized; otherwise, <see langword="false"/>.
        /// </value>
        public bool IsUnauthorized
        {
            get { return this.State == DeviceState.Unauthorized; }
        }

        /// <summary>
        /// Remounts the mount point.
        /// </summary>
        /// <param name="mnt">The mount point.</param>
        /// <param name="readOnly">if set to <see langword="true"/> the mount poine will be set to read-only.</param>
        public void RemountMountPoint(MountPoint mnt, bool readOnly)
        {
            string command = string.Format("mount -o {0},remount -t {1} {2} {3}", readOnly ? "ro" : "rw", mnt.FileSystem, mnt.Block, mnt.Name);
            this.ExecuteShellCommand(command, null);
            this.RefreshMountPoints();
        }

        /// <summary>
        /// Remounts the mount point.
        /// </summary>
        /// <param name="mountPoint">the mount point</param>
        /// <param name="readOnly">if set to <see langword="true"/> the mount poine will be set to read-only.</param>
        /// <exception cref="IOException">Throws if the mount point does not exist.</exception>
        public void RemountMountPoint(string mountPoint, bool readOnly)
        {
            if (this.MountPoints.ContainsKey(mountPoint))
            {
                MountPoint mnt = this.MountPoints[mountPoint];
                this.RemountMountPoint(mnt, readOnly);
            }
            else
            {
                throw new IOException("Invalid mount point");
            }
        }

        /// <summary>
        /// Refreshes the mount points.
        /// </summary>
        public void RefreshMountPoints()
        {
            if (this.IsOnline)
            {
                try
                {
                    this.ExecuteShellCommand(MountPointReceiver.MOUNT_COMMAND, new MountPointReceiver(this));
                }
                catch (AdbException)
                {
                }
            }
        }

        /// <summary>
        /// Refreshes the environment variables.
        /// </summary>
        public void RefreshEnvironmentVariables()
        {
            if (this.IsOnline)
            {
                try
                {
                    this.ExecuteShellCommand(EnvironmentVariablesReceiver.PrintEnvCommand, new EnvironmentVariablesReceiver());
                }
                catch (AdbException)
                {
                }
            }
        }

        /// <summary>
        /// Refreshes the properties.
        /// </summary>
        public void RefreshProperties()
        {
            if (this.IsOnline)
            {
                try
                {
                    this.Properties.Clear();

                    var receiver = new GetPropReceiver();
                    this.ExecuteShellCommand(GetPropReceiver.GetpropCommand, receiver);

                    foreach (var property in receiver.Properties)
                    {
                        this.Properties.Add(property.Key, property.Value);
                }

                    this.OnBuildInfoChanged(EventArgs.Empty);
                }
                catch (AdbException aex)
                {
                    Log.Warn(LOG_TAG, aex);
                }
            }
        }

        /// <summary>
        /// Reboots the device in to the specified state
        /// </summary>
        /// <param name="into">The reboot state</param>
        public void Reboot(string into)
        {
            AdbClient.Instance.Reboot(into, this.DeviceData);
        }

        /// <summary>
        /// Reboots the device.
        /// </summary>
        public void Reboot()
        {
            this.Reboot(string.Empty);
        }

        /// <summary>
        /// Gets the battery level.
        /// </summary>
        /// <returns></returns>
        public BatteryInfo GetBatteryInfo()
        {
            return this.GetBatteryInfo(5 * 60 * 1000);
        }

        /// <summary>
        /// Gets the battery level.
        /// </summary>
        /// <param name="freshness">The freshness.</param>
        /// <returns></returns>
        public BatteryInfo GetBatteryInfo(long freshness)
        {
            if (this.lastBatteryInfo != null
                                && this.lastBatteryCheckTime > DateTime.Now.AddMilliseconds(-freshness))
            {
                return this.lastBatteryInfo;
            }

            var receiver = new BatteryReceiver();
            this.ExecuteShellCommand("dumpsys battery", receiver, BATTERY_TIMEOUT);
            this.lastBatteryInfo = receiver.BatteryInfo;
            this.lastBatteryCheckTime = DateTime.Now;
            return this.lastBatteryInfo;
        }

        /// <summary>
        /// Returns a <see cref="SyncService"/> object to push / pull files to and from the device.
        /// </summary>
        /// <value></value>
        /// <remarks>
        /// 	<see langword="null"/> if the SyncService couldn't be created. This can happen if adb
        /// refuse to open the connection because the {@link IDevice} is invalid (or got disconnected).
        /// </remarks>
        /// <exception cref="IOException">Throws IOException if the connection with adb failed.</exception>
        public ISyncService SyncService
        {
            get
            {
                ISyncService syncService = Factories.SyncServiceFactory(this.DeviceData);
                try
                {
                    syncService.Open();
                    return syncService;
                }
                catch (Exception)
                {
                }

                return null;
            }
        }

        /// <summary>
        /// Takes a screen shot of the device and returns it as a <see cref="RawImage"/>
        /// </summary>
        /// <value>The screenshot.</value>
        public Image Screenshot
        {
            get
            {
                return AdbClient.Instance.GetFrameBufferAsync(this.DeviceData, CancellationToken.None).Result;
            }
        }

        /// <summary>
        /// Executes a shell command on the device, and sends the result to a receiver.
        /// </summary>
        /// <param name="command">The command to execute</param>
        /// <param name="receiver">The receiver object getting the result from the command.</param>
        public void ExecuteShellCommand(string command, IShellOutputReceiver receiver)
        {
            this.ExecuteShellCommand(command, receiver, new object[] { });
        }

        /// <summary>
        /// Executes a shell command on the device, and sends the result to a receiver.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="receiver">The receiver.</param>
        /// <param name="timeout">The timeout.</param>
        public void ExecuteShellCommand(string command, IShellOutputReceiver receiver, int timeout)
        {
            this.ExecuteShellCommand(command, receiver, new object[] { });
        }

        /// <summary>
        /// Executes a shell command on the device, and sends the result to a receiver.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="receiver">The receiver.</param>
        /// <param name="commandArgs">The command args.</param>
        public void ExecuteShellCommand(string command, IShellOutputReceiver receiver, params object[] commandArgs)
        {
            AdbClient.Instance.ExecuteRemoteCommand(string.Format(command, commandArgs), this.DeviceData, receiver);
        }

        /// <summary>
        /// Executes a shell command on the device, and sends the result to a receiver.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="receiver">The receiver.</param>
        /// <param name="timeout">The timeout.</param>
        /// <param name="commandArgs">The command args.</param>
        public void ExecuteShellCommand(string command, IShellOutputReceiver receiver, int timeout, params object[] commandArgs)
        {
            AdbClient.Instance.ExecuteRemoteCommand(string.Format(command, commandArgs), this.DeviceData, receiver);
        }

        /// <summary>
        /// Executes a shell command on the device as root, and sends the result to a receiver.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="receiver">The receiver.</param>
        /// <param name="timeout">The period, in milliseconds, after which the command times out.</param>
        public void ExecuteRootShellCommand(string command, IShellOutputReceiver receiver, int timeout)
        {
            this.ExecuteRootShellCommand(command, receiver, timeout, new object[] { });
        }

        /// <summary>
        /// Executes a shell command on the device as root, and sends the result to a receiver.
        /// </summary>
        /// <param name="command">The command to execute</param>
        /// <param name="receiver">The receiver object getting the result from the command.</param>
        public void ExecuteRootShellCommand(string command, IShellOutputReceiver receiver)
        {
            this.ExecuteRootShellCommand(command, receiver, int.MaxValue);
        }

        /// <summary>
        /// Executes a root shell command on the device, and sends the result to a receiver.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="receiver">The receiver.</param>
        /// <param name="commandArgs">The command args.</param>
        public void ExecuteRootShellCommand(string command, IShellOutputReceiver receiver, params object[] commandArgs)
        {
            this.ExecuteRootShellCommand(command, receiver, int.MaxValue, commandArgs);
        }

        /// <summary>
        /// Executes a root shell command on the device, and sends the result to a receiver.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="receiver">The receiver.</param>
        /// <param name="timeout">The timeout.</param>
        /// <param name="commandArgs">The command args.</param>
        public void ExecuteRootShellCommand(string command, IShellOutputReceiver receiver, int timeout, params object[] commandArgs)
        {
            AdbClient.Instance.ExecuteRemoteCommand(string.Format("su -c \"{0}\"", command), this.DeviceData, receiver);
        }
        
        /// <summary>
        /// Runs the log service.
        /// </summary>
        /// <param name="logname">
        /// The names of the log files to retrieve.
        /// </param>
        public IEnumerable<LogEntry> RunLogService(params LogId[] logNames)
        {
            return AdbClient.Instance.RunLogService(this.DeviceData, logNames);
        }

        /// <summary>
        /// Creates a port forwarding between a local and a remote port.
        /// </summary>
        /// <param name="localPort">the local port to forward</param>
        /// <param name="remotePort">the remote port.</param>
        /// <returns><see langword="true"/> if success.</returns>
        public bool CreateForward(int localPort, int remotePort)
        {
            try
            {
                AdbClient.Instance.CreateForward(this.DeviceData, localPort, remotePort);
                return true;
            }
            catch (IOException e)
            {
                Log.Warn("ddms", e);
                return false;
            }
        }

        /// <summary>
        /// Removes a port forwarding between a local and a remote port.
        /// </summary>
        /// <param name="localPort">the local port to forward</param>
        /// <returns><see langword="true"/> if success.</returns>
        public bool RemoveForward(int localPort)
        {
            try
            {
                AdbClient.Instance.RemoveForward(this.DeviceData, localPort);
                return true;
            }
            catch (IOException e)
            {
                Log.Warn("ddms", e);
                return false;
            }
        }

        public DeviceData DeviceData
        {
            get
            {
                return new DeviceData()
                {
                    Model = this.Model,
                    Name = this.DeviceProperty,
                    Product = this.Product,
                    Serial = this.SerialNumber,
                    State = this.State
                };
            }
        }
        /*
        public String GetClientName ( int pid ) {
            lock ( ClientList ) {
                foreach ( Client c in ClientList ) {
                    if ( c.ClientData ( ).Pid == pid ) {
                        return c.ClientData.ClientDescription;
                    }
                }
            }

            return null;
        }

        DeviceMonitor Monitor { get; private set; }

        void AddClient ( Client client ) {
            lock ( ClientList ) {
                ClientList.Add ( client );
            }
        }

        List<Client> ClientList { get; private set; }

        bool HasClient ( int pid ) {
            lock ( ClientList ) {
                foreach ( Client client in ClientList ) {
                    if ( client.ClientData.Pid == pid ) {
                        return true;
                    }
                }
            }

            return false;
        }

        void ClearClientList ( ) {
            lock ( ClientList ) {
                ClientList.Clear ( );
            }
        }

        SocketChannel ClientMonitoringSocket { get; set; }

        void RemoveClient ( Client client, bool notify ) {
            Monitor.AddPortToAvailableList ( client.DebuggerListenPort );
            lock ( ClientList ) {
                ClientList.Remove ( client );
            }
            if ( notify ) {
                Monitor.Server.DeviceChanged ( this, CHANGE_CLIENT_LIST );
            }
        }

        void Update ( int changeMask ) {
            Monitor.Server.DeviceChanged ( this, changeMask );
        }

        void Update ( Client client, int changeMask ) {
            Monitor.Server.ClientChanged ( client, changeMask );
        }
*/

        /// <summary>
        /// Raises the <see cref="E:StateChanged"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        internal void OnStateChanged(EventArgs e)
        {
            if (this.StateChanged != null)
            {
                this.StateChanged(this, e);
            }
        }

        /// <summary>
        /// Raises the <see cref="E:BuildInfoChanged"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        internal void OnBuildInfoChanged(EventArgs e)
        {
            if (this.BuildInfoChanged != null)
            {
                this.BuildInfoChanged(this, e);
            }
        }

        /// <summary>
        /// Raises the <see cref="E:ClientListChanged"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        internal void OnClientListChanged(EventArgs e)
        {
            if (this.ClientListChanged != null)
            {
                this.ClientListChanged(this, e);
            }
        }
    }
}