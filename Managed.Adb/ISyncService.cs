//-----------------------------------------------------------------------
// <copyright file="ISyncService.cs" company="Quamotion">
//     Copyright (c) 2015 Quamotion. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Managed.Adb
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Interface containing methods for file synchronisation.
    /// </summary>
    public interface ISyncService : IDisposable
    {
        DeviceData Device { get; }

        /// <include file='.\ISyncService.xml' path='/SyncService/PushFile/*'/>
        SyncResult PushFile(string local, string remote, ISyncProgressMonitor monitor);

        /// <include file='.\ISyncService.xml' path='/SyncService/PullFile2/*'/>
        SyncResult PullFile(string remoteFilepath, string localFilename, ISyncProgressMonitor monitor);

        /// <include file='.\ISyncService.xml' path='/SyncService/Close/*'/>
        void Close();

        /// <include file='.\ISyncService.xml' path='/SyncService/Open/*'/>
        void Open();

        /// <include file='.\ISyncService.xml' path='/SyncService/IsOpen/*'/>
        bool IsOpen {get; }

        SyncResult DoPush(IEnumerable<FileSystemInfo> files, string remotePath, ISyncProgressMonitor monitor);

        SyncResult DoPullFile(string remotePath, string localPath, ISyncProgressMonitor monitor);

        long GetTotalLocalFileSize(IEnumerable<FileSystemInfo> fsis);
    }
}
