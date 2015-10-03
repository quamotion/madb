// <copyright file="ClientChangeState.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion. All rights reserved.
// </copyright>

namespace Managed.Adb
{
    using System;

    /// <summary>
    /// Specifies which parts of the client state have changed.
    /// </summary>
    [Flags]
    public enum ClientChangeState
    {
        /// <summary>
        /// The application name has changed.
        /// </summary>
        Name = 0x0001,

        /// <summary>
        /// The debugger status has changed.
        /// </summary>
        DebuggerStatus = 0x0002,

        /// <summary>
        /// The debugger port has changed.
        /// </summary>
        Port = 0x0004,

        /// <summary>
        /// The thread mode has changed.
        /// </summary>
        ThreadMode = 0x0008,

        /// <summary>
        /// The thread data has been updated.
        /// </summary>
        ThreadData = 0x0010,

        /// <summary>
        /// The heap mode has changed.
        /// </summary>
        HeapMode = 0x0020,

        /// <summary>
        /// The heap data has been updated.
        /// </summary>
        HeapData = 0x0040,

        /// <summary>
        /// The native heap data has been updated.
        /// </summary>
        NativeHeapData = 0x0080,

        /// <summary>
        /// The thread stack trace data has been updated.
        /// </summary>
        ThreadStackTrace = 0x0100,

        /// <summary>
        /// The heap allocation information has been updated.
        /// </summary>
        HeapAllocations = 0x0200,

        /// <summary>
        /// The heap allocation information status has changed.
        /// </summary>
        HeapAllocationStatus = 0x0400,

        /// <summary>
        /// The method profiling status has changed.
        /// </summary>
        MethodProfilingStatus = 0x0800,

        /// <summary>
        /// The heap/CPU profiling data has been updated.
        /// </summary>
        Hprof = 0x1000,

        /// <summary>
        /// A combination of the <seealso cref="Name"/>,
        /// <seealso cref="DebuggerStatus"/> and <seealso cref="Port"/>
        /// values.
        /// </summary>
        Info = Name | DebuggerStatus | Port,
    }
}
