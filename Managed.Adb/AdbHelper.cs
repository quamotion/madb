// <copyright file="AdbHelper.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion. All rights reserved.
// </copyright>

namespace Managed.Adb
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;
    using Managed.Adb.Exceptions;
    using Managed.Adb.Logs;
    using MoreLinq;

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
        /// Gets or sets an instance of the <see cref="IAdbSocketFactory"/> that is used
        /// to create new <see cref="IAdbSocket"/> objects.
        /// </summary>
        public static IAdbSocketFactory SocketFactory
        { get; set; } = new AdbSocketFactory();

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
        /// Creates the adb forward request.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="port">The port.</param>
        /// <returns>
        /// This returns an array containing <c>"####tcp:{port}:{addStr}"</c>.
        /// </returns>
        public static byte[] CreateAdbForwardRequest(string address, int port)
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

        // The individual services are listed in the same order as
        // https://android.googlesource.com/platform/system/core/+/master/adb/SERVICES.TXT

        /// <summary>
        /// Ask the ADB server for its internal version number.
        /// </summary>
        /// <param name="endPoint">
        /// The endpoint at which the Android Debug Bridge is listening.
        /// </param>
        /// <returns>
        /// The ADB version number.
        /// </returns>
        /// <exception cref="IOException">
        /// An error occurred connecting to ADB
        /// </exception>
        /// <exception cref="AdbException">
        /// An error occurred connecting to ADB
        /// </exception>
        public int GetAdbVersion(IPEndPoint endPoint)
        {
            using (var socket = SocketFactory.Create(endPoint))
            {
                socket.SendAdbRequest("host:version");
                var response = socket.ReadAdbResponse(false);
                var version = socket.ReadString();

                return int.Parse(version, NumberStyles.HexNumber);
            }
        }

        /// <summary>
        /// Ask the ADB server to quit immediately. This is used when the
        /// ADB client detects that an obsolete server is running after an
        /// upgrade.
        /// </summary>
        /// <param name="endPoint">
        /// The endpoint at which the Android Debug Bridge is listening.
        /// </param>
        /// <exception cref="System.IO.IOException">failed asking to kill adb</exception>
        public void KillAdb(IPEndPoint endPoint)
        {
            using (IAdbSocket socket = SocketFactory.Create(endPoint))
            {
                socket.SendAdbRequest("host:kill");

                // The host will immediately close the connection after the kill
                // command has been sent; no need to read the response.
            }
        }

        /// <summary>
        /// Gets the devices that are available for communication.
        /// </summary>
        /// <param name="endPoint">
        /// The endpoint at which the Android Debug Bridge is listening.
        /// </param>
        /// <returns>A list of devices that are connected.</returns>
        public List<DeviceData> GetDevices(IPEndPoint endPoint)
        {
            using (IAdbSocket socket = SocketFactory.Create(endPoint))
            {
                socket.SendAdbRequest("host:devices-l");
                socket.ReadAdbResponse(false);
                var reply = socket.ReadString();

                string[] data = reply.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

                List<DeviceData> s = new List<DeviceData>();
                data.ForEach(item =>
                {
                    var device = DeviceData.CreateFromAdbData(item);
                    s.Add(device);
                });

                return s;
            }
        }

        // host:track-devices is implemented by the DeviceMonitor.

        // host:emulator is not implemented

        /// <summary>
        /// Ask to switch the connection to the device/emulator identified by
        /// <paramref name="device"/>. After this request, every client request will
        /// be sent directly to the adbd daemon running on the device.
        /// </summary>
        /// <param name="socket">
        /// An instance of the <see cref="IAdbSocket"/> class which is connected to
        /// the Android Debug Bridge.
        /// </param>
        /// <param name="device">
        /// The device to which to connect.
        /// </param>
        /// <remarks>
        /// If <paramref name="device"/> is <see langword="null"/>, this metod
        /// does nothing.
        /// </remarks>
        public void SetDevice(IAdbSocket socket, DeviceData device)
        {
            // if the device is not null, then we first tell adb we're looking to talk
            // to a specific device
            if (device != null)
            {
                socket.SendAdbRequest($"host:transport:{device.Serial}");

                try
                {
                    socket.ReadAdbResponse(false);
                }
                catch (AdbException e)
                {
                    if (string.Equals("device not found", e.AdbError, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new DeviceNotFoundException(device.Serial);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }

        // host:transport-usb is not implemented
        // host:transport-local is not implemented
        // host:transport-any is not implemented

        // <host-prefix>:get-product is not implemented
        // <host-prefix>:get-serialno is not implemented
        // <host-prefix>:get-devpath is not implemented
        // <host-prefix>:get-state is not implemented

        /// <summary>
        /// Asks the ADB server to forward local connections from <paramref name="local"/>
        /// to the <paramref name="remote"/> address on the <paramref name="device"/>.
        /// </summary>
        /// <param name="endPoint">
        /// The endpoint at which the Android Debug Bridge is listening.
        /// </param>
        /// <param name="device">
        /// The device to which to forward the connections.
        /// </param>
        /// <param name="local">
        /// <para>
        /// The local address to forward. This value can be in one of:
        /// </para>
        /// <list type="ordered">
        ///     <item>
        ///         <c>tcp:&lt;port&gt;</c>: TCP connection on localhost:&lt;port&gt;
        ///     </item>
        ///     <item>
        ///         <c>local:&lt;path&gt;</c>: Unix local domain socket on &lt;path&gt;
        ///     </item>
        /// </list>
        /// </param>
        /// <param name="remote">
        /// <para>
        /// The remote address to forward. This value can be in one of:
        /// </para>
        /// <list type="ordered">
        ///     <item>
        ///         <c>tcp:&lt;port&gt;</c>: TCP connection on localhost:&lt;port&gt; on device
        ///     </item>
        ///     <item>
        ///         <c>local:&lt;path&gt;</c>: Unix local domain socket on &lt;path&gt; on device
        ///     </item>
        ///     <item>
        ///         <c>jdwp:&lt;pid&gt;</c>: JDWP thread on VM process &lt;pid&gt; on device.
        ///     </item>
        /// </list>
        /// <param name="allowRebind">
        /// If set to <see langword="true"/>, the request will fail if there is already a forward
        /// connection from <paramref name="local"/>.
        /// </param>
        /// </param>
        public void CreateForward(IPEndPoint endPoint, DeviceData device, string local, string remote, bool allowRebind)
        {
            using (IAdbSocket socket = SocketFactory.Create(endPoint))
            {
                string rebind = allowRebind ? string.Empty : "norebind:";

                socket.SendAdbRequest($"host-serial:{device.Serial}:forward:{rebind}{local};{remote}");
                var response = socket.ReadAdbResponse(false);
            }
        }

        /// <summary>
        ///  Creates a port forwarding between a local and a remote port.
        /// </summary>
        /// <param name="endPoint">
        /// The endpoint at which the Android Debug Bridge is listening.
        /// </param>
        /// <param name="device">
        /// The device to which to forward the connections.
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
        public void CreateForward(IPEndPoint endPoint, DeviceData device, int localPort, int remotePort)
        {
            this.CreateForward(endPoint, device, $"tcp:{localPort}", $"tcp:{remotePort}", true);
        }

        /// <summary>
        /// Forwards a remote Unix socket to a local TCP socket.
        /// </summary>
        /// <param name="endPoint">
        /// The endpoint at which the Android Debug Bridge is listening.
        /// </param>
        /// <param name="device">
        /// The device to which to forward the connections.
        /// </param>
        /// <param name="localPort">
        /// The local port to forward.
        /// </param>
        /// <param name="remoteSocket">
        /// The remote Unix socket.
        /// </param>
        /// <exception cref="Managed.Adb.Exceptions.AdbException">
        /// The client failed to submit the forward command.
        /// </exception>
        /// <exception cref="Managed.Adb.Exceptions.AdbException">
        /// The device rejected command. The error message will include the error message provided by the device.
        /// </exception>
        public void CreateForward(IPEndPoint endPoint, DeviceData device, int localPort, string remoteSocket)
        {
            this.CreateForward(endPoint, device, $"tcp:{localPort}", $"local:{remoteSocket}", true);
        }

        /// <summary>
        /// Remove a port forwarding between a local and a remote port.
        /// </summary>
        /// <param name="endPoint">
        /// The endpoint at which the Android Debug Bridge is listening.
        /// </param>
        /// <param name="device">
        /// The device on which to remove the port forwarding
        /// </param>
        /// <param name="localPort">
        /// Specification of the local port that was forwarded
        /// </param>
        public void RemoveForward(IPEndPoint endPoint, DeviceData device, int localPort)
        {
            using (IAdbSocket socket = SocketFactory.Create(endPoint))
            {
                socket.SendAdbRequest($"host-serial:{device.Serial}:killforward:tcp:{localPort}");
                var response = socket.ReadAdbResponse(false);
            }
        }

        /// <summary>
        /// Removes all forwards for a given device.
        /// </summary>
        /// <param name="endPoint">
        /// The endpoint at which the Android Debug Bridge is listening.
        /// </param>
        /// <param name="device">
        /// The device on which to remove the port forwarding
        /// </param>
        public void RemoveAllForward(IPEndPoint endPoint, DeviceData device)
        {
            using (IAdbSocket socket = SocketFactory.Create(endPoint))
            {
                socket.SendAdbRequest($"host-serial:{device.Serial}:killforward-all");
                var response = socket.ReadAdbResponse(false);
            }
        }

        /// <summary>
        /// List all existing forward connections from this server.
        /// </summary>
        /// <param name="endPoint">
        /// The endpoint at which the Android Debug Bridge is listening.
        /// </param>
        /// <param name="device">
        /// The device for which to list the existing foward connections.
        /// </param>
        /// <returns>
        /// A <see cref="ForwardData"/> entry for each existing forward connection.
        /// </returns>
        public IEnumerable<ForwardData> ListForward(IPEndPoint endPoint, DeviceData device)
        {
            using (IAdbSocket socket = SocketFactory.Create(endPoint))
            {
                socket.SendAdbRequest($"host-serial:{device.Serial}:list-forward");
                var response = socket.ReadAdbResponse(false);

                var data = socket.ReadString();

                var parts = data.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                return parts.Select(p => ForwardData.FromString(p));
            }
        }

        /// <summary>
        /// Executes the remote command.
        /// </summary>
        /// <param name="endPoint">
        /// The endpoint at which the Android Debug Bridge is listening.
        /// </param>
        /// <param name="command">
        /// The command to execute.
        /// </param>
        /// <param name="device">
        /// The device on which to execute the command.
        /// </param>
        /// <param name="rcvr">
        /// A <see cref="IShellOutputReceiver"/> that receives the command output. Set to <see langword="null"/>
        /// if you are not interested in the output.
        /// </param>
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
        public void ExecuteRemoteCommand(IPEndPoint endPoint, string command, DeviceData device, IShellOutputReceiver rcvr, int maxTimeToOutputResponse)
        {
            using (IAdbSocket socket = SocketFactory.Create(endPoint))
            {
                this.SetDevice(socket, device);
                socket.SendAdbRequest($"shell:{command}");
                var resopnse = socket.ReadAdbResponse(false);

                try
                {
                    // Read in blocks of 16kb
                    byte[] data = new byte[16 * 1024];

                    while (true)
                    {
                        if (rcvr != null && rcvr.IsCancelled)
                        {
                            Log.w(TAG, "execute: cancelled");
                            throw new OperationCanceledException();
                        }

                        int count = socket.Read(data, maxTimeToOutputResponse);

                        if (count == 0)
                        {
                            // we're at the end, we flush the output
                            rcvr.Flush();
                            Log.w(TAG, "execute '" + command + "' on '" + device + "' : EOF hit. Read: " + count);
                            break;
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

        // shell: not implemented
        // remount: not implemented
        // dev:<path> not implemented
        // tcp:<port> not implemented
        // tcp:<port>:<server-name> not implemented
        // local:<path> not implemented
        // localreserved:<path> not implemented
        // localabstract:<path> not implemented

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
        public RawImage GetFrameBuffer(IPEndPoint adbSockAddr, DeviceData device)
        {
            using (IAdbSocket socket = SocketFactory.Create(adbSockAddr))
            {
                RawImage imageParams = new RawImage();
                socket.SendAdbRequest($"host-serial:{device.Serial}:framebuffer:");
                socket.ReadAdbResponse(false);

                // After the OKAY, the service sends 16-byte binary structure
                // containing the following fields (little - endian format):
                // depth:
                //    uint32_t:
                //    framebuffer depth
                // size: uint32_t: framebuffer size in bytes
                // width:   uint32_t: framebuffer width in pixels
                // height:  uint32_t: framebuffer height in pixels

                // first the protocol version.
                var reply = new byte[4];
                socket.Read(reply);
                int version = BitConverter.ToInt16(reply, 0);

                // get the header size (this is a count of int)
                int headerSize = RawImage.GetHeaderSize(version);

                // read the header
                reply = new byte[headerSize * 4];
                socket.Read(reply);

                using (MemoryStream ms = new MemoryStream(reply))
                using (BinaryReader reader = new BinaryReader(ms))
                {
                    // fill the RawImage with the header
                    if (imageParams.ReadHeader(version, reader) == false)
                    {
                        Log.w(TAG, "Unsupported protocol: " + version);
                        return null;
                    }
                }

                Log.d(TAG, $"image params: bpp={imageParams.Bpp}, size={imageParams.Size}, width={imageParams.Width}, height={imageParams.Height}");

                reply = new byte[imageParams.Size];
                socket.Read(reply);
                imageParams.Data = reply;
                return imageParams;
            }
        }

        // jdwp:<pid>: not implemented
        // track-jdwp: not implemented
        // sync: not implemented
        // reverse:<forward-command>: not implemented

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
        public static bool Write(Socket socket, byte[] data)
        {
            try
            {
                Write(socket, data, -1, DdmPreferences.Timeout);
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
        public static void Write(Socket socket, byte[] data, int length, int timeout)
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
        public static AdbResponse ReadAdbResponse(Socket socket, bool readDiagString)
        {
            AdbResponse resp = new AdbResponse();

            byte[] reply = new byte[4];
            if (!Read(socket, reply))
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
                if (!Read(socket, lenBuf))
                {
                    Log.w(TAG, "Expected diagnostic string not found");
                    break;
                }

                string lenStr = ReplyToString(lenBuf);

                int len;
                try
                {
                    len = int.Parse(lenStr, System.Globalization.NumberStyles.HexNumber);
                }
                catch (FormatException)
                {
                    Log.e(TAG, "Expected digits, got '{0}' : {1} {2} {3} {4}", lenBuf[0], lenBuf[1], lenBuf[2], lenBuf[3]);
                    Log.e(TAG, "reply was {0}", ReplyToString(reply));
                    break;
                }

                byte[] msg = new byte[len];
                if (!Read(socket, msg))
                {
                    Log.e(TAG, "Failed reading diagnostic string, len={0}", len);
                    break;
                }

                resp.Message = ReplyToString(msg);
                Log.e(TAG, "Got reply '{0}', diag='{1}'", ReplyToString(reply), resp.Message);

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
        public static bool Read(Socket socket, byte[] data)
        {
            try
            {
                Read(socket, data, -1, DdmPreferences.Timeout);
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
        public static string ReplyToString(byte[] reply)
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
        /// Executes a shell command on the remote device
        /// </summary>
        /// <param name="endPoint">The end point.</param>
        /// <param name="command">The command.</param>
        /// <param name="device">The device.</param>
        /// <param name="rcvr">The RCVR.</param>
        /// <remarks>
        /// Should check if you CanSU before calling this.
        /// </remarks>
        public void ExecuteRemoteRootCommand(IPEndPoint endPoint, string command, DeviceData device, IShellOutputReceiver rcvr)
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
        public void ExecuteRemoteRootCommand(IPEndPoint endPoint, string command, DeviceData device, IShellOutputReceiver rcvr, int maxTimeToOutputResponse)
        {
            this.ExecuteRemoteCommand(endPoint, string.Format("su -c \"{0}\"", command), device, rcvr);
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
        public void ExecuteRemoteCommand(IPEndPoint endPoint, string command, DeviceData device, IShellOutputReceiver rcvr)
        {
            this.ExecuteRemoteCommand(endPoint, command, device, rcvr, int.MaxValue);
        }

        /// <summary>
        /// Runs the Event log service on the Device, and provides its output to the LogReceiver.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="device">The device.</param>
        /// <param name="rcvr">The RCVR.</param>
        public void RunEventLogService(IPEndPoint address, DeviceData device, LogReceiver rcvr)
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
        public void RunLogService(IPEndPoint address, DeviceData device, string logName, LogReceiver rcvr)
        {
            using (IAdbSocket socket = SocketFactory.Create(address))
            {
                socket.SendAdbRequest($"log:{logName}");
                var response = socket.ReadAdbResponse(false);

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

                        count = socket.Read(buffer, DdmPreferences.Timeout);
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
        public void Reboot(IPEndPoint adbSocketAddress, DeviceData device)
        {
            this.Reboot(string.Empty, adbSocketAddress, device);
        }

        /// <summary>
        /// Reboots the specified device in to the specified mode.
        /// </summary>
        /// <param name="into">The into.</param>
        /// <param name="adbSockAddr">The adb sock addr.</param>
        /// <param name="device">The device.</param>
        public void Reboot(string into, IPEndPoint adbSockAddr, DeviceData device)
        {
            var request = $"reboot:{into}";

            using (IAdbSocket socket = SocketFactory.Create(adbSockAddr))
            {
                socket.SendAdbRequest(request);
                var response = socket.ReadAdbResponse(false);
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
                if (!Write(socket, request))
                {
                    throw new IOException("failed submitting request to  adb");
                }

                var resp = ReadAdbResponse(socket, false);
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
        /// Returns the host prefix that should be used for a device.
        /// </summary>
        /// <param name="device">
        /// The device for which to get the host prefix.
        /// </param>
        /// <returns>
        /// The host prefix that should be used for the device.
        /// </returns>
        private string HostPrefixFromDevice(TransportType transportType)
        {
            switch (transportType)
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
