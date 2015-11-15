// <copyright file="LogEntry.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion. All rights reserved.
// </copyright>

namespace SharpAdbClient.Logs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public sealed class LogEntry
    {
        public int Length { get; set; }

        public int ProcessId { get; set; }

        public int ThreadId { get; set; }

        public DateTime TimeStamp { get; set; }

        public int NanoSeconds { get; set; }

        public byte[] Data { get; set; }
    }
}
