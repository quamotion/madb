using System;
using System.Collections.Generic;

namespace SharpAdbClient.Tests
{
    /// <summary>
    /// 
    /// </summary>
    internal class DummyAdbCommandLineClient : AdbCommandLineClient
    {
        public DummyAdbCommandLineClient()
            : base("adb.exe")
        {
        }

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

        public override bool IsValidAdbFile(string adbPath)
        {
            // No validation done in the dummy adb client.
            return true;
        }

        protected override int RunAdbProcessInner(string command, List<string> errorOutput, List<string> standardOutput)
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

            return 0;
        }
    }
}
