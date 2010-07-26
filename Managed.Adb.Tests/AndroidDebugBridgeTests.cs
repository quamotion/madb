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
				bool result = adb.Start ( );
				Assert.True ( result, "Failed to start ADB" );

				foreach ( var device in adb.Devices ) {
					Console.WriteLine ( "{0}\t{1}", device.SerialNumber, device.State );
				}

				adb.Stop ( );
			} catch ( Exception ex ) {
				Console.WriteLine ( ex.ToString ( ) );
				throw;
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
