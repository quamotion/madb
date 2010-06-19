using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.IO;
using System.Reflection;
using System.ComponentModel;

namespace Managed.Adb {
	public sealed class Device : IDevice {

		public const String PROP_BUILD_VERSION = "ro.build.version.release";
		public const String PROP_BUILD_API_LEVEL = "ro.build.version.sdk";
		public const String PROP_BUILD_CODENAME = "ro.build.version.codename";

		public const String PROP_DEBUGGABLE = "ro.debuggable";

		/** Serial number of the first connected emulator. */
		public const String FIRST_EMULATOR_SN = "emulator-5554"; //$NON-NLS-1$
		/** Device change bit mask: {@link DeviceState} change. */
		public const int CHANGE_STATE = 0x0001;
		/** Device change bit mask: {@link Client} list change. */
		public const int CHANGE_CLIENT_LIST = 0x0002;
		/** Device change bit mask: build info change. */
		public const int CHANGE_BUILD_INFO = 0x0004;

		/** @deprecated Use {@link #PROP_BUILD_API_LEVEL}. */
		public const String PROP_BUILD_VERSION_NUMBER = PROP_BUILD_API_LEVEL;

		/**
		 * The state of a device.
		 */
		public enum DeviceState {
			[System.ComponentModel.DisplayName ( "bootloader" )]
			BootLoader,
			[System.ComponentModel.DisplayName ( "offline" )]
			Offline,
			[System.ComponentModel.DisplayName ( "device" )]
			Online
		}

		public static DeviceState GetStateFromString ( String state ) {
			if ( Enum.IsDefined ( typeof ( DeviceState ), state ) ) {
				return (DeviceState)Enum.Parse ( typeof ( DeviceState ), state );
			} else {
				foreach ( var fi in typeof ( DeviceState ).GetFields ( ) ) {
					DisplayNameAttribute dna = Reflection.ReflectionHelper.GetCustomAttribute<DisplayNameAttribute> ( fi );
					if ( dna != null ) {
						if ( string.Compare ( dna.DisplayName, state, false ) == 0 ) {
							return (DeviceState)fi.GetValue ( null );
						}
					} else {
						if ( string.Compare ( fi.Name, state, true ) == 0 ) {
							return (DeviceState)fi.GetValue ( null );
						}
					}
				}
			}
		}

		/** Emulator Serial Number regexp. */
		const String RE_EMULATOR_SN = "emulator-(\\d+)"; //$NON-NLS-1$
		private const String LOG_TAG = "Device";
		private string avdName;

		/**
		 * Output receiver for "pm install package.apk" command line.
		 */
		private class InstallReceiver : MultiLineReceiver {

			private const String SUCCESS_OUTPUT = "Success"; //$NON-NLS-1$
			private const String FAILURE_PATTERN = "Failure\\s+\\[(.*)\\]"; //$NON-NLS-1$

			public InstallReceiver ( ) {
			}

			//@Override
			public void ProcessNewLines ( String[] lines ) {
				foreach ( String line in lines ) {
					if ( line.Length > 0 ) {
						if ( line.StartsWith ( SUCCESS_OUTPUT ) ) {
							ErrorMessage = null;
						} else {
							Regex pattern = new Regex ( FAILURE_PATTERN, RegexOptions.Compiled );
							Match m = FAILURE_PATTERN.matcher ( line );
							if ( m.matches ( ) ) {
								ErrorMessage = m.group ( 1 );
							}
						}
					}
				}
			}

			public bool IsCancelled { get { return false; } }

			public String ErrorMessage { get; private set; }
		}

		/*
		 * (non-Javadoc)
		 * @see com.android.ddmlib.IDevice#getSerialNumber()
		 */
		public String SerialNumber { get; private set; }

		/** {@inheritDoc} */
		public String AvdName {
			get { return avdName; }
			private set {
				if ( !IsEmulator ) {
					throw new ArgumentException ( "Cannot set the AVD name of the device is not an emulator" );
				}
				avdName = value;
			}
		}

		/*
		 * (non-Javadoc)
		 * @see com.android.ddmlib.IDevice#getState()
		 */
		public DeviceState State { get; private set; }


		/*
		 * (non-Javadoc)
		 * @see com.android.ddmlib.IDevice#getProperties()
		 */
		public Dictionary<String, String> Properties { get; private set; }

		/*
		 * (non-Javadoc)
		 * @see com.android.ddmlib.IDevice#getPropertyCount()
		 */
		public int PropertyCount {
			get {
				return Properties.Count;
			}
		}

		/*
		 * (non-Javadoc)
		 * @see com.android.ddmlib.IDevice#getProperty(java.lang.String)
		 */
		public String GetProperty ( String name ) {
			return Properties[ name ];
		}


		//@Override
		public override String ToString ( ) {
			return SerialNumber;
		}

		/*
		 * (non-Javadoc)
		 * @see com.android.ddmlib.IDevice#isOnline()
		 */
		public bool IsOnline {
			get {
				return State == DeviceState.ONLINE;
			}
		}

		/*
		 * (non-Javadoc)
		 * @see com.android.ddmlib.IDevice#isEmulator()
		 */
		public bool IsEmulator {
			get {
				return Regex.Match ( SerialNumber, RE_EMULATOR_SN ).Success;
			}
		}

		/*
		 * (non-Javadoc)
		 * @see com.android.ddmlib.IDevice#isOffline()
		 */
		public bool IsOffline {
			get {
				return State == DeviceState.OFFLINE;
			}
		}

		/*
		 * (non-Javadoc)
		 * @see com.android.ddmlib.IDevice#isBootLoader()
		 */
		public bool IsBootLoader {
			get {
				return State == DeviceState.BOOTLOADER;
			}
		}

		/*
		 * (non-Javadoc)
		 * @see com.android.ddmlib.IDevice#hasClients()
		 */
		public bool HasClients {
			get {
				return Clients.Length > 0;
			}
		}

		/*
		 * (non-Javadoc)
		 * @see com.android.ddmlib.IDevice#getClients()
		 */
		public Client[] Clients {
			get {
				lock ( this.ClientList ) {
					return this.ClientList.ToArray ( );
				}
			}
		}

		/*
		 * (non-Javadoc)
		 * @see com.android.ddmlib.IDevice#getClient(java.lang.String)
		 */
		public Client GetClient ( String applicationName ) {
			lock ( ClientList ) {
				foreach ( Client c in ClientList ) {
					if ( string.Compare ( applicationName, c.ClientData ( ).ClientDescription ( ), false ) ) {
						return c;
					}
				}

			}

			return null;
		}

		/*
		 * (non-Javadoc)
		 * @see com.android.ddmlib.IDevice#getSyncService()
		 */
		public SyncService SyncService {
			get {
				SyncService syncService = new SyncService ( AndroidDebugBridge.SocketAddress, this );
				if ( syncService.OpenSync ( ) ) {
					return syncService;
				}

				return null;
			}
		}

		/*
		 * (non-Javadoc)
		 * @see com.android.ddmlib.IDevice#getFileListingService()
		 */
		public FileListingService FileListingService {
			get {
				return new FileListingService ( this );
			}
		}

		/*
		 * (non-Javadoc)
		 * @see com.android.ddmlib.IDevice#getScreenshot()
		 */
		public RawImage Screenshot {
			get {
				return AdbHelper.GetFrameBuffer ( AndroidDebugBridge.SocketAddress, this );
			}
		}

		/*
		 * (non-Javadoc)
		 * @see com.android.ddmlib.IDevice#executeShellCommand(java.lang.String, com.android.ddmlib.IShellOutputReceiver)
		 */
		public void ExecuteShellCommand ( String command, IShellOutputReceiver receiver ) {
			AdbHelper.ExecuteRemoteCommand ( AndroidDebugBridge.sSocketAddress, command, this,
							receiver );
		}

		/*
		 * (non-Javadoc)
		 * @see com.android.ddmlib.IDevice#runEventLogService(com.android.ddmlib.log.LogReceiver)
		 */
		public void RunEventLogService ( LogReceiver receiver ) {
			AdbHelper.RunEventLogService ( AndroidDebugBridge.sSocketAddress, this, receiver );
		}

		/*
		 * (non-Javadoc)
		 * @see com.android.ddmlib.IDevice#runLogService(com.android.ddmlib.log.LogReceiver)
		 */
		public void RunLogService ( String logname, LogReceiver receiver ) {
			AdbHelper.RunLogService ( AndroidDebugBridge.sSocketAddress, this, logname, receiver );
		}

		/*
		 * (non-Javadoc)
		 * @see com.android.ddmlib.IDevice#createForward(int, int)
		 */
		public bool CreateForward ( int localPort, int remotePort ) {
			try {
				return AdbHelper.CreateForward ( AndroidDebugBridge.SocketAddress, this, localPort, remotePort );
			} catch ( IOException e ) {
				Log.e ( "adb-forward", e ); //$NON-NLS-1$
				return false;
			}
		}

		/*
		 * (non-Javadoc)
		 * @see com.android.ddmlib.IDevice#removeForward(int, int)
		 */
		public bool RemoveForward ( int localPort, int remotePort ) {
			try {
				return AdbHelper.RemoveForward ( AndroidDebugBridge.SocketAddress, this, localPort, remotePort );
			} catch ( IOException e ) {
				Log.e ( "adb-remove-forward", e ); //$NON-NLS-1$
				return false;
			}
		}

		/*
		 * (non-Javadoc)
		 * @see com.android.ddmlib.IDevice#getClientName(int)
		 */
		public String GetClientName ( int pid ) {
			lock ( ClientList ) {
				foreach ( Client c in ClientList ) {
					if ( c.ClientData ( ).Pid == pid ) {
						return c.ClientData.ClientDescription;
					}
				}
			}

			return null;
		}


		public Device ( DeviceMonitor monitor, String serialNumber, DeviceState deviceState ) {
			Monitor = monitor;
			SerialNumber = serialNumber;
			State = deviceState;
			ClientList = new List<Client> ( );
		}

		DeviceMonitor Monitor { get; private set; }

		void AddClient ( Client client ) {
			lock ( ClientList ) {
				ClientList.Add ( client );
			}
		}

		List<Client> ClientList { get; private set; }

		bool HasClient ( int pid ) {
			lock ( ClientList ) {
				foreach ( Client client in ClientList ) {
					if ( client.ClientData.Pid == pid ) {
						return true;
					}
				}
			}

			return false;
		}

		void ClearClientList ( ) {
			lock ( ClientList ) {
				ClientList.Clear ( );
			}
		}

		/**
		 * Returns the client monitoring socket.
		 */
		SocketChannel ClientMonitoringSocket { get; set; }

		/**
		 * Removes a {@link Client} from the list.
		 * @param client the client to remove.
		 * @param notify Whether or not to notify the listeners of a change.
		 */
		void RemoveClient ( Client client, bool notify ) {
			Monitor.AddPortToAvailableList ( client.DebuggerListenPort );
			lock ( ClientList ) {
				ClientList.Remove ( client );
			}
			if ( notify ) {
				Monitor.Server.DeviceChanged ( this, CHANGE_CLIENT_LIST );
			}
		}

		void Update ( int changeMask ) {
			Monitor.Server.DeviceChanged ( this, changeMask );
		}

		void Update ( Client client, int changeMask ) {
			Monitor.Server.ClientChanged ( client, changeMask );
		}

		void AddProperty ( String label, String value ) {
			Properties.Add ( label, value );
		}

		/**
		 * {@inheritDoc}
		 */
		public String InstallPackage ( String packageFilePath, bool reinstall ) {
			String remoteFilePath = SyncPackageToDevice ( packageFilePath );
			String result = InstallRemotePackage ( remoteFilePath, reinstall );
			RemoveRemotePackage ( remoteFilePath );
			return result;
		}

		/**
		 * {@inheritDoc}
		 */
		public String SyncPackageToDevice ( String localFilePath ) {
			try {
				String packageFileName = getFileName ( localFilePath );
				String remoteFilePath = String.Format ( "/data/local/tmp/{0}", packageFileName ); //$NON-NLS-1$

				Log.d ( packageFileName, String.Format ( "Uploading {0} onto device '{1}'",
								packageFileName, SerialNumber ) );

				SyncService sync = SyncService;
				if ( sync != null ) {
					String message = String.Format ( "Uploading file onto device '{0}'",
									SerialNumber );
					Log.d ( LOG_TAG, message );
					SyncResult result = sync.PushFile ( localFilePath, remoteFilePath,
									SyncService.NullProgressMonitor );

					if ( result.Code != SyncService.RESULT_OK ) {
						throw new IOException ( String.Format ( "Unable to upload file: {0}",
										result.Message ) );
					}
				} else {
					throw new IOException ( "Unable to open sync connection!" );
				}
				return remoteFilePath;
			} catch ( IOException e ) {
				Log.e ( LOG_TAG, String.Format ( "Unable to open sync connection! reason: {0}",
								e.Message ) );
				throw e;
			}
		}

		/**
		 * Helper method to retrieve the file name given a local file path
		 * @param filePath full directory path to file
		 * @return {@link String} file name
		 */
		private String GetFileName ( String filePath ) {
			return Path.GetFileName ( filePath );
		}

		/**
		 * {@inheritDoc}
		 */
		public String InstallRemotePackage ( String remoteFilePath, bool reinstall ) {
			InstallReceiver receiver = new InstallReceiver ( );
			String cmd = String.Format ( reinstall ? "pm install -r \"{0}\"" : "pm install \"{0}\"", remoteFilePath );
			ExecuteShellCommand ( cmd, receiver );
			return receiver.ErrorMessage;
		}

		/**
		 * {@inheritDoc}
		 */
		public void removeRemotePackage ( String remoteFilePath ) {
			// now we delete the app we sync'ed
			try {
				ExecuteShellCommand ( "rm " + remoteFilePath, new NullOutputReceiver ( ) );
			} catch ( IOException e ) {
				Log.e ( LOG_TAG, String.Format ( "Failed to delete temporary package: {0}", e.Message ) );
				throw e;
			}
		}

		/**
		 * {@inheritDoc}
		 */
		public String UninstallPackage ( String packageName ) {
			InstallReceiver receiver = new InstallReceiver ( );
			ExecuteShellCommand ( "pm uninstall " + packageName, receiver );
			return receiver.ErrorMessage;
		}
	}
}
