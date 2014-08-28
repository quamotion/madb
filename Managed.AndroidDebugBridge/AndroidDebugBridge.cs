using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Text.RegularExpressions;
using System.IO;
using System.Diagnostics;
using System.Threading;

namespace Managed.Adb {
	/// <summary>
	/// The android debug bridge
	/// </summary>
	public sealed class AndroidDebugBridge {

		/// <summary>
		/// Occurs when [bridge changed].
		/// </summary>
		/// <ignore>true</ignore>
		public event EventHandler<AndroidDebugBridgeEventArgs> BridgeChanged;
		/// <summary>
		/// Occurs when [device changed].
		/// </summary>
		/// <ignore>true</ignore>
		public event EventHandler<DeviceEventArgs> DeviceChanged;
		/// <summary>
		/// Occurs when [device connected].
		/// </summary>
		/// <ignore>true</ignore>
		public event EventHandler<DeviceEventArgs> DeviceConnected;
		/// <summary>
		/// Occurs when [device disconnected].
		/// </summary>
		/// <ignore>true</ignore>
		public event EventHandler<DeviceEventArgs> DeviceDisconnected;
		/// <summary>
		/// Occurs when [client changed].
		/// </summary>
		/// <ignore>true</ignore>
		public event EventHandler<ClientEventArgs> ClientChanged;

		/*
		 * Minimum and maximum version of adb supported. This correspond to
		 * ADB_SERVER_VERSION found in //device/tools/adb/adb.h
		 */

		/// <summary>
		/// 
		/// </summary>
		private const int ADB_VERSION_MICRO_MIN = 20;
		/// <summary>
		/// 
		/// </summary>
		private const int ADB_VERSION_MICRO_MAX = -1;


		/// <summary>
		/// The regex pattern for getting the adb version
		/// </summary>
		private const String ADB_VERSION_PATTERN = "^.*(\\d+)\\.(\\d+)\\.(\\d+)$";

#if LINUX
		/// <summary>
		/// The ADB executive
		/// </summary>
		public const String ADB = "adb";
		/// <summary>
		/// The DDMS executive
		/// </summary>
		public const String DDMS = "monitor";
		/// <summary>
		/// The hierarchy viewer
		/// </summary>
		public const String HIERARCHYVIEWER = "hierarchyviewer";
		/// <summary>
		/// The AAPT executive
		/// </summary>
		public const String AAPT = "aapt";
#else
		/// <summary>
		/// The ADB executive
		/// </summary>
		public const String ADB = "adb.exe";
		/// <summary>
		/// The DDMS executive
		/// </summary>
		public const String DDMS = "monitor.bat";
		/// <summary>
		/// The hierarchy viewer
		/// </summary>
		public const String HIERARCHYVIEWER = "hierarchyviewer.bat";
		/// <summary>
		/// The AAPT executive
		/// </summary>
		public const String AAPT = "aapt.exe";

#endif


		// Where to find the ADB bridge.
		/// <summary>
		/// The default ADB bridge port
		/// </summary>
		public const int ADB_PORT = 5037;


		#region statics
		/// <summary>
		/// 
		/// </summary>
		private static AndroidDebugBridge _instance;
		/// <summary>
		/// 
		/// </summary>
		private static bool _clientSupport;

		/// <summary>
		/// Gets or sets the socket address.
		/// </summary>
		/// <value>The socket address.</value>
		public static IPEndPoint SocketAddress { get; private set; }
		/// <summary>
		/// Gets or sets the host address.
		/// </summary>
		/// <value>The host address.</value>
		public static IPAddress HostAddress { get; private set; }

		/// <summary>
		/// Initializes the <see cref="AndroidDebugBridge"/> class.
		/// </summary>
		static AndroidDebugBridge ( ) {
			// built-in local address/port for ADB.
			try {
				HostAddress = IPAddress.Loopback;

				SocketAddress = new IPEndPoint ( HostAddress, ADB_PORT );
			} catch ( ArgumentOutOfRangeException ) {

			}
		}

		/*
		 * Initializes the <code>ddm</code> library.
		 * <p/>This must be called once <b>before</b> any call to
		 * {@link #createBridge(String, boolean)}.
		 * <p>The library can be initialized in 2 ways:
		 * <ul>
		 * <li>Mode 1: <var>clientSupport</var> == <code>true</code>.<br>The library monitors the
		 * devices and the applications running on them. It will connect to each application, as a
		 * debugger of sort, to be able to interact with them through JDWP packets.</li>
		 * <li>Mode 2: <var>clientSupport</var> == <code>false</code>.<br>The library only monitors
		 * devices. The applications are left untouched, letting other tools built on
		 * <code>ddmlib</code> to connect a debugger to them.</li>
		 * </ul>
		 * <p/><b>Only one tool can run in mode 1 at the same time.</b>
		 * <p/>Note that mode 1 does not prevent debugging of applications running on devices. Mode 1
		 * lets debuggers connect to <code>ddmlib</code> which acts as a proxy between the debuggers and
		 * the applications to debug. See {@link Client#getDebuggerListenPort()}.
		 * <p/>The preferences of <code>ddmlib</code> should also be initialized with whatever default
		 * values were changed from the default values.
		 * <p/>When the application quits, {@link #terminate()} should be called.
		 * @param clientSupport Indicates whether the library should enable the monitoring and
		 * interaction with applications running on the devices.
		 * @see AndroidDebugBridge#createBridge(String, boolean)
		 * @see DdmPreferences
		 */
		/// <summary>
		/// Initializes the <code>ddm</code> library.
		/// <para>This must be called once <b>before</b> any call to CreateBridge.</para>
		/// </summary>
		/// <param name="clientSupport">if set to <c>true</c> [client support].</param>
		public static void Initialize ( bool clientSupport ) {
			ClientSupport = clientSupport;

			/*MonitorThread monitorThread = MonitorThread.createInstance ( );
			monitorThread.start ( );

			HandleHello.register ( monitorThread );
			HandleAppName.register ( monitorThread );
			HandleTest.register ( monitorThread );
			HandleThread.register ( monitorThread );
			HandleHeap.register ( monitorThread );
			HandleWait.register ( monitorThread );
			HandleProfiling.register ( monitorThread );*/
		}

		/// <summary>
		/// Terminates the ddm library. This must be called upon application termination.
		/// </summary>
		public static void Terminate ( ) {
			// kill the monitoring services
			if ( Instance != null && Instance.DeviceMonitor != null ) {
				Instance.DeviceMonitor.Stop ( );
				Instance.DeviceMonitor = null;
			}

			/*MonitorThread monitorThread = MonitorThread.getInstance ( );
			if ( monitorThread != null ) {
				monitorThread.quit ( );
			}*/
		}


		/// <summary>
		/// Gets an instance of <see cref="AndroidDebugBridge"/>.
		/// </summary>
		/// <value>The instance.</value>
		public static AndroidDebugBridge Instance {
			get {
				if ( _instance == null ) {
					_instance = CreateBridge ( );
				}
				return _instance;
			}
		}

		/// <summary>
		/// Gets an instance of <see cref="AndroidDebugBridge"/>.
		/// </summary>
		public static AndroidDebugBridge Bridge {
			get { return Instance; }
		}

		/// <summary>
		/// Gets a value indicating whether there is client support.
		/// </summary>
		/// <value>
		///   <c>true</c> if there is client support; otherwise, <c>false</c>.
		/// </value>
		public static bool ClientSupport { get; private set; }



		/// <summary>
		/// Creates a {@link AndroidDebugBridge} that is not linked to any particular executable.
		/// This bridge will expect adb to be running. It will not be able to start/stop/restart</summary>
		/// adb.
		/// If a bridge has already been started, it is directly returned with no changes
		/// <returns></returns>
		public static AndroidDebugBridge CreateBridge ( ) {
			if ( _instance != null ) {
				return _instance;
			}

			try {
				_instance = new AndroidDebugBridge ( );
				_instance.Start ( );
				_instance.OnBridgeChanged ( new AndroidDebugBridgeEventArgs ( _instance ) );
			} catch ( ArgumentException ) {
				_instance.OnBridgeChanged ( new AndroidDebugBridgeEventArgs ( null ) );
				_instance = null;
			}

			return _instance;
		}

		/// <summary>
		/// Creates a new debug bridge from the location of the command line tool.
		/// </summary>
		/// <param name="osLocation">the location of the command line tool 'adb'</param>
		/// <param name="forceNewBridge">force creation of a new bridge even if one with the same location
		/// already exists.</param>
		/// <returns>a connected bridge.</returns>
		/// <remarks>Any existing server will be disconnected, unless the location is the same and
		/// <code>forceNewBridge</code> is set to false.
		/// </remarks>
		public static AndroidDebugBridge CreateBridge ( String osLocation, bool forceNewBridge ) {

			if ( _instance != null ) {
				if ( !String.IsNullOrEmpty ( AdbOsLocation ) && string.Compare ( AdbOsLocation, osLocation, true ) == 0 && !forceNewBridge ) {
					return _instance;
				} else {
					// stop the current server
					Console.WriteLine ( "Stopping Current Instance" );
					_instance.Stop ( );
				}
			}

			try {
				_instance = new AndroidDebugBridge ( osLocation );
				_instance.Start ( );
				_instance.OnBridgeChanged ( new AndroidDebugBridgeEventArgs ( _instance ) );
			} catch ( ArgumentException ) {
				_instance.OnBridgeChanged ( new AndroidDebugBridgeEventArgs ( null ) );
				_instance = null;
			}

			return _instance;
		}

		/// <summary>
		/// Disconnects the current debug bridge, and destroy the object.
		/// </summary>
		/// <remarks>This also stops the current adb host server.</remarks>
		public static void DisconnectBridge ( ) {
			if ( _instance != null ) {
				_instance.Stop ( );

				_instance.OnBridgeChanged ( new AndroidDebugBridgeEventArgs ( null ) );
				_instance = null;
			}
		}

		/// <summary>
		/// Gets the lock.
		/// </summary>
		/// <returns></returns>
		public static object GetLock ( ) {
			return Instance;
		}

		#endregion

		#region constructors

		/// <summary>
		/// Creates a new bridge.
		/// </summary>
		/// <param name="osLocation">the location of the command line tool</param>
		/// <exception cref="ArgumentException"></exception>
		/// <exception cref="FileNotFoundException"></exception>
		private AndroidDebugBridge ( String osLocation ) {
			if ( string.IsNullOrEmpty ( osLocation ) ) {
				throw new ArgumentException ( );
			}

			if ( !File.Exists ( osLocation ) ) {
				Console.WriteLine ( osLocation );
				throw new FileNotFoundException ( "unable to locate adb in the specified location" );
			}

			AdbOsLocation = osLocation;

			CheckAdbVersion ( );
		}

		/// <summary>
		/// Creates a new bridge not linked to any particular adb executable.
		/// </summary>
		private AndroidDebugBridge ( ) {
		}

		#endregion

		#region Event "Raisers"
		/// <summary>
		/// Raises the <see cref="E:BridgeChanged"/> event.
		/// </summary>
		/// <param name="e">The <see cref="Managed.Adb.AndroidDebugBridgeEventArgs"/> instance containing the event data.</param>
		internal void OnBridgeChanged ( AndroidDebugBridgeEventArgs e ) {
			if ( this.BridgeChanged != null ) {
				this.BridgeChanged ( this, e );
			}
		}

		/// <summary>
		/// Raises the <see cref="E:ClientChanged"/> event.
		/// </summary>
		/// <param name="e">The <see cref="Managed.Adb.ClientEventArgs"/> instance containing the event data.</param>
		internal void OnClientChanged ( ClientEventArgs e ) {
			if ( this.ClientChanged != null ) {
				this.ClientChanged ( this, e );
			}
		}

		/// <summary>
		/// Raises the <see cref="E:DeviceChanged"/> event.
		/// </summary>
		/// <param name="e">The <see cref="Managed.Adb.DeviceEventArgs"/> instance containing the event data.</param>
		internal void OnDeviceChanged ( DeviceEventArgs e ) {
			if ( this.DeviceChanged != null ) {
				this.DeviceChanged ( this, e );
			}
		}

		/// <summary>
		/// Raises the <see cref="E:DeviceConnected"/> event.
		/// </summary>
		/// <param name="e">The <see cref="Managed.Adb.DeviceEventArgs"/> instance containing the event data.</param>
		internal void OnDeviceConnected ( DeviceEventArgs e ) {
			if ( this.DeviceConnected != null ) {
				this.DeviceConnected ( this, e );
			}
		}

		/// <summary>
		/// Raises the <see cref="E:DeviceDisconnected"/> event.
		/// </summary>
		/// <param name="e">The <see cref="Managed.Adb.DeviceEventArgs"/> instance containing the event data.</param>
		internal void OnDeviceDisconnected ( DeviceEventArgs e ) {
			if ( this.DeviceDisconnected != null ) {
				this.DeviceDisconnected ( this, e );
			}
		}
		#endregion

		#region public methods
		/// <summary>
		/// Starts the debug bridge.
		/// </summary>
		/// <returns><c>true</c> if success.</returns>
		public bool Start ( ) {
			if ( String.IsNullOrEmpty(AdbOsLocation) || !VersionCheck || !StartAdb ( ) ) {
				return false;
			}

			Started = true;

			// now that the bridge is connected, we start the underlying services.
			DeviceMonitor = new DeviceMonitor ( this );
			DeviceMonitor.Start ( );

			return true;
		}

		/// <summary>
		/// Kills the debug bridge, and the adb host server.
		/// </summary>
		/// <returns><c>true</c> if success.</returns>
		public bool Stop ( ) {
			// if we haven't started we return false;
			if ( !Started ) {
				return false;
			}

			// kill the monitoring services
			if ( DeviceMonitor != null ) {
				DeviceMonitor.Stop ( );
				DeviceMonitor = null;
			}

			if ( !StopAdb ( ) ) {
				return false;
			}

			Started = false;
			return true;
		}

		/// <summary>
		/// Restarts adb, but not the services around it.
		/// </summary>
		/// <returns><c>true</c> if success.</returns>
		public bool Restart ( ) {
			if ( string.IsNullOrEmpty ( AdbOsLocation ) ) {
				Log.e ( ADB, "Cannot restart adb when AndroidDebugBridge is created without the location of adb." );
				return false;
			}

			if ( !VersionCheck ) {
				Log.LogAndDisplay ( LogLevel.Error, ADB, "Attempting to restart adb, but version check failed!" );
				return false;
			}
			lock ( this ) {
				StopAdb ( );

				bool restart = StartAdb ( );

				if ( restart && DeviceMonitor == null ) {
					DeviceMonitor = new DeviceMonitor ( this );
					DeviceMonitor.Start ( );
				}

				return restart;
			}
		}
		#endregion

		#region public properties

		/// <summary>
		/// Gets or Sets the adb location on the OS.
		/// </summary>
		/// <value>The adb location on the OS.</value>
		public static string AdbOsLocation { get; set; }
		/// <summary>
		/// Gets the devices.
		/// </summary>
		/// <value>The devices.</value>
		public IList<Device> Devices {
			get {
				//if ( DeviceMonitor != null ) {
				//	return DeviceMonitor.Devices;
				//}
				//return new List<Device> ( );
				return AdbHelper.Instance.GetDevices(AndroidDebugBridge.SocketAddress);
			}
		}

		/// <summary>
		/// Returns whether the bridge has acquired the initial list from adb after being created.
		/// </summary>
		/// <remarks>
		/// <p/>Calling getDevices() right after createBridge(String, boolean) will
		/// generally result in an empty list. This is due to the internal asynchronous communication
		/// mechanism with <code>adb</code> that does not guarantee that the IDevice list has been
		/// built before the call to getDevices().
		/// <p/>The recommended way to get the list of IDevice objects is to create a
		/// IDeviceChangeListener object.
		/// </remarks>
		/// <returns>
		/// 	<c>true</c> if [has initial device list]; otherwise, <c>false</c>.
		/// </returns>
		public bool HasInitialDeviceList ( ) {
			if ( DeviceMonitor != null ) {
				return DeviceMonitor.HasInitialDeviceList;
			}
			return false;
		}

		/// <summary>
		/// Gets or sets the client to accept debugger connection on the custom "Selected debug port".
		/// </summary>
		/// <remarks>Not Yet Implemented</remarks>
		public IClient SelectedClient {
			get {
				/*MonitorThread monitorThread = MonitorThread.Instance;
				if ( monitorThread != null ) {
					return monitorThread.SelectedClient = selectedClient;
				}*/
				return null;
			}
			set {
				/*MonitorThread monitorThread = MonitorThread.Instance;
				if ( monitorThread != null ) {
					monitorThread.SelectedClient = value;
				}*/
			}
		}
		/// <summary>
		/// Returns whether the AndroidDebugBridge object is still connected to the adb daemon.
		/// </summary>
		/// <value><c>true</c> if this instance is connected; otherwise, <c>false</c>.</value>
		public bool IsConnected {
			get {
				//MonitorThread monitorThread = MonitorThread.Instance;
				if ( DeviceMonitor != null /* && monitorThread != null */ ) {
					return DeviceMonitor.IsMonitoring /* && monitorThread.State != State.TERMINATED*/;
				}
				return false;
			}
		}

		/// <summary>
		/// Returns the number of times the AndroidDebugBridge object attempted to connect
		/// </summary>
		/// <value>The connection attempt count.</value>
		public int ConnectionAttemptCount {
			get {
				if ( DeviceMonitor != null ) {
					return DeviceMonitor.ConnectionAttemptCount;
				}
				return -1;
			}
		}

		/// <summary>
		/// Returns the number of times the AndroidDebugBridge object attempted to restart
		/// the adb daemon.
		/// </summary>
		/// <value>The restart attempt count.</value>
		public int RestartAttemptCount {
			get {
				if ( DeviceMonitor != null ) {
					return DeviceMonitor.RestartAttemptCount;
				}
				return -1;
			}
		}

		/// <summary>
		/// Gets the device monitor
		/// </summary>
		public DeviceMonitor DeviceMonitor { get; private set; }

		/// <summary>
		/// Gets if the adb host has started
		/// </summary>
		private bool Started { get; set; }
		/// <summary>
		/// Gets the result of the version check
		/// </summary>
		private bool VersionCheck { get; set; }
		#endregion

		#region private methods

		/// <summary>
		/// Queries adb for its version number and checks it against #MIN_VERSION_NUMBER and MAX_VERSION_NUMBER
		/// </summary>
		private void CheckAdbVersion ( ) {
			// default is bad check
			VersionCheck = false;

			if ( String.IsNullOrEmpty ( AdbOsLocation ) ) {
				Console.WriteLine ( "AdbOsLocation is Empty" );
				return;
			}

			try {
				Log.d ( DDMS, String.Format ( "Checking '{0} version'", AdbOsLocation ) );

				ProcessStartInfo psi = new ProcessStartInfo ( AdbOsLocation, "version" );
				psi.WindowStyle = ProcessWindowStyle.Hidden;
				psi.CreateNoWindow = true;
				psi.UseShellExecute = false;
				psi.RedirectStandardError = true;
				psi.RedirectStandardOutput = true;

				List<String> errorOutput = new List<String> ( );
				List<String> stdOutput = new List<String> ( );
				using ( Process proc = Process.Start ( psi ) ) {
					int status = GrabProcessOutput ( proc, errorOutput, stdOutput, true /* waitForReaders */);
					if ( status != 0 ) {
						StringBuilder builder = new StringBuilder ( "'adb version' failed!" );
						builder.AppendLine ( string.Empty );
						foreach ( String error in errorOutput ) {
							builder.AppendLine ( error );
						}
						Log.LogAndDisplay ( LogLevel.Error, "adb", builder.ToString ( ) );
					}
				}

				// check both stdout and stderr
				bool versionFound = false;
				foreach ( String line in stdOutput ) {
					versionFound = ScanVersionLine ( line );
					if ( versionFound ) {
						break;
					}
				}

				if ( !versionFound ) {
					foreach ( String line in errorOutput ) {
						versionFound = ScanVersionLine ( line );
						if ( versionFound ) {
							break;
						}
					}
				}

				if ( !versionFound ) {
					// if we get here, we failed to parse the output.
					Log.LogAndDisplay ( LogLevel.Error, ADB, "Failed to parse the output of 'adb version'" ); 
				}

			} catch ( IOException e ) {
				Log.LogAndDisplay ( LogLevel.Error, ADB, "Failed to get the adb version: " + e.Message ); 
			} 
		}

		/// <summary>
		/// Scans a line resulting from 'adb version' for a potential version number.
		/// </summary>
		/// <param name="line">The line to scan.</param>
		/// <returns><c>true</c> if a version number was found (whether it is acceptable or not).</returns>
		/// <remarks>If a version number is found, it checks the version number against what is expected
		/// by this version of ddms.</remarks>
		private bool ScanVersionLine ( String line ) {
			if ( !string.IsNullOrEmpty ( line ) ) {
				Match matcher = Regex.Match ( line, ADB_VERSION_PATTERN );
				if ( matcher.Success ) {
					int majorVersion = int.Parse ( matcher.Groups[1].Value );
					int minorVersion = int.Parse ( matcher.Groups[2].Value );
					int microVersion = int.Parse ( matcher.Groups[3].Value );

					// check only the micro version for now.
					if ( microVersion < ADB_VERSION_MICRO_MIN ) {
						String message = String.Format ( "Required minimum version of adb: {0}.{1}.{2}. Current version is {0}.{1}.{3}",
										majorVersion, minorVersion, ADB_VERSION_MICRO_MIN, microVersion );
						Log.LogAndDisplay ( LogLevel.Error, ADB, message );
					} else if ( ADB_VERSION_MICRO_MAX != -1 && microVersion > ADB_VERSION_MICRO_MAX ) {
						String message = String.Format ( "Required maximum version of adb: {0}.{1}.{2}. Current version is {0}.{1}.{3}", 
										majorVersion, minorVersion, ADB_VERSION_MICRO_MAX, microVersion );
						Log.LogAndDisplay ( LogLevel.Error, ADB, message );
					} else {
						VersionCheck = true;
					}
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Starts the adb host side server.
		/// </summary>
		/// <returns>true if success</returns>
		private bool StartAdb ( ) {
			if ( string.IsNullOrEmpty ( AdbOsLocation ) ) {
				Log.e ( ADB, "Cannot start adb when AndroidDebugBridge is created without the location of adb." ); 
				return false;
			}

			int status = -1;

			try {
				String command = "start-server";
				Log.d ( DDMS, String.Format ( "Launching '{0} {1}' to ensure ADB is running.", AdbOsLocation, command ) );
				ProcessStartInfo psi = new ProcessStartInfo ( AdbOsLocation, command );
				psi.CreateNoWindow = true;
				psi.WindowStyle = ProcessWindowStyle.Hidden;
				psi.UseShellExecute = false;
				psi.RedirectStandardError = true;
				psi.RedirectStandardOutput = true;

				using ( Process proc = Process.Start ( psi ) ) {
					List<String> errorOutput = new List<String> ( );
					List<String> stdOutput = new List<String> ( );
					status = GrabProcessOutput ( proc, errorOutput, stdOutput, false /* waitForReaders */);
				}
			} catch ( IOException ioe ) {
				Log.d ( DDMS, "Unable to run 'adb': {0}", ioe.Message );
			} catch ( ThreadInterruptedException ie ) {
				Log.d ( DDMS, "Unable to run 'adb': {0}", ie.Message );
			} catch ( Exception e ) {
				Log.e ( DDMS, e );
			}

			if ( status != 0 ) {
				Log.w ( DDMS, "'adb start-server' failed -- run manually if necessary" );
				return false;
			}

			Log.d ( DDMS, "'adb start-server' succeeded" );
			return true;
		}

		/// <summary>
		/// Stops the adb host side server.
		/// </summary>
		/// <returns>true if success</returns>
		private bool StopAdb ( ) {
			if ( string.IsNullOrEmpty ( AdbOsLocation ) ) {
				Log.e ( ADB, "Cannot stop adb when AndroidDebugBridge is created without the location of adb." );
				return false;
			}
			int status = -1;

			try {
				String command = "kill-server";
				ProcessStartInfo psi = new ProcessStartInfo ( AdbOsLocation, command );
				psi.CreateNoWindow = true;
				psi.WindowStyle = ProcessWindowStyle.Hidden;
				psi.UseShellExecute = false;
				psi.RedirectStandardError = true;
				psi.RedirectStandardOutput = true;

				using ( Process proc = Process.Start ( psi ) ) {
					proc.WaitForExit ( );
					status = proc.ExitCode;
				}
			} catch ( IOException ) {
				// we'll return false;
			} catch ( Exception ) {
				// we'll return false;
			}

			if ( status != 0 ) {
				Log.w ( DDMS, "'adb kill-server' failed -- run manually if necessary" ); 
				return false;
			}

			Log.d ( DDMS, "'adb kill-server' succeeded" ); 
			return true;
		}

		/// <summary>
		/// Get the stderr/stdout outputs of a process and return when the process is done.
		/// Both <b>must</b> be read or the process will block on windows.
		/// </summary>
		/// <param name="process">The process to get the ouput from</param>
		/// <param name="errorOutput">The array to store the stderr output. cannot be null.</param>
		/// <param name="stdOutput">The array to store the stdout output. cannot be null.</param>
		/// <param name="waitforReaders">if true, this will wait for the reader threads.</param>
		/// <returns>the process return code.</returns>
		private int GrabProcessOutput ( Process process, List<String> errorOutput, List<String> stdOutput, bool waitforReaders ) {
			if ( errorOutput == null ) {
				throw new ArgumentNullException ( "errorOutput" );
			}
			if ( stdOutput == null ) {
				throw new ArgumentNullException ( "stdOutput" );
			}
			// read the lines as they come. if null is returned, it's
			// because the process finished
			Thread t1 = new Thread ( new ThreadStart ( delegate {
				// create a buffer to read the stdoutput
				try {
					using ( StreamReader sr = process.StandardError ) {
						while ( !sr.EndOfStream ) {
							String line = sr.ReadLine ( );
							if ( !String.IsNullOrEmpty ( line ) ) {
								Log.e ( ADB, line );
								errorOutput.Add ( line );
							}
						}
					}
				} catch ( Exception ) {
					// do nothing.
				}
			} ) );

			Thread t2 = new Thread ( new ThreadStart ( delegate {
				// create a buffer to read the std output
				try {
					using ( StreamReader sr = process.StandardOutput ) {
						while ( !sr.EndOfStream ) {
							String line = sr.ReadLine ( );
							if ( !String.IsNullOrEmpty ( line ) ) {
								stdOutput.Add ( line );
							}
						}
					}
				} catch ( Exception ) {
					// do nothing.
				}
			} ) );

			t1.Start ( );
			t2.Start ( );

			// it looks like on windows process#waitFor() can return
			// before the thread have filled the arrays, so we wait for both threads and the
			// process itself.
			if ( waitforReaders ) {
				try {
					t1.Join ( );
				} catch ( ThreadInterruptedException ) {
				}
				try {
					t2.Join ( );
				} catch ( ThreadInterruptedException ) {
				}
			}

			// get the return code from the process
			process.WaitForExit ( );
			return process.ExitCode;
		}
		#endregion

	}
}
