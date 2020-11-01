﻿using System;
using System.Text;

using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

namespace Irseny.Main.Webface {
	public class TcpChannel : IWebChannel<byte[]> {
		NetworkStream stream;
		TcpClient client;
		LinkedList<byte[]> availableMessages;
		byte[] sendBuffer;

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
		public byte[] SnoopMessage(int depth=0) {
			if (depth >= availableMessages.Count || availableMessages.Count == 0) {
				return new byte[0];
			}
			var node = availableMessages.First;
			for (int i = depth; i > 0; i--) {
				node = node.Next;
			}
			return node.Value;
		}
		public void SendMessage(byte[] buffer, int startAt, int length) {
			if (buffer == null) throw new ArgumentNullException("buffer");
			if (!stream.CanWrite) throw new NotSupportedException();
			stream.Write(buffer, startAt, length);
		}
		public void SendMessage(byte[] buffer) {
			if (buffer == null) throw new ArgumentNullException("buffer");
			SendMessage(buffer, 0, buffer.Length);
		}
		public void Process() {
			if (!client.Connected && State != WebChannelState.Closed) {
				Close();
			}
			if (State == WebChannelState.Closed || State == WebChannelState.InitFailed) {
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
			stream.Flush();

		}
		public void Close(bool closeElementary=true) {
			if (closeElementary) {
				//stream.Flush();
				client.Close();
			}
			State = WebChannelState.Closed;
		}
	}
}
