using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Managed.Adb {
	public class MountPoint : ICloneable {
		/// <summary>
		/// 
		/// </summary>
		/// <param name="block"></param>
		/// <param name="name"></param>
		/// <param name="fs"></param>
		/// <param name="access"></param>
		public MountPoint ( string block, string name, string fs, bool readOnly ) {
			this.Block = block;
			this.Name = name;
			this.FileSystem = fs;
			this.IsReadOnly = readOnly;
		}

		/// <summary>
		/// Gets the mount point block
		/// </summary>
		public String Block { get; private set; }

		/// <summary>
		/// Gets the mount point name
		/// </summary>
		public String Name { get; private set; }

		/// <summary>
		/// Gets the mount point file system
		/// </summary>
		public String FileSystem { get; private set; }

		/// <summary>
		/// Gets the mount point access
		/// </summary>
		public bool IsReadOnly { get; private set; }

		/// <summary>
		/// Returns a string representation of the mount point.
		/// </summary>
		/// <returns></returns>
		public override string ToString ( ) {
			return string.Format ( "{0}\t{1}\t{2}\t{3}", Block, Name, FileSystem, IsReadOnly ? "ro" : "rw" );
		}

		/// <summary>
		/// Creates a clone of this MountPoint
		/// </summary>
		/// <returns></returns>
		public MountPoint Clone ( ) {
			return this.MemberwiseClone ( ) as MountPoint;
		}

		object ICloneable.Clone ( ) {
			return this.MemberwiseClone ( );
		}
	}
}
