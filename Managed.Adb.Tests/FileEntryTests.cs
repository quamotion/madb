using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace Managed.Adb.Tests {
	public class FileEntryTests {

		[Fact]
		public void FindEntryTest ( ) {
			Device device = GetFirstDevice ( );

			FileEntry fe = FileEntry.Find ( device, "/system/" );

			fe = FileEntry.Find ( device, "/system/bin/" );
		}

		private Device GetFirstDevice ( ) {
			List<Device> devices = AdbHelper.Instance.GetDevices ( AndroidDebugBridge.SocketAddress );
			Assert.True ( devices.Count >= 1 );
			return devices[0];
		}
	}
}
