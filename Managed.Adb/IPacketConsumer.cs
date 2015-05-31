using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Managed.Adb {
	/// <summary>
	/// 
	/// </summary>
	public interface IPacketConsumer {
		/// <summary>
		/// Reads this instance.
		/// </summary>
		void Read ( );
		// TODO: JdwpPacket
		/*JdwpPacket*/ Object GetJdwpPacket ( );
		/// <summary>
		/// Forwards the packet to client.
		/// </summary>
		/// <param name="packet">The packet.</param>
		void ForwardPacketToClient ( /*JdwpPacket*/ Object packet );
		/// <summary>
		/// Sends the handshake.
		/// </summary>
		bool SendHandshake ( );
		/// <summary>
		/// Sends the and consume.
		/// </summary>
		/// <param name="packet">The packet.</param>
		void SendAndConsume ( /*JdwpPacket*/ Object packet );
	}
}
