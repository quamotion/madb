using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Managed.Adb {
	public enum DeviceChangeMask {
		ChangeBuildInfo,
		ChangeState,
		ChangeClientList
	}

	public class DeviceEventArgs : EventArgs {

		/// <summary>
		/// Initializes a new instance of the <see cref="DeviceEventArgs"/> class.
		/// </summary>
		/// <param name="device">The device.</param>
		public DeviceEventArgs ( IDevice device, DeviceChangeMask changeMask ) {
			this.Device = device;
			this.ChangeMask = changeMask;
		}

		/// <summary>
		/// Gets the change mask.
		/// </summary>
		/// <value>The change mask.</value>
		public DeviceChangeMask ChangeMask { get; private set; }

		/// <summary>
		/// Gets the device.
		/// </summary>
		/// <value>The device.</value>
		public IDevice Device { get; private set; }

	}
}
