using SharpAdbClient.Exceptions;
using SharpAdbClient.Logs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SharpAdbClient.InteractiveShell
{
    public delegate Boolean ShellResponseEventHandler(String adbResponse, ShellResponseEventArgs args);

    public class Shell
    {
        public event ShellResponseEventHandler ResponseAdb = null;

        public AdbClient AdbClient { get; set;}

        public Shell(AdbClient adbClient)
        {
            this.AdbClient = adbClient;
        }

        public async Task ExecuteRemoteCommandAsync(string command, DeviceData device, CancellationToken cancellationToken, int maxTimeToOutputResponse)
        {
            using (AdbSocket adbSocket = new AdbSocket(this.AdbClient.EndPoint))
            {
                cancellationToken.Register(() => adbSocket.Dispose());
                this.AdbClient.SetDevice(adbSocket, device);
                adbSocket.SendAdbRequest($"shell:{command}");
                var response = adbSocket.ReadAdbResponse();

                ShellResponseEventArgs shellResponseEventArgs = new ShellResponseEventArgs(this);
                shellResponseEventArgs.LastCommand = command;

                try
                {
                    var shellStream = (adbSocket.GetShellStream() as ShellStream);
                    
                    using (StreamReader reader = new StreamReader(shellStream.Inner, AdbClient.Encoding))
                    {
                        while (!cancellationToken.IsCancellationRequested && shellResponseEventArgs.CloseShell == false)
                        {
                            var line = await reader.ReadLineAsync().ConfigureAwait(false);

                            if (ResponseAdb(line, shellResponseEventArgs))
                            {
                                if (String.IsNullOrEmpty(shellResponseEventArgs.NextCommand) == false)
                                {
                                    byte[] bytes = FormAdbNextRequest(shellResponseEventArgs.NextCommand);

                                    (shellStream.Inner as System.Net.Sockets.NetworkStream).Write(bytes, 0, bytes.Length);
                                    (shellStream.Inner as System.Net.Sockets.NetworkStream).Flush();
                                    shellResponseEventArgs.LastCommand = shellResponseEventArgs.NextCommand;
                                    shellResponseEventArgs.NextCommand = null;
                                }
                            }

                            
                        }
                    }
                }
                catch (Exception e)
                {
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        throw new ShellCommandUnresponsiveException(e);
                    }
                }
                finally
                {
                    
                }
            }

        }

        public static byte[] FormAdbNextRequest(string req)
        {
            byte[] result = AdbClient.Encoding.GetBytes(req + "\n");
            return result;
        }
    }


    public class ShellResponseEventArgs : EventArgs
    {
        public Shell Shell { get; private set; }
        public String LastCommand { get; internal set; }
        public String NextCommand { get; set; }
        public Boolean CloseShell { get; set; } = false;

        public ShellResponseEventArgs(Shell shell)
        {
            this.Shell = shell;
        }
    }
}

