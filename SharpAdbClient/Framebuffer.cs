// <copyright file="Framebuffer.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion. All rights reserved.
// </copyright>

namespace SharpAdbClient
{
    using System;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Threading.Tasks;

    public class Framebuffer
    {
        private readonly AdbClient client;
        private readonly byte[] headerData;
        private bool headerInitialized;

        public Framebuffer(DeviceData device, AdbClient client)
        {
            if (device == null)
            {
                throw new ArgumentNullException(nameof(device));
            }

            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            this.Device = device;

            this.client = client;

            // Initialize the headerData buffer
            var size = Marshal.SizeOf(typeof(FramebufferHeader));
            this.headerData = new byte[size];
        }

        public DeviceData Device
        {
            get;
            private set;
        }

        public FramebufferHeader Header
        {
            get;
            set;
        }

        public byte[] Data
        {
            get;
            private set;
        }

        public async Task Refresh(CancellationToken cancellationToken)
        {
            var socket = Factories.AdbSocketFactory(this.client.EndPoint);

            // Select the target device
            this.client.SetDevice(socket, this.Device);

            // Send the framebuffer command
            socket.SendAdbRequest("framebuffer:");
            socket.ReadAdbResponse();

            // The result first is a FramebufferHeader object,
            await socket.ReadAsync(this.headerData, cancellationToken).ConfigureAwait(false);

            if (!this.headerInitialized)
            {
                this.Header = FramebufferHeader.Read(this.headerData);
                this.headerInitialized = true;
            }

            if (this.Data == null || this.Data.Length < this.Header.Size)
            {
                this.Data = new byte[this.Header.Size];
            }

            // followed by the actual framebuffer content
            await socket.ReadAsync(this.Data, (int)this.Header.Size, cancellationToken).ConfigureAwait(false);
        }

        public Image ToImage()
        {
            if (this.Data == null)
            {
                throw new InvalidOperationException("Call RefreshAsync first");
            }

            return this.Header.ToImage(this.Data);
        }
    }
}
