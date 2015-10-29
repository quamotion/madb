// <copyright file="IAdbSocket.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion. All rights reserved.
// </copyright>

namespace Managed.Adb
{
    using System;
    using System.Net.Sockets;

    public interface IAdbSocket : IDisposable
    {
        bool Connected { get; }
        void SendAdbRequest(string request);

        AdbResponse ReadAdbResponse(bool readDiagString);

        void Read(byte[] data);
        int Read(byte[] data, int timeout);

        string ReadString();

        void Close();

        void SendFileRequest(string command, string path, SyncService.FileMode mode);
        void SendSyncRequest(string command, int value);

        void Send(byte[] data, int length, int timeout);

        void Read(byte[] data, int length, int timeout);
    }
}
