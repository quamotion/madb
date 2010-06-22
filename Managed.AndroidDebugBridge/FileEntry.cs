using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Managed.Adb {
	public class FileEntry {
		/// <summary>
		/// Pattern to escape filenames for shell command consumption.
		/// </summary>
		private const String ESCAPEPATTERN = "([\\\\()*+?\"'#/\\s])";

		/// <summary>
		///  Creates a new file entry.
		/// </summary>
		/// <param name="parent">parent entry or null if entry is root</param>
		/// <param name="name">name of the entry.</param>
		/// <param name="type">entry type.</param>
		/// <param name="isRoot"></param>
		internal FileEntry ( FileEntry parent, String name, FileListingService.FileTypes type, bool isRoot ) {
			this.FetchTime = 0;
			this.Parent = parent;
			this.Name = name;
			this.Type = type;
			this.IsRoot = isRoot;
			Children = new List<FileEntry> ( );
			CheckAppPackageStatus ( );
		}

		public FileEntry Parent { get; private set; }
		public String Name { get; private set; }
		public String Info { get; set; }
		public String Permissions { get; set; }
		public long Size { get; set; }
		public String Date { get; set; }
		public String Time { get; set; }
		public String Owner { get; set; }
		public String Group { get; set; }
		public FileListingService.FileTypes Type { get; private set; }
		public bool IsApplicationPackage { get; private set; }
		public bool IsRoot { get; private set; }

		public List<FileEntry> Children { get; set; }



		/// <summary>
		/// Indicates whether the entry content has been fetched yet, or not.
		/// </summary>
		public long FetchTime { get; set; }

		internal bool NeedFetch {
			get {
				if ( FetchTime == 0 ) {
					return true;
				}

				long current = DateTimeHelper.CurrentMillis ( );
				if ( current - FetchTime > FileListingService.REFRESH_TEST ) {
					return true;
				}

				return false;
			}
		}

		/// <summary>
		/// Adds a child file entry
		/// </summary>
		/// <param name="child">The child file entry</param>
		public void AddChild ( FileEntry child ) {
			Children.Add ( child );
		}


		/// <summary>
		/// Returns the child {@link FileEntry} matching the name.
		/// This uses the cached children list.
		/// </summary>
		/// <param name="name">the name of the child to return.</param>
		/// <return>the FileEntry matching the name or null.</return>
		public FileEntry FindChild ( String name ) {
			foreach ( FileEntry entry in Children ) {
				if ( String.Compare ( entry.Name, name, false ) == 0 ) {
					return entry;
				}
			}
			return null;
		}


		public bool IsDirectory {
			get {
				return this.Type == FileListingService.FileTypes.Directory || Type == FileListingService.FileTypes.DirectoryLink;
			}
		}

		public bool IsApplicationFileName {
			get {
				Regex regex = new Regex ( FileListingService.APK_FILE_PATTERN, RegexOptions.Compiled );
				return regex.IsMatch ( this.Name );
			}
		}

		/// <summary>
		/// Gets the full path of the entry.
		/// </summary>
		public String FullPath {
			get {
				if ( IsRoot ) {
					return FileListingService.FILE_ROOT;
				}

				StringBuilder pathBuilder = new StringBuilder ( );
				FillPathBuilder ( pathBuilder, false );

				return pathBuilder.ToString ( );
			}
		}

		/// <summary>
		/// Gets the fully escaped path of the entry. This path is safe to use in a shell command line.
		/// </summary>
		public String FullEscapedPath {
			get {
				StringBuilder pathBuilder = new StringBuilder ( );
				FillPathBuilder ( pathBuilder, true );

				return pathBuilder.ToString ( );
			}
		}

		/// <summary>
		/// Gets the path as a list of segments.
		/// </summary>
		public String[] PathSegments {
			get {
				List<String> list = new List<String> ( );
				FillPathSegments ( list );

				return list.ToArray ( );
			}
		}

		/// <summary>
		/// Returns an escaped version of the entry name.
		/// </summary>
		/// <param name="entryName"></param>
		/// <returns></returns>
		private String Escape ( String entryName ) {
			return new Regex ( ESCAPEPATTERN ).Replace ( entryName, new MatchEvaluator ( delegate ( Match m ) {
				return m.Result ( "\\\\$1" );
			} ) );
		}

		/// <summary>
		/// Sets the internal app package status flag. This checks whether the entry is in an app
		/// directory like /data/app or /system/app
		/// </summary>
		private void CheckAppPackageStatus ( ) {
			IsApplicationPackage = false;

			String[] segments = PathSegments;
			if ( this.Type == FileListingService.FileTypes.File && segments.Length == 3 && IsApplicationFileName ) {
				IsApplicationPackage = String.Compare ( FileListingService.DIRECTORY_APP, segments[1], false ) == 0 &&
						( String.Compare ( FileListingService.DIRECTORY_SYSTEM, segments[0], false ) == 0 || String.Compare ( FileListingService.DIRECTORY_DATA, segments[0], false ) == 0 );
			}
		}

		/// <summary>
		/// Recursively fills the segment list with the full path.
		/// </summary>
		/// <param name="list">The list of segments to fill.</param>
		protected void FillPathSegments ( List<String> list ) {
			if ( IsRoot ) {
				return;
			}

			if ( Parent != null ) {
				Parent.FillPathSegments ( list );
			}

			list.Add ( Name );
		}

		/// <summary>
		/// Recursively fills the pathBuilder with the full path
		/// </summary>
		/// <param name="pathBuilder">a StringBuilder used to create the path.</param>
		/// <param name="escapePath"> Whether the path need to be escaped for consumption by a shell command line.</param>
		protected void FillPathBuilder ( StringBuilder pathBuilder, bool escapePath ) {
			if ( IsRoot ) {
				return;
			}

			if ( Parent != null ) {
				Parent.FillPathBuilder ( pathBuilder, escapePath );
			}
			pathBuilder.Append ( FileListingService.FILE_SEPARATOR );
			pathBuilder.Append ( escapePath ? Escape ( Name ) : Name );
		}

		public class FileEntryComparer : Comparer<FileEntry> {

			public override int Compare ( FileEntry x, FileEntry y ) {
				return x.Name.CompareTo ( y.Name );
			}
		}
	}
}
