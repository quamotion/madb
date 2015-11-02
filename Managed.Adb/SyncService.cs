// <copyright file="SyncService.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion. All rights reserved.
// </copyright>

namespace Managed.Adb
{
    using Exceptions;
    using Managed.Adb.IO;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Threading;

    public class SyncService : ISyncService, IDisposable
    {
        /// <summary>
        /// Logging tag
        /// </summary>
        private const string Tag = nameof(SyncService);

        private const string RECV = "RECV";
        private const string DATA = "DATA";
        private const string DONE = "DONE";

        [Flags]
        public enum FileMode
        {
            TypeMask = 0x8000,

            /// <summary>
            /// The unknown
            /// </summary>
            UNKNOWN = 0x0000,

            /// <summary>
            /// The socket
            /// </summary>
            Socket = 0xc000,

            /// <summary>
            /// The symbolic link
            /// </summary>
            SymbolicLink = 0xa000,

            /// <summary>
            /// The regular
            /// </summary>
            Regular = 0x8000,

            /// <summary>
            /// The block
            /// </summary>
            Block = 0x6000,

            /// <summary>
            /// The directory
            /// </summary>
            Directory = 0x4000,

            /// <summary>
            /// The character
            /// </summary>
            Character = 0x2000,

            /// <summary>
            /// The fifo
            /// </summary>
            FIFO = 0x1000
        }

        private const int SYNC_DATA_MAX = 64 * 1024;
        private const int REMOTE_PATH_MAX_LENGTH = 1024;

        public DeviceData Device
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SyncService"/> class.
        /// </summary>
        /// <param name="device">The device.</param>
        public SyncService(DeviceData device)
            : this(AdbClient.SocketFactory.Create(AdbServer.EndPoint), device)
        {
        }

        public SyncService(IAdbSocket socket, DeviceData device)
        {
            this.Socket = socket;
            this.Device = device;
        }

        /// <summary>
        /// Gets the address.
        /// </summary>
        /// <value>
        /// The address.
        /// </value>
        public IPEndPoint Address { get; private set; }

        private IAdbSocket Socket { get; set; }

        /// <include file='.\ISyncService.xml' path='/SyncService/IsOpen/*'/>
        public bool IsOpen
        {
            get
            {
                return this.Socket != null && this.Socket.Connected;
            }
        }

        /// <include file='.\ISyncService.xml' path='/SyncService/Open/*'/>
        public void Open()
        {
            if (this.IsOpen)
            {
                return;
            }

            try
            {
                this.Socket = AdbClient.SocketFactory.Create(this.Address);

                // target a specific device
                AdbClient.Instance.SetDevice(this.Socket, this.Device);

                this.Socket.SendAdbRequest("sync:");
                var resp = this.Socket.ReadAdbResponse(false);
            }
            catch (IOException)
            {
                this.Close();
                throw;
            }

            return;
        }

        /// <include file='.\ISyncService.xml' path='/SyncService/Close/*'/>
        public void Close()
        {
            if (this.Socket != null)
            {
                try
                {
                    this.Socket.Close();
                }
                catch (IOException)
                {
                }

                this.Socket = null;
            }
        }

        public void Push(Stream stream, string remotePath, int permissions, IProgress<int> progress, CancellationToken cancellationToken)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (remotePath == null)
            {
                throw new ArgumentNullException(nameof(remotePath));
            }

            if (remotePath.Length > REMOTE_PATH_MAX_LENGTH)
            {
                throw new ArgumentOutOfRangeException(nameof(remotePath), $"The remote path {remotePath} exceeds the maximum path size {REMOTE_PATH_MAX_LENGTH}");
            }

            this.Socket.SendSyncRequest(SyncCommand.SEND, remotePath, permissions);

            // create the buffer used to read.
            // we read max SYNC_DATA_MAX.
            byte[] buffer = new byte[SYNC_DATA_MAX];

            // look while there is something to read
            while (true)
            {
                // check if we're canceled
                cancellationToken.ThrowIfCancellationRequested();

                // read up to SYNC_DATA_MAX
                int read = stream.Read(buffer, 0, SYNC_DATA_MAX);

                if (read == 0)
                {
                    // we reached the end of the file
                    break;
                }

                // now send the data to the device
                // first write the amount read
                this.Socket.SendSyncRequest(SyncCommand.DATA, read);
                this.Socket.Send(buffer, read, -1);
            }

            // create the DONE message
            int time = (int)(DateTime.Now.CurrentTimeMillis() / 1000d);
            this.Socket.SendSyncRequest(SyncCommand.DONE, time);

            // read the result, in a byte array containing 2 ints
            // (id, size)
            var result = this.Socket.ReadSyncResponse();

            if (result == SyncCommand.FAIL)
            {
                var message = this.Socket.ReadSyncString();

                throw new AdbException(message);
            }
            else if (result != SyncCommand.OKAY)
            {
                throw new AdbException($"The server sent an invali repsonse {result}");
            }
        }

        /// <include file='.\ISyncService.xml' path='/SyncService/PullFile2/*'/>
        public void Pull(string remoteFilepath, Stream stream, IProgress<int> progress, CancellationToken cancellationToken)
        {
            if (remoteFilepath == null)
            {
                throw new ArgumentNullException(nameof(remoteFilepath));
            }

            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            byte[] buffer = new byte[SYNC_DATA_MAX];

            this.Socket.SendSyncRequest(SyncCommand.RECV, remoteFilepath);

            while (true)
            {
                var response = this.Socket.ReadSyncResponse();
                cancellationToken.ThrowIfCancellationRequested();

                if (response == SyncCommand.DONE)
                {
                    break;
                }
                else if (response != SyncCommand.DATA)
                {
                    throw new AdbException($"The server sent an invalid response {response}");
                }

                // The first 4 bytes contain the length of the data packet
                var reply = new byte[4];
                this.Socket.Read(reply);

                if (!BitConverter.IsLittleEndian)
                {
                    Array.Reverse(reply);
                }

                int size = BitConverter.ToInt32(reply, 0);

                if (size > SYNC_DATA_MAX)
                {
                    throw new AdbException($"The adb server is sending {size} bytes of data, which exceeds the maximum chunk size {SYNC_DATA_MAX}");
                }

                // now read the length we received
                this.Socket.Read(buffer, size, -1);
                stream.Write(buffer, 0, size);
            }
        }

        /// <summary>
        /// Returns the mode of the remote file.
        /// </summary>
        /// <param name="path">the remote file</param>
        /// <returns>the mode if all went well; otherwise, FileMode.UNKNOWN</returns>
        public FileStatistics Stat(string path)
        {
            // create the stat request message.
            this.Socket.SendSyncRequest(SyncCommand.STAT, path);

            if (this.Socket.ReadSyncResponse() != SyncCommand.STAT)
            {
                throw new AdbException($"The server returned an invalid sync response.");
            }

            // read the result, in a byte array containing 3 ints
            // (mode, size, time)
            FileStatistics value = new FileStatistics();
            value.Path = path;

            this.ReadStatistics(value);

            return value;
        }

        private void ReadStatistics(FileStatistics value)
        {
            byte[] statResult = new byte[12];
            this.Socket.Read(statResult);

            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(statResult, 0, 4);
                Array.Reverse(statResult, 4, 4);
                Array.Reverse(statResult, 8, 4);
            }

            value.FileMode = (FileMode)BitConverter.ToInt32(statResult, 0);
            value.Size = BitConverter.ToInt32(statResult, 4);
            value.Time = ManagedAdbExtenstions.Epoch.AddSeconds(BitConverter.ToInt32(statResult, 8)).ToLocalTime();
        }

        public IEnumerable<FileStatistics> GetDirectoryListing(string path)
        {
            Collection<FileStatistics> value = new Collection<FileStatistics>();

            // create the stat request message.
            this.Socket.SendSyncRequest(SyncCommand.LIST, path);

            while (true)
            {
                var response = this.Socket.ReadSyncResponse();

                if (response == SyncCommand.DONE)
                {
                    break;
                }
                else if (response != SyncCommand.DENT)
                {
                    throw new AdbException($"The server returned an invalid sync response.");
                }

                FileStatistics entry = new FileStatistics();
                this.ReadStatistics(entry);
                entry.Path = this.Socket.ReadSyncString();

                value.Add(entry);
            }

            return value;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Close();
        }
    }
}
