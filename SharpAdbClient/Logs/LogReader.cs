// <copyright file="LogReader.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion. All rights reserved.
// </copyright>

namespace SharpAdbClient.Logs
{
    using System;
    using System.IO;

    /// <summary>
    /// Processes Android log files in binary format. You usually get the binary format by
    /// running <c>logcat -B</c>.
    /// </summary>
    public class LogReader : BinaryReader
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LogReader"/> class.
        /// </summary>
        /// <param name="stream">
        /// A <see cref="Stream"/> that contains the logcat data.
        /// </param>
        public LogReader(Stream stream)
            : base(stream)
        {
        }

        /// <summary>
        /// Reads the next <see cref="LogEntry"/> from the stream.
        /// </summary>
        /// <returns>
        /// A new <see cref="LogEntry"/> object.
        /// </returns>
        public LogEntry ReadEntry()
        {
            LogEntry value = new LogEntry();

            // Read the log data in binary format. This format is defined at
            // https://android.googlesource.com/platform/system/core/+/master/include/log/logger.h
            var payloadLength = this.ReadUInt16();
            var headerSize = this.ReadUInt16();
            var pid = this.ReadInt32();
            var tid = this.ReadInt32();
            var sec = this.ReadInt32();
            var nsec = this.ReadInt32();

            // If the headerSize is not 0, we have either a logger_entry_v3 or logger_entry_v2 object.
            // For both objects, the size should be 0x18
            uint id = 0;
            if (headerSize != 0)
            {
                if (headerSize == 0x18)
                {
                    id = this.ReadUInt32();
                }
                else
                {
                    throw new Exception();
                }
            }

            byte[] data = this.ReadBytes(payloadLength);

            DateTime timestamp = DateTimeHelper.Epoch.AddSeconds(sec);
            timestamp = timestamp.AddMilliseconds(nsec / 1000000d);

            return new LogEntry()
            {
                Data = data,
                ProcessId = pid,
                ThreadId = tid,
                TimeStamp = timestamp,
                Id = id
            };
        }
    }
}
