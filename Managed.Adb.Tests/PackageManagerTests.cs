using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Managed.Adb.Tests {
    [TestClass]
	public class PackageManagerTests {

        [TestMethod]
        [TestCategory("IntegrationTest")]
		public void ExistsTest ( ) {
			Device device = GetFirstDevice ( );

			PackageManager pm = new PackageManager ( device );
			Assert.IsTrue ( pm.Exists ( "com.android.contacts" ) );
			Assert.IsFalse ( pm.Exists ( "foo.bar.package" ) );

		}

        [TestMethod]
        [TestCategory("IntegrationTest")]
		public void RefreshPackagesTest ( ) {
			Device device = GetFirstDevice ( );

			PackageManager pm = new PackageManager ( device );
		    pm.RefreshPackages ( );

			Assert.IsTrue ( pm.Packages.ContainsKey ( "com.android.contacts" ) );
			Assert.IsTrue ( pm.Packages.ContainsKey ( "android" ) ); // the framework
		}


		private Device GetFirstDevice ( ) {
			List<Device> devices = AdbHelper.Instance.GetDevices ( AndroidDebugBridge.SocketAddress );
			Assert.IsTrue ( devices.Count >= 1 );
			return devices[0];
		}
	}
}
