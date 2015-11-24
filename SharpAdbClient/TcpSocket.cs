// <copyright file="TcpSocket.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion. All rights reserved.
// </copyright>

namespace SharpAdbClient
{
    using System.Net;
    using System.Net.Sockets;
    using System.Threading.Tasks;

    /// <summary>
    /// Implements the <see cref="ITcpSocket" /> interface using the standard <see cref="Socket"/>
    /// class.
    /// </summary>
    public class TcpSocket : ITcpSocket
    {
        private Socket socket;

        public TcpSocket()
        {
            this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public void Connect(IPEndPoint endPoint)
        {
            this.socket.Connect(endPoint);
            this.socket.Blocking = true;
        }

        public void Close()
        {
            this.socket.Close();
        }

        public void Dispose()
        {
            this.socket.Dispose();
        }

        public int Send(byte[] buffer, int offset, int size, SocketFlags socketFlags)
        {
            return this.socket.Send(buffer, offset, size, socketFlags);
        }

        public NetworkStream GetStream()
        {
            return new NetworkStream(this.socket);
        }

        public int Receive(byte[] buffer, int offset, SocketFlags socketFlags)
        {
            return this.socket.Receive(buffer, offset, socketFlags);
        }

        public Task<int> ReceiveAsync(byte[] buffer, int offset, int size, SocketFlags socketFlags)
        {
            return this.socket.ReceiveAsync(buffer, offset, size, socketFlags);
        }

        public bool Connected
        {
            get
            {
                return this.socket.Connected;
            }
        }

        public int ReceiveBufferSize
        {
            get
            {
                return this.socket.ReceiveBufferSize;
            }

            set
            {
                this.socket.ReceiveBufferSize = value;
            }
        }
    }
}
