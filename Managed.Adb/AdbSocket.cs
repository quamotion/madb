// <copyright file="AdbSocket.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion. All rights reserved.
// </copyright>

namespace Managed.Adb
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using Managed.Adb.Exceptions;

    public class AdbSocket : IAdbSocket, IDisposable
    {
        private Socket socket;

        public AdbSocket(IPEndPoint endPoint)
        {
            this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.socket.Connect(endPoint);
            this.socket.Blocking = true;
        }

        public bool Connected
        {
            get { return this.socket.Connected; }
        }

        public void Close()
        {
            this.socket.Close();
        }

        public virtual void Dispose()
        {
            this.socket.Dispose();
        }

        public virtual void Read(byte[] data)
        {
            if (!AdbHelper.Read(this.socket, data))
            {
                this.socket.Close();
                throw new AdbException($"An error occurred while reading {data.Length} bytes of data");
            }
        }

        public virtual int Read(byte[] data, int timeout)
        {
            int currentTimeout = this.socket.ReceiveTimeout;

            try
            {
                this.socket.ReceiveTimeout = timeout;
                return this.socket.Receive(data);
            }
            finally
            {
                this.socket.ReceiveTimeout = currentTimeout;
            }
        }

        public virtual void SendFileRequest(string command, string path, SyncService.FileMode mode)
        {
            byte[] commandContent = command.GetBytes(AdbHelper.Encoding);
            byte[] pathContent = path.GetBytes(AdbHelper.Encoding);

            byte[] request = SyncService.CreateSendFileRequest(commandContent, pathContent, mode);
            AdbHelper.Write(this.socket, request, -1, DdmPreferences.Timeout);
        }

        public virtual void SendSyncRequest(string command, int value)
        {
            var msg = SyncService.CreateRequest(command, value);
            AdbHelper.Write(this.socket, msg, -1, DdmPreferences.Timeout);
        }

        public virtual string ReadString()
        {
            // The first 4 bytes contain the length of the string
            var reply = new byte[4];
            this.Read(reply);

            // Convert the bytes to a hex string
            string lenHex = reply.GetString(AdbHelper.DefaultEncoding);
            int len = int.Parse(lenHex, NumberStyles.HexNumber);

            // And get the string
            reply = new byte[len];
            this.Read(reply);

            string value = reply.GetString(AdbHelper.DefaultEncoding);
            return value;
        }

        public virtual AdbResponse ReadAdbResponse(bool readDiagString)
        {
            var response = AdbHelper.ReadAdbResponse(this.socket, readDiagString);

            if (!response.IOSuccess || !response.Okay)
            {
                this.socket.Close();
                throw new AdbException($"An error occurred while reading a response from ADB: {response.Message}");
            }

            return response;
        }

        public virtual void SendAdbRequest(string request)
        {
            byte[] data = AdbHelper.FormAdbRequest(request);

            if (!AdbHelper.Write(this.socket, data))
            {
                throw new IOException($"Failed sending the request '{request}' to ADB");
            }
        }

        public virtual void Send(byte[] data, int length, int timeout)
        {
            AdbHelper.Write(this.socket, data, length, timeout);
        }

        public virtual void Read(byte[] data, int length, int timeout)
        {
            AdbHelper.Read(this.socket, data, length, timeout);
        }

        public Socket Socket
        {
            get;
        }
    }
}
