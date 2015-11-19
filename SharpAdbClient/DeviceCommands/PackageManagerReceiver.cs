// <copyright file="PackageManagerReceiver.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion. All rights reserved.
// </copyright>

namespace SharpAdbClient.DeviceCommands
{
    using System.Collections.Generic;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Parses the output of the various <c>pm</c> commands.
    /// </summary>
    public class PackageManagerReceiver : MultiLineReceiver
    {
        /// <summary>
        /// Pattern to parse the output of the 'pm -lf' command.
        /// The output format looks like:
        /// <c>/data/app/myapp.apk=com.mypackage.myapp</c>
        /// </summary>
        public const string PackagePattern = "^package:(.+?)=(.+)$";

        /// <summary>
        /// Initializes a new instance of the <see cref="PackageManagerReceiver"/> class.
        /// </summary>
        /// <param name="device">
        /// The device for which the package information is being received.
        /// </param>
        /// <param name="packageManager">
        /// The parent package manager.
        /// </param>
        public PackageManagerReceiver(DeviceData device, PackageManager packageManager)
        {
            this.Device = device;
            this.PackageManager = packageManager;
        }

        /// <summary>
        /// Gets the device.
        /// </summary>
        public DeviceData Device { get; private set; }

        /// <summary>
        /// Gets the package manager.
        /// </summary>
        public PackageManager PackageManager { get; private set; }

        /// <summary>
        /// Processes the new lines.
        /// </summary>
        /// <param name="lines">The lines.</param>
        protected override void ProcessNewLines(IEnumerable<string> lines)
        {
            this.PackageManager.Packages.Clear();

            foreach (var line in lines)
            {
                if (line.Trim().Length > 0)
                {
                    var m = line.Match(PackageManagerReceiver.PackagePattern, RegexOptions.Compiled);
                    if (m.Success)
                    {
                        // get the children with that path
                        string name = m.Groups[2].Value;
                        string path = m.Groups[1].Value;

                        if (!this.PackageManager.Packages.ContainsKey(name))
                        {
                            this.PackageManager.Packages.Add(name, path);
                        }
                    }
                }
            }
        }
    }
}
