using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Managed.Adb {
	public class MountPointReceiver : MultiLineReceiver {
		/// <summary>
		/// The regex for finding mount point info from "cat /etc/fstab"
		/// </summary>
		/// <remarks>
		/// This is more dynamic then the ddms implemntation, they have 3 hard coded mount points and 
		/// they check for them only, and only their name, not any other information.
		/// </remarks>
		private const String RE_FSTAB_MOUNTPOINT_PATTERN = @"(/[\S]+)\s+([\S]+)\s+([\S]+)\s+(.*)";
		private const String RE_MOUNTPOINT_PATTERN = @"^([\S]+)\s+on\s+([\S]+)\s+type\s+([\S]+)\s+\((r[wo]).*\)$";

		/// <summary>
		/// 
		/// </summary>
		public MountPointReceiver ( ) {
			MountPoints = new Dictionary<string, MountPoint> ( );
		}

		protected override void ProcessNewLines ( string[] lines ) {
			MountPoints = new Dictionary<string, MountPoint> ( );
			foreach ( var line in lines ) {
				Match m = Regex.Match ( line, RE_MOUNTPOINT_PATTERN, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace );
				if ( m.Success ) {
					String block = m.Groups[1].Value.Trim().Replace("//","/");
					String name = m.Groups[2].Value.Trim();
					String fs = m.Groups[3].Value.Trim();
					bool ro = String.Compare("ro",m.Groups[4].Value.Trim(),false) == 0;
					MountPoint mnt = new MountPoint ( block, name, fs, ro );
					String key = name.Substring ( 1 );
					this.MountPoints.Add ( name, mnt );
				}
			}
		}

		/// <summary>
		/// Gets the mount points found on the system
		/// </summary>
		public Dictionary<String, MountPoint> MountPoints { get; private set; }
	}
}
