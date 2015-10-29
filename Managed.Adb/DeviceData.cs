using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Managed.Adb
{
    public class DeviceData
    {
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
            Regex re = new Regex(Device.RE_DEVICELIST_INFO, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Match m = re.Match(data);
            if (m.Success)
            {
                return new DeviceData()
                {
                    Serial = m.Groups[1].Value,
                    State = Device.GetStateFromString(m.Groups[2].Value),
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
    }
}
