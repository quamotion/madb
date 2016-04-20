using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace SharpAdbClient.Tests
{
    internal class DummyTcpSocket : ITcpSocket
    {
        /// <summary>
        /// The stream from which the <see cref="DummyTcpSocket"/> reads.
        /// </summary>
        public MemoryStream InputStream
        { get; set; } = new MemoryStream();

        /// <summary>
        /// The stream to which the <see cref="DummyTcpSocket"/> writes.
        /// </summary>
        public MemoryStream OutputStream
        { get; set; } = new MemoryStream();

        public bool Connected
        { get; set; } = true;

        public int ReceiveBufferSize
        { get; set; } = 1024;

        public void Close()
        {
            this.Connected = false;
        }

        public void Connect(EndPoint endPoint)
        {
            this.Connected = true;
        }

        public void Dispose()
        {
            this.Connected = false;
        }

        public Stream GetStream()
        {
            return this.OutputStream;
        }

        public int Receive(byte[] buffer, int size, SocketFlags socketFlags)
        {
            return this.InputStream.Read(buffer, 0, size);
        }

        public Task<int> ReceiveAsync(byte[] buffer, int offset, int size, SocketFlags socketFlags, CancellationToken cancellationToken)
        {
            int value = this.InputStream.Read(buffer, offset, size);
            return Task.FromResult(value);
        }

        public int Send(byte[] buffer, int offset, int size, SocketFlags socketFlags)
        {
            this.OutputStream.Write(buffer, offset, size);
            return size;
        }

        public byte[] GetBytesSent()
        {
            return this.OutputStream.ToArray();
        }

        public void Reconnect()
        {
            throw new NotImplementedException();
        }
    }
}
