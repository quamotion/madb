// <copyright file="FilePermission.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion. All rights reserved.
// </copyright>

namespace SharpAdbClient
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    ///
    /// </summary>
    public class FilePermission
    {
        /// <summary>
        ///
        /// </summary>
        [Flags]
        public enum Modes
        {
            /// <summary>
            ///
            /// </summary>
            NoAccess = 0,

            /// <summary>
            ///
            /// </summary>
            Execute = 1,

            /// <summary>
            ///
            /// </summary>
            Write = 2,

            /// <summary>
            ///
            /// </summary>
            Read = 4
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FilePermission"/> class.
        /// </summary>
        public FilePermission()
            : this("---")
            {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FilePermission"/> class.
        /// </summary>
        /// <param name="linuxPermissions">The linux permissions.</param>
        public FilePermission(string linuxPermissions)
        {
            this.CanRead = string.Compare(linuxPermissions.Substring(0, 1), "r", false) == 0;
            this.CanWrite = string.Compare(linuxPermissions.Substring(1, 1), "w", false) == 0;
            this.CanExecute = string.Compare(linuxPermissions.Substring(2, 1), "x", false) == 0 || string.Compare(linuxPermissions.Substring(2, 1), "t", false) == 0;
            this.CanDelete = this.CanWrite && string.Compare(linuxPermissions.Substring(2, 1), "t", false) != 0;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance can execute.
        /// </summary>
        /// <value>
        /// 	<see langword="true"/> if this instance can execute; otherwise, <see langword="false"/>.
        /// </value>
        public bool CanExecute { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance can write.
        /// </summary>
        /// <value><see langword="true"/> if this instance can write; otherwise, <see langword="false"/>.</value>
        public bool CanWrite { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance can read.
        /// </summary>
        /// <value><see langword="true"/> if this instance can read; otherwise, <see langword="false"/>.</value>
        public bool CanRead { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance can delete.
        /// </summary>
        /// <value>
        /// 	<see langword="true"/> if this instance can delete; otherwise, <see langword="false"/>.
        /// </value>
        public bool CanDelete { get; private set; }

        /// <summary>
        /// Gets a <see cref="string"/> that represents the current <see cref="FilePermission"/> object.
        /// </summary>
        /// <returns>
        /// A <see cref="string"/> that represents the current <see cref="FilePermission"/> object.
        /// </returns>
        public override string ToString()
        {
            StringBuilder perm = new StringBuilder();
            return perm.AppendFormat("{0}", this.CanRead ? "r" : "-").AppendFormat("{0}", this.CanWrite ? "w" : "-").AppendFormat("{0}", this.CanExecute ? this.CanDelete ? "x" : "t" : "-").ToString();
        }

        /// <summary>
        /// Converts the permissions to bit value that can be casted to an integer and used for calling chmod
        /// </summary>
        /// <returns></returns>
        public Modes ToChmod()
        {
            Modes val = Modes.NoAccess;
            if (this.CanRead)
            {
                val |= Modes.Read;
            }

            if (this.CanWrite)
            {
                val |= Modes.Write;
            }

            if (this.CanExecute)
            {
                val |= Modes.Execute;
            }

            int ival = (int)val;
            return val;
        }
    }
}
