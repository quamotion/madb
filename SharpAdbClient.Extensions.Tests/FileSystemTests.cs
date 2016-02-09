using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SharpAdbClient.Tests {
    [TestClass]
	public class FileSystemTests : BaseDeviceTests {
        [TestMethod]
        [TestCategory("IntegrationTest")]
		public void GetDeviceBlocksTest( ) {
			Device d = GetFirstDevice ( );
            FileSystem fileSystem = new FileSystem(d);

			IEnumerable<FileEntry> blocks = fileSystem.DeviceBlocks;
            foreach(var b in blocks)
            {
				Console.WriteLine ( b.ToString ( ) );
			}
			Assert.IsTrue ( blocks.Count() > 0 );
		}

        [TestMethod]
        [TestCategory("IntegrationTest")]
		public void MakeDirectory( ) {
			Device d = GetFirstDevice ( );
            FileSystem fileSystem = new FileSystem(d);

            var testPath = "/mnt/sdcard/test/delete/";
			Console.WriteLine ( "Making directory: {0}", testPath );
			fileSystem.MakeDirectory ( testPath );
			Assert.IsTrue ( fileSystem.Exists ( testPath ) );
			Console.WriteLine ( "Deleting {0}", testPath );
			fileSystem.Delete(testPath);
			Assert.IsTrue ( !fileSystem.Exists ( testPath ) );

			Console.WriteLine ( "Making directory (forced): {0}", testPath );
			fileSystem.MakeDirectory ( testPath,true );
			Assert.IsTrue ( fileSystem.Exists ( testPath ) );
			Console.WriteLine ( "Deleting {0}", testPath );
			fileSystem.Delete ( testPath );
			Assert.IsTrue ( !fileSystem.Exists ( testPath ) );
		}

        [TestMethod]
        [TestCategory("IntegrationTest")]
		public void ResolveLink ( ) {
			Device d = GetFirstDevice ( );
            FileSystem fileSystem = new FileSystem(d);

            var vendor = fileSystem.ResolveLink ( "/vendor" );
			Assert.AreEqual ( vendor, "/system/vendor" );
			Console.WriteLine ( $"/vendor -> {vendor}");

			var nonsymlink = fileSystem.ResolveLink ( "/system" );
			Assert.AreEqual ( nonsymlink, "/system" );
			Console.WriteLine ( $"/system -> {nonsymlink}");


			var legacy = "/storage/emulated/legacy";
			var sdcard0 = "/storage/sdcard0";

			var sdcard = fileSystem.ResolveLink ( "/sdcard" );
			// depending on the version of android
			Assert.IsTrue ( sdcard.Equals ( legacy ) || sdcard.Equals ( sdcard0 ) );
			Console.WriteLine ( $"/sdcard -> {sdcard}");

		}
	}
}
