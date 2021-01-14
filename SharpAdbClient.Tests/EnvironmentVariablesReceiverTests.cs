using System.Linq;
using Xunit;

namespace SharpAdbClient.Tests
{
    public class EnvironmentVariablesReceiverTests
    {
        [Fact]
        public void EnvironmentVariablesReceiverTest()
        {
            EnvironmentVariablesReceiver receiver = new EnvironmentVariablesReceiver();
            receiver.AddOutput("XDG_VTNR=7");
            receiver.AddOutput("XDG_SESSION_ID=c1");
            receiver.AddOutput("CLUTTER_IM_MODULE=xim");
            receiver.AddOutput("GNOME_KEYRING_PID=");
            receiver.AddOutput("#GNOME_KEYRING_PID=test");
            receiver.Flush();

            Assert.Equal(4, receiver.EnvironmentVariables.Count);
            Assert.True(receiver.EnvironmentVariables.ContainsKey("XDG_VTNR"));
            Assert.True(receiver.EnvironmentVariables.ContainsKey("XDG_SESSION_ID"));
            Assert.True(receiver.EnvironmentVariables.ContainsKey("CLUTTER_IM_MODULE"));
            Assert.True(receiver.EnvironmentVariables.ContainsKey("GNOME_KEYRING_PID"));

            Assert.Equal("7", receiver.EnvironmentVariables["XDG_VTNR"]);
            Assert.Equal("c1", receiver.EnvironmentVariables["XDG_SESSION_ID"]);
            Assert.Equal("xim", receiver.EnvironmentVariables["CLUTTER_IM_MODULE"]);
            Assert.Equal(string.Empty, receiver.EnvironmentVariables["GNOME_KEYRING_PID"]);
        }
    }
}
