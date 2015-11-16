using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpAdbClient.Tests
{
    [TestClass]
    public class ListingServiceReceiverTests
    {
        /// <summary>
        /// Tests the <see cref="ListingServiceReceiver"/> in a scenario where one of the files could not be
        /// accessed because of an error. This error output is visible in the shell output.
        /// </summary>
        [TestMethod]
        public void ParseListingWithErrorTest()
        {
            DummyDevice device = new DummyDevice();
            FileEntry root = new FileEntry(device, "/");
            List<FileEntry> entries = new List<FileEntry>();
            List<string> links = new List<string>();

            ListingServiceReceiver receiver = new ListingServiceReceiver(root, entries, links);

            string output = @"drwxr-xr-x root     root              2015-06-01 10:17 acct
drwxrwx--- system   cache             2015-05-13 02:03 cache
-rw-r--r-- root     root          297 1970-01-01 01:00 default.prop
lstat '//factory' failed: Permission denied
lrwxrwxrwx root     root              2015-06-01 10:17 etc -> /system/etc";

            string[] lines = output.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                receiver.AddOutput(line);
            }

            receiver.Flush();
            receiver.FinishLinks();

            Assert.AreEqual<int>(4, entries.Count);

            // Validate the first entry (/acct/)
            // drwxr-xr-x root     root              2015-06-01 10:17 acct
            Assert.AreEqual(new DateTime(2015, 6, 1, 10, 17, 00), entries[0].Date);
            Assert.AreEqual(device, entries[0].Device);
            Assert.IsTrue(entries[0].Exists);
            Assert.AreEqual(0, entries[0].FetchTime);
            Assert.AreEqual("/acct", entries[0].FullEscapedPath);
            Assert.AreEqual("/acct/", entries[0].FullPath);
            Assert.AreEqual("root", entries[0].Group);
            Assert.IsNull(entries[0].Info);
            Assert.IsFalse(entries[0].IsApplicationFileName);
            Assert.IsFalse(entries[0].IsApplicationPackage);
            Assert.IsTrue(entries[0].IsDirectory);
            Assert.IsFalse(entries[0].IsExecutable);
            Assert.IsFalse(entries[0].IsLink);
            Assert.IsFalse(entries[0].IsRoot);
            Assert.IsNull(entries[0].LinkName);
            Assert.AreEqual("acct", entries[0].Name);
            Assert.IsTrue(entries[0].NeedFetch);
            Assert.AreEqual("root", entries[0].Owner);
            Assert.AreEqual(root, entries[0].Parent);
            Assert.AreEqual(1, entries[0].PathSegments.Length);
            Assert.AreEqual("acct", entries[0].PathSegments[0]);
            Assert.AreEqual("rwxr-tr-t", entries[0].Permissions.ToString());
            Assert.AreEqual(0, entries[0].Size);
            Assert.AreEqual(FileListingService.FileTypes.Directory, entries[0].Type);

            // Validate the second entry (/cache)
            // drwxrwx--- system   cache             2015-05-13 02:03 cache
            Assert.AreEqual(new DateTime(2015, 5, 13, 2, 3, 00), entries[1].Date);
            Assert.AreEqual(device, entries[1].Device);
            Assert.IsTrue(entries[1].Exists);
            Assert.AreEqual(0, entries[1].FetchTime);
            Assert.AreEqual("/cache", entries[1].FullEscapedPath);
            Assert.AreEqual("/cache/", entries[1].FullPath);
            Assert.AreEqual("cache", entries[1].Group);
            Assert.IsNull(entries[1].Info);
            Assert.IsFalse(entries[1].IsApplicationFileName);
            Assert.IsFalse(entries[1].IsApplicationPackage);
            Assert.IsTrue(entries[1].IsDirectory);
            Assert.IsFalse(entries[1].IsExecutable);
            Assert.IsFalse(entries[1].IsLink);
            Assert.IsFalse(entries[1].IsRoot);
            Assert.IsNull(entries[1].LinkName);
            Assert.AreEqual("cache", entries[1].Name);
            Assert.IsTrue(entries[1].NeedFetch);
            Assert.AreEqual("system", entries[1].Owner);
            Assert.AreEqual(root, entries[1].Parent);
            Assert.AreEqual(1, entries[1].PathSegments.Length);
            Assert.AreEqual("cache", entries[1].PathSegments[0]);
            Assert.AreEqual("rwxrwx---", entries[1].Permissions.ToString());
            Assert.AreEqual(0, entries[1].Size);
            Assert.AreEqual(FileListingService.FileTypes.Directory, entries[1].Type);

            // Validate the third entry (/default.prop)
            // -rw-r--r-- root     root          297 1970-01-01 01:00 default.prop
            Assert.AreEqual(new DateTime(1970, 1, 1, 1, 0, 0), entries[2].Date);
            Assert.AreEqual(device, entries[2].Device);
            Assert.IsTrue(entries[2].Exists);
            Assert.AreEqual(0, entries[2].FetchTime);
            Assert.AreEqual("/default.prop", entries[2].FullEscapedPath);
            Assert.AreEqual("/default.prop", entries[2].FullPath);
            Assert.AreEqual("root", entries[2].Group);
            Assert.IsNull(entries[2].Info);
            Assert.IsFalse(entries[2].IsApplicationFileName);
            Assert.IsFalse(entries[2].IsApplicationPackage);
            Assert.IsFalse(entries[2].IsDirectory);
            Assert.IsFalse(entries[2].IsExecutable);
            Assert.IsFalse(entries[2].IsLink);
            Assert.IsFalse(entries[2].IsRoot);
            Assert.IsNull(entries[2].LinkName);
            Assert.AreEqual("default.prop", entries[2].Name);
            Assert.IsTrue(entries[2].NeedFetch);
            Assert.AreEqual("root", entries[2].Owner);
            Assert.AreEqual(root, entries[2].Parent);
            Assert.AreEqual(1, entries[2].PathSegments.Length);
            Assert.AreEqual("default.prop", entries[2].PathSegments[0]);
            Assert.AreEqual("rw-r--r--", entries[2].Permissions.ToString());
            Assert.AreEqual(297, entries[2].Size);
            Assert.AreEqual(FileListingService.FileTypes.File, entries[2].Type);

            // Validate the fourth and final entry (/etc)
            // lrwxrwxrwx root     root              2015-06-01 10:17 etc -> /system/etc
            Assert.AreEqual(new DateTime(2015, 6, 1, 10, 17, 0), entries[3].Date);
            Assert.AreEqual(device, entries[3].Device);
            Assert.IsTrue(entries[3].Exists);
            Assert.AreEqual(0, entries[3].FetchTime);
            Assert.AreEqual("/system/etc", entries[3].FullEscapedPath);
            Assert.AreEqual("/etc/", entries[3].FullPath);
            Assert.AreEqual("root", entries[3].Group);
            Assert.AreEqual("-> /system/etc", entries[3].Info);
            Assert.IsFalse(entries[3].IsApplicationFileName);
            Assert.IsFalse(entries[3].IsApplicationPackage);
            Assert.IsTrue(entries[3].IsDirectory);
            Assert.IsFalse(entries[3].IsExecutable);
            Assert.IsTrue(entries[3].IsLink);
            Assert.IsFalse(entries[3].IsRoot);
            Assert.AreEqual("/system/etc", entries[3].LinkName);
            Assert.AreEqual("etc", entries[3].Name);
            Assert.IsTrue(entries[3].NeedFetch);
            Assert.AreEqual("root", entries[3].Owner);
            Assert.AreEqual(root, entries[3].Parent);
            Assert.AreEqual(1, entries[3].PathSegments.Length);
            Assert.AreEqual("etc", entries[3].PathSegments[0]);
            Assert.AreEqual("rwxrwxrwx", entries[3].Permissions.ToString());
            Assert.AreEqual(0, entries[3].Size);
            Assert.AreEqual(FileListingService.FileTypes.DirectoryLink, entries[3].Type);
        }
    }
}
