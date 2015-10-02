namespace Managed.Adb
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;

    /// <summary>
    /// The android debug bridge
    /// </summary>
    public sealed class AndroidDebugBridge
    {
        /// <summary>
        /// Logging tag
        /// </summary>
        private const string TAG = "AndroidDebugBridge";

        /// <summary>
        /// Occurs when [bridge changed].
        /// </summary>
        /// <ignore>true</ignore>
        public event EventHandler<AndroidDebugBridgeEventArgs> BridgeChanged;

        /// <summary>
        /// Occurs when [device changed].
        /// </summary>
        /// <ignore>true</ignore>
        public event EventHandler<DeviceEventArgs> DeviceChanged;

        /// <summary>
        /// Occurs when [device connected].
        /// </summary>
        /// <ignore>true</ignore>
        public event EventHandler<DeviceEventArgs> DeviceConnected;

        /// <summary>
        /// Occurs when [device disconnected].
        /// </summary>
        /// <ignore>true</ignore>
        public event EventHandler<DeviceEventArgs> DeviceDisconnected;

        /// <summary>
        /// Occurs when [client changed].
        /// </summary>
        /// <ignore>true</ignore>
        public event EventHandler<ClientEventArgs> ClientChanged;

        /*
         * Minimum and maximum version of adb supported. This correspond to
         * ADB_SERVER_VERSION found in //device/tools/adb/adb.h
         */

        /// <summary>
        ///
        /// </summary>
        private const int ADB_VERSION_MICRO_MIN = 20;

        /// <summary>
        ///
        /// </summary>
        private const int ADB_VERSION_MICRO_MAX = -1;

        /// <summary>
        /// The regex pattern for getting the adb version
        /// </summary>
        private const string ADB_VERSION_PATTERN = "^.*(\\d+)\\.(\\d+)\\.(\\d+)$";

#if LINUX
		/// <summary>
		/// The ADB executive
		/// </summary>
		public const String ADB = "adb";
		/// <summary>
		/// The DDMS executive
		/// </summary>
		public const String DDMS = "monitor";
		/// <summary>
		/// The hierarchy viewer
		/// </summary>
		public const String HIERARCHYVIEWER = "hierarchyviewer";
		/// <summary>
		/// The AAPT executive
		/// </summary>
		public const String AAPT = "aapt";
#else
        /// <summary>
        /// The ADB executive
        /// </summary>
        public const string ADB = "adb.exe";

        /// <summary>
        /// The DDMS executive
        /// </summary>
        public const string DDMS = "monitor.bat";

        /// <summary>
        /// The hierarchy viewer
        /// </summary>
        public const string HIERARCHYVIEWER = "hierarchyviewer.bat";

        /// <summary>
        /// The AAPT executive
        /// </summary>
        public const string AAPT = "aapt.exe";

#endif

        // Where to find the ADB bridge.

        /// <summary>
        /// The default ADB bridge port
        /// </summary>
        public const int ADB_PORT = 5037;

        #region statics

        /// <summary>
        ///
        /// </summary>
        private static AndroidDebugBridge instance;

        /// <summary>
        ///
        /// </summary>
        private static bool clientSupport;

        /// <summary>
        /// Gets or sets the socket address.
        /// </summary>
        /// <value>The socket address.</value>
        public static IPEndPoint SocketAddress { get; private set; }

        /// <summary>
        /// Gets or sets the host address.
        /// </summary>
        /// <value>The host address.</value>
        public static IPAddress HostAddress { get; private set; }

        /// <summary>
        /// Initializes the <see cref="AndroidDebugBridge"/> class.
        /// </summary>
        static AndroidDebugBridge()
        {
            // built-in local address/port for ADB.
            try
            {
                HostAddress = IPAddress.Loopback;

                SocketAddress = new IPEndPoint(HostAddress, ADB_PORT);
            }
            catch (ArgumentOutOfRangeException)
            {
            }
        }

        /*
         * Initializes the <code>ddm</code> library.
         * <p/>This must be called once <b>before</b> any call to
         * {@link #createBridge(String, boolean)}.
         * <p>The library can be initialized in 2 ways:
         * <ul>
         * <li>Mode 1: <var>clientSupport</var> == <see langword="true"/>.<br>The library monitors the
         * devices and the applications running on them. It will connect to each application, as a
         * debugger of sort, to be able to interact with them through JDWP packets.</li>
         * <li>Mode 2: <var>clientSupport</var> == <code>false</code>.<br>The library only monitors
         * devices. The applications are left untouched, letting other tools built on
         * <code>ddmlib</code> to connect a debugger to them.</li>
         * </ul>
         * <p/><b>Only one tool can run in mode 1 at the same time.</b>
         * <p/>Note that mode 1 does not prevent debugging of applications running on devices. Mode 1
         * lets debuggers connect to <code>ddmlib</code> which acts as a proxy between the debuggers and
         * the applications to debug. See {@link Client#getDebuggerListenPort()}.
         * <p/>The preferences of <code>ddmlib</code> should also be initialized with whatever default
         * values were changed from the default values.
         * <p/>When the application quits, {@link #terminate()} should be called.
         * @param clientSupport Indicates whether the library should enable the monitoring and
         * interaction with applications running on the devices.
         * @see AndroidDebugBridge#createBridge(String, boolean)
         * @see DdmPreferences
         */

        /// <summary>
        /// Initializes the <code>ddm</code> library.
        /// <para>This must be called once <b>before</b> any call to CreateBridge.</para>
        /// </summary>
        /// <param name="clientSupport">if set to <see langword="true"/> [client support].</param>
        public static void Initialize(bool clientSupport)
        {
            ClientSupport = clientSupport;

            /*MonitorThread monitorThread = MonitorThread.createInstance ( );
            monitorThread.start ( );

            HandleHello.register ( monitorThread );
            HandleAppName.register ( monitorThread );
            HandleTest.register ( monitorThread );
            HandleThread.register ( monitorThread );
            HandleHeap.register ( monitorThread );
            HandleWait.register ( monitorThread );
            HandleProfiling.register ( monitorThread );*/
        }

        /// <summary>
        /// Terminates the ddm library. This must be called upon application termination.
        /// </summary>
        public static void Terminate()
        {
            // kill the monitoring services
            if (Instance != null && Instance.DeviceMonitor != null)
            {
                Instance.DeviceMonitor.Stop();
                Instance.DeviceMonitor = null;
            }

            /*MonitorThread monitorThread = MonitorThread.getInstance ( );
            if ( monitorThread != null ) {
                monitorThread.quit ( );
            }*/
        }

        /// <summary>
        /// Gets an instance of <see cref="AndroidDebugBridge"/>.
        /// </summary>
        /// <value>The instance.</value>
        public static AndroidDebugBridge Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = CreateBridge();
                }

                return instance;
            }
        }

        /// <summary>
        /// Gets an instance of <see cref="AndroidDebugBridge"/>.
        /// </summary>
        public static AndroidDebugBridge Bridge
        {
            get { return Instance; }
        }

        /// <summary>
        /// Gets a value indicating whether there is client support.
        /// </summary>
        /// <value>
        ///   <see langword="true"/> if there is client support; otherwise, <see langword="false"/>.
        /// </value>
        public static bool ClientSupport { get; private set; }

        /// <summary>
        /// Creates a {@link AndroidDebugBridge} that is not linked to any particular executable.
        /// This bridge will expect adb to be running. It will not be able to start/stop/restart</summary>
        /// adb.
        /// If a bridge has already been started, it is directly returned with no changes
        /// <returns></returns>
        public static AndroidDebugBridge CreateBridge()
        {
            if (instance != null)
            {
                return instance;
            }

            try
            {
                instance = new AndroidDebugBridge();
                instance.Start();
                instance.OnBridgeChanged(new AndroidDebugBridgeEventArgs(instance));
            }
            catch (ArgumentException)
            {
                instance.OnBridgeChanged(new AndroidDebugBridgeEventArgs(null));
                instance = null;
            }

            return instance;
        }

        /// <summary>
        /// Creates a new debug bridge from the location of the command line tool.
        /// </summary>
        /// <param name="osLocation">the location of the command line tool 'adb'</param>
        /// <param name="forceNewBridge">force creation of a new bridge even if one with the same location
        /// already exists.</param>
        /// <returns>a connected bridge.</returns>
        /// <remarks>Any existing server will be disconnected, unless the location is the same and
        /// <code>forceNewBridge</code> is set to false.
        /// </remarks>
        public static AndroidDebugBridge CreateBridge(string osLocation, bool forceNewBridge)
        {
            if (instance != null)
            {
                if (!string.IsNullOrEmpty(AdbOsLocation) && string.Compare(AdbOsLocation, osLocation, true) == 0 && !forceNewBridge)
                {
                    return instance;
                }
                else
                {
                    // stop the current server
                    Log.i(TAG, "Stopping Current Instance");
                    instance.Stop();
                }
            }

            try
            {
                instance = new AndroidDebugBridge(osLocation);
                instance.Start();
                instance.OnBridgeChanged(new AndroidDebugBridgeEventArgs(instance));
            }
            catch (ArgumentException)
            {
                instance.OnBridgeChanged(new AndroidDebugBridgeEventArgs(null));
                instance = null;
            }

            return instance;
        }

        /// <summary>
        /// Disconnects the current debug bridge, and destroy the object.
        /// </summary>
        /// <remarks>This also stops the current adb host server.</remarks>
        public static void DisconnectBridge()
        {
            if (instance != null)
            {
                instance.Stop();

                instance.OnBridgeChanged(new AndroidDebugBridgeEventArgs(null));
                instance = null;
            }
        }

        /// <summary>
        /// Gets the lock.
        /// </summary>
        /// <returns></returns>
        public static object GetLock()
        {
            return Instance;
        }

        #endregion

        #region constructors

        /// <summary>
        /// Creates a new bridge.
        /// </summary>
        /// <param name="osLocation">the location of the command line tool</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="FileNotFoundException"></exception>
        private AndroidDebugBridge(string osLocation)
        {
            if (string.IsNullOrEmpty(osLocation))
            {
                throw new ArgumentException();
            }

            if (!File.Exists(osLocation))
            {
                Log.e(TAG, string.Format("unable to locate adb in the specified location: {0}", osLocation));
                throw new FileNotFoundException("unable to locate adb in the specified location");
            }

            AdbOsLocation = osLocation;

            this.CheckAdbVersion();
        }

        /// <summary>
        /// Creates a new bridge not linked to any particular adb executable.
        /// </summary>
        private AndroidDebugBridge()
        {
        }

        #endregion

        #region Event "Raisers"

        /// <summary>
        /// Raises the <see cref="E:BridgeChanged"/> event.
        /// </summary>
        /// <param name="e">The <see cref="Managed.Adb.AndroidDebugBridgeEventArgs"/> instance containing the event data.</param>
        internal void OnBridgeChanged(AndroidDebugBridgeEventArgs e)
        {
            if (this.BridgeChanged != null)
            {
                this.BridgeChanged(this, e);
            }
        }

        /// <summary>
        /// Raises the <see cref="E:ClientChanged"/> event.
        /// </summary>
        /// <param name="e">The <see cref="Managed.Adb.ClientEventArgs"/> instance containing the event data.</param>
        internal void OnClientChanged(ClientEventArgs e)
        {
            if (this.ClientChanged != null)
            {
                this.ClientChanged(this, e);
            }
        }

        /// <summary>
        /// Raises the <see cref="E:DeviceChanged"/> event.
        /// </summary>
        /// <param name="e">The <see cref="Managed.Adb.DeviceEventArgs"/> instance containing the event data.</param>
        internal void OnDeviceChanged(DeviceEventArgs e)
        {
            if (this.DeviceChanged != null)
            {
                this.DeviceChanged(this, e);
            }
        }

        /// <summary>
        /// Raises the <see cref="E:DeviceConnected"/> event.
        /// </summary>
        /// <param name="e">The <see cref="Managed.Adb.DeviceEventArgs"/> instance containing the event data.</param>
        internal void OnDeviceConnected(DeviceEventArgs e)
        {
            if (this.DeviceConnected != null)
            {
                this.DeviceConnected(this, e);
            }
        }

        /// <summary>
        /// Raises the <see cref="E:DeviceDisconnected"/> event.
        /// </summary>
        /// <param name="e">The <see cref="Managed.Adb.DeviceEventArgs"/> instance containing the event data.</param>
        internal void OnDeviceDisconnected(DeviceEventArgs e)
        {
            if (this.DeviceDisconnected != null)
            {
                this.DeviceDisconnected(this, e);
            }
        }
        #endregion

        #region public methods

        /// <summary>
        /// Starts the debug bridge.
        /// </summary>
        /// <returns><see langword="true"/> if success.</returns>
        public bool Start()
        {
            if (string.IsNullOrEmpty(AdbOsLocation) || !this.VersionCheck || !this.StartAdb())
            {
                return false;
            }

            this.Started = true;

            // now that the bridge is connected, we start the underlying services.
            this.DeviceMonitor = new DeviceMonitor(this);
            this.DeviceMonitor.Start();

            return true;
        }

        /// <summary>
        /// Kills the debug bridge, and the adb host server.
        /// </summary>
        /// <returns><see langword="true"/> if success.</returns>
        public bool Stop()
        {
            // if we haven't started we return false;
            if (!this.Started)
            {
                return false;
            }

            // kill the monitoring services
            if (this.DeviceMonitor != null)
            {
                this.DeviceMonitor.Stop();
                this.DeviceMonitor = null;
            }

            if (!this.StopAdb())
            {
                return false;
            }

            this.Started = false;
            return true;
        }

        /// <summary>
        /// Restarts adb, but not the services around it.
        /// </summary>
        /// <returns><see langword="true"/> if success.</returns>
        public bool Restart()
        {
            if (string.IsNullOrEmpty(AdbOsLocation))
            {
                Log.e(ADB, "Cannot restart adb when AndroidDebugBridge is created without the location of adb.");
                return false;
            }

            if (!this.VersionCheck)
            {
                Log.LogAndDisplay(LogLevel.Error, ADB, "Attempting to restart adb, but version check failed!");
                return false;
            }

            lock (this)
            {
                this.StopAdb();

                bool restart = this.StartAdb();

                if (restart && this.DeviceMonitor == null)
                {
                    this.DeviceMonitor = new DeviceMonitor(this);
                    this.DeviceMonitor.Start();
                }

                return restart;
            }
        }
        #endregion

        #region public properties

        /// <summary>
        /// Gets or Sets the adb location on the OS.
        /// </summary>
        /// <value>The adb location on the OS.</value>
        public static string AdbOsLocation { get; set; }

        /// <summary>
        /// Gets the devices.
        /// </summary>
        /// <value>The devices.</value>
        public IList<Device> Devices
        {
            get
            {
                //if ( DeviceMonitor != null ) {
                //  return DeviceMonitor.Devices;
                //}
                //return new List<Device> ( );
                return AdbHelper.Instance.GetDevices(AndroidDebugBridge.SocketAddress);
            }
        }

        /// <summary>
        /// Returns whether the bridge has acquired the initial list from adb after being created.
        /// </summary>
        /// <remarks>
        /// <p/>Calling getDevices() right after createBridge(String, boolean) will
        /// generally result in an empty list. This is due to the internal asynchronous communication
        /// mechanism with <code>adb</code> that does not guarantee that the IDevice list has been
        /// built before the call to getDevices().
        /// <p/>The recommended way to get the list of IDevice objects is to create a
        /// IDeviceChangeListener object.
        /// </remarks>
        /// <returns>
        /// 	<see langword="true"/> if [has initial device list]; otherwise, <see langword="false"/>.
        /// </returns>
        public bool HasInitialDeviceList()
        {
            if (this.DeviceMonitor != null)
            {
                return this.DeviceMonitor.HasInitialDeviceList;
            }

            return false;
        }

        /// <summary>
        /// Gets or sets the client to accept debugger connection on the custom "Selected debug port".
        /// </summary>
        /// <remarks>Not Yet Implemented</remarks>
        public IClient SelectedClient
        {
            get
            {
                /*MonitorThread monitorThread = MonitorThread.Instance;
                if ( monitorThread != null ) {
                    return monitorThread.SelectedClient = selectedClient;
                }*/
                return null;
            }

            set
            {
                /*MonitorThread monitorThread = MonitorThread.Instance;
                if ( monitorThread != null ) {
                    monitorThread.SelectedClient = value;
                }*/
            }
        }

        /// <summary>
        /// Returns whether the AndroidDebugBridge object is still connected to the adb daemon.
        /// </summary>
        /// <value><see langword="true"/> if this instance is connected; otherwise, <see langword="false"/>.</value>
        public bool IsConnected
        {
            get
            {
                //MonitorThread monitorThread = MonitorThread.Instance;
                if (this.DeviceMonitor != null /* && monitorThread != null */)
                {
                    return this.DeviceMonitor.IsMonitoring /* && monitorThread.State != State.TERMINATED*/;
                }

                return false;
            }
        }

        /// <summary>
        /// Returns the number of times the AndroidDebugBridge object attempted to connect
        /// </summary>
        /// <value>The connection attempt count.</value>
        public int ConnectionAttemptCount
        {
            get
            {
                if (this.DeviceMonitor != null)
                {
                    return this.DeviceMonitor.ConnectionAttemptCount;
                }

                return -1;
            }
        }

        /// <summary>
        /// Returns the number of times the AndroidDebugBridge object attempted to restart
        /// the adb daemon.
        /// </summary>
        /// <value>The restart attempt count.</value>
        public int RestartAttemptCount
        {
            get
            {
                if (this.DeviceMonitor != null)
                {
                    return this.DeviceMonitor.RestartAttemptCount;
                }

                return -1;
            }
        }

        /// <summary>
        /// Gets the device monitor
        /// </summary>
        public DeviceMonitor DeviceMonitor { get; private set; }

        /// <summary>
        /// Gets if the adb host has started
        /// </summary>
        private bool Started { get; set; }

        /// <summary>
        /// Gets the result of the version check
        /// </summary>
        private bool VersionCheck { get; set; }
        #endregion

        #region private methods

        /// <summary>
        /// Queries adb for its version number and checks it against #MIN_VERSION_NUMBER and MAX_VERSION_NUMBER
        /// </summary>
        private void CheckAdbVersion()
        {
            // default is bad check
            this.VersionCheck = false;

            if (string.IsNullOrEmpty(AdbOsLocation))
            {
                Log.w(TAG, "AdbOsLocation is Empty");
                return;
            }

            try
            {
                Log.d(DDMS, string.Format("Checking '{0} version'", AdbOsLocation));

                ProcessStartInfo psi = new ProcessStartInfo(AdbOsLocation, "version");
                psi.WindowStyle = ProcessWindowStyle.Hidden;
                psi.CreateNoWindow = true;
                psi.UseShellExecute = false;
                psi.RedirectStandardError = true;
                psi.RedirectStandardOutput = true;

                List<string> errorOutput = new List<string>();
                List<string> stdOutput = new List<string>();
                using (Process proc = Process.Start(psi))
                {
                    int status = this.GrabProcessOutput(proc, errorOutput, stdOutput, true /* waitForReaders */);
                    if (status != 0)
                    {
                        StringBuilder builder = new StringBuilder("'adb version' failed!");
                        builder.AppendLine(string.Empty);
                        foreach (string error in errorOutput)
                        {
                            builder.AppendLine(error);
                        }

                        Log.LogAndDisplay(LogLevel.Error, "adb", builder.ToString());
                    }
                }

                // check both stdout and stderr
                bool versionFound = false;
                foreach (string line in stdOutput)
                {
                    versionFound = this.ScanVersionLine(line);
                    if (versionFound)
                    {
                        break;
                    }
                }

                if (!versionFound)
                {
                    foreach (string line in errorOutput)
                    {
                        versionFound = this.ScanVersionLine(line);
                        if (versionFound)
                        {
                            break;
                        }
                    }
                }

                if (!versionFound)
                {
                    // if we get here, we failed to parse the output.
                    Log.LogAndDisplay(LogLevel.Error, ADB, "Failed to parse the output of 'adb version'");
                }
            }
            catch (IOException e)
            {
                Log.LogAndDisplay(LogLevel.Error, ADB, "Failed to get the adb version: " + e.Message);
            }
        }

        /// <summary>
        /// Scans a line resulting from 'adb version' for a potential version number.
        /// </summary>
        /// <param name="line">The line to scan.</param>
        /// <returns><see langword="true"/> if a version number was found (whether it is acceptable or not).</returns>
        /// <remarks>If a version number is found, it checks the version number against what is expected
        /// by this version of ddms.</remarks>
        private bool ScanVersionLine(string line)
        {
            if (!string.IsNullOrEmpty(line))
            {
                Match matcher = Regex.Match(line, ADB_VERSION_PATTERN);
                if (matcher.Success)
                {
                    int majorVersion = int.Parse(matcher.Groups[1].Value);
                    int minorVersion = int.Parse(matcher.Groups[2].Value);
                    int microVersion = int.Parse(matcher.Groups[3].Value);

                    // check only the micro version for now.
                    if (microVersion < ADB_VERSION_MICRO_MIN)
                    {
                        string message = string.Format("Required minimum version of adb: {0}.{1}.{2}. Current version is {0}.{1}.{3}",
                                        majorVersion, minorVersion, ADB_VERSION_MICRO_MIN, microVersion);
                        Log.LogAndDisplay(LogLevel.Error, ADB, message);
                    }
                    else if (ADB_VERSION_MICRO_MAX != -1 && microVersion > ADB_VERSION_MICRO_MAX)
                    {
                        string message = string.Format("Required maximum version of adb: {0}.{1}.{2}. Current version is {0}.{1}.{3}",
                                        majorVersion, minorVersion, ADB_VERSION_MICRO_MAX, microVersion);
                        Log.LogAndDisplay(LogLevel.Error, ADB, message);
                    }
                    else
                    {
                        this.VersionCheck = true;
                    }

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Starts the adb host side server.
        /// </summary>
        /// <returns>true if success</returns>
        private bool StartAdb()
        {
            if (string.IsNullOrEmpty(AdbOsLocation))
            {
                Log.e(ADB, "Cannot start adb when AndroidDebugBridge is created without the location of adb.");
                return false;
            }

            int status = -1;

            try
            {
                string command = "start-server";
                Log.d(DDMS, string.Format("Launching '{0} {1}' to ensure ADB is running.", AdbOsLocation, command));
                ProcessStartInfo psi = new ProcessStartInfo(AdbOsLocation, command);
                psi.CreateNoWindow = true;
                psi.WindowStyle = ProcessWindowStyle.Hidden;
                psi.UseShellExecute = false;
                psi.RedirectStandardError = true;
                psi.RedirectStandardOutput = true;

                using (Process proc = Process.Start(psi))
                {
                    List<string> errorOutput = new List<string>();
                    List<string> stdOutput = new List<string>();
                    status = this.GrabProcessOutput(proc, errorOutput, stdOutput, false /* waitForReaders */);
                }
            }
            catch (IOException ioe)
            {
                Log.d(DDMS, "Unable to run 'adb': {0}", ioe.Message);
            }
            catch (ThreadInterruptedException ie)
            {
                Log.d(DDMS, "Unable to run 'adb': {0}", ie.Message);
            }
            catch (Exception e)
            {
                Log.e(DDMS, e);
            }

            if (status != 0)
            {
                Log.w(DDMS, "'adb start-server' failed -- run manually if necessary");
                return false;
            }

            Log.d(DDMS, "'adb start-server' succeeded");
            return true;
        }

        /// <summary>
        /// Stops the adb host side server.
        /// </summary>
        /// <returns>true if success</returns>
        private bool StopAdb()
        {
            if (string.IsNullOrEmpty(AdbOsLocation))
            {
                Log.e(ADB, "Cannot stop adb when AndroidDebugBridge is created without the location of adb.");
                return false;
            }

            int status = -1;

            try
            {
                string command = "kill-server";
                ProcessStartInfo psi = new ProcessStartInfo(AdbOsLocation, command);
                psi.CreateNoWindow = true;
                psi.WindowStyle = ProcessWindowStyle.Hidden;
                psi.UseShellExecute = false;
                psi.RedirectStandardError = true;
                psi.RedirectStandardOutput = true;

                using (Process proc = Process.Start(psi))
                {
                    proc.WaitForExit();
                    status = proc.ExitCode;
                }
            }
            catch (IOException)
            {
                // we'll return false;
            }
            catch (Exception)
            {
                // we'll return false;
            }

            if (status != 0)
            {
                Log.w(DDMS, "'adb kill-server' failed -- run manually if necessary");
                return false;
            }

            Log.d(DDMS, "'adb kill-server' succeeded");
            return true;
        }

        /// <summary>
        /// Get the stderr/stdout outputs of a process and return when the process is done.
        /// Both <b>must</b> be read or the process will block on windows.
        /// </summary>
        /// <param name="process">The process to get the ouput from</param>
        /// <param name="errorOutput">The array to store the stderr output. cannot be null.</param>
        /// <param name="stdOutput">The array to store the stdout output. cannot be null.</param>
        /// <param name="waitforReaders">if true, this will wait for the reader threads.</param>
        /// <returns>the process return code.</returns>
        private int GrabProcessOutput(Process process, List<string> errorOutput, List<string> stdOutput, bool waitforReaders)
        {
            if (errorOutput == null)
            {
                throw new ArgumentNullException("errorOutput");
            }

            if (stdOutput == null)
            {
                throw new ArgumentNullException("stdOutput");
            }

            // read the lines as they come. if null is returned, it's
            // because the process finished
            Thread t1 = new Thread(new ThreadStart(delegate
            {
                // create a buffer to read the stdoutput
                try
                {
                    using (StreamReader sr = process.StandardError)
                    {
                        while (!sr.EndOfStream)
                        {
                            string line = sr.ReadLine();
                            if (!string.IsNullOrEmpty(line))
                            {
                                Log.e(ADB, line);
                                errorOutput.Add(line);
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    // do nothing.
                }
            }));

            Thread t2 = new Thread(new ThreadStart(delegate
            {
                // create a buffer to read the std output
                try
                {
                    using (StreamReader sr = process.StandardOutput)
                    {
                        while (!sr.EndOfStream)
                        {
                            string line = sr.ReadLine();
                            if (!string.IsNullOrEmpty(line))
                            {
                                stdOutput.Add(line);
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    // do nothing.
                }
            }));

            t1.Start();
            t2.Start();

            // it looks like on windows process#waitFor() can return
            // before the thread have filled the arrays, so we wait for both threads and the
            // process itself.
            if (waitforReaders)
            {
                try
                {
                    t1.Join();
                }
                catch (ThreadInterruptedException)
                {
                }

                try
                {
                    t2.Join();
                }
                catch (ThreadInterruptedException)
                {
                }
            }

            // get the return code from the process
            process.WaitForExit();
            return process.ExitCode;
        }
        #endregion

    }
}
