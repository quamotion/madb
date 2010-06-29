using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace Managed.Adb {
	public class PackageManager {
		private const String PM_LIST_FULL = "pm list packages -f";
		public PackageManager ( Device device ) {
			Device = device;
			Packages = new Dictionary<string, FileEntry> ( );
		}

		public Dictionary<String,FileEntry> Packages { get; set; }

		public void RefreshPackages ( ) {
			if ( this.Device.IsOffline ) {
				throw new IOException ( "Device is offline" );
			}

			PackageManagerReceiver pmr = new PackageManagerReceiver ( this.Device, this );
			this.Device.ExecuteShellCommand ( PM_LIST_FULL, pmr );

		}

		public bool Exists ( String package ) {
			try {
				return GetApkFileEntry ( package ) != null;
			} catch ( FileNotFoundException) {
				return false;
			}
		}

		public FileEntry GetApkFileEntry ( String package ) {
			return FileEntry.Find ( this.Device, GetApkPath ( package ) );
		}

		public String GetApkPath ( String package ) {

			if ( this.Device.IsOffline ) {
				throw new IOException ( "Device is offline" );
			}

			PackageManagerPathReceiver receiver = new PackageManagerPathReceiver();
			this.Device.ExecuteShellCommand ( "pm path {0}", receiver, package );
			if ( !String.IsNullOrEmpty ( receiver.Path ) ) {
				return receiver.Path;
			} else {
				throw new FileNotFoundException ( String.Format ( "The package '{0}' is not installed on the device: {1}", package, Device.SerialNumber ) );
			}

		}

		private Device Device { get; set; }

		private class PackageManagerPathReceiver : MultiLineReceiver {
			/// <summary>
			/// Pattern to parse the output of the 'pm path &lt;package&gt;' command.
			/// The output format looks like:
			/// /data/app/myapp.apk=com.mypackage.myapp
			/// </summary>
			public const String PM_PATH_PATTERN = "^package:(.+?)$";

			public PackageManagerPathReceiver ( ) {
				Path = null;
			}

			public String Path { get; private set; }

			protected override void ProcessNewLines ( string[] lines ) {
				foreach ( String line in lines ) {
					if ( !String.IsNullOrEmpty ( line ) && !line.StartsWith ( "#" ) ) {
						// get the filepath and package from the line
						Match m = Regex.Match ( line, PM_PATH_PATTERN, RegexOptions.Compiled );
						if ( m.Success ) {
							Path = m.Groups[1].Value;
						}
					}
				}
			}
		}
	}
}
