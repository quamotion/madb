using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace Managed.Adb.Tests {
	public class PackageManagerTests {

		[Fact]
		public void ExistsTest ( ) {
			Device device = GetFirstDevice ( );

			PackageManager pm = new PackageManager ( device );
			Assert.True ( pm.Exists ( "com.android.contacts" ) );
			Assert.False ( pm.Exists ( "foo.bar.package" ) );

		}

		[Fact]
		public void RefreshPackagesTest ( ) {
			Device device = GetFirstDevice ( );

			PackageManager pm = new PackageManager ( device );
			Assert.DoesNotThrow ( new Assert.ThrowsDelegate ( delegate ( ) {
				pm.RefreshPackages ( );
			} ) );

			Assert.True ( pm.Packages.ContainsKey ( "com.android.contacts" ) );
			Assert.True ( pm.Packages.ContainsKey ( "com.android.gallery" ) );
			Assert.True ( pm.Packages.ContainsKey ( "android" ) ); // the framework
		}


		private Device GetFirstDevice ( ) {
			List<Device> devices = AdbHelper.Instance.GetDevices ( AndroidDebugBridge.SocketAddress );
			Assert.True ( devices.Count >= 1 );
			return devices[0];
		}
	}
}
