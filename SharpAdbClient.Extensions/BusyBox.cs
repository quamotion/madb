using SharpAdbClient.DeviceCommands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SharpAdbClient
{
    /// <summary>
    /// A class to help with working with BusyBox
    /// </summary>
    public class BusyBox : IBusyBox
    {
        private IFileListingService fileListingService;
        private FileSystem fileSystem;

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
		public BusyBox ( IDevice device ) {
			this.Device = device;
            this.fileSystem = new FileSystem(this.Device);
            this.fileListingService = new FileListingService(this.Device);
			Version = new System.Version ( "0.0.0.0" );
			Commands = new List<String> ( );
			CheckForBusyBox ( );

		}

        /// <include file='.\BusyBox.xml' path='/BusyBox/Install/*'/>
        public bool Install ( String busybox ) {
			busybox.ThrowIfNullOrWhiteSpace ( "busybox" );

			FileEntry bb = null;

			try {
				Device.ExecuteShellCommand ( BUSYBOX_COMMAND, null );
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
					path = this.fileListingService.FindFileEntry ( BUSYBOX_BIN );
				} catch ( FileNotFoundException ) {
					// path doesn't exist, so we make it.
					this.fileSystem.MakeDirectory ( BUSYBOX_BIN );
					// attempt to get the FileEntry after the directory has been made
					path = this.fileListingService.FindFileEntry ( BUSYBOX_BIN );
				}

				this.fileSystem.Chmod ( path.FullPath, "0755" );

				String bbPath = LinuxPath.Combine ( path.FullPath, BUSYBOX_COMMAND );

				this.fileSystem.Copy ( busybox, bbPath );


				bb = this.fileListingService.FindFileEntry ( bbPath );
				this.fileSystem.Chmod ( bb.FullPath, "0755" );

				Device.ExecuteShellCommand ( "{0}/busybox --install {0}", new ConsoleOutputReceiver ( ), path.FullPath );

				// check if this path exists in the path already
				if ( Device.EnvironmentVariables.ContainsKey ( "PATH" ) ) {
					var paths = Device.EnvironmentVariables["PATH"].Split ( ':' );
					var found = paths.Where ( p => String.Compare ( p, BUSYBOX_BIN, false ) == 0 ).Count ( ) > 0;

					// we didnt find it, so add it.
					if ( !found ) {
						// this doesn't seem to actually work
						Device.ExecuteShellCommand ( @"echo \ Mad Bee buxybox >> /init.rc", null );
						Device.ExecuteShellCommand ( @"echo export PATH={0}:\$PATH >> /init.rc", null, BUSYBOX_BIN );
					}
				}


				if ( mp.IsReadOnly != isRO ) {
					// Put it back, if we changed it
					Device.RemountMountPoint ( mp, isRO );
				}

				Device.ExecuteShellCommand ( "sync", null );
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

        /// <include file='.\BusyBox.xml' path='/BusyBox/ExecuteShellCommand/*'/>
        public void ExecuteShellCommand( String command, IShellOutputReceiver receiver, params object[] commandArgs ) {
			command.ThrowIfNullOrWhiteSpace ( "command" );
			var cmd = String.Format ( "{0} {1}", BUSYBOX_COMMAND, String.Format ( command, commandArgs ) );
			Log.Debug ( "executing: {0}", cmd );
			AdbClient.Instance.ExecuteRemoteCommand(cmd, this.Device.DeviceData, receiver);
		}

        /// <include file='.\BusyBox.xml' path='/BusyBox/ExecuteRootShellCommand/*'/>
        public void ExecuteRootShellCommand( String command, IShellOutputReceiver receiver, params object[] commandArgs ) {
			command.ThrowIfNullOrWhiteSpace ( "command" );
			var cmd = String.Format ( "{0} {1}", BUSYBOX_COMMAND, String.Format ( command, commandArgs ) );
			Log.Debug ( "executing (su): {0}", cmd );
            Device.ExecuteRootShellCommand(cmd, receiver );
		}

        /// <include file='.\BusyBox.xml' path='/BusyBox/Device/*'/>
        public IDevice Device { get; set; }

        /// <include file='.\BusyBox.xml' path='/BusyBox/Available/*'/>
		public bool Available { get; private set; }

        /// <include file='.\BusyBox.xml' path='/BusyBox/Version/*'/>
		public Version Version { get; internal set; }

        /// <include file='.\BusyBox.xml' path='/BusyBox/Commands/*'/>
        public List<String> Commands { get; private set; }

        /// <include file='.\BusyBox.xml' path='/BusyBox/Supports/*'/>
        public bool Supports ( String command ) {
			command.ThrowIfNullOrWhiteSpace ( "command" );

			if ( Available && ( Commands == null || Commands.Count == 0 ) ) {
				CheckForBusyBox ( );
			}

			return Commands.Where( c => String.Compare(c,command,false) == 0).FirstOrDefault() != null;
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
			private const String BB_FUNCTIONS_PATTERN = @"(?:([\[a-z0-9]+)(?:,\s*))";

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
			/// <workitem id="16000">Issues w/ BusyBox.cs/ProcessNewLines()</workitem>
			protected override void ProcessNewLines ( IEnumerable<string> lines ) {
				Match match = null;
				int state = 0;
				foreach ( var line in lines ) {
					if ( string.IsNullOrEmpty(line) ) {
						continue;
					}
					switch ( state ) {
						case 0:
							match = line.Match ( BB_VERSION_PATTERN, RegexOptions.Compiled | RegexOptions.IgnoreCase );
							if ( match.Success ) {
								BusyBox.Version = new Version ( match.Groups[1].Value );
								state = 1;
								continue;
							}
							break;
						case 1:
							if ( line.Contains ( "defined functions" ) ) {
								state = 2;
							}
							break;
						case 2:
							match = line.Trim ( ).Match ( BB_FUNCTIONS_PATTERN, RegexOptions.Compiled );
							while ( match.Success ) {
								BusyBox.Commands.Add ( match.Groups[1].Value.Trim ( ) );
								match = match.NextMatch ( );
							}
							break;
					}
				}
			}
		}

	}
}
