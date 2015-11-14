//-----------------------------------------------------------------------
// <copyright file="IFileSystem.cs" company="Quamotion">
//     Copyright (c) 2015 Quamotion. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Collections.Generic;

namespace SharpAdbClient
{
    public interface IFileSystem
    {
        IEnumerable<FileEntry> DeviceBlocks { get; }

        void Chmod(string path, string permissions);
        void Chmod(string path, FilePermissions permissions);
        void Copy(string source, string destination);
        FileEntry Create(string path);
        FileEntry Create(FileEntry fileEntry);
        void Delete(string path);
        void Delete(FileEntry fileEntry);
        bool Exists(string path);
        bool IsMountPointReadOnly(string mount);
        void MakeDirectory(string path);
        void MakeDirectory(string path, bool forceDeviceMethod);
        void Mount(string mountPoint);
        void Mount(MountPoint mountPoint);
        void Mount(MountPoint mountPoint, string options);
        void Mount(string directory, string device, string fileSytemType, bool isReadOnly);
        void Mount(string directory, string device, string fileSytemType, bool isReadOnly, string options);
        void Move(string source, string destination);
        string ResolveLink(string path);
        void Unmount(string mountPoint);
        void Unmount(MountPoint mountPoint);
        void Unmount(string mountPoint, string options);
        void Unmount(MountPoint mountPoint, string options);
    }
}