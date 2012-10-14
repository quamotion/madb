using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Managed.Adb {
	public class BatteryInfo {
		public enum StatusTypes {
			Unknown = 1,
			Charging = 2,
			Discharging = 3,
			NotCharging = 4,
			Full = 5
		}

		public enum HealthTypes {
			Unknown = 1,
			Good = 2,
			Overheat = 3,
			Cold = 4, 
			OverVoltage = 5,
			UnspecifiedFailure = 6
		}

		public bool ACPowered { get; set; }
		public bool UsbPowered { get; set; }
		public StatusTypes Status { get; set; }
		public HealthTypes Health { get; set; }
		public bool Present { get; set; }
		public int Level { get; set; }
		public int Scale { get; set; }
		public int Voltage { get; set; }
		public int Temperature { get; set; }
		public String Type { get; set; }

		public int CalculatedLevel {
			get {
				return ( Level * 100 ) / Scale;
			}
		}
	}
}
