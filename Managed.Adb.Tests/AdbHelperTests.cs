using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using System.IO;

namespace Managed.Adb.Tests {
	public class AdbHelperTests {
		[Fact]
		public void GetDevicesTest ( ) {
			List<Device> devices = AdbHelper.Instance.GetDevices ( AndroidDebugBridge.SocketAddress );
			Assert.True ( devices.Count == 1 );
			Assert.Equal<string> ( "HT845GZ51275", devices[0].SerialNumber );
		}

		[Fact]
		public void ExecuteRemoteCommandTest ( ) {
			List<Device> devices = AdbHelper.Instance.GetDevices ( AndroidDebugBridge.SocketAddress );
			Assert.True ( devices.Count >= 1 );
			Device device = devices[0];
			ConsoleReceiver creciever = new ConsoleReceiver ( );

			Assert.DoesNotThrow ( new Assert.ThrowsDelegate ( delegate ( ) {
				AdbHelper.Instance.ExecuteRemoteCommand ( AndroidDebugBridge.SocketAddress, "ls -lF --color=never", device, creciever );
			} ) );
			Assert.DoesNotThrow ( new Assert.ThrowsDelegate ( delegate ( ) {
				AdbHelper.Instance.ExecuteRemoteCommand ( AndroidDebugBridge.SocketAddress, "busybox", device, creciever );
			} ) );

			Assert.Throws<FileNotFoundException> ( new Assert.ThrowsDelegate ( delegate ( ) {
				AdbHelper.Instance.ExecuteRemoteCommand ( AndroidDebugBridge.SocketAddress, "notsobusybox", device, creciever );
			} ) );
		}

		[Fact]
		public void GetRawImageTest ( ) {
			List<Device> devices = AdbHelper.Instance.GetDevices ( AndroidDebugBridge.SocketAddress );
			Assert.True ( devices.Count >= 1 );
			Device device = devices[0];

			RawImage rawImage = AdbHelper.Instance.GetFrameBuffer ( AndroidDebugBridge.SocketAddress, device );

			Assert.NotNull ( rawImage );
			Assert.Equal<int> ( 16, rawImage.Bpp );
			Assert.Equal<int> ( 320, rawImage.Width );
			Assert.Equal<int> ( 480, rawImage.Height );

		}

		public class ConsoleReceiver : MultiLineReceiver {

			public override void ProcessNewLines ( string[] lines ) {
				foreach ( var line in lines ) {
					Console.WriteLine ( line );
				}
			}
		}
	}
}
