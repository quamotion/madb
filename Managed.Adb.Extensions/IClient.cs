// <copyright file="IClient.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion. All rights reserved.
// </copyright>

namespace Managed.Adb
{
    using System.Net.Sockets;

    /// <summary>
    /// <para>This represents a single client, usually a Dalvik VM process.
    /// </para>
    /// <para>
    /// This class gives access to basic client information, as well as methods to perform actions
    /// on the client.
    /// </para>
    /// <para>
    /// More detailed information, usually updated in real time, can be access through the
    /// <see cref="ClientData"/> class. Each <see cref="Client"/> object has its own <see cref="ClientData"/>
    /// accessed through <see cref="ClientData"/>.
    /// </para>
    /// </summary>
    /// <seealso cref=""/>
    public interface IClient : IPacketConsumer
    {
        ClientConnectionState ConnectionState { get; }

        ClientChangeState ChangeState { get; }

        Socket Channel { get; set; }

        IDevice Device { get; }

        Device DeviceImplementation { get; }

        int DebuggerListenPort { get; }

        bool IsDdmAware { get; }

        bool IsDebuggerAttached { get; }

        Debugger Debugger { get; }

        ClientData ClientData { get; }

        bool IsThreadUpdateEnabled { get; set; }

        bool IsHeapUpdateEnabled { get; set; }

        bool IsSelectedClient { get; set; }

        bool IsValid { get; }

        void ExecuteGarbageCollector();

        void DumpHprof();

        void ToggleMethodProfiling();

        void RequestMethodProfilingStatus();

        void RequestThreadUpdate();

        void RequestThreadStackTrace(int threadID);

        bool RequestNativeHeapInformation();

        void EnableAllocationTracker(bool enable);

        void RequestAllocationStatus();

        void RequestAllocationDetails();

        void Kill();

        // TODO: Define Selector
        void Register(/*Selector*/ object selector);

        void ListenForDebugger(int listenPort);

        // TODO: JdwpPacket
        void SendAndConsume(/*JdwpPacket*/ object packet, ChunkHandler replyHandler);

        void AddRequestId(int id, ChunkHandler handler);

        void RemoveRequestId(int id);

        ChunkHandler IsResponseToUs(int id);

        // TODO: JdwpPacket
        void PacketFailed(/*JdwpPacket*/ object packet);

        bool DdmSeen();

        void Close(bool notify);

        void Update(ClientChangeMask changeMask);
    }
}
