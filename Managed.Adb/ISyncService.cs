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
    using System.Threading;

    /// <summary>
    /// Interface containing methods for file synchronisation.
    /// </summary>
    public interface ISyncService : IDisposable
    {
        void Push(Stream stream, string remotePath, int permissions, IProgress<int> progress, CancellationToken cancellationToken);

        /// <include file='.\ISyncService.xml' path='/SyncService/PullFile2/*'/>
        void Pull(string remoteFilepath, Stream stream, IProgress<int> progress, CancellationToken cancellationToken);

        /// <include file='.\ISyncService.xml' path='/SyncService/Close/*'/>
        void Close();

        /// <include file='.\ISyncService.xml' path='/SyncService/Open/*'/>
        void Open();

        /// <include file='.\ISyncService.xml' path='/SyncService/IsOpen/*'/>
        bool IsOpen { get; }
    }
}
