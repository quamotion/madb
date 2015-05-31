using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Managed.Adb.Tests {
    [TestClass]
	public class FileEntryTests : BaseDeviceTests {

        [TestMethod]
        [TestCategory("IntegrationTest")]
		public void FindEntryTest ( ) {
			Device device = GetFirstDevice ( );

			FileEntry fe = FileEntry.Find ( device, "/system/" );

			fe = FileEntry.Find ( device, "/system/bin/" );

			fe = FileEntry.Find ( device, "/mnt/sdcard/Android/data/com.camalotdesigns.myandroider/Injector.jar" );
			// test links
			fe = FileEntry.Find ( device, "/sdcard/Android/data/com.camalotdesigns.myandroider/Injector.jar" );

		}


        [TestMethod]
        [TestCategory("IntegrationTest")]
		public void FindOrCreateTest( ) {
			Device device = GetFirstDevice ( );
			var path ="/mnt/sdcard/test/delete/";
			FileEntry fe = FileEntry.FindOrCreate ( device, path );
			Assert.IsTrue ( fe.Exists );
			device.FileSystem.Delete ( fe.FullResolvedPath );
		}


	}
}
