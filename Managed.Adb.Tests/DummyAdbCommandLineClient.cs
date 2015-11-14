using System;
using System.Collections.Generic;

namespace Managed.Adb.Tests
{
    /// <summary>
    /// 
    /// </summary>
    internal class DummyAdbCommandLineClient : AdbCommandLineClient
    {
        public Version Version
        {
            get;
            set;
        }

        public bool ServerStarted
        {
            get;
            private set;
        }

        protected override void RunAdbProcess(string command, List<string> errorOutput, List<string> standardOutput)
        {
            if (errorOutput != null)
            {
                errorOutput.Add(null);
            }

            if (standardOutput != null)
            {
                standardOutput.Add(null);
            }

            if (command == "start-server")
            {
                this.ServerStarted = true;
            }
            else if (command == "version")
            {
                if (standardOutput != null && this.Version != null)
                {
                    standardOutput.Add($"Android Debug Bridge version {this.Version.ToString(3)}");
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(command));
            }
        }
    }
}
