using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using System.IO;
using Managed.Adb.IO;

namespace Managed.Adb.Tests {
	public class AdbHelperTests {
		[Fact]
		public void GetDevicesTest ( ) {
			List<Device> devices = AdbHelper.Instance.GetDevices ( AndroidDebugBridge.SocketAddress );
			Assert.True ( devices.Count >= 1 );
			foreach ( var item in devices ) {
				Console.WriteLine ( "{0}-{1}", item.SerialNumber, item.State );
			}
		}

		[Fact]
		public void ExecuteRemoteCommandTest ( ) {
			Device device = GetFirstDevice ( );
			ConsoleReceiver creciever = new ConsoleReceiver ( );

			Assert.DoesNotThrow ( new Assert.ThrowsDelegate ( delegate ( ) {
				try {
					AdbHelper.Instance.ExecuteRemoteCommand ( AndroidDebugBridge.SocketAddress, "ls -lF --color=never", device, creciever );
				} catch ( FileNotFoundException fex ) {
					AdbHelper.Instance.ExecuteRemoteCommand ( AndroidDebugBridge.SocketAddress, "ls -l", device, creciever );
				}
			} ) );


			Assert.DoesNotThrow ( new Assert.ThrowsDelegate ( delegate ( ) {
				bool hasBB = false;
				try {
					AdbHelper.Instance.ExecuteRemoteCommand ( AndroidDebugBridge.SocketAddress, "busybox", device, creciever );
					hasBB = true;
				} catch ( FileNotFoundException ) {
					hasBB = false;
				} finally {
					Console.WriteLine ( "Busybox enabled: {0}", hasBB );
				}
			} ) );

			Assert.Throws<FileNotFoundException> ( new Assert.ThrowsDelegate ( delegate ( ) {
				AdbHelper.Instance.ExecuteRemoteCommand ( AndroidDebugBridge.SocketAddress, "notsobusybox", device, creciever );
			} ) );

			Assert.Throws<FileNotFoundException> ( new Assert.ThrowsDelegate ( delegate ( ) {
				AdbHelper.Instance.ExecuteRemoteCommand ( AndroidDebugBridge.SocketAddress, "ls /system/foo", device, creciever );
			} ) );

		}

		[Fact]
		public void GetRawImageTest ( ) {
			RawImage rawImage = AdbHelper.Instance.GetFrameBuffer ( AndroidDebugBridge.SocketAddress, GetFirstDevice ( ) );

			Assert.NotNull ( rawImage );
			Assert.Equal<int> ( 16, rawImage.Bpp );
			Assert.Equal<int> ( 320, rawImage.Width );
			Assert.Equal<int> ( 480, rawImage.Height );

		}

		[Fact]
		public void FileListingServiceTest ( ) {
			FileListingService fls = new FileListingService ( GetFirstDevice ( ), false );
			//ListingServiceReceiver lsr = new ListingServiceReceiver(null,null,null);
			FileEntry[] entries = fls.GetChildren ( fls.Root, false, null );
			foreach ( var item in entries ) {
				Console.WriteLine ( item.FullPath );
			}
		}

		[Fact]
		public void SyncServicePullFileTest ( ) {
			using ( SyncService sync = new SyncService ( AndroidDebugBridge.SocketAddress, GetFirstDevice ( ) ) ) {
				FileListingService fls = new FileListingService ( GetFirstDevice ( ), false );
				String rfile = "/sdcard/bootanimations/bootanimation-cm.zip";
				FileEntry rentry = fls.FindEntry ( rfile );

				String lpath = Environment.GetFolderPath ( Environment.SpecialFolder.DesktopDirectory );
				String lfile = Path.Combine ( lpath, LinuxPath.GetFileName ( rfile ) );
				FileInfo lfi = new FileInfo ( lfile );
				SyncResult result = sync.PullFile ( rfile, lfile, new FileSyncProgressMonitor ( ) );

				Assert.True ( lfi.Exists );
				Assert.True ( ErrorCodeHelper.RESULT_OK == result.Code, ErrorCodeHelper.ErrorCodeToString ( result.Code ) );
				lfi.Delete ( );

				result = sync.PullFile ( rentry, lfile, new FileSyncProgressMonitor ( ) );
				Assert.True ( lfi.Exists );
				Assert.True ( ErrorCodeHelper.RESULT_OK == result.Code, ErrorCodeHelper.ErrorCodeToString ( result.Code ) );
				lfi.Delete ( );

			}
		}

		[Fact]
		public void SyncServicePushFileTest ( ) {
			String testFile = CreateTestFile ( );
			FileInfo localFile = new FileInfo ( testFile );
			String remoteFile = String.Format ( "/sdcard/{0}", Path.GetFileName ( testFile ) );
			Device device = GetFirstDevice ( );
			using ( SyncService sync = new SyncService ( AndroidDebugBridge.SocketAddress, device ) ) {
				FileListingService fls = new FileListingService ( device, false );

				SyncResult result = sync.PushFile ( localFile.FullName,
					remoteFile, new FileSyncProgressMonitor ( ) );
				Assert.True ( ErrorCodeHelper.RESULT_OK == result.Code, ErrorCodeHelper.ErrorCodeToString ( result.Code ) );
				FileEntry remoteEntry = null;
				Assert.DoesNotThrow ( new Assert.ThrowsDelegate ( delegate ( ) {
					remoteEntry = fls.FindEntry ( remoteFile );
				} ) );

				// check the size
				Assert.Equal<long> ( localFile.Length, remoteEntry.Size );

				// clean up temp file on sdcard
				AdbHelper.Instance.ExecuteRemoteCommand ( AndroidDebugBridge.SocketAddress, String.Format ( "rm {0}", remoteEntry.FullEscapedPath ), device, new ConsoleReceiver ( ) );
			}
		}

		[Fact]
		public void SyncServicePullFilesTest ( ) {
			Assert.True ( false );
			// doesnt work yet...
			using ( SyncService sync = new SyncService ( AndroidDebugBridge.SocketAddress, GetFirstDevice ( ) ) ) {
				FileListingService fls = new FileListingService ( GetFirstDevice ( ), false );
				String lpath = Path.Combine(Environment.GetFolderPath ( Environment.SpecialFolder.DesktopDirectory ),"bin");
				String rpath = "/system/bin";
				DirectoryInfo ldir = new DirectoryInfo(lpath);
				
				FileEntry fentry = fls.FindEntry(rpath);
				Assert.True(fentry.IsDirectory);

				FileEntry[] entries = fls.GetChildren ( fentry, true, new FileListingServiceReceiver() );
				//Console.WriteLine ( "Count: {0}", entries.Length );
				sync.Pull ( entries, ldir.FullName, new FileSyncProgressMonitor ( ) );

			}
		}

		private String CreateTestFile ( ) {
			String tfile = Path.GetTempFileName ( );
			Random r = new Random ( (int)DateTime.Now.Ticks );

			using ( var fs = new FileStream ( tfile, System.IO.FileMode.Create, FileAccess.Write ) ) {
				for ( int i = 0; i < 1024; i++ ) {
					byte[] buffer = new byte[1024];
					r.NextBytes ( buffer );
					fs.Write ( buffer, 0, buffer.Length );
				}
			}
			return tfile;
		}

		private Device GetFirstDevice ( ) {
			List<Device> devices = AdbHelper.Instance.GetDevices ( AndroidDebugBridge.SocketAddress );
			Assert.True ( devices.Count >= 1 );
			return devices[0];
		}

		public class FileListingServiceReceiver : IListingReceiver {

			public void SetChildren ( FileEntry entry, FileEntry[] children ) {
				entry.Children.Clear ( );
				entry.Children.AddRange ( children );
			}

			public void RefreshEntry ( FileEntry entry ) {
				entry.FetchTime = 0;
			}
		}

		public class FileSyncProgressMonitor : ISyncProgressMonitor {

			public void Start ( long totalWork ) {
				Console.WriteLine ( "Starting Transfer" );
				this.TotalWork = this.Remaining = totalWork;
				Transfered = 0;
			}

			public void Stop ( ) {
				IsCanceled = true;
			}

			public bool IsCanceled { get; private set; }

			public void StartSubTask ( string name ) {
				Console.WriteLine ( "Syncing {0}", name );
			}

			public void Advance ( long work ) {
				Transfered += work;
				Remaining -= work;
				Console.WriteLine ( "Transfered {0} of {1} - {2} remaining", Transfered, TotalWork, Remaining );
			}

			public long TotalWork { get; set; }
			public long Remaining { get; set; }
			public long Transfered { get; set; }
		}

		public class ConsoleReceiver : MultiLineReceiver {

			public override void ProcessNewLines ( string[] lines ) {
				foreach ( var line in lines ) {
					Console.WriteLine ( line );
				}
			}
		}
	}
}
