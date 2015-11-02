using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Managed.Adb
{
    public class FileStatistics
    {
        public SyncService.FileMode FileMode
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the total file size, in bytes.
        /// </summary>
        public int Size
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the time of last modification.
        /// </summary>
        public DateTime Time
        {
            get;
            set;
        }
    }
}
