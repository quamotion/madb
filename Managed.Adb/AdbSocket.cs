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

        public void Dispose()
        {
            this.socket.Dispose();
        }

        public void Read(byte[] data)
        {
            if (!AdbHelper.Read(this.socket, data))
            {
                this.socket.Close();
                throw new AdbException($"An error occurred while reading {data.Length} bytes of data");
            }
        }

        public string ReadString()
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

        public AdbResponse ReadAdbResponse(bool readDiagString)
        {
            var response = AdbHelper.ReadAdbResponse(this.socket, readDiagString);

            if (!response.IOSuccess || !response.Okay)
            {
                this.socket.Close();
                throw new AdbException($"An error occurred while reading a response from ADB: {response.Message}");
            }

            return response;
        }

        public void SendAdbRequest(string request)
        {
            byte[] data = AdbHelper.FormAdbRequest(request);

            if (!AdbHelper.Write(this.socket, data))
            {
                throw new IOException($"Failed sending the request '{request}' to ADB");
            }
        }
    }
}
