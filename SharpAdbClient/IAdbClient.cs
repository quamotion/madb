// <copyright file="IAdbClient.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion. All rights reserved.
// </copyright>

namespace SharpAdbClient
{
    using Logs;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// A common interface for any class that allows you to interact with the
    /// adb server and devices that are connected to that adb server.
    /// </summary>
    public interface IAdbClient
    {
        /// <summary>
        /// Gets the <see cref="EndPoint"/> at which the Android Debug Bridge server is listening.
        /// </summary>
        EndPoint EndPoint { get; }

        // The individual services are listed in the same order as
        // https://android.googlesource.com/platform/system/core/+/master/adb/SERVICES.TXT

        /// <include file='IAdbClient.xml' path='/IAdbClient/GetAdbVersion/*'/>
        int GetAdbVersion();

        /// <include file='IAdbClient.xml' path='/IAdbClient/KillAdb/*'/>
        void KillAdb();

        /// <include file='IAdbClient.xml' path='/IAdbClient/GetDevices/*'/>
        List<DeviceData> GetDevices();

        // host:track-devices is implemented by the DeviceMonitor.
        // host:emulator is not implemented

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
        /// <param name="device">
        /// The device to which to forward the connections.
        /// </param>
        /// <param name="local">
        /// <para>
        /// The local address to forward. This value can be in one of:
        /// </para>
        /// <list type="ordered">
        ///   <item>
        ///     <c>tcp:&lt;port&gt;</c>: TCP connection on localhost:&lt;port&gt;
        ///   </item>
        ///   <item>
        ///     <c>local:&lt;path&gt;</c>: Unix local domain socket on &lt;path&gt;
        ///   </item>
        /// </list>
        /// </param>
        /// <param name="remote">
        /// <para>
        /// The remote address to forward. This value can be in one of:
        /// </para>
        /// <list type="ordered">
        ///   <item>
        ///     <c>tcp:&lt;port&gt;</c>: TCP connection on localhost:&lt;port&gt; on device
        ///   </item>
        ///   <item>
        ///     <c>local:&lt;path&gt;</c>: Unix local domain socket on &lt;path&gt; on device
        ///   </item>
        ///   <item>
        ///     <c>jdwp:&lt;pid&gt;</c>: JDWP thread on VM process &lt;pid&gt; on device.
        ///   </item>
        /// </list>
        /// </param>
        /// <param name="allowRebind">
        /// If set to <see langword="true"/>, the request will fail if there is already a forward
        /// connection from <paramref name="local"/>.
        /// </param>
        /// <returns>
        /// If your requested to start forwarding to local port TCP:0, the port number of the TCP port
        /// which has been opened. In all other cases, <c>0</c>.
        /// </returns>
        int CreateForward(DeviceData device, string local, string remote, bool allowRebind);

        /// <summary>
        /// Asks the ADB server to forward local connections from <paramref name="local"/>
        /// to the <paramref name="remote"/> address on the <paramref name="device"/>.
        /// </summary>
        /// <param name="device">
        /// The device to which to forward the connections.
        /// </param>
        /// <param name="local">
        /// <para>
        /// The local address to forward. This value can be in one of:
        /// </para>
        /// <list type="ordered">
        ///   <item>
        ///     <c>tcp:&lt;port&gt;</c>: TCP connection on localhost:&lt;port&gt;
        ///   </item>
        ///   <item>
        ///     <c>local:&lt;path&gt;</c>: Unix local domain socket on &lt;path&gt;
        ///   </item>
        /// </list>
        /// </param>
        /// <param name="remote">
        /// <para>
        /// The remote address to forward. This value can be in one of:
        /// </para>
        /// <list type="ordered">
        ///   <item>
        ///     <c>tcp:&lt;port&gt;</c>: TCP connection on localhost:&lt;port&gt; on device
        ///   </item>
        ///   <item>
        ///     <c>local:&lt;path&gt;</c>: Unix local domain socket on &lt;path&gt; on device
        ///   </item>
        ///   <item>
        ///     <c>jdwp:&lt;pid&gt;</c>: JDWP thread on VM process &lt;pid&gt; on device.
        ///   </item>
        /// </list>
        /// </param>
        /// <param name="allowRebind">
        /// If set to <see langword="true"/>, the request will fail if there is already a forward
        /// connection from <paramref name="local"/>.
        /// </param>
        /// <returns>
        /// If your requested to start forwarding to local port TCP:0, the port number of the TCP port
        /// which has been opened. In all other cases, <c>0</c>.
        /// </returns>
        int CreateForward(DeviceData device, ForwardSpec local, ForwardSpec remote, bool allowRebind);

        /// <include file='IAdbClient.xml' path='/IAdbClient/RemoveForward/*'/>
        void RemoveForward(DeviceData device, int localPort);

        /// <include file='IAdbClient.xml' path='/IAdbClient/RemoveAllForwards/*'/>
        void RemoveAllForwards(DeviceData device);

        /// <include file='IAdbClient.xml' path='/IAdbClient/ListForward/*'/>
        IEnumerable<ForwardData> ListForward(DeviceData device);

        /// <include file='IAdbClient.xml' path='/IAdbClient/ExecuteRemoteCommand/*'/>
        Task ExecuteRemoteCommandAsync(string command, DeviceData device, IShellOutputReceiver receiver, CancellationToken cancellationToken, int maxTimeToOutputResponse);

        /// <summary>
        /// Executes a command on the device.
        /// </summary>
        /// <param name="command">
        /// The command to execute.
        /// </param>
        /// <param name="device">
        /// The device on which to run the command.
        /// </param>
        /// <param name="receiver">
        /// The receiver which will get the command output.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <param name="maxTimeToOutputResponse">
        /// A default timeout for the command.
        /// </param>
        /// <param name="encoding">
        /// The encoding to use when parsing the command output.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation.
        /// </returns>
        Task ExecuteRemoteCommandAsync(string command, DeviceData device, IShellOutputReceiver receiver, CancellationToken cancellationToken, int maxTimeToOutputResponse, Encoding encoding);

        // shell: not implemented
        // remount: not implemented
        // dev:<path> not implemented
        // tcp:<port> not implemented
        // tcp:<port>:<server-name> not implemented
        // local:<path> not implemented
        // localreserved:<path> not implemented
        // localabstract:<path> not implemented

        /// <summary>
        /// Gets a <see cref="Framebuffer"/> which contains the framebuffer data for this device. The framebuffer data can be refreshed,
        /// giving you high performance access to the device's framebuffer.
        /// </summary>
        /// <param name="device">
        /// The device for which to get the framebuffer.
        /// </param>
        /// <returns>
        /// A <see cref="Framebuffer"/> object which can be used to get the framebuffer of the device.
        /// </returns>
        Framebuffer CreateRefreshableFramebuffer(DeviceData device);

        /// <include file='IAdbClient.xml' path='/IAdbClient/GetFrameBuffer/*'/>
        Task<Image> GetFrameBufferAsync(DeviceData device, CancellationToken cancellationToken);

        // jdwp:<pid>: not implemented
        // track-jdwp: not implemented
        // sync: not implemented
        // reverse:<forward-command>: not implemented

        /// <include file='IAdbClient.xml' path='/IAdbClient/RunLogService/*'/>
        Task RunLogServiceAsync(DeviceData device, Action<LogEntry> messageSink, CancellationToken cancellationToken, params LogId[] logNames);

        /// <include file='IAdbClient.xml' path='/IAdbClient/Reboot/*'/>
        void Reboot(string into, DeviceData device);

        /// <include file='IAdbClient.xml' path='/IAdbClient/Connect/*'/>
        void Connect(DnsEndPoint endpoint);

        /// <summary>
        /// Disconnects a remote device from this local ADB server.
        /// </summary>
        /// <param name="endpoint">
        /// The endpoint of the remote device to disconnect.
        /// </param>
        void Disconnect(DnsEndPoint endpoint);

        /// <include file='IAdbClient.xml' path='/IAdbClient/SetDevice/*'/>
        void SetDevice(IAdbSocket socket, DeviceData device);

        /// <summary>
        /// Restarts the ADB daemon running on the device with root privileges.
        /// </summary>
        /// <param name="device">
        /// The device on which to restart ADB with root privileges.
        /// </param>
        void Root(DeviceData device);

        /// <summary>
        /// Restarts the ADB daemon running on the device without root privileges.
        /// </summary>
        /// <param name="device">
        /// The device on which to restart ADB without root privileges.
        /// </param>
        void Unroot(DeviceData device);

        /// Installs an Android application on an device.
        /// </summary>
        /// <param name="device">
        /// The device on which to install the application.
        /// </param>
        /// <param name="apk">
        /// A <see cref="Stream"/> which represents the application to install.
        /// </param>
        /// <param name="arguments">
        /// The arguments to pass to <c>adb install</c>.
        /// </param>
        void Install(DeviceData device, Stream apk, params string[] arguments);

        /// <summary>
        /// Lists all features supported by the current device.
        /// </summary>
        /// <param name="device">
        /// The device for which to get the list of features supported.
        /// </param>
        /// <returns>
        /// A list of all features supported by the current device.
        /// </returns>
        List<string> GetFeatureSet(DeviceData device);
    }
}
