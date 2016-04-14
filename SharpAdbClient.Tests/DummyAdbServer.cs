using System;
using System.Net;

namespace SharpAdbClient.Tests
{
    /// <summary>
    /// A mock implementation of the <see cref="IAdbServer"/> class.
    /// </summary>
    internal class DummyAdbServer : IAdbServer
    {
        /// <inheritdoc/>
        /// <remarks>
        /// The value is set to a value different from the default adb end point, to detect the dummy
        /// server being used. 
        /// </remarks>
        public EndPoint EndPoint
        { get; set; } = new IPEndPoint(IPAddress.Loopback, 9999);

        /// <summary>
        /// Gets or sets the status as is to be reported by the <see cref="DummyAdbServer"/>.
        /// </summary>
        public AdbServerStatus Status
        { get; set; }

        /// <summary>
        /// Gets a value indicating whether the server was restarted.
        /// </summary>
        public bool WasRestarted
        { get; private set; }

        /// <inheritdoc/>
        public AdbServerStatus GetStatus()
        {
            return this.Status;
        }

        /// <inheritdoc/>
        public void RestartServer()
        {
            this.WasRestarted = true;
        }

        /// <inheritdoc/>
        public StartServerResult StartServer(string adbPath, bool restartServerIfNewer)
        {
            if (this.Status.IsRunning == true)
            {
                return StartServerResult.AlreadyRunning;
            }

            this.Status = new AdbServerStatus()
            {
                IsRunning = true,
                Version = new Version(1, 0, 20)
            };

            return StartServerResult.Started;
        }
    }
}
