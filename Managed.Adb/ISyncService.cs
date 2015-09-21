//-----------------------------------------------------------------------
// <copyright file="ISyncService.cs" company="Quamotion">
//     Copyright (c) 2015 Quamotion. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Managed.Adb
{
    /// <summary>
    /// Interface containing methods for file synchronisation.
    /// </summary>
    public interface ISyncService: IDisposable
    {
        /// <include file='.\ISyncService.xml' path='/SyncService/PushFile/*'/>
        SyncResult PushFile(String local, String remote, ISyncProgressMonitor monitor);

        /// <include file='.\ISyncService.xml' path='/SyncService/Push/*'/>
        SyncResult Push(IEnumerable<String> local, FileEntry remote, ISyncProgressMonitor monitor);

        /// <include file='.\ISyncService.xml' path='/SyncService/PullFile2/*'/>
        SyncResult PullFile(String remoteFilepath, String localFilename, ISyncProgressMonitor monitor);

        /// <include file='.\ISyncService.xml' path='/SyncService/PullFile/*'/>
        SyncResult PullFile(FileEntry remote, String localFilename, ISyncProgressMonitor monitor);

        /// <include file='.\ISyncService.xml' path='/SyncService/Pull/*'/>
        SyncResult Pull(IEnumerable<FileEntry> entries, String localPath, ISyncProgressMonitor monitor);

        /// <include file='.\ISyncService.xml' path='/SyncService/Close/*'/>
        void Close();

        /// <include file='.\ISyncService.xml' path='/SyncService/Open/*'/>
        bool Open();

        /// <include file='.\ISyncService.xml' path='/SyncService/IsOpen/*'/>
        bool IsOpen {get; }
    }
}
