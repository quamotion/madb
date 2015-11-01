﻿// <copyright file="AdbHelper.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion. All rights reserved.
// </copyright>

namespace Managed.Adb
{
    using Managed.Adb.Exceptions;
    using Managed.Adb.Logs;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;

    /// <summary>
    /// <para>
    /// The Android Debug Bridge Helper class, to handle requests and connections to adb.
    /// </para>
    /// <para>
    /// <seealso cref="AndroidDebugBridge"/> is the public API to connection to adb, while <see cref="AdbClient"/>
    /// does the low level stuff.
    /// </para>
    /// </summary>
    /// <seealso href="https://github.com/android/platform_system_core/blob/master/adb/SERVICES.TXT">SERVICES.TXT</seealso>
    /// <seealso href="https://github.com/android/platform_system_core/blob/master/adb/adb_client.c">adb_client.c</seealso>
    /// <seealso href="https://github.com/android/platform_system_core/blob/master/adb/adb.c">adb.c</seealso>
    public class AdbClient : IAdbClient
    {
        /// <summary>
        /// The default encoding
        /// </summary>
        public const string DefaultEncoding = "ISO-8859-1";

        /// <summary>
        /// Logging tag
        /// </summary>
        private const string Tag = nameof(AdbClient);

        /// <summary>
        /// The default port to use when connecting to a device over TCP/IP.
        /// </summary>
        public const int DefaultPort = 5555;

        /// <summary>
        /// The default time to wait in the milliseconds.
        /// </summary>
        private const int WaitTime = 5;

        /// <summary>
        /// The singleton instance of the <see cref="AdbClient"/> class.
        /// </summary>
        private static AdbClient instance = null;

        public AdbClient(IPEndPoint endPoint)
        {
            if (endPoint == null)
            {
                throw new ArgumentNullException();
            }

            this.EndPoint = endPoint;
        }

        /// <summary>
        /// Gets the encoding used when communicating with adb.
        /// </summary>
        public static Encoding Encoding
        { get; } = Encoding.GetEncoding(DefaultEncoding);

        /// <summary>
        /// Gets an instance of the AdbHelper.
        /// </summary>
        public static AdbClient Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new AdbClient(AdbServer.EndPoint);
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

        public IPEndPoint EndPoint
        {
            get;
            private set;
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

        /// <include file='IAdbClient.xml' path='/IAdbClient/GetAdbVersion/*'/>
        public int GetAdbVersion()
        {
            using (var socket = SocketFactory.Create(this.EndPoint))
            {
                socket.SendAdbRequest("host:version");
                var response = socket.ReadAdbResponse(false);
                var version = socket.ReadString();

                return int.Parse(version, NumberStyles.HexNumber);
            }
        }

        /// <include file='IAdbClient.xml' path='/IAdbClient/KillAdb/*'/>
        public void KillAdb()
        {
            using (IAdbSocket socket = SocketFactory.Create(this.EndPoint))
            {
                socket.SendAdbRequest("host:kill");

                // The host will immediately close the connection after the kill
                // command has been sent; no need to read the response.
            }
        }

        /// <include file='IAdbClient.xml' path='/IAdbClient/GetDevices/*'/>
        public List<DeviceData> GetDevices()
        {
            using (IAdbSocket socket = SocketFactory.Create(this.EndPoint))
            {
                socket.SendAdbRequest("host:devices-l");
                socket.ReadAdbResponse(false);
                var reply = socket.ReadString();

                string[] data = reply.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

                return data.Select(d => DeviceData.CreateFromAdbData(d)).ToList();
            }
        }

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

        /// <include file='IAdbClient.xml' path='/IAdbClient/CreateForward/*'/>
        public void CreateForward(DeviceData device, string local, string remote, bool allowRebind)
        {
            using (IAdbSocket socket = SocketFactory.Create(this.EndPoint))
            {
                string rebind = allowRebind ? string.Empty : "norebind:";

                socket.SendAdbRequest($"host-serial:{device.Serial}:forward:{rebind}{local};{remote}");
                var response = socket.ReadAdbResponse(false);
            }
        }

        /// <include file='IAdbClient.xml' path='/IAdbClient/RemoveForward/*'/>
        public void RemoveForward(DeviceData device, int localPort)
        {
            using (IAdbSocket socket = SocketFactory.Create(this.EndPoint))
            {
                socket.SendAdbRequest($"host-serial:{device.Serial}:killforward:tcp:{localPort}");
                var response = socket.ReadAdbResponse(false);
            }
        }

        /// <include file='IAdbClient.xml' path='/IAdbClient/RemoveAllForwards/*'/>
        public void RemoveAllForwards(DeviceData device)
        {
            using (IAdbSocket socket = SocketFactory.Create(this.EndPoint))
            {
                socket.SendAdbRequest($"host-serial:{device.Serial}:killforward-all");
                var response = socket.ReadAdbResponse(false);
            }
        }

        /// <include file='IAdbClient.xml' path='/IAdbClient/ListForward/*'/>
        public IEnumerable<ForwardData> ListForward(DeviceData device)
        {
            using (IAdbSocket socket = SocketFactory.Create(this.EndPoint))
            {
                socket.SendAdbRequest($"host-serial:{device.Serial}:list-forward");
                var response = socket.ReadAdbResponse(false);

                var data = socket.ReadString();

                var parts = data.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                return parts.Select(p => ForwardData.FromString(p));
            }
        }

        /// <include file='IAdbClient.xml' path='/IAdbClient/ExecuteRemoteCommand/*'/>
        public void ExecuteRemoteCommand(string command, DeviceData device, IShellOutputReceiver rcvr, int maxTimeToOutputResponse)
        {
            using (IAdbSocket socket = SocketFactory.Create(this.EndPoint))
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
                            Log.w(Tag, "execute: cancelled");
                            throw new OperationCanceledException();
                        }

                        int count = socket.Read(data, maxTimeToOutputResponse);

                        if (count == 0)
                        {
                            // we're at the end, we flush the output
                            rcvr.Flush();
                            Log.w(Tag, "execute '" + command + "' on '" + device + "' : EOF hit. Read: " + count);
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
                                string sdata = AdbClient.Encoding.GetString(data);

                                var sdataTrimmed = sdata.Trim();
                                if (sdataTrimmed.EndsWith(string.Format("{0}: not found", cmd[0])))
                                {
                                    Log.w(Tag, "The remote execution returned: '{0}: not found'", cmd[0]);
                                    throw new FileNotFoundException(string.Format("The remote execution returned: '{0}: not found'", cmd[0]));
                                }

                                if (sdataTrimmed.EndsWith("No such file or directory"))
                                {
                                    Log.w(Tag, "The remote execution returned: {0}", sdataTrimmed);
                                    throw new FileNotFoundException(string.Format("The remote execution returned: {0}", sdataTrimmed));
                                }

                                // for "unknown options"
                                if (sdataTrimmed.Contains("Unknown option"))
                                {
                                    Log.w(Tag, "The remote execution returned: {0}", sdataTrimmed);
                                    throw new UnknownOptionException(sdataTrimmed);
                                }

                                // for "aborting" commands
                                if (sdataTrimmed.IsMatch("Aborting.$"))
                                {
                                    Log.w(Tag, "The remote execution returned: {0}", sdataTrimmed);
                                    throw new CommandAbortingException(sdataTrimmed);
                                }

                                // for busybox applets
                                // cmd: applet not found
                                if (sdataTrimmed.IsMatch("applet not found$") && cmd.Length > 1)
                                {
                                    Log.w(Tag, "The remote execution returned: '{0}'", sdataTrimmed);
                                    throw new FileNotFoundException(string.Format("The remote execution returned: '{0}'", sdataTrimmed));
                                }

                                // checks if the permission to execute the command was denied.
                                // workitem: 16822
                                if (sdataTrimmed.IsMatch("(permission|access) denied$"))
                                {
                                    Log.w(Tag, "The remote execution returned: '{0}'", sdataTrimmed);
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

        /// <include file='IAdbClient.xml' path='/IAdbClient/GetFrameBuffer/*'/>
        public RawImage GetFrameBuffer(DeviceData device)
        {
            using (IAdbSocket socket = SocketFactory.Create(this.EndPoint))
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
                        Log.w(Tag, "Unsupported protocol: " + version);
                        return null;
                    }
                }

                Log.d(Tag, $"image params: bpp={imageParams.Bpp}, size={imageParams.Size}, width={imageParams.Width}, height={imageParams.Height}");

                reply = new byte[imageParams.Size];
                socket.Read(reply);
                imageParams.Data = reply;
                return imageParams;
            }
        }
        
        /// <include file='IAdbClient.xml' path='/IAdbClient/RunLogService/*'/>
        public void RunLogService(DeviceData device, string logName, LogReceiver receiver)
        {
            using (IAdbSocket socket = SocketFactory.Create(this.EndPoint))
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
                        if (receiver != null && receiver.IsCancelled)
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
                            if (receiver != null)
                            {
                                var d = ms.ToArray();
                                receiver.ParseNewData(d, 0, d.Length);
                            }
                        }
                    }
                }
            }
        }

        /// <include file='IAdbClient.xml' path='/IAdbClient/Reboot/*'/>
        public void Reboot(string into, DeviceData device)
        {
            var request = $"reboot:{into}";

            using (IAdbSocket socket = SocketFactory.Create(this.EndPoint))
            {
                socket.SendAdbRequest(request);
                var response = socket.ReadAdbResponse(false);
            }
        }

        /// <include file='IAdbClient.xml' path='/IAdbClient/Connect/*'/>
        public void Connect(DnsEndPoint endpoint)
        {
            if (endpoint == null)
            {
                throw new ArgumentNullException(nameof(endpoint));
            }

            using (IAdbSocket socket = SocketFactory.Create(this.EndPoint))
            {
                socket.SendAdbRequest($"host:connect:{endpoint.Host}:{endpoint.Port}");
                var response = socket.ReadAdbResponse(false);
            }
        }
    }
}