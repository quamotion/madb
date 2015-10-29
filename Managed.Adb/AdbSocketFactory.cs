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

    public class AdbSocketFactory : IAdbSocketFactory
    {
        public IAdbSocket Create(IPEndPoint endPoint)
        {
            return new AdbSocket(endPoint);
        }
    }
}
