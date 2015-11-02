using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Managed.Adb
{
    public static class SyncCommandConverter
    {
        public static byte[] GetBytes(SyncCommand command)
        {
            string commandText = null;

            switch (command)
            {
                case SyncCommand.LIST:
                    commandText = "LIST";
                    break;

                case SyncCommand.RECV:
                    commandText = "RECV";
                    break;

                case SyncCommand.SEND:
                    commandText = "SEND";
                    break;

                case SyncCommand.STAT:
                    commandText = "STAT";
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(command));
            }

            byte[] commandBytes = AdbClient.Encoding.GetBytes(commandText);

            return commandBytes;
        }

        public static SyncCommand GetCommand(byte[] value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (value.Length != 4)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            string commandText = AdbClient.Encoding.GetString(value);

            if (string.Equals(commandText, "LIST", StringComparison.OrdinalIgnoreCase))
            {
                return SyncCommand.LIST;
            }
            else if (string.Equals(commandText, "RECV", StringComparison.OrdinalIgnoreCase))
            {
                return SyncCommand.RECV;
            }
            else if (string.Equals(commandText, "SEND", StringComparison.OrdinalIgnoreCase))
            {
                return SyncCommand.SEND;
            }
            else if (string.Equals(commandText, "STAT", StringComparison.OrdinalIgnoreCase))
            {
                return SyncCommand.STAT;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }
        }
    }
}
