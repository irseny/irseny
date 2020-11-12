using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using Irseny.Core.Util;
using Irseny.Core.Log;

namespace Irseny.Main.Webface {
	public class LiveWireServer {
		const int ServerPort = 9234;
		const int ServerOrigin = 0;

		TcpListener listener = null;
		Dictionary<int, WebSocketChannel> connections;
		int nextClientOrigin = 1;

		private int GenerateClientOrigin() {
			return nextClientOrigin++;
		}
		public LiveWireServer () {
			connections = new Dictionary<int, WebSocketChannel>(16);
		}
		public bool Started {
			get { return listener != null; }
		}
		public void Start() {
			if (Started) return;
			listener = new TcpListener(IPAddress.Parse("127.0.0.1"), ServerPort);
			listener.Start();

		}
		public void Stop() {
			if (!Started) return;
			foreach (var client in connections.Values) {
				client.Close();
			}
			listener.Stop();
			listener = null;
		}
		public void Process() {
			if (!Started) return;
			// accept pending connections
			if (listener.Pending()) {
				TcpClient client = listener.AcceptTcpClient();
				var channel = new WebSocketChannel(new TcpChannel(client));
				int clientOrigin = GenerateClientOrigin();
				connections.Add(clientOrigin, channel);
				JsonString configMessage = GenerateConfigMessage(clientOrigin);
				channel.SendMessage(new WebSocketMessage(configMessage.ToString()));
				LogManager.Instance.LogMessage(this, string.Format("New LiveWire client {0}", clientOrigin));
			}
			// process connections
			var toRemove = new LinkedList<int>();
			foreach (var pair in connections) {
				WebSocketChannel channel = pair.Value;
				int clientOrigin = pair.Key;
				channel.Process();
				while (channel.AvailableMessageNo > 0) {
					var message = channel.EmitMessage();
					LogManager.Instance.LogMessage(this, string.Format("LiveWire received from client {0}: {1}", 
						clientOrigin, message.Text));
					// TODO handle message
					JsonString str = JsonString.Parse(message.Text);
				}
				if (channel.State == WebChannelState.InitFailed) {
					LogManager.Instance.LogMessage(this, string.Format("LiveWire client {0} failed to connect", 
						clientOrigin));
					toRemove.AddLast(clientOrigin);
					channel.Close(true);
				} else if (channel.State == WebChannelState.Closed) {
					LogManager.Instance.LogMessage(this, string.Format("LiveWire client {0} disconnected",
						clientOrigin));
					toRemove.AddLast(clientOrigin);
					channel.Close(true);
				}
			}
			// remove dead connections
			foreach (int origin in toRemove) {
				connections.Remove(origin);
			}
		}
		private JsonString GenerateConfigMessage(int clientOrigin) {
			var result = JsonString.CreateDict();
			result.Add("config", "type");
			result.Add(JsonString.CreateDict(), "subject");
			result.Add(ServerOrigin, "subject", "serverOrigin");
			result.Add(clientOrigin, "subject", "clientOrigin");
			return result;
		}
	}
}

