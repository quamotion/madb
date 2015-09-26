using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using MoreLinq;

namespace Managed.Adb
{
    /// <summary>
    /// A Device monitor. This connects to the Android Debug Bridge and get device and
    /// debuggable process information from it.
    /// </summary>
    public class DeviceMonitor
    {
        /// <summary>
        /// Logging tag
        /// </summary>
        private const string TAG = "DeviceMonitor";

        /// <summary>
        /// 
        /// </summary>
        private byte[] LengthBuffer = null;
        /// <summary>
        /// 
        /// </summary>
        private byte[] LengthBuffer2 = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceMonitor"/> class.
        /// </summary>
        /// <param name="bridge">The bridge.</param>
        public DeviceMonitor(AndroidDebugBridge bridge)
        {
            this.Server = bridge;
            this.Devices = new List<Device>();
            this.DebuggerPorts = new List<int>();
            this.ClientsToReopen = new Dictionary<IClient, int>();
            this.DebuggerPorts.Add(DdmPreferences.DebugPortBase);
            this.LengthBuffer = new byte[4];
            this.LengthBuffer2 = new byte[4];
        }

        /// <summary>
        /// Gets the devices.
        /// </summary>
        public IList<Device> Devices { get; private set; }
        /// <summary>
        /// Gets the debugger ports.
        /// </summary>
        public IList<int> DebuggerPorts { get; private set; }
        /// <summary>
        /// Gets the clients to reopen.
        /// </summary>
        public Dictionary<IClient, int> ClientsToReopen { get; private set; }
        /// <summary>
        /// Gets the server.
        /// </summary>
        public AndroidDebugBridge Server { get; private set; }
        /// <summary>
        /// Gets a value indicating whether this instance is monitoring.
        /// </summary>
        /// <value>
        /// 	<see langword="true"/> if this instance is monitoring; otherwise, <see langword="false"/>.
        /// </value>
        public bool IsMonitoring { get; private set; }
        /// <summary>
        /// Gets a value indicating whether this instance is running.
        /// </summary>
        /// <value>
        /// 	<see langword="true"/> if this instance is running; otherwise, <see langword="false"/>.
        /// </value>
        public bool IsRunning { get; private set; }
        /// <summary>
        /// Gets the connection attempt count.
        /// </summary>
        public int ConnectionAttemptCount { get; private set; }
        /// <summary>
        /// Gets the restart attempt count.
        /// </summary>
        public int RestartAttemptCount { get; private set; }
        /// <summary>
        /// Gets a value indicating whether this instance has initial device list.
        /// </summary>
        /// <value>
        /// 	<see langword="true"/> if this instance has initial device list; otherwise, <see langword="false"/>.
        /// </value>
        public bool HasInitialDeviceList { get; private set; }
        /// <summary>
        /// Gets or sets the main adb connection.
        /// </summary>
        /// <value>
        /// The main adb connection.
        /// </value>
        private Socket MainAdbConnection { get; set; }

        /// <summary>
        /// Adds the client to drop and reopen.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="port">The port.</param>
        public void AddClientToDropAndReopen(IClient client, int port)
        {
            lock (this.ClientsToReopen)
            {
                Log.d(TAG, "Adding {0} to list of client to reopen ({1})", client, port);
                if (!this.ClientsToReopen.ContainsKey(client))
                {
                    this.ClientsToReopen.Add(client, port);
                }
            }
        }

        /// <summary>
        /// Starts the monitoring
        /// </summary>
        public void Start()
        {
            Thread t = new Thread(new ThreadStart(this.DeviceMonitorLoop));
            t.Name = "Device List Monitor";
            t.Start();
        }

        /// <summary>
        /// Stops the monitoring
        /// </summary>
        public void Stop()
        {
            this.IsRunning = false;

            // wakeup the main loop thread by closing the main connection to adb.
            try
            {
                if (this.MainAdbConnection != null)
                {
                    this.MainAdbConnection.Close();
                }
            }
            catch (IOException)
            {
            }

            // wake up the secondary loop by closing the selector.
            /*if ( Selector != null ) {
                Selector.WakeUp ( );
            }*/
        }

        /// <summary>
        /// Monitors the devices. This connects to the Debug Bridge
        /// </summary>
        private void DeviceMonitorLoop()
        {
            this.IsRunning = true;
            do
            {
                try
                {
                    if (this.MainAdbConnection == null)
                    {
                        Log.d(TAG, "Opening adb connection");
                        this.MainAdbConnection = this.OpenAdbConnection();

                        if (this.MainAdbConnection == null)
                        {
                            this.ConnectionAttemptCount++;
                            Log.i(TAG, "Connection attempts: {0}", this.ConnectionAttemptCount);

                            if (this.ConnectionAttemptCount > 10)
                            {
                                if (this.Server.Start() == false)
                                {
                                    this.RestartAttemptCount++;
                                    Log.e(TAG, "adb restart attempts: {0}", this.RestartAttemptCount);
                                }
                                else
                                {
                                    this.RestartAttemptCount = 0;
                                }
                            }
                            this.WaitBeforeContinue();
                        }
                        else
                        {
                            Log.d(TAG, "Connected to adb for device monitoring");
                            this.ConnectionAttemptCount = 0;
                        }
                    }
                    if (this.MainAdbConnection != null && !this.IsMonitoring && this.MainAdbConnection.Connected)
                    {
                        this.IsMonitoring = this.SendDeviceListMonitoringRequest();
                    }

                    if (this.IsMonitoring)
                    {
                        // read the length of the incoming message
                        int length = this.ReadLength(this.MainAdbConnection, this.LengthBuffer);

                        if (length >= 0)
                        {
                            // read the incoming message
                            this.ProcessIncomingDeviceData(length);

                            // flag the fact that we have build the list at least once.
                            this.HasInitialDeviceList = true;
                        }
                    }
                }
                catch (IOException ioe)
                {
                    if (!this.IsRunning)
                    {
                        Log.e(TAG, "Adb connection Error: ", ioe);
                        this.IsMonitoring = false;
                        if (this.MainAdbConnection != null)
                        {
                            try
                            {
                                this.MainAdbConnection.Close();
                            }
                            catch (IOException)
                            {
                                // we can safely ignore that one.
                            }
                            this.MainAdbConnection = null;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.w(TAG, ex);
                }
            }
            while (this.IsRunning);
        }

        /// <summary>
        /// Waits before continuing.
        /// </summary>
        private void WaitBeforeContinue()
        {
            Thread.Sleep(1000);
        }

        /// <summary>
        /// Sends the device list monitoring request.
        /// </summary>
        /// <returns></returns>
        private bool SendDeviceListMonitoringRequest()
        {
            byte[] request = AdbHelper.Instance.FormAdbRequest("host:track-devices");

            if (AdbHelper.Instance.Write(this.MainAdbConnection, request) == false)
            {
                Log.e(TAG, "Sending Tracking request failed!");
                this.MainAdbConnection.Close();
                throw new IOException("Sending Tracking request failed!");
            }

            AdbResponse resp = AdbHelper.Instance.ReadAdbResponse(this.MainAdbConnection, false /* readDiagString */);

            if (!resp.IOSuccess)
            {
                Log.e(TAG, "Failed to read the adb response!");
                this.MainAdbConnection.Close();
                throw new IOException("Failed to read the adb response!");
            }

            if (!resp.Okay)
            {
                // request was refused by adb!
                Log.e(TAG, "adb refused request: {0}", resp.Message);
            }

            return resp.Okay;
        }

        /// <summary>
        /// Processes the incoming device data.
        /// </summary>
        /// <param name="length">The length.</param>
        private void ProcessIncomingDeviceData(int length)
        {
            List<Device> list = new List<Device>();

            if (length > 0)
            {
                byte[] buffer = new byte[length];
                string result = this.Read(this.MainAdbConnection, buffer);

                string[] devices = result.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                devices.ForEach(d =>
                {
                    try
                    {
                        var dv = Device.CreateFromAdbData(d);
                        if (dv != null)
                        {
                            list.Add(dv);
                        }
                    }
                    catch (ArgumentException ae)
                    {
                        Log.e(TAG, ae);
                    }
                });
            }

            // now merge the new devices with the old ones.
            this.UpdateDevices(list);
        }

        private void UpdateDevices(List<Device> list)
        {
            // because we are going to call mServer.deviceDisconnected which will acquire this lock
            // we lock it first, so that the AndroidDebugBridge lock is always locked first.
            lock (AndroidDebugBridge.GetLock())
            {
                lock (this.Devices)
                {
                    // For each device in the current list, we look for a matching the new list.
                    // * if we find it, we update the current object with whatever new information
                    //   there is
                    //   (mostly state change, if the device becomes ready, we query for build info).
                    //   We also remove the device from the new list to mark it as "processed"
                    // * if we do not find it, we remove it from the current list.
                    // Once this is done, the new list contains device we aren't monitoring yet, so we
                    // add them to the list, and start monitoring them.

                    for (int d = 0; d < this.Devices.Count;)
                    {
                        Device device = this.Devices[d];

                        // look for a similar device in the new list.
                        int count = list.Count;
                        bool foundMatch = false;
                        for (int dd = 0; dd < count; dd++)
                        {
                            Device newDevice = list[dd];
                            // see if it matches in id and serial number.
                            if (string.Compare(newDevice.SerialNumber, device.SerialNumber, true) == 0)
                            {
                                foundMatch = true;

                                // update the state if needed.
                                if (device.State != newDevice.State)
                                {
                                    device.State = newDevice.State;
                                    device.OnStateChanged(EventArgs.Empty);

                                    // if the device just got ready/online, we need to start
                                    // monitoring it.
                                    if (device.IsOnline)
                                    {
                                        if (AndroidDebugBridge.ClientSupport)
                                        {
                                            if (this.StartMonitoringDevice(device) == false)
                                            {
                                                Log.e(TAG, "Failed to start monitoring {0}", device.SerialNumber);
                                            }
                                        }

                                        if (device.Properties.Count == 0)
                                        {
                                            this.QueryNewDeviceForInfo(device);
                                        }
                                    }
                                }

                                // remove the new device from the list since it's been used
                                list.RemoveAt(dd);
                                break;
                            }
                        }

                        if (foundMatch == false)
                        {
                            // the device is gone, we need to remove it, and keep current index
                            // to process the next one.
                            this.RemoveDevice(device);
                            device.State = DeviceState.Offline;
                            device.OnStateChanged(EventArgs.Empty);
                            this.Server.OnDeviceDisconnected(new DeviceEventArgs(device));
                        }
                        else
                        {
                            // process the next one
                            d++;
                        }
                    }

                    // at this point we should still have some new devices in newList, so we
                    // process them.
                    foreach (Device newDevice in list)
                    {
                        // add them to the list
                        this.Devices.Add(newDevice);
                        if (this.Server != null)
                        {
                            newDevice.State = DeviceState.Online;
                            newDevice.OnStateChanged(EventArgs.Empty);
                            this.Server.OnDeviceConnected(new DeviceEventArgs(newDevice));
                        }

                        // start monitoring them.
                        if (AndroidDebugBridge.ClientSupport)
                        {
                            if (newDevice.IsOnline)
                            {
                                this.StartMonitoringDevice(newDevice);
                            }
                        }

                        // look for their build info.
                        if (newDevice.IsOnline)
                        {
                            this.QueryNewDeviceForInfo(newDevice);
                        }
                    }
                }
            }
            list.Clear();
        }

        /// <summary>
        /// Removes the device.
        /// </summary>
        /// <param name="device">The device.</param>
        private void RemoveDevice(Device device)
        {
            //device.Clients.Clear ( );
            this.Devices.Remove(device);

            Socket channel = device.ClientMonitoringSocket;
            if (channel != null)
            {
                try
                {
                    channel.Close();
                }
                catch (IOException)
                {
                    // doesn't really matter if the close fails.
                }
            }
        }

        private void QueryNewDeviceForInfo(Device device)
        {
            // TODO: do this in a separate thread.
            try
            {
                // first get the list of properties.
                if (device.State != DeviceState.Offline && device.State != DeviceState.Unknown)
                {
                    // get environment variables
                    this.QueryNewDeviceForEnvironmentVariables(device);
                    // instead of getting the 3 hard coded ones, we use mount command and get them all...
                    // if that fails, then it automatically falls back to the hard coded ones.
                    this.QueryNewDeviceForMountingPoint(device);

                    // now get the emulator Virtual Device name (if applicable).
                    if (device.IsEmulator)
                    {
                        /*EmulatorConsole console = EmulatorConsole.getConsole ( device );
                        if ( console != null ) {
                            device.AvdName = console.AvdName;
                        }*/
                    }
                }
            }
            catch (IOException)
            {
                // if we can't get the build info, it doesn't matter too much
            }
        }

        private void QueryNewDeviceForEnvironmentVariables(Device device)
        {
            try
            {
                if (device.State != DeviceState.Offline && device.State != DeviceState.Unknown)
                {
                    device.RefreshEnvironmentVariables();
                }
            }
            catch (IOException)
            {
                // if we can't get the build info, it doesn't matter too much
            }
        }

        private void QueryNewDeviceForMountingPoint(Device device)
        {
            try
            {
                if (device.State != DeviceState.Offline && device.State != DeviceState.Unknown)
                {
                    device.RefreshMountPoints();
                }
            }
            catch (IOException)
            {
                // if we can't get the build info, it doesn't matter too much
            }
        }

        private bool StartMonitoringDevice(Device device)
        {
            Socket socket = this.OpenAdbConnection();

            if (socket != null)
            {
                try
                {
                    bool result = this.SendDeviceMonitoringRequest(socket, device);
                    if (result)
                    {

                        /*if ( Selector == null ) {
                            StartDeviceMonitorThread ( );
                        }*/

                        device.ClientMonitoringSocket = socket;

                        lock (this.Devices)
                        {
                            // always wakeup before doing the register. The synchronized block
                            // ensure that the selector won't select() before the end of this block.
                            // @see deviceClientMonitorLoop
                            //Selector.wakeup ( );

                            socket.Blocking = true;
                            //socket.register(mSelector, SelectionKey.OP_READ, device);
                        }

                        return true;
                    }
                }
                catch (IOException e)
                {
                    try
                    {
                        // attempt to close the socket if needed.
                        socket.Close();
                    }
                    catch (IOException e1)
                    {
                        // we can ignore that one. It may already have been closed.
                    }
                    Log.d(TAG, "Connection Failure when starting to monitor device '{0}' : {1}", device, e.Message);
                }
            }

            return false;
        }

        private void StartDeviceMonitorThread()
        {
            //Selector = Selector.Open();
            Thread t = new Thread(new ThreadStart(this.DeviceClientMonitorLoop));
            t.Name = "Device Client Monitor";
            t.Start();
        }

        private void DeviceClientMonitorLoop()
        {
            do
            {
                try
                {
                    // This synchronized block stops us from doing the select() if a new
                    // Device is being added.
                    // @see startMonitoringDevice()
                    lock (this.Devices)
                    {
                    }

                    //int count = Selector.Select ( );
                    int count = 0;

                    if (!this.IsRunning)
                    {
                        return;
                    }

                    lock (this.ClientsToReopen)
                    {
                        if (this.ClientsToReopen.Count > 0)
                        {
                            Dictionary<IClient, int>.KeyCollection clients = this.ClientsToReopen.Keys;
                            MonitorThread monitorThread = MonitorThread.Instance;

                            foreach (IClient client in clients)
                            {
                                Device device = client.DeviceImplementation;
                                int pid = client.ClientData.Pid;

                                monitorThread.DropClient(client, false /* notify */);

                                // This is kinda bad, but if we don't wait a bit, the client
                                // will never answer the second handshake!
                                this.WaitBeforeContinue();

                                int port = this.ClientsToReopen[client];

                                if (port == DebugPortManager.NO_STATIC_PORT)
                                {
                                    port = this.GetNextDebuggerPort();
                                }
                                Log.d("DeviceMonitor", "Reopening " + client);
                                this.OpenClient(device, pid, port, monitorThread);
                                device.OnClientListChanged(EventArgs.Empty);
                            }

                            this.ClientsToReopen.Clear();
                        }
                    }

                    if (count == 0)
                    {
                        continue;
                    }

                    /*List<SelectionKey> keys = Selector.selectedKeys();
                    List<SelectionKey>.Enumerator iter = keys.GetEnumerator();

                    while (iter.MoveNext()) {
                            SelectionKey key = iter.next();
                            iter.remove();

                            if (key.isValid() && key.isReadable()) {
                                    Object attachment = key.attachment();

                                    if (attachment instanceof Device) {
                                            Device device = (Device)attachment;

                                            SocketChannel socket = device.getClientMonitoringSocket();

                                            if (socket != null) {
                                                    try {
                                                            int length = readLength(socket, mLengthBuffer2);

                                                            processIncomingJdwpData(device, socket, length);
                                                    } catch (IOException ioe) {
                                                            Log.d("DeviceMonitor",
                                                                            "Error reading jdwp list: " + ioe.getMessage());
                                                            socket.close();

                                                            // restart the monitoring of that device
                                                            synchronized (mDevices) {
                                                                    if (mDevices.contains(device)) {
                                                                            Log.d("DeviceMonitor",
                                                                                            "Restarting monitoring service for " + device);
                                                                            startMonitoringDevice(device);
                                                                    }
                                                            }
                                                    }
                                            }
                                    }
                            }
                    }*/
                }
                catch (IOException e)
                {
                    if (!this.IsRunning)
                    {

                    }
                }

            }
            while (this.IsRunning);
        }

        /// <summary>
        /// Sends the device monitoring request.
        /// </summary>
        /// <param name="socket">The socket.</param>
        /// <param name="device">The device.</param>
        /// <returns></returns>
        private bool SendDeviceMonitoringRequest(Socket socket, Device device)
        {
            AdbHelper.Instance.SetDevice(socket, device);
            byte[] request = AdbHelper.Instance.FormAdbRequest("track-jdwp");
            if (!AdbHelper.Instance.Write(socket, request))
            {
                Log.e(TAG, "Sending jdwp tracking request failed!");
                socket.Close();
                throw new IOException();
            }
            AdbResponse resp = AdbHelper.Instance.ReadAdbResponse(socket, false /* readDiagString */);
            if (resp.IOSuccess == false)
            {
                Log.e(TAG, "Failed to read the adb response!");
                socket.Close();
                throw new IOException();
            }

            if (resp.Okay == false)
            {
                // request was refused by adb!
                Log.e(TAG, "adb refused request: " + resp.Message);
            }

            return resp.Okay;
        }

        /// <summary>
        /// Opens the client.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="pid">The pid.</param>
        /// <param name="port">The port.</param>
        /// <param name="monitorThread">The monitor thread.</param>
        private void OpenClient(Device device, int pid, int port, MonitorThread monitorThread)
        {

            Socket clientSocket;
            try
            {
                clientSocket = AdbHelper.Instance.CreatePassThroughConnection(AndroidDebugBridge.SocketAddress, device, pid);

                clientSocket.Blocking = true;
            }
            catch (IOException ioe)
            {
                Log.w(TAG, "Failed to connect to client {0}: {1}'", pid, ioe.Message);
                return;
            }

            this.CreateClient(device, pid, clientSocket, port, monitorThread);
        }

        /// <summary>
        /// Creates the client.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="pid">The pid.</param>
        /// <param name="socket">The socket.</param>
        /// <param name="debuggerPort">The debugger port.</param>
        /// <param name="monitorThread">The monitor thread.</param>
        private void CreateClient(Device device, int pid, Socket socket, int debuggerPort, MonitorThread monitorThread)
        {

            /*
             * Successfully connected to something. Create a Client object, add
             * it to the list, and initiate the JDWP handshake.
             */

            Client client = new Client(device, socket, pid);

            if (client.SendHandshake())
            {
                try
                {
                    if (AndroidDebugBridge.ClientSupport)
                    {
                        client.ListenForDebugger(debuggerPort);
                    }
                }
                catch (IOException)
                {
                    client.ClientData.DebuggerConnectionStatus = Managed.Adb.ClientData.DebuggerStatus.ERROR;
                    Log.e("ddms", "Can't bind to local {0} for debugger", debuggerPort);
                    // oh well
                }

                client.RequestAllocationStatus();
            }
            else
            {
                Log.e("ddms", "Handshake with {0} failed!", client);
                /*
                 * The handshake send failed. We could remove it now, but if the
                 * failure is "permanent" we'll just keep banging on it and
                 * getting the same result. Keep it in the list with its "error"
                 * state so we don't try to reopen it.
                 */
            }

            if (client.IsValid)
            {
                device.Clients.Add(client);
                monitorThread.Clients.Add(client);
            }
            else
            {
                client = null;
            }
        }

        /// <summary>
        /// Gets the next debugger port.
        /// </summary>
        /// <returns></returns>
        private int GetNextDebuggerPort()
        {
            // get the first port and remove it
            lock (this.DebuggerPorts)
            {
                if (this.DebuggerPorts.Count > 0)
                {
                    int port = this.DebuggerPorts[0];

                    // remove it.
                    this.DebuggerPorts.RemoveAt(0);

                    // if there's nothing left, add the next port to the list
                    if (this.DebuggerPorts.Count == 0)
                    {
                        this.DebuggerPorts.Add(port + 1);
                    }

                    return port;
                }
            }

            return -1;
        }

        /// <summary>
        /// Adds the port to available list.
        /// </summary>
        /// <param name="port">The port.</param>
        public void AddPortToAvailableList(int port)
        {
            if (port > 0)
            {
                lock (this.DebuggerPorts)
                {
                    // because there could be case where clients are closed twice, we have to make
                    // sure the port number is not already in the list.
                    if (this.DebuggerPorts.IndexOf(port) == -1)
                    {
                        // add the port to the list while keeping it sorted. It's not like there's
                        // going to be tons of objects so we do it linearly.
                        int count = this.DebuggerPorts.Count;
                        for (int i = 0; i < count; i++)
                        {
                            if (port < this.DebuggerPorts[i])
                            {
                                this.DebuggerPorts.Insert(i, port);
                                break;
                            }
                        }
                        // TODO: check if we can compact the end of the list.
                    }
                }
            }
        }

        /// <summary>
        /// Reads the length of the next message from a socket.
        /// </summary>
        /// <param name="socket">The Socket to read from.</param>
        /// <param name="buffer"></param>
        /// <returns>the length, or 0 (zero) if no data is available from the socket.</returns>
        private int ReadLength(Socket socket, byte[] buffer)
        {
            string msg = this.Read(socket, buffer);
            if (msg != null)
            {
                try
                {
                    int len = int.Parse(msg, System.Globalization.NumberStyles.HexNumber);
                    return len;
                }
                catch (FormatException nfe)
                {
                    // we'll throw an exception below.
                }
            }
            //throw new IOException ( "unable to read data length" );
            // we receive something we can't read. It's better to reset the connection at this point.
            return -1;
        }

        /// <summary>
        /// Reads the specified socket.
        /// </summary>
        /// <param name="socket">The socket.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        private string Read(Socket socket, byte[] data)
        {
            int count = -1;
            int totalRead = 0;

            while (count != 0 && totalRead < data.Length)
            {
                try
                {
                    int left = data.Length - totalRead;
                    int buflen = left < socket.ReceiveBufferSize ? left : socket.ReceiveBufferSize;

                    byte[] buffer = new byte[buflen];
                    socket.ReceiveBufferSize = buffer.Length;
                    count = socket.Receive(buffer, buflen, SocketFlags.None);
                    if (count < 0)
                    {
                        throw new IOException("EOF");
                    }
                    else if (count == 0)
                    {
                    }
                    else
                    {
                        Array.Copy(buffer, 0, data, totalRead, count);
                        totalRead += count;
                    }
                }
                catch (SocketException sex)
                {
                    if (!this.IsRunning)
                    {
                        return string.Empty;
                    }
                    else
                    {
                        if (sex.Message.Contains("connection was aborted"))
                        {
                            // ignore this?
                            return string.Empty;
                        }
                        else
                        {
                            throw new IOException(string.Format("No Data to read: {0}", sex.Message));
                        }
                    }
                }
            }

            return data.GetString(AdbHelper.DEFAULT_ENCODING);
        }

        /// <summary>
        /// Attempts to connect to the debug bridge server.
        /// </summary>
        /// <returns>a connect socket if success, null otherwise</returns>
        private Socket OpenAdbConnection()
        {
            Log.d(TAG, "Connecting to adb for Device List Monitoring...");
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                socket.Connect(AndroidDebugBridge.SocketAddress);
                socket.NoDelay = true;
            }
            catch (IOException e)
            {
                Log.w(TAG, e);
            }

            return socket;
        }
    }
}
