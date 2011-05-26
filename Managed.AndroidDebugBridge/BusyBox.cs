using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Managed.Adb.IO;
using System.Text.RegularExpressions;

namespace Managed.Adb {
	/// <summary>
	/// A class to help with working with BusyBox
	/// </summary>
	public class BusyBox {
		/// <summary>
		/// 
		/// </summary>
		private const String BUSYBOX_BIN = "/data/local/bin/";
		/// <summary>
		/// 
		/// </summary>
		private const String BUSYBOX_COMMAND = "busybox";

		/// <summary>
		/// Initializes a new instance of the <see cref="BusyBox"/> class.
		/// </summary>
		/// <param name="device">The device.</param>
		public BusyBox ( Device device ) {
			this.Device = device;
			Version = new System.Version ( "0.0.0.0" );
			Commands = new List<String> ( );
			CheckForBusyBox ( );

		}

		/// <summary>
		/// Attempts to install on the device
		/// </summary>
		/// <param name="busybox">The path to the busybox binary to install.</param>
		/// <returns><c>true</c>, if successful; otherwise, <c>false</c></returns>
		public bool Install ( String busybox ) {
			FileEntry bb = null;

			try {
				Device.ExecuteShellCommand ( BUSYBOX_COMMAND, NullOutputReceiver.Instance );
				return true;
			} catch {
				// we are just checking if it is already installed so we really expect it to wind up here.
			}

			try {
				MountPoint mp = Device.MountPoints["/data"];
				bool isRO = mp.IsReadOnly;
				Device.RemountMountPoint ( Device.MountPoints["/data"], false );

				FileEntry path = null;
				try {
					path = Device.FileListingService.FindFileEntry ( BUSYBOX_BIN );
				} catch ( FileNotFoundException ) {
					// path doesn't exist, so we make it.
					Device.FileSystem.MakeDirectory ( BUSYBOX_BIN );
					// attempt to get the FileEntry after the directory has been made
					path = Device.FileListingService.FindFileEntry ( BUSYBOX_BIN );
				}

				Device.FileSystem.Chmod ( path.FullPath, "0755" );

				String bbPath = LinuxPath.Combine ( path.FullPath, BUSYBOX_COMMAND );

				Device.FileSystem.Copy ( busybox, bbPath );


				bb = Device.FileListingService.FindFileEntry ( bbPath );
				Device.FileSystem.Chmod ( bb.FullPath, "0755" );

				Device.ExecuteShellCommand ( "{0}/busybox --install {0}", new ConsoleOutputReceiver ( ), path.FullPath );

				// check if this path exists in the path already
				if ( Device.EnvironmentVariables.ContainsKey ( "PATH" ) ) {
					String[] paths = Device.EnvironmentVariables["PATH"].Split ( ':' );
					bool found = false;
					foreach ( var tpath in paths ) {
						if ( String.Compare ( tpath, BUSYBOX_BIN, false ) == 0 ) {
							Console.WriteLine ( "Already in PATH" );
							found = true;
							break;
						}
					}

					// we didnt find it, so add it.
					if ( !found ) {
						// this doesn't seem to actually work
						Device.ExecuteShellCommand ( @"echo \ Mad Bee buxybox >> /init.rc", NullOutputReceiver.Instance );
						Device.ExecuteShellCommand ( @"echo export PATH={0}:\$PATH >> /init.rc", NullOutputReceiver.Instance, BUSYBOX_BIN );
					}
				}


				if ( mp.IsReadOnly != isRO ) {
					// Put it back, if we changed it
					Device.RemountMountPoint ( mp, isRO );
				}

				Device.ExecuteShellCommand ( "sync", NullOutputReceiver.Instance );
			} catch ( Exception ) {
				throw;
			}

			CheckForBusyBox ( );
			return true;
		}

		/// <summary>
		/// Checks for busy box.
		/// </summary>
		private void CheckForBusyBox ( ) {
			if ( this.Device.IsOnline ) {
				try {
					Commands.Clear ( );
					Device.ExecuteShellCommand ( BUSYBOX_COMMAND, new BusyBoxCommandsReceiver ( this ) );
					Available = true;
				} catch ( FileNotFoundException ) {
					Available = false;
				}
			} else {
				Available = false;
			}
		}


		/// <summary>
		/// Gets or sets the device.
		/// </summary>
		/// <value>
		/// The device.
		/// </value>
		public Device Device { get; set; }
		/// <summary>
		/// Gets if busybox is available on this device.
		/// </summary>
		public bool Available { get; private set; }
		/// <summary>
		/// Gets the version of busybox
		/// </summary>
		public Version Version { get; private set; }
		/// <summary>
		/// Gets a collection of the supported commands
		/// </summary>
		public List<String> Commands { get; private set; }

		/// <summary>
		/// Gets if the specified command name is supported by this version of busybox
		/// </summary>
		/// <param name="command">The command name to check</param>
		/// <returns><c>true</c>, if supported; otherwise, <c>false</c>.</returns>
		public bool Supports ( String command ) {
			if ( String.IsNullOrEmpty ( command ) || String.IsNullOrEmpty ( command.Trim ( ) ) ) {
				throw new ArgumentException ( "Command must not be empty", "command" );
			}

			return Commands.Contains ( command );
		}

		/// <summary>
		/// 
		/// </summary>
		private class BusyBoxCommandsReceiver : MultiLineReceiver {
			/// <summary>
			/// The busybox version regex pattern
			/// </summary>
			private const String BB_VERSION_PATTERN = @"^BusyBox\sv(\d{1,}\.\d{1,}\.\d{1,})";
			/// <summary>
			/// the busybox commands list regex pattern
			/// </summary>
			private const String BB_FUNCTIONS_PATTERN = @"(?:([\[a-z0-9]+)(?:,\s|$))";

			public BusyBoxCommandsReceiver ( BusyBox bb )
				: base ( ) {
				TrimLines = true;
				BusyBox = bb;
			}

			/// <summary>
			/// Gets or sets the busy box.
			/// </summary>
			/// <value>
			/// The busy box.
			/// </value>
			private BusyBox BusyBox { get; set; }

			/// <summary>
			/// Processes the new lines.
			/// </summary>
			/// <param name="lines">The lines.</param>
			protected override void ProcessNewLines ( string[] lines ) {
				foreach ( var line in lines ) {
					if ( String.IsNullOrEmpty ( line ) || line.StartsWith ( "#" ) ) {
						continue;
					}

					Match m = Regex.Match ( line, BB_VERSION_PATTERN, RegexOptions.Compiled |  RegexOptions.IgnoreCase );
					if ( m.Success ) {
						BusyBox.Version = new Version ( m.Groups[1].Value );
						BusyBox.Commands.Clear ( );
						continue;
					}

					if ( line.Contains ( "defined functions" ) ) {
						BusyBox.Commands.Clear ( );
					}

					m = Regex.Match ( line, BB_FUNCTIONS_PATTERN, RegexOptions.Compiled  );
					while ( m.Success ) {
						BusyBox.Commands.Add ( m.Groups[1].Value.Trim() );
						m = m.NextMatch ( );
					}

				}
			}
		}

	}
}
