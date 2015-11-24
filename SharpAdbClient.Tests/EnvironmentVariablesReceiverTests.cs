using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpAdbClient.Tests
{
    [TestClass]
    public class EnvironmentVariablesReceiverTests
    {
        [TestMethod]
        public void EnvironmentVariablesReceiverTest()
        {
            EnvironmentVariablesReceiver receiver = new EnvironmentVariablesReceiver();
            receiver.AddOutput("XDG_VTNR=7");
            receiver.AddOutput("XDG_SESSION_ID=c1");
            receiver.AddOutput("CLUTTER_IM_MODULE=xim");
            receiver.AddOutput("GNOME_KEYRING_PID=");
            receiver.AddOutput("#GNOME_KEYRING_PID=test");
            receiver.Flush();

            Assert.AreEqual(4, receiver.EnvironmentVariables.Count);
            Assert.IsTrue(receiver.EnvironmentVariables.ContainsKey("XDG_VTNR"));
            Assert.IsTrue(receiver.EnvironmentVariables.ContainsKey("XDG_SESSION_ID"));
            Assert.IsTrue(receiver.EnvironmentVariables.ContainsKey("CLUTTER_IM_MODULE"));
            Assert.IsTrue(receiver.EnvironmentVariables.ContainsKey("GNOME_KEYRING_PID"));

            Assert.AreEqual("7", receiver.EnvironmentVariables["XDG_VTNR"]);
            Assert.AreEqual("c1", receiver.EnvironmentVariables["XDG_SESSION_ID"]);
            Assert.AreEqual("xim", receiver.EnvironmentVariables["CLUTTER_IM_MODULE"]);
            Assert.AreEqual(string.Empty, receiver.EnvironmentVariables["GNOME_KEYRING_PID"]);
        }
    }
}
