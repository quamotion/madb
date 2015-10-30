// <copyright file="AdbSocketFactory.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion. All rights reserved.
// </copyright>

namespace Managed.Adb
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Text;

    /// <summary>
    /// Implements the <see cref="IAdbSocketFactory"/> protocol for the <see cref="AdbSocket"/>
    /// class.
    /// </summary>
    public class AdbSocketFactory : IAdbSocketFactory
    {
        /// <summary>
        /// Creates a new instance of the <see cref="AdbSocket"/> class.
        /// </summary>
        /// <param name="endPoint">
        /// The <see cref="IPEndPoint"/> to which the socket should connect.
        /// </param>
        /// <returns>
        /// A new instance of the <see cref="AdbSocket"/> class.
        /// </returns>
        public IAdbSocket Create(IPEndPoint endPoint)
        {
            return new AdbSocket(endPoint);
        }
    }
}
