using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace Managed.Adb.Tests {
	public class FileSystemTests {
		[Fact]
		public void GetDeviceBlocksTest( ) {
			Device d = GetFirstDevice ( );
			List<String> blocks = d.FileSystem.DeviceBlocks;
			Assert.True ( blocks.Count > 0 );
		}

		private Device GetFirstDevice( ) {
			List<Device> devices = AdbHelper.Instance.GetDevices ( AndroidDebugBridge.SocketAddress );
			Assert.True ( devices.Count >= 1 );
			return devices[0];
		}
	}
}
