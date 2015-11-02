// <copyright file="ShellCommandUnresponsiveException.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion. All rights reserved.
// </copyright>

namespace Managed.Adb.Exceptions
{
    /// <summary>
    /// The exception that is thrown when a shell command becomes unresponsive.
    /// </summary>
    public class ShellCommandUnresponsiveException : AdbException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ShellCommandUnresponsiveException"/> class.
        /// </summary>
        public ShellCommandUnresponsiveException()
            : base("The shell command has become unresponsive")
        {
        }
    }
}
