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
        /// The tag to use when logging.
        /// </summary>
        public const string Tag = nameof(AndroidDebugBridge);

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
        /// Gets the devices.
        /// </summary>
        /// <value>The devices.</value>
        public IList<DeviceData> Devices
        {
            get
            {
                return AdbHelper.Instance.GetDevices(AdbServer.SocketAddress);
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
        /// Terminates the ddm library. This must be called upon application termination.
        /// </summary>
        public void Dispose()
        {
            // kill the monitoring services
            if (this.DeviceMonitor != null)
            {
                this.DeviceMonitor.Dispose();
                this.DeviceMonitor = null;
            }
        }

        /// <summary>
        /// Creates a <see cref="AndroidDebugBridge"/> that is not linked to any particular executable.
        /// This bridge will expect <c>adb.exe</c> to be running. It will not be able to start/stop/restart
        /// adb.
        /// </summary>
        /// <returns>
        /// A new instance of the <see cref="AndroidDebugBridge"/> class.
        /// </returns>
        public static AndroidDebugBridge CreateBridge()
        {
            var value = new AndroidDebugBridge();
            value.Start();
            return value;
        }

        /// <summary>
        /// Starts the debug bridge server.
        /// </summary>
        public void Start()
        {
            // now that the bridge is connected, we start the underlying services.
            this.DeviceMonitor = new DeviceMonitor(AdbHelper.SocketFactory.Create(AdbServer.SocketAddress));
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
                this.DeviceMonitor.Dispose();
                this.DeviceMonitor = null;
            }

            this.Started = false;
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
    }
}
