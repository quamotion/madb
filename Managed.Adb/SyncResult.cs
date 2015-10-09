// <copyright file="SyncResult.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion. All rights reserved.
// </copyright>

namespace Managed.Adb
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    ///
    /// </summary>
    public sealed class SyncResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SyncResult"/> class.
        /// </summary>
        /// <param name="code">The error code</param>
        /// <param name="message">The error message</param>
        public SyncResult(int code, string message)
        {
            this.Message = message ?? ErrorCodeHelper.ErrorCodeToString(code);
            this.Code = code;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SyncResult"/> class.
        /// </summary>
        /// <param name="code">The error code</param>
        /// <param name="ex">The exception</param>
        public SyncResult(int code, Exception ex)
            : this(code, ex.Message)
            {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SyncResult"/> class.
        /// </summary>
        /// <param name="code">The error code</param>
        public SyncResult(int code)
            : this(code, (string)null)
            {
        }

        /// <summary>
        /// Gets the error message
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// Gets the error code
        /// </summary>
        public int Code { get; private set; }
    }
}
