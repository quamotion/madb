// <copyright file="DeviceMonitor.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion. All rights reserved.
// </copyright>

namespace Managed.Adb
{
    using MoreLinq;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net.Sockets;
    using System.Threading;

    /// <summary>
    /// A Device monitor. This connects to the Android Debug Bridge and get device and
    /// debuggable process information from it.
    /// </summary>
    public class DeviceMonitor : IDisposable
    {
        /// <summary>
        /// Logging tag
        /// </summary>
        private const string Tag = nameof(DeviceMonitor);

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceMonitor"/> class.
        /// </summary>
        public DeviceMonitor(IAdbSocket socket)
        {
            this.Socket = socket;
            this.Devices = new List<DeviceData>();
        }

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
        /// Gets the devices that are currently connected to the Android Debug Bridge.
        /// </summary>
        public IList<DeviceData> Devices { get; private set; }

        /// <summary>
        /// Gets the <see cref="IAdbSocket"/> that represents the connection to the
        /// Android Debug Bridge.
        /// </summary>
        public IAdbSocket Socket { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance is running.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if this instance is running; otherwise, <see langword="false"/>.
        /// </value>
        public bool IsRunning { get; private set; }

        Thread monitorThread;

        /// <summary>
        /// Starts the monitoring
        /// </summary>
        public void Start()
        {
            if (this.monitorThread == null)
            {
                this.monitorThread = new Thread(new ThreadStart(this.DeviceMonitorLoop));
                this.monitorThread.Name = "Device List Monitor";
                this.monitorThread.Start();
            }
        }

        /// <summary>
        /// Stops the monitoring
        /// </summary>
        public void Dispose()
        {
            // Signal the monitor thread to stop.
            if (this.monitorThread != null)
            {
                this.IsRunning = false;

                // Wait for the monitor thread to stop.
                this.monitorThread.Join();

                this.monitorThread = null;
            }

            // Close the connection to adb
            if (this.Socket != null)
            {
                this.Socket.Dispose();
                this.Socket = null;
            }
        }

        /// <summary>
        /// Raises the <see cref="DeviceChanged"/> event.
        /// </summary>
        /// <param name="e">The <see cref="DeviceDataEventArgs"/> instance containing the event data.</param>
        protected void OnDeviceChanged(DeviceDataEventArgs e)
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
        protected void OnDeviceConnected(DeviceDataEventArgs e)
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
        protected void OnDeviceDisconnected(DeviceDataEventArgs e)
        {
            if (this.DeviceDisconnected != null)
            {
                this.DeviceDisconnected(this, e);
            }
        }

        /// <summary>
        /// Monitors the devices. This connects to the Debug Bridge
        /// </summary>
        private void DeviceMonitorLoop()
        {
            this.IsRunning = true;

            // Set up the connection to track the list of devices.
            this.Socket.SendAdbRequest("host:track-devices");
            this.Socket.ReadAdbResponse(false);

            do
            {
                try
                {
                    // read the length of the incoming message
                    // The first 4 bytes contain the length of the string
                    var reply = new byte[4];
                    this.Socket.Read(reply, int.MaxValue);

                    // Convert the bytes to a hex string
                    string lenHex = AdbHelper.Encoding.GetString(reply);
                    int len = int.Parse(lenHex, NumberStyles.HexNumber);

                    // And get the string
                    reply = new byte[len];
                    this.Socket.Read(reply);

                    string value = AdbHelper.Encoding.GetString(reply);

                    this.ProcessIncomingDeviceData(value);
                }
                catch (Exception ex)
                {
                    Log.e(Tag, ex);
                }
            }
            while (this.IsRunning);
        }

        /// <summary>
        /// Processes the incoming device data.
        /// </summary>
        private void ProcessIncomingDeviceData(string result)
        {
            List<DeviceData> list = new List<DeviceData>();

            string[] devices = result.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            devices.ForEach(d =>
            {
                try
                {
                    var dv = DeviceData.CreateFromAdbData(d);
                    if (dv != null)
                    {
                        list.Add(dv);
                    }
                }
                catch (ArgumentException ae)
                {
                    Log.e(Tag, ae);
                }
            });

            // now merge the new devices with the old ones.
            this.UpdateDevices(list);
        }

        private void UpdateDevices(List<DeviceData> list)
        {
            lock (this.Devices)
            {
                // For each device in the current list, we look for a matching the new list.
                // * if we find it, we update the current object with whatever new information
                //   there is
                //   (mostly state change, if the device becomes ready, we query for build info).
                //   We also remove the device from the new list to mark it as "processed"
                // * if we do not find it, we remove it from the current list.
                // Once this is done, the new list contains device we aren't monitoring yet, so we
                // add them to the list, and start monitoring them.

                // Add or update existing devices
                foreach (var device in list)
                {
                    var existingDevice = this.Devices.SingleOrDefault(d => d.Serial == device.Serial);

                    if (existingDevice == null)
                    {
                        this.Devices.Add(device);
                    }
                    else
                    {
                        existingDevice.State = device.State;
                    }
                }

                // Remove devices
                foreach (var device in this.Devices.Where(d => !list.Any(e => e.Serial == d.Serial)).ToArray())
                {
                    this.Devices.Remove(device);
                }
            }
        }
    }
}
