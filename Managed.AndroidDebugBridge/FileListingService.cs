using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Text.RegularExpressions;
using Managed.Adb.IO;

namespace Managed.Adb {
	/// <summary>
	/// Provides Device side file listing service.
	/// </summary>
	public class FileListingService {
		/// <summary>
		/// 
		/// </summary>
		public const string APK_FILE_PATTERN = ".*\\.apk";

		/// <summary>
		/// 
		/// </summary>
		public const String PM_FULL_LISTING = "pm list packages -f";

		// mine is better, it supports both toolbox ls and busybox ls
		/// <summary>
		/// 
		/// </summary>
		[Obsolete ( "Use LS_PATTERN_EX, it supports busybox, plus standard ls", true )]
		public const String LS_PATTERN = "^([bcdlsp-][-r][-w][-xsS][-r][-w][-xsS][-r][-w][-xstST])\\s+(\\S+)\\s+(\\S+)\\s+([\\d\\s,]*)\\s+(\\d{4}-\\d\\d-\\d\\d)\\s+(\\d\\d:\\d\\d)\\s+(.*)$";

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
		public const String BUSYBOX_LS = "busybox ls -lF --color=never {0}";
		/// <summary>
		/// 
		/// </summary>
		public const String TOOLBOX_LS = "ls -l {0}";

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

		/// <summary>
		/// Initializes a new instance of the <see cref="FileListingService"/> class.
		/// </summary>
		/// <param name="device">The device.</param>
		/// <param name="forceBusyBox">if set to <c>true</c> [force busy box].</param>
		public FileListingService ( Device device, bool forceBusyBox ) {
			this.Device = device;
			this.Threads = new List<Thread> ( );
			this.ForceBusyBox = forceBusyBox && device.BusyBox.Available;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="FileListingService"/> class.
		/// </summary>
		/// <param name="device">The device.</param>
		public FileListingService ( Device device )
			: this ( device, device.BusyBox.Available ) {

		}

		/// <summary>
		/// Gets or sets the device.
		/// </summary>
		/// <value>The device.</value>
		public Device Device { get; private set; }
		/// <summary>
		/// Gets or sets the root.
		/// </summary>
		/// <value>The root.</value>
		public FileEntry Root {
			get {
				if ( Device != null ) {
					if ( _root == null ) {
						_root = new FileEntry (this.Device,  null /* parent */, string.Empty /* name */, FileTypes.Directory, true /* isRoot */ );
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
		/// Gets or sets a value indicating whether [force busy box].
		/// </summary>
		/// <value><c>true</c> if [force busy box]; otherwise, <c>false</c>.</value>
		public bool ForceBusyBox { get; set; }


		/// <summary>
		/// Gets the children.
		/// </summary>
		/// <param name="entry">The entry.</param>
		/// <param name="useCache">if set to <c>true</c> [use cache].</param>
		/// <param name="receiver">The receiver.</param>
		/// <returns></returns>
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
				ThreadState state = stateData as ThreadState;

				DoLS ( entry );

				receiver.SetChildren ( state.Entry, state.Entry.Children.ToArray ( ) );

				FileEntry[] children = state.Entry.Children.ToArray ( );
				if ( children.Length > 0 && children[0].IsApplicationPackage ) {
					Dictionary<String, FileEntry> map = new Dictionary<String, FileEntry> ( );

					foreach ( FileEntry child in children ) {
						String path = child.FullPath;
						map.Add ( path, child );
					}

					// call pm.
					String command = PM_FULL_LISTING;
					try {
						this.Device.ExecuteShellCommand ( command, new PackageManagerListingReceiver ( map, receiver ) );
					} catch ( IOException e ) {
						// adb failed somehow, we do nothing.
						Log.e ( "FileListingService", e );
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
				String command = String.Format ( ForceBusyBox ? BUSYBOX_LS : TOOLBOX_LS, entry.FullPath );
				// create the receiver object that will parse the result from ls
				ListingServiceReceiver receiver = new ListingServiceReceiver ( entry, entryList, linkList );

				// call ls.
				Device.ExecuteShellCommand ( command, receiver );

				// finish the process of the receiver to handle links
				receiver.FinishLinks ( );
			} catch ( IOException e ) {
				Log.e ( "ddms", e );
				throw;
			}


			// at this point we need to refresh the viewer
			entry.FetchTime = DateTime.Now.CurrentTimeMillis ( );
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

		/// <summary>
		/// Finds an entry from the path.
		/// </summary>
		/// <param name="path">The file path of</param>
		/// <returns>The FileEntry</returns>
		/// <exception cref="FileNotFoundException">Throws if unable to locate the file or directory</exception>
		public FileEntry FindFileEntry ( String path ) {
			return FindFileEntry ( this.Root, path );
		}

		/// <summary>
		/// Finds the file entry.
		/// </summary>
		/// <param name="parent">The parent.</param>
		/// <param name="path">The path.</param>
		/// <returns></returns>
		public FileEntry FindFileEntry ( FileEntry parent, String path ) {
			String[] entriesString = path.Split ( new char[] { LinuxPath.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries );
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
			if ( String.Compare ( current.FullPath, path, false ) == 0 || String.Compare ( current.FullResolvedPath, path, false ) == 0 ||
				( String.Compare(current.LinkName,path,false) == 0 && current.IsLink ) ) {
				return current;
			} else {
				throw new FileNotFoundException ( String.Format ( "Unable to locate {0}", path ) );
			}
		}
	}
}
