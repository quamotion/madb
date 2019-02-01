// <copyright file="DeviceData.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion. All rights reserved.
// </copyright>

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
        /// A regular expression that can be used to parse the device information that is returned
        /// by the Android Debut Bridge.
        /// </summary>
        internal const string DeviceDataRegexString = @"^(?<serial>[a-zA-Z0-9_-]+(?:\s?[\.a-zA-Z0-9_-]+)?(?:\:\d{1,})?)\s+(?<state>device|connecting|offline|unknown|bootloader|recovery|download|authorizing|unauthorized|host|no permissions)(?<message>.*?)(\s+usb:(?<usb>[^:]+))?(?:\s+product:(?<product>[^:]+))?(\s+model\:(?<model>[\S]+))?(\s+device\:(?<device>[\S]+))?(\s+features:(?<features>[^:]+))?(\s+transport_id:(?<transport_id>[^:]+))?$";

        /// <summary>
        /// A regular expression that can be used to parse the device information that is returned
        /// by the Android Debut Bridge.
        /// </summary>
        private static readonly Regex Regex = new Regex(DeviceDataRegexString, RegexOptions.Compiled | RegexOptions.IgnoreCase);

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
        /// Gets or sets the transport ID for this device.
        /// </summary>
        public string TransportId
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the device info message. Currently only seen for NoPermissions state.
        /// </summary>
        public string Message
        {
            get;
            set;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="DeviceData"/> class based on
        /// data retrieved from the Android Debug Bridge.
        /// </summary>
        /// <param name="data">
        /// The data retrieved from the Android Debug Bridge that represents a device.
        /// </param>
        /// <returns>
        /// A <see cref="DeviceData"/> object that represents the device.
        /// </returns>
        public static DeviceData CreateFromAdbData(string data)
        {
            Match m = Regex.Match(data);
            if (m.Success)
            {
                return new DeviceData()
                {
                    Serial = m.Groups["serial"].Value,
                    State = GetStateFromString(m.Groups["state"].Value),
                    Model = m.Groups["model"].Value,
                    Product = m.Groups["product"].Value,
                    Name = m.Groups["device"].Value,
                    Features = m.Groups["features"].Value,
                    Usb = m.Groups["usb"].Value,
                    TransportId = m.Groups["transport_id"].Value,
                    Message = m.Groups["message"].Value
                };
            }
            else
            {
                throw new ArgumentException($"Invalid device list data '{data}'");
            }
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
