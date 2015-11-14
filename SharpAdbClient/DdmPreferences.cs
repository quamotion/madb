// <copyright file="DdmPreferences.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion. All rights reserved.
// </copyright>

namespace SharpAdbClient
{
    public static class DdmPreferences
    {
        /// <summary>
        /// Default timeout values for adb connection (milliseconds)
        /// </summary>
        public const int DEFAULT_TIMEOUT = 5000; // standard delay, in ms

        private static LogLevel.LogLevelInfo logLevel;

        public static LogLevel.LogLevelInfo DEFAULT_LOG_LEVEL
        { get; } = SharpAdbClient.LogLevel.Error;

        static DdmPreferences()
        {
            Timeout = DEFAULT_TIMEOUT;
            LogLevel = DEFAULT_LOG_LEVEL;
        }

        public static int Timeout { get; set; }

        public static LogLevel.LogLevelInfo LogLevel
        {
            get
            {
                return logLevel;
            }

            set
            {
                logLevel = value;
                Log.Level = value;
            }
        }
    }
}
