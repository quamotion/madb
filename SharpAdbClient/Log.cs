// <copyright file="Log.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion. All rights reserved.
// </copyright>

namespace SharpAdbClient
{
    using System;

    /// <summary>
    ///
    /// </summary>
    public static class Log
    {
        /// <summary>
        /// Gets or sets the level.
        /// </summary>
        /// <value>The level.</value>
        public static LogLevel.LogLevelInfo Level { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ILogOutput">LogOutput</see>
        /// </summary>
        public static ILogOutput LogOutput { get; set; }

        /// <summary>
        /// Initializes static members of the <see cref="Log"/> class.
        /// </summary>
        static Log()
        {
            Level = DdmPreferences.LogLevel;
        }

        /// <summary>
        /// Outputs a Verbose level message.
        /// </summary>
        /// <param name="tag">The tag associated with the message.</param>
        /// <param name="format">The message to output format string.</param>
        /// <param name="args">The values for the format message</param>
        public static void v(string tag, string format, params object[] args)
        {
            WriteLine(LogLevel.Verbose, tag, string.Format(format, args));
        }

        /// <summary>
        /// Outputs a Debug level message.
        /// </summary>
        /// <param name="tag">The tag associated with the message.</param>
        /// <param name="format">The message to output format string.</param>
        /// <param name="args">The values for the format message</param>
        public static void d(string tag, string format, params object[] args)
        {
            WriteLine(LogLevel.Debug, tag, string.Format(format, args));
        }

        /// <summary>
        /// Outputs a Info level message.
        /// </summary>
        /// <param name="tag">The tag associated with the message.</param>
        /// <param name="format">The message to output format string.</param>
        /// <param name="args">The values for the format message</param>
        public static void i(string tag, string format, params object[] args)
        {
            WriteLine(LogLevel.Info, tag, string.Format(format, args));
        }

        /// <summary>
        /// Outputs a Warn level message.
        /// </summary>
        /// <param name="tag">The tag associated with the message.</param>
        /// <param name="format">The message to output format string.</param>
        /// <param name="args">The values for the format message</param>
        public static void w(string tag, string format, params object[] args)
        {
            WriteLine(LogLevel.Warn, tag, string.Format(format, args));
        }

        /// <summary>
        /// Outputs a Warn level message.
        /// </summary>
        /// <param name="tag">The tag associated with the message.</param>
        /// <param name="exception">The exception to warn</param>
        public static void w(string tag, Exception exception)
        {
            if (exception != null)
            {
                w(tag, exception.ToString());
            }
        }

        /// <summary>
        /// Outputs a Error level message.
        /// </summary>
        /// <param name="tag">The tag associated with the message.</param>
        /// <param name="format">The message to output format string.</param>
        /// <param name="args">The values for the format message</param>
        /// <gist id="16a731d7e4f074fca809" />
        public static void e(string tag, string format, params object[] args)
        {
            WriteLine(LogLevel.Error, tag, string.Format(format, args));
        }

        /// <summary>
        /// Outputs a Error level message.
        /// </summary>
        /// <param name="tag">The tag associated with the message.</param>
        /// <param name="exception">The exception to warn</param>
        /// <gist id="4e0438f59a00d57af4ef"/>
        public static void e(string tag, Exception exception)
        {
            if (exception != null)
            {
                e(tag, exception.ToString());
            }
        }

        /// <summary>
        /// Outputs a log message and attempts to display it in a dialog.
        /// </summary>
        /// <param name="logLevel">The log level</param>
        /// <param name="tag">The tag associated with the message.</param>
        /// <param name="message">The message to output.</param>
        public static void LogAndDisplay(LogLevel.LogLevelInfo logLevel, string tag, string message)
        {
            if (LogOutput != null)
            {
                LogOutput.WriteAndPromptLog(logLevel, tag, message);
            }
            else
            {
                WriteLine(logLevel, tag, message);
            }
        }

        /// <summary>
        /// prints to stdout; could write to a log window
        /// </summary>
        /// <param name="logLevel"></param>
        /// <param name="tag"></param>
        /// <param name="message"></param>
        private static void WriteLine(LogLevel.LogLevelInfo logLevel, string tag, string message)
        {
            if (logLevel.Priority >= Level.Priority)
            {
                if (LogOutput != null)
                {
                    LogOutput.Write(logLevel, tag, message);
                }
                else
                {
                    Write(logLevel, tag, message);
                }
            }
        }

        /// <summary>
        /// Prints a log message.
        /// </summary>
        /// <param name="logLevel"></param>
        /// <param name="tag"></param>
        /// <param name="message"></param>
        public static void Write(LogLevel.LogLevelInfo logLevel, string tag, string message)
        {
            Console.Write(GetLogFormatString(logLevel, tag, message));
        }

        /// <summary>
        /// Formats a log message.
        /// </summary>
        /// <param name="logLevel"></param>
        /// <param name="tag"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static string GetLogFormatString(LogLevel.LogLevelInfo logLevel, string tag, string message)
        {
            long totalmsec = DateTime.Now.ToUnixEpoch();
            var sec = (totalmsec / 60000) % 60;
            var msec = (totalmsec / 1000) % 60;
            return string.Format($"{sec:00}:{msec:00} {logLevel.Letter}/{tag}: {message}\n");
        }
    }
}
