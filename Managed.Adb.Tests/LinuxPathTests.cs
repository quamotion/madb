using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Managed.Adb.IO;
using Xunit;

namespace Managed.Adb.Tests {
	public class LinuxPathTests {

		[Fact]
		public void CombineTest ( ) {
			String result = LinuxPath.Combine ( "/system", "busybox" );
			Assert.Equal<String> ( "/system/busybox", result );

			result = LinuxPath.Combine ( "/system/", "busybox" );
			Assert.Equal<String> ( "/system/busybox", result );

			result = LinuxPath.Combine ( "/system/xbin", "busybox" );
			Assert.Equal<String> ( "/system/xbin/busybox", result );

			result = LinuxPath.Combine ( "/system/xbin/", "busybox" );
			Assert.Equal<String> ( "/system/xbin/busybox", result );

			result = LinuxPath.Combine ( "/system//xbin", "busybox" );
			Assert.Equal<String> ( "/system/xbin/busybox", result );
		}

		[Fact]
		public void GetDirectoryNameTest ( ) {
			String result = LinuxPath.GetDirectoryName ( "/system/busybox" );
			Assert.Equal<String> ( "/system/", result );

			result = LinuxPath.GetDirectoryName ( "/" );
			Assert.Equal<String> ( "/", result );

			result = LinuxPath.GetDirectoryName ( "/system/xbin/" );
			Assert.Equal<String> ( "/system/xbin/", result );
		}

		[Fact]
		public void GetFileNameTest ( ) {
			String result = LinuxPath.GetFileName ( "/system/busybox" );
			Assert.Equal<String> ( "busybox", result );

			result = LinuxPath.GetFileName ( "/" );
			Assert.Equal<String> ( "", result );

			result = LinuxPath.GetFileName ( "/system/xbin/" );
			Assert.Equal<String> ( "", result );

			result = LinuxPath.GetFileName ( "/system/xbin/file.ext" );
			Assert.Equal<String> ( "file.ext", result );
		}


		[Fact]
		public void GetFileNameExtensionTest ( ) {
			String result = LinuxPath.GetExtension ( "/system/busybox" );
			Assert.Equal<String> ( "", result );

			result = LinuxPath.GetExtension ( "/" );
			Assert.Equal<String> ( "", result );

			result = LinuxPath.GetExtension ( "/system/xbin/" );
			Assert.Equal<String> ( "", result );

			result = LinuxPath.GetExtension ( "/system/xbin/file.ext" );
			Assert.Equal<String> ( ".ext", result );
		}

		[Fact]
		public void GetFileNameWithoutExtensionTest ( ) {
			String result = LinuxPath.GetFileNameWithoutExtension ( "/system/busybox" );
			Assert.Equal<String> ( "busybox", result );

			result = LinuxPath.GetFileNameWithoutExtension ( "/" );
			Assert.Equal<String> ( "", result );

			result = LinuxPath.GetFileNameWithoutExtension ( "/system/xbin/" );
			Assert.Equal<String> ( "", result );

			result = LinuxPath.GetFileNameWithoutExtension ( "/system/xbin/file.ext" );
			Assert.Equal<String> ( "file", result );
		}

		[Fact]
		public void ChangeExtensionTest ( ) {
			String result = LinuxPath.ChangeExtension ( "/system/busybox", "foo" );
			Assert.Equal<String> ( "/system/busybox.foo", result );

			result = LinuxPath.ChangeExtension ( "/system/xbin/file.ext", "myext" );
			Assert.Equal<String> ( "/system/xbin/file.myext", result );

			result = LinuxPath.ChangeExtension ( "/system/xbin/file.ext", "" );
			Assert.Equal<String> ( "/system/xbin/file", result );

			result = LinuxPath.ChangeExtension ( "/system/busybox.foo", "" );
			Assert.Equal<String> ( "/system/busybox", result );
		}

		[Fact]
		public void GetPathWithoutFileTest ( ) {
			String result = LinuxPath.GetPathWithoutFile ( "/system/busybox" );
			Assert.Equal<String> ( "/system/", result );

			result = LinuxPath.GetPathWithoutFile ( "/system/xbin/" );
			Assert.Equal<String> ( "/system/xbin/", result );

			result = LinuxPath.GetPathWithoutFile ( "/system/xbin/file.ext");
			Assert.Equal<String> ( "/system/xbin/", result );
		}

		[Fact]
		public void GetPathRootTest ( ) {
			String result = LinuxPath.GetPathRoot ( "/system/busybox" );
			Assert.Equal<String> ( "/", result );

			result = LinuxPath.GetPathRoot ( "/system/xbin/" );
			Assert.Equal<String> ( "/", result );

			result = LinuxPath.GetPathRoot ( "/system/xbin/file.ext" );
			Assert.Equal<String> ( "/", result );
		}


		[Fact]
		public void IsPathRootedTest ( ) {
			bool result = LinuxPath.IsPathRooted ( "/system/busybox" );
			Assert.Equal<bool> ( true, result );

			result = LinuxPath.IsPathRooted ( "/system/xbin/" );
			Assert.Equal<bool> ( true, result );

			result = LinuxPath.IsPathRooted ( "system/xbin/" );
			Assert.Equal<bool> ( false, result );
		}

		[Fact]
		public void HasExtensionTest ( ) {
			bool result = LinuxPath.HasExtension ( "/system/busybox" );
			Assert.Equal<bool> ( false, result );

			result = LinuxPath.HasExtension ( "/system/xbin.foo/" );
			Assert.Equal<bool> ( false, result );

			result = LinuxPath.HasExtension ( "system/xbin/file.ext" );
			Assert.Equal<bool> ( true, result );
		}
	}
}
