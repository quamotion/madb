using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace Managed.Adb.Tests {
	public class BatteryInfoTests : BaseDeviceTests {

		[Fact]
		public void GetBatteryInfoTest ( ) {
			Device device = GetFirstDevice ( );
			Assert.NotNull ( device );

			var batteryInfo = device.GetBatteryInfo ( );
			Assert.True ( batteryInfo.Present );
			Console.WriteLine ( batteryInfo.ToString ( ) );
		}

	}
}
