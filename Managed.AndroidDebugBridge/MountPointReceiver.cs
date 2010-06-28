using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Managed.Adb {
	/// <summary>
	/// A dynamic mount point receiver.
	/// </summary>
	/// <remarks>Works on busybox and non-busybox devices</remarks>
	public class MountPointReceiver : MultiLineReceiver {
		/// <summary>
		/// The mount parsing pattern.
		/// </summary>
		private const String RE_MOUNTPOINT_PATTERN = @"^([\S]+)\s+([\S]+)\s+([\S]+)\s+(r[wo]).*$";
		/// <summary>
		/// The mount command
		/// </summary>
		public const String MOUNT_COMMAND = "cat /proc/mounts";

		/// <summary>
		/// 
		/// </summary>
		public MountPointReceiver ( Device device ) {
			Device = device;
		}

		protected override void ProcessNewLines ( string[] lines ) {
			Device.MountPoints.Clear ( );
			foreach ( var line in lines ) {
				Match m = Regex.Match ( line, RE_MOUNTPOINT_PATTERN, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace );
				if ( m.Success ) {
					String block = m.Groups[1].Value.Trim ( ).Replace ( "//", "/" );
					String name = m.Groups[2].Value.Trim ( );
					String fs = m.Groups[3].Value.Trim ( );
					bool ro = String.Compare ( "ro", m.Groups[4].Value.Trim ( ), false ) == 0;
					MountPoint mnt = new MountPoint ( block, name, fs, ro );
					String key = name.Substring ( 1 );
					Device.MountPoints.Add ( name, mnt );
				}
			}
		}

		protected override void Done ( ) {
			this.Device.OnBuildInfoChanged ( EventArgs.Empty );
			base.Done ( );
		}

		public Device Device { get; set; }
	}
}
