using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Managed.Adb
{
    public class DeviceData
    {
        /// <summary>
        /// Device list info regex
        /// </summary>
        /// <workitem>21136</workitem>
        internal const string RE_DEVICELIST_INFO = @"^([a-z0-9_-]+(?:\s?[\.a-z0-9_-]+)?(?:\:\d{1,})?)\s+(device|offline|unknown|bootloader|recovery|download|unauthorized)(?:\s+product:([^:]+)\s+model\:([\S]+)\s+device\:([\S]+))?$";

        public string Serial
        {
            get;
            set;
        }

        public DeviceState State
        {
            get;
            set;
        }

        public string Model
        {
            get;
            set;
        }

        public string Product
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public static DeviceData CreateFromAdbData(string data)
        {
            Regex re = new Regex(RE_DEVICELIST_INFO, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Match m = re.Match(data);
            if (m.Success)
            {
                return new DeviceData()
                {
                    Serial = m.Groups[1].Value,
                    State = GetStateFromString(m.Groups[2].Value),
                    Model = m.Groups[4].Value,
                    Product = m.Groups[3].Value,
                    Name = m.Groups[5].Value
                };
            }
            else
            {
                throw new ArgumentException("Invalid device list data");
            }
        }

        /// <summary>
        /// Get the device state from the string value
        /// </summary>
        /// <param name="state">The device state string</param>
        /// <returns></returns>
        internal static DeviceState GetStateFromString(string state)
        {
            string tstate = state;

            if (string.Compare(state, "device", false) == 0)
            {
                tstate = "online";
            }

            if (Enum.IsDefined(typeof(DeviceState), tstate))
            {
                return (DeviceState)Enum.Parse(typeof(DeviceState), tstate, true);
            }
            else
            {
                foreach (var fi in typeof(DeviceState).GetFields())
                {
                    if (string.Compare(fi.Name, tstate, true) == 0)
                    {
                        return (DeviceState)fi.GetValue(null);
                    }
                }
            }

            return DeviceState.Unknown;
        }
    }
}
