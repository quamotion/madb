// <copyright file="SyncCommand.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion. All rights reserved.
// </copyright>

namespace Managed.Adb
{
    public enum SyncCommand
    {
        /// <summary>
        /// List the files in a folder.
        /// </summary>
        LIST,

        /// <summary>
        /// Retrieve a file from device
        /// </summary>
        RECV,

        /// <summary>
        /// Send a file to device
        /// </summary>
        SEND,

        /// <summary>
        /// Stat a file
        /// </summary>
        STAT,

        DENT,

        FAIL,

        DATA,

        OKAY,

        DONE
    }
}
