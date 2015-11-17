// <copyright file="DeviceExtensions.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion. All rights reserved.
// </copyright>

namespace SharpAdbClient.DeviceCommands
{
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
            using (SyncService service = new SyncService(device))
            {
                return service.Stat(path);
            }
        }
    }
}
