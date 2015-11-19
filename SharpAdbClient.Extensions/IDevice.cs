// <copyright file="IDevice.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion. All rights reserved.
// </copyright>

namespace SharpAdbClient
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using SharpAdbClient.Exceptions;

    /// <summary>
    ///
    /// </summary>
    public interface IDevice
    {
        /// <summary>
        /// Occurs when device state changed.
        /// </summary>
        event EventHandler<EventArgs> StateChanged;

        /// <summary>
        /// Occurs when build info changed.
        /// </summary>
        event EventHandler<EventArgs> BuildInfoChanged;

        /// <summary>
        /// Occurs when client list changed.
        /// </summary>
        event EventHandler<EventArgs> ClientListChanged;

        DeviceData DeviceData { get; }

        /// <summary>
        /// Gets the serial number of the device.
        /// </summary>
        /// <value>The serial number.</value>
        string SerialNumber { get; }

        /// <summary>
        /// Gets the TCP endpoint defined when the transport is TCP.
        /// </summary>
        /// <value>
        /// The endpoint.
        /// </value>
        IPEndPoint Endpoint { get; }

        /// <summary>
        /// Gets the type of the transport used to connect to this device.
        /// </summary>
        /// <value>
        /// The type of the transport.
        /// </value>
        TransportType TransportType { get; }

        /// <summary>
        /// Returns the name of the AVD the emulator is running.
        /// <p/>
        /// This is only valid if {@link #isEmulator()} returns true.
        /// <p/>
        /// If the emulator is not running any AVD (for instance it's running from an Android source
        /// tree build), this method will return "<code>&lt;build&gt;</code>"
        /// @return the name of the AVD or  <see langword="null"/>
        ///  if there isn't any.
        /// </summary>
        /// <value>The name of the avd.</value>
        string AvdName { get; set; }

        /// <summary>
        /// Gets the environment variables.
        /// </summary>
        /// <value>The environment variables.</value>
        Dictionary<string, string> EnvironmentVariables { get; }

        /// <summary>
        /// Gets the mount points.
        /// </summary>
        /// <value>The mount points.</value>
        Dictionary<string, MountPoint> MountPoints { get; }

        /// <summary>
        /// Gets the state.
        /// </summary>
        /// <value>The state.</value>
        /// Returns the state of the device.
        DeviceState State { get; }

        /// <summary>
        /// Returns the device properties. It contains the whole output of 'getprop'
        /// </summary>
        /// <value>The properties.</value>
        Dictionary<string, string> Properties { get; }

        /// <summary>
        /// Gets the property value.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <returns>
        /// the value or <see langword="null"/> if the property does not exist.
        /// </returns>
        string GetProperty(string name);

        /// <summary>
        /// Gets the first property that exists in the array of property names.
        /// </summary>
        /// <param name="name">The array of property names.</param>
        /// <returns>
        /// the value or <see langword="null"/> if the property does not exist.
        /// </returns>
        string GetProperty(params string[] name);

        /// <summary>
        /// Gets a value indicating whether the device is online.
        /// </summary>
        /// <value><see langword="true"/> if the device is online; otherwise, <see langword="false"/>.</value>
        bool IsOnline { get; }

        /// <summary>
        /// Gets a value indicating whether this device is emulator.
        /// </summary>
        /// <value>
        /// 	<see langword="true"/> if this device is emulator; otherwise, <see langword="false"/>.
        /// </value>
        bool IsEmulator { get; }

        /// <summary>
        /// Gets a value indicating whether this device is offline.
        /// </summary>
        /// <value>
        /// 	<see langword="true"/> if this device is offline; otherwise, <see langword="false"/>.
        /// </value>
        bool IsOffline { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is unauthorized.
        /// </summary>
        /// <value>
        /// 	<see langword="true"/> if this instance is unauthorized; otherwise, <see langword="false"/>.
        /// </value>
        bool IsUnauthorized { get; }

        /// <summary>
        /// Gets a value indicating whether this device is in boot loader mode.
        /// </summary>
        /// <value>
        /// 	<see langword="true"/> if this device is in boot loader mode; otherwise, <see langword="false"/>.
        /// </value>
        bool IsBootLoader { get; }

        /// <summary>
        /// Returns a <see cref="SyncService"/> object to push / pull files to and from the device.
        /// </summary>
        /// <remarks>
        /// <see langword="null"/> if the SyncService couldn't be created. This can happen if adb
        /// refuse to open the connection because the {@link IDevice} is invalid (or got disconnected).
        /// </remarks>
        /// <exception cref="IOException">Throws IOException if the connection with adb failed.</exception>
        ISyncService SyncService { get; }

        /// <summary>
        /// Takes a screen shot of the device and returns it as a <see cref="RawImage"/>
        /// </summary>
        /// <value>The screenshot.</value>
        Image Screenshot { get; }

        /// <summary>
        /// Determines whether this instance can use the SU shell.
        /// </summary>
        /// <returns>
        ///   <see langword="true"/> if this instance can use the SU shell; otherwise, <see langword="false"/>.
        /// </returns>
        bool CanSU();

        /// <summary>
        /// Remounts the mount point.
        /// </summary>
        /// <param name="mnt">The mount point.</param>
        /// <param name="readOnly">if set to <see langword="true"/> the mount poine will be set to read-only.</param>
        void RemountMountPoint(MountPoint mnt, bool readOnly);

        /// <summary>
        /// Remounts the mount point.
        /// </summary>
        /// <param name="mountPoint">the mount point</param>
        /// <param name="readOnly">if set to <see langword="true"/> the mount poine will be set to read-only.</param>
        /// <exception cref="IOException">Throws if the mount point does not exist.</exception>
        void RemountMountPoint(string mountPoint, bool readOnly);

        /// <summary>
        /// Executes a shell command on the device, and sends the result to a receiver.
        /// </summary>
        /// <param name="command">The command to execute</param>
        /// <param name="receiver">The receiver object getting the result from the command.</param>
        void ExecuteShellCommand(string command, IShellOutputReceiver receiver);

        /// <summary>
        /// Executes a shell command on the device, and sends the result to a receiver.
        /// </summary>
        /// <param name="command">The command to execute</param>
        /// <param name="receiver">The receiver object getting the result from the command.</param>
        /// <param name="maxTimeToOutputResponse">The max time to output response.</param>
        void ExecuteShellCommand(string command, IShellOutputReceiver receiver, int maxTimeToOutputResponse);

        /// <summary>
        /// Executes a shell command on the device, and sends the result to a receiver.
        /// </summary>
        /// <param name="command">The command to execute</param>
        /// <param name="receiver">The receiver object getting the result from the command.</param>
        /// <param name="commandArgs">The command args.</param>
        void ExecuteShellCommand(string command, IShellOutputReceiver receiver, params object[] commandArgs);

        /// <summary>
        /// Executes a shell command on the device, and sends the result to a receiver.
        /// </summary>
        /// <param name="command">The command to execute</param>
        /// <param name="receiver">The receiver object getting the result from the command.</param>
        /// <param name="maxTimeToOutputResponse">The max time to output response.</param>
        /// <param name="commandArgs">The command args.</param>
        void ExecuteShellCommand(string command, IShellOutputReceiver receiver, int maxTimeToOutputResponse, params object[] commandArgs);

        /// <summary>
        /// Executes a shell command on the device as root, and sends the result to a receiver.
        /// </summary>
        /// <param name="command">The command to execute</param>
        /// <param name="receiver">The receiver object getting the result from the command.</param>
        /// <param name="commandArgs">The command args.</param>
        void ExecuteRootShellCommand(string command, IShellOutputReceiver receiver, params object[] commandArgs);

        /// <summary>
        /// Executes a shell command on the device as root, and sends the result to a receiver.
        /// </summary>
        /// <param name="command">The command to execute</param>
        /// <param name="receiver">The receiver object getting the result from the command.</param>
        /// <param name="maxTimeToOutputResponse">The max time to output response.</param>
        /// <param name="commandArgs">The command args.</param>
        void ExecuteRootShellCommand(string command, IShellOutputReceiver receiver, int maxTimeToOutputResponse, params object[] commandArgs);

        /// <summary>
        /// Executes a shell command on the device as root, and sends the result to a receiver.
        /// </summary>
        /// <param name="command">The command to execute</param>
        /// <param name="receiver">The receiver object getting the result from the command.</param>
        void ExecuteRootShellCommand(string command, IShellOutputReceiver receiver);

        /// <summary>
        /// Executes a shell command on the device as root, and sends the result to a receiver.
        /// </summary>
        /// <param name="command">The command to execute</param>
        /// <param name="receiver">The receiver object getting the result from the command.</param>
        /// <param name="maxTimeToOutputResponse">The max time to output response.</param>
        void ExecuteRootShellCommand(string command, IShellOutputReceiver receiver, int maxTimeToOutputResponse);

        /// <summary>
        /// Creates a port forwarding between a local and a remote port.
        /// </summary>
        /// <param name="localPort">the local port to forward</param>
        /// <param name="remotePort">the remote port.</param>
        /// <returns><see langword="true"/> if success.</returns>
        bool CreateForward(int localPort, int remotePort);

        /// <summary>
        /// Removes a port forwarding between a local and a remote port.
        /// </summary>
        /// <param name="localPort"> the local port to forward</param>
        /// <returns><see langword="true"/> if success.</returns>
        bool RemoveForward(int localPort);

        /// <summary>
        /// Refreshes the environment variables.
        /// </summary>
        void RefreshEnvironmentVariables();

        /// <summary>
        /// Refreshes the mount points.
        /// </summary>
        void RefreshMountPoints();

        /// <summary>
        /// Refreshes the properties.
        /// </summary>
        void RefreshProperties();

        /// <summary>
        /// Reboots the device.
        /// </summary>
        void Reboot();
    }
}
