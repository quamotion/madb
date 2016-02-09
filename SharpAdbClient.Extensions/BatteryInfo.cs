// <copyright file="BatteryInfo.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion. All rights reserved.
// </copyright>

namespace SharpAdbClient
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;

    /// <summary>
    ///
    /// </summary>
    public class BatteryInfo
    {
        /// <summary>
        /// Battery Status Types
        /// </summary>
        public enum StatusTypes
        {
            /// <summary>
            /// Unknown Status
            /// </summary>
            Unknown = 1,

            /// <summary>
            /// Charging
            /// </summary>
            Charging = 2,

            /// <summary>
            /// Discharging
            /// </summary>
            Discharging = 3,

            /// <summary>
            /// Discharging
            /// </summary>
            NotCharging = 4,

            /// <summary>
            /// Battery Full
            /// </summary>
            Full = 5
        }

        /// <summary>
        /// Battery Health Types
        /// </summary>
        public enum HealthTypes
        {
            /// <summary>
            /// Unknown Health
            /// </summary>
            Unknown = 1,

            /// <summary>
            /// Good
            /// </summary>
            Good = 2,

            /// <summary>
            /// Overheating
            /// </summary>
            Overheat = 3,

            /// <summary>
            /// Cold
            /// </summary>
            Cold = 4,

            /// <summary>
            /// Over voltage
            /// </summary>
            OverVoltage = 5,

            /// <summary>
            /// Unspecified Failure
            /// </summary>
            UnspecifiedFailure = 6
        }

        /// <summary>
        /// Gets or sets a value indicating whether AC powered.
        /// </summary>
        /// <value>
        ///   <see langword="true"/> if AC powered; otherwise, <see langword="false"/>.
        /// </value>
        public bool ACPowered { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether usb powered.
        /// </summary>
        /// <value>
        ///   <see langword="true"/> if usb powered; otherwise, <see langword="false"/>.
        /// </value>
        public bool UsbPowered { get; set; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        public StatusTypes Status { get; set; }

        /// <summary>
        /// Gets or sets the health.
        /// </summary>
        /// <value>
        /// The health.
        /// </value>
        public HealthTypes Health { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a battery is present.
        /// </summary>
        /// <value>
        ///   <see langword="true"/> if present; otherwise, <see langword="false"/>.
        /// </value>
        public bool Present { get; set; }

        /// <summary>
        /// Gets or sets the level.
        /// </summary>
        /// <value>
        /// The level.
        /// </value>
        public int Level { get; set; }

        /// <summary>
        /// Gets or sets the scale.
        /// </summary>
        /// <value>
        /// The scale.
        /// </value>
        public int Scale { get; set; }

        /// <summary>
        /// Gets or sets the voltage.
        /// </summary>
        /// <value>
        /// The voltage.
        /// </value>
        public int Voltage { get; set; }

        /// <summary>
        /// Gets or sets the temperature.
        /// </summary>
        /// <value>
        /// The temperature.
        /// </value>
        public int Temperature { get; set; }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public string Type { get; set; }

        /// <summary>
        /// Gets the calculated battery level.
        /// </summary>
        /// <value>
        /// The calculated level.
        /// </value>
        public int CalculatedLevel
        {
            get
            {
                return (this.Level * 100) / this.Scale;
            }
        }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            var sb = new StringBuilder();

            foreach(var p in this.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty | BindingFlags.GetProperty | BindingFlags.IgnoreCase))
            {
                var n = p.Name;
                var v = p.GetValue(this, null);
                sb.AppendLine(string.Format("{0}:{1}", n, v));
            }

            return sb.ToString();
        }
    }
}
