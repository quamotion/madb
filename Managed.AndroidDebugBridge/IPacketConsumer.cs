using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Managed.Adb {
	public interface IPacketConsumer {
		void Read ( );
		// TODO: JdwpPacket
		/*JdwpPacket*/ Object GetJdwpPacket ( );
		void ForwardPacketToClient ( /*JdwpPacket*/ Object packet );
		void SendHandshake ( );
		void SendAndConsume ( /*JdwpPacket*/ Object packet );
	}
}
