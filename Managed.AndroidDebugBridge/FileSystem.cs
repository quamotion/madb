using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Managed.Adb {
	public class FileSystem {

		public FileSystem ( Device device ) {
			Device = device;
		}

		private Device Device { get; set; }

		public void MakeDirectory ( String path ) {
			CommandErrorReceiver cer = new CommandErrorReceiver ( );
			Device.ExecuteShellCommand ( "mkdir {0}", cer, path );
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
			FileEntry entry = Device.FileListingService.FindFileEntry(path);
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
			if ( !Device.MountPoints.ContainsKey(mount)) {
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

	}
}
