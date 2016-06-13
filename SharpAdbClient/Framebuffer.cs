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

    /// <summary>
    /// Provides access to the framebuffer (that is, a copy of the image being displayed on the device screen).
    /// </summary>
    public class Framebuffer
    {
        private readonly AdbClient client;
        private readonly byte[] headerData;
        private bool headerInitialized;

        /// <summary>
        /// Initializes a new instance of the <see cref="Framebuffer"/> class.
        /// </summary>
        /// <param name="device">
        /// The device for which to fetch the frame buffer.
        /// </param>
        /// <param name="client">
        /// A <see cref="AdbClient"/> which manages the connection with adb.
        /// </param>
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

        /// <summary>
        /// Gets the device for which to fetch the frame buffer.
        /// </summary>
        public DeviceData Device
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the framebuffer header. The header contains information such as the width and height and the color encoding.
        /// This property is set after you call <see cref="RefreshAsync(CancellationToken)"/>.
        /// </summary>
        public FramebufferHeader Header
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the framebuffer data. You need to parse the <see cref="FramebufferHeader"/> to interpret this data (such as the color encoding).
        /// This property is set after you call <see cref="RefreshAsync(CancellationToken)"/>.
        /// </summary>
        public byte[] Data
        {
            get;
            private set;
        }

        /// <summary>
        /// Asynchronously refreshes the framebuffer: fetches the latest framebuffer data from the device. Access the <see cref="Header"/>
        /// and <see cref="Data"/> properties to get the updated framebuffer data.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous task.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous operation.
        /// </returns>
        public async Task RefreshAsync(CancellationToken cancellationToken)
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

        /// <summary>
        /// Converts the framebuffer data to a <see cref="Image"/>.
        /// </summary>
        /// <returns>
        /// An <see cref="Image"/> which represents the framebuffer data.
        /// </returns>
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
