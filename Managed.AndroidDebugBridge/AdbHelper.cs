using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Threading;
using Managed.Adb.Exceptions;
using MoreLinq;
using Managed.Adb.IO;
using Managed.Adb.Logs;

// services that are supported by adb: https://github.com/android/platform_system_core/blob/master/adb/SERVICES.TXT
namespace Managed.Adb {

	public enum TransportType {
		Usb,
		Local,
		Any,
		Host
	}

	/// <summary>
	/// The ADB Helper class
	/// </summary>
	public class AdbHelper {
		/// <summary>
		/// Logging tag
		/// </summary>
		private const string TAG = "AdbHelper";
		/// <summary>
		/// The time to wait
		/// </summary>
		private const int WAIT_TIME = 5;
		/// <summary>
		/// The default encoding
		/// </summary>
		public static String DEFAULT_ENCODING = "ISO-8859-1";

		/// <summary>
		/// Prevents a default instance of the <see cref="AdbHelper"/> class from being created.
		/// </summary>
		private AdbHelper() {

		}

		/// <summary>
		/// 
		/// </summary>
		private static AdbHelper _instance = null;
		/// <summary>
		/// Gets an instance of the <see cref="AdbHelper"/> class
		/// </summary>
		public static AdbHelper Instance {
			get {
				if(_instance == null) {
					_instance = new AdbHelper();
				}
				return _instance;
			}
		}

		/// <summary>
		/// Opens the specified address on the device on the specified port.
		/// </summary>
		/// <param name="address">The address.</param>
		/// <param name="device">The device.</param>
		/// <param name="port">The port.</param>
		/// <returns></returns>
		public Socket Open(IPAddress address, IDevice device, int port) {
			Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			try {
				s.Connect(address, port);
				s.Blocking = true;
				s.NoDelay = false;

				SetDevice(s, device);

				byte[] req = CreateAdbForwardRequest(null, port);
				if(!Write(s, req)) {
					throw new AdbException("failed submitting request to ADB");
				}
				AdbResponse resp = ReadAdbResponse(s, false);
				if(!resp.Okay) {
					throw new AdbException("connection request rejected");
				}
				s.Blocking = true;
			} catch(AdbException) {
				s.Close();
				throw;
			}
			return s;
		}

		/// <summary>
		/// Kills the adb server.
		/// </summary>
		/// <param name="address">The address.</param>
		/// <returns></returns>
		/// <exception cref="System.IO.IOException">failed asking to kill adb</exception>
		public int KillAdb(IPEndPoint address) {
			byte[] request = FormAdbRequest("host:kill");
			using(var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)) {
				socket.Connect(address);
				socket.Blocking = true;
				if(!Write(socket, request)) {
					throw new IOException("failed asking to kill adb");
				}
				var resp = ReadAdbResponse(socket, false);
				if(!resp.IOSuccess || !resp.Okay) {
					Log.e(TAG, "Got timeout or unhappy response from ADB req: " + resp.Message);
					socket.Close();
					return -1;
				}
				return 0;
			}
		}


		//https://github.com/android/platform_system_core/blob/master/adb/backup_service.c
		/// <summary>
		/// Backups the specified address.
		/// </summary>
		/// <param name="address">The address.</param>
		/// <exception cref="System.IO.IOException">failed asking to backup device</exception>
		[Obsolete("This is not yet functional")]
		public void Backup(IPEndPoint address) {
			byte[] request = FormAdbRequest("backup:all");
			using(var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)) {
				socket.Connect(address);
				socket.Blocking = true;
				if(!Write(socket, request)) {
					throw new IOException("failed asking to backup device");
				}
				var resp = ReadAdbResponse(socket, false);
				if(!resp.IOSuccess || !resp.Okay) {
					Log.e(TAG, "Got timeout or unhappy response from ADB req: " + resp.Message);
					socket.Close();
					return;
				}

				var data = new byte[6000];
				int count = -1;
				while(count != 0) {
					count = socket.Receive(data);
					Console.Write("received: {0}", count);
				}
			}
		}

		// https://github.com/android/platform_system_core/blob/master/adb/backup_service.c
		/// <summary>
		/// Restores this instance.
		/// </summary>
		/// <exception cref="System.NotImplementedException"></exception>
		[Obsolete("This is not yet functional")]
		public void Restore() {
			throw new NotImplementedException();
		}

		/// <summary>
		/// Gets the adb version.
		/// </summary>
		/// <param name="address">The address.</param>
		/// <returns></returns>
		public int GetAdbVersion(IPEndPoint address) {
			byte[] request = FormAdbRequest("host:version");
			byte[] reply;
			Socket adbChan = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			try {
				adbChan.Connect(address);
				adbChan.Blocking = true;
				if(!Write(adbChan, request))
					throw new IOException("failed asking for adb version");

				AdbResponse resp = ReadAdbResponse(adbChan, false /* readDiagString */);
				if(!resp.IOSuccess || !resp.Okay) {
					Log.e(TAG, "Got timeout or unhappy response from ADB fb req: " + resp.Message);
					adbChan.Close();
					return -1;
				}

				reply = new byte[4];
				if(!Read(adbChan, reply)) {
					Log.e(TAG, "error in getting data length");

					adbChan.Close();
					return -1;
				}

				String lenHex = reply.GetString(AdbHelper.DEFAULT_ENCODING);
				int len = int.Parse(lenHex, System.Globalization.NumberStyles.HexNumber);

				// the protocol version.
				reply = new byte[len];
				if(!Read(adbChan, reply)) {
					Log.e(TAG, "did not get the version info");

					adbChan.Close();
					return -1;
				}

				String sReply = reply.GetString(AdbHelper.DEFAULT_ENCODING);
				return int.Parse(sReply, System.Globalization.NumberStyles.HexNumber);

			} catch(Exception ex) {
				Console.WriteLine(ex);
				throw;
			}
		}

		/// <summary>
		/// Creates and connects a new pass-through socket, from the host to a port on the device.
		/// </summary>
		/// <param name="endpoint"></param>
		/// <param name="device">the device to connect to. Can be null in which case the connection will be 
		/// to the first available device.</param>
		/// <param name="pid">the process pid to connect to.</param>
		/// <returns>The Socket</returns>
		public Socket CreatePassThroughConnection(IPEndPoint endpoint, Device device, int pid) {
			Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			try {
				socket.Connect(endpoint);
				socket.NoDelay = true;

				// if the device is not -1, then we first tell adb we're looking to
				// talk to a specific device
				SetDevice(socket, device);

				byte[] req = CreateJdwpForwardRequest(pid);
				// Log.hexDump(req);

				if(!Write(socket, req))
					throw new AdbException("failed submitting request to ADB"); //$NON-NLS-1$

				AdbResponse resp = ReadAdbResponse(socket, false /* readDiagString */);
				if(!resp.Okay)
					throw new AdbException("connection request rejected: " + resp.Message); //$NON-NLS-1$

			} catch(AdbException ioe) {
				socket.Close();
				throw ioe;
			}

			return socket;
		}

		/// <summary>
		/// Creates the adb forward request.
		/// </summary>
		/// <param name="address">The address.</param>
		/// <param name="port">The port.</param>
		/// <returns></returns>
		public byte[] CreateAdbForwardRequest(String address, int port) {
			String request;

			if(address == null)
				request = "tcp:" + port;
			else
				request = "tcp:" + port + ":" + address;
			return FormAdbRequest(request);
		}

		/// <summary>
		/// Forms the adb request.
		/// </summary>
		/// <param name="req">The req.</param>
		/// <returns></returns>
		public byte[] FormAdbRequest(String req) {
			String resultStr = String.Format("{0}{1}\n", req.Length.ToString("X4"), req);
			byte[] result;
			try {
				result = resultStr.GetBytes(AdbHelper.DEFAULT_ENCODING);
			} catch(EncoderFallbackException efe) {
				Log.e(TAG, efe);
				return null;
			}

			System.Diagnostics.Debug.Assert(result.Length == req.Length + 5, String.Format("result: {1}{0}\nreq: {3}{2}", result.Length, result.GetString(AdbHelper.DEFAULT_ENCODING), req.Length, req));
			return result;
		}

		/// <summary>
		/// Writes the specified data to the specified socket.
		/// </summary>
		/// <param name="socket">The socket.</param>
		/// <param name="data">The data.</param>
		/// <returns></returns>
		public bool Write(Socket socket, byte[] data) {
			try {
				Write(socket, data, -1, DdmPreferences.Timeout);
			} catch(IOException e) {
				Log.e(TAG, e);
				return false;
			}

			return true;
		}

		/// <summary>
		/// Writes the specified data to the specified socket.
		/// </summary>
		/// <param name="socket">The socket.</param>
		/// <param name="data">The data.</param>
		/// <param name="length">The length.</param>
		/// <param name="timeout">The timeout.</param>
		public void Write(Socket socket, byte[] data, int length, int timeout) {
			//using ( var buf = new MemoryStream ( data, 0, length != -1 ? length : data.Length ) ) {
			int numWaits = 0;
			int count = -1;

			//while ( buf.Position != buf.Length ) {
			try {
				count = socket.Send(data, 0, length != -1 ? length : data.Length, SocketFlags.None);
				if(count < 0) {
					throw new AdbException("channel EOF");
				} else if(count == 0) {
					// TODO: need more accurate timeout?
					if(timeout != 0 && numWaits * WAIT_TIME > timeout) {
						throw new AdbException("timeout");
					}
					// non-blocking spin
					Thread.Sleep(WAIT_TIME);
					numWaits++;
				} else {
					numWaits = 0;
				}
			} catch(SocketException sex) {
				Console.WriteLine(sex);
				throw;
			}
			//}
			//}
		}

		/// <summary>
		/// Reads the adb response.
		/// </summary>
		/// <param name="socket">The socket.</param>
		/// <param name="readDiagString">if set to <c>true</c> [read diag string].</param>
		/// <returns></returns>
		public AdbResponse ReadAdbResponse(Socket socket, bool readDiagString) {

			AdbResponse resp = new AdbResponse();

			byte[] reply = new byte[4];
			if(!Read(socket, reply)) {
				return resp;
			}
			resp.IOSuccess = true;

			if(IsOkay(reply)) {
				resp.Okay = true;
			} else {
				readDiagString = true; // look for a reason after the FAIL
				resp.Okay = false;
			}

			// not a loop -- use "while" so we can use "break"
			while(readDiagString) {
				// length string is in next 4 bytes
				byte[] lenBuf = new byte[4];
				if(!Read(socket, lenBuf)) {
					Console.WriteLine("Expected diagnostic string not found");
					break;
				}

				String lenStr = ReplyToString(lenBuf);

				int len;
				try {
					len = int.Parse(lenStr, System.Globalization.NumberStyles.HexNumber);

				} catch(FormatException) {
					Log.e(TAG, "Expected digits, got '{0}' : {1} {2} {3} {4}", lenBuf[0], lenBuf[1], lenBuf[2], lenBuf[3]);
					Log.e(TAG, "reply was {0}", ReplyToString(reply));
					break;
				}

				byte[] msg = new byte[len];
				if(!Read(socket, msg)) {
					Log.e(TAG, "Failed reading diagnostic string, len={0}", len);
					break;
				}

				resp.Message = ReplyToString(msg);
				Log.e(TAG, "Got reply '{0}', diag='{1}'", ReplyToString(reply), resp.Message);

				break;
			}

			return resp;
		}

		/// <summary>
		/// Reads the data from specified socket.
		/// </summary>
		/// <param name="socket">The socket.</param>
		/// <param name="data">The data.</param>
		/// <returns></returns>
		public bool Read(Socket socket, byte[] data) {
			try {
				Read(socket, data, -1, DdmPreferences.Timeout);
			} catch(AdbException e) {
				return false;
			}

			return true;
		}

		/// <summary>
		/// Reads the data from specified socket.
		/// </summary>
		/// <param name="socket">The socket.</param>
		/// <param name="data">The data.</param>
		/// <param name="length">The length.</param>
		/// <param name="timeout">The timeout.</param>
		public void Read(Socket socket, byte[] data, int length, int timeout) {
			int expLen = length != -1 ? length : data.Length;
			int count = -1;
			int totalRead = 0;

			while(count != 0 && totalRead < expLen) {
				try {
					int left = expLen - totalRead;
					int buflen = left < socket.ReceiveBufferSize ? left : socket.ReceiveBufferSize;

					byte[] buffer = new byte[buflen];
					socket.ReceiveBufferSize = expLen;
					count = socket.Receive(buffer, buflen, SocketFlags.None);
					if(count < 0) {
						Log.e(TAG, "read: channel EOF");
						throw new AdbException("EOF");
					} else if(count == 0) {
						Console.WriteLine("DONE with Read");
					} else {
						Array.Copy(buffer, 0, data, totalRead, count);
						totalRead += count;
					}
				} catch(SocketException sex) {
					throw new AdbException(String.Format("No Data to read: {0}", sex.Message));
				}
			}

		}

		/// <summary>
		/// Creates the JDWP forward request.
		/// </summary>
		/// <param name="pid">The pid.</param>
		/// <returns></returns>
		private byte[] CreateJdwpForwardRequest(int pid) {
			String req = String.Format("jdwp:{0}", pid);
			return FormAdbRequest(req);
		}

		/// <summary>
		/// Creates the forward.
		/// </summary>
		/// <param name="adbSockAddr">The adb sock addr.</param>
		/// <param name="device">The device.</param>
		/// <param name="localPort">The local port.</param>
		/// <param name="remotePort">The remote port.</param>
		/// <returns></returns>
		public bool CreateForward(IPEndPoint adbSockAddr, Device device, int localPort, int remotePort) {

			Socket adbChan = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			try {
				adbChan.Connect(adbSockAddr);
				adbChan.Blocking = true;

				// host-serial should be different based on the transport...


				byte[] request = FormAdbRequest(String.Format("host-serial:{0}:forward:tcp:{1};tcp:{2}", //$NON-NLS-1$
								device.SerialNumber, localPort, remotePort));

				if(!Write(adbChan, request)) {
					throw new AdbException("failed to submit the forward command.");
				}

				AdbResponse resp = ReadAdbResponse(adbChan, false /* readDiagString */);
				if(!resp.IOSuccess || !resp.Okay) {
					throw new AdbException("Device rejected command: " + resp.Message);
				}
			} finally {
				if(adbChan != null) {
					adbChan.Close();
				}
			}

			return true;
		}

		public void ListForward(IPEndPoint address, Device device) {

		}

		/// <summary>
		/// Removes the forward.
		/// </summary>
		/// <param name="address">The adb socket address.</param>
		/// <param name="device">The device.</param>
		/// <param name="localPort">The local port.</param>
		/// <returns></returns>
		public bool RemoveForward(IPEndPoint address, Device device, int localPort) {
			using(var socket = ExecuteRawSocketCommand(address, device, "host-serial:{0}:killforward:tcp:{1}".With(device.SerialNumber, localPort))) {
				// do nothing...
				return true;
			}
		}

		/// <summary>
		/// Removes all forwards.
		/// </summary>
		/// <param name="address">The address.</param>
		/// <param name="device">The device.</param>
		/// <returns></returns>
		public bool RemoveAllForward(IPEndPoint address, Device device) {
			using(var socket = ExecuteRawSocketCommand(address, device, "host-serial:{0}:killforward-all".With(device.SerialNumber))) {
				// do nothing...
				return true;
			}
		}

		/// <summary>
		/// Determines whether the specified reply is okay.
		/// </summary>
		/// <param name="reply">The reply.</param>
		/// <returns>
		///   <c>true</c> if the specified reply is okay; otherwise, <c>false</c>.
		/// </returns>
		public bool IsOkay(byte[] reply) {
			return reply.GetString().Equals("OKAY");
		}

		/// <summary>
		/// Replies to string.
		/// </summary>
		/// <param name="reply">The reply.</param>
		/// <returns></returns>
		public String ReplyToString(byte[] reply) {
			String result;
			try {
				result = Encoding.Default.GetString(reply);
			} catch(DecoderFallbackException uee) {
				Log.e(TAG, uee);
				result = "";
			}
			return result;
		}

		/// <summary>
		/// Gets the devices that are available for communication.
		/// </summary>
		/// <param name="address">The address.</param>
		/// <returns></returns>
		public List<Device> GetDevices(IPEndPoint address) {
			// -l will return additional data
			using(var socket = ExecuteRawSocketCommand(address, "host:devices-l")) {
				byte[] reply = new byte[4];

				if(!Read(socket, reply)) {
					Log.e(TAG, "error in getting data length");
					return null;
				}
				String lenHex = reply.GetString(Encoding.Default);
				int len = int.Parse(lenHex, System.Globalization.NumberStyles.HexNumber);

				reply = new byte[len];
				if(!Read(socket, reply)) {
					Log.e(TAG, "error in getting data");
					return null;
				}

				List<Device> s = new List<Device>();
				String[] data = reply.GetString(Encoding.Default).Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
				data.ForEach(item => {
					var device = Device.CreateFromAdbData(item);
					s.Add(device);
				});

				return s;
			}
		}

		/// <summary>
		/// Gets the frame buffer from the specified end point.
		/// </summary>
		/// <param name="adbSockAddr">The adb sock addr.</param>
		/// <param name="device">The device.</param>
		/// <returns></returns>
		public RawImage GetFrameBuffer(IPEndPoint adbSockAddr, IDevice device) {

			RawImage imageParams = new RawImage();
			byte[] request = FormAdbRequest("framebuffer:"); //$NON-NLS-1$
			byte[] nudge = {
						0
				};
			byte[] reply;

			Socket adbChan = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			try {
				adbChan.Connect(adbSockAddr);
				adbChan.Blocking = true;

				// if the device is not -1, then we first tell adb we're looking to talk
				// to a specific device
				SetDevice(adbChan, device);
				if(!Write(adbChan, request))
					throw new AdbException("failed asking for frame buffer");

				AdbResponse resp = ReadAdbResponse(adbChan, false /* readDiagString */);
				if(!resp.IOSuccess || !resp.Okay) {
					Log.w(TAG, "Got timeout or unhappy response from ADB fb req: " + resp.Message);
					adbChan.Close();
					return null;
				}

				// first the protocol version.
				reply = new byte[4];
				if(!Read(adbChan, reply)) {
					Log.w(TAG, "got partial reply from ADB fb:");

					adbChan.Close();
					return null;
				}
				BinaryReader buf;
				int version = 0;
				using(MemoryStream ms = new MemoryStream(reply)) {
					buf = new BinaryReader(ms);
					version = buf.ReadInt16();
				}

				// get the header size (this is a count of int)
				int headerSize = RawImage.GetHeaderSize(version);
				// read the header
				reply = new byte[headerSize * 4];
				if(!Read(adbChan, reply)) {
					Log.w(TAG, "got partial reply from ADB fb:");

					adbChan.Close();
					return null;
				}

				using(MemoryStream ms = new MemoryStream(reply)) {
					buf = new BinaryReader(ms);

					// fill the RawImage with the header
					if(imageParams.ReadHeader(version, buf) == false) {
						Log.w(TAG, "Unsupported protocol: " + version);
						return null;
					}
				}

				Log.d(TAG, "image params: bpp=" + imageParams.Bpp + ", size="
								+ imageParams.Size + ", width=" + imageParams.Width
								+ ", height=" + imageParams.Height);

				if(!Write(adbChan, nudge))
					throw new AdbException("failed nudging");

				reply = new byte[imageParams.Size];
				if(!Read(adbChan, reply)) {
					Log.w(TAG, "got truncated reply from ADB fb data");
					adbChan.Close();
					return null;
				}

				imageParams.Data = reply;
			} finally {
				if(adbChan != null) {
					adbChan.Close();
				}
			}

			return imageParams;
		}

		/// <summary>
		/// Executes a shell command on the remote device
		/// </summary>
		/// <param name="endPoint">The end point.</param>
		/// <param name="command">The command.</param>
		/// <param name="device">The device.</param>
		/// <param name="rcvr">The RCVR.</param>
		/// <remarks>Should check if you CanSU before calling this.</remarks>
		public void ExecuteRemoteRootCommand(IPEndPoint endPoint, String command, Device device, IShellOutputReceiver rcvr) {
			ExecuteRemoteRootCommand(endPoint, String.Format("su -c \"{0}\"", command), device, rcvr, int.MaxValue);
		}

		/// <summary>
		/// Executes a shell command on the remote device
		/// </summary>
		/// <param name="endPoint">The end point.</param>
		/// <param name="command">The command.</param>
		/// <param name="device">The device.</param>
		/// <param name="rcvr">The RCVR.</param>
		/// <param name="maxTimeToOutputResponse">The max time to output response.</param>
		public void ExecuteRemoteRootCommand(IPEndPoint endPoint, String command, Device device, IShellOutputReceiver rcvr, int maxTimeToOutputResponse) {
			ExecuteRemoteCommand(endPoint, String.Format("su -c \"{0}\"", command), device, rcvr);
		}

		/// <summary>
		/// Executes the remote command.
		/// </summary>
		/// <param name="endPoint">The end point.</param>
		/// <param name="command">The command.</param>
		/// <param name="device">The device.</param>
		/// <param name="rcvr">The RCVR.</param>
		/// <param name="maxTimeToOutputResponse">The max time to output response.</param>
		/// <exception cref="AdbException">failed submitting shell command</exception>
		/// <exception cref="System.OperationCanceledException"></exception>
		/// <exception cref="Managed.Adb.Exceptions.ShellCommandUnresponsiveException"></exception>
		/// <exception cref="System.IO.FileNotFoundException"></exception>
		/// <exception cref="UnknownOptionException"></exception>
		/// <exception cref="CommandAbortingException"></exception>
		/// <exception cref="PermissionDeniedException"></exception>
		public void ExecuteRemoteCommand(IPEndPoint endPoint, String command, Device device, IShellOutputReceiver rcvr, int maxTimeToOutputResponse) {

			using(var socket = ExecuteRawSocketCommand(endPoint, device, "shell:{0}".With(command))) {
				socket.ReceiveTimeout = maxTimeToOutputResponse;
				socket.SendTimeout = maxTimeToOutputResponse;

				try {
					byte[] data = new byte[16384];
					int count = -1;
					while(count != 0) {

						if(rcvr != null && rcvr.IsCancelled) {
							Log.w(TAG, "execute: cancelled");
							throw new OperationCanceledException();
						}

						count = socket.Receive(data);
						if(count == 0) {
							// we're at the end, we flush the output
							rcvr.Flush();
							Log.w(TAG, "execute '" + command + "' on '" + device + "' : EOF hit. Read: " + count);
						} else {
							string[] cmd = command.Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
							string sdata = data.GetString(0, count, AdbHelper.DEFAULT_ENCODING);

							var sdataTrimmed = sdata.Trim();
							if(sdataTrimmed.EndsWith(String.Format("{0}: not found", cmd[0]))) {
								Log.w(TAG, "The remote execution returned: '{0}: not found'", cmd[0]);
								throw new FileNotFoundException(string.Format("The remote execution returned: '{0}: not found'", cmd[0]));
							}

							if(sdataTrimmed.EndsWith("No such file or directory")) {
								Log.w(TAG, "The remote execution returned: {0}", sdataTrimmed);
								throw new FileNotFoundException(String.Format("The remote execution returned: {0}", sdataTrimmed));
							}

							// for "unknown options"
							if(sdataTrimmed.Contains("Unknown option")) {
								Log.w(TAG, "The remote execution returned: {0}", sdataTrimmed);
								throw new UnknownOptionException(sdataTrimmed);
							}

							// for "aborting" commands
							if(sdataTrimmed.IsMatch("Aborting.$")) {
								Log.w(TAG, "The remote execution returned: {0}", sdataTrimmed);
								throw new CommandAbortingException(sdataTrimmed);
							}

							// for busybox applets 
							// cmd: applet not found
							if(sdataTrimmed.IsMatch("applet not found$") && cmd.Length > 1) {
								Log.w(TAG, "The remote execution returned: '{0}'", sdataTrimmed);
								throw new FileNotFoundException(string.Format("The remote execution returned: '{0}'", sdataTrimmed));
							}

							// checks if the permission to execute the command was denied.
							// workitem: 16822
							if(sdataTrimmed.IsMatch("(permission|access) denied$")) {
								Log.w(TAG, "The remote execution returned: '{0}'", sdataTrimmed);
								throw new PermissionDeniedException(String.Format("The remote execution returned: '{0}'", sdataTrimmed));
							}

							// Add the data to the receiver
							if(rcvr != null) {
								rcvr.AddOutput(data, 0, count);
							}

						}
					}
				} catch(SocketException) {
					throw new ShellCommandUnresponsiveException();
				} finally {
					rcvr.Flush();
				}
			}

		}

		/// <summary>
		/// Executes a shell command on the remote device
		/// </summary>
		/// <param name="endPoint">The socket end point</param>
		/// <param name="command">The command to execute</param>
		/// <param name="device">The device to execute on</param>
		/// <param name="rcvr">The shell output receiver</param>
		/// <exception cref="FileNotFoundException">Throws if the result is 'command': not found</exception>
		/// <exception cref="IOException">Throws if there is a problem reading / writing to the socket</exception>
		/// <exception cref="OperationCanceledException">Throws if the execution was canceled</exception>
		/// <exception cref="EndOfStreamException">Throws if the Socket.Receice ever returns -1</exception>
		public void ExecuteRemoteCommand(IPEndPoint endPoint, String command, Device device, IShellOutputReceiver rcvr) {
			ExecuteRemoteCommand(endPoint, command, device, rcvr, int.MaxValue);
		}


		/// <summary>
		/// Sets the device.
		/// </summary>
		/// <param name="adbChan">The adb chan.</param>
		/// <param name="device">The device.</param>
		public void SetDevice(Socket adbChan, IDevice device) {
			// if the device is not null, then we first tell adb we're looking to talk
			// to a specific device
			if(device != null) {
				String msg = "host:transport:" + device.SerialNumber;
				byte[] device_query = FormAdbRequest(msg);

				if(!Write(adbChan, device_query)) {
					throw new AdbException("failed submitting device (" + device + ") request to ADB");
				}

				AdbResponse resp = ReadAdbResponse(adbChan, false /* readDiagString */);
				if(!resp.Okay) {
					if(String.Compare("device not found", resp.Message, true) == 0) {
						throw new DeviceNotFoundException(device.SerialNumber);
					} else {
						throw new AdbException("device (" + device + ") request rejected: " + resp.Message);
					}
				}
			}
		}

		/// <summary>
		/// Runs the Event log service on the Device, and provides its output to the LogReceiver.
		/// </summary>
		/// <param name="address">The address.</param>
		/// <param name="device">The device.</param>
		/// <param name="rcvr">The RCVR.</param>
		public void RunEventLogService(IPEndPoint address, Device device, LogReceiver rcvr) {
			RunLogService(address, device, "events", rcvr);
		}

		/// <summary>
		/// Runs the Event log service on the Device, and provides its output to the LogReceiver.
		/// </summary>
		/// <param name="address">The address.</param>
		/// <param name="device">The device.</param>
		/// <param name="logName">Name of the log.</param>
		/// <param name="rcvr">The RCVR.</param>
		/// <exception cref="AdbException">failed asking for log</exception>
		/// <exception cref="Managed.Adb.Exceptions.AdbCommandRejectedException"></exception>
		public void RunLogService(IPEndPoint address, Device device, String logName, LogReceiver rcvr) {
			using(var socket = ExecuteRawSocketCommand(address, device, "log:{0}".With(logName))) {
				byte[] data = new byte[16384];
				using(var ms = new MemoryStream(data)) {
					int offset = 0;

					while(true) {
						int count;
						if(rcvr != null && rcvr.IsCancelled) {
							break;
						}
						var buffer = new byte[4 * 1024];

						count = socket.Receive(buffer);
						if(count < 0) {
							break;
						} else if(count == 0) {
							try {
								Thread.Sleep(WAIT_TIME * 5);
							} catch(ThreadInterruptedException) {
							}
						} else {
							ms.Write(buffer, offset, count);
							offset += count;
							if(rcvr != null) {
								var d = ms.ToArray();
								rcvr.ParseNewData(d, 0, d.Length);
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// Reboots the specified adb socket address.
		/// </summary>
		/// <param name="adbSocketAddress">The adb socket address.</param>
		/// <param name="device">The device.</param>
		public void Reboot(IPEndPoint adbSocketAddress, Device device) {
			Reboot(string.Empty, adbSocketAddress, device);
		}

		/// <summary>
		/// Reboots the specified device in to the specified mode.
		/// </summary>
		/// <param name="into">The into.</param>
		/// <param name="adbSockAddr">The adb sock addr.</param>
		/// <param name="device">The device.</param>
		public void Reboot(String into, IPEndPoint adbSockAddr, Device device) {
			byte[] request;
			if(string.IsNullOrEmpty(into)) {
				request = FormAdbRequest("reboot:");
			} else {
				request = FormAdbRequest("reboot:" + into);
			}

			using(ExecuteRawSocketCommand(adbSockAddr, device, request)) {
				// nothing to do...
			}
		}

		/// <summary>
		/// Executes a raw socket command.
		/// </summary>
		/// <param name="adbSockAddr">The adb sock addr.</param>
		/// <param name="command">The command.</param>
		/// <returns></returns>
		/// <exception cref="AdbException">
		/// failed to submit the command: {0}.With(command)
		/// or
		/// Device rejected command: {0}.With(resp.Message)
		/// </exception>
		private Socket ExecuteRawSocketCommand(IPEndPoint address, Device device, string command) {
			return ExecuteRawSocketCommand(address, device, FormAdbRequest(command));
		}

		/// <summary>
		/// Executes the raw socket command.
		/// </summary>
		/// <param name="address">The address.</param>
		/// <param name="command">The command.</param>
		/// <returns></returns>
		private Socket ExecuteRawSocketCommand(IPEndPoint address, string command) {
			return ExecuteRawSocketCommand(address, FormAdbRequest(command));
		}

		/// <summary>
		/// Executes the raw socket command.
		/// </summary>
		/// <param name="address">The address.</param>
		/// <param name="command">The command.</param>
		/// <returns></returns>
		private Socket ExecuteRawSocketCommand(IPEndPoint address, byte[] command) {
			return ExecuteRawSocketCommand(address, null, command);
		}
		/// <summary>
		/// Executes the raw socket command.
		/// </summary>
		/// <param name="address">The address.</param>
		/// <param name="device">The device.</param>
		/// <param name="command">The command. Should call FormAdbRequest on the string to create the byte array.</param>
		/// <returns></returns>
		/// <exception cref="AdbException">
		/// Device is offline. 
		/// or
		/// failed to submit the command: {0}.With(command)
		/// or
		/// Device rejected command: {0}.With(resp.Message)
		/// </exception>
		private Socket ExecuteRawSocketCommand(IPEndPoint address, Device device, byte[] command) {
			if(device != null && !device.IsOnline) {
				throw new AdbException("Device is offline");
			}

			Socket adbChan = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			adbChan.Connect(address);
			adbChan.Blocking = true;
			if(device != null) {
				SetDevice(adbChan, device);
			}
			if(!Write(adbChan, command)) {
				throw new AdbException("failed to submit the command: {0}.".With(command.GetString().Trim()));
			}

			AdbResponse resp = ReadAdbResponse(adbChan, false /* readDiagString */);
			if(!resp.IOSuccess || !resp.Okay) {
				throw new AdbException("Device rejected command: {0}".With(resp.Message));
			}
			return adbChan;
		}

		private string HostPrefixFromDevice(IDevice device) {
			switch(device.TransportType) {
				case TransportType.Host:
					return "host-serial";
				case TransportType.Usb:
					return "host-usb";
				case TransportType.Local:
					return "host-local";
				case TransportType.Any:
				default:
					return "host";
			}
		}
	}
}

