// <copyright file="ConsoleOutputReceiver.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion. All rights reserved.
// </copyright>

namespace SharpAdbClient
{
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
        private const string TAG = "ConsoleOutputReceiver";

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
        /// Processes the new lines.
        /// </summary>
        /// <param name="lines">The lines.</param>
        protected override void ProcessNewLines(string[] lines)
        {
            foreach (var line in lines)
            {
                if (string.IsNullOrEmpty(line) || line.StartsWith("#") || line.StartsWith("$"))
                {
                    continue;
                }

                this.output.AppendLine(line);

                Log.d(TAG, line);
            }
        }
    }
}
