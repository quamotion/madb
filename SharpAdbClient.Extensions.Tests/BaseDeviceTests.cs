using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpAdbClient.Tests {
	public class BaseDeviceTests {
		protected Device GetFirstDevice( ) {
			List<DeviceData> devices = AdbClient.Instance.GetDevices();
			Assert.IsTrue ( devices.Count >= 1 );
			return new Device(devices[0]);
		}

	}
}
