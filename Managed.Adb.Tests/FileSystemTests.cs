using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Managed.Adb.MoreLinq;

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
	}
}
