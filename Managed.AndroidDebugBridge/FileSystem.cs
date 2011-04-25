using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Managed.Adb.IO;

namespace Managed.Adb {
	public class FileSystem {

		public FileSystem ( Device device ) {
			Device = device;
		}

		private Device Device { get; set; }

		public void MakeDirectory ( String path ) {
			CommandErrorReceiver cer = new CommandErrorReceiver ( );
			try {
				string[] segs = path.Split ( new char[] { LinuxPath.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries );
				FileEntry current = Device.FileListingService.Root;
				foreach ( var pathItem in segs ) {
					FileEntry[] entries = Device.FileListingService.GetChildren ( current, true, null );
					bool found = false;
					foreach ( var e in entries ) {
						if ( String.Compare ( e.Name, pathItem, false ) == 0 ) {
							current = e;
							found = true;
							break;
						}
					}

					if ( !found ) {
						Device.ExecuteShellCommand ( "mkdir {0}", cer, LinuxPath.Combine ( current.FullPath, pathItem ) );
					}
				}
			} catch {

			}
			if ( !String.IsNullOrEmpty ( cer.ErrorMessage ) ) {
				throw new IOException ( cer.ErrorMessage );
			}
		}

		public void Copy ( String source, String destination ) {
			CommandErrorReceiver cer = new CommandErrorReceiver ( );
			FileEntry sfe = Device.FileListingService.FindFileEntry ( source );

			Device.ExecuteShellCommand ( "cat {0} > {1}", cer, sfe.FullEscapedPath, destination );
			if ( !String.IsNullOrEmpty ( cer.ErrorMessage ) ) {
				throw new IOException ( cer.ErrorMessage );
			}
		}

		public void Chmod ( String path, String permissions ) {
			FileEntry entry = Device.FileListingService.FindFileEntry ( path );
			CommandErrorReceiver cer = new CommandErrorReceiver ( );
			Device.ExecuteShellCommand ( "chmod {0} {1}", cer, permissions, entry.FullEscapedPath );
		}

		/// <summary>
		/// Gets if the specified mount point is read-only
		/// </summary>
		/// <param name="mount"></param>
		/// <returns><code>true</code>, if read-only; otherwise, <code>false</code></returns>
		/// <exception cref="IOException">If mount point doesnt exist</exception>
		public bool IsMountPointReadOnly ( String mount ) {
			if ( !Device.MountPoints.ContainsKey ( mount ) ) {
				throw new IOException ( "Invalid mount point" );
			}

			return Device.MountPoints[mount].IsReadOnly;
		}

		public void Delete ( String path ) {
			CommandErrorReceiver cer = new CommandErrorReceiver ( );
			FileEntry entry = Device.FileListingService.FindFileEntry ( path );
			if ( entry != null ) {
				Device.ExecuteShellCommand ( "rm -f {0} {1}", cer, entry.IsDirectory ? "-r" : String.Empty, entry.FullEscapedPath );
			}

			if ( !String.IsNullOrEmpty ( cer.ErrorMessage ) ) {
				throw new IOException ( cer.ErrorMessage );
			}
		}

		public void Mount ( MountPoint mp, String options ) {
			CommandErrorReceiver cer = new CommandErrorReceiver ( );
			if ( Device.BusyBox.Available ) {
				Device.ExecuteShellCommand ( "busybox mount {0} {4} -t {1} {2} {3}", cer, mp.IsReadOnly ? "-r" : "-w", mp.FileSystem, mp.Block, mp.Name, !String.IsNullOrEmpty(options) ? String.Format("-o {0}",options) : String.Empty);
			} else {
				Device.ExecuteShellCommand ( "mount {0} {4} -t {1} {2} {3}", cer, mp.IsReadOnly ? "-r" : "-w", mp.FileSystem, mp.Block, mp.Name, !String.IsNullOrEmpty ( options ) ? String.Format ( "-o {0}", options ) : String.Empty );
			}
		}

		public void Mount ( MountPoint mp ) {
			Mount ( mp, String.Empty );
		}

		public void Mount ( String directory, String device, String fileSytemType, bool isReadOnly, String options ) {
			Mount ( new MountPoint ( device, directory, fileSytemType, isReadOnly ), options );
		}

		public void Mount ( String directory, String device, String fileSytemType, bool isReadOnly ) {
			Mount ( new MountPoint ( device, directory, fileSytemType, isReadOnly ), String.Empty );
		}

		public void Unmount ( MountPoint mp ) {
			Unmount ( mp, String.Empty );
		}


		public void Unmount ( MountPoint mp, String options ) {
			Unmount ( mp.Name, options );
		}

		public void Unmount ( String mountPoint ) {
			Unmount ( mountPoint, String.Empty );
		}

		public void Unmount ( String mountPoint, String options ) {
			CommandErrorReceiver cer = new CommandErrorReceiver ( );
			if ( Device.BusyBox.Available ) {
				Device.ExecuteShellCommand ( "busybox umount {1} {0}", cer, !String.IsNullOrEmpty ( options ) ? String.Format ( "-o {0}", options ) : String.Empty, mountPoint );
			} else {
				Device.ExecuteShellCommand ( "umount {1} {0}", cer, !String.IsNullOrEmpty ( options ) ? String.Format ( "-o {0}", options ) : String.Empty, mountPoint );
			}
		}

	}
}
