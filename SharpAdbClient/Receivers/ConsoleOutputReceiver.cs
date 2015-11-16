// <copyright file="ConsoleOutputReceiver.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion. All rights reserved.
// </copyright>

namespace SharpAdbClient
{
    using Exceptions;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    /// <summary>
    /// Recieves console output, and makes the console output available as a <see cref="string"/>. To
    /// fetch the console output that was received, used the <see cref="ToString"/> method.
    /// </summary>
    public class ConsoleOutputReceiver : MultiLineReceiver
    {
        /// <summary>
        /// Logging tag
        /// </summary>
        private const string Tag = nameof(ConsoleOutputReceiver);

        private static ConsoleOutputReceiver instance = null;

        private StringBuilder output = new StringBuilder();

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        public static ConsoleOutputReceiver Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ConsoleOutputReceiver();
                }

                return instance;
            }
        }

        /// <summary>
        /// Gets a <see cref="string"/> that represents the current <see cref="ConsoleOutputReceiver"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="string"/> that represents the current <see cref="ConsoleOutputReceiver"/>.
        /// </returns>
        public override string ToString()
        {
            return this.output.ToString();
        }

        /// <summary>
        /// Throws an error message if the console output line contains an error message.
        /// </summary>
        /// <param name="line">
        /// The line to inspect.
        /// </param>
        public void ThrowOnError(string line)
        {
            if (!this.ParsesErrors)
            {
                if (line.EndsWith(": not found"))
                {
                    Log.w(Tag, $"The remote execution returned: '{line}'");
                    throw new FileNotFoundException($"The remote execution returned: '{line}");
                }

                if (line.EndsWith("No such file or directory"))
                {
                    Log.w(Tag, $"The remote execution returned: {line}");
                    throw new FileNotFoundException($"The remote execution returned: {line}");
                }

                // for "unknown options"
                if (line.Contains("Unknown option"))
                {
                    Log.w(Tag, $"The remote execution returned: {line}");
                    throw new UnknownOptionException(line);
                }

                // for "aborting" commands
                if (line.IsMatch("Aborting.$"))
                {
                    Log.w(Tag, $"The remote execution returned: {line}");
                    throw new CommandAbortingException(line);
                }

                // for busybox applets
                // cmd: applet not found
                if (line.IsMatch("applet not found$"))
                {
                    Log.w(Tag, $"The remote execution returned: '{line}'");
                    throw new FileNotFoundException($"The remote execution returned: '{line}'");
                }

                // checks if the permission to execute the command was denied.
                // workitem: 16822
                if (line.IsMatch("(permission|access) denied$"))
                {
                    Log.w(Tag, $"The remote execution returned: '{line}'");
                    throw new PermissionDeniedException(string.Format("The remote execution returned: '{line}'"));
                }
            }
        }

        /// <summary>
        /// Processes the new lines.
        /// </summary>
        /// <param name="lines">The lines.</param>
        protected override void ProcessNewLines(IEnumerable<string> lines)
        {
            foreach (var line in lines)
            {
                if (string.IsNullOrEmpty(line) || line.StartsWith("#") || line.StartsWith("$"))
                {
                    continue;
                }

                this.output.AppendLine(line);

                Log.d(Tag, line);
            }
        }
    }
}
