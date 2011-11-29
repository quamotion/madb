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
