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
    public class DeviceMonitor
    {
        /// <summary>
        /// Logging tag
        /// </summary>
        private const string TAG = "DeviceMonitor";

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceMonitor"/> class.
        /// </summary>
        public DeviceMonitor(AdbSocket socket)
        {
            this.Socket = socket;
            this.Devices = new List<DeviceData>();
        }

        /// <summary>
        /// Gets the devices.
        /// </summary>
        public IList<DeviceData> Devices { get; private set; }

        public AdbSocket Socket { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance is monitoring.
        /// </summary>
        /// <value>
        /// 	<see langword="true"/> if this instance is monitoring; otherwise, <see langword="false"/>.
        /// </value>
        public bool IsMonitoring { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance is running.
        /// </summary>
        /// <value>
        /// 	<see langword="true"/> if this instance is running; otherwise, <see langword="false"/>.
        /// </value>
        public bool IsRunning { get; private set; }

        /// <summary>
        /// Gets or sets the main adb connection.
        /// </summary>
        /// <value>
        /// The main adb connection.
        /// </value>
        private Socket MainAdbConnection { get; set; }

        /// <summary>
        /// Starts the monitoring
        /// </summary>
        public void Start()
        {
            Thread t = new Thread(new ThreadStart(this.DeviceMonitorLoop));
            t.Name = "Device List Monitor";
            t.Start();
        }

        /// <summary>
        /// Stops the monitoring
        /// </summary>
        public void Stop()
        {
            this.IsRunning = false;

            // wakeup the main loop thread by closing the main connection to adb.
            try
            {
                if (this.MainAdbConnection != null)
                {
                    this.MainAdbConnection.Close();
                }
            }
            catch (IOException)
            {
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
                    string lenHex = reply.GetString(AdbHelper.DefaultEncoding);
                    int len = int.Parse(lenHex, NumberStyles.HexNumber);

                    // And get the string
                    reply = new byte[len];
                    this.Socket.Read(reply);

                    string value = reply.GetString(AdbHelper.DefaultEncoding);

                    this.ProcessIncomingDeviceData(value);
                }
                catch (Exception ex)
                {
                    Log.e(TAG, ex);
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
                    Log.e(TAG, ae);
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
