using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace SharpAdbClient.Tests
{
    public class DummyDevice : IDevice
    {
        public event EventHandler<EventArgs> StateChanged;

        public event EventHandler<EventArgs> BuildInfoChanged;

        public event EventHandler<EventArgs> ClientListChanged;

        public DeviceData DeviceData
        {
            get;
            set;
        }

        public string SerialNumber
        {
            get;
            set;
        }

        public System.Net.IPEndPoint Endpoint
        {
            get { throw new NotImplementedException(); }
        }

        public TransportType TransportType
        {
            get { throw new NotImplementedException(); }
        }

        public string AvdName
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public Dictionary<string, string> EnvironmentVariables
        {
            get { throw new NotImplementedException(); }
        }

        public Dictionary<string, MountPoint> MountPoints
        {
            get { throw new NotImplementedException(); }
        }

        public DeviceState State
        {
            get;
            set;
        } = DeviceState.Online;

        public Dictionary<string, string> Properties
        {
            get { throw new NotImplementedException(); }
        }

        public string GetProperty(string name)
        {
            throw new NotImplementedException();
        }

        public string GetProperty(params string[] name)
        {
            throw new NotImplementedException();
        }

        public bool IsOnline
        {
            get { return true; }
        }

        public bool IsEmulator
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsOffline
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsUnauthorized
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsBootLoader
        {
            get { throw new NotImplementedException(); }
        }

        public ISyncService SyncService
        {
            get { throw new NotImplementedException(); }
        }

        public Image Screenshot
        {
            get { throw new NotImplementedException(); }
        }

        public void ExecuteShellCommand(string command, IShellOutputReceiver receiver)
        {
            this.ExecuteShellCommand(command, receiver, -1);
        }

        public void ExecuteShellCommand(string command, IShellOutputReceiver receiver, int maxTimeToOutputResponse)
        {
            this.ExecuteShellCommand(command, receiver, -1, null);
        }

        public void ExecuteShellCommand(string command, IShellOutputReceiver receiver, params object[] commandArgs)
        {
            this.ExecuteShellCommand(command, receiver, -1, command);
        }

        public void ExecuteShellCommand(string command, IShellOutputReceiver receiver, int maxTimeToOutputResponse, params object[] commandArgs)
        {
            if (command == "busybox")
            {
                throw new FileNotFoundException();
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public void ExecuteRootShellCommand(string command, IShellOutputReceiver receiver, params object[] commandArgs)
        {
            throw new NotImplementedException();
        }

        public void ExecuteRootShellCommand(string command, IShellOutputReceiver receiver, int maxTimeToOutputResponse, params object[] commandArgs)
        {
            throw new NotImplementedException();
        }

        public void ExecuteRootShellCommand(string command, IShellOutputReceiver receiver)
        {
            throw new NotImplementedException();
        }

        public void ExecuteRootShellCommand(string command, IShellOutputReceiver receiver, int maxTimeToOutputResponse)
        {
            throw new NotImplementedException();
        }

        public bool CreateForward(int localPort, int remotePort)
        {
            throw new NotImplementedException();
        }

        public bool RemoveForward(int localPort)
        {
            throw new NotImplementedException();
        }

        public void InstallPackage(string packageFilePath, bool reinstall)
        {
            throw new NotImplementedException();
        }

        public string SyncPackageToDevice(string localFilePath)
        {
            throw new NotImplementedException();
        }

        public void InstallRemotePackage(string remoteFilePath, bool reinstall)
        {
            throw new NotImplementedException();
        }

        public void RemoveRemotePackage(string remoteFilePath)
        {
            throw new NotImplementedException();
        }

        public void UninstallPackage(string packageName)
        {
            throw new NotImplementedException();
        }

        public void RefreshEnvironmentVariables()
        {
            throw new NotImplementedException();
        }

        public void RefreshMountPoints()
        {
            throw new NotImplementedException();
        }

        public void RefreshProperties()
        {
            throw new NotImplementedException();
        }


        public void RemountMountPoint(MountPoint mnt, bool readOnly)
        {
            throw new NotImplementedException();
        }

        public void RemountMountPoint(string mountPoint, bool readOnly)
        {
            throw new NotImplementedException();
        }


        public bool CanSU()
        {
            throw new NotImplementedException();
        }

        public void Reboot()
        {
            throw new NotImplementedException();
        }
    }
}
