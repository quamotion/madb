using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace Managed.Adb.Tests {
	public class FileEntryTests : BaseDeviceTests {

		[Fact]
		public void FindEntryTest ( ) {
			Device device = GetFirstDevice ( );

			FileEntry fe = FileEntry.Find ( device, "/system/" );

			fe = FileEntry.Find ( device, "/system/bin/" );

			fe = FileEntry.Find ( device, "/mnt/sdcard/Android/data/com.camalotdesigns.myandroider/Injector.jar" );
			// test links
			fe = FileEntry.Find ( device, "/sdcard/Android/data/com.camalotdesigns.myandroider/Injector.jar" );

		}


		[Fact]
		public void FindOrCreateTest( ) {
			Device device = GetFirstDevice ( );
			var path ="/mnt/sdcard/test/delete/";
			FileEntry fe = FileEntry.FindOrCreate ( device, path );
			Console.WriteLine ( fe.FullResolvedPath );
			Assert.True ( fe.Exists );
			Assert.True(String.Compare(fe.FullPath,path,false ) == 0 );
		}


	}
}
