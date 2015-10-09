// <copyright file="AndroidDebugBridgeEventArgs.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion. All rights reserved.
// </copyright>

namespace Managed.Adb
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    ///
    /// </summary>
    /// <ignore>true</ignore>
    public class AndroidDebugBridgeEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AndroidDebugBridgeEventArgs"/> class.
        /// </summary>
        /// <param name="bridge">The bridge.</param>
        public AndroidDebugBridgeEventArgs(AndroidDebugBridge bridge)
        {
            this.Bridge = bridge;
        }

        /// <summary>
        /// Gets the bridge.
        /// </summary>
        /// <value>The bridge.</value>
        public AndroidDebugBridge Bridge { get; private set; }
    }
}
