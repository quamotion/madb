//-----------------------------------------------------------------------
// <copyright file="AndroidProcess.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace SharpAdbClient.DeviceCommands
{
    using System;
    using System.Globalization;

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
        /// Gest or sets the parent Process ID number.
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
        public uint WChan
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
        /// Parses an line of output of the <c>ps</c> command into a <see cref="AndroidProcess"/>
        /// object.
        /// </summary>
        /// <param name="line">
        /// The line to parse.
        /// </param>
        /// <returns>
        /// A <see cref="AndroidProcess"/> that represents the process.
        /// </returns>
        public static AndroidProcess Parse(string line)
        {
            if (line == null)
            {
                throw new ArgumentNullException(nameof(line));
            }

            // Sample input:
            // USER     PID   PPID  VSIZE  RSS     WCHAN    PC         NAME
            // system    479   138   446284 21100 ffffffff b765ffe6 S com.microsoft.xde.donatelloservice
            string[] parts = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 9)
            {
                throw new ArgumentOutOfRangeException(nameof(line));
            }

            AndroidProcess value = new AndroidProcess()
            {
                User = parts[0],
                ProcessId = int.Parse(parts[1]),
                ParentProcessId = int.Parse(parts[2]),
                VirtualSize = int.Parse(parts[3]),
                ResidentSetSize = int.Parse(parts[4]),
                WChan = uint.Parse(parts[5], NumberStyles.HexNumber),
                Pc = uint.Parse(parts[6], NumberStyles.HexNumber),
                State = (AndroidProcessState)Enum.Parse(typeof(AndroidProcessState), parts[7]),
                Name = parts[8]
            };

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
    }
}
