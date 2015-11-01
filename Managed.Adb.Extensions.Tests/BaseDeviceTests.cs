using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Managed.Adb.Tests {
	public class BaseDeviceTests {
		protected Device GetFirstDevice( ) {
			List<DeviceData> devices = AdbHelper.Instance.GetDevices (AdbServer.SocketAddress );
			Assert.IsTrue ( devices.Count >= 1 );
			return new Device(devices[0]);
		}

	}
}
