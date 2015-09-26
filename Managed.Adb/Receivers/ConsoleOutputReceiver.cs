namespace Managed.Adb
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    ///
    /// </summary>
    public class ConsoleOutputReceiver : MultiLineReceiver
    {
        /// <summary>
        /// Logging tag
        /// </summary>
        private const string TAG = "ConsoleOutputReceiver";

        private static ConsoleOutputReceiver _instance = null;

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
                if (_instance == null)
                {
                    _instance = new ConsoleOutputReceiver();
                }

                return _instance;
            }
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

                    Log.d(TAG, line);
                }
            }
    }
}
