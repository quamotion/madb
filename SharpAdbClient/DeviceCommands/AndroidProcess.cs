//-----------------------------------------------------------------------
// <copyright file="AndroidProcess.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace SharpAdbClient.DeviceCommands
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    /// <summary>
    /// Represents a process running on an Android device.
    /// </summary>
    public class AndroidProcess
    {
        /// <summary>
        /// Gets or sets the username of the process's owner
        /// </summary>
        public string User
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the Process ID number.
        /// </summary>
        public int ProcessId
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the parent Process ID number.
        /// </summary>
        public int ParentProcessId
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets total VM size in bytes.
        /// </summary>
        public int VirtualSize
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the process resident set size.
        /// </summary>
        public int ResidentSetSize
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the memory address of the event the process is waiting for
        /// </summary>
        public string WChan
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the value of the <c>PC</c> field.
        /// </summary>
        public uint Pc
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name of the process, including arguments, if any.
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the state of the process.
        /// </summary>
        public AndroidProcessState State
        {
            get;
            set;
        }

        /// <summary>
        /// Parses the header of the <c>ps</c> command.
        /// </summary>
        /// <param name="header">
        /// A <see cref="string"/> that contains the <c>ps</c> command output header.
        /// </param>
        /// <returns>
        /// A <see cref="AndroidProcessHeader"/> that represents the header information.
        /// </returns>
        public static AndroidProcessHeader ParseHeader(string header)
        {
            // Sample input:
            // USER     PID   PPID  VSIZE  RSS     WCHAN    PC         NAME
            // system    479   138   446284 21100 ffffffff b765ffe6 S com.microsoft.xde.donatelloservice
            // OR:
            // PID USER       VSZ STAT COMMAND
            // 1 root       340 S /init
            AndroidProcessHeader value = new AndroidProcessHeader();

            List<string> parts = new List<string>(header.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries));
            value.UserIndex = parts.IndexOf("USER");
            value.ProcessIdIndex = parts.IndexOf("PID");
            value.ParentProcessIdIndex = parts.IndexOf("PPID");
            value.VirtualSizeIndex = IndexOf(parts, "VSIZE", "VSZ");
            value.ResidentSetSizeIndex = parts.IndexOf("RSS");
            value.WChanIndex = parts.IndexOf("WCHAN");
            value.PcIndex = parts.IndexOf("PC");
            value.StateIndex = value.PcIndex != -1 ? value.PcIndex + 1 : parts.IndexOf("STAT");
            value.NameIndex = IndexOf(parts, "NAME", "COMMAND");

            // If the pcIndex is present, we should also increase the name index by one, because the
            // state field comes in the middle yet has no header
            if (value.PcIndex != -1)
            {
                value.NameIndex++;
            }

            return value;
        }

        /// <summary>
        /// Parses an line of output of the <c>ps</c> command into a <see cref="AndroidProcess"/>
        /// object.
        /// </summary>
        /// <param name="line">
        /// The line to parse.
        /// </param>
        /// <param name="header">
        /// THe header information, that defines how the <paramref name="line"/> is structured.
        /// </param>
        /// <returns>
        /// A <see cref="AndroidProcess"/> that represents the process.
        /// </returns>
        public static AndroidProcess Parse(string line, AndroidProcessHeader header)
        {
            if (line == null)
            {
                throw new ArgumentNullException(nameof(line));
            }

            // Sample input:
            // USER     PID   PPID  VSIZE  RSS     WCHAN    PC         NAME
            // system    479   138   446284 21100 ffffffff b765ffe6 S com.microsoft.xde.donatelloservice
            // OR:
            // PID USER       VSZ STAT COMMAND
            // 1 root       340 S /init
            string[] parts = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

            AndroidProcess value = new AndroidProcess()
            {
                User = header.UserIndex != -1 ? parts[header.UserIndex] : null,
                ProcessId = header.ProcessIdIndex != -1 ? int.Parse(parts[header.ProcessIdIndex]) : -1,
                ParentProcessId = header.ParentProcessIdIndex != -1 ? int.Parse(parts[header.ParentProcessIdIndex]) : -1,
                VirtualSize = header.VirtualSizeIndex != -1 ? int.Parse(parts[header.VirtualSizeIndex]) : -1,
                ResidentSetSize = header.ResidentSetSizeIndex != -1 ? int.Parse(parts[header.ResidentSetSizeIndex]) : -1,
                WChan = header.WChanIndex != -1 ? parts[header.WChanIndex] : null,
                Pc = header.PcIndex != -1 ? uint.Parse(parts[header.PcIndex], NumberStyles.HexNumber) : uint.MaxValue,
                State = header.StateIndex != -1 ? (AndroidProcessState)Enum.Parse(typeof(AndroidProcessState), parts[header.StateIndex].Substring(0, 1)) : AndroidProcessState.Unknown,
                Name = header.NameIndex != -1 ? parts[header.NameIndex] : null
            };

            // If the name starts with [, remove the starting & trailing ] characters
            if (value.Name != null && value.Name.StartsWith("["))
            {
                value.Name = value.Name.Substring(1, value.Name.Length - 2);
            }

            return value;
        }

        /// <summary>
        /// Gets a <see cref="string"/> that represents this <see cref="AndroidProcess"/>,
        /// in the format of "<see cref="Name"/> (<see cref="ProcessId"/>)".
        /// </summary>
        /// <returns>
        /// A <see cref="string"/> that represents this <see cref="AndroidProcess"/>.
        /// </returns>
        public override string ToString()
        {
            return $"{this.Name} ({this.ProcessId})";
        }

        /// <summary>
        /// Gets the index of the first value of a set of values that is part of a list.
        /// </summary>
        /// <param name="list">
        /// The list in which to search for the value.
        /// </param>
        /// <param name="values">
        /// The values to search for.
        /// </param>
        /// <returns>
        /// The index of the first element in <paramref name="values"/> that is present in the list, or
        /// <c>-1</c>.
        /// </returns>
        private static int IndexOf(List<string> list, params string[] values)
        {
            foreach (var value in values)
            {
                int index = list.IndexOf(value);

                if (index != -1)
                {
                    return index;
                }
            }

            return -1;
        }
    }
}
