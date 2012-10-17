using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;

namespace Managed.Adb {
	/// <summary>
	/// 
	/// </summary>
	public class ListingServiceReceiver : MultiLineReceiver {
		/// <summary>
		/// 
		/// </summary>
		private const String LINK_FORMAT = "-> {0}";
		/// <summary>
		/// Create an ls receiver/parser.
		/// </summary>
		/// <param name="parent">The list of current children. To prevent collapse during update, reusing the same 
		/// FileEntry objects for files that were already there is paramount.</param>
		/// <param name="entries">the list of new children to be filled by the receiver.</param>
		/// <param name="links">the list of link path to compute post ls, to figure out if the link 
		/// pointed to a file or to a directory.</param>
		public ListingServiceReceiver ( FileEntry parent, List<FileEntry> entries, List<String> links ) {
			Parent = parent;
			Entries = entries ?? new List<FileEntry>();
			Links = links ?? new List<String> ( );
			CurrentChildren = Parent.Children.ToArray ( );
		}

		/// <summary>
		/// Gets or sets the entries.
		/// </summary>
		/// <value>The entries.</value>
		public List<FileEntry> Entries { get; private set; }
		/// <summary>
		/// Gets or sets the links.
		/// </summary>
		/// <value>The links.</value>
		public List<String> Links { get; private set; }
		/// <summary>
		/// Gets or sets the current children.
		/// </summary>
		/// <value>The current children.</value>
		public FileEntry[] CurrentChildren { get; private set; }
		/// <summary>
		/// Gets or sets the parent.
		/// </summary>
		/// <value>The parent.</value>
		public FileEntry Parent { get; private set; }

		/// <summary>
		/// Processes the new lines.
		/// </summary>
		/// <param name="lines">The lines.</param>
		protected override void ProcessNewLines ( string[] lines ) {
			foreach ( String line in lines ) {
				// no need to handle empty lines.
				if ( line.Length == 0 ) {
					continue;
				}
				// run the line through the regexp
				var m = line.Trim ( ).Match ( FileListingService.LS_PATTERN_EX, RegexOptions.Compiled );
				if ( !m.Success ) {
					Log.v ( "madb", "no match on file pattern: {0}", line );
					continue;
				}
				// get the name
				String name = m.Groups[9].Value;

				if ( String.Compare ( name, ".", true ) == 0 || String.Compare ( name, "..", true ) == 0 ) {
					// we don't care if the entry is a "." or ".."
					continue;
				}

				// get the rest of the groups
				String permissions = m.Groups[1].Value;
				String owner = m.Groups[2].Value;
				String group = m.Groups[3].Value;
				bool isExec = String.Compare ( m.Groups[10].Value, "*", true ) == 0;
				long size = 0;
				String sizeData = m.Groups[4].Value.Trim ( );
				long.TryParse ( String.IsNullOrEmpty ( sizeData ) ? "0" : sizeData, out size );
				String date1 = m.Groups[5].Value.Trim ( );
				String date2 = m.Groups[6].Value.Trim ( );
				String date3 = m.Groups[7].Value.Trim ( );

				DateTime date = DateTime.Now.GetEpoch ( );
				String time = m.Groups[8].Value.Trim();
				if ( String.IsNullOrEmpty ( time ) ) {
					time = date.ToString ( "HH:mm" );
				}
				if ( date1.Length == 3 ) {
					// check if we don't have a year and use current if we don't
					String tyear = String.IsNullOrEmpty ( date3 ) ? DateTime.Now.Year.ToString ( ) : date3;
					date = DateTime.ParseExact ( String.Format ( "{0}-{1}-{2} {3}", date1, date2.PadLeft(2,'0'), tyear, time ), "MMM-dd-yyyy HH:mm", CultureInfo.CurrentCulture );
				} else if ( date1.Length == 4 ) {
					date = DateTime.ParseExact ( String.Format ( "{0}-{1}-{2} {3}", date1, date2.PadLeft ( 2, '0' ), date3, time ), "yyyy-MM-dd HH:mm", CultureInfo.CurrentCulture );
				}

				String info = null;
				String linkName = null;

				// and the type
				FileListingService.FileTypes objectType = FileListingService.FileTypes.Other;
				switch ( permissions[0] ) {
					case '-':
						objectType = FileListingService.FileTypes.File;
						break;
					case 'b':
						objectType = FileListingService.FileTypes.Block;
						break;
					case 'c':
						objectType = FileListingService.FileTypes.Character;
						break;
					case 'd':
						objectType = FileListingService.FileTypes.Directory;
						break;
					case 'l':
						objectType = FileListingService.FileTypes.Link;
						break;
					case 's':
						objectType = FileListingService.FileTypes.Socket;
						break;
					case 'p':
						objectType = FileListingService.FileTypes.FIFO;
						break;
				}


				// now check what we may be linking to
				if ( objectType == FileListingService.FileTypes.Link ) {
					String[] segments = name.Split ( new string[] { " -> " }, StringSplitOptions.RemoveEmptyEntries );
					// we should have 2 segments
					if ( segments.Length == 2 ) {
						// update the entry name to not contain the link
						name = segments[0];
						// and the link name
						info = segments[1];

						// now get the path to the link
						String[] pathSegments = info.Split ( new String[] { FileListingService.FILE_SEPARATOR }, StringSplitOptions.RemoveEmptyEntries );
						if ( pathSegments.Length == 1 ) {
							// the link is to something in the same directory,
							// unless the link is ..
							if ( String.Compare ( "..", pathSegments[0], false ) == 0 ) {
								// set the type and we're done.
								objectType = FileListingService.FileTypes.DirectoryLink;
							} else {
								// either we found the object already
								// or we'll find it later.
							}
						}
					} else {
						
					}

					linkName = info;
					// add an arrow in front to specify it's a link.
					info = String.Format ( LINK_FORMAT, info );
				}

				// get the entry, either from an existing one, or a new one
				FileEntry entry = GetExistingEntry ( name );
				if ( entry == null ) {
					entry = new FileEntry ( Parent.Device, Parent, name, objectType, false /* isRoot */);
				}

				// add some misc info
				entry.Permissions = new FilePermissions ( permissions );
				entry.Size = size;
				entry.Date = date;
				entry.Owner = owner;
				entry.Group =  group;
				entry.IsExecutable = isExec;
				entry.LinkName = linkName;
				if ( objectType == FileListingService.FileTypes.Link ) {
					entry.Info = info;
				}

				Entries.Add ( entry );
			}
		}


		/// <summary>
		/// Gets a value indicating whether this instance is cancelled.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is cancelled; otherwise, <c>false</c>.
		/// </value>
		public override bool IsCancelled {
			get {
				return false;
			}
		}

		/// <summary>
		/// Queries for an already existing Entry per name
		/// </summary>
		/// <param name="name">the name of the entry</param>
		/// <returns>the existing FileEntry or null if no entry with a matching name exists.</returns>
		private FileEntry GetExistingEntry ( String name ) {
			for ( int i = 0; i < CurrentChildren.Length; i++ ) {
				FileEntry e = CurrentChildren[i];
				// since we're going to "erase" the one we use, we need to
				// check that the item is not null.
				if ( e != null ) {
					// compare per name, case-sensitive.
					if ( string.Compare ( name, e.Name, false ) == 0 ) {
						// erase from the list
						CurrentChildren[i] = null;
						// and return the object
						return e;
					}
				}
			}

			// couldn't find any matching object, return null
			return null;
		}


		/// <summary>
		/// Finishes the links.
		/// </summary>
		public void FinishLinks ( ) {
			// this isnt done in the DDMS lib either... 
			// TODO: Handle links in the listing service
		}
	}
}
