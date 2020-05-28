// <copyright file="LogReader.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion. All rights reserved.
// </copyright>

namespace SharpAdbClient.Logs
{
    using SharpAdbClient.Exceptions;
    using System;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Processes Android log files in binary format. You usually get the binary format by
    /// running <c>logcat -B</c>.
    /// </summary>
    public class LogReader
    {
        private readonly Stream stream;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogReader"/> class.
        /// </summary>
        /// <param name="stream">
        /// A <see cref="Stream"/> that contains the logcat data.
        /// </param>
        public LogReader(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            this.stream = stream;
        }

        /// <summary>
        /// Reads the next <see cref="LogEntry"/> from the stream.
        /// </summary>
        /// <returns>
        /// A new <see cref="LogEntry"/> object.
        /// </returns>
        public async Task<LogEntry> ReadEntry(CancellationToken cancellationToken)
        {
            LogEntry value = new LogEntry();

            // Read the log data in binary format. This format is defined at
            // https://android.googlesource.com/platform/system/core/+/master/include/log/logger.h
            // https://android.googlesource.com/platform/system/core/+/67d7eaf/include/log/logger.h
            var payloadLengthValue = await this.ReadUInt16Async(cancellationToken).ConfigureAwait(false);
            var headerSizeValue = payloadLengthValue == null ? null : await this.ReadUInt16Async(cancellationToken).ConfigureAwait(false);
            var pidValue = headerSizeValue == null ? null : await this.ReadInt32Async(cancellationToken).ConfigureAwait(false);
            var tidValue = pidValue == null ? null : await this.ReadInt32Async(cancellationToken).ConfigureAwait(false);
            var secValue = tidValue == null ? null : await this.ReadInt32Async(cancellationToken).ConfigureAwait(false);
            var nsecValue = secValue == null ? null : await this.ReadInt32Async(cancellationToken).ConfigureAwait(false);

            if (nsecValue == null)
            {
                return null;
            }

            var payloadLength = payloadLengthValue.Value;
            var headerSize = headerSizeValue.Value;
            var pid = pidValue.Value;
            var tid = tidValue.Value;
            var sec = secValue.Value;
            var nsec = nsecValue.Value;

            // If the headerSize is not 0, we have on of the logger_entry_v* objects.
            // In all cases, it appears that they always start with a two uint16's giving the
            // header size and payload length.
            // For both objects, the size should be 0x18
            uint id = 0;
            uint uid = 0;

            if (headerSize != 0)
            {
                if (headerSize >= 0x18)
                {
                    var idValue = await this.ReadUInt32Async(cancellationToken).ConfigureAwait(false);

                    if (idValue == null)
                    {
                        return null;
                    }

                    id = idValue.Value;
                }

                if (headerSize >= 0x1c)
                {
                    var uidValue = await this.ReadUInt32Async(cancellationToken).ConfigureAwait(false);

                    if (uidValue == null)
                    {
                        return null;
                    }

                    uid = uidValue.Value;
                }

                if (headerSize >= 0x20)
                {
                    // Not sure what this is.
                    await this.ReadUInt32Async(cancellationToken).ConfigureAwait(false);
                }

                if (headerSize > 0x20)
                {
                    throw new AdbException($"An error occurred while reading data from the ADB stream. Although the header size was expected to be 0x18, a header size of 0x{headerSize:X} was sent by the device");
                }
            }

            byte[] data = await this.ReadBytesSafeAsync(payloadLength, cancellationToken).ConfigureAwait(false);

            if (data == null)
            {
                return null;
            }

            var timestamp = DateTimeOffset.FromUnixTimeSeconds(sec);

            switch ((LogId)id)
            {
                case LogId.Crash:
                case LogId.Kernel:
                case LogId.Main:
                case LogId.Radio:
                case LogId.System:
                    {
                        // format: <priority:1><tag:N>\0<message:N>\0
                        var priority = data[0];

                        // Find the first \0 byte in the array. This is the seperator
                        // between the tag and the actual message
                        int tagEnd = 1;

                        while (data[tagEnd] != '\0' && tagEnd < data.Length)
                        {
                            tagEnd++;
                        }

                        // Message should be null termintated, so remove the last entry, too (-2 instead of -1)
                        string tag = Encoding.ASCII.GetString(data, 1, tagEnd - 1);
                        string message = Encoding.ASCII.GetString(data, tagEnd + 1, data.Length - tagEnd - 2);

                        return new AndroidLogEntry()
                        {
                            Data = data,
                            ProcessId = pid,
                            ThreadId = tid,
                            TimeStamp = timestamp,
                            Id = id,
                            Priority = (Priority)priority,
                            Message = message,
                            Tag = tag
                        };
                    }

                case LogId.Events:
                    {
                        // https://android.googlesource.com/platform/system/core.git/+/master/liblog/logprint.c#547
                        var entry = new EventLogEntry()
                        {
                            Data = data,
                            ProcessId = pid,
                            ThreadId = tid,
                            TimeStamp = timestamp,
                            Id = id
                        };

                        // Use a stream on the data buffer. This will make sure that,
                        // if anything goes wrong parsing the data, we never go past
                        // the message boundary itself.
                        using (MemoryStream dataStream = new MemoryStream(data))
                        using (BinaryReader reader = new BinaryReader(dataStream))
                        {
                            var priority = reader.ReadInt32();

                            while (dataStream.Position < dataStream.Length)
                            {
                                this.ReadLogEntry(reader, entry.Values);
                            }
                        }

                        return entry;
                    }

                default:
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

        private void ReadLogEntry(BinaryReader reader, Collection<object> parent)
        {
            var type = (EventLogType)reader.ReadByte();

            switch (type)
            {
                case EventLogType.Float:
                    parent.Add(reader.ReadSingle());

                    break;

                case EventLogType.Integer:
                    parent.Add(reader.ReadInt32());
                    break;

                case EventLogType.Long:
                    parent.Add(reader.ReadInt64());
                    break;

                case EventLogType.List:
                    var listLength = reader.ReadByte();

                    Collection<object> list = new Collection<object>();

                    for (int i = 0; i < listLength; i++)
                    {
                        this.ReadLogEntry(reader, list);
                    }

                    parent.Add(list);
                    break;

                case EventLogType.String:
                    int stringLength = reader.ReadInt32();
                    byte[] messageData = reader.ReadBytes(stringLength);
                    string message = Encoding.ASCII.GetString(messageData);
                    parent.Add(message);
                    break;
            }
        }

        private async Task<ushort?> ReadUInt16Async(CancellationToken cancellationToken)
        {
            byte[] data = await this.ReadBytesSafeAsync(2, cancellationToken).ConfigureAwait(false);

            if (data == null)
            {
                return null;
            }

            return BitConverter.ToUInt16(data, 0);
        }

        private async Task<uint?> ReadUInt32Async(CancellationToken cancellationToken)
        {
            byte[] data = await this.ReadBytesSafeAsync(4, cancellationToken).ConfigureAwait(false);

            if (data == null)
            {
                return null;
            }

            return BitConverter.ToUInt32(data, 0);
        }

        private async Task<int?> ReadInt32Async(CancellationToken cancellationToken)
        {
            byte[] data = await this.ReadBytesSafeAsync(4, cancellationToken).ConfigureAwait(false);

            if (data == null)
            {
                return null;
            }

            return BitConverter.ToInt32(data, 0);
        }

        private async Task<byte[]> ReadBytesSafeAsync(int count, CancellationToken cancellationToken)
        {
            int totalRead = 0;
            int read = 0;

            byte[] data = new byte[count];

            while ((read = await this.stream.ReadAsync(data, totalRead, count - totalRead, cancellationToken).ConfigureAwait(false)) > 0)
            {
                totalRead += read;
            }

            if (totalRead < count)
            {
                return null;
            }

            return data;
        }
    }
}
