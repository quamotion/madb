using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace Managed.Adb.Tests {
	public class DeviceTests : BaseDeviceTests {
		[Fact]
		public void CanBackupTest ( ) {
			var device = GetFirstDevice ( );

			Assert.True ( device.CanBackup ( ) );
		}

		[Fact]
		public void BackupTest ( ) {
			var device = GetFirstDevice ( );
			device.Backup ( );
		}
	}
}
