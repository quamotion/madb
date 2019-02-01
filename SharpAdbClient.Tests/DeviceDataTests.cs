using Xunit;
using System;

namespace SharpAdbClient.Tests
{
    public class DeviceDataTests
    {
        [Fact]
        public void CreateFromDeviceDataVSEmulatorTest()
        {
            string data = @"169.254.138.177:5555   offline product:VS Emulator Android Device - 480 x 800 model:Android_Device___480_x_800 device:donatello";

            var device = DeviceData.CreateFromAdbData(data);
            Assert.Equal("169.254.138.177:5555", device.Serial);
            Assert.Equal("VS Emulator Android Device - 480 x 800", device.Product);
            Assert.Equal("Android_Device___480_x_800", device.Model);
            Assert.Equal("donatello", device.Name);
            Assert.Equal<DeviceState>(DeviceState.Offline, device.State);
            Assert.Equal(string.Empty, device.Usb);
        }

        [Fact]
        public void CreateFromDeviceNoPermissionTest2()
        {
            string data = "009d1cd696d5194a        no permissions (user in plugdev group; are your udev rules wrong?); see [http://developer.android.com/tools/device.html";

            var device = DeviceData.CreateFromAdbData(data);
            Assert.Equal("009d1cd696d5194a", device.Serial);
            Assert.Equal(string.Empty, device.Product);
            Assert.Equal(string.Empty, device.Model);
            Assert.Equal(string.Empty, device.Name);
            Assert.Equal(string.Empty, device.Features);
            Assert.Equal<DeviceState>(DeviceState.NoPermissions, device.State);
            Assert.Equal(" (user in plugdev group; are your udev rules wrong?); see [http://developer.android.com/tools/device.html", device.Message);
            Assert.Equal(string.Empty, device.Usb);
            Assert.Equal(string.Empty, device.TransportId);
        }

        [Fact]
        public void CreateFromDeviceDataAuthorizingTest()
        {
            string data = "52O00ULA01             authorizing usb:9-1.4.1 transport_id:8149";

            var device = DeviceData.CreateFromAdbData(data);
            Assert.Equal("52O00ULA01", device.Serial);
            Assert.Equal(string.Empty, device.Product);
            Assert.Equal(string.Empty, device.Model);
            Assert.Equal(string.Empty, device.Name);
            Assert.Equal(string.Empty, device.Features);
            Assert.Equal<DeviceState>(DeviceState.Authorizing, device.State);
            Assert.Equal("9-1.4.1", device.Usb);
            Assert.Equal("8149", device.TransportId);
        }

        [Fact]
        public void CreateFromDeviceDataUnauthorizedTest()
        {
            string data = "R32D102SZAE            unauthorized";

            var device = DeviceData.CreateFromAdbData(data);
            Assert.Equal("R32D102SZAE", device.Serial);
            Assert.Equal("", device.Product);
            Assert.Equal("", device.Model);
            Assert.Equal("", device.Name);
            Assert.Equal<DeviceState>(DeviceState.Unauthorized, device.State);
            Assert.Equal(string.Empty, device.Usb);
        }

        [Fact]
        public void CreateFromEmulatorTest()
        {
            string data = "emulator-5586          host features:shell_2";

            var device = DeviceData.CreateFromAdbData(data);
            Assert.Equal("emulator-5586", device.Serial);
            Assert.Equal<DeviceState>(DeviceState.Host, device.State);
            Assert.Equal("shell_2", device.Features);
            Assert.Equal(string.Empty, device.Usb);
        }

        [Fact]
        public void CreateWithFeaturesTest()
        {
            string data = "0100a9ee51a18f2b device product:bullhead model:Nexus_5X device:bullhead features:shell_v2,cmd";

            var device = DeviceData.CreateFromAdbData(data);
            Assert.Equal("0100a9ee51a18f2b", device.Serial);
            Assert.Equal<DeviceState>(DeviceState.Online, device.State);
            Assert.Equal("Nexus_5X", device.Model);
            Assert.Equal("bullhead", device.Product);
            Assert.Equal("bullhead", device.Name);
            Assert.Equal("shell_v2,cmd", device.Features);
            Assert.Equal(string.Empty, device.Usb);
        }

        [Fact]
        public void CreateWithUsbDataTest()
        {
            // As seen on Linux
            string data = "EAOKCY112414           device usb:1-1 product:WW_K013 model:K013 device:K013_1";

            var device = DeviceData.CreateFromAdbData(data);
            Assert.Equal("EAOKCY112414", device.Serial);
            Assert.Equal<DeviceState>(DeviceState.Online, device.State);
            Assert.Equal("K013", device.Model);
            Assert.Equal("WW_K013", device.Product);
            Assert.Equal("K013_1", device.Name);
            Assert.Equal("1-1", device.Usb);
        }

        [Fact]
        public void CreateWithoutModelTest()
        {
            // As seen for devices in recovery mode
            // See https://github.com/quamotion/madb/pull/85/files
            string data = "ZY3222LBDC recovery usb:337641472X product:omni_cedric device:cedric";

            var device = DeviceData.CreateFromAdbData(data);
            Assert.Equal("ZY3222LBDC", device.Serial);
            Assert.Equal<DeviceState>(DeviceState.Recovery, device.State);
            Assert.Equal("337641472X", device.Usb);
            Assert.Equal(string.Empty, device.Model);
            Assert.Equal("omni_cedric", device.Product);
            Assert.Equal("cedric", device.Name);
        }

        [Fact]
        public void CreateNoPermissionTest()
        {
            string data = "009d1cd696d5194a     no permissions";

            var device = DeviceData.CreateFromAdbData(data);
            Assert.Equal("009d1cd696d5194a", device.Serial);
            Assert.Equal(DeviceState.NoPermissions, device.State);
        }

        [Fact]
        public void CreateWithUnderscoresTest()
        {
            string data = "99000000               device product:if_s200n model:NL_V100KR device:if_s200n";

            var device = DeviceData.CreateFromAdbData(data);
            Assert.Equal("99000000", device.Serial);
            Assert.Equal("if_s200n", device.Product);
            Assert.Equal("NL_V100KR", device.Model);
            Assert.Equal("if_s200n", device.Name);
        }

        [Fact]
        public void CreateFromInvalidDatatest()
        {
            string data = "xyz";

            Assert.Throws<ArgumentException>(() => DeviceData.CreateFromAdbData(data));
        }

        [Fact]
        public void ToStringTest()
        {
            DeviceData d = new DeviceData();
            d.Serial = "xyz";

            Assert.Equal("xyz", d.ToString());
        }

        [Fact]
        public void GetStateFromStringTest()
        {
            Assert.Equal(DeviceState.NoPermissions, DeviceData.GetStateFromString("no permissions"));
            Assert.Equal(DeviceState.Unknown, DeviceData.GetStateFromString("hello"));
        }

        [Fact]
        public void CreateFromDeviceDataTransportIdTest()
        {
            string data = "R32D102SZAE            device transport_id:6";

            var device = DeviceData.CreateFromAdbData(data);
            Assert.Equal("R32D102SZAE", device.Serial);
            Assert.Equal("", device.Product);
            Assert.Equal("", device.Model);
            Assert.Equal("", device.Name);
            Assert.Equal<DeviceState>(DeviceState.Online, device.State);
            Assert.Equal(string.Empty, device.Usb);
        }

        [Fact]
        public void CreateFromDeviceDataTransportIdTest2()
        {
            string data = "emulator-5554          device product:sdk_google_phone_x86 model:Android_SDK_built_for_x86 device:generic_x86 transport_id:1";

            var device = DeviceData.CreateFromAdbData(data);
            Assert.Equal("emulator-5554", device.Serial);
            Assert.Equal("sdk_google_phone_x86", device.Product);
            Assert.Equal("Android_SDK_built_for_x86", device.Model);
            Assert.Equal("generic_x86", device.Name);
            Assert.Equal("1", device.TransportId);
            Assert.Equal<DeviceState>(DeviceState.Online, device.State);
            Assert.Equal(string.Empty, device.Usb);
        }

        [Fact]
        public void CreateFromDeviceDataTransportIdTest3()
        {
            string data = "00bc13bcf4bacc62 device product:bullhead model:Nexus_5X device:bullhead transport_id:1";

            var device = DeviceData.CreateFromAdbData(data);
            Assert.Equal("00bc13bcf4bacc62", device.Serial);
            Assert.Equal("bullhead", device.Product);
            Assert.Equal("Nexus_5X", device.Model);
            Assert.Equal("bullhead", device.Name);
            Assert.Equal("1", device.TransportId);
            Assert.Equal<DeviceState>(DeviceState.Online, device.State);
            Assert.Equal(string.Empty, device.Usb);
        }

        [Fact]
        public void CreateFromDeviceDataConnecting()
        {
            string data = "00bc13bcf4bacc62 connecting";

            var device = DeviceData.CreateFromAdbData(data);
            Assert.Equal("00bc13bcf4bacc62", device.Serial);
            Assert.Equal(string.Empty, device.Product);
            Assert.Equal(string.Empty, device.Model);
            Assert.Equal(string.Empty, device.Name);
            Assert.Equal(string.Empty, device.TransportId);
            Assert.Equal<DeviceState>(DeviceState.Unknown, device.State);
            Assert.Equal(string.Empty, device.Usb);
        }

    }
}
