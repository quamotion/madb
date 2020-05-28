using SharpAdbClient.DeviceCommands;
using Xunit;

namespace SharpAdbClient.Tests
{
    public class PackageManagerReceiverTests
    {
        [Fact]
        public void ParseThirdPartyPackage()
        {
            // Arrange
            DeviceData device = new DeviceData()
            {
                State = DeviceState.Online
            };

            DummyAdbClient client = new DummyAdbClient();

            PackageManager manager = new PackageManager(client, device, thirdPartyOnly: false, syncServiceFactory: null, skipInit: true);
            PackageManagerReceiver receiver = new PackageManagerReceiver(device, manager);

            // Act
            receiver.AddOutput("package:/data/app/com.google.android.apps.plus-qQaDuXCpNqJuQSbIS6OxGA==/base.apk=com.google.android.apps.plus");
            receiver.AddOutput("package:/system/app/LegacyCamera.apk=com.android.camera");
            receiver.AddOutput("package:mwc2015.be");
            receiver.Flush();

            // Assert
            Assert.Equal(3, manager.Packages.Count);
            Assert.True(manager.Packages.ContainsKey("com.google.android.apps.plus"));
            Assert.True(manager.Packages.ContainsKey("com.android.camera"));
            Assert.True(manager.Packages.ContainsKey("mwc2015.be"));

            Assert.Equal("/data/app/com.google.android.apps.plus-qQaDuXCpNqJuQSbIS6OxGA==/base.apk", manager.Packages["com.google.android.apps.plus"]);
        }
    }
}
