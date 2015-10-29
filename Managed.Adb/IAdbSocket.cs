// <copyright file="IAdbSocket.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion. All rights reserved.
// </copyright>

namespace Managed.Adb
{
    using System;

    public interface IAdbSocket : IDisposable
    {
        void SendAdbRequest(string request);

        AdbResponse ReadAdbResponse(bool readDiagString);

        void Read(byte[] data);

        string ReadString();
    }
}
