using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        STAT
    }
}
