using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Managed.Adb;
using System.IO;

namespace MadBee.Console {
	class Program {

		public enum Actions {
			Devices,
			Monitor,
			Start_Server,
			Kill_Server,
		}

		static void Main( string[] arguments ) {
			var args = new Arguments ( arguments );
			AndroidDebugBridge bridge = AndroidDebugBridge.CreateBridge(Path.Combine ( Environment.GetEnvironmentVariable ( "ANDROID_SDK_HOME" ), "platform-tools\\adb.exe" ),true);
			foreach ( var item in Enum.GetNames ( typeof ( Actions ) ) ) {
				var actionName = item.Replace ( '_', '-' ).ToLower().Trim();
				if ( args.ContainsKey ( actionName ) ) {
					Actions action = (Actions)Enum.Parse ( typeof ( Actions ), item, true );
					switch ( action ) {
						case Actions.Devices:
							GetDevices ( );
							break;
						case Actions.Monitor:
							bridge.DeviceChanged += delegate ( object sender, DeviceEventArgs e ) {
								System.Console.WriteLine ( "Changed: {0}\t{1}", e.Device.SerialNumber, e.Device.State );
							};
							bridge.DeviceConnected += delegate ( object sender, DeviceEventArgs e ) {
								System.Console.WriteLine ( "{0}\t{1}", e.Device.SerialNumber, e.Device.State );
							};
							bridge.DeviceDisconnected += delegate ( object sender, DeviceEventArgs e ) {
								System.Console.WriteLine ( "{0}\t{1}", e.Device.SerialNumber, e.Device.State );
							};
							/*if ( !bridge.Start ( ) ) {
								System.Console.WriteLine ( "Unable to start Android Debug Bridge" );
								break;
							}*/
							System.Console.ReadLine ( );
							try {
								bridge.Stop ( );
							} catch ( IOException ) {
								// ignore
							}
							break;
						case Actions.Start_Server:
							StartServer ( );
							break;
						case Actions.Kill_Server:
							break;
						default:
							break;
					}
					return;
				}
			}

			PrintUsage ( );

		}

		private static void StartServer( ) {
			
		}

		
		private static void PrintUsage( ) {
			System.Console.WriteLine ( "Print Usage: " );
		}

		private static void GetDevices( ) {
			foreach ( var device in AdbHelper.Instance.GetDevices ( AndroidDebugBridge.SocketAddress ) ) {
				System.Console.WriteLine ( "{0}\t{1}", device.SerialNumber, device.State );
			}
		}


	}
}
