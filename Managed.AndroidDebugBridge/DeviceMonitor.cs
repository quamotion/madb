using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace Managed.Adb {
	/// <summary>
	/// A Device monitor. This connects to the Android Debug Bridge and get device and
	/// debuggable process information from it.
	/// </summary>
	public class DeviceMonitor {
		private const String TAG = "DeviceMonitor";

		private byte[] LengthBuffer = null;
		private byte[] LengthBuffer2 = null;


		public DeviceMonitor ( AndroidDebugBridge bridge ) {
			Devices = new List<Device> ( );
			DebuggerPorts = new List<int> ( );
			ClientsToReopen = new Dictionary<IClient, int> ( );
			DebuggerPorts.Add ( DdmPreferences.DebugPortBase );
			LengthBuffer = new byte[4];
			LengthBuffer2 = new byte[4];

		}

		public List<Device> Devices { get; private set; }
		public List<int> DebuggerPorts { get; private set; }
		public Dictionary<IClient, int> ClientsToReopen { get; private set; }
		public AndroidDebugBridge Server { get; private set; }
		public bool IsMonitoring { get; private set; }
		public bool IsRunning { get; private set; }
		public int ConnectionAttemptCount { get; private set; }
		public int RestartAttemptCount { get; private set; }
		public bool HasInitialDeviceList { get; private set; }
		private Socket MainAdbConnection { get; set; }

		public void AddClientToDropAndReopen ( IClient client, int port ) {
			lock ( ClientsToReopen ) {
				Log.d ( TAG, "Adding {0} to list of client to reopen ({1})", client, port );
				if ( !ClientsToReopen.ContainsKey ( client ) ) {
					ClientsToReopen.Add ( client, port );
				}
			}
		}


		public void Start ( ) {

		}

		public void Stop ( ) {

		}

		/// <summary>
		/// Monitors the devices. This connects to the Debug Bridge
		/// </summary>
		private void DeviceMonitorLoop ( ) {
			do {
				try {
					if ( MainAdbConnection == null ) {
						Log.d ( TAG, "Opening adb connection" );
						MainAdbConnection = OpenAdbConnection ( );
						if ( MainAdbConnection == null ) {
							ConnectionAttemptCount++;
							Log.e ( TAG, "Connection attempts: " + ConnectionAttemptCount );
							if ( ConnectionAttemptCount > 10 ) {
								if ( Server.Start ( ) == false ) {
									RestartAttemptCount++;
									Log.e ( TAG, "adb restart attempts: {0}", RestartAttemptCount );
								} else {
									RestartAttemptCount = 0;
								}
							}
							WaitBeforeContinue ( );
						} else {
							Log.d ( TAG, "Connected to adb for device monitoring" );
							ConnectionAttemptCount = 0;
						}
					}

					if ( MainAdbConnection != null && !IsMonitoring ) {
						IsMonitoring = SendDeviceListMonitoringRequest ( );
					}
					if ( IsMonitoring ) {
						// read the length of the incoming message
						int length = ReadLength ( MainAdbConnection, LengthBuffer );

						if ( length >= 0 ) {
							// read the incoming message
							ProcessIncomingDeviceData ( length );

							// flag the fact that we have build the list at least once.
							HasInitialDeviceList = true;
						}
					}
				} catch ( IOException ioe ) {
					if ( !IsRunning ) {
						Log.e ( TAG, "Adb connection Error: ", ioe );
						IsMonitoring = false;
						if ( MainAdbConnection != null ) {
							try {
								MainAdbConnection.Close ( );
							} catch ( IOException ) {
								// we can safely ignore that one.
							}
							MainAdbConnection = null;
						}
					}
				}
			} while ( IsRunning );
		}

		/// <summary>
		/// Waits before continuing.
		/// </summary>
		private void WaitBeforeContinue ( ) {
			Thread.Sleep ( 1000 );
		}

		/// <summary>
		/// Sends the device list monitoring request.
		/// </summary>
		/// <returns></returns>
		private bool SendDeviceListMonitoringRequest ( ) {
			byte[] request = AdbHelper.Instance.FormAdbRequest ( "host:track-devices" );

			if ( AdbHelper.Instance.Write ( MainAdbConnection, request ) == false ) {
				Log.e ( TAG, "Sending Tracking request failed!" );
				MainAdbConnection.Close ( );
				throw new IOException ( "Sending Tracking request failed!" );
			}

			AdbResponse resp = AdbHelper.Instance.ReadAdbResponse ( MainAdbConnection, false /* readDiagString */);

			if ( !resp.IOSuccess ) {
				Log.e ( TAG, "Failed to read the adb response!" );
				MainAdbConnection.Close ( );
				throw new IOException ( "Failed to read the adb response!" );
			}

			if ( !resp.Okay ) {
				// request was refused by adb!
				Log.e ( TAG, "adb refused request: {0}", resp.Message );
			}

			return resp.Okay;
		}

		/// <summary>
		/// Reads the length.
		/// </summary>
		/// <param name="MainAdbConnection">The main adb connection.</param>
		/// <param name="LengthBuffer">The length buffer.</param>
		/// <returns></returns>
		private int ReadLength ( Socket MainAdbConnection, byte[] LengthBuffer ) {
			throw new NotImplementedException ( );
		}

		/// <summary>
		/// Processes the incoming device data.
		/// </summary>
		/// <param name="length">The length.</param>
		private void ProcessIncomingDeviceData ( int length ) {
			List<IDevice> list = new List<IDevice> ( );

			if ( length > 0 ) {
				byte[] buffer = new byte[length];
				String result = Read ( MainAdbConnection, buffer );

				String[] devices = result.Split ( new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries );

				foreach ( String d in devices ) {
					try {
						Device device = Device.CreateFromAdbData ( d );
						if ( device != null ) {
							list.Add ( device );
						}
					} catch ( ArgumentException ae ) {
						Log.e ( TAG, ae );
					}
				}

			}

			// now merge the new devices with the old ones.
			UpdateDevices ( list );
		}

		private void UpdateDevices ( List<Device> list ) {
			// because we are going to call mServer.deviceDisconnected which will acquire this lock
			// we lock it first, so that the AndroidDebugBridge lock is always locked first.
			lock ( AndroidDebugBridge.GetLock ( ) ) {
				lock ( Devices ) {
					// For each device in the current list, we look for a matching the new list.
					// * if we find it, we update the current object with whatever new information
					//   there is
					//   (mostly state change, if the device becomes ready, we query for build info).
					//   We also remove the device from the new list to mark it as "processed"
					// * if we do not find it, we remove it from the current list.
					// Once this is done, the new list contains device we aren't monitoring yet, so we
					// add them to the list, and start monitoring them.

					for ( int d = 0; d < Devices.Count; ) {
						Device device = Devices[d];

						// look for a similar device in the new list.
						int count = list.Count;
						bool foundMatch = false;
						for ( int dd = 0; dd < count; dd++ ) {
							Device newDevice = list[dd];
							// see if it matches in id and serial number.
							if ( String.Compare ( newDevice.SerialNumber, device.SerialNumber, true ) == 0 ) {
								foundMatch = true;

								// update the state if needed.
								if ( device.State != newDevice.State ) {
									device.State = newDevice.State;
									device.OnStateChanged ( EventArgs.Empty );

									// if the device just got ready/online, we need to start
									// monitoring it.
									if ( device.IsOnline ) {
										if ( AndroidDebugBridge.ClientSupport ) {
											if ( StartMonitoringDevice ( device ) == false ) {
												Log.e ( TAG, "Failed to start monitoring {0}", device.SerialNumber );
											}
										}

										if ( device.Properties.Count == 0 ) {
											QueryNewDeviceForInfo ( device );
										}
									}
								}

								// remove the new device from the list since it's been used
								list.RemoveAt ( dd );
								break;
							}
						}

						if ( foundMatch == false ) {
							// the device is gone, we need to remove it, and keep current index
							// to process the next one.
							RemoveDevice ( device );
							Server.OnDeviceDisconnected ( new DeviceEventArgs ( newDevice ) );
						} else {
							// process the next one
							d++;
						}
					}

					// at this point we should still have some new devices in newList, so we
					// process them.
					foreach ( Device newDevice in list ) {
						// add them to the list
						Devices.Add ( newDevice );
						Server.OnDeviceConnected ( new DeviceEventArgs ( newDevice ) );

						// start monitoring them.
						if ( AndroidDebugBridge.ClientSupport == true ) {
							if ( newDevice.IsOnline ) {
								StartMonitoringDevice ( newDevice );
							}
						}

						// look for their build info.
						if ( newDevice.IsOnline ) {
							QueryNewDeviceForInfo ( newDevice );
						}
					}
				}
			}
			list.Clear ( );
		}

		/// <summary>
		/// Removes the device.
		/// </summary>
		/// <param name="device">The device.</param>
		private void RemoveDevice ( Device device ) {
			//device.Clients.Clear ( );
			Devices.Remove ( device );

			Socket channel = device.ClientMonitoringSocket;
			if ( channel != null ) {
				try {
					channel.Close ( );
				} catch ( IOException e ) {
					// doesn't really matter if the close fails.
				}
			}
		}

		private void QueryNewDeviceForInfo ( Device device ) {
			// TODO: do this in a separate thread.
			try {
				// first get the list of properties.
				device.ExecuteShellCommand ( GetPropReceiver.GETPROP_COMMAND, new GetPropReceiver ( device ) );

				// get environment variables
				QueryNewDeviceForEnvironmentVariables ( device );
				// instead of getting the 3 hard coded ones, we use mount from busybox and get them all...
				// if that fails, then it automatically falls back to the hard coded ones.
				QueryNewDeviceForMountingPoint ( device );

				// now get the emulator Virtual Device name (if applicable).
				if ( device.IsEmulator ) {
					/*EmulatorConsole console = EmulatorConsole.getConsole ( device );
					if ( console != null ) {
						device.AvdName = console.AvdName;
					}*/
				}
			} catch ( IOException ) {
				// if we can't get the build info, it doesn't matter too much
			}
		}

		private void QueryNewDeviceForEnvironmentVariables ( Device device ) {
			device.ExecuteShellCommand ( EnvironmentVariablesReceiver.ENV_COMMAND, new EnvironmentVariablesReceiver ( device ) );
		}

		private void QueryNewDeviceForMountingPoint ( Device device ) {
			try {
				device.RefreshMountPoints ( );
			} catch ( IOException ) {
				// if we can't get the build info, it doesn't matter too much
			}
		}

		private bool StartMonitoringDevice ( Device device ) {
			Socket socket = OpenAdbConnection ( );

			if ( socket != null ) {
				try {
					bool result = SendDeviceMonitoringRequest ( socket, device );
					if ( result ) {

						if ( Selector == null ) {
							StartDeviceMonitorThread ( );
						}

						device.ClientMonitoringSocket = socket;

						lock ( Devices ) {
							// always wakeup before doing the register. The synchronized block
							// ensure that the selector won't select() before the end of this block.
							// @see deviceClientMonitorLoop
							Selector.wakeup ( );

							socket.Blocking = true;
							//socket.register(mSelector, SelectionKey.OP_READ, device);
						}

						return true;
					}
				} catch ( IOException e ) {
					try {
						// attempt to close the socket if needed.
						socket.Close ( );
					} catch ( IOException e1 ) {
						// we can ignore that one. It may already have been closed.
					}
					Log.d ( TAG, "Connection Failure when starting to monitor device '{0}' : {1}", device, e.Message );
				}
			}

			return false;
		}

		private void StartDeviceMonitorThread() {
        Selector = Selector.Open();
				new Thread ( new ThreadStart ( DeviceClientMonitorLoop ) ).Start ( );
    }

		/// <summary>
		/// Attempts to connect to the debug bridge server.
		/// </summary>
		/// <returns>a connect socket if success, null otherwise</returns>
		private Socket OpenAdbConnection ( ) {
			Log.d ( TAG, "Connecting to adb for Device List Monitoring..." );
			Socket socket = new Socket ( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );
			try {
				socket.Connect ( AndroidDebugBridge.SocketAddress );
				socket.NoDelay = true;
			} catch ( IOException e ) {
				Log.w ( TAG, e );
			}

			return socket;
		}
	}
}
