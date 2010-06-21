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
	public sealed class AndroidDebugBridge {

		public event EventHandler<AndroidDebugBridgeEventArgs> BridgeChanged;
		public event EventHandler<DeviceEventArgs> DeviceChanged;
		public event EventHandler<DeviceEventArgs> DeviceConnected;
		public event EventHandler<DeviceEventArgs> DeviceDisconnected;
		public event EventHandler<ClientEventArgs> ClientChanged;

		/*
		 * Minimum and maximum version of adb supported. This correspond to
		 * ADB_SERVER_VERSION found in //device/tools/adb/adb.h
		 */

		private const int ADB_VERSION_MICRO_MIN = 20;
		private const int ADB_VERSION_MICRO_MAX = -1;


		private const String ADB_VERSION_PATTERN = "^.*(\\d+)\\.(\\d+)\\.(\\d+)$"; //$NON-NLS-1$

		private const String ADB = "adb"; //$NON-NLS-1$
		private const String DDMS = "ddms"; //$NON-NLS-1$

		// Where to find the ADB bridge.
		public const String ADB_HOST = "127.0.0.1"; //$NON-NLS-1$
		public const int ADB_PORT = 5037;


		#region statics
		private static IPAddress _hostAddr;
		private static IPEndPoint _socketAddr;
		private static AndroidDebugBridge _instance;
		private static bool _clientSupport;

		static AndroidDebugBridge ( ) {
			// built-in local address/port for ADB.
			try {
				_hostAddr = IPAddress.Parse ( ADB_HOST );

				_socketAddr = new IPEndPoint ( _hostAddr, ADB_PORT );
			} catch ( ArgumentOutOfRangeException e ) {

			}
		}

		/**
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
		public static void Initialize ( bool clientSupport ) {
			ClientSupport = clientSupport;

			MonitorThread monitorThread = MonitorThread.createInstance ( );
			monitorThread.start ( );

			HandleHello.register ( monitorThread );
			HandleAppName.register ( monitorThread );
			HandleTest.register ( monitorThread );
			HandleThread.register ( monitorThread );
			HandleHeap.register ( monitorThread );
			HandleWait.register ( monitorThread );
			HandleProfiling.register ( monitorThread );
		}

		/**
		* Terminates the ddm library. This must be called upon application termination.
		*/
		public static void Terminate ( ) {
			// kill the monitoring services
			if ( Instance != null && Instance.DeviceMonitor != null ) {
				Instance.DeviceMonitor.Stop ( );
				Instance.DeviceMonitor = null;
			}

			MonitorThread monitorThread = MonitorThread.getInstance ( );
			if ( monitorThread != null ) {
				monitorThread.quit ( );
			}
		}


		/// <summary>
		/// Gets the instance.
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

		public static AndroidDebugBridge Bridge {
			get { return Instance; }
		}

		public static bool ClientSupport { get; private set; }

		/**
     * Creates a {@link AndroidDebugBridge} that is not linked to any particular executable.
     * <p/>This bridge will expect adb to be running. It will not be able to start/stop/restart
     * adb.
     * <p/>If a bridge has already been started, it is directly returned with no changes (similar
     * to calling {@link #getBridge()}).
     * @return a connected bridge.
     */
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

		/**
     * Creates a new debug bridge from the location of the command line tool.
     * <p/>
     * Any existing server will be disconnected, unless the location is the same and
     * <code>forceNewBridge</code> is set to false.
     * @param osLocation the location of the command line tool 'adb'
     * @param forceNewBridge force creation of a new bridge even if one with the same location
     * already exists.
     * @return a connected bridge.
     */
		public static AndroidDebugBridge CreateBridge ( String osLocation, bool forceNewBridge ) {

			if ( _instance != null ) {
				if ( _instance.AdbOsLocation != null && string.Compare ( _instance.AdbOsLocation, osLocation, true ) == 0 && forceNewBridge == false ) {
					return _instance;
				} else {
					// stop the current server
					_instance.Stop ( );
				}
			}

			try {
				_instance = new AndroidDebugBridge ( osLocation );
				_instance.Start ( );
				_instance.OnBridgeChanged ( new AndroidDebugBridgeEventArgs ( _instance ) );
			} catch ( ArgumentException e ) {
				_instance.OnBridgeChanged ( new AndroidDebugBridgeEventArgs ( null ) );
				_instance = null;
			}

			return _instance;
		}

		/**
     * Disconnects the current debug bridge, and destroy the object.
     * <p/>This also stops the current adb host server.
     * <p/>
     * A new object will have to be created with {@link #createBridge(String, boolean)}.
     */
		public static void disconnectBridge ( ) {
			if ( _instance != null ) {
				_instance.Stop ( );

				_instance.OnBridgeChanged ( new AndroidDebugBridgeEventArgs ( null ) );
				_instance = null;
			}
		}


		#endregion

		#region constructors
		/**
     * Creates a new bridge.
     * @param osLocation the location of the command line tool
     * @throws InvalidParameterException
     */
		private AndroidDebugBridge ( String osLocation ) {
			if ( string.IsNullOrEmpty ( osLocation ) ) {
				throw new ArgumentException ( );
			}
			AdbOsLocation = osLocation;

			CheckAdbVersion ( );
		}

		/**
		 * Creates a new bridge not linked to any particular adb executable.
		 */
		private AndroidDebugBridge ( ) {
		}

		#endregion

		#region Event "Raisers"
		/// <summary>
		/// Raises the <see cref="E:BridgeChanged"/> event.
		/// </summary>
		/// <param name="e">The <see cref="Managed.Adb.AndroidDebugBridgeEventArgs"/> instance containing the event data.</param>
		protected void OnBridgeChanged ( AndroidDebugBridgeEventArgs e ) {
			if ( this.BridgeChanged != null ) {
				this.BridgeChanged ( this, e );
			}
		}

		/// <summary>
		/// Raises the <see cref="E:ClientChanged"/> event.
		/// </summary>
		/// <param name="e">The <see cref="Managed.Adb.ClientEventArgs"/> instance containing the event data.</param>
		protected void OnClientChanged ( ClientEventArgs e ) {
			if ( this.ClientChanged != null ) {
				this.ClientChanged ( this, e );
			}
		}

		/// <summary>
		/// Raises the <see cref="E:DeviceChanged"/> event.
		/// </summary>
		/// <param name="e">The <see cref="Managed.Adb.DeviceEventArgs"/> instance containing the event data.</param>
		protected void OnDeviceChanged ( DeviceEventArgs e ) {
			if ( this.DeviceChanged != null ) {
				this.DeviceChanged ( this, e );
			}
		}

		/// <summary>
		/// Raises the <see cref="E:DeviceConnected"/> event.
		/// </summary>
		/// <param name="e">The <see cref="Managed.Adb.DeviceEventArgs"/> instance containing the event data.</param>
		protected void OnDeviceConnected ( DeviceEventArgs e ) {
			if ( this.DeviceConnected != null ) {
				this.DeviceConnected ( this, e );
			}
		}

		/// <summary>
		/// Raises the <see cref="E:DeviceDisconnected"/> event.
		/// </summary>
		/// <param name="e">The <see cref="Managed.Adb.DeviceEventArgs"/> instance containing the event data.</param>
		protected void OnDeviceDisconnected ( DeviceEventArgs e ) {
			if ( this.DeviceDisconnected != null ) {
				this.DeviceDisconnected ( this, e );
			}
		}
		#endregion

		#region public methods
		/**
     * Starts the debug bridge.
     * @return true if success.
     */
		public bool Start ( ) {
			if ( AdbOsLocation != null && ( VersionCheck == false || StartAdb ( ) == false ) ) {
				return false;
			}

			Started = true;

			// now that the bridge is connected, we start the underlying services.
			DeviceMonitor = new DeviceMonitor ( this );
			DeviceMonitor.Start ( );

			return true;
		}

		/**
     * Kills the debug bridge, and the adb host server.
     * @return true if success
     */
		public bool Stop ( ) {
			// if we haven't started we return false;
			if ( Started == false ) {
				return false;
			}

			// kill the monitoring services
			DeviceMonitor.Stop ( );
			DeviceMonitor = null;

			if ( StopAdb ( ) == false ) {
				return false;
			}

			Started = false;
			return true;
		}

		/**
		 * Restarts adb, but not the services around it.
		 * @return true if success.
		 */
		public bool Restart ( ) {
			if ( string.IsNullOrEmpty ( AdbOsLocation ) ) {
				Log.e ( ADB, "Cannot restart adb when AndroidDebugBridge is created without the location of adb." ); //$NON-NLS-1$
				return false;
			}

			if ( VersionCheck == false ) {
				Log.LogAndDisplay ( LogLevel.ERROR, ADB, "Attempting to restart adb, but version check failed!" ); //$NON-NLS-1$
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
		/// Gets the adb location on the OS.
		/// </summary>
		/// <value>The adb location on the OS.</value>
		public string AdbOsLocation { get; internal set; }
		/// <summary>
		/// Gets the devices.
		/// </summary>
		/// <value>The devices.</value>
		public List<IDevice> Devices {
			get {
				if ( DeviceMonitor != null ) {
					return DeviceMonitor.Devices;
				}
				return new List<IDevice> ( );
			}
		}

		/// <summary>
		/// Returns whether the bridge has acquired the initial list from adb after being created.
		/// </summary>
		/// <remarks>
		/// <p/>Calling {@link #getDevices()} right after {@link #createBridge(String, boolean)} will
		/// generally result in an empty list. This is due to the internal asynchronous communication
		/// mechanism with <code>adb</code> that does not guarantee that the {@link IDevice} list has been
		/// built before the call to {@link #getDevices()}.
		/// <p/>The recommended way to get the list of {@link IDevice} objects is to create a
		/// {@link IDeviceChangeListener} object.
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

		/**
     * Sets the client to accept debugger connection on the custom "Selected debug port".
     * @param selectedClient the client. Can be null.
     */
		public Client SelectedClient {
			get {
				MonitorThread monitorThread = MonitorThread.Instance;
				if ( monitorThread != null ) {
					return monitorThread.SelectedClient = selectedClient;
				}
				return null;
			}
			set {
				MonitorThread monitorThread = MonitorThread.Instance;
				if ( monitorThread != null ) {
					monitorThread.SelectedClient = value;
				}
			}
		}
		/// <summary>
		/// Returns whether the {@link AndroidDebugBridge} object is still connected to the adb daemon.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is connected; otherwise, <c>false</c>.
		/// </value>
		public bool IsConnected {
			get {
				MonitorThread monitorThread = MonitorThread.Instance;
				if ( DeviceMonitor != null && monitorThread != null ) {
					return DeviceMonitor.IsMonitoring && monitorThread.State != State.TERMINATED;
				}
				return false;
			}
		}

		/// <summary>
		/// Returns the number of times the {@link AndroidDebugBridge} object attempted to connect
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
		/// Returns the number of times the {@link AndroidDebugBridge} object attempted to restart
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

		//public DeviceMonitor DeviceMonitor { get; private set; }
		#endregion


		#region private methods

		/**
     * Queries adb for its version number and checks it against {@link #MIN_VERSION_NUMBER} and
     * {@link #MAX_VERSION_NUMBER}
     */
		private void CheckAdbVersion ( ) {
			// default is bad check
			VersionCheck = false;

			if ( AdbOsLocation == null ) {
				return;
			}

			try {
				String[] command = new String[ 2 ];
				command[ 0 ] = AdbOsLocation;
				command[ 1 ] = "version"; //$NON-NLS-1$
				Log.d ( DDMS, String.Format ( "Checking '{0} version'", AdbOsLocation ) ); //$NON-NLS-1$
				Process process = Process.Start ( command[0], command[1] );

				List<String> errorOutput = new List<String> ( );
				List<String> stdOutput = new List<String> ( );
				int status = GrabProcessOutput ( process, errorOutput, stdOutput, true /* waitForReaders */);

				if ( status != 0 ) {
					StringBuilder builder = new StringBuilder ( "'adb version' failed!" ); //$NON-NLS-1$
					builder.AppendLine ( string.Empty );
					foreach ( String error in errorOutput ) {
						builder.AppendLine ( error );
					}
					Log.LogAndDisplay ( LogLevel.Error, "adb", builder.ToString ( ) );
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
					Log.LogAndDisplay ( LogLevel.Error, ADB, "Failed to parse the output of 'adb version'" ); //$NON-NLS-1$
				}

			} catch ( IOException e ) {
				Log.LogAndDisplay ( LogLevel.Error, ADB,
								"Failed to get the adb version: " + e.Message ); //$NON-NLS-1$
			} catch ( InterruptedException e ) {
			} finally {

			}
		}


		/**
     * Scans a line resulting from 'adb version' for a potential version number.
     * <p/>
     * If a version number is found, it checks the version number against what is expected
     * by this version of ddms.
     * <p/>
     * Returns true when a version number has been found so that we can stop scanning,
     * whether the version number is in the acceptable range or not.
     *
     * @param line The line to scan.
     * @return True if a version number was found (whether it is acceptable or not).
     */
		private bool ScanVersionLine ( String line ) {
			if ( !string.IsNullOrEmpty ( line ) ) {

				Match matcher = Regex.Match ( line, sAdbVersion );
				if ( matcher.Success ) {
					int majorVersion = int.Parse ( matcher.Groups[ 1 ].Value );
					int minorVersion = int.Parse ( matcher.Groups[ 2 ].Value );
					int microVersion = int.Parse ( matcher.Groups[ 3 ].Value );

					// check only the micro version for now.
					if ( microVersion < ADB_VERSION_MICRO_MIN ) {
						String message = String.format (
										"Required minimum version of adb: {0}.{1}.{2}." //$NON-NLS-1$
										+ "Current version is {0}.{1}.{3}", //$NON-NLS-1$
										majorVersion, minorVersion, ADB_VERSION_MICRO_MIN,
										microVersion );
						Log.LogAndDisplay ( LogLevel.ERROR, ADB, message );
					} else if ( ADB_VERSION_MICRO_MAX != -1 && microVersion > ADB_VERSION_MICRO_MAX ) {
						String message = String.format (
										"Required maximum version of adb: {0}.{1}.{2}." //$NON-NLS-1$
										+ "Current version is {0}.{1}.{3}", //$NON-NLS-1$
										majorVersion, minorVersion, ADB_VERSION_MICRO_MAX,
										microVersion );
						Log.LogAndDisplay ( LogLevel.ERROR, ADB, message );
					} else {
						mVersionCheck = true;
					}
					return true;
				}
			}
			return false;
		}

		private bool StartAdb ( ) {
			if ( string.IsNullOrEmpty ( AdbOsLocation ) ) {
				Log.e ( ADB, "Cannot start adb when AndroidDebugBridge is created without the location of adb." ); //$NON-NLS-1$
				return false;
			}

			Process proc;
			int status = -1;

			try {
				String[] command = new String[ 2 ];
				command[ 0 ] = mAdbOsLocation;
				command[ 1 ] = "start-server"; //$NON-NLS-1$
				Log.d ( DDMS,
								String.Format ( "Launching '{0} {1}' to ensure ADB is running.", //$NON-NLS-1$
								mAdbOsLocation, command[ 1 ] ) );
				proc = Runtime.getRuntime ( ).exec ( command );

				List<String> errorOutput = new List<String> ( );
				List<String> stdOutput = new List<String> ( );
				status = GrabProcessOutput ( proc, errorOutput, stdOutput, false /* waitForReaders */);

			} catch ( IOException ioe ) {
				Log.d ( DDMS, "Unable to run 'adb': " + ioe.Message ); //$NON-NLS-1$
				// we'll return false;
			} catch ( InterruptedException ie ) {
				Log.d ( DDMS, "Unable to run 'adb': " + ie.Message ); //$NON-NLS-1$
				// we'll return false;
			}

			if ( status != 0 ) {
				Log.w ( DDMS, "'adb start-server' failed -- run manually if necessary" ); //$NON-NLS-1$
				return false;
			}

			Log.d ( DDMS, "'adb start-server' succeeded" ); //$NON-NLS-1$

			return true;
		}

		/**
     * Stops the adb host side server.
     * @return true if success
     */
		private bool StopAdb ( ) {
			if ( string.IsNullOrEmpty ( AdbOsLocation ) ) {
				Log.e ( ADB, "Cannot stop adb when AndroidDebugBridge is created without the location of adb." ); //$NON-NLS-1$
				return false;
			}

			Process proc;
			int status = -1;

			try {
				String[] command = new String[ 2 ];
				command[ 0 ] = AdbOsLocation;
				command[ 1 ] = "kill-server"; //$NON-NLS-1$
				proc = Process.Start ( command[0], command[1] );
				proc.WaitForExit ( );
				status = proc.ExitCode;
			} catch ( IOException ioe ) {
				// we'll return false;
			} catch ( Exception e ) {
				// we'll return false;
			}

			if ( status != 0 ) {
				Log.w ( DDMS, "'adb kill-server' failed -- run manually if necessary" ); //$NON-NLS-1$
				return false;
			}

			Log.d ( DDMS, "'adb kill-server' succeeded" ); //$NON-NLS-1$
			return true;
		}


		/**
		 * Get the stderr/stdout outputs of a process and return when the process is done.
		 * Both <b>must</b> be read or the process will block on windows.
		 * @param process The process to get the ouput from
		 * @param errorOutput The array to store the stderr output. cannot be null.
		 * @param stdOutput The array to store the stdout output. cannot be null.
		 * @param displayStdOut If true this will display stdout as well
		 * @param waitforReaders if true, this will wait for the reader threads.
		 * @return the process return code.
		 * @throws InterruptedException
		 */
		private int GrabProcessOutput ( Process process, List<String> errorOutput,
						List<String> stdOutput, bool waitforReaders ) {
			if ( errorOutput == null ) {
				throw new ArgumentNullException ( "errorOutput" );
			}
			if ( stdOutput == null ) {
				throw new ArgumentNullException ( "stdOutput" );
			}
			// read the lines as they come. if null is returned, it's
			// because the process finished
			Thread t1 = new Thread ( new ThreadStart ( delegate {
				// create a buffer to read the stderr output
				using ( StreamReader sr = process.StandardError ) {
					try {
						while ( !sr.EndOfStream ) {
							String line = sr.ReadLine ( );
							if ( line != null ) {
								Log.e ( ADB, line );
								errorOutput.Add ( line );
							} else {
								break;
							}
						}
					} catch ( IOException e ) {
						// do nothing.
					}
				}
			}
	) );

			Thread t2 = new Thread ( new ThreadStart ( delegate {
				// create a buffer to read the stderr output
				using ( StreamReader sr = process.StandardError ) {
					try {
						while ( !sr.EndOfStream ) {
							String line = sr.ReadLine ( );
							if ( line != null ) {
								Log.d ( ADB, line );
								stdOutput.Add ( line );
							} else {
								break;
							}
						}
					} catch ( IOException e ) {
						// do nothing.
					}
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
				} catch ( ThreadInterruptedException e ) {
				}
				try {
					t2.Join ( );
				} catch ( ThreadInterruptedException e ) {
				}
			}

			// get the return code from the process
			process.WaitForExit ( );
			return process.ExitCode;
		}
		#endregion

	}
}
