using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Managed.Adb {
	public interface IDevice {
		/**
     * Returns the serial number of the device.
     */
		String SerialNumber { get; }

    /**
     * Returns the name of the AVD the emulator is running.
     * <p/>This is only valid if {@link #isEmulator()} returns true.
     * <p/>If the emulator is not running any AVD (for instance it's running from an Android source
     * tree build), this method will return "<code>&lt;build&gt;</code>".
     * @return the name of the AVD or <code>null</code> if there isn't any.
     */
		String AvdName { get; }

    /**
     * Returns the state of the device.
     */
		DeviceState State { get; }

    /**
     * Returns the device properties. It contains the whole output of 'getprop'
     */
    Dictionary<String, String> getProperties();

    /**
     * Returns the number of property for this device.
     */
		int PropertyCount { get; }

    /**
     * Returns a property value.
     * @param name the name of the value to return.
     * @return the value or <code>null</code> if the property does not exist.
     */
    String GetProperty(String name);

    /**
     * Returns if the device is ready.
     * @return <code>true</code> if {@link #getState()} returns {@link DeviceState#ONLINE}.
     */
		bool IsOnline { get; }

    /**
     * Returns <code>true</code> if the device is an emulator.
     */
		bool IsEmulator { get; }

		/**
     * Returns if the device is offline.
     * @return <code>true</code> if {@link #getState()} returns {@link DeviceState#OFFLINE}.
     */
		bool IsOffline { get; }

    /**
     * Returns if the device is in bootloader mode.
     * @return <code>true</code> if {@link #getState()} returns {@link DeviceState#BOOTLOADER}.
     */
		bool IsBootLoader { get; }

    /**
     * Returns whether the {@link Device} has {@link Client}s.
     */
		bool HasClients { get; }

    /**
     * Returns the array of clients.
     */
		Client[] Clients { get; }

    /**
     * Returns a {@link Client} by its application name.
     * @param applicationName the name of the application
     * @return the <code>Client</code> object or <code>null</code> if no match was found.
     */
    Client GetClient(String applicationName);

    /**
     * Returns a {@link SyncService} object to push / pull files to and from the device.
     * @return <code>null</code> if the SyncService couldn't be created. This can happen if adb
     * refuse to open the connection because the {@link IDevice} is invalid (or got disconnected).
     * @throws IOException if the connection with adb failed.
     */
		SyncService SyncService { get; }

    /**
     * Returns a {@link FileListingService} for this device.
     */
		FileListingService FileListingService { get; }

    /**
     * Takes a screen shot of the device and returns it as a {@link RawImage}.
     * @return the screenshot as a <code>RawImage</code> or <code>null</code> if
     * something went wrong.
     * @throws IOException
     */
		RawImage Screenshot { get; }

    /**
     * Executes a shell command on the device, and sends the result to a receiver.
     * @param command The command to execute
     * @param receiver The receiver object getting the result from the command.
     * @throws IOException
     */
    void ExecuteShellCommand(String command, IShellOutputReceiver receiver);

    /**
     * Runs the event log service and outputs the event log to the {@link LogReceiver}.
     * @param receiver the receiver to receive the event log entries.
     * @throws IOException
     */
    void RunEventLogService(LogReceiver receiver);

    /**
     * Runs the log service for the given log and outputs the log to the {@link LogReceiver}.
     * @param logname the logname of the log to read from.
     * @param receiver the receiver to receive the event log entries.
     * @throws IOException
     */
    void RunLogService(String logname, LogReceiver receiver);

    /**
     * Creates a port forwarding between a local and a remote port.
     * @param localPort the local port to forward
     * @param remotePort the remote port.
     * @return <code>true</code> if success.
     */
    bool CreateForward(int localPort, int remotePort);

    /**
     * Removes a port forwarding between a local and a remote port.
     * @param localPort the local port to forward
     * @param remotePort the remote port.
     * @return <code>true</code> if success.
     */
    bool RemoveForward(int localPort, int remotePort);

    /**
     * Returns the name of the client by pid or <code>null</code> if pid is unknown
     * @param pid the pid of the client.
     */
    String GetClientName(int pid);

    /**
     * Installs an Android application on device.
     * This is a helper method that combines the syncPackageToDevice, installRemotePackage,
     * and removePackage steps
     * @param packageFilePath the absolute file system path to file on local host to install
     * @param reinstall set to <code>true</code> if re-install of app should be performed
     * @return a {@link String} with an error code, or <code>null</code> if success.
     * @throws IOException
     */
    String InstallPackage(String packageFilePath, bool reinstall) ;

    /**
     * Pushes a file to device
     * @param localFilePath the absolute path to file on local host
     * @return {@link String} destination path on device for file
     * @throws IOException if fatal error occurred when pushing file
     */
		String SyncPackageToDevice ( String localFilePath );

    /**
     * Installs the application package that was pushed to a temporary location on the device.
     * @param remoteFilePath absolute file path to package file on device
     * @param reinstall set to <code>true</code> if re-install of app should be performed
     * @throws InstallException if installation failed
     */
    String InstallRemotePackage(String remoteFilePath, bool reinstall);

    /**
     * Remove a file from device
     * @param remoteFilePath path on device of file to remove
     * @throws IOException if file removal failed
     */
    void RemoveRemotePackage(String remoteFilePath);

    /**
     * Uninstall an package from the device.
     * @param packageName the Android application package name to uninstall
     * @return a {@link String} with an error code, or <code>null</code> if success.
     * @throws IOException
     */
    String UninstallPackage(String packageName) ;
	}
}
