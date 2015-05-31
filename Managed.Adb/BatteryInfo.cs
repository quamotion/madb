using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using MoreLinq;

namespace Managed.Adb {
	/// <summary>
	/// 
	/// </summary>
	public class BatteryInfo {
		/// <summary>
		/// Battery Status Types
		/// </summary>
		public enum StatusTypes {
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
		public enum HealthTypes {
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
		///   <c>true</c> if AC powered; otherwise, <c>false</c>.
		/// </value>
		public bool ACPowered { get; set; }
		/// <summary>
		/// Gets or sets a value indicating whether usb powered.
		/// </summary>
		/// <value>
		///   <c>true</c> if usb powered; otherwise, <c>false</c>.
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
		///   <c>true</c> if present; otherwise, <c>false</c>.
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
		public String Type { get; set; }

		/// <summary>
		/// Gets the calculated battery level.
		/// </summary>
		/// <value>
		/// The calculated level.
		/// </value>
		public int CalculatedLevel {
			get {
				return ( Level * 100 ) / Scale;
			}
		}

		/// <summary>
		/// Returns a <see cref="System.String" /> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String" /> that represents this instance.
		/// </returns>
		public override string ToString ( ) {
			var sb = new StringBuilder ( );

			this.GetType ( ).GetProperties ( BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty | BindingFlags.GetProperty | BindingFlags.IgnoreCase ).ForEach ( p => {
				var n = p.Name;
				var v = p.GetValue ( this, null );
				sb.AppendLine ( String.Format ( "{0}:{1}", n, v ) );
			} );

			return sb.ToString ( );
		}
	}
}
