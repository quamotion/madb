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
    using System.Threading;

    public class AdbSocket : IAdbSocket, IDisposable
    {
        /// <summary>
        /// Logging tag
        /// </summary>
        private const string TAG = nameof(AdbSocket);

        private Socket socket;

        /// <summary>
        /// The default time to wait in the milliseconds.
        /// </summary>
        private const int WaitTime = 5;

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
            if (!this.ReadInner(data))
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
            this.Write(request, -1, DdmPreferences.Timeout);
        }

        public virtual void SendSyncRequest(string command, int value)
        {
            var msg = SyncService.CreateRequest(command, value);
            this.Write(msg, -1, DdmPreferences.Timeout);
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
            var response = this.ReadAdbResponseInner(readDiagString);

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

            if (!this.Write(data))
            {
                throw new IOException($"Failed sending the request '{request}' to ADB");
            }
        }

        public virtual void Send(byte[] data, int length, int timeout)
        {
            this.Write(data, length, timeout);
        }

        public virtual void Read(byte[] data, int length, int timeout)
        {
            this.Read(data, length, timeout);
        }

        public Socket Socket
        {
            get;
        }

        /// <summary>
        /// Write until all data in "data" is written or the connection fails or times out.
        /// </summary>
        /// <param name="socket">The socket to write to.</param>
        /// <param name="data">The data to send.</param>
        /// <returns>
        /// Returns <see langword="true"/> if all data was written; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// This uses the default time out value.
        /// </remarks>
        protected bool Write(byte[] data)
        {
            try
            {
                this.Write(data, -1, DdmPreferences.Timeout);
            }
            catch (IOException e)
            {
                Log.e(TAG, e);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Write until all data in <paramref name="data"/> is written, the optional <paramref name="length"/> is reached,
        /// the <paramref name="timeout"/> expires, or the connection fails.
        /// </summary>
        /// <param name="socket">The socket to write the data to.</param>
        /// <param name="data">The data to send.</param>
        /// <param name="length">The length to write or -1 to send the whole buffer.</param>
        /// <param name="timeout">The timeout value. A timeout of zero means "wait forever".</param>
        /// <exception cref="AdbException">
        /// channel EOF
        /// or
        /// timeout
        /// </exception>
        protected void Write(byte[] data, int length, int timeout)
        {
            int numWaits = 0;
            int count = -1;

            try
            {
                count = this.socket.Send(data, 0, length != -1 ? length : data.Length, SocketFlags.None);
                if (count < 0)
                {
                    throw new AdbException("channel EOF");
                }
                else if (count == 0)
                {
                    // TODO: need more accurate timeout?
                    if (timeout != 0 && numWaits * WaitTime > timeout)
                    {
                        throw new AdbException("timeout");
                    }

                    // non-blocking spin
                    Thread.Sleep(WaitTime);
                    numWaits++;
                }
                else
                {
                    numWaits = 0;
                }
            }
            catch (SocketException sex)
            {
                Log.e(TAG, sex);
                throw;
            }
        }

        /// <summary>
        /// Reads the response from ADB after a command.
        /// </summary>
        /// <param name="socket">The socket channel that is connected to adb.</param>
        /// <param name="readDiagString">
        /// if <see langword="true"/>, we're expecting an <c>OKAY</c> response to be
        /// followed by a diagnostic string. Otherwise, we only expect the
        /// diagnostic string to follow a <c>FAIL</c>.</param>
        /// <returns>
        /// A <see cref="AdbResponse"/> that represents the response received from ADB.
        /// </returns>
        protected AdbResponse ReadAdbResponseInner(bool readDiagString)
        {
            AdbResponse resp = new AdbResponse();

            byte[] reply = new byte[4];
            if (!this.ReadInner(reply))
            {
                return resp;
            }

            resp.IOSuccess = true;

            if (IsOkay(reply))
            {
                resp.Okay = true;
            }
            else
            {
                readDiagString = true; // look for a reason after the FAIL
                resp.Okay = false;
            }

            // not a loop -- use "while" so we can use "break"
            while (readDiagString)
            {
                // length string is in next 4 bytes
                byte[] lenBuf = new byte[4];
                if (!ReadInner(lenBuf))
                {
                    Log.w(TAG, "Expected diagnostic string not found");
                    break;
                }

                string lenStr = this.ReplyToString(lenBuf);

                int len;
                try
                {
                    len = int.Parse(lenStr, System.Globalization.NumberStyles.HexNumber);
                }
                catch (FormatException)
                {
                    Log.e(TAG, "Expected digits, got '{0}' : {1} {2} {3} {4}", lenBuf[0], lenBuf[1], lenBuf[2], lenBuf[3]);
                    Log.e(TAG, "reply was {0}", this.ReplyToString(reply));
                    break;
                }

                byte[] msg = new byte[len];
                if (!ReadInner(msg))
                {
                    Log.e(TAG, "Failed reading diagnostic string, len={0}", len);
                    break;
                }

                resp.Message = this.ReplyToString(msg);
                Log.e(TAG, "Got reply '{0}', diag='{1}'", this.ReplyToString(reply), resp.Message);

                break;
            }

            return resp;
        }

        /// <summary>
        /// Reads from the socket until the array is filled, or no more data is coming (because
        /// the socket closed or the timeout expired).
        /// </summary>
        /// <param name="socket">
        /// The opened socket to read from. It must be in non-blocking mode for timeouts to work.
        /// </param>
        /// <param name="data">
        /// The buffer to store the read data into.</param>
        /// <returns>
        /// <see langword="true"/> if the data was read successfully; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// This uses the default time out value.
        /// </remarks>
        protected bool ReadInner(byte[] data)
        {
            try
            {
                Read(this.socket, data, -1, DdmPreferences.Timeout);
            }
            catch (AdbException e)
            {
                Log.e(TAG, e);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Reads from the socket until the array is filled, the optional <paramref name="length"/>
        /// is reached, or no more data is coming (because the socket closed or the
        /// timeout expired). After <paramref name="timeout"/> milliseconds since the
        /// previous successful read, this will return whether or not new data has
        /// been found.
        /// </summary>
        /// <param name="socket">
        /// The opened socket to read from. It must be in non-blocking
        /// mode for timeouts to work
        /// </param>
        /// <param name="data">
        /// The buffer to store the read data into.
        /// </param>
        /// <param name="length">
        /// The length to read or -1 to fill the data buffer completely
        /// </param>
        /// <param name="timeout">
        /// The timeout value in ms. A timeout of zero means "wait forever".
        /// </param>
        /// <exception cref="AdbException">
        /// EOF
        /// or
        /// No Data to read: exception.Message
        /// </exception>
        public static void Read(Socket socket, byte[] data, int length, int timeout)
        {
            int expLen = length != -1 ? length : data.Length;
            int count = -1;
            int totalRead = 0;

            while (count != 0 && totalRead < expLen)
            {
                try
                {
                    int left = expLen - totalRead;
                    int buflen = left < socket.ReceiveBufferSize ? left : socket.ReceiveBufferSize;

                    byte[] buffer = new byte[buflen];
                    socket.ReceiveBufferSize = expLen;
                    count = socket.Receive(buffer, buflen, SocketFlags.None);
                    if (count < 0)
                    {
                        Log.e(TAG, "read: channel EOF");
                        throw new AdbException("EOF");
                    }
                    else if (count == 0)
                    {
                        Log.i(TAG, "DONE with Read");
                    }
                    else
                    {
                        Array.Copy(buffer, 0, data, totalRead, count);
                        totalRead += count;
                    }
                }
                catch (SocketException sex)
                {
                    throw new AdbException(string.Format("No Data to read: {0}", sex.Message));
                }
            }
        }

        /// <summary>
        /// Determines whether the specified reply is okay.
        /// </summary>
        /// <param name="reply">The reply.</param>
        /// <returns>
        ///   <see langword="true"/> if the specified reply is okay; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool IsOkay(byte[] reply)
        {
            return reply.GetString().Equals("OKAY");
        }

        /// <summary>
        /// Converts an ADB reply to a string.
        /// </summary>
        /// <param name="reply">
        /// A <see cref="byte"/> array that represents the ADB reply.
        /// </param>
        /// <returns>
        /// A <see cref="string"/> that represents the ADB reply.
        /// </returns>
        protected string ReplyToString(byte[] reply)
        {
            string result;
            try
            {
                result = Encoding.Default.GetString(reply);
            }
            catch (DecoderFallbackException uee)
            {
                Log.e(TAG, uee);
                result = string.Empty;
            }

            return result;
        }
    }
}
