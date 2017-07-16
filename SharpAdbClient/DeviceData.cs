// <copyright file="DeviceData.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion. All rights reserved.
// </copyright>

namespace SharpAdbClient
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a device that is connected to the Android Debug Bridge.
    /// </summary>
    public class DeviceData
    {
        /// <summary>
        /// Gets or sets the device serial number.
        /// </summary>
        public string Serial
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the device state.
        /// </summary>
        public DeviceState State
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the device model name.
        /// </summary>
        public string Model
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the device product name.
        /// </summary>
        public string Product
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the device name.
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the features available on the device.
        /// </summary>
        public string Features
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the USB port to which this device is connected. Usually available on Linux only.
        /// </summary>
        public string Usb
        {
            get;
            set;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="DeviceData"/> class based on
        /// data retrieved from the Android Debug Bridge.
        /// </summary>
        /// <param name="dataString">
        /// The data retrieved from the Android Debug Bridge that represents a device.
        /// </param>
        /// <returns>
        /// A <see cref="DeviceData"/> object that represents the device.
        /// </returns>
        public static DeviceData CreateFromAdbData(string dataString)
        {
            string[] dataArr = dataString.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (dataArr.Length < 2) 
                throw new ArgumentException($"Invalid device list data '{dataString}'");
            Dictionary<string, string> data = new Dictionary<string, string>();
            for (int i = 0; i < dataArr.Length; i++) 
            {
                if (dataArr[i].Split(':').Length < 2) 
                {
                    if (dataArr[i].Equals("device") ||
                        dataArr[i].Equals("offline") ||
                        dataArr[i].Equals("unknown") ||
                        dataArr[i].Equals("bootloader") ||
                        dataArr[i].Equals("recovery") ||
                        dataArr[i].Equals("download") ||
                        dataArr[i].Equals("unauthorized") ||
                        dataArr[i].Equals("host"))
                        dataArr[i] = "state:" + dataArr[i];
                    else
                        dataArr[i] = "serial:" + dataArr[i];
                }
                var x = dataArr[i].Split(':');
                string val = x[1];
                if (x.Length > 2) 
                    for (int j = 2; j < x.Length; j++)
                        val += ":" + x[j];
                data.Add(x[0], x[1]);
            }
            string serial, state, model, product, device, features, usb;
            data.TryGetValue("serial", out serial);
            data.TryGetValue("state", out state);
            data.TryGetValue("model", out model);
            data.TryGetValue("product", out product);
            data.TryGetValue("device", out device);
            data.TryGetValue("features", out features);
            data.TryGetValue("usb", out usb);
			return new DeviceData() {
                Serial = serial,
                State = (DeviceState)Enum.Parse(typeof(DeviceState), state, true),
                Model = model,
                Product = product,
                Name = device,
                Features = features,
				Usb = usb
			};
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return this.Serial;
        }

        /// <summary>
        /// Get the device state from the string value
        /// </summary>
        /// <param name="state">The device state string</param>
        /// <returns></returns>
        internal static DeviceState GetStateFromString(string state)
        {
            // Default to the unknown state
            DeviceState value = DeviceState.Unknown;

            if (string.Equals(state, "device", StringComparison.OrdinalIgnoreCase))
            {
                // As a special case, the "device" state in ADB is translated to the
                // "Online" state in Managed.Adb
                value = DeviceState.Online;
            }
            else if (string.Equals(state, "no permissions", StringComparison.OrdinalIgnoreCase))
            {
                value = DeviceState.NoPermissions;
            }
            else
            {
                // Else, we try to match a value of the DeviceState enumeration.
                if (!Enum.TryParse<DeviceState>(state, true, out value))
                {
                    value = DeviceState.Unknown;
                }
            }

            return value;
        }
    }
}
