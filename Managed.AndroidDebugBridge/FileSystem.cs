using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Managed.Adb.IO;
using Managed.Adb.MoreLinq;
using System.Text.RegularExpressions;

namespace Managed.Adb {
	public class FileSystem {

		/// <summary>
		/// Initializes a new instance of the <see cref="FileSystem"/> class.
		/// </summary>
		/// <param name="device">The device.</param>
		public FileSystem( Device device ) {
			Device = device;
		}

		/// <summary>
		/// Creates the specified path.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <returns></returns>
		public FileEntry Create( String path ) {
			Device.ThrowIfNull ( "Device" );

			path.ThrowIfNullOrWhiteSpace ( "path" );

			if ( !Device.IsOffline ) {
				if ( Exists ( path ) ) {
					throw new ArgumentException ( "The specified path already exists." );
				} else {
					var cer = new CommandErrorReceiver ( );
					var escaped = LinuxPath.Escape ( path );
					// use native touch command if its available.
					var cmd = Device.BusyBox.Available ? "touch" : ">";
					var command = String.Format ( "{0} {1}", cmd, escaped );
					if ( Device.CanSU ( ) ) {
						Device.ExecuteRootShellCommand ( command, cer );
					} else {
						Device.ExecuteShellCommand ( command, cer );
					}
					if ( !String.IsNullOrEmpty ( cer.ErrorMessage ) ) {
						throw new IOException ( String.Format ( "Error creating file: {0}", cer.ErrorMessage ) );
					} else {
						// at this point, the newly created file should exist.
						return Device.FileListingService.FindFileEntry ( path );
					}
				}
			} else {
				throw new IOException ( "Device is not online" );
			}
		}

		/// <summary>
		/// Creates the specified file entry.
		/// </summary>
		/// <param name="fileEntry">The file entry.</param>
		/// <returns></returns>
		public FileEntry Create( FileEntry fileEntry ) {
			if ( fileEntry.IsDirectory ) {
				MakeDirectory ( fileEntry.FullPath );
				return Device.FileListingService.FindFileEntry ( fileEntry.FullPath );
			} else {
				return Create ( fileEntry.FullPath );
			}
		}

		/// <summary>
		/// Gets if the specified path exists on the device.
		/// </summary>
		/// <param name="path">the path to check</param>
		/// <returns><c>true</c>, if the path exists; otherwise, <c>false</c></returns>
		/// <exception cref="IOException">If the device is not connected.</exception>
		/// <exception cref="ArgumentNullException">If the device or path is null.</exception>
		public bool Exists( String path ) {
			Device.ThrowIfNull ( "Device" );
			path.ThrowIfNullOrWhiteSpace ( "path" );

			if ( !Device.IsOffline ) {
				try {
					FileEntry fe = Device.FileListingService.FindFileEntry ( path );
					return fe != null;
				} catch ( FileNotFoundException e ) {
					return false;
				}
			} else {
				throw new IOException ( "Device is not online" );
			}
		}

		/// <summary>
		/// Gets or sets the device.
		/// </summary>
		/// <value>
		/// The device.
		/// </value>
		private Device Device { get; set; }

		/// <summary>
		/// Makes the directory from the specified path.
		/// </summary>
		/// <param name="path">The path.</param>
		public void MakeDirectory( String path ) {
			MakeDirectory ( path, false );
		}

		/// <summary>
		/// Makes the directory.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <param name="forceDeviceMethod">if set to <c>true</c> forces the use of the "non-busybox" method.</param>
		public void MakeDirectory( String path, bool forceDeviceMethod ) {
			Device.ThrowIfNull ( "Device" );
			path.ThrowIfNullOrWhiteSpace ( "path" );

			CommandErrorReceiver cer = new CommandErrorReceiver ( );
			try {
				//var fileEntry = FileEntry.FindOrCreate ( Device, path );
				// if we have busybox we can use the mkdir in there as it supports --parents
				if ( Device.BusyBox.Available && !forceDeviceMethod ) {
					try {
						Device.BusyBox.ExecuteShellCommand ( "mkdir -p {0}", cer, path );
					} catch {
						try {
							// if there was an error, then fallback too.
							MakeDirectoryFallbackInternal (  path, cer );
						} catch { }
					}
				} else {
					// if busybox is not available then we have to fallback
					MakeDirectoryFallbackInternal ( path, cer );
				}
			} catch {

			}
			if ( !String.IsNullOrEmpty ( cer.ErrorMessage ) ) {
				throw new IOException ( cer.ErrorMessage );
			}
		}

		/// <summary>
		/// this is a fallback if the mkdir -p fails for somereason
		/// </summary>
		/// <param name="path"></param>
		/// <param name="cer"></param>
		internal void MakeDirectoryFallbackInternal( String path, CommandErrorReceiver cer ) {
			Device.ExecuteShellCommand ( "mkdir {0}", cer, path );
		}

		/// <summary>
		/// Copies the specified source to the specified destination.
		/// </summary>
		/// <param name="source">The source.</param>
		/// <param name="destination">The destination.</param>
		public void Copy( String source, String destination ) {
			Device.ThrowIfNull ( "Device" );
			source.ThrowIfNullOrWhiteSpace ( "source" );
			destination.ThrowIfNullOrWhiteSpace ( "destination" );

			CommandErrorReceiver cer = new CommandErrorReceiver ( );
			FileEntry sfe = Device.FileListingService.FindFileEntry ( source );

			Device.ExecuteShellCommand ( "cat {0} > {1}", cer, sfe.FullEscapedPath, destination );
			if ( !String.IsNullOrEmpty ( cer.ErrorMessage ) ) {
				throw new IOException ( cer.ErrorMessage );
			}
		}

		/// <summary>
		/// Moves the specified source to the specified destination.
		/// </summary>
		/// <param name="source">The source.</param>
		/// <param name="destination">The destination.</param>
		public void Move( String source, String destination ) {
			Device.ThrowIfNull ( "Device" );
			source.ThrowIfNullOrWhiteSpace ( "source" );
			destination.ThrowIfNullOrWhiteSpace ( "destination" );
			
			CommandErrorReceiver cer = new CommandErrorReceiver ( );
			FileEntry sfe = Device.FileListingService.FindFileEntry ( source );

			Device.ExecuteShellCommand ( "mv {0} {1}", cer, sfe.FullEscapedPath, destination );
			if ( !String.IsNullOrEmpty ( cer.ErrorMessage ) ) {
				throw new IOException ( cer.ErrorMessage );
			}
		}

		/// <summary>
		/// Chmods the specified path.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <param name="permissions">The permissions.</param>
		public void Chmod( String path, String permissions ) {
			Device.ThrowIfNull ( "Device" );
			path.ThrowIfNullOrWhiteSpace ( "path" );
			permissions.ThrowIfNullOrWhiteSpace ( "permissions" );

			FileEntry entry = Device.FileListingService.FindFileEntry ( path );
			CommandErrorReceiver cer = new CommandErrorReceiver ( );
			Device.ExecuteShellCommand ( "chmod {0} {1}", cer, permissions, entry.FullEscapedPath );
		}

		/// <summary>
		/// Chmods the specified path.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <param name="permissions">The permissions.</param>
		public void Chmod( String path, FilePermissions permissions ) {
			Device.ThrowIfNull ( "Device" );
			path.ThrowIfNullOrWhiteSpace ( "path" );
			permissions.ThrowIfNull ( "permissions" );

			FileEntry entry = Device.FileListingService.FindFileEntry ( path );
			CommandErrorReceiver cer = new CommandErrorReceiver ( );
			Device.ExecuteShellCommand ( "chmod {0} {1}", cer, permissions.ToChmod ( ), entry.FullEscapedPath );
		}

		/// <summary>
		/// Gets if the specified mount point is read-only
		/// </summary>
		/// <param name="mount"></param>
		/// <returns><code>true</code>, if read-only; otherwise, <code>false</code></returns>
		/// <exception cref="IOException">If mount point doesnt exist</exception>
		public bool IsMountPointReadOnly( String mount ) {
			Device.ThrowIfNull ( "Device" );
			mount.ThrowIfNullOrWhiteSpace ( mount );

			if ( !Device.MountPoints.ContainsKey ( mount ) ) {
				throw new IOException ( "Invalid mount point" );
			}

			return Device.MountPoints[mount].IsReadOnly;
		}

		/// <summary>
		/// Deletes the specified path.
		/// </summary>
		/// <param name="path">The path.</param>
		public void Delete( String path ) {
			Device.ThrowIfNull ( "Device" );
			path.ThrowIfNullOrWhiteSpace ( "path" );

			CommandErrorReceiver cer = new CommandErrorReceiver ( );
			FileEntry entry = Device.FileListingService.FindFileEntry ( path );

			Delete ( entry );
		}

		/// <summary>
		/// Deletes the specified path.
		/// </summary>
		/// <param name="path">The path.</param>
		public void Delete ( FileEntry fileEntry ) {
			Device.ThrowIfNull ( "Device" );
			fileEntry.ThrowIfNull ( "fileEntry" );
			if ( fileEntry.Exists ) {
				CommandErrorReceiver cer = new CommandErrorReceiver ( );
				Device.ExecuteShellCommand ( "rm -f {0} {1}", cer, fileEntry.IsDirectory ? "-r" : String.Empty, fileEntry.FullResolvedPath );

				if ( !String.IsNullOrEmpty ( cer.ErrorMessage ) ) {
					throw new IOException ( cer.ErrorMessage );
				}
			}
		}

		/// <summary>
		/// Gets the dev blocks for the device.
		/// </summary>
		/// <exception cref="System.IO.FileNotFoundException">Throws if unable to locate /dev/block </exception>
		public IEnumerable<FileEntry> DeviceBlocks {
			get {
				Device.ThrowIfNull ( "Device" );

				var blocks = FileEntry.Find ( Device, "/dev/block/" );
				blocks.Children = Device.FileListingService.GetChildren ( blocks, true, null ).ToList();
				return blocks.Children.Where ( b => b.Type == FileListingService.FileTypes.Block );
			}
		}

		/// <summary>
		/// Mounts the specified device.
		/// </summary>
		/// <param name="mountPoint">The mp.</param>
		/// <param name="options">The options.</param>
		public void Mount ( MountPoint mountPoint, String options ) {
			mountPoint.ThrowIfNull ( "mountPoint" );
			Device.ThrowIfNull ( "Device" );

			CommandErrorReceiver cer = new CommandErrorReceiver ( );
			if ( Device.BusyBox.Available ) {
				Device.ExecuteShellCommand ( "busybox mount {0} {4} -t {1} {2} {3}", cer, mountPoint.IsReadOnly ? "-r" : "-w",
					mountPoint.FileSystem, mountPoint.Block, mountPoint.Name, 
					!String.IsNullOrEmpty ( options ) ? String.Format ( "-o {0}", options ) : String.Empty );
			} else {
				Device.ExecuteShellCommand ( "mount {0} {4} -t {1} {2} {3}", cer, mountPoint.IsReadOnly ? "-r" : "-w",
					mountPoint.FileSystem, mountPoint.Block, mountPoint.Name, 
					!String.IsNullOrEmpty ( options ) ? String.Format ( "-o {0}", options ) : String.Empty );
			}
		}

		/// <summary>
		/// Attempts to mount the mount point to the associated device without knowing the device or the type.
		/// Some devices may not support this method.
		/// </summary>
		/// <param name="mountPoint"></param>
		public void Mount( String mountPoint ) {
			mountPoint.ThrowIfNull ( "mountPoint" );
			Device.ThrowIfNull ( "Device" );

			CommandErrorReceiver cer = new CommandErrorReceiver ( );
			if ( Device.BusyBox.Available ) {
				Device.ExecuteShellCommand ( "busybox mount {0}", cer, mountPoint );
			} else {
				Device.ExecuteShellCommand ( "mount {0}", cer, mountPoint );
			}
		}

		/// <summary>
		/// Mounts the specified mount point.
		/// </summary>
		/// <param name="mountPoint">The mountPoint.</param>
		public void Mount( MountPoint mountPoint ) {
			Mount ( mountPoint, String.Empty );
		}

		/// <summary>
		/// Mounts the specified devices to the specified directory.
		/// </summary>
		/// <param name="directory">The directory.</param>
		/// <param name="device">The device.</param>
		/// <param name="fileSytemType">Type of the file sytem.</param>
		/// <param name="isReadOnly">if set to <c>true</c> is read only.</param>
		/// <param name="options">The options.</param>
		public void Mount( String directory, String device, String fileSytemType, bool isReadOnly, String options ) {
			Mount ( new MountPoint ( device, directory, fileSytemType, isReadOnly ), options );
		}

		/// <summary>
		/// Mounts the specified devices to the specified directory.
		/// </summary>
		/// <param name="directory">The directory.</param>
		/// <param name="device">The device.</param>
		/// <param name="fileSytemType">Type of the file sytem.</param>
		/// <param name="isReadOnly">if set to <c>true</c> is read only.</param>
		public void Mount( String directory, String device, String fileSytemType, bool isReadOnly ) {
			Mount ( new MountPoint ( device, directory, fileSytemType, isReadOnly ), String.Empty );
		}

		/// <summary>
		/// Unmounts the specified mount point.
		/// </summary>
		/// <param name="mountPoint">The mountPoint.</param>
		public void Unmount( MountPoint mountPoint ) {
			Unmount ( mountPoint, String.Empty );
		}


		/// <summary>
		/// Unmounts the specified mount point.
		/// </summary>
		/// <param name="mountPoint">The mountPoint.</param>
		/// <param name="options">The options.</param>
		public void Unmount( MountPoint mountPoint, String options ) {
			mountPoint.ThrowIfNull ( "mountPoint" );

			Unmount ( mountPoint.Name, options );
		}

		/// <summary>
		/// Unmounts the specified mount point.
		/// </summary>
		/// <param name="mountPoint">The mount point.</param>
		public void Unmount( String mountPoint ) {
			Unmount ( mountPoint, String.Empty );
		}

		/// <summary>
		/// Unmounts the specified mount point.
		/// </summary>
		/// <param name="mountPoint">The mount point.</param>
		/// <param name="options">The options.</param>
		public void Unmount( String mountPoint, String options ) {
			mountPoint.ThrowIfNull ( "mountPoint" );
			Device.ThrowIfNull ( "Device" );

			CommandErrorReceiver cer = new CommandErrorReceiver ( );
			if ( Device.BusyBox.Available ) {
				Device.ExecuteShellCommand ( "busybox umount {1} {0}", cer, !String.IsNullOrEmpty ( options ) ? String.Format ( "-o {0}", options ) : String.Empty, mountPoint );
			} else {
				Device.ExecuteShellCommand ( "umount {1} {0}", cer, !String.IsNullOrEmpty ( options ) ? String.Format ( "-o {0}", options ) : String.Empty, mountPoint );
			}
		}

		/// <summary>
		/// Resolves the link to the true path.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <returns></returns>
		public String ResolveLink ( String path ) {
			if ( this.Device.BusyBox.Available ) {
				var cresult = new CommandResultReceiver ( );
				this.Device.BusyBox.ExecuteShellCommand ( "readlink -f {0}", cresult, path );
				// if cresult is empty, return the path
				return (cresult == null || String.IsNullOrEmpty ( cresult.Result )) ? path : cresult.Result;
			} else {
				// what do we do here? readlink is not available on devices without busybox...
				return path;
			}
		}

	}
}
