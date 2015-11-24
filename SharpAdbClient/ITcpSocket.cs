// <copyright file="ITcpSocket.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion. All rights reserved.
// </copyright>

namespace SharpAdbClient
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides an interface that allows access to the standard .NET <see cref="Socket"/>
    /// class. The main purpose of this interface is to enable mocking of the <see cref="Socket"/>
    /// in unit test scenarios.
    /// </summary>
    public interface ITcpSocket : IDisposable
    {
        void Connect(IPEndPoint endPoint);

        void Close();

        int Send(byte[] buffer, int offset, int size, SocketFlags socketFlags);

        int Receive(byte[] buffer, int size, SocketFlags socketFlags);

        Task<int> ReceiveAsync(byte[] buffer, int offset, int size, SocketFlags socketFlags);

        Stream GetStream();

        bool Connected
        {
            get;
        }

        int ReceiveBufferSize
        {
            get;
            set;
        }
    }
}
