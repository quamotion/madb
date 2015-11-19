using SharpAdbClient.Logs;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Threading;

namespace SharpAdbClient.Tests
{
    internal class DummyAdbClient : IAdbClient
    {
        public Dictionary<string, string> Commands
        { get; private set; } = new Dictionary<string, string>();

        public void Connect(DnsEndPoint endpoint)
        {
            throw new NotImplementedException();
        }

        public void CreateForward(DeviceData device, string local, string remote, bool allowRebind)
        {
            throw new NotImplementedException();
        }

        public void ExecuteRemoteCommand(string command, DeviceData device, IShellOutputReceiver rcvr, CancellationToken cancellationToken, int maxTimeToOutputResponse)
        {
            if (this.Commands.ContainsKey(command))
            {
                if (rcvr != null)
                {
                    StringReader reader = new StringReader(this.Commands[command]);

                    while (reader.Peek() != -1)
                    {
                        rcvr.AddOutput(reader.ReadLine());
                    }

                    rcvr.Flush();
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(command));
            }
        }

        public int GetAdbVersion()
        {
            throw new NotImplementedException();
        }

        public List<DeviceData> GetDevices()
        {
            throw new NotImplementedException();
        }

        public Image GetFrameBuffer(DeviceData device)
        {
            throw new NotImplementedException();
        }

        public void KillAdb()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ForwardData> ListForward(DeviceData device)
        {
            throw new NotImplementedException();
        }

        public void Reboot(string into, DeviceData device)
        {
            throw new NotImplementedException();
        }

        public void RemoveAllForwards(DeviceData device)
        {
            throw new NotImplementedException();
        }

        public void RemoveForward(DeviceData device, int localPort)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<LogEntry> RunLogService(DeviceData device, params LogId[] logNames)
        {
            throw new NotImplementedException();
        }

        public void SetDevice(IAdbSocket socket, DeviceData device)
        {
            throw new NotImplementedException();
        }
    }
}
