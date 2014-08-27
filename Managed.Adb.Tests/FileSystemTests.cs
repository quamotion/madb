using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using MoreLinq;

namespace Managed.Adb.Tests {
	public class FileSystemTests : BaseDeviceTests {
		[Fact]
		public void GetDeviceBlocksTest( ) {
			Device d = GetFirstDevice ( );
			IEnumerable<FileEntry> blocks = d.FileSystem.DeviceBlocks;
			blocks.ForEach ( b => {
				Console.WriteLine ( b.ToString ( ) );
			} );
			Assert.True ( blocks.Count() > 0 );
		}

		[Fact]
		public void MakeDirectory( ) {
			Device d = GetFirstDevice ( );
			var testPath = "/mnt/sdcard/test/delete/";
			Console.WriteLine ( "Making directory: {0}", testPath );
			d.FileSystem.MakeDirectory ( testPath );
			Assert.True ( d.FileSystem.Exists ( testPath ) );
			Console.WriteLine ( "Deleting {0}", testPath );
			d.FileSystem.Delete(testPath);
			Assert.True ( !d.FileSystem.Exists ( testPath ) );

			Console.WriteLine ( "Making directory (forced): {0}", testPath );
			d.FileSystem.MakeDirectory ( testPath,true );
			Assert.True ( d.FileSystem.Exists ( testPath ) );
			Console.WriteLine ( "Deleting {0}", testPath );
			d.FileSystem.Delete ( testPath );
			Assert.True ( !d.FileSystem.Exists ( testPath ) );
		}

		[Fact]
		public void ResolveLink ( ) {
			Device d = GetFirstDevice ( );
			var vendor = d.FileSystem.ResolveLink ( "/vendor" );
			Assert.Equal ( vendor, "/system/vendor" );
			Console.WriteLine ( "/vendor -> {0}".With ( vendor ) );

			var nonsymlink = d.FileSystem.ResolveLink ( "/system" );
			Assert.Equal ( nonsymlink, "/system" );
			Console.WriteLine ( "/system -> {0}".With ( nonsymlink ) );


			var legacy = "/storage/emulated/legacy";
			var sdcard0 = "/storage/sdcard0";

			var sdcard = d.FileSystem.ResolveLink ( "/sdcard" );
			// depending on the version of android
			Assert.True ( sdcard.Equals ( legacy ) || sdcard.Equals ( sdcard0 ) );
			Console.WriteLine ( "/sdcard -> {0}".With ( sdcard ) );

		}
	}
}
