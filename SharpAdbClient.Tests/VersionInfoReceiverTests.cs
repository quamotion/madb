//-----------------------------------------------------------------------
// <copyright file="VersionInfoReceiverTests.cs" company="Quamotion">
//     Copyright (c) 2015 Quamotion. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpAdbClient;
using SharpAdbClient.DeviceCommands;
using System;
using System.IO;

namespace Quamotion.Test.Devices.Android
{
    /// <summary>
    /// Tests the <see cref="VersionInfoReceiver"/> class.
    /// </summary>
    [TestClass]
    public class VersionInfoReceiverTests
    {
        /// <summary>
        /// Tests the <see cref="VersionInfoReceiver.GetVersionName(string)"/> and the <see cref="VersionInfoReceiver.GetVersionCode(string)"/> methods
        /// </summary>
        [TestMethod]
        [DeploymentItem(@"dumpsys_package.txt")]
        public void GetVersionTest()
        {
            VersionInfoReceiver receiver = new VersionInfoReceiver();
            Assert.AreEqual<int>(10210, (int)receiver.GetVersionCode("versionCode=10210 targetSdk=18"));
            Assert.AreEqual<object>(null, receiver.GetVersionCode(null));
            Assert.AreEqual<object>(null, receiver.GetVersionCode(string.Empty));
            Assert.AreEqual<object>(null, receiver.GetVersionCode("versionCode=10210targetSdk=18"));

            Assert.AreEqual<string>("4.7.1", (string)receiver.GetVersionName("    versionName=4.7.1"));
            Assert.AreEqual<string>(null, (string)receiver.GetVersionName(null));
            Assert.AreEqual<string>(null, (string)receiver.GetVersionName(" test"));
            Assert.AreEqual<string>(null, (string)receiver.GetVersionName("    versionName"));
            Assert.AreEqual<string>(string.Empty, (string)receiver.GetVersionName("    versionName="));

            DeviceData device = new DeviceData();

            var dumpsys = string.Join(Environment.NewLine, File.ReadAllLines(@"dumpsys_package.txt"));


            StringReader reader = new StringReader(dumpsys);

            while (reader.Peek() >= 0)
            {
                receiver.AddOutput(reader.ReadLine());
            }

            receiver.Flush();

            Assert.AreEqual(10210, receiver.VersionInfo.VersionCode);
            Assert.AreEqual("4.7.1", receiver.VersionInfo.VersionName);
        }
    }
}