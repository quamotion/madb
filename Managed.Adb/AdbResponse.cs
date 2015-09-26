using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Managed.Adb
{
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
    }
}
