using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Managed.Adb.Extensions {
	public static class FileInfoHelper {

		public static bool IsDirectory ( String path ) {
			return File.Exists ( path ) && ( File.GetAttributes ( path ) & FileAttributes.Directory ) == FileAttributes.Directory;
		}

		public static bool IsDirectory ( this FileSystemInfo fsi ) {
			return (fsi.Attributes & FileAttributes.Directory) == FileAttributes.Directory;
		}

		public static bool IsFile ( String path ) {
			return File.Exists ( path ) && !IsDirectory ( path );
		}

		public static bool IsFile ( this FileSystemInfo fsi ) {
			return fsi is FileInfo && !IsDirectory ( fsi.FullName );
		}

		public static FileSystemInfo GetFileSystemInfo ( this String path ) {
			if ( IsDirectory ( path ) ) {
				return new DirectoryInfo ( path );
			} else {
				return new FileInfo ( path );
			}
		}

	}
}
