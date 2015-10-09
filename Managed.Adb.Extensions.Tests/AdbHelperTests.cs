using Managed.Adb.IO;
using Managed.Adb.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Managed.Adb.Extensions.Tests
{
    [TestClass]
    public class AdbHelperTests : BaseDeviceTests
    {

        public class FileSyncProgressMonitor : ISyncProgressMonitor
        {

            public void Start(long totalWork)
            {
                Console.WriteLine("Starting Transfer");
                this.TotalWork = this.Remaining = totalWork;
                Transfered = 0;
            }

            public void Stop()
            {
                IsCanceled = true;
            }

            public bool IsCanceled { get; private set; }

            public void StartSubTask(String source, String destination)
            {
                Console.WriteLine("Syncing {0} -> {1}", source, destination);
            }

            public void Advance(long work)
            {
                Transfered += work;
                Remaining -= work;
                Console.WriteLine("Transfered {0} of {1} - {2} remaining", Transfered, TotalWork, Remaining);
            }

            public long TotalWork { get; set; }
            public long Remaining { get; set; }
            public long Transfered { get; set; }
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void FileListingServiceTest()
        {
            Device device = GetFirstDevice();
            FileListingService fileListingService = new FileListingService(device);

            FileEntry[] entries = fileListingService.GetChildren(fileListingService.Root, false, null);
            foreach (var item in entries)
            {
                Console.WriteLine(item.FullPath);
            }
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void SyncServicePullFileTest()
        {
            Device device = GetFirstDevice();
            FileListingService fileListingService = new FileListingService(device);

            using (ISyncService sync = device.SyncService)
            {
                String rfile = "/sdcard/bootanimations/bootanimation-cm.zip";
                FileEntry rentry = fileListingService.FindFileEntry(rfile);

                String lpath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                String lfile = Path.Combine(lpath, LinuxPath.GetFileName(rfile));
                FileInfo lfi = new FileInfo(lfile);
                SyncResult result = sync.PullFile(rfile, lfile, new FileSyncProgressMonitor());

                Assert.IsTrue(lfi.Exists);
                Assert.IsTrue(ErrorCodeHelper.RESULT_OK == result.Code, ErrorCodeHelper.ErrorCodeToString(result.Code));
                lfi.Delete();

                result = sync.PullFile(rentry, lfile, new FileSyncProgressMonitor());
                Assert.IsTrue(lfi.Exists);
                Assert.IsTrue(ErrorCodeHelper.RESULT_OK == result.Code, ErrorCodeHelper.ErrorCodeToString(result.Code));
                lfi.Delete();

            }
        }
        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void SyncServicePushFileTest()
        {
            String testFile = CreateTestFile();
            FileInfo localFile = new FileInfo(testFile);
            String remoteFile = String.Format("/sdcard/{0}", Path.GetFileName(testFile));
            Device device = GetFirstDevice();
            FileListingService fileListingService = new FileListingService(device);


            using (ISyncService sync = device.SyncService)
            {
                SyncResult result = sync.PushFile(localFile.FullName, remoteFile, new FileSyncProgressMonitor());
                Assert.IsTrue(ErrorCodeHelper.RESULT_OK == result.Code, ErrorCodeHelper.ErrorCodeToString(result.Code));
                FileEntry remoteEntry = null;
                remoteEntry = fileListingService.FindFileEntry(remoteFile);

                // check the size
                Assert.AreEqual<long>(localFile.Length, remoteEntry.Size);

                // clean up temp file on sdcard
                device.ExecuteShellCommand(String.Format("rm {0}", remoteEntry.FullEscapedPath), new ConsoleOutputReceiver());
            }
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void SyncServicePullFilesTest()
        {
            Device device = GetFirstDevice();
            FileListingService fileListingService = new FileListingService(device);

            using (ISyncService sync = device.SyncService)
            {
                String lpath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "apps");
                String rpath = "/system/app/";
                DirectoryInfo ldir = new DirectoryInfo(lpath);
                if (!ldir.Exists)
                {
                    ldir.Create();
                }
                FileEntry fentry = fileListingService.FindFileEntry(rpath);
                Assert.IsTrue(fentry.IsDirectory);

                FileEntry[] entries = fileListingService.GetChildren(fentry, false, null);
                SyncResult result = sync.Pull(entries, ldir.FullName, new FileSyncProgressMonitor());

                Assert.IsTrue(ErrorCodeHelper.RESULT_OK == result.Code, ErrorCodeHelper.ErrorCodeToString(result.Code));
            }
        }

        private String CreateTestFile()
        {
            String tfile = Path.GetTempFileName();
            Random r = new Random((int)DateTime.Now.Ticks);

            using (var fs = new FileStream(tfile, System.IO.FileMode.Create, FileAccess.Write))
            {
                for (int i = 0; i < 1024; i++)
                {
                    byte[] buffer = new byte[1024];
                    r.NextBytes(buffer);
                    fs.Write(buffer, 0, buffer.Length);
                }
            }
            return tfile;
        }
    }
}
