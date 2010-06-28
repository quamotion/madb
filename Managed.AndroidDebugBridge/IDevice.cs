using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Managed.Adb {

	/// <summary>
	/// 
	/// </summary>
	public interface IDevice {

		event EventHandler<EventArgs> StateChanged;
		event EventHandler<EventArgs> BuildInfoChanged;
		event EventHandler<EventArgs> ClientListChanged;

		FileSystem FileSystem { get; }

		BusyBox BusyBox { get; }

		/// <summary>
		/// Gets the serial number of the device.
		/// </summary>
		/// <value>The serial number.</value>
		String SerialNumber { get; }

		/// <summary>
		/// Returns the name of the AVD the emulator is running.
		/// <p/>
		/// This is only valid if {@link #isEmulator()} returns true.
		/// <p/>
		/// If the emulator is not running any AVD (for instance it's running from an Android source
		/// tree build), this method will return "<code>&lt;build&gt;</code>"
		/// @return the name of the AVD or  <code>null</code>
		///  if there isn't any.
		/// </summary>
		/// <value>The name of the avd.</value>
		String AvdName { get; set; }

		/// <summary>
		/// Gets the environment variables.
		/// </summary>
		/// <value>The environment variables.</value>
		Dictionary<String, String> EnvironmentVariables { get; }

		/// <summary>
		/// Gets the mount points.
		/// </summary>
		/// <value>The mount points.</value>
		Dictionary<String, MountPoint> MountPoints { get; }

		/// <summary>
		/// Gets the state.
		/// </summary>
		/// <value>The state.</value>
		/// Returns the state of the device.
		DeviceState State { get; }

		/// <summary>
		/// Returns the device properties. It contains the whole output of 'getprop'
		/// </summary>
		/// <value>The properties.</value>
		Dictionary<String, String> Properties { get; }

		/// <summary>
		/// Gets a value indicating whether the device is online.
		/// </summary>
		/// <value><c>true</c> if the device is online; otherwise, <c>false</c>.</value>
		bool IsOnline { get; }


		/// <summary>
		/// Gets a value indicating whether this device is emulator.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this device is emulator; otherwise, <c>false</c>.
		/// </value>
		bool IsEmulator { get; }

		/// <summary>
		/// Gets a value indicating whether this device is offline.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this device is offline; otherwise, <c>false</c>.
		/// </value>
		bool IsOffline { get; }


		/// <summary>
		/// Gets a value indicating whether this device is in boot loader mode.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this device is in boot loader mode; otherwise, <c>false</c>.
		/// </value>
		bool IsBootLoader { get; }

		/**
		 * Returns whether the {@link Device} has {@link Client}s.
		 */
		//bool HasClients { get; }

		/// <summary>
		/// Gets the list of clients
		/// </summary>
		List<IClient> Clients { get; }

		/**
		 * Returns a {@link Client} by its application name.
		 * @param applicationName the name of the application
		 * @return the <code>Client</code> object or <code>null</code> if no match was found.
		 */
		//Client GetClient(String applicationName);

		/// <summary>
		/// Returns a <see cref="SyncService"/> object to push / pull files to and from the device.
		/// </summary>
		/// <remarks>
		/// <code>null</code> if the SyncService couldn't be created. This can happen if adb
		/// refuse to open the connection because the {@link IDevice} is invalid (or got disconnected).
		/// </remarks>
		/// <exception cref="IOException">Throws IOException if the connection with adb failed.</exception>
		SyncService SyncService { get; }

		/// <summary>
		/// Returns a <see cref="FileListingService"/> for this device.
		/// </summary>
		FileListingService FileListingService { get; }


		/// <summary>
		/// Takes a screen shot of the device and returns it as a <see cref="RawImage"/>
		/// </summary>
		/// <value>The screenshot.</value>
		RawImage Screenshot { get; }

		/// <summary>
		/// Executes a shell command on the device, and sends the result to a receiver.
		/// </summary>
		/// <param name="command">The command to execute</param>
		/// <param name="receiver">The receiver object getting the result from the command.</param>
		void ExecuteShellCommand(String command, IShellOutputReceiver receiver);

		/**
		 * Runs the event log service and outputs the event log to the {@link LogReceiver}.
		 * @param receiver the receiver to receive the event log entries.
		 * @throws IOException
		 */
		//void RunEventLogService(LogReceiver receiver);

		/**
		 * Runs the log service for the given log and outputs the log to the {@link LogReceiver}.
		 * @param logname the logname of the log to read from.
		 * @param receiver the receiver to receive the event log entries.
		 * @throws IOException
		 */
		//void RunLogService(String logname, LogReceiver receiver);
		
		/// <summary>
		/// Creates a port forwarding between a local and a remote port.
		/// </summary>
		/// <param name="localPort">the local port to forward</param>
		/// <param name="remotePort">the remote port.</param>
		/// <returns><code>true</code> if success.</returns>
		bool CreateForward(int localPort, int remotePort);

		/// <summary>
		/// Removes a port forwarding between a local and a remote port.
		/// </summary>
		/// <param name="localPort"> the local port to forward</param>
		/// <param name="remotePort">the remote port.</param>
		/// <returns><code>true</code> if success.</returns>
		bool RemoveForward ( int localPort, int remotePort );

		/**
		 * Returns the name of the client by pid or <code>null</code> if pid is unknown
		 * @param pid the pid of the client.
		 */
		//String GetClientName(int pid);

		/// <summary>
		/// Installs an Android application on device.
		/// This is a helper method that combines the syncPackageToDevice, installRemotePackage,
		/// and removePackage steps
		/// </summary>
		/// <param name="packageFilePath">the absolute file system path to file on local host to install</param>
		/// <param name="reinstall">set to <code>true</code>if re-install of app should be performed</param>
		void InstallPackage ( String packageFilePath, bool reinstall );

		/// <summary>
		/// Pushes a file to device
		/// </summary>
		/// <param name="localFilePath">the absolute path to file on local host</param>
		/// <returns>destination path on device for file</returns>
		/// <exception cref="IOException">if fatal error occurred when pushing file</exception>
		String SyncPackageToDevice ( String localFilePath );
 
		/// <summary>
		/// Installs the application package that was pushed to a temporary location on the device.
		/// </summary>
		/// <param name="remoteFilePath">absolute file path to package file on device</param>
		/// <param name="reinstall">set to <code>true</code> if re-install of app should be performed</param>
		void InstallRemotePackage(String remoteFilePath, bool reinstall);

		/*
		 * Remove a file from device
		 * @param remoteFilePath path on device of file to remove
		 * @throws IOException if file removal failed
		 */


		/// <summary>
		/// Remove a file from device
		/// </summary>
		/// <param name="remoteFilePath">path on device of file to remove</param>
		/// <exception cref="IOException">if file removal failed</exception>
		void RemoveRemotePackage(String remoteFilePath);

		/// <summary>
		/// Uninstall an package from the device.
		/// </summary>
		/// <param name="packageName">Name of the package.</param>
		/// <exception cref="IOException"></exception>
		/// <exception cref="PackageInstallException"></exception>
		void UninstallPackage(String packageName) ;

		void RefreshEnvironmentVariables ( );

		void RefreshMountPoints ( );

		void RefreshProperties ( );
	}
}
