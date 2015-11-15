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
        internal const string DeviceDataRegex = @"^([a-z0-9_-]+(?:\s?[\.a-z0-9_-]+)?(?:\:\d{1,})?)\s+(device|offline|unknown|bootloader|recovery|download|unauthorized)(?:\s+product:([^:]+)\s+model\:([\S]+)\s+device\:([\S]+))?$";

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
            Regex re = new Regex(DeviceDataRegex, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Match m = re.Match(data);
            if (m.Success)
            {
                return new DeviceData()
                {
                    Serial = m.Groups[1].Value,
                    State = GetStateFromString(m.Groups[2].Value),
                    Model = m.Groups[4].Value,
                    Product = m.Groups[3].Value,
                    Name = m.Groups[5].Value
                };
            }
            else
            {
                throw new ArgumentException("Invalid device list data");
            }
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
