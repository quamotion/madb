using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Managed.Adb.MoreLinq;

namespace Managed.Adb {
	/// <summary>
	/// 
	/// </summary>
	public class BatteryReceiver : MultiLineReceiver {
		private const String TAG = "BatteryReceiver";
		private const String BATTERY_LEVEL = @"\s*level:\s?(\d+)";
		private const String AC_POWER = @"\s*AC\spowered:\s?(true|false)";
		private const String USB_POWER = @"\s*USB\spowered:\s?(true|false)";
		private const String PRESENT = @"\s*present:\s?(true|false)";
		private const String STATUS = @"\s*status:\s(\d+)";
		private const String HEALTH = @"\s*health:\s(\d+)";
		private const String SCALE = @"\s*scale:\s(\d+)";
		private const String VOLTAGE = @"\s*voltage:\s(\d+)";
		private const String TYPE = @"\s*technology:\s(.*?)$";
		private const String TEMP = @"\s*temperature:\s(\d+)";

		private const RegexOptions REOPTIONS = RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace;


		/// <summary>
		/// Gets or sets the battery info.
		/// </summary>
		/// <value>
		/// The battery info.
		/// </value>
		public BatteryInfo BatteryInfo { get; private set; }
		
		/// <summary>
		/// Processes the new lines.
		/// </summary>
		/// <param name="lines">The lines.</param>
		protected override void ProcessNewLines ( string[] lines ) {
			BatteryInfo = new BatteryInfo ( );
			foreach ( var line in lines ) {
				var match = line.Match( BATTERY_LEVEL, REOPTIONS );
				if ( match.Success ) {
					try {
						BatteryInfo.Level = Int32.Parse ( match.Groups[1].Value );
					} catch ( FormatException ) {
						Log.w ( TAG, String.Format ( "Failed to parse {0} as an integer", match.Groups[1].Value ) );
					}
				}

				match = line.Match ( SCALE, REOPTIONS );
				if ( match.Success ) {
					try {
						BatteryInfo.Scale = Int32.Parse ( match.Groups[1].Value );
					} catch ( FormatException ) {
						Log.w ( TAG, String.Format ( "Failed to parse {0} as an integer", match.Groups[1].Value ) );
					}
				}

				match = line.Match ( AC_POWER, REOPTIONS );
				if ( match.Success ) {
					BatteryInfo.ACPowered = String.Compare ( match.Groups[1].Value, bool.TrueString, true ) == 0;
				}

				match = line.Match ( USB_POWER, REOPTIONS );
				if ( match.Success ) {
					BatteryInfo.UsbPowered = String.Compare ( match.Groups[1].Value, bool.TrueString, true ) == 0;
				}

				match = line.Match ( PRESENT, REOPTIONS );
				if ( match.Success ) {
					BatteryInfo.Present = String.Compare ( match.Groups[1].Value, bool.TrueString, true ) == 0;
				}

				match = line.Match ( STATUS, REOPTIONS );
				if ( match.Success ) {
					try {
						var i = Int32.Parse ( match.Groups[1].Value );
						BatteryInfo.Status = i.ToEnum<BatteryInfo.StatusTypes> ( );
					} catch ( FormatException ) {
						Log.w ( TAG, String.Format ( "Failed to parse {0} as an integer", match.Groups[1].Value ) );
					}
				}

				match = line.Match ( HEALTH, REOPTIONS );
				if ( match.Success ) {
					try {
						var i = Int32.Parse ( match.Groups[1].Value );
						BatteryInfo.Health = i.ToEnum<BatteryInfo.HealthTypes> ( );
					} catch ( FormatException ) {
						Log.w ( TAG, String.Format ( "Failed to parse {0} as an integer", match.Groups[1].Value ) );
					}
				}

				match = line.Match ( VOLTAGE, REOPTIONS );
				if ( match.Success ) {
					try {
						BatteryInfo.Voltage = Int32.Parse ( match.Groups[1].Value );
					} catch ( FormatException ) {
						Log.w ( TAG, String.Format ( "Failed to parse {0} as an integer", match.Groups[1].Value ) );
					}
				}

				match = line.Match ( TYPE, REOPTIONS );
				if ( match.Success ) {
					BatteryInfo.Type = match.Groups[1].Value;
				}

				match = line.Match ( TEMP, REOPTIONS );
				if ( match.Success ) {
					try {
						BatteryInfo.Temperature = Int32.Parse ( match.Groups[1].Value );
					} catch ( FormatException ) {
						Log.w ( TAG, String.Format ( "Failed to parse {0} as an integer", match.Groups[1].Value ) );
					}
				}
			}
		}
	}
}
