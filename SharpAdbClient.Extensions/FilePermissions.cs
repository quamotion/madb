// <copyright file="FilePermissions.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion. All rights reserved.
// </copyright>

namespace SharpAdbClient
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class FilePermissions
    {
        public FilePermissions()
            : this("---------")
        {
        }

        public FilePermissions(string permissions)
        {
            if (permissions.Length > 9)
            {
                permissions = permissions.Substring(1, 9);
            }

            if (permissions.Length < 9)
            {
                throw new ArgumentException(string.Format("Invalid permissions string: {0}", permissions));
            }

            this.User = new FilePermission(permissions.Substring(0, 3));
            this.Group = new FilePermission(permissions.Substring(3, 3));
            this.Other = new FilePermission(permissions.Substring(6, 3));
        }

        public FilePermissions(FilePermission user, FilePermission group, FilePermission other)
        {
            this.User = user;
            this.Group = group;
            this.Other = other;
        }

        public FilePermission User { get; set; }

        public FilePermission Group { get; set; }

        public FilePermission Other { get; set; }

        public override string ToString()
        {
            return string.Format("{0}{1}{2}", this.User.ToString(), this.Group.ToString(), this.Other.ToString());
        }

        public string ToChmod()
        {
            return string.Format("{0}{1}{2}", (int)this.User.ToChmod(), (int)this.Group.ToChmod(), (int)this.Other.ToChmod());
        }
    }
}
