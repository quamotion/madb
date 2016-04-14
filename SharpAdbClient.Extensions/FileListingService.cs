using SharpAdbClient.DeviceCommands;
using SharpAdbClient.Receivers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace SharpAdbClient
{
    /// <summary>
    /// Provides Device side file listing service.
    /// </summary>
    public class FileListingService: IFileListingService
    {
		/// <summary>
		/// 
		/// </summary>
		public const string APK_FILE_PATTERN = ".*\\.apk";

		/// <summary>
		/// 
		/// </summary>
		public const String PM_FULL_LISTING = "pm list packages -f";

		/// <summary>
		/// This is the pattern that supports busybox ls and toolbox ls.
		/// Groups
		/// <ol>
		///		<li>Permissions</li>
		///		<li>Group</li>
		///		<li>Owner</li>
		///		<li>Size, if empty or whitespace use 0</li>
		///		<li>Month Name (or year in toolbox ls)</li>
		///		<li>Date (or month number in toolbox ls)</li>
		///		<li>Year, if empty, use current year (or date number in toolbox ls)</li>
		///		<li>Time, if empty, use 00:00</li>
		///		<li>Name</li>
		///		<li>File types (or empty in toolbox ls)</li>
		/// </ol>
		/// 
		/// </summary>
		/// <remarks>
		/// added non-capture "(?:\d{1,},\s+)?" for "blocks"
		/// </remarks>
		public const String LS_PATTERN_EX = @"^([bcdlsp-][-r][-w][-xsS][-r][-w][-xsS][-r][-w][-xstST])\s+(?:\d{0,})?\s*(\S+)\s+(\S+)\s+(?:\d{1,},\s+)?(\d{1,}|\s)\s+(\w{3}|\d{4})[\s-](?:\s?(\d{1,2})\s?)[\s-]\s?(?:(\d{2}|\d{4}|\s)\s*)?(\d{2}:\d{2}|\s)\s*(.*?)([/@=*\|]?)$";

		/// <summary>
		///  Top level data folder.
		/// </summary>
		public const String DIRECTORY_DATA = "data";
		/// <summary>
		/// Top level sdcard folder.
		/// </summary>
		public const String DIRECTORY_SDCARD = "sdcard";
		/// <summary>
		/// Top level mount folder.
		/// </summary>
		public const String DIRECTORY_MNT = "mnt";
		/// <summary>
		/// Top level system folder.
		/// </summary>
		public const String DIRECTORY_SYSTEM = "system";
		/// <summary>
		/// Top level temp folder.
		/// </summary>
		public const String DIRECTORY_TEMP = "tmp";
		/// <summary>
		/// Application folder. 
		/// </summary>
		public const String DIRECTORY_APP = "app";

		/// <summary>
		/// 
		/// </summary>
		public const String DIRECTORY_SD = "sdcard";

		/// <summary>
		/// 
		/// </summary>
		public const String DIRECTORY_SDEXT = "sd-ext";


		/// <summary>
		/// 
		/// </summary>
		public const long REFRESH_RATE = 5000L;
		/// <summary>
		/// 
		/// </summary>
		public const long REFRESH_TEST = (long)( REFRESH_RATE * .8 );

		/// <summary>
		/// 
		/// </summary>
		public const String FILE_SEPARATOR = "/";
		/// <summary>
		/// 
		/// </summary>
		public const String FILE_ROOT = "/";

		/// <summary>
		/// 
		/// </summary>
		public const String BUSYBOX_LS = "busybox ls -lFa --color=never {0}";
		/// <summary>
		/// 
		/// </summary>
		public const String TOOLBOX_LS = "ls -la {0}";

		/// <summary>
		/// 
		/// </summary>
		public static readonly String[] RootLevelApprovedItems = {
				DIRECTORY_DATA,
				DIRECTORY_SDCARD,
				DIRECTORY_SYSTEM,
				DIRECTORY_TEMP,
				DIRECTORY_MNT,
				// ADDED
				DIRECTORY_APP
		};

		/// <summary>
		/// 
		/// </summary>
		public enum FileTypes {
			/// <summary>
			/// 
			/// </summary>
			File = 0,
			/// <summary>
			/// 
			/// </summary>
			Directory = 1,
			/// <summary>
			/// 
			/// </summary>
			DirectoryLink = 2,
			/// <summary>
			/// 
			/// </summary>
			Block = 3,
			/// <summary>
			/// 
			/// </summary>
			Character = 4,
			/// <summary>
			/// 
			/// </summary>
			Link = 5,
			/// <summary>
			/// 
			/// </summary>
			Socket = 6,
			/// <summary>
			/// 
			/// </summary>
			FIFO = 7,
			/// <summary>
			/// 
			/// </summary>
			Other = 8
		}

		/// <summary>
		/// 
		/// </summary>
		private FileEntry _root = null;

        private FileSystem fileSystem;

		/// <summary>
		/// Initializes a new instance of the <see cref="FileListingService"/> class.
		/// </summary>
		/// <param name="device">The device.</param>
		public FileListingService ( IDevice device )
        {
            this.Device = device;
            this.fileSystem = new FileSystem(this.Device);
            this.Threads = new List<Thread>();
        }

        /// <include file='.\FileListingService.xml' path='/FileListingService/Device/*'/>
        public IDevice Device { get; private set; }

        /// <include file='.\FileListingService.xml' path='/FileListingService/Root/*'/>
		public FileEntry Root {
			get {
				if ( Device != null ) {
					if ( _root == null ) {
						_root = new FileEntry ( this.fileSystem, null /* parent */, string.Empty /* name */, FileTypes.Directory, true /* isRoot */ );
					}
					return _root;
				}
				return null;
			}
			private set {
				_root = value;
			}
		}


        /// <summary>
        /// Gets the file type from the mode
        /// </summary>
        /// <param name="mode">the file mode flags</param>
        /// <returns></returns>
        private static FileTypes GetFileType(UnixFileMode mode)
        {
            if ((mode & UnixFileMode.Socket) == UnixFileMode.Socket)
            {
                return FileListingService.FileTypes.Socket;
            }

            if ((mode & UnixFileMode.SymbolicLink) == UnixFileMode.SymbolicLink)
            {
                return FileListingService.FileTypes.Link;
            }

            if ((mode & UnixFileMode.Regular) == UnixFileMode.Regular)
            {
                return FileListingService.FileTypes.File;
            }

            if ((mode & UnixFileMode.Block) == UnixFileMode.Block)
            {
                return FileListingService.FileTypes.Block;
            }

            if ((mode & UnixFileMode.Directory) == UnixFileMode.Directory)
            {
                return FileListingService.FileTypes.Directory;
            }

            if ((mode & UnixFileMode.Character) == UnixFileMode.Character)
            {
                return FileListingService.FileTypes.Character;
            }

            if ((mode & UnixFileMode.FIFO) == UnixFileMode.FIFO)
            {
                return FileListingService.FileTypes.FIFO;
            }

            return FileListingService.FileTypes.Other;
        }

        /// <include file='.\FileListingService.xml' path='/FileListingService/GetChildren/*'/>
		public FileEntry[] GetChildren ( FileEntry entry, bool useCache, IListingReceiver receiver ) {
			// first thing we do is check the cache, and if we already have a recent
			// enough children list, we just return that.
			if ( useCache && !entry.NeedFetch ) {
				return entry.Children.ToArray ( );
			}

			// if there's no receiver, then this is a synchronous call, and we
			// return the result of ls
			if ( receiver == null ) {
				DoLS ( entry );
				return entry.Children.ToArray ( );
			}

			// this is a asynchronous call.
			// we launch a thread that will do ls and give the listing
			// to the receiver
			Thread t = new Thread ( new ParameterizedThreadStart ( delegate ( object stateData ) {
				var state = stateData as ThreadState;

				DoLS ( entry );

				receiver.SetChildren ( state.Entry, state.Entry.Children.ToArray ( ) );

				FileEntry[] children = state.Entry.Children.ToArray ( );
				if ( children.Length > 0 && children[0].IsApplicationPackage ) {
					var map = new Dictionary<String, FileEntry> ( );

					foreach(var child in children)
                    {
						map.Add ( child.FullPath, child );
					}
				}


				// if another thread is pending, launch it
				lock ( Threads ) {
					// first remove ourselves from the list
					Threads.Remove ( state.Thread );

					// then launch the next one if applicable.
					if ( Threads.Count > 0 ) {
						Thread ct = Threads[0];
						ct.Start ( new ThreadState { Thread = ct, Entry = entry } );
					}
				}

			} ) );
			t.Name = "ls " + entry.FullPath;

			// we don't want to run multiple ls on the device at the same time, so we
			// store the thread in a list and launch it only if there's no other thread running.
			// the thread will launch the next one once it's done.
			lock ( Threads ) {
				// add to the list
				Threads.Add ( t );

				// if it's the only one, launch it.
				if ( Threads.Count == 1 ) {
					t.Start ( new ThreadState { Thread = t } );
				}
			}

			// and we return null.
			return null;
		}

		/// <summary>
		/// Gets or sets the threads.
		/// </summary>
		/// <value>The threads.</value>
		private List<Thread> Threads { get; set; }

		/// <summary>
		/// Does the LS.
		/// </summary>
		/// <param name="entry">The entry.</param>
		private void DoLS ( FileEntry entry ) {
			// create a list that will receive the list of the entries
			List<FileEntry> entryList = new List<FileEntry> ( );

			// create a list that will receive the link to compute post ls;
			List<String> linkList = new List<String> ( );

			try {
				// create the command
				String command = String.Format ( TOOLBOX_LS, entry.FullPath );
				// create the receiver object that will parse the result from ls
				ListingServiceReceiver receiver = new ListingServiceReceiver ( entry, entryList, linkList );

				// call ls.
				Device.ExecuteShellCommand ( command, receiver );

				// finish the process of the receiver to handle links
				receiver.FinishLinks ( );
			} catch ( IOException e ) {
				Log.Error ( "ddms", e );
				throw;
			}


			// at this point we need to refresh the viewer
			entry.FetchTime = DateTime.Now.ToUnixEpoch();
			// sort the children and set them as the new children
			entryList.Sort ( new FileEntry.FileEntryComparer ( ) );
			entry.Children = entryList;
		}

		/// <summary>
		/// 
		/// </summary>
		private class ThreadState {
			/// <summary>
			/// 
			/// </summary>
			public Thread Thread;
			/// <summary>
			/// 
			/// </summary>
			public FileEntry Entry;

		}

        /// <include file='.\FileListingService.xml' path='/FileListingService/FindFileEntry/*'/>
		public FileEntry FindFileEntry ( String path ) {
			return FindFileEntry ( this.Root, path );
		}

        /// <include file='.\FileListingService.xml' path='/FileListingService/FindFileEntry2/*'/>
        public FileEntry FindFileEntry ( FileEntry parent, String path ) {
			var rpath = this.fileSystem.ResolveLink ( path );
			var entriesString = rpath.Split ( new char[] { LinuxPath.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries );
			FileEntry current = parent;


			foreach ( var pathItem in entriesString ) {
				FileEntry[] entries = GetChildren ( current, true, null );
				foreach ( var e in entries ) {
					if ( String.Compare ( e.Name, pathItem, false ) == 0 ) {
						current = e;
						break;
					}
				}
			}

			// better checking if the file is the "same" based on the link or the reference
			if ( ( String.Compare ( current.FullPath, path, false ) == 0 ||
				String.Compare ( current.FullResolvedPath, path, false ) == 0 ||
				String.Compare ( current.FullPath, rpath, false ) == 0 ||
				String.Compare ( current.FullResolvedPath, rpath, false ) == 0 ) ) {
				return current;
			} else {
				throw new FileNotFoundException ( String.Format ( "Unable to locate {0}", path ) );
			}
		}
	}
}
