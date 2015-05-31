using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Managed.Adb.Tests {
    [TestClass]
	public class DeviceTests : BaseDeviceTests {
        [TestMethod]
        [TestCategory("IntegrationTest")]
		public void CanBackupTest ( ) {
			var device = GetFirstDevice ( );

			Assert.IsTrue ( device.CanBackup ( ) );
		}

        [TestMethod]
        [TestCategory("IntegrationTest")]
		public void BackupTest ( ) {
			var device = GetFirstDevice ( );
			device.Backup ( );
		}
	}
}
