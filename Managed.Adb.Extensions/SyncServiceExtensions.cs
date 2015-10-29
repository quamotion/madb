using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Managed.Adb.Extensions
{
    public static class SyncServiceExtensions
    {
        /// <summary>
        /// Logging tag
        /// </summary>
        private const string TAG = "SyncServiceExtensions";

        /// <include file='.\ISyncService.xml' path='/SyncService/Pull/*'/>
        public static SyncResult Pull(this ISyncService syncService, IEnumerable<FileEntry> entries, String localPath, ISyncProgressMonitor monitor)
        {
            throw new NotImplementedException();
            /*
            if (monitor == null)
            {
                throw new ArgumentNullException("monitor", "Monitor cannot be null");
            }

            // first we check the destination is a directory and exists
            DirectoryInfo d = new DirectoryInfo(localPath);
            if (!d.Exists)
            {
                return new SyncResult(ErrorCodeHelper.RESULT_NO_DIR_TARGET);
            }

            if (!d.IsDirectory())
            {
                return new SyncResult(ErrorCodeHelper.RESULT_TARGET_IS_FILE);
            }

            // get a FileListingService object
            FileListingService fls = new FileListingService(syncService.Device);

            // compute the number of file to move
            long total = GetTotalRemoteFileSize(entries, fls);
            Log.i(TAG, "total transfer: {0}", total);

            // start the monitor
            monitor.Start(total);

            SyncResult result = syncService.DoPull(entries, localPath, fls, monitor);

            monitor.Stop();

            return result;
            */
        }

        /// <include file='.\ISyncService.xml' path='/SyncService/PullFile/*'/>
		public static SyncResult PullFile(this ISyncService syncService, FileEntry remote, String localFilename, ISyncProgressMonitor monitor)
        {
            if (monitor == null)
            {
                throw new ArgumentNullException("monitor", "Monitor cannot be null");
            }

            long total = remote.Size;
            monitor.Start(total);

            SyncResult result = syncService.DoPullFile(remote.FullPath, localFilename, monitor);

            monitor.Stop();
            return result;
        }

        /// <include file='.\ISyncService.xml' path='/SyncService/Push/*'/>
        public static SyncResult Push(this ISyncService syncService, IEnumerable<String> local, FileEntry remote, ISyncProgressMonitor monitor)
        {
            if (monitor == null)
            {
                throw new ArgumentNullException("monitor", "Monitor cannot be null");
            }

            if (!remote.IsDirectory)
            {
                return new SyncResult(ErrorCodeHelper.RESULT_REMOTE_IS_FILE);
            }

            // make a list of File from the list of String
            List<FileSystemInfo> files = new List<FileSystemInfo>();
            foreach (String path in local)
            {
                files.Add(path.GetFileSystemInfo());
            }

            // get the total count of the bytes to transfer
            long total = syncService.GetTotalLocalFileSize(files);

            monitor.Start(total);
            SyncResult result = syncService.DoPush(files, remote.FullPath, monitor);
            monitor.Stop();

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entries"></param>
        /// <param name="localPath"></param>
        /// <param name="fls"></param>
        /// <param name="monitor"></param>
        /// <returns></returns>
        /// <exception cref="System.IO.IOException">Throws if unable to create a file or folder</exception>
        /// <exception cref="System.ArgumentNullException">Throws if the ISyncProgressMonitor is null</exception>
        private static SyncResult DoPull(this ISyncService syncService, IEnumerable<FileEntry> entries, string localPath, FileListingService fileListingService, ISyncProgressMonitor monitor)
        {
            if (monitor == null)
            {
                throw new ArgumentNullException("monitor", "Monitor cannot be null");
            }

            // check if we're cancelled
            if (monitor.IsCanceled)
            {

                return new SyncResult(ErrorCodeHelper.RESULT_CANCELED);
            }

            // check if we need to create the local directory
            DirectoryInfo localDir = new DirectoryInfo(localPath);
            if (!localDir.Exists)
            {
                localDir.Create();
            }

            foreach (FileEntry e in entries)
            {
                // check if we're canceled
                if (monitor.IsCanceled)
                {
                    return new SyncResult(ErrorCodeHelper.RESULT_CANCELED);
                }

                // the destination item (folder or file)


                String dest = Path.Combine(localPath, e.Name);

                // get type (we only pull directory and files for now)
                FileListingService.FileTypes type = e.Type;
                if (type == FileListingService.FileTypes.Directory)
                {
                    monitor.StartSubTask(e.FullPath, dest);
                    // then recursively call the content. Since we did a ls command
                    // to get the number of files, we can use the cache
                    FileEntry[] children = fileListingService.GetChildren(e, true, null);
                    SyncResult result = syncService.DoPull(children, dest, fileListingService, monitor);
                    if (result.Code != ErrorCodeHelper.RESULT_OK)
                    {
                        return result;
                    }
                    monitor.Advance(1);
                }
                else if (type == FileListingService.FileTypes.File)
                {
                    monitor.StartSubTask(e.FullPath, dest);
                    SyncResult result = syncService.DoPullFile(e.FullPath, dest, monitor);
                    if (result.Code != ErrorCodeHelper.RESULT_OK)
                    {
                        return result;
                    }
                }
                else if (type == FileListingService.FileTypes.Link)
                {
                    monitor.StartSubTask(e.FullPath, dest);
                    SyncResult result = syncService.DoPullFile(e.FullResolvedPath, dest, monitor);
                    if (result.Code != ErrorCodeHelper.RESULT_OK)
                    {
                        return result;
                    }
                }
                else
                {
                    Log.d("ddms-sync", String.Format("unknown type to transfer: {0}", type));
                }
            }

            return new SyncResult(ErrorCodeHelper.RESULT_OK);
        }

        /// <summary>
        /// compute the recursive file size of all the files in the list. Folder have a weight of 1.
        /// </summary>
        /// <param name="entries">The remote files</param>
        /// <param name="fls">The FileListingService</param>
        /// <returns>The total number of bytes of the specified remote files</returns>
        private static long GetTotalRemoteFileSize(IEnumerable<FileEntry> entries, FileListingService fls)
        {
            long count = 0;
            foreach (FileEntry e in entries)
            {
                FileListingService.FileTypes type = e.Type;
                if (type == FileListingService.FileTypes.Directory)
                {
                    // get the children
                    IEnumerable<FileEntry> children = fls.GetChildren(e, false, null);
                    count += GetTotalRemoteFileSize(children, fls) + 1;
                }
                else if (type == FileListingService.FileTypes.File)
                {
                    count += e.Size;
                }
            }

            return count;
        }
    }
}
