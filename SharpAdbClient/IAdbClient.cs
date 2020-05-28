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

        /// <summary>
        /// Ask the ADB server for its internal version number.
        /// </summary>
        /// <returns>
        /// The ADB version number.
        /// </returns>
        int GetAdbVersion();

        /// <summary>
        /// Ask the ADB server to quit immediately. This is used when the
        /// ADB client detects that an obsolete server is running after an
        /// upgrade.
        /// </summary>
        void KillAdb();

        /// <summary>
        /// Gets the devices that are available for communication.
        /// </summary>
        /// <returns>
        /// A list of devices that are connected.
        /// </returns>
        /// <example>
        /// <para>
        /// The following example list all Android devices that are currently connected to this PC:
        /// </para>
        /// <code>
        /// var devices = AdbClient.Instance.GetDevices();
        /// 
        /// foreach(var device in devices)
        /// {
        ///     Console.WriteLine(device.Name);
        /// }
        /// </code>
        /// </example>
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

        /// <summary>
        /// Asks the ADB server to reverse forward local connections from <paramref name="remote"/>
        /// to the <paramref name="local"/> address on the <paramref name="device"/>.
        /// </summary>
        /// <param name="device">
        /// The device to which to reverse forward the connections.
        /// </param>
        /// <param name="remote">
        /// <para>
        /// The remote address to reverse forward. This value can be in one of:
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
        /// <param name="local">
        /// <para>
        /// The local address to reverse forward. This value can be in one of:
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
        /// <param name="allowRebind">
        /// If set to <see langword="true"/>, the request will fail if if the specified socket is already bound through a previous reverse command.
        /// </param>
        /// <returns>
        /// If your requested to start reverse to remote port TCP:0, the port number of the TCP port
        /// which has been opened. In all other cases, <c>0</c>.
        /// </returns>
        int CreateReverseForward(DeviceData device, string remote, string local, bool allowRebind);


        /// <summary>
        /// Remove a reverse port forwarding between a remote and a local port.
        /// </summary>
        /// <param name="device">
        /// The device on which to remove the reverse port forwarding
        /// <param name="remote">
        /// Specification of the remote that was forwarded
        /// </param>
        void RemoveReverseForward(DeviceData device, string remote);

        /// <summary>
        /// Removes all reverse forwards for a given device.
        /// </summary>
        /// <param name="device">
        /// The device on which to remove all reverse port forwarding
        /// </param>
        void RemoveAllReverseForwards(DeviceData device);

        /// <summary>
        /// Remove a port forwarding between a local and a remote port.
        /// </summary>
        /// <param name="device">
        /// The device on which to remove the port forwarding.
        /// </param>
        /// <param name="localPort">
        /// Specification of the local port that was forwarded.
        /// </param>
        void RemoveForward(DeviceData device, int localPort);

        /// <summary>
        /// Removes all forwards for a given device.
        /// </summary>
        /// <param name="device">
        /// The device on which to remove the port forwarding.
        /// </param>
        void RemoveAllForwards(DeviceData device);

        /// <summary>
        /// List all existing forward connections from this server.
        /// </summary>
        /// <param name="device">
        /// The device for which to list the existing forward connections.
        /// </param>
        /// <returns>
        /// A <see cref="ForwardData"/> entry for each existing forward connection.
        /// </returns>
        IEnumerable<ForwardData> ListForward(DeviceData device);

        /// <summary>
        /// List all existing reverse forward connections from this server.
        /// </summary>
        /// <param name = "device" >
        /// The device for which to list the existing reverse foward connections.
        /// </param>
        /// <returns>
        /// A<see cref="ForwardData"/> entry for each existing reverse forward connection.
        /// </returns>
        IEnumerable<ForwardData> ListReverseForward(DeviceData device);

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
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation.
        /// </returns>
        Task ExecuteRemoteCommandAsync(string command, DeviceData device, IShellOutputReceiver receiver, CancellationToken cancellationToken);

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
        /// <param name="encoding">
        /// The encoding to use when parsing the command output.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation.
        /// </returns>
        Task ExecuteRemoteCommandAsync(string command, DeviceData device, IShellOutputReceiver receiver, Encoding encoding, CancellationToken cancellationToken);

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

        /// <summary>
        /// Gets the frame buffer from the specified end point.
        /// </summary>
        /// <param name="device">
        /// The device for which to get the framebuffer.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> that can be used to cancel the asynchronous task.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which returns the raw frame buffer.
        /// </returns>
        /// <exception cref="AdbException">
        /// failed asking for frame buffer
        /// </exception>
        /// <exception cref="AdbException">
        /// failed nudging
        /// </exception>
        Task<Image> GetFrameBufferAsync(DeviceData device, CancellationToken cancellationToken);

        // jdwp:<pid>: not implemented
        // track-jdwp: not implemented
        // sync: not implemented
        // reverse:<forward-command>: not implemented

        /// <summary>
        /// Asynchronously runs the event log service on a device.
        /// </summary>
        /// <param name="device">
        /// The device on which to run the event log service.
        /// </param>
        /// <param name="messageSink">
        /// A callback which will receive the event log messages as they are received.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the event log service. Use this
        /// to stop reading from the event log.
        /// </param>
        /// <param name="logNames">
        /// Optionally, the names of the logs to receive.
        /// </param>
        /// <returns>
        /// An <see cref="Task"/> which represents the asynchronous operation.
        /// </returns>
        Task RunLogServiceAsync(DeviceData device, Action<LogEntry> messageSink, CancellationToken cancellationToken, params LogId[] logNames);

        /// <summary>
        /// Reboots the specified device in to the specified mode.
        /// </summary>
        /// <param name="into">
        /// The mode into which to reboot the device.
        /// </param>
        /// <param name="device">
        /// The device to reboot.
        /// </param>
        void Reboot(string into, DeviceData device);

        /// <summary>
        /// Connect to a device via TCP/IP.
        /// </summary>
        /// <param name="endpoint">
        /// The DNS endpoint at which the <c>adb</c> server on the device is running.
        /// </param>
        void Connect(DnsEndPoint endpoint);

        /// <summary>
        /// Disconnects a remote device from this local ADB server.
        /// </summary>
        /// <param name="endpoint">
        /// The endpoint of the remote device to disconnect.
        /// </param>
        void Disconnect(DnsEndPoint endpoint);

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
