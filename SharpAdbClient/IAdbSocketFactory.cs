// <copyright file="IAdbSocketFactory.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion. All rights reserved.
// </copyright>

namespace SharpAdbClient
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Text;

    /// <summary>
    /// Provides an interface for any class that can create a new instance of a class that
    /// implements the <see cref="IAdbSocket"/> interface.
    /// This helps keeping the code testable.
    /// </summary>
    public interface IAdbSocketFactory
    {
        /// <summary>
        /// Creates a new instance of a class that implements the <see cref="IAdbSocket"/>
        /// interface
        /// </summary>
        /// <param name="endPoint">
        /// The <see cref="IPEndPoint"/> to which the socket should connect.
        /// </param>
        /// <returns>
        /// A new instance of a class that implements the <see cref="IAdbSocket"/> protocol.
        /// </returns>
        IAdbSocket Create(IPEndPoint endPoint);
    }
}
