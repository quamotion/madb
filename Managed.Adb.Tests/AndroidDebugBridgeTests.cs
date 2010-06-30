using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using System.IO;

namespace Managed.Adb.Tests {
	public class AndroidDebugBridgeTests {

		[Fact]
		public void CreateBridgeTest ( ) {
			try {
				AndroidDebugBridge adb = CreateBridge ( @"S:\Android\sdk\tools\adb.exe" );
				adb.DeviceChanged += delegate ( object sender, DeviceEventArgs e ) {
					Console.WriteLine ( "{0} changed", e.Device.SerialNumber );
				};
				adb.DeviceConnected += delegate ( object sender, DeviceEventArgs e ) {
					Console.WriteLine ( "{0} connected", e.Device.SerialNumber );
				};
				adb.DeviceDisconnected += delegate ( object sender, DeviceEventArgs e ) {
					Console.WriteLine ( "{0} disconnected", e.Device.SerialNumber );
				};

				bool result = adb.Start ( );
				Assert.True ( result, "Failed to start ADB" );

				Assert.Throws<FileNotFoundException> ( new Assert.ThrowsDelegate ( delegate ( ) {
					AndroidDebugBridge adb2 = CreateBridge ( @"C:\Bad\Path\Doesnt\Exist\adb.exe" );
				} ) );
			} catch ( Exception ex ) {
				Console.WriteLine ( ex.ToString ( ) );
			}
		}

		void adb_DeviceChanged ( object sender, DeviceEventArgs e ) {
			throw new NotImplementedException ( );
		}

		private AndroidDebugBridge CreateBridge ( String location ) {
			AndroidDebugBridge adb = AndroidDebugBridge.CreateBridge ( location, false );
			return adb;
		}
	}
}
