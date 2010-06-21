using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Managed.Adb {
	public enum ClientChangeMask {
		ChangeInfo,
		ChangeDebuggerStatus,
		ChangeThreadMode,
		ChangeThreadData,
		ChangeHeapMode,
		ChangeHeapData,
		ChangeNatvieHeapData
	}

	public class ClientEventArgs : EventArgs {

		/// <summary>
		/// Initializes a new instance of the <see cref="ClientEventArgs"/> class.
		/// </summary>
		/// <param name="client">The client.</param>
		/// <param name="changeMask">The change mask.</param>
		public ClientEventArgs ( IClient client, ClientChangeMask changeMask ) {
			this.Client = client;
			this.ChangeMask = changeMask;
		}

		/// <summary>
		/// Gets the change mask.
		/// </summary>
		/// <value>The change mask.</value>
		public ClientChangeMask ChangeMask { get; private set; }
		/// <summary>
		/// Gets the client.
		/// </summary>
		/// <value>The client.</value>
		public IClient Client { get; private set; }

	}
}
