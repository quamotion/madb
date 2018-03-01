// <copyright file="DeviceData.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion. All rights reserved.
// </copyright>

using System.Linq;

namespace SharpAdbClient
{
    using System;
    using System.Text.RegularExpressions;

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
        /// <para>device output format is described at here: <![CDATA[https://github.com/aosp-mirror/platform_system_core/blob/09d5e258ef493e823f18412bd7f159f489ddc8bb/adb/transport.cpp#L988</remarks>]]></para>
        /// </summary>
        /// <param name="data">
        /// The data retrieved from the Android Debug Bridge that represents a device.
        /// </param>
        /// <returns>
        /// A <see cref="DeviceData"/> object that represents the device.
        /// </returns>
        public static DeviceData CreateFromAdbData(string data)
        {
            const string productKey = "product:";
            const string modelKey = "model:";
            const string deviceKey = "device:";
            const string transportKey = " transport_id:";

            int productIndex = data.IndexOf(productKey, StringComparison.Ordinal);
            int modelIndex = data.IndexOf(modelKey, StringComparison.Ordinal);
            int deviceIndex = data.IndexOf(deviceKey, StringComparison.Ordinal);
            int transportIndex = data.IndexOf(transportKey, StringComparison.Ordinal);

            int productKeyLength = productIndex + productKey.Length;
            int modelKeyLength = modelIndex + modelKey.Length;
            int deviceKeyLength = deviceIndex + deviceKey.Length;

            // parse serial and state
            var prefix = data.Substring(0, productIndex);
            var splits = prefix.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            var serial = splits.First().Trim();
            var state = GetStateFromString(splits.Last().Trim());

            // parse product, model, device, transport_id
            var product = data.Substring(productKeyLength, modelIndex - productKeyLength).Trim();
            var model = data.Substring(modelKeyLength, deviceIndex - modelKeyLength).Trim();

            string device;
            if (transportIndex == -1) // if only one device is attached, no transport_id is given.
                device = data.Substring(deviceKeyLength).Trim();
            else
            {
                device = data.Substring(deviceKeyLength, transportIndex - deviceKeyLength).Trim();
                var transportId = int.Parse(data.Substring(data.LastIndexOf(":", StringComparison.Ordinal) + 1)); // use when applicable
            }

            return new DeviceData()
            {
                Serial = serial,
                State = state,
                Model = model,
                Product = product,
                Name = device,
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
