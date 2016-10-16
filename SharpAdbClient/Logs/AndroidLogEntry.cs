// <copyright file="AndroidLogEntry.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion. All rights reserved.
// </copyright>

using System.Collections.Generic;

namespace SharpAdbClient.Logs
{
    /// <summary>
    /// Represents a standard Android log entry (an entry in any Android log buffer
    /// except the Event buffer).
    /// </summary>
    /// <seealso href="https://android.googlesource.com/platform/system/core/+/master/liblog/logprint.c#442"/>
    public class AndroidLogEntry : LogEntry
    {
        /// <summary>
        /// Maps Android log priorities to chars used to represent them in the system log.
        /// </summary>
        private static readonly Dictionary<Priority, char> PriorityFormatters;

        /// <summary>
        /// Initializes static members of the <see cref="AndroidLogEntry"/> class.
        /// </summary>
        static AndroidLogEntry()
        {
            PriorityFormatters = new Dictionary<Priority, char>();
            PriorityFormatters.Add(Priority.Verbose, 'V');
            PriorityFormatters.Add(Priority.Debug, 'D');
            PriorityFormatters.Add(Priority.Info, 'I');
            PriorityFormatters.Add(Priority.Warn, 'W');
            PriorityFormatters.Add(Priority.Error, 'E');
            PriorityFormatters.Add(Priority.Assert, 'A');
        }

        /// <summary>
        /// Gets or sets the priority of the log message.
        /// </summary>
        public Priority Priority
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the log tag of the message. Used to identify the source of a log message.
        /// It usually identifies the class or activity where the log call occured.
        /// </summary>
        public string Tag
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the message that has been logged.
        /// </summary>
        public string Message
        {
            get;
            set;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{this.TimeStamp:yy-MM HH:mm:ss.fff} {this.ProcessId, 5} {this.ProcessId, 5} {FormatPriority(this.Priority)} {this.Tag, -8}: {this.Message}";
        }

        /// <summary>
        /// Converts a <see cref="Priority"/> value to a char that represents that value in the system log.
        /// </summary>
        /// <param name="value">
        /// The value to convert.
        /// </param>
        /// <returns>
        /// A <see cref="char"/> that represents <paramref name="value"/> in the sysem log.
        /// </returns>
        private static char FormatPriority(Priority value)
        {
            if (PriorityFormatters == null || !PriorityFormatters.ContainsKey(value))
            {
                return '?';
            }
            else
            {
                return PriorityFormatters[value];
            }
        }
    }
}
