//-----------------------------------------------------------------------
// <copyright file="VersionInfoReceiver.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace SharpAdbClient.DeviceCommands
{
    using SharpAdbClient;
    using System;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Processes command line output of the <c>dumpsys package</c> command.
    /// </summary>
    internal class VersionInfoReceiver : InfoReceiver
    {
        /// <summary>
        /// The name of the version code property.
        /// </summary>
        private static string versionCode = "VersionCode";

        /// <summary>
        /// The name of the version name property.
        /// </summary>
        private static string versionName = "VersionName";

        /// <summary>
        /// Tracks whether we're currently in the packages section or not.
        /// </summary>
        private bool inPackagesSection = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="VersionInfoReceiver"/> class.
        /// </summary>
        public VersionInfoReceiver()
        {
            this.AddPropertyParser(versionCode, this.GetVersionCode);
            this.AddPropertyParser(versionName, this.GetVersionName);
        }

        /// <summary>
        /// Gets the version code of the specified package
        /// </summary>
        public VersionInfo VersionInfo
        {
            get
            {
                if (this.GetPropertyValue(versionCode) != null && this.GetPropertyValue(versionName) != null)
                {
                    return new VersionInfo((int)this.GetPropertyValue(versionCode), (string)this.GetPropertyValue(versionName));
                }
                else
                {
                    return null;
                }
            }
        }

        private void CheckPackagesSection(string line)
        {
            // This method check whether we're in the packages section of the dumpsys package output.
            // See gapps.txt for what the output looks for. Each section starts with a header
            // which looks like:
            //
            // HeaderName:
            //
            // and then there's indented data.

            // We check whether the line is indented. If it's not, and it's not an empty line, we take it is
            // a section header line and update the data accordingly.
            if (string.IsNullOrWhiteSpace(line))
            {
                return;
            }

            if (char.IsWhiteSpace(line[0]))
            {
                return;
            }

            this.inPackagesSection = string.Equals("Packages:", line, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Parses the given line and extracts the version name if possible.
        /// </summary>
        /// <param name="line">The line to be parsed.</param>
        /// <returns>The extracted version name.</returns>
        internal object GetVersionName(string line)
        {
            this.CheckPackagesSection(line);

            if (!this.inPackagesSection)
            {
                return null;
            }

            if (line != null && line.Trim().StartsWith("versionName="))
            {
                return line.Trim().Substring(12).Trim();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Parses the given line and extracts the version code if possible.
        /// </summary>
        /// <param name="line">The line to be parsed.</param>
        /// <returns>The extracted version code.</returns>
        internal object GetVersionCode(string line)
        {
            this.CheckPackagesSection(line);

            if (!this.inPackagesSection)
            {
                return null;
            }

            if (line == null)
            {
                return null;
            }

            // versionCode=4 minSdk=9 targetSdk=22
            string versionCodeRegex = @"versionCode=(\d*)( minSdk=(\d*))?( targetSdk=(\d*))?$";
            Match match = Regex.Match(line, versionCodeRegex);
            if (match.Success)
            {
                return int.Parse(match.Groups[1].Value.Trim());
            }
            else
            {
                return null;
            }
        }
    }
}
