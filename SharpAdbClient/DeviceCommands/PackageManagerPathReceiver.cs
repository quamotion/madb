// <copyright file="PackageManagerPathReceiver.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion. All rights reserved.
// </copyright>

namespace SharpAdbClient.DeviceCommands
{
    using System.Collections.Generic;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Parses the output of the <c>pm path {package}</c> command.
    /// </summary>
    internal class PackageManagerPathReceiver : MultiLineReceiver
    {
        /// <summary>
        /// Pattern to parse the output of the 'pm path &lt;package&gt;' command.
        /// The output format looks like:
        /// /data/app/myapp.apk=com.mypackage.myapp
        /// </summary>
        public const string PathPattern = "^package:(.+?)$";

        /// <summary>
        /// Initializes a new instance of the <see cref="PackageManagerPathReceiver"/> class.
        /// </summary>
        public PackageManagerPathReceiver()
        {
            this.Path = null;
        }

        /// <summary>
        /// Gets or sets the path.
        /// </summary>
        public string Path { get; private set; }

        /// <summary>
        /// Processes the new lines.
        /// </summary>
        /// <param name="lines">The lines.</param>
        protected override void ProcessNewLines(IEnumerable<string> lines)
        {
            foreach (string line in lines)
            {
                if (!string.IsNullOrEmpty(line) && !line.StartsWith("#"))
                {
                    // get the filepath and package from the line
                    Match m = Regex.Match(line, PathPattern, RegexOptions.Compiled);
                    if (m.Success)
                    {
                        this.Path = m.Groups[1].Value;
                    }
                }
            }
        }
    }
}
