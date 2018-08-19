//-----------------------------------------------------------------------
// <copyright file="VersionInfoReceiverTests.cs" company="Quamotion">
//     Copyright (c) 2015 Quamotion. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using SharpAdbClient;
using SharpAdbClient.DeviceCommands;
using System;
using System.IO;
using Xunit;

namespace Quamotion.Test.Devices.Android
{
    /// <summary>
    /// Tests the <see cref="VersionInfoReceiver"/> class.
    /// </summary>
    public class VersionInfoReceiverTests
    {
        /// <summary>
        /// Tests the <see cref="VersionInfoReceiver.GetVersionName(string)"/> and the <see cref="VersionInfoReceiver.GetVersionCode(string)"/> methods
        /// </summary>
        [Fact]
        public void GetVersionTest()
        {
            VersionInfoReceiver receiver = new VersionInfoReceiver();

            // Trick the receiver into thinking we're in the package section
            Assert.Null(receiver.GetVersionCode("Packages:"));

            Assert.Equal<int>(10210, (int)receiver.GetVersionCode(" versionCode=10210 targetSdk=18"));
            Assert.Null(receiver.GetVersionCode(null));
            Assert.Null(receiver.GetVersionCode(string.Empty));
            Assert.Null(receiver.GetVersionCode(" versionCode=10210targetSdk=18"));

            Assert.Equal("4.7.1", (string)receiver.GetVersionName("    versionName=4.7.1"));
            Assert.Null((string)receiver.GetVersionName(null));
            Assert.Null((string)receiver.GetVersionName(" test"));
            Assert.Null((string)receiver.GetVersionName("    versionName"));
            Assert.Equal(string.Empty, (string)receiver.GetVersionName("    versionName="));

            DeviceData device = new DeviceData();

            var dumpsys = string.Join(Environment.NewLine, File.ReadAllLines(@"dumpsys_package.txt"));
            receiver = new VersionInfoReceiver();

            StringReader reader = new StringReader(dumpsys);

            while (reader.Peek() >= 0)
            {
                receiver.AddOutput(reader.ReadLine());
            }

            receiver.Flush();

            Assert.Equal(10210, receiver.VersionInfo.VersionCode);
            Assert.Equal("4.7.1", receiver.VersionInfo.VersionName);
        }
    }
}