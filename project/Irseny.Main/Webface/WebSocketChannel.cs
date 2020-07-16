using System;
using System.Text;
using System.Collections.Generic;
using Irseny.Core.Util;

namespace Irseny.Main.Webface {
	public class WebSocketChannel : IWebChannel<byte[]> {
		TcpChannel source;
		// messages received
		LinkedList<byte[]> availableMessages;
		// messages to send after init
		List<byte[]> toSend;
		// temporary message storage
		byte[] receiveBuffer;
		int receiveBufferLength;
		// received messages in init
		int snoopedMessageNo = 0;
		// header in receiveBuffer, < 0 if not found
		int headerLength = -1;
		HttpHeader messageHeader = null;

		public WebChannelState State { get; private set; }

		private WebSocketChannel() {
			State = WebChannelState.Initializing;
			availableMessages = new LinkedList<byte[]>();
			toSend = new List<byte[]>();
			receiveBuffer = new byte[512];
		}
		public int AvailableMessageNo {
			get { return availableMessages.Count; }
		}

		public WebSocketChannel(TcpChannel source) : this() {
			if (source == null) throw new ArgumentNullException("source");
			this.source = source;
		}

		public byte[] EmitMessage() {
			if (availableMessages.Count > 0) {
				byte[] result = availableMessages.First.Value;
				availableMessages.RemoveFirst();
				return result;
			} else {
				return new byte[0];
			}
		}
		public byte[] SnoopMessage(int index = 0) {
			if (index >= availableMessages.Count || index < 0) {
				return new byte[0];
			}
			var node = availableMessages.First;
			for (int i = index; i > 0; i--) {
				node = node.Next;
			}
			return node.Value;

		}
		public void SendMessage(byte[] message) {
			switch (State) {
			case WebChannelState.Closed:
			case WebChannelState.SetupFailed:
				return;
			case WebChannelState.Initializing:
				toSend.Add(message);
				break;
			case WebChannelState.Open:
				source.SendMessage(message);
				break;
			default:
				break;
			}
		}

		public void Close() {
			source.Close();
			State = WebChannelState.Closed;
		}
		public void Process() {
			if (State == WebChannelState.Closed || State == WebChannelState.SetupFailed) {
				return;
			}
			source.Process();
			if (State == WebChannelState.Initializing) {
				bool messageAdded = false;
				while (snoopedMessageNo < source.AvailableMessageNo) {
					byte[] bit = source.SnoopMessage(snoopedMessageNo);
					snoopedMessageNo += 1;
					messageAdded = true;
					ProvideReceiveCapacity(receiveBufferLength + bit.Length);
					Array.Copy(bit, 0, receiveBuffer, receiveBufferLength, bit.Length);
					receiveBufferLength += bit.Length;
				}
				if (messageAdded) {
					if (messageHeader == null) { 
						messageHeader = ExtractHeader(out headerLength);
					}
				}
			} else {
				// open state, normal interaction
			}
		}
		public void Flush() {
			source.Flush();
		}
		private void ProvideReceiveCapacity(int capacity) {
			if (capacity < 0) throw new InvalidOperationException();
			if (receiveBuffer.Length < capacity) {
				int nextCapacity = (capacity*3)/2;
				byte[] nextBuffer = new byte[nextCapacity];
				Array.Copy(receiveBuffer, 0, nextBuffer, 0, receiveBufferLength);
				receiveBuffer = nextBuffer;
			}
		}
		private HttpHeader ExtractHeader(out int headerLength) {
			byte[] pattern = new byte[] { (byte)'\r', (byte)'\n', (byte)'\r', (byte)'\n' };
			int patternStart = receiveBuffer.IndexOfPattern(pattern, 0, receiveBufferLength);
			if (patternStart < 0) {
				// not found
				headerLength = -1;
				return null;
			}
			headerLength = patternStart + pattern.Length;

			string sHeader = Encoding.UTF8.GetString(receiveBuffer, 0, headerLength);
			return HttpHeader.Parse(sHeader);
		}
	}
}
