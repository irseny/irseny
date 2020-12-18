using System;
using System.Net;
using System.Text;
using System.Collections.Generic;
using Irseny.Core.Util;

namespace Irseny.Main.Webface {
	public class WebSocketChannel : IWebChannel<WebSocketMessage> {
		// data source
		TcpChannel messageSource;
		// protocol starts with upgrade from http connection
		HttpChannel initSource;
		// messages received
		LinkedList<WebSocketMessage> availableMessages;
		// messages to send after init
		LinkedList<WebSocketMessage> toSend;
		// storage for messages that consist of multiple fragments
		LinkedList<byte[]> binaryFrame;
		StringBuilder textFrame;
		byte[] fragmentBuffer;
		int fragmentLength;



		public WebChannelState State { get; private set; }


		public int AvailableMessageNo {
			get { return availableMessages.Count; }
		}
		private WebSocketChannel() {
			State = WebChannelState.Closed;
			availableMessages = new LinkedList<WebSocketMessage>();
			binaryFrame = new LinkedList<byte[]>();
			textFrame = new StringBuilder();
			toSend = new LinkedList<WebSocketMessage>();
			fragmentBuffer = new byte[256];
			fragmentLength = 0;
		}
		public WebSocketChannel(TcpChannel source) : this() {
			if (source == null) throw new ArgumentNullException("source");
			this.messageSource = source;
			this.initSource = new HttpChannel(source);
			this.State = WebChannelState.Initializing;
		}

		public WebSocketMessage EmitMessage() {
			if (availableMessages.Count > 0) {
				WebSocketMessage result = availableMessages.First.Value;
				availableMessages.RemoveFirst();
				return result;
			} else {
				return new WebSocketMessage(string.Empty);
			}
		}
		public WebSocketMessage SnoopMessage(int index = 0) {
			if (index >= availableMessages.Count || index < 0) {
				return new WebSocketMessage(string.Empty);
			}
			var node = availableMessages.First;
			for (int i = index; i > 0; i--) {
				node = node.Next;
			}
			return node.Value;

		}
		public bool SendMessage(WebSocketMessage message) {
			switch (State) {
			case WebChannelState.Closed:
			case WebChannelState.InitFailed:
				return false;
			case WebChannelState.Initializing:
				toSend.AddLast(message);
				return true;
			case WebChannelState.Open:
				// TODO encode and send
				byte[] encoded = EncodeMessage(message);
				return messageSource.SendMessage(encoded);
			default:
				return false;
			}
		}

		public void Close(bool closeInternal=true) {
			// TODO send close message
			byte[] message = new byte[2]; // fin bit and opcode 0x8
			message[0] = 0x88; // fin bit and opcode 0x8
			message[1] = 0x0; // no mask and 0 payload
			messageSource.SendMessage(message);
			messageSource.Flush();
			// close connections
			if (initSource != null) {
				initSource.Close(false);
				initSource = null;
			}
			if (closeInternal) {
				messageSource.Close();
			}
			// clear frame and fragment
			fragmentLength = 0;
			textFrame.Clear();
			binaryFrame.Clear();
			State = WebChannelState.Closed;
		}
		public void Process() {
			if (State == WebChannelState.Closed || State == WebChannelState.InitFailed) {
				return;
			}
			if (State == WebChannelState.Initializing) {
				ProcessInit();
			} else if (State == WebChannelState.Open) {
				ProcessOpen();
			}
		}
		public void Flush() {
			messageSource.Flush();
		}
		private byte[] EncodeMessage(WebSocketMessage message) {
			byte[] payload;
			if (message.IsText) {
				payload = Encoding.UTF8.GetBytes(message.Text);
			} else if (message.IsBinary) {
				payload = message.Binary;
			} else {
				throw new InvalidOperationException();
			}
			int payloadLengthExt = 0;
			if (payload.Length <= 125) {
				payloadLengthExt = 0;
			} else {
				if (payload.Length <= short.MaxValue) {
					payloadLengthExt = 2;
				} else {
					payloadLengthExt = 8;
				}
			}
			const int headLength = 2;
			int resultLength = headLength + payloadLengthExt + payload.Length;
			byte[] result = new byte[resultLength];
			// fin 1, rsv 0
			result[0] = 0x80;
			// opcode 1 or 2
			if (message.IsText) {
				result[0] |= 0x1;
			} 
			if (message.IsBinary) {
				result[0] |= 0x2;
			}
			// masked 0, payload length, payload length ext
			if (payloadLengthExt == 0) {
				result[1] = (byte)payload.Length;
			} else {
				byte[] lengthExt;
				if (payloadLengthExt == sizeof(ushort)) {
					result[1] = 126;
					lengthExt = BitConverter.GetBytes((ushort)payload.Length);
				} else if (payloadLengthExt == sizeof(ulong)) {
					result[1] = 127;
					lengthExt = BitConverter.GetBytes((ulong)payload.LongLength);
				} else {
					throw new InvalidOperationException();
				}
				if (BitConverter.IsLittleEndian) {
					Array.Reverse(lengthExt);
				}
				for (int i = 0; i < payloadLengthExt; i++) {
					result[headLength + i] = lengthExt[i];
				}
			}
			// payload
			Array.Copy(payload, 0, result, headLength + payloadLengthExt, payload.Length);
			return result;
		}
		private void ProcessInit() {
			// continue initializing through the html channel
			initSource.Process();
			// stop processing if something went wrong
			if (initSource.State == WebChannelState.Closed || initSource.State == WebChannelState.InitFailed) {
				State = WebChannelState.InitFailed;
				initSource = null;
			}
			// wait for the first header to arrive
			if (initSource.AvailableMessageNo > 0) {
				HttpHeader header = initSource.EmitMessage().Header;
				string headerKey = string.Empty;
				bool headerValid = false;
				bool canRecover = false;
				bool denySubprotocol = false;
				bool denyExtension = false;
				do {
					// check the header for errors
					if (header.Method != HttpMethod.Get) {
						break;
					}
					// TODO optional origin
					string connection;
					if (!header.Fields.TryGetValue("Connection", out connection)) {
						break;
					}
					if (!connection.ToLower().Contains("upgrade")) {
						break;
					}
					string upgrade;
					if (!header.Fields.TryGetValue("Upgrade", out upgrade)) {
						break;
					}
					if (!upgrade.ToLower().Contains("websocket")) {
						break;
					}
					if (!header.Fields.TryGetValue("Sec-WebSocket-Key", out headerKey)) {
						break;
					}
					string sVersion;
					if (header.Fields.TryGetValue("Sec-WebSocket-Version", out sVersion)) {
						// TODO improve detection
						bool acceptVersion = false;
						if (sVersion.Contains("13")) {
							acceptVersion = true;

						}
						if (!acceptVersion) {
							headerValid = false;
							canRecover = true;
							HttpHeader response = new HttpHeader(header.Version, HttpStatusCode.UpgradeRequired);
							response.Fields.Add("Sec-WebSocket-Version", "13");
							initSource.SendMessage(new HttpMessage(response));
							break;
						}
					}
					// this implementation does not support subprotocols or extensions
					// send null fields if these exist in the request
					if (header.Fields.ContainsKey("Subprotocol")) {
						denySubprotocol = true;
					}
					if (header.Fields.ContainsKey("Extension")) {
						denyExtension = true;
					}
					// TODO optional handle resource or send 404 not found
					headerValid = true;
				} while (false);
				if (!headerValid) {
					if (!canRecover) {
						// set the connection on ice if the setup failed
						State = WebChannelState.InitFailed;
						initSource.Close(false);
						initSource = null;
					}
				} else {
					// send an appropriate response header
					HttpHeader response = new HttpHeader(HttpVersion.Version11, HttpStatusCode.SwitchingProtocols);
					response.Fields.Add("Upgrade", "websocket");
					response.Fields.Add("Connection", "Upgrade");
					byte[] byteKey = Encoding.UTF8.GetBytes(headerKey + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11");
					byte[] sha1Key = System.Security.Cryptography.SHA1.Create().ComputeHash(byteKey);
					string base64Key = Convert.ToBase64String(sha1Key);
					response.Fields.Add("Sec-WebSocket-Accept", base64Key);

					if (denySubprotocol) {
						response.Fields.Add("Subprotocol", "null");
					}
					if (denyExtension) {
						response.Fields.Add("Extension", "null");
					}
					initSource.SendMessage(new HttpMessage(response));
					// since the upgrade is done no more http communication is necessary
					// ACK has to be sent first so initSource can not contain any websocket messages at this point
					initSource.Flush();
					initSource.Close(false);
					initSource = null;
					// the handshake has been completed
					// messages can now be sent
					State = WebChannelState.Open;
					// continue with sending all pending messages
					foreach (WebSocketMessage send in toSend) {
						byte[] encoded = EncodeMessage(send);
						messageSource.SendMessage(encoded);
					}
					toSend.Clear();				
				} 
			}
		}

		private void ProcessOpen() {
			messageSource.Process();
			while (messageSource.AvailableMessageNo > 0) {
				byte[] message = messageSource.EmitMessage();
				if (message != null && message.Length > 0) {
					DecodeMessage(message);
				}
			}
			if (messageSource.State == WebChannelState.Closed || messageSource.State == WebChannelState.InitFailed) {
				State = WebChannelState.Closed;
			}
		}
		private void DecodeMessage(byte[] message) {
			int messageLength = message.Length;
			bool appended = false;
			if (fragmentLength > 0) {
				// we also need to consider what was sent before
				// so first append to the fragment buffer
				AppendToFragment(message);
				appended = true;
				// and process the whole fragment
				message = fragmentBuffer;
				messageLength = fragmentLength;

			}
			const int basicHeadLength = 2;
			// we need at least two bytes for useful readings
			if (messageLength < basicHeadLength) {
				if (!appended) {
					AppendToFragment(message);
				}
				return;
			}
			// look at the first byte to gather information
			bool fin = (message[0] & 0x80) != 0x0;
			byte rsv = (byte)(message[0] & 0x70);
			byte opcode = (byte)(message[0] & 0x0F);
			// look at the second byte for more information
			bool masked = (message[1] & 0x80) != 0x0;
			byte payloadLength1 = (byte)(message[1] & 0x7F);
			// TODO handle unexpected results
			if (rsv != 0) {
				HandleInvalidMessage();
				return;
			}
			if (!masked) {
				HandleInvalidMessage();
				return;
			}
			if (opcode == 8) {
				// close
				Close(true);
			} else if (opcode == 9) {
				// ping
				DecodePingMessage(message);
			} else if (opcode == 10) {
				// pong
				DecodePongMessage(message);
			} else if (opcode > 2) {
				// not valid
				HandleInvalidMessage();
			} else {
				if (!masked) {
					HandleInvalidMessage();
				} else {
					DecodeMessage(message, messageLength, appended, fin, opcode, payloadLength1);
				}
			}
		}
		private void DecodeMessage(byte[] message, int messageLength, bool appended, bool fin, byte opcode, byte payloadLength1) {
			// determine message length
			int payloadLengthExt = 0;
			if (payloadLength1 <= 125) {
				payloadLengthExt = 0;
			} else if (payloadLength1 == 126) {
				payloadLengthExt = 2;
			} else if (payloadLength1 == 127) {
				payloadLengthExt = 8;
			} else {
				throw new InvalidOperationException();
			}
			const int basicHeadLength = 2;
			const int maskLength = 4;
			int maskStart = basicHeadLength + payloadLengthExt;
			int payloadStart = basicHeadLength + payloadLengthExt + maskLength;
			// check if the payload length and mask can be read
			if (messageLength < basicHeadLength + payloadLengthExt + maskLength) {
				if (!appended) {
					AppendToFragment(message);
				}
				return;
			}
			// read the extended payload length
			ulong payloadLength = payloadLength1;
			if (payloadLengthExt > 0) {
				byte[] ext = new byte[payloadLengthExt];
				for (int i = 0; i < payloadLengthExt; i++) {
					ext[i] = message[basicHeadLength + i];
				}
				// payload length is always written in big-endian (MSB0) style
				// reverse the byte order in case of little-endian bit conversion
				if (BitConverter.IsLittleEndian) {
					Array.Reverse(ext);
				}
				if (payloadLengthExt == sizeof(ushort)) {
					payloadLength = BitConverter.ToUInt16(ext, 0);
				} else if (payloadLengthExt == sizeof(ulong)) {
					payloadLength = BitConverter.ToUInt64(ext, 0);
				} else {
					throw new InvalidOperationException();
				}
			}
			// terminate on massive messages
			if (payloadLength > 0x0000FFFF) {
				HandleInvalidMessage();
				return;
			}
			// check if the message can be read
			if (messageLength < payloadStart + (int)payloadLength) {
				if (!appended) {
					AppendToFragment(message);
				}
				return;
			}
			// extract the mask and apply it to the message
			byte[] mask = new byte[] {
				message[maskStart + 0],
				message[maskStart + 1],
				message[maskStart + 2],
				message[maskStart + 3]
			};
			byte[] content = new byte[payloadLength];
			for (int i = 0; (ulong)i < payloadLength; i++) {
				content[i] = (byte)(message[payloadStart + i]^mask[i&0x3]);
			}
			int nextFragmentStart = payloadStart + (int)payloadLength;
			// create a frame bit from the fragment or message
			// the first message in a frame indicates text or binary data
			// all following messages have opcode 0
			// the last fragment has the fin bit set (which could also be the first fragment)
			string textFragment = null;
			byte[] binaryFragment = null;
			if (opcode == 1 || (opcode == 0 && textFrame.Length > 0)) {
				textFragment = Encoding.UTF8.GetString(content);
			} else if (opcode == 2 || (opcode == 0 && binaryFrame.First != null)) {
				binaryFragment = content;
			} else {
				// unknown data type
				throw new InvalidOperationException();
			}
			// the next fragment might already be part of the current message
			// extract remaining bytes in order to build the next message
			byte[] nextMessage = null;
			if (nextFragmentStart < message.Length) {
				int nextMessageLength = message.Length - nextFragmentStart;
				nextMessage = new byte[nextMessageLength];
				Array.Copy(message, nextFragmentStart, nextMessage, 0, nextMessageLength);
			}
			if (fin) {
				EndFrame(textFragment, binaryFragment);
			} else {
				EndFragment(textFragment, binaryFragment);
			}
			// the rest of the message belongs to another frame
			// the client might not send another message any time soon
			// so its better to process everything right away
			if (nextMessage != null) {
				DecodeMessage(nextMessage);
			}
		}
		private void DecodePingMessage(byte[] message) {
			// TODO implement
		}
		private void DecodePongMessage(byte[] message) {
			// TODO implement
		}
		private void HandleInvalidMessage() {
			Close();
			// TODO do not throw
			throw new InvalidOperationException();
		}
		private void EndFrame(string textFragment, byte[] binaryFragment) {
			if (textFragment != null && binaryFragment != null) throw new InvalidOperationException();
			if (textFragment != null) {
				textFrame.Append(textFragment);
				availableMessages.AddLast(new WebSocketMessage(textFrame.ToString()));
				textFrame.Clear();
			} else if (binaryFragment != null) {
				int capacity = binaryFragment.Length;
				foreach (byte[] frag in binaryFrame) {
					capacity += frag.Length;
				}
				byte[] binary = new byte[capacity];
				int binaryEnd = 0;
				foreach (byte[] frag in binaryFrame) {
					Array.Copy(frag, 0, binary, binaryEnd, frag.Length);
					binaryEnd += frag.Length;
				}
				Array.Copy(binaryFragment, 0, binary, binaryEnd, binaryFragment.Length);
				availableMessages.AddLast(new WebSocketMessage(binary));
				binaryFrame.Clear();
			} else {
				throw new InvalidOperationException();
			}
			fragmentLength = 0;
		}
		private void EndFragment(string textFragment, byte[] binaryFragment) {
			if (textFragment != null && binaryFragment != null) throw new InvalidOperationException();
			// the frame is not finished
			// so add the fragment to the frame
			if (textFragment != null) {
				textFrame.Append(textFragment);
			} else if (binaryFragment != null) {
				binaryFrame.AddLast(binaryFragment);
			} else {
				throw new InvalidOperationException();
			}
			fragmentLength = 0;
		}


		private int AppendToFragment(byte[] toAppend) {
			ProvideFragmentCapacity(fragmentLength + toAppend.Length);
			Array.Copy(toAppend, 0, fragmentBuffer, fragmentLength, toAppend.Length);
			fragmentLength += toAppend.Length;
			return fragmentLength;
		}
		private void ProvideFragmentCapacity(int capacity) {
			if (capacity < 0) throw new InvalidOperationException();
			if (fragmentBuffer.Length < capacity) {
				int nextCapacity = (capacity*3)/2;
				byte[] nextBuffer = new byte[nextCapacity];
				Array.Copy(fragmentBuffer, 0, nextBuffer, 0, fragmentLength);
				fragmentBuffer = nextBuffer;
			}
		}
	}
}
