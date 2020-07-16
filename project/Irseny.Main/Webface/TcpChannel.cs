using System;
using System.Text;

using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

namespace Irseny.Main.Webface {
	public class TcpChannel : IWebChannel<byte[]> {
		NetworkStream stream;
		TcpClient client;
		//Queue<byte[]> availableMessages;
		LinkedList<byte[]> availableMessages;
		byte[] sendBuffer;
		//int receiveStart = 0;
		//int receiveEnd = -1;
		//StringBuilder messageBits = null;

		public int AvailableMessageNo {
			get {  return availableMessages.Count; }
		}

		public WebChannelState State { get; protected set; }

		protected TcpChannel() {
			State = WebChannelState.Open;
			availableMessages = new LinkedList<byte[]>();
			sendBuffer = new byte[0];
		}
		public TcpChannel(TcpClient client) : this() {
			this.client = client;
			this.stream = client.GetStream();

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
		public byte[] SnoopMessage(int index=0) {
			if (index >= availableMessages.Count || availableMessages.Count == 0) {
				return new byte[0];
			}
			var node = availableMessages.First;
			for (int i = index; i > 0; i--) {
				node = node.Next;
			}
			return node.Value;
		}
		//public void SendMessage(string message) {
		//	if (message == null) throw new ArgumentNullException("message");
		//	if (!stream.CanWrite) throw new NotSupportedException();
		//	if (message.Length > sendBuffer.Length) {
		//		sendBuffer = new byte[message.Length];
		//	}
		//	int writteSize = Encoding.UTF8.GetBytes(message, 0, message.Length, sendBuffer, 0);
		//	// TODO account for non ascii characters
		//	stream.Write(sendBuffer, 0, sendBuffer.Length);
		//}
		public void SendMessage(byte[] buffer, int startAt, int length) {
			if (buffer == null) throw new ArgumentNullException("buffer");
			if (!stream.CanWrite) throw new NotSupportedException();
			stream.Write(buffer, startAt, length);
			//stream.Flush();
		}
		public void SendMessage(byte[] buffer) {
			if (buffer == null) throw new ArgumentNullException("buffer");
			SendMessage(buffer, 0, buffer.Length);
		}
		public void Process() {
			if (!client.Connected && State != WebChannelState.Closed) {
				Close();
			}
			if (State == WebChannelState.Closed || State == WebChannelState.SetupFailed) {
				return;
			}
			if (!stream.CanRead) {
				return;
			}
			int bytesToRead = client.Available;
			if (bytesToRead == 0) {
				return;
			}
			byte[] buffer = new byte[bytesToRead];
			int bytesRead = stream.Read(buffer, 0, bytesToRead);
			if (bytesRead < bytesToRead) {
				byte[] result = new byte[bytesRead];
				Array.Copy(buffer, result, bytesRead);
				availableMessages.AddLast(result);
			} else {
				availableMessages.AddLast(buffer);
			}
		}
		public void Flush() {
			if (State == WebChannelState.Closed) {
				return;
			}
			//stream.Write(new byte[] { (byte)'\r', (byte)'\n'}, 0, 2);
			//client.NoDelay = true;
			stream.Flush();

		}
		public void Close() {
			if (State == WebChannelState.Closed) {
				return;
			}			
			stream.Flush();
			client.Close();
			State = WebChannelState.Closed;
		}
		//public void Process() {
		//	if (!stream.CanRead) {
		//		return;
		//	}
		//	if (client.Available == 0) {
		//		return;
		//	}
		//	while (client.Available > 0) {
		//		// read to end of data or end of buffer
		//		// limit of buffer
		//		int byteReadLimit = receiveBuffer.Length - receiveEnd - 1;
		//		// read from stream
		//		int bytesToRead = System.Math.Min(client.Available, byteReadLimit);
		//		int bytesRead = stream.Read(receiveBuffer, receiveEnd + 1, bytesToRead);
		//		// extend valid buffer range
		//		int localReceiveStart = receiveEnd + 1;
		//		receiveEnd += bytesRead;
		//		// go through the buffer and check for message end
		//		for (int i = localReceiveStart; i <= receiveEnd; i++) {
		//			if (receiveBuffer[i] == '\n') {
		//				// found end of message
		//				// read the whole message from start to newline
		//				string bit  = Encoding.UTF8.GetString(receiveBuffer, receiveStart, i - receiveStart);
		//				if (messageBits != null) {
		//					messageBits.Append(bit);
		//					availableMessages.Enqueue(messageBits.ToString());
		//					messageBits = null;
		//				} else {
		//					availableMessages.Enqueue(bit);
		//				}
		//				// shrink valid buffer range
		//				receiveStart = i + 1;
		//			}
		//		}
		//		// read a message bit if the buffer is full
		//		if (receiveEnd >=  receiveBuffer.Length - 1) {
		//			if (receiveStart <= receiveEnd) {
		//				string bit = Encoding.UTF8.GetString(receiveBuffer, receiveStart, receiveEnd - receiveStart + 1);
		//				if (messageBits == null) {
		//					messageBits = new StringBuilder();
		//				}
		//				messageBits.Append(bit);
		//			}
		//			// reset valid buffer range
		//			receiveStart = 0;
		//			receiveEnd = -1;
		//		}

		//	}
		//}
	}
}
