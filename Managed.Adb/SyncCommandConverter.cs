// <copyright file="SyncCommandConverter.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion. All rights reserved.
// </copyright>

namespace Managed.Adb
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class SyncCommandConverter
    {
        private static readonly Dictionary<SyncCommand, string> values = new Dictionary<SyncCommand, string>();

        static SyncCommandConverter()
        {
            values.Add(SyncCommand.DATA, "DATA");
            values.Add(SyncCommand.DENT, "DENT");
            values.Add(SyncCommand.DONE, "DONE");
            values.Add(SyncCommand.FAIL, "FAIL");
            values.Add(SyncCommand.LIST, "LIST");
            values.Add(SyncCommand.OKAY, "OKAY");
            values.Add(SyncCommand.RECV, "RECV");
            values.Add(SyncCommand.SEND, "SEND");
            values.Add(SyncCommand.STAT, "STAT");
        }

        public static byte[] GetBytes(SyncCommand command)
        {
            if (!values.ContainsKey(command))
            {
                throw new ArgumentOutOfRangeException(nameof(command), $"{command} is not a valid sync command");
            }

            string commandText = values[command];
            byte[] commandBytes = AdbClient.Encoding.GetBytes(commandText);

            return commandBytes;
        }

        public static SyncCommand GetCommand(byte[] value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (value.Length != 4)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            string commandText = AdbClient.Encoding.GetString(value);

            var key = values.Where(d => string.Equals(commandText, commandText, StringComparison.OrdinalIgnoreCase)).Select(d => new SyncCommand?(d.Key)).SingleOrDefault();

            if (key == null)
            {
                throw new ArgumentOutOfRangeException(nameof(value), $"{commandText} is not a valid sync command");
            }

            return key.Value;
        }
    }
}
