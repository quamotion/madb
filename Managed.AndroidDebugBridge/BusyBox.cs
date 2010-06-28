using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Managed.Adb.IO;

namespace Managed.Adb {
	public class BusyBox {
		private const String BUSYBOX_XBIN = "/system/xbin";
		private const String BUSYBOX_COMMAND = "busybox";

		public BusyBox ( Device device ) {
			this.Device = device;
			CheckForBusyBox ( );
		}

		public bool Install ( String busybox ) {
			FileEntry bb = null;

			try {
				bb = Device.FileListingService.FindFileEntry ( LinuxPath.Combine ( BUSYBOX_XBIN, BUSYBOX_COMMAND ) );
				if ( bb != null && !bb.IsDirectory ) {
					return true;
				}
			} catch {
				// we are just checking if it is already installed so we really expect it to wind up here.
			}

			try {
				MountPoint mp = Device.MountPoints["/system"];
				bool isRO = mp.IsReadOnly;
				Device.RemountMountPoint ( Device.MountPoints["/system"], false );

				FileEntry path = null;
				try {
					path = Device.FileListingService.FindFileEntry ( BUSYBOX_XBIN );
				} catch ( FileNotFoundException ) {
					// path doesn't exist, so we make it.
					Device.FileSystem.MakeDirectory ( BUSYBOX_XBIN );
					// attempt to get the FileEntry after the directory has been made
					path = Device.FileListingService.FindFileEntry ( BUSYBOX_XBIN );
				}

				String bbPath = LinuxPath.Combine ( path.FullPath, BUSYBOX_COMMAND );
				Device.FileSystem.Copy ( busybox, bbPath );
				bb = Device.FileListingService.FindFileEntry ( bbPath );
				Device.FileSystem.Chmod ( bb.FullEscapedPath, "0755" );

				Device.ExecuteShellCommand ( "{0}/busybox --install {0}", NullOutputReceiver.Instance, BUSYBOX_XBIN );

				// check if this path exists in the path already
				if ( Device.EnvironmentVariables.ContainsKey ( "PATH" ) ) {
					String[] paths = Device.EnvironmentVariables["PATH"].Split ( ':' );
					bool found = false;
					foreach ( var tpath in paths ) {
						if ( String.Compare ( tpath, BUSYBOX_XBIN, false ) == 0 ) {
							found = true;
							break;
						}
					}

					// we didnt find it, so add it.
					if ( !found ) {
						Device.ExecuteShellCommand ( "export PATH={0}:$PATH", NullOutputReceiver.Instance, BUSYBOX_XBIN );
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

		private void CheckForBusyBox ( ) {
			if ( this.Device.IsOnline ) {
				try {
					Device.ExecuteShellCommand ( BUSYBOX_COMMAND, NullOutputReceiver.Instance );
					Available = true;
				} catch ( FileNotFoundException ) {
					Available = false;
				}
			} else {
				Available = false;
			}
		}


		private Device Device { get; set; }

		public bool Available { get; private set; }

	}
}
