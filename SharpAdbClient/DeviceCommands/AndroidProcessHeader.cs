// <copyright file="AndroidProcessHeader.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion. All rights reserved.
// </copyright>

namespace SharpAdbClient.DeviceCommands
{
    /// <summary>
    /// Represents information of the <c>ps</c> command output.
    /// </summary>
    public class AndroidProcessHeader
    {
        /// <summary>
        /// Gets or sets the index of the <see cref="AndroidProcess.User"/> field.
        /// </summary>
        public int UserIndex { get; set; }

        /// <summary>
        /// Gets or sets the index of the <see cref="AndroidProcess.ProcessId"/> field.
        /// </summary>
        public int ProcessIdIndex { get; set; }

        /// <summary>
        /// Gets or sets the index of the <see cref="AndroidProcess.ParentProcessId"/> field.
        /// </summary>
        public int ParentProcessIdIndex { get; set; }

        /// <summary>
        /// Gets or sets the index of the <see cref="AndroidProcess.VirtualSize"/> field.
        /// </summary>
        public int VirtualSizeIndex { get; set; }

        /// <summary>
        /// Gets or sets the index of the <see cref="AndroidProcess.ResidentSetSize"/> field.
        /// </summary>
        public int ResidentSetSizeIndex { get; set; }

        /// <summary>
        /// Gets or sets the index of the <see cref="AndroidProcess.WChan"/> field.
        /// </summary>
        public int WChanIndex { get; set; }

        /// <summary>
        /// Gets or sets the index of the <see cref="AndroidProcess.Pc"/> field.
        /// </summary>
        public int PcIndex { get; set; }

        /// <summary>
        /// Gets or sets the index of the <see cref="AndroidProcess.State"/> field.
        /// </summary>
        public int StateIndex { get; set; }

        /// <summary>
        /// Gets or sets the index of the <see cref="AndroidProcess.Name"/> field.
        /// </summary>
        public int NameIndex { get; set; }
    }
}
