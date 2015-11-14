using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SharpAdbClient.Tests
{
    [TestClass]
    public class ListingServiceTests : BaseDeviceTests
    {
        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void ResolveStorageDirectoryTest()
        {
            // Tests the path to the /storage/emulated/legacy folder on a Nexus 10 device.
            var device = this.GetFirstDevice();
            FileListingService fileListingService = new FileListingService(device);

            // Make sure the /storage/ folder is resolved correctly. It is not a symlink, but it may
            // contain symlinks - e.g. /storage/sdcard.
            // In previous builds, the /storage/ folder would be resolved to symlink of its children.
            FileEntry storage = fileListingService.FindFileEntry("/storage/");
            Assert.AreEqual("/storage/", storage.FullResolvedPath);

            FileEntry apk = fileListingService.FindFileEntry("/storage/emulated/legacy/a584a9d6-1e29-4a4b-b8fb-23aa3f378b56.apk");
            Assert.AreEqual("/mnt/shell/emulated/0/a584a9d6-1e29-4a4b-b8fb-23aa3f378b56.apk", apk.FullEscapedPath);
        }
    }
}
