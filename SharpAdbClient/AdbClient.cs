// <copyright file="AdbClient.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion. All rights reserved.
// </copyright>

namespace SharpAdbClient
{
    using SharpAdbClient.Exceptions;
    using SharpAdbClient.Logs;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;

    /// <summary>
    /// <para>
    ///     Implements the <see cref="IAdbClient"/> interface, and allows you to interact with the
    ///     adb server and devices that are connected to that adb server.
    /// </para>
    /// <para>
    ///     For example, to fetch a list of all devices that are currently connected to this PC, you can
    ///     call the <see cref="GetDevices"/> method.
    /// </para>
    /// <para>
    ///     To run a command on a device, you can use the <see cref="ExecuteRemoteCommand(string, DeviceData, IShellOutputReceiver, int)"/>
    ///     method.
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
        /// The default port to use when connecting to a device over TCP/IP.
        /// </summary>
        public const int DefaultPort = 5555;

        /// <summary>
        /// Logging tag
        /// </summary>
        private const string Tag = nameof(AdbClient);

        /// <summary>
        /// The default time to wait in the milliseconds.
        /// </summary>
        private const int WaitTime = 5;

        /// <summary>
        /// The singleton instance of the <see cref="AdbClient"/> class.
        /// </summary>
        private static IAdbClient instance = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdbClient"/> class.
        /// </summary>
        /// <param name="endPoint">
        /// The <see cref="IPEndPoint"/> at which the adb server is listening.
        /// </param>
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
        public static IAdbClient Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new AdbClient(AdbServer.EndPoint);
                }

                return instance;
            }

            set
            {
                instance = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="IPEndPoint"/> at which the adb server is listening.
        /// </summary>
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
            using (var socket = Factories.AdbSocketFactory(this.EndPoint))
            {
                socket.SendAdbRequest("host:version");
                var response = socket.ReadAdbResponse();
                var version = socket.ReadString();

                return int.Parse(version, NumberStyles.HexNumber);
            }
        }

        /// <include file='IAdbClient.xml' path='/IAdbClient/KillAdb/*'/>
        public void KillAdb()
        {
            using (IAdbSocket socket = Factories.AdbSocketFactory(this.EndPoint))
            {
                socket.SendAdbRequest("host:kill");

                // The host will immediately close the connection after the kill
                // command has been sent; no need to read the response.
            }
        }

        /// <include file='IAdbClient.xml' path='/IAdbClient/GetDevices/*'/>
        public List<DeviceData> GetDevices()
        {
            using (IAdbSocket socket = Factories.AdbSocketFactory(this.EndPoint))
            {
                socket.SendAdbRequest("host:devices-l");
                socket.ReadAdbResponse();
                var reply = socket.ReadString();

                string[] data = reply.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

                return data.Select(d => DeviceData.CreateFromAdbData(d)).ToList();
            }
        }

        public void SetDevice(IAdbSocket socket, DeviceData device)
        {
            // if the device is not null, then we first tell adb we're looking to talk
            // to a specific device
            if (device != null)
            {
                socket.SendAdbRequest($"host:transport:{device.Serial}");

                try
                {
                    var response = socket.ReadAdbResponse();
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
            using (IAdbSocket socket = Factories.AdbSocketFactory(this.EndPoint))
            {
                string rebind = allowRebind ? string.Empty : "norebind:";

                socket.SendAdbRequest($"host-serial:{device.Serial}:forward:{rebind}{local};{remote}");
                var response = socket.ReadAdbResponse();
            }
        }

        /// <include file='IAdbClient.xml' path='/IAdbClient/RemoveForward/*'/>
        public void RemoveForward(DeviceData device, int localPort)
        {
            using (IAdbSocket socket = Factories.AdbSocketFactory(this.EndPoint))
            {
                socket.SendAdbRequest($"host-serial:{device.Serial}:killforward:tcp:{localPort}");
                var response = socket.ReadAdbResponse();
            }
        }

        /// <include file='IAdbClient.xml' path='/IAdbClient/RemoveAllForwards/*'/>
        public void RemoveAllForwards(DeviceData device)
        {
            using (IAdbSocket socket = Factories.AdbSocketFactory(this.EndPoint))
            {
                socket.SendAdbRequest($"host-serial:{device.Serial}:killforward-all");
                var response = socket.ReadAdbResponse();
            }
        }

        /// <include file='IAdbClient.xml' path='/IAdbClient/ListForward/*'/>
        public IEnumerable<ForwardData> ListForward(DeviceData device)
        {
            using (IAdbSocket socket = Factories.AdbSocketFactory(this.EndPoint))
            {
                socket.SendAdbRequest($"host-serial:{device.Serial}:list-forward");
                var response = socket.ReadAdbResponse();

                var data = socket.ReadString();

                var parts = data.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                return parts.Select(p => ForwardData.FromString(p));
            }
        }

        /// <include file='IAdbClient.xml' path='/IAdbClient/ExecuteRemoteCommand/*'/>
        public void ExecuteRemoteCommand(string command, DeviceData device, IShellOutputReceiver rcvr, CancellationToken cancellationToken, int maxTimeToOutputResponse)
        {
            using (IAdbSocket socket = Factories.AdbSocketFactory(this.EndPoint))
            {
                this.SetDevice(socket, device);
                socket.SendAdbRequest($"shell:{command}");
                var response = socket.ReadAdbResponse();

                try
                {
                    using (StreamReader reader = new StreamReader(socket.GetShellStream(), Encoding))
                    {
                        while (reader.Peek() >= 0)
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            var line = reader.ReadLine();

                            if (rcvr != null)
                            {
                                rcvr.AddOutput(line);
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
                    if (rcvr != null)
                    {
                        rcvr.Flush();
                    }
                }
            }
        }

        /// <include file='IAdbClient.xml' path='/IAdbClient/GetFrameBuffer/*'/>
        public Image GetFrameBuffer(DeviceData device)
        {
            using (IAdbSocket socket = Factories.AdbSocketFactory(this.EndPoint))
            {
                // Select the target device
                this.SetDevice(socket, device);

                // Send the framebuffer command
                socket.SendAdbRequest("framebuffer:");
                socket.ReadAdbResponse();

                // The result first is a FramebufferHeader object,
                var size = Marshal.SizeOf(typeof(FramebufferHeader));
                var headerData = new byte[size];
                socket.Read(headerData);

                var header = FramebufferHeader.Read(headerData);

                // followed by the actual framebuffer content
                var imageData = new byte[header.Size];
                socket.Read(imageData);

                // Convert the framebuffer to an image, and return that.
                return header.ToImage(imageData);
            }
        }

        /// <include file='IAdbClient.xml' path='/IAdbClient/RunLogService/*'/>
        public IEnumerable<LogEntry> RunLogService(DeviceData device, params LogId[] logNames)
        {
            // The 'log' service has been deprecated, see
            // https://android.googlesource.com/platform/system/core/+/7aa39a7b199bb9803d3fd47246ee9530b4a96177
            using (IAdbSocket socket = Factories.AdbSocketFactory(this.EndPoint))
            {
                this.SetDevice(socket, device);

                StringBuilder request = new StringBuilder();
                request.Append("shell:logcat -B");

                foreach (var logName in logNames)
                {
                    request.Append($" -b {logName.ToString().ToLower()}");
                }

                socket.SendAdbRequest(request.ToString());
                var response = socket.ReadAdbResponse();

                using (Stream stream = socket.GetShellStream())
                using (LogReader reader = new LogReader(stream))
                {
                    while (true)
                    {
                        LogEntry entry = null;

                        try
                        {
                            entry = reader.ReadEntry();
                        }
                        catch (EndOfStreamException)
                        {
                            // This indicates the end of the stream; the entry will remain null.
                        }

                        if (entry != null)
                        {
                            yield return entry;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
        }

        /// <include file='IAdbClient.xml' path='/IAdbClient/Reboot/*'/>
        public void Reboot(string into, DeviceData device)
        {
            var request = $"reboot:{into}";

            using (IAdbSocket socket = Factories.AdbSocketFactory(this.EndPoint))
            {
                this.SetDevice(socket, device);
                socket.SendAdbRequest(request);
                var response = socket.ReadAdbResponse();
            }
        }

        /// <include file='IAdbClient.xml' path='/IAdbClient/Connect/*'/>
        public void Connect(DnsEndPoint endpoint)
        {
            if (endpoint == null)
            {
                throw new ArgumentNullException(nameof(endpoint));
            }

            using (IAdbSocket socket = Factories.AdbSocketFactory(this.EndPoint))
            {
                socket.SendAdbRequest($"host:connect:{endpoint.Host}:{endpoint.Port}");
                var response = socket.ReadAdbResponse();
            }
        }
    }
}
