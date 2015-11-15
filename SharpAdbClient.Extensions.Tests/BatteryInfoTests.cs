using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpAdbClient.Tests {
    [TestClass]
	public class BatteryInfoTests : BaseDeviceTests {

        [TestMethod]
        [TestCategory("IntegrationTest")]
		public void GetBatteryInfoTest ( ) {
			Device device = GetFirstDevice ( );
			Assert.IsNotNull ( device );

			var batteryInfo = device.GetBatteryInfo ( );
			Assert.IsTrue ( batteryInfo.Present );
			Console.WriteLine ( batteryInfo.ToString ( ) );
		}

	}
}
