// <copyright file="PackageInstallationException.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion. All rights reserved.
// </copyright>

namespace SharpAdbClient.DeviceCommands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
#if !NETSTANDARD1_3
    using System.Runtime.Serialization;
#endif
    using System.Text;

    /// <summary>
    /// An exception while installing a package on the device
    /// </summary>
#if !NETSTANDARD1_3
    [Serializable]
#endif
    public class PackageInstallationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PackageInstallationException"/> class.
        /// </summary>
        public PackageInstallationException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PackageInstallationException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public PackageInstallationException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PackageInstallationException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="inner">The inner.</param>
        public PackageInstallationException(string message, Exception inner)
            : base(message, inner)
        {
        }

#if !NETSTANDARD1_3
        /// <summary>
        /// Initializes a new instance of the <see cref="PackageInstallationException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// The <paramref name="info"/> parameter is null.
        ///   </exception>
        ///
        /// <exception cref="T:System.Runtime.Serialization.SerializationException">
        /// The class name is null or <see cref="P:System.Exception.HResult"/> is zero (0).
        ///   </exception>
        protected PackageInstallationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
#endif
    }
}
