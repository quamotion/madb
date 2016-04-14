using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace SharpAdbClient.Tests
{
    [TestClass]
    public class SyncCommandConverterTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetCommandNullTest()
        {
            SyncCommandConverter.GetCommand(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void GetCommandInvalidNumberOfBytesTest()
        {
            SyncCommandConverter.GetCommand(new byte[] { });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void GetCommandInvalidCommandTest()
        {
            SyncCommandConverter.GetCommand(new byte[] { (byte)'Q', (byte)'M', (byte)'T', (byte)'V' });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void GetBytesInvalidCommandTest()
        {
            SyncCommandConverter.GetBytes((SyncCommand)99);
        }
    }
}
