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
			Server = bridge;
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

		/// <summary>
		/// Starts the monitoring
		/// </summary>
		public void Start ( ) {
			Thread t = new Thread ( new ThreadStart ( DeviceMonitorLoop ) );
			t.Name = "Device List Monitor";
			t.Start ( );
		}

		/// <summary>
		/// Stops the monitoring
		/// </summary>
		public void Stop ( ) {
			IsRunning = false;

			// wakeup the main loop thread by closing the main connection to adb.
			try {
				if ( MainAdbConnection != null ) {
					MainAdbConnection.Close ( );
				}
			} catch ( IOException ) {
			}

			// wake up the secondary loop by closing the selector.
			/*if ( Selector != null ) {
				Selector.WakeUp ( );
			}*/
		}

		/// <summary>
		/// Monitors the devices. This connects to the Debug Bridge
		/// </summary>
		private void DeviceMonitorLoop ( ) {
			IsRunning = true;
			do {
				try {
					if ( MainAdbConnection == null ) {
						Log.d ( TAG, "Opening adb connection" );
						MainAdbConnection = OpenAdbConnection ( );

						if ( MainAdbConnection == null ) {
							ConnectionAttemptCount++;
							Console.WriteLine ( "Connection attempts: {0}", ConnectionAttemptCount );
							Log.e ( TAG, "Connection attempts: {0}", ConnectionAttemptCount );

							if ( ConnectionAttemptCount > 10 ) {
								if ( Server.Start ( ) == false ) {
									RestartAttemptCount++;
									Console.WriteLine ( "adb restart attempts: {0}", RestartAttemptCount );
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
					//break;
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
					Console.WriteLine ( ioe );
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
				} catch ( Exception ex ) {
					Console.WriteLine ( ex );
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
		/// Processes the incoming device data.
		/// </summary>
		/// <param name="length">The length.</param>
		private void ProcessIncomingDeviceData ( int length ) {
			List<Device> list = new List<Device> ( );

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
							device.State = DeviceState.Offline;
							device.OnStateChanged ( EventArgs.Empty );
							Server.OnDeviceDisconnected ( new DeviceEventArgs ( device ) );
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
						if ( Server != null ) {
							newDevice.State = DeviceState.Online;
							newDevice.OnStateChanged ( EventArgs.Empty );
							Server.OnDeviceConnected ( new DeviceEventArgs ( newDevice ) );
						}

						// start monitoring them.
						if ( AndroidDebugBridge.ClientSupport ) {
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
				} catch ( IOException ) {
					// doesn't really matter if the close fails.
				}
			}
		}

		private void QueryNewDeviceForInfo ( Device device ) {
			// TODO: do this in a separate thread.
			try {
				// first get the list of properties.

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
			try {
				device.RefreshEnvironmentVariables ( );
			} catch ( IOException ) {
				// if we can't get the build info, it doesn't matter too much
			}
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

						/*if ( Selector == null ) {
							StartDeviceMonitorThread ( );
						}*/

						device.ClientMonitoringSocket = socket;

						lock ( Devices ) {
							// always wakeup before doing the register. The synchronized block
							// ensure that the selector won't select() before the end of this block.
							// @see deviceClientMonitorLoop
							//Selector.wakeup ( );

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

		private void StartDeviceMonitorThread ( ) {
			//Selector = Selector.Open();
			Thread t = new Thread ( new ThreadStart ( DeviceClientMonitorLoop ) );
			t.Name = "Device Client Monitor";
			t.Start ( );
		}

		private void DeviceClientMonitorLoop ( ) {
			do {
				try {
					// This synchronized block stops us from doing the select() if a new
					// Device is being added.
					// @see startMonitoringDevice()
					lock ( Devices ) {
					}

					//int count = Selector.Select ( );
					int count = 0;

					if ( !IsRunning ) {
						return;
					}

					lock ( ClientsToReopen ) {
						if ( ClientsToReopen.Count > 0 ) {
							Dictionary<IClient, int>.KeyCollection clients = ClientsToReopen.Keys;
							MonitorThread monitorThread = MonitorThread.Instance;

							foreach ( IClient client in clients ) {
								Device device = client.DeviceImplementation;
								int pid = client.ClientData.Pid;

								monitorThread.DropClient ( client, false /* notify */);

								// This is kinda bad, but if we don't wait a bit, the client
								// will never answer the second handshake!
								WaitBeforeContinue ( );

								int port = ClientsToReopen[client];

								if ( port == DebugPortManager.NO_STATIC_PORT ) {
									port = GetNextDebuggerPort ( );
								}
								Log.d ( "DeviceMonitor", "Reopening " + client );
								OpenClient ( device, pid, port, monitorThread );
								device.OnClientListChanged ( EventArgs.Empty );
							}

							ClientsToReopen.Clear ( );
						}
					}

					if ( count == 0 ) {
						continue;
					}

					/*List<SelectionKey> keys = Selector.selectedKeys();
					List<SelectionKey>.Enumerator iter = keys.GetEnumerator();

					while (iter.MoveNext()) {
							SelectionKey key = iter.next();
							iter.remove();

							if (key.isValid() && key.isReadable()) {
									Object attachment = key.attachment();

									if (attachment instanceof Device) {
											Device device = (Device)attachment;

											SocketChannel socket = device.getClientMonitoringSocket();

											if (socket != null) {
													try {
															int length = readLength(socket, mLengthBuffer2);

															processIncomingJdwpData(device, socket, length);
													} catch (IOException ioe) {
															Log.d("DeviceMonitor",
																			"Error reading jdwp list: " + ioe.getMessage());
															socket.close();

															// restart the monitoring of that device
															synchronized (mDevices) {
																	if (mDevices.contains(device)) {
																			Log.d("DeviceMonitor",
																							"Restarting monitoring service for " + device);
																			startMonitoringDevice(device);
																	}
															}
													}
											}
									}
							}
					}*/
				} catch ( IOException e ) {
					if ( !IsRunning ) {

					}
				}

			} while ( !IsRunning );
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="socket"></param>
		/// <param name="device"></param>
		/// <returns></returns>
		private bool SendDeviceMonitoringRequest ( Socket socket, Device device ) {
			AdbHelper.Instance.SetDevice ( socket, device );
			byte[] request = AdbHelper.Instance.FormAdbRequest ( "track-jdwp" );
			if ( !AdbHelper.Instance.Write ( socket, request ) ) {
				Log.e ( TAG, "Sending jdwp tracking request failed!" );
				socket.Close ( );
				throw new IOException ( );
			}
			AdbResponse resp = AdbHelper.Instance.ReadAdbResponse ( socket, false /* readDiagString */);
			if ( resp.IOSuccess == false ) {
				Log.e ( TAG, "Failed to read the adb response!" );
				socket.Close ( );
				throw new IOException ( );
			}

			if ( resp.Okay == false ) {
				// request was refused by adb!
				Log.e ( TAG, "adb refused request: " + resp.Message );
			}

			return resp.Okay;
		}

		private void OpenClient ( Device device, int pid, int port, MonitorThread monitorThread ) {

			Socket clientSocket;
			try {
				clientSocket = AdbHelper.Instance.CreatePassThroughConnection ( AndroidDebugBridge.SocketAddress, device, pid );

				clientSocket.Blocking = true;
			} catch ( IOException ioe ) {
				Log.w ( TAG, "Failed to connect to client {0}: {1}'", pid, ioe.Message );
				return;
			}

			CreateClient ( device, pid, clientSocket, port, monitorThread );
		}

		private void CreateClient ( Device device, int pid, Socket socket, int debuggerPort, MonitorThread monitorThread ) {

			/*
			 * Successfully connected to something. Create a Client object, add
			 * it to the list, and initiate the JDWP handshake.
			 */

			Client client = new Client ( device, socket, pid );

			if ( client.SendHandshake ( ) ) {
				try {
					if ( AndroidDebugBridge.ClientSupport ) {
						client.ListenForDebugger ( debuggerPort );
					}
				} catch ( IOException ) {
					client.ClientData.DebuggerConnectionStatus = Managed.Adb.ClientData.DebuggerStatus.ERROR;
					Log.e ( "ddms", "Can't bind to local {0} for debugger", debuggerPort );
					// oh well
				}

				client.RequestAllocationStatus ( );
			} else {
				Log.e ( "ddms", "Handshake with {0} failed!", client );
				/*
				 * The handshake send failed. We could remove it now, but if the
				 * failure is "permanent" we'll just keep banging on it and
				 * getting the same result. Keep it in the list with its "error"
				 * state so we don't try to reopen it.
				 */
			}

			if ( client.IsValid ) {
				device.Clients.Add ( client );
				monitorThread.Clients.Add ( client );
			} else {
				client = null;
			}
		}

		private int GetNextDebuggerPort ( ) {
			// get the first port and remove it
			lock ( DebuggerPorts ) {
				if ( DebuggerPorts.Count > 0 ) {
					int port = DebuggerPorts[0];

					// remove it.
					DebuggerPorts.RemoveAt ( 0 );

					// if there's nothing left, add the next port to the list
					if ( DebuggerPorts.Count == 0 ) {
						DebuggerPorts.Add ( port + 1 );
					}

					return port;
				}
			}

			return -1;
		}

		public void AddPortToAvailableList ( int port ) {
			if ( port > 0 ) {
				lock ( DebuggerPorts ) {
					// because there could be case where clients are closed twice, we have to make
					// sure the port number is not already in the list.
					if ( DebuggerPorts.IndexOf ( port ) == -1 ) {
						// add the port to the list while keeping it sorted. It's not like there's
						// going to be tons of objects so we do it linearly.
						int count = DebuggerPorts.Count;
						for ( int i = 0; i < count; i++ ) {
							if ( port < DebuggerPorts[i] ) {
								DebuggerPorts.Insert ( i, port );
								break;
							}
						}
						// TODO: check if we can compact the end of the list.
					}
				}
			}
		}

		/// <summary>
		/// Reads the length of the next message from a socket.
		/// </summary>
		/// <param name="socket">The Socket to read from.</param>
		/// <param name="buffer"></param>
		/// <returns>the length, or 0 (zero) if no data is available from the socket.</returns>
		private int ReadLength ( Socket socket, byte[] buffer ) {
			String msg = Read ( socket, buffer );
			if ( msg != null ) {
				try {
					int len = int.Parse ( msg, System.Globalization.NumberStyles.HexNumber );
					return len;
				} catch ( FormatException nfe ) {
					// we'll throw an exception below.
				}
			}
			// we receive something we can't read. It's better to reset the connection at this point.
			return 0;
		}

		private String Read ( Socket socket, byte[] data ) {
			int count = -1;
			int totalRead = 0;

			while ( count != 0 && totalRead < data.Length ) {
				try {
					int left = data.Length - totalRead;
					int buflen = left < socket.ReceiveBufferSize ? left : socket.ReceiveBufferSize;

					byte[] buffer = new byte[buflen];
					socket.ReceiveBufferSize = buffer.Length;
					count = socket.Receive ( buffer, buflen, SocketFlags.None );
					if ( count < 0 ) {
						throw new IOException ( "EOF" );
					} else if ( count == 0 ) {
					} else {
						Array.Copy ( buffer, 0, data, totalRead, count );
						totalRead += count;
					}
				} catch ( SocketException sex ) {
					if ( sex.Message.Contains ( "connection was aborted" ) ) {
						// ignore this?
						return String.Empty;
					} else {
						throw new IOException ( String.Format ( "No Data to read: {0}", sex.Message ) );
					}
				}
			}

			return data.GetString ( AdbHelper.DEFAULT_ENCODING );
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
