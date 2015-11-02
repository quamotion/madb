// <copyright file="AdbSocket.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion. All rights reserved.
// </copyright>

namespace Managed.Adb
{
    using Exceptions;
    using System;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// <para>
    /// Implements a client for the Android Debug Bridge client-server protocol. Using the client, you
    /// can send messages to and receive messages from the Android Debug Bridge.
    /// </para>
    /// <para>
    /// The <see cref="AdbSocket"/> class implements the raw messaging protocol; that is,
    /// sending and receiving messages. For interacting with the services the Android Debug
    /// Bridge exposes, use the <see cref="AdbClient"/>.
    /// </para>
    /// <para>
    /// For more information about the protocol that is implemented here, see chapter
    /// II Protocol Details, section 1. Client &lt;-&gt;Server protocol at
    /// <see href="https://android.googlesource.com/platform/system/core/+/master/adb/OVERVIEW.TXT"/>.
    /// </para>
    /// </summary>
    public class AdbSocket : IAdbSocket, IDisposable
    {
        /// <summary>
        /// The default time to wait in the milliseconds.
        /// </summary>
        private const int WaitTime = 5;

        /// <summary>
        /// Logging tag
        /// </summary>
        private const string TAG = nameof(AdbSocket);

        /// <summary>
        /// The underlying TCP socket that manages the connection with the ADB server.
        /// </summary>
        private Socket socket;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdbSocket"/> class.
        /// </summary>
        /// <param name="endPoint">
        /// The <see cref="IPEndPoint"/> at which the Android Debug Bridge is listening
        /// for clients.
        /// </param>
        public AdbSocket(IPEndPoint endPoint)
        {
            this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.socket.Connect(endPoint);
            this.socket.Blocking = true;
        }

        /// <include file='IAdbSocket.xml' path='/IAdbSocket/Connected/*'/>
        public bool Connected
        {
            get { return this.socket.Connected; }
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
            return AdbClient.Encoding.GetString(reply).Equals("OKAY");
        }

        /// <include file='IAdbSocket.xml' path='/IAdbSocket/Close/*'/>
        public void Close()
        {
            this.socket.Close();
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="AdbSocket"/>
        /// class.
        /// </summary>
        public virtual void Dispose()
        {
            this.socket.Dispose();
        }

        /// <include file='IAdbSocket.xml' path='/IAdbSocket/Read_byte/*'/>
        public virtual void Read(byte[] data)
        {
            this.Read(data, -1, DdmPreferences.Timeout);
        }

        /// <include file='IAdbSocket.xml' path='/IAdbSocket/AsyncRead_byte/*'/>
        public virtual Task ReadAsync(byte[] data)
        {
            return this.ReadAsync(data, -1);
        }

        /// <include file='IAdbSocket.xml' path='/IAdbSocket/Read_byte_int/*'/>
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

        /// <include file='IAdbSocket.xml' path='/IAdbSocket/SendFileRequest/*'/>
        public virtual void SendFileRequest(string command, string path, SyncService.FileMode mode)
        {
            byte[] commandContent = AdbClient.Encoding.GetBytes(command);
            byte[] pathContent = AdbClient.Encoding.GetBytes(path);

            byte[] request = SyncService.CreateSendFileRequest(commandContent, pathContent, mode);
            this.Send(request, -1, DdmPreferences.Timeout);
        }

        public virtual void SendSyncRequest(SyncCommand command, string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            // The message structure is:
            // First four bytes: command
            // Next four bytes: length of the path
            // Final four bytes: path
            byte[] commandBytes = SyncCommandConverter.GetBytes(command);

            byte[] lengthBytes = BitConverter.GetBytes(path.Length);

            if (!BitConverter.IsLittleEndian)
            {
                // Convert from big endian to little endian
                Array.Reverse(lengthBytes);
            }

            byte[] pathBytes = AdbClient.Encoding.GetBytes(path);

            this.Write(commandBytes);
            this.Write(lengthBytes);
            this.Write(pathBytes);
        }

        public virtual SyncCommand ReadSyncResponse()
        {
            byte[] data = new byte[4];
            this.Read(data);

            return SyncCommandConverter.GetCommand(data);
        }

        /// <include file='IAdbSocket.xml' path='/IAdbSocket/SendSyncRequest/*'/>
        public virtual void SendSyncRequest(string command, int value)
        {
            var msg = SyncService.CreateRequest(command, value);
            this.Send(msg, -1, DdmPreferences.Timeout);
        }

        /// <include file='IAdbSocket.xml' path='/IAdbSocket/ReadString/*'/>
        public virtual string ReadString()
        {
            // The first 4 bytes contain the length of the string
            var reply = new byte[4];
            this.Read(reply);

            // Convert the bytes to a hex string
            string lenHex = AdbClient.Encoding.GetString(reply);
            int len = int.Parse(lenHex, NumberStyles.HexNumber);

            // And get the string
            reply = new byte[len];
            this.Read(reply);

            string value = AdbClient.Encoding.GetString(reply);
            return value;
        }

        /// <include file='IAdbSocket.xml' path='/IAdbSocket/ReadStringAsync/*'/>
        public virtual async Task<string> ReadStringAsync()
        {
            // The first 4 bytes contain the length of the string
            var reply = new byte[4];
            await this.ReadAsync(reply);

            // Convert the bytes to a hex string
            string lenHex = AdbClient.Encoding.GetString(reply);
            int len = int.Parse(lenHex, NumberStyles.HexNumber);

            // And get the string
            reply = new byte[len];
            await this.ReadAsync(reply);

            string value = AdbClient.Encoding.GetString(reply);
            return value;
        }

        /// <include file='IAdbSocket.xml' path='/IAdbSocket/ReadAdbResponse/*'/>
        public virtual AdbResponse ReadAdbResponse(bool readDiagString)
        {
            var response = this.ReadAdbResponseInner(readDiagString);

            if (!response.IOSuccess || !response.Okay)
            {
                this.socket.Close();
                throw new AdbException($"An error occurred while reading a response from ADB: {response.Message}", response);
            }

            return response;
        }

        /// <include file='IAdbSocket.xml' path='/IAdbSocket/SendAdbRequest/*'/>
        public virtual void SendAdbRequest(string request)
        {
            byte[] data = AdbClient.FormAdbRequest(request);

            if (!this.Write(data))
            {
                throw new IOException($"Failed sending the request '{request}' to ADB");
            }
        }

        /// <include file='IAdbSocket.xml' path='/IAdbSocket/Send_byte_int_int/*'/>
        public virtual void Send(byte[] data, int length, int timeout)
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

        /// <include file='IAdbSocket.xml' path='/IAdbSocket/ReadAsync_byte_int/*'/>
        public virtual async Task<int> ReadAsync(byte[] data, int length)
        {
            int expLen = length != -1 ? length : data.Length;
            int count = -1;
            int totalRead = 0;

            while (count != 0 && totalRead < expLen)
            {
                try
                {
                    int left = expLen - totalRead;
                    int buflen = left < this.socket.ReceiveBufferSize ? left : this.socket.ReceiveBufferSize;

                    byte[] buffer = new byte[buflen];
                    this.socket.ReceiveBufferSize = expLen;
                    count = await this.socket.ReceiveAsync(buffer, 0, buflen, SocketFlags.None);

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

            return totalRead;
        }

        /// <include file='IAdbSocket.xml' path='/IAdbSocket/Read_byte_int_int/*'/>
        public void Read(byte[] data, int length, int timeout)
        {
            int expLen = length != -1 ? length : data.Length;
            int count = -1;
            int totalRead = 0;

            while (count != 0 && totalRead < expLen)
            {
                try
                {
                    int left = expLen - totalRead;
                    int buflen = left < this.socket.ReceiveBufferSize ? left : this.socket.ReceiveBufferSize;

                    byte[] buffer = new byte[buflen];
                    this.socket.ReceiveBufferSize = expLen;
                    count = this.socket.Receive(buffer, buflen, SocketFlags.None);
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
        /// Write until all data in "data" is written or the connection fails or times out.
        /// </summary>
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
                this.Send(data, -1, DdmPreferences.Timeout);
            }
            catch (IOException e)
            {
                Log.e(TAG, e);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Reads the response from ADB after a command.
        /// </summary>
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
            this.Read(reply);

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
                var message = this.ReadString();
                resp.Message = message;
                Log.e(TAG, "Got reply '{0}', diag='{1}'", this.ReplyToString(reply), resp.Message);

                break;
            }

            return resp;
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
