using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.IO;
using System.Reflection;
using System.ComponentModel;
using Managed.Adb.Exceptions;

namespace Managed.Adb {
	public enum DeviceState {
		Recovery,
		BootLoader,
		Offline,
		Online,
		Unknown
	}

	public sealed class Device : IDevice {
		/// <summary>
		/// 
		/// </summary>
		public const String PROP_BUILD_VERSION = "ro.build.version.release";

		/// <summary>
		/// 
		/// </summary>
		public const String PROP_BUILD_API_LEVEL = "ro.build.version.sdk";

		/// <summary>
		/// 
		/// </summary>
		public const String PROP_BUILD_CODENAME = "ro.build.version.codename";

		/// <summary>
		/// 
		/// </summary>
		public const String PROP_DEBUGGABLE = "ro.debuggable";

		/// <summary>
		/// Serial number of the first connected emulator. 
		/// </summary>
		public const String FIRST_EMULATOR_SN = "emulator-5554"; //$NON-NLS-1$

		/** @deprecated Use {@link #PROP_BUILD_API_LEVEL}. */
		[Obsolete ( "Use PROP_BUILD_API_LEVEL" )]
		public const String PROP_BUILD_VERSION_NUMBER = PROP_BUILD_API_LEVEL;

		/// <summary>
		///  Emulator Serial Number regexp.
		/// </summary>
		private const String RE_EMULATOR_SN = @"emulator-(\d+)"; //$NON-NLS-1$

		/// <summary>
		/// Device list info regex
		/// </summary>
		private const String RE_DEVICELIST_INFO = @"^([^\s]+)\s+(device|offline|unknown|bootloader|recovery)$";
		/// <summary>
		/// Tag
		/// </summary>
		private const String LOG_TAG = "Device";


		public const String MNT_EXTERNAL_STORAGE = "EXTERNAL_STORAGE";
		public const String MNT_ROOT = "ANDROID_ROOT";
		public const String MNT_DATA = "ANDROID_DATA";


		/// <summary>
		/// 
		/// </summary>
		private string avdName;


		/// <summary>
		/// 
		/// </summary>
		/// <param name="serial"></param>
		/// <param name="state"></param>
		public Device ( String serial, DeviceState state ) {
			this.SerialNumber = serial;
			this.State = state;
			MountPoints = new Dictionary<String, MountPoint> ( );
			RefreshMountPoints ( );
		}

		/*public Device ( DeviceMonitor monitor, String serialNumber, DeviceState deviceState ) {
			//Monitor = monitor;
			SerialNumber = serialNumber;
			State = deviceState;
			//ClientList = new List<IClient> ( );
		}*/

		/// <summary>
		/// Get the device state from the string value
		/// </summary>
		/// <param name="state">The device state string</param>
		/// <returns></returns>
		public static DeviceState GetStateFromString ( String state ) {
			if ( Enum.IsDefined ( typeof ( DeviceState ), state ) ) {
				return (DeviceState)Enum.Parse ( typeof ( DeviceState ), state, true );
			} else {
				foreach ( var fi in typeof ( DeviceState ).GetFields ( ) ) {
					if ( string.Compare ( fi.Name, state, true ) == 0 ) {
						return (DeviceState)fi.GetValue ( null );
					}
				}
			}
			return DeviceState.Unknown;
		}

		/// <summary>
		/// Create a device from Adb Device list data
		/// </summary>
		/// <param name="data">the line data for the device</param>
		/// <returns></returns>
		public static Device CreateFromAdbData ( String data ) {
			Regex re = new Regex ( RE_DEVICELIST_INFO, RegexOptions.Compiled | RegexOptions.IgnoreCase );
			Match m = re.Match ( data );
			if ( m.Success ) {
				return new Device ( m.Groups[1].Value, GetStateFromString ( m.Groups[2].Value ) );

			} else {
				throw new ArgumentException ( "Invalid device list data" );
			}
		}

		/// <summary>
		/// Gets the device serial number
		/// </summary>
		public String SerialNumber { get; private set; }

		/// <summary>
		/// Gets or sets the Avd name.
		/// </summary>
		public String AvdName {
			get { return avdName; }
			set {
				if ( !IsEmulator ) {
					throw new ArgumentException ( "Cannot set the AVD name of the device is not an emulator" );
				}
				avdName = value;
			}
		}

		/// <summary>
		/// Gets the device state
		/// </summary>
		public DeviceState State { get; private set; }

		/// <summary>
		/// Gets the device mount points.
		/// </summary>
		public Dictionary<String, MountPoint> MountPoints { get; set; }

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

		public String GetProperty ( String name ) {
			return Properties[name];
		}


		public override String ToString ( ) {
			return SerialNumber;
		}

		public bool IsOnline {
			get {
				return State == DeviceState.Online;
			}
		}

		public bool IsEmulator {
			get {
				return Regex.Match ( SerialNumber, RE_EMULATOR_SN ).Success;
			}
		}

		public bool IsOffline {
			get {
				return State == DeviceState.Offline;
			}
		}

		public bool IsBootLoader {
			get {
				return State == DeviceState.BootLoader;
			}
		}

		public bool IsRecovery {
			get { return State == DeviceState.Recovery; }
		}

		public void RemountMountPoint ( MountPoint mnt, bool readOnly ) {
			if ( mnt.IsReadOnly == readOnly ) {
				throw new ArgumentException ( String.Format ( "Mount point is already set as {0}", readOnly ? "ro" : "rw" ) );
			}

			String command = String.Format ( "mount -o {0},remount -t {1} {2} {3}", readOnly ? "ro" : "rw", mnt.FileSystem, mnt.Block, mnt.Name );
			this.ExecuteShellCommand ( command, NullOutputReceiver.Instance );

			RefreshMountPoints ( );
		}

		public void RefreshMountPoints ( ) {
			if ( !IsOffline ) {
				var receiver = new MountPointReceiver ( );
				this.ExecuteShellCommand ( "mount", receiver );
				foreach ( var item in receiver.MountPoints.Keys ) {
					if ( this.MountPoints.ContainsKey ( item ) ) {
						this.MountPoints.Remove ( item );
					}
					this.MountPoints.Add ( item, receiver.MountPoints[item].Clone ( ) );
				}
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
}*/

		public SyncService SyncService {
			get {
				SyncService syncService = new SyncService ( AndroidDebugBridge.SocketAddress, this );
				if ( syncService.Open ( ) ) {
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

		public RawImage Screenshot {
			get {
				return AdbHelper.Instance.GetFrameBuffer ( AndroidDebugBridge.SocketAddress, this );
			}
		}

		public void ExecuteShellCommand ( String command, IShellOutputReceiver receiver ) {
			ExecuteShellCommand ( command, receiver, new object[] { } );
		}

		public void ExecuteShellCommand ( String command, IShellOutputReceiver receiver, params object[] commandArgs ) {
			AdbHelper.Instance.ExecuteRemoteCommand ( AndroidDebugBridge.SocketAddress, string.Format ( command, commandArgs ), this, receiver );
		}

		/*
		public void RunEventLogService ( LogReceiver receiver ) {
			AdbHelper.RunEventLogService ( AndroidDebugBridge.sSocketAddress, this, receiver );
		}

		public void RunLogService ( String logname, LogReceiver receiver ) {
			AdbHelper.RunLogService ( AndroidDebugBridge.sSocketAddress, this, logname, receiver );
		}
		*/
		public bool CreateForward ( int localPort, int remotePort ) {
			try {
				return AdbHelper.Instance.CreateForward ( AndroidDebugBridge.SocketAddress, this, localPort, remotePort );
			} catch ( IOException e ) {
				Log.w ( "ddms", e );
				return false;
			}
		}

		public bool RemoveForward ( int localPort, int remotePort ) {
			try {
				return AdbHelper.Instance.RemoveForward ( AndroidDebugBridge.SocketAddress, this, localPort, remotePort );
			} catch ( IOException e ) {
				Log.w ( "ddms", e );
				return false;
			}
		}

		/*
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
		*/
		void AddProperty ( String label, String value ) {
			Properties.Add ( label, value );
		}

		public void InstallPackage ( String packageFilePath, bool reinstall ) {
			String remoteFilePath = SyncPackageToDevice ( packageFilePath );
			InstallRemotePackage ( remoteFilePath, reinstall );
			RemoveRemotePackage ( remoteFilePath );
		}

		public String SyncPackageToDevice ( String localFilePath ) {
			try {
				String packageFileName = Path.GetFileName ( localFilePath );
				String remoteFilePath = String.Format ( "/data/local/tmp/{0}", packageFileName );

				Log.d ( packageFileName, String.Format ( "Uploading {0} onto device '{1}'", packageFileName, SerialNumber ) );

				SyncService sync = SyncService;
				if ( sync != null ) {
					String message = String.Format ( "Uploading file onto device '{0}'", SerialNumber );
					Log.d ( LOG_TAG, message );
					SyncResult result = sync.PushFile ( localFilePath, remoteFilePath, SyncService.NullProgressMonitor );

					if ( result.Code != ErrorCodeHelper.RESULT_OK ) {
						throw new IOException ( String.Format ( "Unable to upload file: {0}", result.Message ) );
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

		public void InstallRemotePackage ( String remoteFilePath, bool reinstall ) {
			InstallReceiver receiver = new InstallReceiver ( );
			String cmd = String.Format ( reinstall ? "pm install -r \"{0}\"" : "pm install \"{0}\"", remoteFilePath );
			ExecuteShellCommand ( cmd, receiver );

			if ( !String.IsNullOrEmpty ( receiver.ErrorMessage ) ) {
				throw new PackageInstallationException ( receiver.ErrorMessage );
			}
		}


		public void RemoveRemotePackage ( String remoteFilePath ) {
			// now we delete the app we sync'ed
			try {
				ExecuteShellCommand ( "rm " + remoteFilePath, NullOutputReceiver.Instance );
			} catch ( IOException e ) {
				Log.e ( LOG_TAG, String.Format ( "Failed to delete temporary package: {0}", e.Message ) );
				throw e;
			}
		}

		public void UninstallPackage ( String packageName ) {
			InstallReceiver receiver = new InstallReceiver ( );
			ExecuteShellCommand ( String.Format ( "pm uninstall {0}", packageName ), receiver );
			if ( !String.IsNullOrEmpty ( receiver.ErrorMessage ) ) {
				throw new PackageInstallationException ( receiver.ErrorMessage );
			}
		}


	}
}