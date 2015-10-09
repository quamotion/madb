// <copyright file="MonitorThread.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion. All rights reserved.
// </copyright>

namespace Managed.Adb
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;

    /// <summary>
    ///
    /// </summary>
    internal class MonitorThread
    {
        /// <summary>
        ///
        /// </summary>
        public enum ThreadState
        {
            /// <summary>
            ///
            /// </summary>
            UNKNOWN = -1,

            /// <summary>
            ///
            /// </summary>
            Ready = 2,

            /// <summary>
            ///
            /// </summary>
            Disconnected = 3,
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MonitorThread"/> class.
        /// </summary>
        private MonitorThread()
        {
        }

        /// <summary>
        ///
        /// </summary>
        private static MonitorThread instance;

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static MonitorThread Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new MonitorThread();
                }

                return instance;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="MonitorThread"/> is quit.
        /// </summary>
        /// <value><see langword="true"/> if quit; otherwise, <see langword="false"/>.</value>
        public bool Quit { get; private set; }

        /// <summary>
        /// Sets the debug selected port.
        /// </summary>
        /// <param name="value">The value.</param>
        internal void SetDebugSelectedPort(int value)
        {
        }
    }
}
