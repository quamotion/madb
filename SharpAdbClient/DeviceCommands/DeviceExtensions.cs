// <copyright file="DeviceExtensions.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion. All rights reserved.
// </copyright>

namespace SharpAdbClient.DeviceCommands
{
    using System.Collections.Generic;

    /// <summary>
    /// Provides extension methods for the <see cref="DeviceData"/> class, allowing you to run
    /// commands directory against a <see cref="DeviceData"/> object.
    /// </summary>
    public static class DeviceExtensions
    {
        /// <summary>
        /// Executes a shell command on the device.
        /// </summary>
        /// <param name="device">
        /// The device on which to run the command.
        /// </param>
        /// <param name="command">
        /// The command to execute.
        /// </param>
        /// <param name="receiver">
        /// Optionally, a <see cref="IShellOutputReceiver"/> that processes the command output.
        /// </param>
        public static void ExecuteShellCommand(this DeviceData device, string command, IShellOutputReceiver receiver)
        {
            AdbClient.Instance.ExecuteRemoteCommand(command, device, receiver);
        }

        /// <summary>
        /// Gets the file statistics of a given file.
        /// </summary>
        /// <param name="device">
        /// The device on which to look for the file.
        /// </param>
        /// <param name="path">
        /// The path to the file.
        /// </param>
        /// <returns>
        /// A <see cref="FileStatistics"/> object that represents the file.
        /// </returns>
        public static FileStatistics Stat(this DeviceData device, string path)
        {
            using (ISyncService service = Factories.SyncServiceFactory(device))
            {
                return service.Stat(path);
            }
        }

        /// <summary>
        /// Gets the properties of a device.
        /// </summary>
        /// <param name="device">
        /// The device for which to list the properties.
        /// </param>
        /// <returns>
        /// A dictionary containing the properties of the device, and their values.
        /// </returns>
        public static Dictionary<string, string> GetProperties(this DeviceData device)
        {
            var receiver = new GetPropReceiver();
            AdbClient.Instance.ExecuteRemoteCommand(GetPropReceiver.GetpropCommand, device, receiver);
            return receiver.Properties;
        }

        /// <summary>
        /// Gets the environment variables currently defined on a device.
        /// </summary>
        /// <param name="device">
        /// The device for which to list the environment variables.
        /// </param>
        /// <returns>
        /// A dictionary containing the environment variables of the device, and their values.
        /// </returns>
        public static Dictionary<string, string> GetEnvironmentVariables(this DeviceData device)
        {
            var receiver = new EnvironmentVariablesReceiver();
            AdbClient.Instance.ExecuteRemoteCommand(EnvironmentVariablesReceiver.PrintEnvCommand, device, receiver);
            return receiver.EnvironmentVariables;
        }

        /// <summary>
        /// Uninstalls a package from the device.
        /// </summary>
        /// <param name="device">
        /// The device on which to uninstall the package.
        /// </param>
        /// <param name="packageName">
        /// The name of the package to uninstall.
        /// </param>
        public static void UninstallPackage(this DeviceData device, string packageName)
        {
            PackageManager manager = new PackageManager(device);
            manager.UninstallPackage(packageName);
        }

        /// <summary>
        /// Requests the version information from the device.
        /// </summary>
        /// <param name="device">
        /// The device on which to uninstall the package.
        /// </param>
        /// <param name="packageName">
        /// The name of the package from which to get the application version.
        /// </param>
        public static VersionInfo GetPackageVersion(this DeviceData device, string packageName)
        {
            PackageManager manager = new PackageManager(device);
            return manager.GetVersionInfo(packageName);
        }

        /// <summary>
        /// Lists all processes running on the device.
        /// </summary>
        /// <param name="device">
        /// The device on which to list the processes that are running.
        /// </param>
        /// <returns>
        /// An <see cref="IEnumerable{AndroidProcess}"/> that will iterate over all
        /// processes that are currently running on the device.
        /// </returns>
        public static IEnumerable<AndroidProcess> ListProcesses(this DeviceData device)
        {
            var receiver = new ProcessOutputReceiver();
            device.ExecuteShellCommand("ps", receiver);
            return receiver.Processes;
        }
    }
}
