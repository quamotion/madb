// <copyright file="IAdbSocketFactory.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion. All rights reserved.
// </copyright>

namespace Managed.Adb
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Text;

    public interface IAdbSocketFactory
    {
        IAdbSocket Create(IPEndPoint endPoint);
    }
}
