// <copyright file="AdbHelper.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion. All rights reserved.
// </copyright>

namespace Managed.Adb
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;
    using Managed.Adb.Exceptions;
    using Managed.Adb.Logs;
    using MoreLinq;
    using System.Diagnostics;

    /// <summary>
    /// <para>
    /// The Android Debug Bridge Helper class, to handle requests and connections to adb.
    /// </para>
    /// <para>
    /// <seealso cref="AndroidDebugBridge"/> is the public API to connection to adb, while <see cref="AdbHelper"/>
    /// does the low level stuff.
    /// </para>
    /// </summary>
    /// <seealso href="https://github.com/android/platform_system_core/blob/master/adb/SERVICES.TXT">SERVICES.TXT</seealso>
    /// <seealso href="https://github.com/android/platform_system_core/blob/master/adb/adb_client.c">adb_client.c</seealso>
    /// <seealso href="https://github.com/android/platform_system_core/blob/master/adb/adb.c">adb.c</seealso>
    public class AdbHelper
    {
        /// <summary>
        /// The default encoding
        /// </summary>
        public const string DefaultEncoding = "ISO-8859-1";

        /// <summary>
        /// Logging tag
        /// </summary>
        private const string TAG = "AdbHelper";

        /// <summary>
        /// The default time to wait in the milliseconds.
        /// </summary>
        private const int WaitTime = 5;

        /// <summary>
        /// The default port to use when connecting to a device over TCP/IP.
        /// </summary>
        private const int DefaultPort = 5555;

        /// <summary>
        /// The singleton instance of the <see cref="AdbHelper"/> class.
        /// </summary>
        private static AdbHelper instance = null;

        /// <summary>
        /// Prevents a default instance of the <see cref="AdbHelper"/> class from being created.
        /// </summary>
        private AdbHelper()
        {
        }

        /// <summary>
        /// Gets the encoding used when communicating with adb.
        /// </summary>
        public static Encoding Encoding
        { get; } = Encoding.GetEncoding(DefaultEncoding);

        /// <summary>
        /// Gets an instance of the AdbHelper.
        /// </summary>
        public static AdbHelper Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new AdbHelper();
                }

                return instance;
            }
        }

        /// <summary>
        /// Create an ASCII string preceded by four hex digits. The opening "####"
        /// is the length of the rest of the string, encoded as ASCII hex(case
        /// doesn't matter).
        /// </summary>
        /// <param name="req">The request to form.
        /// </param>
        /// <returns>
        /// An array containing <c>####req</c>.
        /// </returns>
        public static byte[] FormAdbRequest(string req)
        {
            string resultStr = string.Format("{0}{1}\n", req.Length.ToString("X4"), req);
            byte[] result = Encoding.GetBytes(resultStr);
            return result;
        }

        /// <summary>
        /// Opens the specified address on the device on the specified port.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="device">The device.</param>
        /// <param name="port">The port.</param>
        /// <returns>The open socket</returns>
        /// <exception cref="Managed.Adb.Exceptions.AdbException">
        /// failed submitting request to ADB
        /// or
        /// connection request rejected
        /// </exception>
        public Socket Open(IPAddress address, IDevice device, int port)
        {
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                s.Connect(address, port);
                s.Blocking = true;
                s.NoDelay = false;

                this.SetDevice(s, device);

                byte[] req = this.CreateAdbForwardRequest(null, port);
                if (!this.Write(s, req))
                {
                    throw new AdbException("failed submitting request to ADB");
                }

                AdbResponse resp = this.ReadAdbResponse(s, false);
                if (!resp.Okay)
                {
                    throw new AdbException("connection request rejected");
                }

                s.Blocking = true;
            }
            catch (AdbException)
            {
                s.Close();
                throw;
            }

            return s;
        }

        /// <summary>
        /// Kills the running adb server.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <returns>0 for success; -1 for failure.</returns>
        /// <exception cref="System.IO.IOException">failed asking to kill adb</exception>
        /// <gist id="cbacc7b384ec7a4c27f7" />
        public int KillAdb(IPEndPoint address)
        {
            byte[] request = FormAdbRequest("host:kill");
            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                socket.Connect(address);
                socket.Blocking = true;
                if (!this.Write(socket, request))
                {
                    throw new IOException("failed asking to kill adb");
                }

                var resp = this.ReadAdbResponse(socket, false);
                if (!resp.IOSuccess || !resp.Okay)
                {
                    Log.e(TAG, "Got timeout or unhappy response from ADB req: " + resp.Message);
                    socket.Close();
                    return -1;
                }

                return 0;
            }
        }

        // https://github.com/android/platform_system_core/blob/master/adb/backup_service.c

        /// <summary>
        /// Backups the specified address.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <exception cref="System.IO.IOException">failed asking to backup device</exception>
        /// <seealso href="https://github.com/android/platform_system_core/blob/master/adb/backup_service.c">backup_service.c</seealso>
        [Obsolete("This is not yet functional")]
        public void Backup(IPEndPoint address)
        {
            byte[] request = FormAdbRequest("backup:all");
            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                socket.Connect(address);
                socket.Blocking = true;
                if (!this.Write(socket, request))
                {
                    throw new IOException("failed asking to backup device");
                }

                var resp = this.ReadAdbResponse(socket, false);
                if (!resp.IOSuccess || !resp.Okay)
                {
                    Log.e(TAG, "Got timeout or unhappy response from ADB req: " + resp.Message);
                    socket.Close();
                    return;
                }

                var data = new byte[6000];
                int count = -1;
                while (count != 0)
                {
                    count = socket.Receive(data);
                    Console.Write("received: {0}", count);
                }
            }
        }

        // https://github.com/android/platform_system_core/blob/master/adb/backup_service.c

        /// <summary>
        /// Restores this instance.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        /// <seealso href="https://github.com/android/platform_system_core/blob/master/adb/backup_service.c">backup_service.c</seealso>
        [Obsolete("This is not yet functional")]
        public void Restore()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the adb version of the server.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <returns>The version number; or -1 if failure.</returns>
        /// <exception cref="System.IO.IOException">failed asking for adb version</exception>
        /// <gist id="3a130af63ca1f94d0152" />
        public int GetAdbVersion(IPEndPoint address)
        {
            byte[] request = FormAdbRequest("host:version");
            byte[] reply;
            Socket adbChan = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                adbChan.Connect(address);
                adbChan.Blocking = true;
                if (!this.Write(adbChan, request))
                {
                    throw new IOException("failed asking for adb version");
                }

                AdbResponse resp = this.ReadAdbResponse(adbChan, false /* readDiagString */);
                if (!resp.IOSuccess || !resp.Okay)
                {
                    Log.e(TAG, "Got timeout or unhappy response from ADB fb req: " + resp.Message);
                    adbChan.Close();
                    return -1;
                }

                reply = new byte[4];
                if (!this.Read(adbChan, reply))
                {
                    Log.e(TAG, "error in getting data length");

                    adbChan.Close();
                    return -1;
                }

                string lenHex = reply.GetString(AdbHelper.DefaultEncoding);
                int len = int.Parse(lenHex, System.Globalization.NumberStyles.HexNumber);

                // the protocol version.
                reply = new byte[len];
                if (!this.Read(adbChan, reply))
                {
                    Log.e(TAG, "did not get the version info");

                    adbChan.Close();
                    return -1;
                }

                string sReply = reply.GetString(AdbHelper.DefaultEncoding);
                return int.Parse(sReply, System.Globalization.NumberStyles.HexNumber);
            }
            catch (Exception ex)
            {
                Log.e(TAG, ex);
                throw;
            }
        }

        /// <summary>
        /// Creates and connects a new pass-through socket, from the host to a port on the device.
        /// </summary>
        /// <param name="endpoint">The endpoint.</param>
        /// <param name="device">the device to connect to. Can be null in which case the connection will be
        /// to the first available device.</param>
        /// <param name="pid">the process pid to connect to.</param>
        /// <returns>
        /// The Socket
        /// </returns>
        /// <exception cref="Managed.Adb.Exceptions.AdbException">
        /// failed submitting request to ADB
        /// or
        /// connection request rejected:  + resp.Message
        /// </exception>
        public Socket CreatePassThroughConnection(IPEndPoint endpoint, IDevice device, int pid)
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                socket.Connect(endpoint);
                socket.NoDelay = true;

                // if the device is not -1, then we first tell adb we're looking to
                // talk to a specific device
                this.SetDevice(socket, device);

                byte[] req = this.CreateJdwpForwardRequest(pid);

                if (!this.Write(socket, req))
                {
                    throw new AdbException("failed submitting request to ADB");
                }

                AdbResponse resp = this.ReadAdbResponse(socket, false /* readDiagString */);
                if (!resp.Okay)
                {
                    throw new AdbException("connection request rejected: " + resp.Message);
                }
            }
            catch (AdbException ioe)
            {
                socket.Close();
                throw ioe;
            }

            return socket;
        }

        /// <summary>
        /// Creates the adb forward request.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="port">The port.</param>
        /// <returns>
        /// This returns an array containing <c>"####tcp:{port}:{addStr}"</c>.
        /// </returns>
        public byte[] CreateAdbForwardRequest(string address, int port)
        {
            string request;

            if (address == null)
            {
                request = "tcp:" + port;
            }
            else
            {
                request = "tcp:" + port + ":" + address;
            }

            return FormAdbRequest(request);
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
        public bool Write(Socket socket, byte[] data)
        {
            try
            {
                this.Write(socket, data, -1, DdmPreferences.Timeout);
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
        public void Write(Socket socket, byte[] data, int length, int timeout)
        {
            int numWaits = 0;
            int count = -1;

            try
            {
                count = socket.Send(data, 0, length != -1 ? length : data.Length, SocketFlags.None);
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
        public AdbResponse ReadAdbResponse(Socket socket, bool readDiagString)
        {
            AdbResponse resp = new AdbResponse();

            byte[] reply = new byte[4];
            if (!this.Read(socket, reply))
            {
                return resp;
            }

            resp.IOSuccess = true;

            if (this.IsOkay(reply))
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
                if (!this.Read(socket, lenBuf))
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
                if (!this.Read(socket, msg))
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
        public bool Read(Socket socket, byte[] data)
        {
            try
            {
                this.Read(socket, data, -1, DdmPreferences.Timeout);
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
        public void Read(Socket socket, byte[] data, int length, int timeout)
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
        ///  Creates a port forwarding between a local and a remote port.
        /// </summary>
        /// <param name="adbSockAddr">
        /// The socket address to connect to adb
        /// </param>
        /// <param name="device">
        /// The device on which to do the port forwarding
        /// </param>
        /// <param name="localPort">
        /// The local port to forward.
        /// </param>
        /// <param name="remotePort">
        /// The remote port to forward to
        /// </param>
        /// <exception cref="Managed.Adb.Exceptions.AdbException">
        /// failed to submit the forward command.
        /// or
        /// Device rejected command:  + resp.Message
        /// </exception>
        public void CreateForward(IPEndPoint adbSockAddr, IDevice device, int localPort, int remotePort)
        {
            Socket adbChan = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                adbChan.Connect(adbSockAddr);
                adbChan.Blocking = true;

                // host-serial should be different based on the transport...
                byte[] request = FormAdbRequest($"host-serial:{device.SerialNumber}:forward:tcp:{localPort};tcp:{remotePort}");

                if (!this.Write(adbChan, request))
                {
                    throw new AdbException("failed to submit the forward command.");
                }

                AdbResponse resp = this.ReadAdbResponse(adbChan, false /* readDiagString */);
                if (!resp.IOSuccess || !resp.Okay)
                {
                    throw new AdbException("Device rejected command: " + resp.Message);
                }
            }
            finally
            {
                if (adbChan != null)
                {
                    adbChan.Close();
                }
            }
        }

        /// <summary>
        /// Forwards a remote Unix socket to a local TCP socket.
        /// </summary>
        /// <param name="adbSockAddr">The adb socket address.</param>
        /// <param name="device">The device.</param>
        /// <param name="localPort">The local port.</param>
        /// <param name="remoteSocket">The remote Unix socket.</param>
        /// <returns>
        /// This method always returns <see langword="true"/>.
        /// </returns>
        /// <exception cref="Managed.Adb.Exceptions.AdbException">
        /// The client failed to submit the forward command.
        /// </exception>
        /// <exception cref="Managed.Adb.Exceptions.AdbException">
        /// The device rejected command. The error message will include the error message provided by the device.
        /// </exception>
        public bool CreateForward(IPEndPoint adbSockAddr, IDevice device, int localPort, string remoteSocket)
        {
            Socket adbChan = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                adbChan.Connect(adbSockAddr);
                adbChan.Blocking = true;

                // host-serial should be different based on the transport...
                byte[] request = FormAdbRequest(
                    string.Format(
                        "host-serial:{0}:forward:tcp:{1};localabstract:{2}", // $NON-NLS-1$
                        device.SerialNumber,
                        localPort,
                        remoteSocket));

                if (!this.Write(adbChan, request))
                {
                    throw new AdbException("failed to submit the forward command.");
                }

                AdbResponse resp = this.ReadAdbResponse(adbChan, false /* readDiagString */);
                if (!resp.IOSuccess || !resp.Okay)
                {
                    throw new AdbException("Device rejected command: " + resp.Message);
                }
            }
            finally
            {
                if (adbChan != null)
                {
                    adbChan.Close();
                }
            }

            return true;
        }

        /// <summary>
        /// Lists the forward.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="device">The device.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void ListForward(IPEndPoint address, IDevice device)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Remove a port forwarding between a local and a remote port.
        /// </summary>
        /// <param name="address">
        /// The socket address to connect to adb
        /// </param>
        /// <param name="device">
        /// The device on which to remove the port forwarding
        /// </param>
        /// <param name="localPort">
        /// Specification of the local port that was forwarded
        /// </param>
        public void RemoveForward(IPEndPoint address, IDevice device, int localPort)
        {
            using (var socket = this.ExecuteRawSocketCommand(address, device, "host-serial:{0}:killforward:tcp:{1}".With(device.SerialNumber, localPort)))
            {
            }
        }

        /// <summary>
        /// Removes all forwards for a given device.
        /// </summary>
        /// <param name="address">
        /// The socket address to connect to adb
        /// </param>
        /// <param name="device">
        /// The device on which to remove the port forwarding
        /// </param>
        public void RemoveAllForward(IPEndPoint address, IDevice device)
        {
            using (var socket = this.ExecuteRawSocketCommand(address, device, "host-serial:{0}:killforward-all".With(device.SerialNumber)))
            {
            }
        }

        /// <summary>
        /// Determines whether the specified reply is okay.
        /// </summary>
        /// <param name="reply">The reply.</param>
        /// <returns>
        ///   <see langword="true"/> if the specified reply is okay; otherwise, <see langword="false"/>.
        /// </returns>
        public bool IsOkay(byte[] reply)
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
        public string ReplyToString(byte[] reply)
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

        /// <summary>
        /// Gets the devices that are available for communication.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <returns>A list of devices that are connected.</returns>
        /// <gist id="a8acf10d48370d138247" />
        public List<Device> GetDevices(IPEndPoint address)
        {
            // -l will return additional data
            using (var socket = this.ExecuteRawSocketCommand(address, "host:devices-l"))
            {
                byte[] reply = new byte[4];

                if (!this.Read(socket, reply))
                {
                    Log.e(TAG, "error in getting data length");
                    return null;
                }

                string lenHex = reply.GetString(Encoding.Default);
                int len = int.Parse(lenHex, System.Globalization.NumberStyles.HexNumber);

                reply = new byte[len];
                if (!this.Read(socket, reply))
                {
                    Log.e(TAG, "error in getting data");
                    return null;
                }

                List<Device> s = new List<Device>();
                string[] data = reply.GetString(Encoding.Default).Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                data.ForEach(item =>
                {
                    var device = Device.CreateFromAdbData(item);
                    s.Add(device);
                });

                return s;
            }
        }

        /// <summary>
        /// Gets the frame buffer from the specified end point.
        /// </summary>
        /// <param name="adbSockAddr">The adb sock addr.</param>
        /// <param name="device">The device.</param>
        /// <returns>Returns the RawImage.</returns>
        /// <exception cref="Managed.Adb.Exceptions.AdbException">
        /// failed asking for frame buffer
        /// or
        /// failed nudging
        /// </exception>
        public RawImage GetFrameBuffer(IPEndPoint adbSockAddr, IDevice device)
        {
            RawImage imageParams = new RawImage();
            byte[] request = FormAdbRequest("framebuffer:");
            byte[] nudge =
            {
                        0
                };
            byte[] reply;

            Socket adbChan = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                adbChan.Connect(adbSockAddr);
                adbChan.Blocking = true;

                // if the device is not -1, then we first tell adb we're looking to talk
                // to a specific device
                this.SetDevice(adbChan, device);
                if (!this.Write(adbChan, request))
                {
                    throw new AdbException("failed asking for frame buffer");
                }

                AdbResponse resp = this.ReadAdbResponse(adbChan, false /* readDiagString */);
                if (!resp.IOSuccess || !resp.Okay)
                {
                    Log.w(TAG, "Got timeout or unhappy response from ADB fb req: " + resp.Message);
                    adbChan.Close();
                    return null;
                }

                // first the protocol version.
                reply = new byte[4];
                if (!this.Read(adbChan, reply))
                {
                    Log.w(TAG, "got partial reply from ADB fb:");

                    adbChan.Close();
                    return null;
                }

                BinaryReader buf;
                int version = 0;
                using (MemoryStream ms = new MemoryStream(reply))
                {
                    buf = new BinaryReader(ms);
                    version = buf.ReadInt16();
                }

                // get the header size (this is a count of int)
                int headerSize = RawImage.GetHeaderSize(version);

                // read the header
                reply = new byte[headerSize * 4];
                if (!this.Read(adbChan, reply))
                {
                    Log.w(TAG, "got partial reply from ADB fb:");

                    adbChan.Close();
                    return null;
                }

                using (MemoryStream ms = new MemoryStream(reply))
                {
                    buf = new BinaryReader(ms);

                    // fill the RawImage with the header
                    if (imageParams.ReadHeader(version, buf) == false)
                    {
                        Log.w(TAG, "Unsupported protocol: " + version);
                        return null;
                    }
                }

                Log.d(TAG, $"image params: bpp={imageParams.Bpp}, size={imageParams.Size}, width={imageParams.Width}, height={imageParams.Height}");

                if (!this.Write(adbChan, nudge))
                {
                    throw new AdbException("failed nudging");
                }

                reply = new byte[imageParams.Size];
                if (!this.Read(adbChan, reply))
                {
                    Log.w(TAG, "got truncated reply from ADB fb data");
                    adbChan.Close();
                    return null;
                }

                imageParams.Data = reply;
            }
            finally
            {
                if (adbChan != null)
                {
                    adbChan.Close();
                }
            }

            return imageParams;
        }

        /// <summary>
        /// Executes a shell command on the remote device
        /// </summary>
        /// <param name="endPoint">The end point.</param>
        /// <param name="command">The command.</param>
        /// <param name="device">The device.</param>
        /// <param name="rcvr">The RCVR.</param>
        /// <remarks>
        /// Should check if you CanSU before calling this.
        /// </remarks>
        public void ExecuteRemoteRootCommand(IPEndPoint endPoint, string command, IDevice device, IShellOutputReceiver rcvr)
        {
            this.ExecuteRemoteRootCommand(endPoint, string.Format("su -c \"{0}\"", command), device, rcvr, int.MaxValue);
        }

        /// <summary>
        /// Executes a shell command on the remote device
        /// </summary>
        /// <param name="endPoint">The end point.</param>
        /// <param name="command">The command.</param>
        /// <param name="device">The device.</param>
        /// <param name="rcvr">The RCVR.</param>
        /// <param name="maxTimeToOutputResponse">The max time to output response.</param>
        public void ExecuteRemoteRootCommand(IPEndPoint endPoint, string command, IDevice device, IShellOutputReceiver rcvr, int maxTimeToOutputResponse)
        {
            this.ExecuteRemoteCommand(endPoint, string.Format("su -c \"{0}\"", command), device, rcvr);
        }

        /// <summary>
        /// Executes the remote command.
        /// </summary>
        /// <param name="endPoint">The end point.</param>
        /// <param name="command">The command.</param>
        /// <param name="device">The device.</param>
        /// <param name="rcvr">The RCVR.</param>
        /// <param name="maxTimeToOutputResponse">The max time to output response.</param>
        /// <exception cref="System.OperationCanceledException"></exception>
        /// <exception cref="System.IO.FileNotFoundException">
        /// </exception>
        /// <exception cref="Managed.Adb.Exceptions.UnknownOptionException"></exception>
        /// <exception cref="Managed.Adb.Exceptions.CommandAbortingException"></exception>
        /// <exception cref="Managed.Adb.Exceptions.PermissionDeniedException"></exception>
        /// <exception cref="Managed.Adb.Exceptions.ShellCommandUnresponsiveException"></exception>
        /// <exception cref="AdbException">failed submitting shell command</exception>
        /// <exception cref="UnknownOptionException"></exception>
        /// <exception cref="CommandAbortingException"></exception>
        /// <exception cref="PermissionDeniedException"></exception>
        public void ExecuteRemoteCommand(IPEndPoint endPoint, string command, IDevice device, IShellOutputReceiver rcvr, int maxTimeToOutputResponse)
        {
            using (var socket = this.ExecuteRawSocketCommand(endPoint, device, "shell:{0}".With(command)))
            {
                socket.ReceiveTimeout = maxTimeToOutputResponse;
                socket.SendTimeout = maxTimeToOutputResponse;

                try
                {
                    byte[] data = new byte[16384];
                    int count = -1;
                    while (count != 0)
                    {
                        if (rcvr != null && rcvr.IsCancelled)
                        {
                            Log.w(TAG, "execute: cancelled");
                            throw new OperationCanceledException();
                        }

                        count = socket.Receive(data);
                        if (count == 0)
                        {
                            // we're at the end, we flush the output
                            rcvr.Flush();
                            Log.w(TAG, "execute '" + command + "' on '" + device + "' : EOF hit. Read: " + count);
                        }
                        else
                        {
                            // Attempt to detect error messages and throw an exception based on them. The caller can override
                            // this behavior by specifying a receiver that has the ParsesErrors flag set to true; in this case,
                            // the receiver is responsible for all error handling.
                            if (rcvr == null || !rcvr.ParsesErrors)
                            {
                                string[] cmd = command.Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                                string sdata = data.GetString(0, count, AdbHelper.DefaultEncoding);

                                var sdataTrimmed = sdata.Trim();
                                if (sdataTrimmed.EndsWith(string.Format("{0}: not found", cmd[0])))
                                {
                                    Log.w(TAG, "The remote execution returned: '{0}: not found'", cmd[0]);
                                    throw new FileNotFoundException(string.Format("The remote execution returned: '{0}: not found'", cmd[0]));
                                }

                                if (sdataTrimmed.EndsWith("No such file or directory"))
                                {
                                    Log.w(TAG, "The remote execution returned: {0}", sdataTrimmed);
                                    throw new FileNotFoundException(string.Format("The remote execution returned: {0}", sdataTrimmed));
                                }

                                // for "unknown options"
                                if (sdataTrimmed.Contains("Unknown option"))
                                {
                                    Log.w(TAG, "The remote execution returned: {0}", sdataTrimmed);
                                    throw new UnknownOptionException(sdataTrimmed);
                                }

                                // for "aborting" commands
                                if (sdataTrimmed.IsMatch("Aborting.$"))
                                {
                                    Log.w(TAG, "The remote execution returned: {0}", sdataTrimmed);
                                    throw new CommandAbortingException(sdataTrimmed);
                                }

                                // for busybox applets
                                // cmd: applet not found
                                if (sdataTrimmed.IsMatch("applet not found$") && cmd.Length > 1)
                                {
                                    Log.w(TAG, "The remote execution returned: '{0}'", sdataTrimmed);
                                    throw new FileNotFoundException(string.Format("The remote execution returned: '{0}'", sdataTrimmed));
                                }

                                // checks if the permission to execute the command was denied.
                                // workitem: 16822
                                if (sdataTrimmed.IsMatch("(permission|access) denied$"))
                                {
                                    Log.w(TAG, "The remote execution returned: '{0}'", sdataTrimmed);
                                    throw new PermissionDeniedException(string.Format("The remote execution returned: '{0}'", sdataTrimmed));
                                }
                            }

                            // Add the data to the receiver
                            if (rcvr != null)
                            {
                                rcvr.AddOutput(data, 0, count);
                            }
                        }
                    }
                }
                catch (SocketException)
                {
                    throw new ShellCommandUnresponsiveException();
                }
                finally
                {
                    rcvr.Flush();
                }
            }
        }

        /// <summary>
        /// Executes a shell command on the remote device
        /// </summary>
        /// <param name="endPoint">The socket end point</param>
        /// <param name="command">The command to execute</param>
        /// <param name="device">The device to execute on</param>
        /// <param name="rcvr">The shell output receiver</param>
        /// <exception cref="FileNotFoundException">Throws if the result is 'command': not found</exception>
        /// <exception cref="IOException">Throws if there is a problem reading / writing to the socket</exception>
        /// <exception cref="OperationCanceledException">Throws if the execution was canceled</exception>
        /// <exception cref="EndOfStreamException">Throws if the Socket.Receice ever returns -1</exception>
        public void ExecuteRemoteCommand(IPEndPoint endPoint, string command, IDevice device, IShellOutputReceiver rcvr)
        {
            this.ExecuteRemoteCommand(endPoint, command, device, rcvr, int.MaxValue);
        }

        /// <summary>
        /// Sets the device.
        /// </summary>
        /// <param name="adbChan">The adb chan.</param>
        /// <param name="device">The device.</param>
        /// <exception cref="Managed.Adb.Exceptions.AdbException">
        /// failed submitting device ( + device + ) request to ADB
        /// or
        /// device ( + device + ) request rejected:  + resp.Message
        /// </exception>
        /// <exception cref="Managed.Adb.Exceptions.DeviceNotFoundException"></exception>
        public void SetDevice(Socket adbChan, IDevice device)
        {
            // if the device is not null, then we first tell adb we're looking to talk
            // to a specific device
            if (device != null)
            {
                string msg = "host:transport:" + device.SerialNumber;
                byte[] device_query = FormAdbRequest(msg);

                if (!this.Write(adbChan, device_query))
                {
                    throw new AdbException("failed submitting device (" + device + ") request to ADB");
                }

                AdbResponse resp = this.ReadAdbResponse(adbChan, false /* readDiagString */);
                if (!resp.Okay)
                {
                    if (string.Compare("device not found", resp.Message, true) == 0)
                    {
                        throw new DeviceNotFoundException(device.SerialNumber);
                    }
                    else
                    {
                        throw new AdbException("device (" + device + ") request rejected: " + resp.Message);
                    }
                }
            }
        }

        /// <summary>
        /// Runs the Event log service on the Device, and provides its output to the LogReceiver.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="device">The device.</param>
        /// <param name="rcvr">The RCVR.</param>
        public void RunEventLogService(IPEndPoint address, IDevice device, LogReceiver rcvr)
        {
            this.RunLogService(address, device, "events", rcvr);
        }

        /// <summary>
        /// Runs the Event log service on the Device, and provides its output to the LogReceiver.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="device">The device.</param>
        /// <param name="logName">Name of the log.</param>
        /// <param name="rcvr">The RCVR.</param>
        /// <exception cref="AdbException">failed asking for log</exception>
        public void RunLogService(IPEndPoint address, IDevice device, string logName, LogReceiver rcvr)
        {
            using (var socket = this.ExecuteRawSocketCommand(address, device, "log:{0}".With(logName)))
            {
                byte[] data = new byte[16384];
                using (var ms = new MemoryStream(data))
                {
                    int offset = 0;

                    while (true)
                    {
                        int count;
                        if (rcvr != null && rcvr.IsCancelled)
                        {
                            break;
                        }

                        var buffer = new byte[4 * 1024];

                        count = socket.Receive(buffer);
                        if (count < 0)
                        {
                            break;
                        }
                        else if (count == 0)
                        {
                            try
                            {
                                Thread.Sleep(WaitTime * 5);
                            }
                            catch (ThreadInterruptedException)
                            {
                            }
                        }
                        else
                        {
                            ms.Write(buffer, offset, count);
                            offset += count;
                            if (rcvr != null)
                            {
                                var d = ms.ToArray();
                                rcvr.ParseNewData(d, 0, d.Length);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Reboots the specified adb socket address.
        /// </summary>
        /// <param name="adbSocketAddress">The adb socket address.</param>
        /// <param name="device">The device.</param>
        public void Reboot(IPEndPoint adbSocketAddress, IDevice device)
        {
            this.Reboot(string.Empty, adbSocketAddress, device);
        }

        /// <summary>
        /// Reboots the specified device in to the specified mode.
        /// </summary>
        /// <param name="into">The into.</param>
        /// <param name="adbSockAddr">The adb sock addr.</param>
        /// <param name="device">The device.</param>
        public void Reboot(string into, IPEndPoint adbSockAddr, IDevice device)
        {
            byte[] request;
            if (string.IsNullOrEmpty(into))
            {
                request = FormAdbRequest("reboot:");
            }
            else
            {
                request = FormAdbRequest("reboot:" + into);
            }

            using (this.ExecuteRawSocketCommand(adbSockAddr, device, request))
            {
                // nothing to do...
            }
        }

        /// <summary>
        /// Connect to a device via TCP/IP.
        /// </summary>
        /// <param name="adbEndpoint">
        /// The socket where the <c>adb</c> server is listening.
        /// </param>
        /// <param name="address">
        /// The IP address of the remote device.
        /// </param>
        /// <returns>
        /// <c>0</c> if the operation completed successfully; otherwise,
        /// <c>-1</c>.
        /// </returns>
        public int Connect(IPEndPoint adbEndpoint, IPAddress address)
        {
            if (address == null)
            {
                throw new ArgumentNullException("address");
            }

            return this.Connect(adbEndpoint, new IPEndPoint(address, DefaultPort));
        }

        /// <summary>
        /// Connect to a device via TCP/IP.
        /// </summary>
        /// <param name="adbEndpoint">
        /// The socket where the <c>adb</c> server is listening.
        /// </param>
        /// <param name="host">
        /// The host address of the remote device.
        /// </param>
        /// <returns>
        /// <c>0</c> if the operation completed successfully; otherwise,
        /// <c>-1</c>.
        /// </returns>
        public int Connect(IPEndPoint adbEndpoint, string host)
        {
            if (string.IsNullOrEmpty(host))
            {
                throw new ArgumentNullException("host");
            }

            return this.Connect(adbEndpoint, new DnsEndPoint(host, DefaultPort));
        }

        /// <summary>
        /// Connect to a device via TCP/IP.
        /// </summary>
        /// <param name="adbEndpoint">
        /// The socket where the <c>adb</c> server is listening.
        /// </param>
        /// <param name="endpoint">
        /// The IP endpoint at which the <c>adb</c> server on the device is running.
        /// </param>
        /// <returns>
        /// <c>0</c> if the operation completed successfully; otherwise,
        /// <c>-1</c>.
        /// </returns>
        public int Connect(IPEndPoint adbEndpoint, IPEndPoint endpoint)
        {
            if (endpoint == null)
            {
                throw new ArgumentNullException("endpoint");
            }

            return this.Connect(adbEndpoint, new DnsEndPoint(endpoint.Address.ToString(), endpoint.Port));
        }

        /// <summary>
        /// Connect to a device via TCP/IP.
        /// </summary>
        /// <param name="adbEndpoint">
        /// The socket where the <c>adb</c> server is listening.
        /// </param>
        /// <param name="endpoint">
        /// The DNS endpoint at which the <c>adb</c> server on the device is running.
        /// </param>
        /// <returns>
        /// <c>0</c> if the operation completed successfully; otherwise,
        /// <c>-1</c>.
        /// </returns>
        public int Connect(IPEndPoint adbEndpoint, DnsEndPoint endpoint)
        {
            if (endpoint == null)
            {
                throw new ArgumentNullException("endpoint");
            }

            byte[] request = FormAdbRequest(string.Format("host:connect:{0}:{1}", endpoint.Host, endpoint.Port));

            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                socket.Connect(adbEndpoint);
                socket.Blocking = true;
                if (!this.Write(socket, request))
                {
                    throw new IOException("failed submitting request to  adb");
                }

                var resp = this.ReadAdbResponse(socket, false);
                if (!resp.IOSuccess || !resp.Okay)
                {
                    Log.e(TAG, "Got timeout or unhappy response from ADB req: " + resp.Message);
                    socket.Close();
                    return -1;
                }

                return 0;
            }
        }

        /// <summary>
        /// Creates a port forwarding request to a jdwp process.
        /// </summary>
        /// <param name="pid">
        /// The jdwp process pid on the device.
        /// </param>
        /// <returns>
        /// An array containing <c>####jwdp:{pid}</c>.
        /// </returns>
        private byte[] CreateJdwpForwardRequest(int pid)
        {
            string req = string.Format("jdwp:{0}", pid);
            return FormAdbRequest(req);
        }

        /// <summary>
        /// Executes a raw socket command.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="device">The device.</param>
        /// <param name="command">The command.</param>
        /// <returns>
        /// The socket on which the command was executed.
        /// </returns>
        /// <exception cref="AdbException">failed to submit the command: {0}.With(command)
        /// or
        /// Device rejected command: {0}.With(resp.Message)</exception>
        private Socket ExecuteRawSocketCommand(IPEndPoint address, IDevice device, string command)
        {
            return this.ExecuteRawSocketCommand(address, device, FormAdbRequest(command));
        }

        /// <summary>
        /// Executes the raw socket command.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="command">The command.</param>        /// <returns>
        /// The socket on which the command was executed.
        /// </returns>
        private Socket ExecuteRawSocketCommand(IPEndPoint address, string command)
        {
            return this.ExecuteRawSocketCommand(address, FormAdbRequest(command));
        }

        /// <summary>
        /// Executes the raw socket command.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="command">The command.</param>        /// <returns>
        /// The socket on which the command was executed.
        /// </returns>
        private Socket ExecuteRawSocketCommand(IPEndPoint address, byte[] command)
        {
            return this.ExecuteRawSocketCommand(address, null, command);
        }

        /// <summary>
        /// Executes the raw socket command.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="device">The device.</param>
        /// <param name="command">The command. Should call FormAdbRequest on the string to create the byte array.</param>
        /// <returns>
        /// The socket on which the command was executed.
        /// </returns>
        /// <exception cref="Managed.Adb.Exceptions.AdbException">
        /// Device is offline
        /// or
        /// failed to submit the command: {0}..With(command.GetString().Trim())
        /// or
        /// Device rejected command: {0}.With(resp.Message)
        /// </exception>
        /// <exception cref="AdbException">Device is offline.
        /// or
        /// failed to submit the command: {0}.With(command)
        /// or
        /// Device rejected command: {0}.With(resp.Message)</exception>
        private Socket ExecuteRawSocketCommand(IPEndPoint address, IDevice device, byte[] command)
        {
            if (device != null && !device.IsOnline)
            {
                throw new AdbException("Device is offline");
            }

            Socket adbChan = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            adbChan.Connect(address);
            adbChan.Blocking = true;
            if (device != null)
            {
                this.SetDevice(adbChan, device);
            }

            if (!this.Write(adbChan, command))
            {
                throw new AdbException("failed to submit the command: {0}.".With(command.GetString().Trim()));
            }

            AdbResponse resp = this.ReadAdbResponse(adbChan, false /* readDiagString */);
            if (!resp.IOSuccess || !resp.Okay)
            {
                throw new AdbException("Device rejected command: {0}".With(resp.Message));
            }

            return adbChan;
        }

        /// <summary>
        /// Returns the host prefix that should be used for a device.
        /// </summary>
        /// <param name="device">
        /// The device for which to get the host prefix.
        /// </param>
        /// <returns>
        /// The host prefix that should be used for the device.
        /// </returns>
        private string HostPrefixFromDevice(IDevice device)
        {
            switch (device.TransportType)
            {
                case TransportType.Host:
                    return "host-serial";
                case TransportType.Usb:
                    return "host-usb";
                case TransportType.Local:
                    return "host-local";
                case TransportType.Any:
                default:
                    return "host";
            }
        }
    }
}
