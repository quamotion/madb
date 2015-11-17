using SharpAdbClient.DeviceCommands;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SharpAdbClient.Receivers
{
    /// <summary>
    /// 
    /// </summary>
    public class PackageManagerListingReceiver : MultiLineReceiver
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PackageManagerReceiver"/> class.
        /// </summary>
        /// <param name="entryMap">The entry map.</param>
        /// <param name="receiver">The receiver.</param>
        public PackageManagerListingReceiver(Dictionary<String, FileEntry> entryMap, IListingReceiver receiver)
        {
            this.Map = entryMap;
            this.Receiver = receiver;
        }

        /// <summary>
        /// Gets or sets the map.
        /// </summary>
        /// <value>The map.</value>
        public Dictionary<String, FileEntry> Map { get; set; }
        /// <summary>
        /// Gets or sets the receiver.
        /// </summary>
        /// <value>The receiver.</value>
        public IListingReceiver Receiver { get; set; }

        /// <summary>
        /// Processes the new lines.
        /// </summary>
        /// <param name="lines">The lines.</param>
        protected override void ProcessNewLines(IEnumerable<string> lines)
        {
            foreach (String line in lines)
            {
                if (line.Length > 0)
                {
                    // get the filepath and package from the line
                    Match m = Regex.Match(line, PackageManagerReceiver.PackagePattern, RegexOptions.Compiled);
                    if (m.Success)
                    {
                        // get the children with that path
                        FileEntry entry = Map[m.Groups[1].Value];
                        if (entry != null)
                        {
                            entry.Info = m.Groups[2].Value;
                            Receiver.RefreshEntry(entry);
                        }
                    }
                }
            }
        }
    }
}
