using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using System.IO;
using MoreLinq;
namespace Managed.Adb.Tests {
	public class AndroidDebugBridgeTests : BaseDeviceTests {

		[Fact]
		public void CreateBridgeTest ( ) {
			try {
				AndroidDebugBridge adb = CreateBridge ( @"d:\android\android-sdk\platform-tools\adb.exe" );
				bool result = adb.Start ( );
				Assert.True ( result, "Failed to start ADB" );
				
				adb.Devices.ForEach ( d => {
					Console.WriteLine ( "{0}\t{1}", d.SerialNumber, d.State );
				} );

				adb.Stop ( );
			} catch ( Exception ex ) {
				Console.WriteLine ( ex.ToString ( ) );
				throw;
			}
		}

		private AndroidDebugBridge CreateBridge ( String location ) {
			AndroidDebugBridge adb = AndroidDebugBridge.CreateBridge ( location, false );
			return adb;
		}
	}
}
