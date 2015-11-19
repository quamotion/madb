using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpAdbClient.Exceptions;
using System;
using System.Drawing.Imaging;
using System.IO;

namespace SharpAdbClient.Tests
{
    [TestClass]
    public class AdbHelperIntegrationTests : BaseDeviceTests
    {
        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void DeviceGetMountPointsTest()
        {
            Device device = GetFirstDevice();
            foreach (var item in device.MountPoints.Keys)
            {
                Console.WriteLine(device.MountPoints[item]);
            }

            Assert.IsTrue(device.MountPoints.ContainsKey("/system"));
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void DeviceRemountMountPointTest()
        {
            Device device = GetFirstDevice();

            Assert.IsTrue(device.MountPoints.ContainsKey("/system"), "Device does not contain mount point /system");
            bool isReadOnly = device.MountPoints["/system"].IsReadOnly;

            device.RemountMountPoint(device.MountPoints["/system"], !isReadOnly);

            Assert.AreEqual<bool>(!isReadOnly, device.MountPoints["/system"].IsReadOnly);
            Console.WriteLine("Successfully mounted /system as {0}", !isReadOnly ? "ro" : "rw");

            // revert it back...
            device.RemountMountPoint(device.MountPoints["/system"], isReadOnly);
            Assert.AreEqual<bool>(isReadOnly, device.MountPoints["/system"].IsReadOnly);
            Console.WriteLine("Successfully mounted /system as {0}", isReadOnly ? "ro" : "rw");

        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void ExecuteRemoteCommandTest()
        {

            Device device = GetFirstDevice();
            ConsoleOutputReceiver creciever = new ConsoleOutputReceiver();


            device.ExecuteShellCommand("pm list packages -f", creciever);

            Console.WriteLine("Executing 'ls':");
            try
            {
                device.ExecuteShellCommand("ls -lF --color=never", creciever);
            }
            catch (UnknownOptionException)
            {
                device.ExecuteShellCommand("ls -l", creciever);
            }


            Console.WriteLine("Executing 'busybox':");
            bool hasBB = false;
            try
            {
                device.ExecuteShellCommand("busybox", creciever);
                hasBB = true;
            }
            catch (FileNotFoundException)
            {
                hasBB = false;
            }
            finally
            {
                Console.WriteLine("Busybox enabled: {0}", hasBB);
            }

            Console.WriteLine("Executing 'unknowncommand':");
            try
            {
                device.ExecuteShellCommand("unknowncommand", creciever);
                Assert.Fail();
            }
            catch (FileNotFoundException)
            {
                // Expected exception
            }

            Console.WriteLine("Executing 'ls /system/foo'");
            try
            {
                device.ExecuteShellCommand("ls /system/foo", creciever);
                Assert.Fail();
            }
            catch (FileNotFoundException)
            {
                // Expected exception
            }

        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void ExecuteRemoteRootCommandTest()
        {
            Device device = GetFirstDevice();
            ConsoleOutputReceiver creciever = new ConsoleOutputReceiver();

            Console.WriteLine("Executing 'ls':");
            if (device.CanSU())
            {
                try
                {
                    device.ExecuteRootShellCommand("busybox ls -lFa --color=never", creciever);
                }
                catch (UnknownOptionException)
                {
                    device.ExecuteRootShellCommand("ls -lF", creciever);
                }
            }
            else
            {
                // if the device doesn't have root, then we check that it is throwing the PermissionDeniedException
                try
                {
                    try
                    {
                        device.ExecuteRootShellCommand("busybox ls -lFa --color=never", creciever);
                    }
                    catch (UnknownOptionException)
                    {
                        device.ExecuteRootShellCommand("ls -lF", creciever);
                    }

                    Assert.Fail();
                }
                catch (PermissionDeniedException)
                {
                    // Expected exception
                }

            }
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void DeviceEnvironmentVariablesTest()
        {
            Device device = GetFirstDevice();
            foreach (var key in device.EnvironmentVariables.Keys)
            {
                Console.WriteLine("{0}={1}", key, device.EnvironmentVariables[key]);
            }

            Assert.IsTrue(device.EnvironmentVariables.Count > 0);
            Assert.IsTrue(device.EnvironmentVariables.ContainsKey("ANDROID_ROOT"));
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void DevicePropertiesTest()
        {
            Device device = GetFirstDevice();
            foreach (var key in device.Properties.Keys)
            {
                Console.WriteLine("[{0}]: {1}", key, device.Properties[key]);
            }

            Assert.IsTrue(device.Properties.Count > 0);
            Assert.IsTrue(device.Properties.ContainsKey("ro.product.device"));
        }
    }
}
