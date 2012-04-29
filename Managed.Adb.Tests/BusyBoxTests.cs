using System;
using System.IO;
using Xunit;

namespace Managed.Adb.Tests {
	public class BusyBoxTests : BaseDeviceTests {

		[Fact]
		public void GetCommandsTest( ) {
			Device device = GetFirstDevice ( );
			bool avail = device.BusyBox.Available;
			Assert.True ( avail, "BusyBox is not available" );


			foreach ( var item in device.BusyBox.Commands ) {
				Console.Write ( "{0},", item );
			}

			Assert.True ( avail && device.BusyBox.Commands.Count > 0 );
		}

		[Fact]
		public void InstallTest( ) {
			Device device = GetFirstDevice ( );
			bool avail = device.BusyBox.Available;
			if ( !avail ) {
				Assert.DoesNotThrow ( new Assert.ThrowsDelegate ( delegate ( ) {
					bool result = device.BusyBox.Install ( "/sdcard/busybox" );
					Assert.True ( result, "BusyBox Install returned false" );
				} ) );
			}

			device.ExecuteShellCommand ( "printenv", new ConsoleOutputReceiver ( ) );

			Assert.True ( device.BusyBox.Available, "BusyBox is not installed" );
		}

		[Fact]
		public void ExecuteRemoteCommandTest( ) {
			Device device = GetFirstDevice ( );
			ConsoleOutputReceiver creciever = new ConsoleOutputReceiver ( );

			Console.WriteLine ( "Executing 'busybox':" );
			Assert.DoesNotThrow ( new Assert.ThrowsDelegate ( delegate ( ) {
				bool hasBB = false;
				try {
					device.ExecuteShellCommand ( "busybox", creciever );
					hasBB = true;
				} catch ( FileNotFoundException ) {
					hasBB = false;
				} finally {
					Console.WriteLine ( "Busybox enabled: {0}", hasBB );
				}
			} ) );


			Console.WriteLine ( "Executing 'busybox ls': " );
			Assert.DoesNotThrow ( new Assert.ThrowsDelegate ( delegate ( ) {
				try {
					device.BusyBox.ExecuteShellCommand ( "ls", creciever );
				} catch ( Exception  ex) {
					Console.WriteLine ( ex.Message );
					throw;
				} 
			} ) );
		}

	}
}
