using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;


namespace Managed.Adb.IO {
	/// <summary>
	/// Just like System.IO.Path, except it is geared for Linux
	/// </summary>
	public static class LinuxPath {

		/// <summary>
		/// Pattern to escape filenames for shell command consumption.
		/// </summary>
		private const String ESCAPEPATTERN = "([\\\\()*+?\"'#/\\s])";

		/// <summary>
		/// The directory separator character
		/// </summary>
		public static readonly char DirectorySeparatorChar = '/';
		/// <summary>
		/// The directory separator alternate character
		/// </summary>
		public static readonly char AltDirectorySeparatorChar = '/';
		/// <summary>
		/// Invalid characters for a file name
		/// </summary>
		private static readonly char[] InvalidFileNameChars = new char[] { 
						'"', '<', '>', '|', '\0', '\x0001', '\x0002', '\x0003', '\x0004', '\x0005', '\x0006', '\a', '\b', '\t', '\n', '\v', 
						'\f', '\r', '\x000e', '\x000f', '\x0010', '\x0011', '\x0012', '\x0013', '\x0014', '\x0015', '\x0016', '\x0017', '\x0018', '\x0019', '\x001a', '\x001b', 
						'\x001c', '\x001d', '\x001e', '\x001f', '*', '?', '\\', '/'
				 };
		/// <summary>
		/// The max length of a directory path
		/// </summary>
		internal const int MAX_DIRECTORY_PATH = 0xf8;
		/// <summary>
		/// the max length of a file path
		/// </summary>
		internal const int MAX_PATH = 260;
		/// <summary>
		/// the max length of a file path
		/// </summary>
		internal static readonly int MaxPath = 260;
		/// <summary>
		/// The character used to separate multiple paths
		/// </summary>
		public static readonly char PathSeparator = ';';
		/// <summary>
		/// Invalid characters for a file/directory path
		/// </summary>
		private static readonly char[] RealInvalidPathChars = new char[] { 
						'"', '<', '>', '|', '\0', '\x0001', '\x0002', '\x0003', '\x0004', '\x0005', '\x0006', '\a', '\b', '\t', '\n', '\v', 
						'\f', '\r', '\x000e', '\x000f', '\x0010', '\x0011', '\x0012', '\x0013', '\x0014', '\x0015', '\x0016', '\x0017', '\x0018', '\x0019', '\x001a', '\x001b', 
						'\x001c', '\x001d', '\x001e', '\x001f'
				 };

		/// <summary>
		/// Determines whether this instance [can path circumvent security native] the specified part of path.
		/// </summary>
		/// <param name="partOfPath">The part of path.</param>
		/// <returns>
		///   <c>true</c> if this instance [can path circumvent security native] the specified part of path; otherwise, <c>false</c>.
		/// </returns>
		[MethodImpl ( MethodImplOptions.InternalCall )]
		private static extern bool CanPathCircumventSecurityNative ( string partOfPath );

		/// <summary>Changes the extension of a path string.</summary>
		/// <returns>A string containing the modified path information.On Windows-based desktop platforms, if path is null or an empty string (""), the path information is returned unmodified. If extension is null, the returned string contains the specified path with its extension removed. If path has no extension, and extension is not null, the returned path string contains extension appended to the end of path.</returns>
		/// <param name="extension">The new extension (with a leading period). Specify null to remove an existing extension from path. </param>
		/// <param name="path">The path information to modify. The path cannot contain any of the characters defined in <see cref="F:System.IO.Path.InvalidPathChars"></see>. </param>
		/// <exception cref="T:System.ArgumentException">The path contains one or more of the invalid characters defined in <see cref="F:System.IO.Path.InvalidPathChars"></see>, or contains a wildcard character. </exception>
		/// <filterpriority>1</filterpriority>
		public static string ChangeExtension ( string path, string extension ) {
			if ( path == null ) {
				return null;
			}
			CheckInvalidPathChars ( path );
			string str = path;
			int length = path.Length;
			while ( --length >= 0 ) {
				char ch = path[length];
				if ( ch == '.' ) {
					str = path.Substring ( 0, length );
					break;
				}
				if ( ( ( ch == DirectorySeparatorChar ) || ( ch == AltDirectorySeparatorChar ) ) ) {
					break;
				}
			}
			if ( String.IsNullOrEmpty ( extension ) || path.Length == 0 ) {
				return str;
			}
			if ( extension.Length > 0 && extension[0] != '.' ) {
				str = str + ".";
			}

			return ( str + extension );
		}

		/// <summary>
		/// Checks if the char array starts with ordinal
		/// </summary>
		/// <param name="array">The array.</param>
		/// <param name="numChars">The num chars.</param>
		/// <param name="compareTo">The compare to.</param>
		/// <param name="ignoreCase">if set to <c>true</c> [ignore case].</param>
		/// <returns></returns>
		private static unsafe bool CharArrayStartsWithOrdinal ( char* array, int numChars, string compareTo, bool ignoreCase ) {
			if ( numChars < compareTo.Length ) {
				return false;
			}
			if ( ignoreCase ) {
				string str = new string ( array, 0, compareTo.Length );
				return compareTo.Equals ( str, StringComparison.OrdinalIgnoreCase );
			}
			for ( int i = 0; i < compareTo.Length; i++ ) {
				if ( array[i] != compareTo[i] ) {
					return false;
				}
			}
			return true;
		}

		/// <summary>
		/// Checks if the char array starts with ordinal
		/// </summary>
		/// <param name="array">The array.</param>
		/// <param name="numChars">The num chars.</param>
		/// <param name="compareTo">The compare to.</param>
		/// <param name="ignoreCase">if set to <c>true</c> [ignore case].</param>
		/// <returns></returns>
		private static bool CharArrayStartsWithOrdinal ( char[] array, int numChars, string compareTo, bool ignoreCase ) {
			if ( numChars < compareTo.Length ) {
				return false;
			}
			if ( ignoreCase ) {
				string str = new string ( array, 0, compareTo.Length );
				return compareTo.Equals ( str, StringComparison.OrdinalIgnoreCase );
			}
			for ( int i = 0; i < compareTo.Length; i++ ) {
				if ( array[i] != compareTo[i] ) {
					return false;
				}
			}
			return true;
		}

		/// <summary>
		/// Checks the invalid path chars.
		/// </summary>
		/// <param name="path">The path.</param>
		internal static void CheckInvalidPathChars( string path ) {
			for ( int i = 0; i < path.Length; i++ ) {
				int num2 = path[i];
				if ( ( ( num2 == 0x22 ) || ( num2 == 60 ) ) || ( ( ( num2 == 0x3e ) || ( num2 == 0x7c ) ) || ( num2 < 0x20 ) ) ) {
					throw new ArgumentException ( "Path contains invalid characters" );
				}
			}
		}

		/// <summary>Combines two path strings.</summary>
		/// <returns>A string containing the combined paths. If one of the specified paths is a zero-length string, this method returns the other path. If path2 contains an absolute path, this method returns path2.</returns>
		/// <param name="path2">The second path. </param>
		/// <param name="path1">The first path. </param>
		/// <exception cref="T:System.ArgumentNullException">path1 or path2 is null. </exception>
		/// <exception cref="T:System.ArgumentException">path1 or path2 contain one or more of the invalid characters defined in <see cref="F:System.IO.Path.InvalidPathChars"></see>, or contains a wildcard character. </exception>
		public static string Combine ( string path1, string path2 ) {
			if ( ( path1 == null ) || ( path2 == null ) ) {
				throw new ArgumentNullException ( ( path1 == null ) ? "path1" : "path2" );
			}
			CheckInvalidPathChars ( path1 );
			CheckInvalidPathChars ( path2 );
			return CombineNoChecks ( path1, path2 );
		}

		/// <summary>
		/// Combine the specified paths to form one path
		/// </summary>
		/// <param name="path1">The path1.</param>
		/// <param name="path2">The path2.</param>
		/// <param name="path3">The path3.</param>
		/// <returns></returns>
		public static string Combine ( string path1, string path2, string path3 ) {
			if ( ( ( path1 == null ) || ( path2 == null ) ) || ( path3 == null ) ) {
				throw new ArgumentNullException ( ( path1 == null ) ? "path1" : ( ( path2 == null ) ? "path2" : "path3" ) );
			}
			CheckInvalidPathChars ( path1 );
			CheckInvalidPathChars ( path2 );
			CheckInvalidPathChars ( path3 );
			return CombineNoChecks ( CombineNoChecks ( path1, path2 ), path3 );
		}

		/// <summary>
		/// Combine the specified paths to form one path
		/// </summary>
		/// <param name="path1">The path1.</param>
		/// <param name="path2">The path2.</param>
		/// <param name="path3">The path3.</param>
		/// <param name="path4">The path4.</param>
		/// <returns></returns>
		public static string Combine ( string path1, string path2, string path3, string path4 ) {
			if ( ( ( path1 == null ) || ( path2 == null ) ) || ( ( path3 == null ) || ( path4 == null ) ) ) {
				throw new ArgumentNullException ( ( path1 == null ) ? "path1" : ( ( path2 == null ) ? "path2" : ( ( path3 == null ) ? "path3" : "path4" ) ) );
			}
			CheckInvalidPathChars ( path1 );
			CheckInvalidPathChars ( path2 );
			CheckInvalidPathChars ( path3 );
			CheckInvalidPathChars ( path4 );
			return CombineNoChecks ( CombineNoChecks ( CombineNoChecks ( path1, path2 ), path3 ), path4 );
		}

		/// <summary>
		/// Combine the specified paths to form one path
		/// </summary>
		/// <param name="paths">The paths.</param>
		/// <returns></returns>
		public static String Combine ( params String[] paths ) {
			if ( paths == null ) {
				throw new ArgumentNullException ( "paths" );
			}
			int capacity = 0;
			int num2 = 0;
			for ( int i = 0; i < paths.Length; i++ ) {
				if ( paths[i] == null ) {
					throw new ArgumentNullException ( "paths" );
				}
				if ( paths[i].Length != 0 ) {
					CheckInvalidPathChars ( paths[i] );
					if ( IsPathRooted ( paths[i] ) ) {
						num2 = i;
						capacity = paths[i].Length;
					} else {
						capacity += paths[i].Length;
					}
					char ch = paths[i][paths[i].Length - 1];
					if ( ( ( ch != DirectorySeparatorChar ) && ( ch != AltDirectorySeparatorChar ) ) ) {
						capacity++;
					}
				}
			}
			StringBuilder builder = new StringBuilder ( capacity );
			for ( int j = num2; j < paths.Length; j++ ) {
				if ( paths[j].Length != 0 ) {
					if ( builder.Length == 0 ) {
						builder.Append ( FixupPath ( paths[j] ) );
					} else {
						char ch2 = builder[builder.Length - 1];
						if ( ( ( ch2 != DirectorySeparatorChar ) && ( ch2 != AltDirectorySeparatorChar ) ) ) {
							builder.Append ( DirectorySeparatorChar );
						}
							builder.Append ( paths[j] );
					}
				}
			}
			return builder.ToString ( );

		}

		/// <summary>
		/// Combines the path with no checks.
		/// </summary>
		/// <param name="path1">The path1.</param>
		/// <param name="path2">The path2.</param>
		/// <returns></returns>
		private static string CombineNoChecks ( string path1, string path2 ) {
			if ( path2.Length == 0 ) {
				return FixupPath ( path1 );
			}
			if ( path1.Length == 0 ) {
				return FixupPath ( path2 );
			}
			if ( IsPathRooted ( path2 ) ) {
				return FixupPath ( path2 );
			}
			char ch = path1[path1.Length - 1];
			char ch2 = path2[0];
			if ( ch != DirectorySeparatorChar && ch != AltDirectorySeparatorChar &&
				ch2 != DirectorySeparatorChar && ch2 != AltDirectorySeparatorChar ) {
				return ( FixupPath ( path1 ) + path2 );
			}
			return ( FixupPath ( path1 ) + path2 );
		}



		/// <summary>Returns the directory information for the specified path string.</summary>
		/// <returns>A <see cref="T:System.String"></see> containing directory information for path, or null if path denotes a root directory, is the empty string (""), or is null. Returns <see cref="F:System.String.Empty"></see> if path does not contain directory information.</returns>
		/// <param name="path">The path of a file or directory. </param>
		/// <exception cref="T:System.ArgumentException">The path parameter contains invalid characters, is empty, or contains only white spaces, or contains a wildcard character. </exception>
		/// <exception cref="T:System.IO.PathTooLongException">The path parameter is longer than the system-defined maximum length.</exception>
		/// <filterpriority>1</filterpriority>
		public static string GetDirectoryName ( String path ) {
			if ( path != null ) {
				CheckInvalidPathChars ( path );
				//path = FixupPath ( path );

				String tpath = path;
				if ( tpath.Length > 1 ) {
					if ( tpath.EndsWith ( new String ( new char[] { DirectorySeparatorChar } ) ) ) {
						return tpath.Substring ( 0, tpath.Length );
					}

					tpath = tpath.Substring ( 0, tpath.LastIndexOf ( DirectorySeparatorChar ) + 1 );

					return FixupPath ( tpath );
				} else if ( tpath.Length == 1 ) {
					return new String ( new char[] { DirectorySeparatorChar } );
				}
			}
			return null;
		}

		/// <summary>
		/// Gets the path without file name.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <returns></returns>
		public static string GetPathWithoutFile ( string path ) {
			if ( path != null ) {
				CheckInvalidPathChars ( path );
				string tfilename = GetFileName ( path );
				string tpath = path.Substring ( 0, path.Length - tfilename.Length );
				tpath = FixupPath ( tpath );
				return tpath.Substring ( 0, tpath.LastIndexOf ( DirectorySeparatorChar ) + 1 );
			}
			return null;
		}

		/// <summary>
		/// Fixups the path.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <returns></returns>
		private static string FixupPath ( string path ) {
			string sb = path;
			sb = sb.Replace ( System.IO.Path.DirectorySeparatorChar, DirectorySeparatorChar );


			if ( !sb.StartsWith ( new String ( new char[] { DirectorySeparatorChar } ) ) ) {
				sb = string.Format ( ".{0}{1}", DirectorySeparatorChar, sb );
			}

			if ( !sb.EndsWith ( new String ( new char[] { DirectorySeparatorChar } ) ) ) {
				sb = string.Format ( "{0}{1}", sb, DirectorySeparatorChar );
			}

			sb = sb.Replace ( "//", new byte[] { (byte)DirectorySeparatorChar }.GetString ( ) );

			return sb;
		}

		/// <summary>Returns the extension of the specified path string.</summary>
		/// <returns>A <see cref="T:System.String"></see> containing the extension of the specified path (including the "."), null, or <see cref="F:System.String.Empty"></see>. If path is null, GetExtension returns null. If path does not have extension information, GetExtension returns Empty.</returns>
		/// <param name="path">The path string from which to get the extension. </param>
		/// <exception cref="T:System.ArgumentException">path contains one or more of the invalid characters defined in <see cref="F:System.IO.Path.InvalidPathChars"></see>, or contains a wildcard character. </exception>
		/// <filterpriority>1</filterpriority>
		public static string GetExtension ( string path ) {
			if ( path == null ) {
				return null;
			}
			CheckInvalidPathChars ( path );
			int length = path.Length;
			int startIndex = length;
			while ( --startIndex >= 0 ) {
				char ch = path[startIndex];
				if ( ch == '.' ) {
					if ( startIndex != ( length - 1 ) ) {
						return path.Substring ( startIndex, length - startIndex );
					}
					return string.Empty;
				}
				if ( ( ( ch == DirectorySeparatorChar ) || ( ch == AltDirectorySeparatorChar ) ) ) {
					break;
				}
			}
			return string.Empty;
		}

		/// <summary>Returns the file name and extension of the specified path string.</summary>
		/// <returns>A <see cref="T:System.String"></see> consisting of the characters after the last directory character in path. If the last character of path is a directory or volume separator character, this method returns <see cref="F:System.String.Empty"></see>. If path is null, this method returns null.</returns>
		/// <param name="path">The path string from which to obtain the file name and extension. </param>
		/// <exception cref="T:System.ArgumentException">path contains one or more of the invalid characters defined in <see cref="F:System.IO.Path.InvalidPathChars"></see>, or contains a wildcard character. </exception>
		/// <filterpriority>1</filterpriority>
		public static string GetFileName ( string path ) {
			if ( path != null ) {
				CheckInvalidPathChars ( path );
				int length = path.Length;
				int num2 = length;
				while ( --num2 >= 0 ) {
					char ch = path[num2];
					if ( ( ( ch == DirectorySeparatorChar ) || ( ch == AltDirectorySeparatorChar ) ) ) {
						return path.Substring ( num2 + 1, ( length - num2 ) - 1 );
					}
				}
			}
			return path;
		}

		/// <summary>Returns the file name of the specified path string without the extension.</summary>
		/// <returns>A <see cref="T:System.String"></see> containing the string returned by <see cref="M:System.IO.Path.GetFileName(System.String)"></see>, minus the last period (.) and all characters following it.</returns>
		/// <param name="path">The path of the file. </param>
		/// <exception cref="T:System.ArgumentException">path contains one or more of the invalid characters defined in <see cref="F:System.IO.Path.InvalidPathChars"></see>, or contains a wildcard character. </exception>
		/// <filterpriority>1</filterpriority>
		public static string GetFileNameWithoutExtension ( string path ) {
			path = GetFileName ( path );
			if ( path == null ) {
				return null;
			}
			int length = path.LastIndexOf ( '.' );
			if ( length == -1 ) {
				return path;
			}
			return path.Substring ( 0, length );
		}


		/// <summary>Gets an array containing the characters that are not allowed in file names.</summary>
		/// <returns>An array containing the characters that are not allowed in file names.</returns>
		public static char[] GetInvalidFileNameChars ( ) {
			return (char[])InvalidFileNameChars.Clone ( );
		}

		/// <summary>Gets an array containing the characters that are not allowed in path names.</summary>
		/// <returns>An array containing the characters that are not allowed in path names.</returns>
		public static char[] GetInvalidPathChars ( ) {
			return (char[])RealInvalidPathChars.Clone ( );
		}

		/// <summary>Gets the root directory information of the specified path.</summary>
		/// <returns>A string containing the root directory of path, such as "C:\", or null if path is null, or an empty string if path does not contain root directory information.</returns>
		/// <param name="path">The path from which to obtain root directory information. </param>
		/// <exception cref="T:System.ArgumentException">path contains one or more of the invalid characters defined in <see cref="F:System.IO.Path.InvalidPathChars"></see>, or contains a wildcard character.-or- <see cref="F:System.String.Empty"></see> was passed to path. </exception>
		/// <filterpriority>1</filterpriority>
		public static string GetPathRoot ( string path ) {
			if ( path == null ) {
				return null;
			}
			path = FixupPath ( path );
			return path.Substring ( 0, GetRootLength ( path ) );
		}

		/// <summary>
		/// Gets the length of the root.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <returns></returns>
		internal static int GetRootLength ( string path ) {
			CheckInvalidPathChars ( path );
			int num = 0;
			int length = path.Length;
			if ( ( length >= 1 ) && IsDirectorySeparator ( path[0] ) ) {
				num = 1;
				if ( ( length >= 2 ) && IsDirectorySeparator ( path[1] ) ) {
					num = 2;
					int num3 = 2;
					while ( ( num < length ) && ( ( ( path[num] != DirectorySeparatorChar ) && ( path[num] != AltDirectorySeparatorChar ) ) || ( --num3 > 0 ) ) ) {
						num++;
					}
				}
				return num;
			}
			if ( ( length >= 2 ) ) {
				num = 2;
				if ( ( length >= 3 ) && IsDirectorySeparator ( path[2] ) ) {
					num++;
				}
			}
			return num;
		}

		/// <summary>Determines whether a path includes a file name extension.</summary>
		/// <returns>true if the characters that follow the last directory separator (\\ or /) or volume separator (:) in the path include a period (.) followed by one or more characters; otherwise, false.</returns>
		/// <param name="path">The path to search for an extension. </param>
		/// <exception cref="T:System.ArgumentException">path contains one or more of the invalid characters defined in <see cref="F:System.IO.Path.InvalidPathChars"></see>, or contains a wildcard character. </exception>
		/// <filterpriority>1</filterpriority>
		public static bool HasExtension ( string path ) {
			if ( path != null ) {
				CheckInvalidPathChars ( path );
				if ( path.EndsWith ( new String ( new char[] { DirectorySeparatorChar } ) ) || path.EndsWith ( new String ( new char[] { AltDirectorySeparatorChar } ) ) ) {
					return false;
				}

				int length = path.Length;
				while ( --length >= 0 ) {
					char ch = path[length];
					if ( ch == '.' ) {
						return ( length != ( path.Length - 1 ) );
					}
					if ( ( ( ch == DirectorySeparatorChar ) || ( ch == AltDirectorySeparatorChar ) ) ) {
						break;
					}
				}
			}
			return false;
		}

		/// <summary>
		/// Combine the specified paths to form one path
		/// </summary>
		/// <param name="path1">The path1.</param>
		/// <param name="path2">The path2.</param>
		/// <returns></returns>
		internal static string InternalCombine ( string path1, string path2 ) {
			if ( ( path1 == null ) || ( path2 == null ) ) {
				throw new ArgumentNullException ( ( path1 == null ) ? "path1" : "path2" );
			}
			CheckInvalidPathChars ( path1 );
			CheckInvalidPathChars ( path2 );
			if ( path2.Length == 0 ) {
				throw new ArgumentException ( "Path can not empty", "path2" );
			}
			if ( IsPathRooted ( path2 ) ) {
				throw new ArgumentException ( "Path is already rooted", "path2" );
			}
			int length = path1.Length;
			if ( length == 0 ) {
				return path2;
			}
			char ch = path1[length - 1];
			if ( ( ( ch != DirectorySeparatorChar ) && ( ch != AltDirectorySeparatorChar ) ) ) {
				return ( path1 + DirectorySeparatorChar + path2 );
			}
			return ( path1 + path2 );
		}

		internal static bool IsDirectorySeparator ( char c ) {
			if ( c != DirectorySeparatorChar ) {
				return ( c == AltDirectorySeparatorChar );
			}
			return true;
		}

		/// <summary>Gets a value indicating whether the specified path string contains absolute or relative path information.</summary>
		/// <returns>true if path contains an absolute path; otherwise, false.</returns>
		/// <param name="path">The path to test. </param>
		/// <exception cref="T:System.ArgumentException">path contains one or more of the invalid characters defined in <see cref="F:System.IO.Path.InvalidPathChars"></see>, or contains a wildcard character. </exception>
		/// <filterpriority>1</filterpriority>
		public static bool IsPathRooted ( string path ) {
			if ( path != null ) {
				CheckInvalidPathChars ( path );
				int length = path.Length;
				if ( ( length >= 1 && ( path[0] == DirectorySeparatorChar || path[0] == AltDirectorySeparatorChar ) ) ||
					( length == 1 ) ) {
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Returns an escaped version of the entry name.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <returns></returns>
		public static String Escape( String path ) {
			return new Regex ( ESCAPEPATTERN ).Replace ( path, new MatchEvaluator ( delegate ( Match m ) {
				return m.Result ( "\\\\$1" );
			} ) );
		}

		/// <summary>
		/// Quotes the specified path.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <returns></returns>
		public static String Quote( String path ) {
			if ( path.Contains ( " " ) ) {
				return String.Format ( "\"{0}\"", path );
			} else {
				return path;
			}
		}


	}
}
