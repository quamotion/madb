using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Net.Sockets;

namespace Managed.Adb {
	public class SyncService {
		private const String OKAY = "OKAY";
		private const String FAIL = "FAIL";
		private const String STAT = "STAT";
		private const String RECV = "RECV";
		private const String DATA = "DATA";
		private const String DONE = "DONE";
		private const String SEND = "SEND";
		private const String LIST = "LIST";
		private const String DENT = "DENT";

		[Flags]
		public enum FileMode {
			UNKNOWN = 0x0000, // unknown
			Socket = 0xc000, // type: socket
			SymbolicLink = 0xa000, // type: symbolic link
			Regular = 0x8000, // type: regular file
			Block = 0x6000, // type: block device
			Directory = 0x4000, // type: directory
			Character = 0x2000, // type: character device
			FIFO = 0x1000  // type: fifo
		}

		/*private const int S_ISOCK = 0xc000; // type: symbolic link
		private const int S_IFLNK = 0xa000; // type: symbolic link
		private const int S_IFREG = 0x8000; // type: regular file
		private const int S_IFBLK = 0x6000; // type: block device
		private const int S_IFDIR = 0x4000; // type: directory
		private const int S_IFCHR = 0x2000; // type: character device
		private const int S_IFIFO = 0x1000; // type: fifo

		private const int S_ISUID = 0x0800; // set-uid bit
		private const int S_ISGID = 0x0400; // set-gid bit
		private const int S_ISVTX = 0x0200; // sticky bit
		private const int S_IRWXU = 0x01C0; // user permissions
		private const int S_IRUSR = 0x0100; // user: read;
		private const int S_IWUSR = 0x0080; // user: write;
		private const int S_IXUSR = 0x0040; // user: execute;
		private const int S_IRWXG = 0x0038; // group permissions
		private const int S_IRGRP = 0x0020; // group: read;
		private const int S_IWGRP = 0x0010; // group: write;
		private const int S_IXGRP = 0x0008; // group: execute;
		private const int S_IRWXO = 0x0007; // other permissions
		private const int S_IROTH = 0x0004; // other: read;
		private const int S_IWOTH = 0x0002; // other: write;
		private const int S_IXOTH = 0x0001; // other: execute;*/

		private const int SYNC_DATA_MAX = 64 * 1024;
		private const int REMOTE_PATH_MAX_LENGTH = 1024;

		#region static members
		static SyncService ( ) {
			NullSyncMonitor = new NullSyncProgressMonitor ( );
		}

		private static NullSyncProgressMonitor NullSyncMonitor { get; set; }

		/// <summary>
		/// Checks the result array starts with the provided code
		/// </summary>
		/// <param name="result">The result array to check</param>
		/// <param name="code">The 4 byte code.</param>
		/// <returns>true if the code matches.</returns>
		private static bool CheckResult ( byte[] result, byte[] code ) {
			if ( result.Length >= code.Length ) {
				for ( int i = 0; i < code.Length; i++ ) {
					if ( result[i] != code[i] ) {
						return false;
					}
				}
				return true;
			}
			return false;
		}

		/// <summary>
		/// Gets the file type from the mode
		/// </summary>
		/// <param name="mode">the file mode flags</param>
		/// <returns></returns>
		private static FileListingService.FileTypes GetFileType ( FileMode mode ) {
			if ( ( mode & FileMode.Socket ) == FileMode.Socket ) {
				return FileListingService.FileTypes.Socket;
			}

			if ( ( mode & FileMode.SymbolicLink ) == FileMode.SymbolicLink ) {
				return FileListingService.FileTypes.Link;
			}

			if ( ( mode & FileMode.Regular ) == FileMode.Regular ) {
				return FileListingService.FileTypes.File;
			}

			if ( ( mode & FileMode.Block ) == FileMode.Block ) {
				return FileListingService.FileTypes.Block;
			}

			if ( ( mode & FileMode.Directory ) == FileMode.Directory ) {
				return FileListingService.FileTypes.Directory;
			}

			if ( ( mode & FileMode.Character ) == FileMode.Character ) {
				return FileListingService.FileTypes.Character;
			}

			if ( ( mode & FileMode.FIFO ) == FileMode.FIFO ) {
				return FileListingService.FileTypes.FIFO;
			}

			return FileListingService.FileTypes.Other;
		}



		/// <summary>
		/// Create a command with a code and an int values
		/// </summary>
		/// <param name="command">the 4 byte command (STAT, RECV, ...).</param>
		/// <param name="value"></param>
		/// <returns>the byte[] to send to the device through adb</returns>
		private static byte[] CreateRequest ( String command, int value ) {
			return CreateRequest ( Encoding.Default.GetBytes ( command ), value );
		}

		/// <summary>
		/// Create a command with a code and an int values
		/// </summary>
		/// <param name="command">the 4 byte command (STAT, RECV, ...).</param>
		/// <param name="value"></param>
		/// <returns>the byte[] to send to the device through adb</returns>
		private static byte[] CreateRequest ( byte[] command, int value ) {
			byte[] array = new byte[8];

			Array.Copy ( command, 0, array, 0, 4 );
			value.Swap32bitsToArray ( array, 4 );

			return array;
		}

		/// <summary>
		/// Creates the data array for a file request. This creates an array with a 4 byte command + the remote file name.
		/// </summary>
		/// <param name="command">the 4 byte command (STAT, RECV, ...).</param>
		/// <param name="path">The path, as a byte array, of the remote file on which to execute the command</param>
		/// <returns>the byte[] to send to the device through adb</returns>
		private static byte[] CreateFileRequest ( String command, String path ) {
			return CreateFileRequest ( Encoding.Default.GetBytes ( command ), Encoding.Default.GetBytes ( path ) );
		}

		/// <summary>
		/// Creates the data array for a file request. This creates an array with a 4 byte command + the remote file name.
		/// </summary>
		/// <param name="command">the 4 byte command (STAT, RECV, ...).</param>
		/// <param name="path">he path, as a byte array, of the remote file on which to execute the command</param>
		/// <returns>the byte[] to send to the device through adb</returns>
		private static byte[] CreateFileRequest ( byte[] command, byte[] path ) {
			byte[] array = new byte[8 + path.Length];

			Array.Copy ( command, 0, array, 0, 4 );
			path.Length.Swap32bitsToArray ( array, 4 );
			Array.Copy ( path, 0, array, 8, path.Length );

			return array;
		}

		private static byte[] CreateSendFileRequest ( String command, String path, FileMode mode ) {
			return CreateSendFileRequest ( Encoding.Default.GetBytes ( command ), Encoding.Default.GetBytes ( path ), mode );
		}

		private static byte[] CreateSendFileRequest ( byte[] command, byte[] path, FileMode mode ) {
			String modeString = String.Format ( ",{0}", ( (int)mode & 0777 ) );
			byte[] modeContent = null;
			try {
				modeContent = Encoding.Default.GetBytes ( modeString );
			} catch ( EncoderFallbackException ) {
				return null;
			}

			byte[] array = new byte[8 + path.Length + modeContent.Length];
			Array.Copy ( command, 0, array, 0, 4 );
			( path.Length + modeContent.Length ).Swap32bitsToArray ( array, 4 );
			Array.Copy ( path, 0, array, 8, path.Length );
			Array.Copy ( modeContent, 0, array, 8 + path.Length, modeContent.Length );

			return array;
		}
		#endregion

		public SyncService ( IPEndPoint address, Device device ) {
			Address = address;
			Device = device;
		}

		public IPEndPoint Address { get; private set; }
		public Device Device { get; private set; }
		private Socket Channel { get; set; }

		public bool OpenSync ( ) {
			try {
				Channel = new Socket ( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );
				Channel.Connect ( this.Address );
				Channel.Blocking = false;

				// target a specific device
				AdbHelper.Instance.SetDevice ( Channel, Device );

				byte[] request = AdbHelper.Instance.FormAdbRequest ( "sync:" );
				AdbHelper.Instance.Write ( Channel, request, -1, DdmPreferences.Timeout );

				AdbResponse resp = AdbHelper.Instance.ReadAdbResponse ( Channel, false /* readDiagString */);

				if ( !resp.IOSuccess || !resp.Okay ) {
					Log.w ( "ddms:syncservice",
									"Got timeout or unhappy response from ADB sync req: "
									+ resp.Message );
					Channel.Close ( );
					Channel = null;
					return false;
				}
			} catch ( IOException e ) {
				if ( Channel != null ) {
					try {
						Channel.Close ( );
					} catch ( IOException ) {
						// we want to throw the original exception, so we ignore this one.
					}
					Channel = null;
				}

				throw e;
			}

			return true;
		}

		/**
     * Closes the connection.
     */
		public void Close ( ) {
			if ( Channel != null ) {
				try {
					Channel.Close ( );
				} catch ( IOException ) {
					// nothing to be done really...
				}
				Channel = null;
			}
		}

		public SyncResult Pull(FileEntry[] entries, String localPath, ISyncProgressMonitor monitor) {
			FileAttributes attributes = File.GetAttributes ( localPath );
			bool isDirectory = (attributes & FileAttributes.Directory) == FileAttributes.Directory;

        // first we check the destination is a directory and exists
        FileInfo f = new FileInfo(localPath);
        if (!f.Exists) {
            return new SyncResult(ErrorCodeHelper.RESULT_NO_DIR_TARGET);
        }
				if ( !isDirectory ) {
					return new SyncResult ( ErrorCodeHelper.RESULT_TARGET_IS_FILE );
        }

        // get a FileListingService object
        FileListingService fls = new FileListingService(Device);

        // compute the number of file to move
        long total = GetTotalRemoteFileSize(entries, fls);

        // start the monitor
        monitor.Start(total);

        SyncResult result = DoPull(entries, localPath, fls, monitor);

        monitor.Stop();

        return result;
    }

		private SyncResult DoPull ( FileEntry[] entries, string localPath, FileListingService fls, ISyncProgressMonitor monitor ) {
			throw new NotImplementedException ( );
		}

		private long GetTotalRemoteFileSize ( FileEntry[] entries, FileListingService fls ) {
			long count = 0;
			foreach ( FileEntry e in entries ) {
				FileListingService.FileTypes type = e.Type;
				if ( type == FileListingService.FileTypes.Directory ) {
					// get the children
					FileEntry[] children = fls.GetChildren ( e, false, null );
					count += GetTotalRemoteFileSize ( children, fls ) + 1;
				} else if ( type == FileListingService.FileTypes.File ) {
					count += e.Size;
				}
			}

			return count;
		}

		/// <summary>
		/// compute the recursive file size of all the files in the list. Folders have a weight of 1.
		/// </summary>
		/// <param name="files"></param>
		/// <returns></returns>
		/// <remarks>This does not check for circular links.</remarks>
		private long GetTotalLocalFileSize ( FileSystemInfo[] fsis ) {
			long count = 0;

			foreach ( FileSystemInfo fsi in fsis ) {
				if ( fsi.Exists ) {
					if ( fsi is DirectoryInfo ) {
						return GetTotalLocalFileSize ( ( fsi as DirectoryInfo ).GetFileSystemInfos ( ) ) + 1;
					} else if ( fsi is FileInfo ) {
						count += ( fsi as FileInfo ).Length;
					}
				}
			}

			return count;
		}

		/// <summary>
		/// Returns the mode of the remote file.
		/// </summary>
		/// <param name="path">the remote file</param>
		/// <returns>the mode if all went well; otherwise, FileMode.UNKNOWN</returns>
		private FileMode ReadMode ( String path ) {
			try {
				// create the stat request message.
				byte[] msg = CreateFileRequest ( STAT, path );

				AdbHelper.Instance.Write ( Channel, msg, -1 /* full length */, DdmPreferences.Timeout );

				// read the result, in a byte array containing 4 ints
				// (id, mode, size, time)
				byte[] statResult = new byte[16];
				AdbHelper.Instance.Read ( Channel, statResult, -1 /* full length */, DdmPreferences.Timeout );

				// check we have the proper data back
				if ( CheckResult ( statResult, Encoding.Default.GetBytes ( STAT ) ) == false ) {
					return FileMode.UNKNOWN;
				}

				// we return the mode (2nd int in the array)
				return (FileMode)statResult.Swap32bitFromArray ( 4 );
			} catch ( IOException e ) {
				Log.w ( "SyncService", e );
				return FileMode.UNKNOWN;
			}
		}
	}
}
