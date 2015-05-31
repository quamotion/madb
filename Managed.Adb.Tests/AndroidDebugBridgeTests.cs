using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using MoreLinq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Managed.Adb.Tests {
    [TestClass]
	public class AndroidDebugBridgeTests : BaseDeviceTests {

        [TestMethod]
        [TestCategory("IntegrationTest")]
		public void CreateBridgeTest ( ) {
			try {
				AndroidDebugBridge adb = CreateBridge ( @"d:\android\android-sdk\platform-tools\adb.exe" );
				bool result = adb.Start ( );
				Assert.IsTrue ( result, "Failed to start ADB" );
				
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
