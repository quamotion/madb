// <copyright file="Priority.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion. All rights reserved.
// </copyright>

namespace SharpAdbClient.Logs
{
    /// <summary>
    /// Represents a log priority.
    /// </summary>
    /// <seealso href="https://developer.android.com/reference/android/util/Log.html#ASSERT"/>
    public enum Priority : byte
    {
        /// <summary>
        /// Represents a verbose message.
        /// </summary>
        Verbose = 2,

        /// <summary>
        /// Represents a debug message.
        /// </summary>
        Debug = 3,

        /// <summary>
        /// Represents an informational message.
        /// </summary>
        Info = 4,

        /// <summary>
        /// Reprents a warning.
        /// </summary>
        Warn = 5,

        /// <summary>
        /// Represents an error.
        /// </summary>
        Error = 6,

        /// <summary>
        /// Represents an assertion which failed.
        /// </summary>
        Assert = 7,
    }
}
