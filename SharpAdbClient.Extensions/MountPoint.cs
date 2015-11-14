// <copyright file="MountPoint.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion. All rights reserved.
// </copyright>

namespace SharpAdbClient
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Represents a mount point.
    /// </summary>
    public class MountPoint : ICloneable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MountPoint"/> class.
        /// </summary>
        /// <param name="block">The block.</param>
        /// <param name="name">The name.</param>
        /// <param name="fs">The fs.</param>
        /// <param name="readOnly">if set to <see langword="true"/> [read only].</param>
        public MountPoint(string block, string name, string fs, bool readOnly)
        {
            this.Block = block;
            this.Name = name;
            this.FileSystem = fs;
            this.IsReadOnly = readOnly;
        }

        /// <summary>
        /// Gets the mount point block
        /// </summary>
        /// <value>The block.</value>
        public string Block { get; private set; }

        /// <summary>
        /// Gets the mount point name
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the mount point file system
        /// </summary>
        /// <value>The file system.</value>
        public string FileSystem { get; private set; }

        /// <summary>
        /// Gets the mount point access
        /// </summary>
        /// <value>
        /// 	<see langword="true"/> if this instance is read only; otherwise, <see langword="false"/>.
        /// </value>
        public bool IsReadOnly { get; private set; }

        /// <summary>
        /// Returns a string representation of the mount point.
        /// </summary>
        /// <returns>
        /// A <see cref="string"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format("{0}\t{1}\t{2}\t{3}", this.Block, this.Name, this.FileSystem, this.IsReadOnly ? "ro" : "rw");
        }

        /// <summary>
        /// Creates a clone of this MountPoint
        /// </summary>
        /// <returns></returns>
        public MountPoint Clone()
        {
            return this.MemberwiseClone() as MountPoint;
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        object ICloneable.Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
