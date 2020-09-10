// <copyright file="TcpSocket.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion. All rights reserved.
// </copyright>

namespace SharpAdbClient
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Implements the <see cref="ITcpSocket" /> interface using the standard <see cref="Socket"/>
    /// class.
    /// </summary>
    public class TcpSocket : ITcpSocket
    {
        private Socket socket;
        private EndPoint endPoint;

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpSocket"/> class.
        /// </summary>
        public TcpSocket()
        {
            this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        /// <inheritdoc/>
        public bool Connected
        {
            get
            {
                return this.socket.Connected;
            }
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public int ReceiveTimeout
        {
            get
            {
                return this.socket.ReceiveTimeout;
            }

            set
            {
                this.socket.ReceiveTimeout = value;
            }
        }

        /// <inheritdoc/>
        public void Connect(EndPoint endPoint)
        {
            if (!(endPoint is IPEndPoint || endPoint is DnsEndPoint))
            {
                throw new NotSupportedException();
            }

            this.socket.Connect(endPoint);
            this.socket.Blocking = true;
            this.endPoint = endPoint;
        }

        /// <inheritdoc/>
        public Task ConnectAsync(EndPoint endPoint, CancellationToken cancellationToken)
        {
            if (!(endPoint is IPEndPoint || endPoint is DnsEndPoint))
            {
                throw new NotSupportedException();
            }

            return Task.Run(() =>
            {
                var completedEvent = new ManualResetEvent(false);
                bool successful = false;
                var asyncEvent = new SocketAsyncEventArgs();
                asyncEvent.RemoteEndPoint = endPoint;
                asyncEvent.Completed += (sender, args) =>
                {
                    successful = true;
                    completedEvent.Set();
                };
                cancellationToken.Register(() =>
                {
                    Socket.CancelConnectAsync(asyncEvent);
                    completedEvent.Set();
                });

                this.socket.ConnectAsync(asyncEvent);
                completedEvent.WaitOne();
                if (successful)
                {
                    this.socket.Blocking = true;
                    this.endPoint = endPoint;
                }
                if (cancellationToken.IsCancellationRequested)
                    throw new TaskCanceledException();
            });
        }

        /// <inheritdoc/>
        public Task ReconnectAsync(CancellationToken cancellationToken)
        {
            if (this.socket.Connected)
            {
                // Already connected - nothing to do.
                return Task.CompletedTask;
            }

            this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            return this.ConnectAsync(this.endPoint, cancellationToken);
        }

        /// <inheritdoc/>
        public void Reconnect()
        {
            if (this.socket.Connected)
            {
                // Already connected - nothing to do.
                return;
            }

            this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.Connect(this.endPoint);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.socket.Dispose();
        }

        /// <inheritdoc/>
        public int Send(byte[] buffer, int offset, int size, SocketFlags socketFlags)
        {
            return this.socket.Send(buffer, offset, size, socketFlags);
        }

        /// <inheritdoc/>
        public Task<int> SendAsync(byte[] buffer, int offset, int size, SocketFlags socketFlags, CancellationToken cancellationToken)
        {
            return this.socket.SendAsync(buffer, offset, size, socketFlags, cancellationToken);
        }

        /// <inheritdoc/>
        public int Receive(byte[] buffer, int offset, SocketFlags socketFlags)
        {
            return this.socket.Receive(buffer, offset, socketFlags);
        }

        /// <inheritdoc/>
        public Task<int> ReceiveAsync(byte[] buffer, int offset, int size, SocketFlags socketFlags, CancellationToken cancellationToken)
        {
            return this.socket.ReceiveAsync(buffer, offset, size, socketFlags, cancellationToken);
        }

        /// <inheritdoc/>
        public Stream GetStream()
        {
            return new NetworkStream(this.socket);
        }
    }
}
