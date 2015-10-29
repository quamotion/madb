using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Managed.Adb
{

    /// <summary>
    ///
    /// </summary>
    public enum DeviceState
    {
        /// <summary>
        /// The device is in recovery mode.
        /// </summary>
        Recovery,

        /// <summary>
        /// The device is in bootloader mode
        /// </summary>
        BootLoader,

        /// <summary>
        /// The instance is not connected to adb or is not responding.
        /// </summary>
        Offline,

        /// <summary>
        /// The instance is now connected to the adb server. Note that this state does not imply that the Android system is
        /// fully booted and operational, since the instance connects to adb while the system is still booting.
        /// However, after boot-up, this is the normal operational state of an emulator/device instance.
        /// </summary>
        Online,

        /// <summary>
        /// The device is in download mode.
        /// </summary>
        Download,

        /// <summary>
        /// The device state is unknown.
        /// </summary>
        Unknown,

        /// <summary>
        /// The device is connected to adb, but adb is not authorized for remote debugging of this device.
        /// </summary>
        Unauthorized
    }
}
