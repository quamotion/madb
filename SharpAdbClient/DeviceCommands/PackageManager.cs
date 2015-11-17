// <copyright file="PackageManager.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion. All rights reserved.
// </copyright>

namespace SharpAdbClient.DeviceCommands
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// Allows you to get information about packages that are installed on a device.
    /// </summary>
    public class PackageManager
    {
        /// <summary>
        /// The command that list all packages installed on the device.
        /// </summary>
        private const string ListFull = "pm list packages -f";

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
    }
}
