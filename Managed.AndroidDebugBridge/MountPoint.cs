using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Managed.Adb {
	public class MountPoint : ICloneable {
		/// <summary>
		/// Initializes a new instance of the <see cref="MountPoint"/> class.
		/// </summary>
		/// <param name="block">The block.</param>
		/// <param name="name">The name.</param>
		/// <param name="fs">The fs.</param>
		/// <param name="readOnly">if set to <c>true</c> [read only].</param>
		public MountPoint ( string block, string name, string fs, bool readOnly ) {
			this.Block = block;
			this.Name = name;
			this.FileSystem = fs;
			this.IsReadOnly = readOnly;
		}

		/// <summary>
		/// Gets the mount point block
		/// </summary>
		/// <value>The block.</value>
		public String Block { get; private set; }

		/// <summary>
		/// Gets the mount point name
		/// </summary>
		/// <value>The name.</value>
		public String Name { get; private set; }

		/// <summary>
		/// Gets the mount point file system
		/// </summary>
		/// <value>The file system.</value>
		public String FileSystem { get; private set; }

		/// <summary>
		/// Gets the mount point access
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is read only; otherwise, <c>false</c>.
		/// </value>
		public bool IsReadOnly { get; private set; }

		/// <summary>
		/// Returns a string representation of the mount point.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/> that represents this instance.
		/// </returns>
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

		/// <summary>
		/// Creates a new object that is a copy of the current instance.
		/// </summary>
		/// <returns>
		/// A new object that is a copy of this instance.
		/// </returns>
		object ICloneable.Clone ( ) {
			return this.MemberwiseClone ( );
		}
	}
}
