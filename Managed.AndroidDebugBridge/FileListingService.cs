using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using Managed.Adb.Extensions;
using System.Text.RegularExpressions;

namespace Managed.Adb {
	/// <summary>
	/// Provides Device side file listing service.
	/// </summary>
	public class FileListingService {
		public const string APK_FILE_PATTERN = ".*\\.apk";

		public const String PM_FULL_LISTING = "pm list packages -f";

		// mine is better
		public const String LS_PATTERN = "^([bcdlsp-][-r][-w][-xsS][-r][-w][-xsS][-r][-w][-xstST])\\s+(\\S+)\\s+(\\S+)\\s+([\\d\\s,]*)\\s+(\\d{4}-\\d\\d-\\d\\d)\\s+(\\d\\d:\\d\\d)\\s+(.*)$";

		/// <summary>
		/// This is the busy box ls pattern (busybox ls -lF --color=never)
		/// </summary>
		public const String BB_LS_PATTERN = "^([ldcbp-])([rwxt-]{3}){3}\\s+(\\d{1,})\\s+(\\d{1,}|\\S{1,})\\s+(\\d{1,}|\\S{1,})\\s+(\\d{1,})\\s+(\\w+\\s+\\d{1,2}\\s+(?:\\d{4})?)(\\d{2}:\\d{2})?\\s+(.+?)([/@=*\\|]?)\\s?$";

		public const String LS_PATTERN_EX = @"^([bcdlsp-][-r][-w][-xsS][-r][-w][-xsS][-r][-w][-xstST])\s+(?:\d{0,})?\s*(\S+)\s+(\S+)\s+(\d{1,})[\s-](\w{3}|\d{2})[\s-](\d{2})\s+(\d{1,2}:\d{2})\s+(.*)([/@=*\|]?)$";

		/// <summary>
		///  Top level data folder.
		/// </summary>
		public const String DIRECTORY_DATA = "data"; //$NON-NLS-1$
		/// <summary>
		/// Top level sdcard folder.
		/// </summary>
		public const String DIRECTORY_SDCARD = "sdcard"; //$NON-NLS-1$
		/// <summary>
		/// Top level mount folder.
		/// </summary>
		public const String DIRECTORY_MNT = "mnt"; //$NON-NLS-1$
		/// <summary>
		/// Top level system folder.
		/// </summary>
		public const String DIRECTORY_SYSTEM = "system"; //$NON-NLS-1$
		/// <summary>
		/// Top level temp folder.
		/// </summary>
		public const String DIRECTORY_TEMP = "tmp"; //$NON-NLS-1$
		/// <summary>
		/// Application folder. 
		/// </summary>
		public const String DIRECTORY_APP = "app"; //$NON-NLS-1$


		public const long REFRESH_RATE = 5000L;
		public const long REFRESH_TEST = (long)( REFRESH_RATE * .8 );

		public const String FILE_SEPARATOR = "/";
		public const String FILE_ROOT = "/";

		public static readonly String[] RootLevelApprovedItems = {
        DIRECTORY_DATA,
        DIRECTORY_SDCARD,
        DIRECTORY_SYSTEM,
        DIRECTORY_TEMP,
        DIRECTORY_MNT,
				// ADDED
				DIRECTORY_APP
    };

		public enum FileTypes {
			File = 0,
			Directory = 1,
			DirectoryLink = 2,
			Block = 3,
			Character = 4,
			Link = 5,
			Socket = 6,
			FIFO = 7,
			Other = 8
		}

		private FileEntry _root = null;

		public FileListingService ( Device device ) {
			this.Device = device;
			this.Threads = new List<Thread> ( );
		}

		public Device Device { get; private set; }
		public FileEntry Root {
			get {
				if ( Device != null ) {
					if ( _root == null ) {
						_root = new FileEntry ( null /* parent */, string.Empty /* name */, FileTypes.Directory, true /* isRoot */ );
					}
					return _root;
				}
				return null;
			}
			private set {
				_root = value;
			}
		}



		public FileEntry[] GetChildren ( FileEntry entry, bool useCache, IListingReceiver receiver ) {
			// first thing we do is check the cache, and if we already have a recent
			// enough children list, we just return that.
			if ( useCache && !entry.NeedFetch ) {
				return entry.Children.ToArray ( );
			}

			// if there's no receiver, then this is a synchronous call, and we
			// return the result of ls
			if ( receiver == null ) {
				Console.WriteLine ( "No receiver" );
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
						this.Device.ExecuteShellCommand ( command, new PackageManagerReceiver ( map, receiver ) );
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

		private List<Thread> Threads { get; set; }

		private void DoLS ( FileEntry entry ) {
			// create a list that will receive the list of the entries
			List<FileEntry> entryList = new List<FileEntry> ( );

			// create a list that will receive the link to compute post ls;
			List<String> linkList = new List<String> ( );

			try {
				// create the command
				String command = "ls -l " + entry.FullPath; //$NON-NLS-1$

				// create the receiver object that will parse the result from ls
				ListingServiceReceiver receiver = new ListingServiceReceiver ( entry, entryList, linkList );

				// call ls.
				Device.ExecuteShellCommand ( command, receiver );

				// finish the process of the receiver to handle links
				receiver.FinishLinks ( );
			} catch ( IOException e ) {
				Console.WriteLine ( e.ToString ( ) );
			}


			// at this point we need to refresh the viewer
			entry.FetchTime = DateTime.Now.CurrentTimeMillis ( );

			// sort the children and set them as the new children
			entryList.Sort ( new FileEntry.FileEntryComparer ( ) );
			entry.Children = entryList;
		}

		private class ThreadState {
			public Thread Thread;
			public FileEntry Entry;

		}

		internal class PackageManagerReceiver : MultiLineReceiver {
			/// <summary>
			/// Pattern to parse the output of the 'pm -lf' command.
			/// The output format looks like:
			/// /data/app/myapp.apk=com.mypackage.myapp
			/// </summary>
			private const String PM_PATTERN = "^package:(.+?)=(.+)$";


			public PackageManagerReceiver ( Dictionary<String,FileEntry> entryMap, IListingReceiver receiver ) {
				this.Map = entryMap;
				this.Receiver = receiver;
			}

			public Dictionary<String, FileEntry> Map { get; set; }
			public IListingReceiver Receiver { get; set; }

			public override void ProcessNewLines ( string[] lines ) {
				foreach ( String line in lines ) {
					if ( line.Length > 0 ) {
						// get the filepath and package from the line
						Match m = new Regex ( PM_PATTERN, RegexOptions.Compiled ).Match ( line );
						if ( m.Success ) {
							// get the children with that path
							FileEntry entry = Map[m.Groups[1].Value];
							if ( entry != null ) {
								entry.Info = m.Groups[2].Value;
								Receiver.RefreshEntry ( entry );
							}
						}
					}
				}

			}
		}
	}
}
