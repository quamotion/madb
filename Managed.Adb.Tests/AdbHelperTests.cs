using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Managed.Adb.IO;
using System.Drawing.Imaging;
using Managed.Adb.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Managed.Adb.Tests {
    [TestClass]
	public class AdbHelperTests : BaseDeviceTests {


		[TestMethod]
        [TestCategory("IntegrationTest")]
		public void GetDevicesTest ( ) {
			List<Device> devices = AdbHelper.Instance.GetDevices ( AndroidDebugBridge.SocketAddress );
			Assert.IsTrue ( devices.Count >= 1 );
			foreach ( var item in devices ) {
				Console.WriteLine ( "{0}\t{1}{2}", item.SerialNumber, item.State, item.IsEmulator ? " - Emulator" : String.Empty );
			}
		}

        [TestMethod]
        [TestCategory("IntegrationTest")]
		public void KillAdbTest ( ) {
			var device = GetFirstDevice ( );
			AdbHelper.Instance.KillAdb ( AndroidDebugBridge.SocketAddress );
		}

        [TestMethod]
        [TestCategory("IntegrationTest")]
		public void DeviceGetMountPointsTest ( ) {
			Device device = GetFirstDevice ( );
			foreach ( var item in device.MountPoints.Keys ) {
				Console.WriteLine ( device.MountPoints[item] );
			}

			Assert.IsTrue ( device.MountPoints.ContainsKey ( "/system" ) );
		}

        [TestMethod]
        [TestCategory("IntegrationTest")]
		public void DeviceRemountMountPointTest ( ) {
			Device device = GetFirstDevice ( );

			Assert.IsTrue ( device.MountPoints.ContainsKey ( "/system" ), "Device does not contain mount point /system" );
			bool isReadOnly = device.MountPoints["/system"].IsReadOnly;

			device.RemountMountPoint ( device.MountPoints["/system"], !isReadOnly );

			Assert.AreEqual<bool> ( !isReadOnly, device.MountPoints["/system"].IsReadOnly );
			Console.WriteLine ( "Successfully mounted /system as {0}", !isReadOnly ? "ro" : "rw" );

			// revert it back...
			device.RemountMountPoint ( device.MountPoints["/system"], isReadOnly );
			Assert.AreEqual<bool> ( isReadOnly, device.MountPoints["/system"].IsReadOnly );
			Console.WriteLine ( "Successfully mounted /system as {0}", isReadOnly ? "ro" : "rw" );

		}

        [TestMethod]
        [TestCategory("IntegrationTest")]
		public void ExecuteRemoteCommandTest ( ) {

			Device device = GetFirstDevice ( );
			ConsoleOutputReceiver creciever = new ConsoleOutputReceiver ( );


			device.ExecuteShellCommand("pm list packages -f",creciever);

			Console.WriteLine ( "Executing 'ls':" );
			try {
				device.ExecuteShellCommand ( "ls -lF --color=never", creciever );
			} catch ( UnknownOptionException ) {
				device.ExecuteShellCommand ( "ls -l", creciever );
			}


			Console.WriteLine ( "Executing 'busybox':" );
			bool hasBB = false;
			try {
				device.ExecuteShellCommand ( "busybox", creciever );
				hasBB = true;
			} catch ( FileNotFoundException ) {
				hasBB = false;
			} finally {
				Console.WriteLine ( "Busybox enabled: {0}", hasBB );
			}

			Console.WriteLine ( "Executing 'unknowncommand':" );
            try {
				device.ExecuteShellCommand ( "unknowncommand", creciever );
                Assert.Fail();
			} catch(FileNotFoundException)
            {
                // Expected exception
            }

			Console.WriteLine ( "Executing 'ls /system/foo'" );
            try {
				device.ExecuteShellCommand ("ls /system/foo", creciever );
                Assert.Fail();
			} catch(FileNotFoundException)
            {
                // Expected exception
            }

		}

        [TestMethod]
        [TestCategory("IntegrationTest")]
		public void ExecuteRemoteRootCommandTest( ) {
			Device device = GetFirstDevice ( );
			ConsoleOutputReceiver creciever = new ConsoleOutputReceiver ( );

			Console.WriteLine ( "Executing 'ls':" );
			if ( device.CanSU ( ) ) {
				try {
					device.ExecuteRootShellCommand ( "busybox ls -lFa --color=never", creciever );
				} catch ( UnknownOptionException ) {
					device.ExecuteRootShellCommand ( "ls -lF", creciever );
				}
			} else {
				// if the device doesn't have root, then we check that it is throwing the PermissionDeniedException
				try {
					try {
						device.ExecuteRootShellCommand ( "busybox ls -lFa --color=never", creciever );
					} catch ( UnknownOptionException ) {
						device.ExecuteRootShellCommand ( "ls -lF", creciever );
					}

                    Assert.Fail();
                } catch (PermissionDeniedException) {
                    // Expected exception
                }

			}
		}

        [TestMethod]
        [TestCategory("IntegrationTest")]
		public void GetRawImageTest ( ) {
			Device device = GetFirstDevice ( );

			RawImage rawImage = device.Screenshot;

			Assert.IsNotNull ( rawImage );
			Assert.AreEqual<int> ( 32, rawImage.Bpp );
			Assert.AreEqual<int> ( 480, rawImage.Width );
			Assert.AreEqual<int> ( 800, rawImage.Height );

			rawImage.ToImage ( PixelFormat.Format32bppArgb ).Save ( @"c:\Users\Ryan\Desktop\file.png",ImageFormat.Png );

		}

        [TestMethod]
        [TestCategory("IntegrationTest")]
		public void FileListingServiceTest ( ) {
			Device device = GetFirstDevice ( );
			device.FileListingService.ForceBusyBox = true;
			FileEntry[] entries = device.FileListingService.GetChildren ( device.FileListingService.Root, false, null );
			foreach ( var item in entries ) {
				Console.WriteLine ( item.FullPath );
			}
		}

        [TestMethod]
        [TestCategory("IntegrationTest")]
		public void SyncServicePullFileTest ( ) {
			Device device = GetFirstDevice ( );
			using ( SyncService sync = device.SyncService ) {
				String rfile = "/sdcard/bootanimations/bootanimation-cm.zip";
				FileEntry rentry = device.FileListingService.FindFileEntry ( rfile );

				String lpath = Environment.GetFolderPath ( Environment.SpecialFolder.DesktopDirectory );
				String lfile = Path.Combine ( lpath, LinuxPath.GetFileName ( rfile ) );
				FileInfo lfi = new FileInfo ( lfile );
				SyncResult result = sync.PullFile ( rfile, lfile, new FileSyncProgressMonitor ( ) );

				Assert.IsTrue ( lfi.Exists );
				Assert.IsTrue ( ErrorCodeHelper.RESULT_OK == result.Code, ErrorCodeHelper.ErrorCodeToString ( result.Code ) );
				lfi.Delete ( );

				result = sync.PullFile ( rentry, lfile, new FileSyncProgressMonitor ( ) );
				Assert.IsTrue ( lfi.Exists );
				Assert.IsTrue ( ErrorCodeHelper.RESULT_OK == result.Code, ErrorCodeHelper.ErrorCodeToString ( result.Code ) );
				lfi.Delete ( );

			}
		}

        [TestMethod]
        [TestCategory("IntegrationTest")]
		public void SyncServicePushFileTest ( ) {
			String testFile = CreateTestFile ( );
			FileInfo localFile = new FileInfo ( testFile );
			String remoteFile = String.Format ( "/sdcard/{0}", Path.GetFileName ( testFile ) );
			Device device = GetFirstDevice ( );


			using ( SyncService sync = device.SyncService ) {
				SyncResult result = sync.PushFile ( localFile.FullName, remoteFile, new FileSyncProgressMonitor ( ) );
				Assert.IsTrue ( ErrorCodeHelper.RESULT_OK == result.Code, ErrorCodeHelper.ErrorCodeToString ( result.Code ) );
				FileEntry remoteEntry = null;
				remoteEntry = device.FileListingService.FindFileEntry ( remoteFile );

				// check the size
				Assert.AreEqual<long> ( localFile.Length, remoteEntry.Size );

				// clean up temp file on sdcard
				device.ExecuteShellCommand ( String.Format ( "rm {0}", remoteEntry.FullEscapedPath ), new ConsoleOutputReceiver ( ) );
			}
		}

        [TestMethod]
        [TestCategory("IntegrationTest")]
		public void SyncServicePullFilesTest ( ) {
			Device device = GetFirstDevice ( );
			using ( SyncService sync = device.SyncService ) {
				String lpath = Path.Combine ( Environment.GetFolderPath ( Environment.SpecialFolder.DesktopDirectory ), "apps" );
				String rpath = "/system/app/";
				DirectoryInfo ldir = new DirectoryInfo ( lpath );
				if ( !ldir.Exists ) {
					ldir.Create ( );
				}
				FileEntry fentry = device.FileListingService.FindFileEntry ( rpath );
				Assert.IsTrue ( fentry.IsDirectory );

				FileEntry[] entries = device.FileListingService.GetChildren ( fentry, false, null );
				SyncResult result = sync.Pull ( entries, ldir.FullName, new FileSyncProgressMonitor ( ) );

				Assert.IsTrue ( ErrorCodeHelper.RESULT_OK == result.Code, ErrorCodeHelper.ErrorCodeToString ( result.Code ) );
			}
		}

        [TestMethod]
        [TestCategory("IntegrationTest")]
		public void DeviceInstallPackageTest ( ) {
			Device device = GetFirstDevice ( );
			String package = Path.Combine ( Environment.GetFolderPath ( Environment.SpecialFolder.DesktopDirectory ), "com.camalotdesigns.httpdump.apk" );
			Assert.IsTrue ( File.Exists ( package ) );

			device.InstallPackage ( package, false );
		}

        [TestMethod]
        [TestCategory("IntegrationTest")]
		public void DeviceUninstallPackageTest ( ) {
			Device device = GetFirstDevice ( );
			device.UninstallPackage ( "com.camalotdesigns.httpdump" );
		}

        [TestMethod]
        [TestCategory("IntegrationTest")]
		public void DeviceEnvironmentVariablesTest ( ) {
			Device device = GetFirstDevice ( );
			foreach ( var key in device.EnvironmentVariables.Keys ) {
				Console.WriteLine ( "{0}={1}", key, device.EnvironmentVariables[key] );
			}

			Assert.IsTrue ( device.EnvironmentVariables.Count > 0 );
			Assert.IsTrue ( device.EnvironmentVariables.ContainsKey ( "ANDROID_ROOT" ) );
		}

        [TestMethod]
        [TestCategory("IntegrationTest")]
		public void DevicePropertiesTest ( ) {
			Device device = GetFirstDevice ( );
			foreach ( var key in device.Properties.Keys ) {
				Console.WriteLine ( "[{0}]: {1}", key, device.Properties[key] );
			}

			Assert.IsTrue ( device.Properties.Count > 0 );
			Assert.IsTrue ( device.Properties.ContainsKey ( "ro.product.device" ) );
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

			public void StartSubTask ( String source, String destination ) {
				Console.WriteLine ( "Syncing {0} -> {1}", source, destination );
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
	}
}
