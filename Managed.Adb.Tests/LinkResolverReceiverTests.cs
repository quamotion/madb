using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Managed.Adb.Tests
{
    [TestClass]
    public class LinkResolverReceiverTests
    {
        [TestMethod]
        public void ResolveTest()
        {
            string output = @"drwxr-xr-x root     root              2015-06-01 10:17 acct
drwxrwx--- system   cache             2015-05-13 02:03 cache
lrwxrwxrwx root     root              1970-01-01 01:00 charger -> /sbin/healthd
dr-x------ root     root              2015-06-01 10:17 config
lrwxrwxrwx root     root              2015-06-01 10:17 d -> /sys/kernel/debug
drwxrwx--x system   system            2015-05-13 00:24 data
-rw-r--r-- root     root          297 1970-01-01 01:00 default.prop
drwxr-xr-x root     root              2015-06-01 10:17 dev
lrwxrwxrwx root     root              2015-06-01 10:17 etc -> /system/etc
lstat '//factory' failed: Permission denied
-rw-r--r-- root     root        12819 1970-01-01 01:00 file_contexts
-rw-r----- root     root         1523 1970-01-01 01:00 fstab.manta
-rw-r----- root     root          657 1970-01-01 01:00 fstab.smdk5250
-rwxr-x--- root     root       227652 1970-01-01 01:00 init
-rwxr-x--- root     root          944 1970-01-01 01:00 init.environ.rc
-rwxr-x--- root     root         6801 1970-01-01 01:00 init.manta.rc
-rwxr-x--- root     root         3061 1970-01-01 01:00 init.manta.usb.rc
-rwxr-x--- root     root        21728 1970-01-01 01:00 init.rc
-rwxr-x--- root     root          203 1970-01-01 01:00 init.recovery.manta.rc
-rwxr-x--- root     root         1754 1970-01-01 01:00 init.smdk5250.rc
-rwxr-x--- root     root         2109 1970-01-01 01:00 init.smdk5250.usb.rc
-rwxr-x--- root     root         1927 1970-01-01 01:00 init.trace.rc
-rwxr-x--- root     root         3885 1970-01-01 01:00 init.usb.rc
-rwxr-x--- root     root          301 1970-01-01 01:00 init.zygote32.rc
drwxrwxr-x root     system            2015-06-01 10:17 mnt
dr-xr-xr-x root     root              1970-01-01 01:00 proc
-rw-r--r-- root     root         2771 1970-01-01 01:00 property_contexts
drwxr-xr-x root     root              1970-01-01 01:00 res
drwx------ root     root              2015-02-25 01:40 root
drwxr-x--- root     root              1970-01-01 01:00 sbin
lrwxrwxrwx root     root              2015-06-01 10:17 sdcard -> /storage/emulat
ed/legacy
-rw-r--r-- root     root          471 1970-01-01 01:00 seapp_contexts
-rw-r--r-- root     root           56 1970-01-01 01:00 selinux_version
-rw-r--r-- root     root       115127 1970-01-01 01:00 sepolicy
-rw-r--r-- root     root         9438 1970-01-01 01:00 service_contexts
drwxr-x--x root     sdcard_r          2015-06-01 10:17 storage
dr-xr-xr-x root     root              2015-06-01 10:17 sys
drwxr-xr-x root     root              2015-05-13 01:54 system
-rw-r--r-- root     root         7155 1970-01-01 01:00 ueventd.manta.rc
-rw-r--r-- root     root         4464 1970-01-01 01:00 ueventd.rc
-rw-r--r-- root     root         2107 1970-01-01 01:00 ueventd.smdk5250.rc
lrwxrwxrwx root     root              2015-06-01 10:17 vendor -> /system/vendor
shell@manta:/storage/emulated/legacy $ ls / -l
ls / -l
drwxr-xr-x root     root              2015-06-01 10:17 acct
drwxrwx--- system   cache             2015-05-13 02:03 cache
lrwxrwxrwx root     root              1970-01-01 01:00 charger -> /sbin/healthd
dr-x------ root     root              2015-06-01 10:17 config
lrwxrwxrwx root     root              2015-06-01 10:17 d -> /sys/kernel/debug
drwxrwx--x system   system            2015-05-13 00:24 data
-rw-r--r-- root     root          297 1970-01-01 01:00 default.prop
drwxr-xr-x root     root              2015-06-01 10:17 dev
lrwxrwxrwx root     root              2015-06-01 10:17 etc -> /system/etc
lstat '//factory' failed: Permission denied
-rw-r--r-- root     root        12819 1970-01-01 01:00 file_contexts
-rw-r----- root     root         1523 1970-01-01 01:00 fstab.manta
-rw-r----- root     root          657 1970-01-01 01:00 fstab.smdk5250
-rwxr-x--- root     root       227652 1970-01-01 01:00 init
-rwxr-x--- root     root          944 1970-01-01 01:00 init.environ.rc
-rwxr-x--- root     root         6801 1970-01-01 01:00 init.manta.rc
-rwxr-x--- root     root         3061 1970-01-01 01:00 init.manta.usb.rc
-rwxr-x--- root     root        21728 1970-01-01 01:00 init.rc
-rwxr-x--- root     root          203 1970-01-01 01:00 init.recovery.manta.rc
-rwxr-x--- root     root         1754 1970-01-01 01:00 init.smdk5250.rc
-rwxr-x--- root     root         2109 1970-01-01 01:00 init.smdk5250.usb.rc
-rwxr-x--- root     root         1927 1970-01-01 01:00 init.trace.rc
-rwxr-x--- root     root         3885 1970-01-01 01:00 init.usb.rc
-rwxr-x--- root     root          301 1970-01-01 01:00 init.zygote32.rc
drwxrwxr-x root     system            2015-06-01 10:17 mnt
dr-xr-xr-x root     root              1970-01-01 01:00 proc
-rw-r--r-- root     root         2771 1970-01-01 01:00 property_contexts
drwxr-xr-x root     root              1970-01-01 01:00 res
drwx------ root     root              2015-02-25 01:40 root
drwxr-x--- root     root              1970-01-01 01:00 sbin
lrwxrwxrwx root     root              2015-06-01 10:17 sdcard -> /storage/emulat
ed/legacy
-rw-r--r-- root     root          471 1970-01-01 01:00 seapp_contexts
-rw-r--r-- root     root           56 1970-01-01 01:00 selinux_version
-rw-r--r-- root     root       115127 1970-01-01 01:00 sepolicy
-rw-r--r-- root     root         9438 1970-01-01 01:00 service_contexts
drwxr-x--x root     sdcard_r          2015-06-01 10:17 storage
dr-xr-xr-x root     root              2015-06-01 10:17 sys
drwxr-xr-x root     root              2015-05-13 01:54 system
-rw-r--r-- root     root         7155 1970-01-01 01:00 ueventd.manta.rc
-rw-r--r-- root     root         4464 1970-01-01 01:00 ueventd.rc
-rw-r--r-- root     root         2107 1970-01-01 01:00 ueventd.smdk5250.rc
lrwxrwxrwx root     root              2015-06-01 10:17 vendor -> /system/vendor
";

            output = string.Join(Environment.NewLine, output.Split(Environment.NewLine.ToCharArray()));
            byte[] data = Encoding.GetEncoding(MultiLineReceiver.ENCODING).GetBytes(output);

            // Storage is not a symlink -> output should be null
            LinkResolverReceiver receiver = new LinkResolverReceiver("storage");
            Assert.AreEqual("storage", receiver.FileName);

            receiver.AddOutput(data, 0, data.Length);
            receiver.Flush();
            Assert.IsNull(receiver.ResolvedPath);

            // vendor is a symlink to /system/vendor -> should return that symlink
            receiver = new LinkResolverReceiver("vendor");
            Assert.AreEqual("vendor", receiver.FileName);

            receiver.AddOutput(data, 0, data.Length);
            receiver.Flush();
            Assert.AreEqual("/system/vendor", receiver.ResolvedPath);

        }
    }
}
