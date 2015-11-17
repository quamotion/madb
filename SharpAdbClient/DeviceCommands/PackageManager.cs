// <copyright file="PackageManager.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion. All rights reserved.
// </copyright>

namespace SharpAdbClient.DeviceCommands
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;

    /// <summary>
    /// Allows you to get information about packages that are installed on a device.
    /// </summary>
    public class PackageManager
    {
        /// <summary>
        ///
        /// </summary>
        public const string TEMP_DIRECTORY_FOR_INSTALL = "/storage/sdcard0/tmp/";

        /// <summary>
        /// The command that list all packages installed on the device.
        /// </summary>
        private const string ListFull = "pm list packages -f";

        private const string Tag = nameof(PackageManager);

        /// <summary>
        /// Initializes a new instance of the <see cref="PackageManager"/> class.
        /// </summary>
        /// <param name="device">
        /// The device on which to look for packages.
        /// </param>
        public PackageManager(DeviceData device)
        {
            if (device == null)
            {
                throw new ArgumentNullException(nameof(device));
            }

            this.Device = device;
            this.Packages = new Dictionary<string, string>();
        }

        /// <summary>
        /// Gets the list of packages currently installed on the device.
        /// </summary>
        public Dictionary<string, string> Packages { get; private set; }

        /// <summary>
        /// Gets or sets the device.
        /// </summary>
        public DeviceData Device { get; private set; }

        /// <summary>
        /// Refreshes the packages.
        /// </summary>
        public void RefreshPackages()
        {
            if (this.Device.State == DeviceState.Offline)
            {
                throw new IOException("Device is offline");
            }

            PackageManagerReceiver pmr = new PackageManagerReceiver(this.Device, this);
            this.Device.ExecuteShellCommand(ListFull, pmr);
        }

        /// <summary>
        /// Gets a value indicating whether a specific package exists.
        /// </summary>
        /// <param name="package">
        /// The package for which to determine whether it exists on the device.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the package exists on the device; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        public bool Exists(string package)
        {
            try
            {
                return this.GetApkFileEntry(package) != null;
            }
            catch (FileNotFoundException)
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the full path to the APK file which backs up an application package.
        /// </summary>
        /// <param name="package">
        /// The package for which to get the path to the APK file entry.
        /// </param>
        /// <returns>
        /// A <see cref="FileStatistics"/> object that represents the APK file for the package.
        /// </returns>
        public FileStatistics GetApkFileEntry(string package)
        {
            return this.Device.Stat(this.GetApkPath(package));
        }

        /// <summary>
        /// Gets the apk path.
        /// </summary>
        /// <param name="package">
        /// The package.
        /// </param>
        /// <returns>
        /// The full path to the APK file that backs the package.
        /// </returns>
        public string GetApkPath(string package)
        {
            if (this.Device.State == DeviceState.Offline)
            {
                throw new IOException("Device is offline");
            }

            PackageManagerPathReceiver receiver = new PackageManagerPathReceiver();
            this.Device.ExecuteShellCommand($"pm path {package}", receiver);
            if (!string.IsNullOrEmpty(receiver.Path))
            {
                return receiver.Path;
            }
            else
            {
                throw new FileNotFoundException($"The package '{package}' is not installed on the device: {this.Device.Serial}");
            }
        }

        /// <summary>
        /// Installs an Android application on device.
        /// This is a helper method that combines the syncPackageToDevice, installRemotePackage,
        /// and removePackage steps
        /// </summary>
        /// <param name="packageFilePath">the absolute file system path to file on local host to install</param>
        /// <param name="reinstall">set to <see langword="true"/>if re-install of app should be performed</param>
        public void InstallPackage(string packageFilePath, bool reinstall)
        {
            string remoteFilePath = this.SyncPackageToDevice(packageFilePath);
            this.InstallRemotePackage(remoteFilePath, reinstall);
            this.RemoveRemotePackage(remoteFilePath);
        }

        /// <summary>
        /// Pushes a file to device
        /// </summary>
        /// <param name="localFilePath">the absolute path to file on local host</param>
        /// <returns>destination path on device for file</returns>
        /// <exception cref="IOException">if fatal error occurred when pushing file</exception>
        public string SyncPackageToDevice(string localFilePath)
        {
            try
            {
                string packageFileName = Path.GetFileName(localFilePath);

                // only root has access to /data/local/tmp/... not sure how adb does it then...
                // workitem: 16823
                // workitem: 19711
                string remoteFilePath = LinuxPath.Combine(TEMP_DIRECTORY_FOR_INSTALL, packageFileName);

                Log.d(packageFileName, string.Format("Uploading {0} onto device '{1}'", packageFileName, this.Device.Serial));

                using (SyncService sync = new SyncService(this.Device))
                using (Stream stream = File.OpenRead(localFilePath))
                {
                    string message = string.Format("Uploading file onto device '{0}'", this.Device.Serial);
                    Log.d(Tag, message);

                    sync.Push(stream, remoteFilePath, 644, File.GetLastWriteTime(localFilePath), null, CancellationToken.None);
                }

                return remoteFilePath;
            }
            catch (IOException e)
            {
                Log.e(Tag, string.Format("Unable to open sync connection! reason: {0}", e.Message));
                throw;
            }
        }

        /// <summary>
        /// Installs the application package that was pushed to a temporary location on the device.
        /// </summary>
        /// <param name="remoteFilePath">absolute file path to package file on device</param>
        /// <param name="reinstall">set to <see langword="true"/> if re-install of app should be performed</param>
        public void InstallRemotePackage(string remoteFilePath, bool reinstall)
        {
            InstallReceiver receiver = new InstallReceiver();
            string cmd = string.Format("pm install {1}{0}", remoteFilePath, reinstall ? "-r " : string.Empty);
            this.Device.ExecuteShellCommand(cmd, receiver);

            if (!string.IsNullOrEmpty(receiver.ErrorMessage))
            {
                throw new PackageInstallationException(receiver.ErrorMessage);
            }
        }

        /// <summary>
        /// Remove a file from device
        /// </summary>
        /// <param name="remoteFilePath">path on device of file to remove</param>
        /// <exception cref="IOException">if file removal failed</exception>
        public void RemoveRemotePackage(string remoteFilePath)
        {
            // now we delete the app we sync'ed
            try
            {
                this.Device.ExecuteShellCommand("rm " + remoteFilePath, null);
            }
            catch (IOException e)
            {
                Log.e(Tag, string.Format("Failed to delete temporary package: {0}", e.Message));
                throw e;
            }
        }

        /// <summary>
        /// Uninstall an package from the device.
        /// </summary>
        /// <param name="packageName">Name of the package.</param>
        /// <exception cref="IOException"></exception>
        ///
        /// <exception cref="PackageInstallationException"></exception>
        public void UninstallPackage(string packageName)
        {
            InstallReceiver receiver = new InstallReceiver();
            this.Device.ExecuteShellCommand(string.Format("pm uninstall {0}", packageName), receiver);
            if (!string.IsNullOrEmpty(receiver.ErrorMessage))
            {
                throw new PackageInstallationException(receiver.ErrorMessage);
            }
        }
    }
}
