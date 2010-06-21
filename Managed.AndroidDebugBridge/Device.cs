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
	public enum DeviceState {
		//[FieldDisplayName ( "bootloader" )]
		BootLoader,
		//[FieldDisplayName ( "offline" )]
		Offline,
		//[FieldDisplayName ( "device" )]
		Online,
		//[FieldDisplayName ( "unknown" )]
		Unknown
	}

	public sealed class Device : IDevice {
		public Device ( String serial, DeviceState state ) {
			this.SerialNumber = serial;
			this.State = state;
		}
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


		public static DeviceState GetStateFromString ( String state ) {
			if ( Enum.IsDefined ( typeof ( DeviceState ), state ) ) {
				return (DeviceState)Enum.Parse ( typeof ( DeviceState ), state, true );
			} else {
				foreach ( var fi in typeof ( DeviceState ).GetFields ( ) ) {
					/*
					FieldDisplayNameAttribute dna = ReflectionHelper.GetCustomAttribute<FieldDisplayNameAttribute> ( fi );
					if ( dna != null ) {
						if ( string.Compare ( dna.DisplayName, state, false ) == 0 ) {
							return (DeviceState)fi.GetValue ( null );
						}
					} else { */
					if ( string.Compare ( fi.Name, state, true ) == 0 ) {
						return (DeviceState)fi.GetValue ( null );
					}
					// }
				}
			}
			return DeviceState.Unknown;
		}

		public static Device CreateFromAdbData ( String data ) {
			Regex re = new Regex ( RE_DEVICELIST_INFO, RegexOptions.Compiled | RegexOptions.IgnoreCase );
			Match m = re.Match ( data );
			if ( m.Success ) {
				return new Device ( m.Groups[1].Value, GetStateFromString ( m.Groups[2].Value ) );
			} else {
				throw new ArgumentException ( "Invalid device list data" );
			}
		}

		/** Emulator Serial Number regexp. */
		const String RE_EMULATOR_SN = @"emulator-(\d+)"; //$NON-NLS-1$
		const String RE_DEVICELIST_INFO = @"^([^\s]+)\s+(device|offline|unknown|bootloader)$";
		private const String LOG_TAG = "Device";
		private string avdName;



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
			return Properties[name];
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
				return State == DeviceState.Online;
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
				return State == DeviceState.Offline;
			}
		}

		/*
		 * (non-Javadoc)
		 * @see com.android.ddmlib.IDevice#isBootLoader()
		 */
		public bool IsBootLoader {
			get {
				return State == DeviceState.BootLoader;
			}
		}




		/*public bool HasClients {
	get {
		return Clients.Length > 0;
	}
}


public Client[] Clients {
	get {
		lock ( this.ClientList ) {
			return this.ClientList.ToArray ( );
		}
	}
}

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

public SyncService SyncService {
	get {
		SyncService syncService = new SyncService ( AndroidDebugBridge.SocketAddress, this );
		if ( syncService.OpenSync ( ) ) {
			return syncService;
		}

		return null;
	}
}

public FileListingService FileListingService {
	get {
		return new FileListingService ( this );
	}
}
*/
		public RawImage Screenshot {
			get {
				return AdbHelper.Instance.GetFrameBuffer ( AndroidDebugBridge.SocketAddress, this );
			}
		}

		public void ExecuteShellCommand ( String command, IShellOutputReceiver receiver ) {
			AdbHelper.Instance.ExecuteRemoteCommand ( AndroidDebugBridge.SocketAddress, command, this,
							receiver );
		}
		/*
		public void RunEventLogService ( LogReceiver receiver ) {
			AdbHelper.RunEventLogService ( AndroidDebugBridge.sSocketAddress, this, receiver );
		}

		public void RunLogService ( String logname, LogReceiver receiver ) {
			AdbHelper.RunLogService ( AndroidDebugBridge.sSocketAddress, this, logname, receiver );
		}

		public bool CreateForward ( int localPort, int remotePort ) {
			try {
				return AdbHelper.CreateForward ( AndroidDebugBridge.SocketAddress, this, localPort, remotePort );
			} catch ( IOException e ) {
				Console.WriteLine( e ); //$NON-NLS-1$
				return false;
			}
		}

		public bool RemoveForward ( int localPort, int remotePort ) {
			try {
				return AdbHelper.RemoveForward ( AndroidDebugBridge.SocketAddress, this, localPort, remotePort );
			} catch ( IOException e ) {
				Console.WriteLine ( e ); //$NON-NLS-1$
				return false;
			}
		}

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
		
		SocketChannel ClientMonitoringSocket { get; set; }

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

		public String InstallPackage ( String packageFilePath, bool reinstall ) {
			String remoteFilePath = SyncPackageToDevice ( packageFilePath );
			String result = InstallRemotePackage ( remoteFilePath, reinstall );
			RemoveRemotePackage ( remoteFilePath );
			return result;
		}

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

		private String GetFileName ( String filePath ) {
			return Path.GetFileName ( filePath );
		}

		public String InstallRemotePackage ( String remoteFilePath, bool reinstall ) {
			InstallReceiver receiver = new InstallReceiver ( );
			String cmd = String.Format ( reinstall ? "pm install -r \"{0}\"" : "pm install \"{0}\"", remoteFilePath );
			ExecuteShellCommand ( cmd, receiver );
			return receiver.ErrorMessage;
		}
		public void removeRemotePackage ( String remoteFilePath ) {
			// now we delete the app we sync'ed
			try {
				ExecuteShellCommand ( "rm " + remoteFilePath, NullOutputReceiver.Instance );
			} catch ( IOException e ) {
				Log.e ( LOG_TAG, String.Format ( "Failed to delete temporary package: {0}", e.Message ) );
				throw e;
			}
		}

		public String UninstallPackage ( String packageName ) {
			InstallReceiver receiver = new InstallReceiver ( );
			ExecuteShellCommand ( "pm uninstall " + packageName, receiver );
			return receiver.ErrorMessage;
		}
		*/

	}
}