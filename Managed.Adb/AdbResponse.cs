// <copyright file="AdbResponse.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion. All rights reserved.
// </copyright>

namespace Managed.Adb
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// An Adb Communication Response
    /// </summary>
    public class AdbResponse
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AdbResponse"/> class.
        /// </summary>
        public AdbResponse()
        {
            this.Message = string.Empty;
        }

        public static AdbResponse OK
        { get; } = new AdbResponse()
        {
            IOSuccess = true,
            Okay = true,
            Message = string.Empty,
            Timeout = false
        };

        public static AdbResponse FromError(string message)
        {
            return new AdbResponse()
            {
                IOSuccess = true,
                Message = message,
                Okay = false,
                Timeout = false
            };
        }

        /// <summary>
        /// Gets or sets a value indicating whether the IO communication was a success.
        /// </summary>
        /// <value>
        ///   <see langword="true"/> if successful; otherwise, <see langword="false"/>.
        /// </value>
        public bool IOSuccess { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="AdbResponse"/> is okay.
        /// </summary>
        /// <value>
        ///   <see langword="true"/> if okay; otherwise, <see langword="false"/>.
        /// </value>
        public bool Okay { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="AdbResponse"/> is timeout.
        /// </summary>
        /// <value>
        ///   <see langword="true"/> if timeout; otherwise, <see langword="false"/>.
        /// </value>
        public bool Timeout { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        public string Message { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as AdbResponse;

            if (other == null)
            {
                return false;
            }

            return other.IOSuccess == this.IOSuccess
                && string.Equals(other.Message, this.Message, StringComparison.OrdinalIgnoreCase)
                && other.Okay == this.Okay
                && other.Timeout == this.Timeout;
        }

        public override int GetHashCode()
        {
            int hash = 17;
            hash = (hash * 23) + this.IOSuccess.GetHashCode();
            hash = (hash * 23) + this.Message == null ? 0 : this.Message.GetHashCode();
            hash = (hash * 23) + this.Okay.GetHashCode();
            hash = (hash * 23) + this.Timeout.GetHashCode();

            return hash;
        }
    }
}
