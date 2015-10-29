using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Managed.Adb
{
    public class ForwardData
    {
        public string SerialNumber
        {
            get;
            set;
        }

        public string Local
        {
            get;
            set;
        }

        public string Remote
        {
            get;
            set;
        }

        public static ForwardData FromString(string value)
        {
            string[] parts = value.Split(' ');
            return new ForwardData()
            {
                SerialNumber = parts[0],
                Local = parts[1],
                Remote = parts[2]
            };
        }
    }
}
