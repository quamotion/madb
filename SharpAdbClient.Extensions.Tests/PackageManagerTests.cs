using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpAdbClient.DeviceCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpAdbClient.Tests {
    [TestClass]
	public class PackageManagerTests {

        [TestMethod]
        [TestCategory("IntegrationTest")]
		public void ExistsTest ( ) {
			DeviceData device = GetFirstDevice ( );

			PackageManager pm = new PackageManager ( device );
			Assert.IsTrue ( pm.Exists ( "com.android.contacts" ) );
			Assert.IsFalse ( pm.Exists ( "foo.bar.package" ) );

		}

        [TestMethod]
        [TestCategory("IntegrationTest")]
		public void RefreshPackagesTest ( ) {
			DeviceData device = GetFirstDevice ( );

			PackageManager pm = new PackageManager ( device );
		    pm.RefreshPackages ( );

			Assert.IsTrue ( pm.Packages.ContainsKey ( "com.android.contacts" ) );
			Assert.IsTrue ( pm.Packages.ContainsKey ( "android" ) ); // the framework
		}


		private DeviceData GetFirstDevice ( ) {
			List<DeviceData> devices = AdbClient.Instance.GetDevices();
			Assert.IsTrue ( devices.Count >= 1 );
			return devices[0];
		}
	}
}
