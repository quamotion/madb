//-----------------------------------------------------------------------
// <copyright file="ProcessOutputReceiver.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace SharpAdbClient.DeviceCommands
{
    using SharpAdbClient;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Text;

    /// <summary>
    /// Parses output of the <c>ps</c> command into <see cref="AndroidProcess"/> objects.
    /// </summary>
    internal class ProcessOutputReceiver : IShellOutputReceiver
    {
        /// <summary>
        /// Backing field for the <see cref="Processes"/> property.
        /// </summary>
        private readonly Collection<AndroidProcess> processes = new Collection<AndroidProcess>();

        /// <summary>
        /// A buffer that contains the output that was sent from the device to the PC.
        /// </summary>
        private StringBuilder buffer = new StringBuilder();

        /// <summary>
        /// Gets a value indicating whether the receiver has been cancelled.
        /// </summary>
        public bool IsCancelled
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether any errors occurred during parsing.
        /// </summary>
        public bool ParsesErrors
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the list of processes that have been detected.
        /// </summary>
        public Collection<AndroidProcess> Processes
        {
            get
            {
                return this.processes;
            }
        }

        /// <summary>
        /// Adds the output.
        /// </summary>
        /// <param name="line">
        /// The output htat was received.
        /// </param>
        public void AddOutput(string line)
        {
            this.buffer.AppendLine(line);
        }

        /// <summary>
        /// Flushes the output and populates the <see cref="Processes"/> property.
        /// </summary>
        public void Flush()
        {
            bool firstLine = true;

            using (StringReader reader = new StringReader(this.buffer.ToString()))
            {
                while (reader.Peek() >= 0)
                {
                    string line = reader.ReadLine();

                    if (firstLine)
                    {
                        firstLine = false;
                        continue;
                    }

                    this.processes.Add(AndroidProcess.Parse(line));
                }
            }
        }
    }
}
