using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Managed.Adb
{
    /// <ignore>true</ignore>
    public static partial class ManagedAdbExtenstions
    {

        /// <summary>
        /// Determines whether the specified path is directory.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>
        ///   <see langword="true"/> if the specified path is directory; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool IsDirectory(string path)
        {
            return File.Exists(path) && (File.GetAttributes(path) & FileAttributes.Directory) == FileAttributes.Directory;
        }

        /// <summary>
        /// Determines whether the specified fsi is directory.
        /// </summary>
        /// <param name="fsi">The fsi.</param>
        /// <returns>
        ///   <see langword="true"/> if the specified fsi is directory; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool IsDirectory(this FileSystemInfo fsi)
        {
            return (fsi.Attributes & FileAttributes.Directory) == FileAttributes.Directory;
        }

        /// <summary>
        /// Determines whether the specified path is file.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>
        ///   <see langword="true"/> if the specified path is file; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool IsFile(string path)
        {
            return File.Exists(path) && !IsDirectory(path);
        }

        /// <summary>
        /// Determines whether the specified fsi is file.
        /// </summary>
        /// <param name="fsi">The fsi.</param>
        /// <returns>
        ///   <see langword="true"/> if the specified fsi is file; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool IsFile(this FileSystemInfo fsi)
        {
            return fsi is FileInfo && !IsDirectory(fsi.FullName);
        }

        /// <summary>
        /// Gets the file system info.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public static FileSystemInfo GetFileSystemInfo(this string path)
        {
            if (IsDirectory(path))
            {
                return new DirectoryInfo(path);
            }
            else
            {
                return new FileInfo(path);
            }
        }
    }
}
