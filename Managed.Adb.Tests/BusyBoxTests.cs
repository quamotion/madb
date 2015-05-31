using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace Managed.Adb.Tests {
    [TestClass]
	public class BusyBoxTests : BaseDeviceTests {

        [TestMethod]
        [TestCategory("IntegrationTest")]
		public void GetCommandsTest( ) {
			Device device = GetFirstDevice ( );
			bool avail = device.BusyBox.Available;
			Assert.IsTrue ( avail, "BusyBox is not available" );


			foreach ( var item in device.BusyBox.Commands ) {
				Console.Write ( "{0},", item );
			}

			Assert.IsTrue ( avail && device.BusyBox.Commands.Count > 0 );
		}

        [TestMethod]
        [TestCategory("IntegrationTest")]
		public void InstallTest( ) {
			Device device = GetFirstDevice ( );
			bool avail = device.BusyBox.Available;
			if ( !avail ) {
				bool result = device.BusyBox.Install ( "/sdcard/busybox" );
				Assert.IsTrue ( result, "BusyBox Install returned false" );
			}

			device.ExecuteShellCommand ( "printenv", new ConsoleOutputReceiver ( ) );

			Assert.IsTrue ( device.BusyBox.Available, "BusyBox is not installed" );
		}

        [TestMethod]
        [TestCategory("IntegrationTest")]
		public void ExecuteRemoteCommandTest( ) {
			Device device = GetFirstDevice ( );
			ConsoleOutputReceiver creciever = new ConsoleOutputReceiver ( );

			Console.WriteLine ( "Executing 'busybox':" );
			bool hasBB = false;
			try {
				device.ExecuteShellCommand ( "busybox", creciever );
				hasBB = true;
			} catch ( FileNotFoundException ) {
				hasBB = false;
			} finally {
				Console.WriteLine ( "Busybox enabled: {0}", hasBB );
			}


			Console.WriteLine ( "Executing 'busybox ls': " );
			try {
				device.BusyBox.ExecuteShellCommand ( "ls", creciever );
			} catch ( Exception  ex) {
				Console.WriteLine ( ex.Message );
				throw;
			} 
		}

	}
}
