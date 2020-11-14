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
		LiveRequestHandler requestHandler;
		LiveUpdateHandler updateHandler;


		private int GenerateClientOrigin() {
			return nextClientOrigin++;
		}
		public LiveWireServer () {
			connections = new Dictionary<int, WebSocketChannel>(16);
			requestHandler = new LiveRequestHandler();
			updateHandler = new LiveUpdateHandler();
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
			var updates = new LinkedList<WebSocketMessage>();
			
			foreach (var pair in connections) {
				WebSocketChannel channel = pair.Value;
				int clientOrigin = pair.Key;
				channel.Process();
				while (channel.AvailableMessageNo > 0) {
					var message = channel.EmitMessage();
					LogManager.Instance.LogMessage(this, string.Format("LiveWire received from client {0}: {1}", 
						clientOrigin, message.Text));
					// outsourced message handling
					JsonString str = JsonString.Parse(message.Text);
					JsonString answer;
					bool handled = false;
					if (requestHandler.TryAnswer(str, out answer)) {
						channel.SendMessage(new WebSocketMessage(answer.ToString()));
						handled = true;
					} 
					JsonString update;
					if (updateHandler.TryApplyUpdate(str, out update)) {
						if (update != null) {
							updates.AddLast(new WebSocketMessage(update.ToString()));
						}
						handled = true;
					}
					if (!handled) {
						LogManager.Instance.LogError(this, string.Format("Failed to handle live message\n{0}\n",
							str.ToJsonString()));
					}

				}

				if (channel.State == WebChannelState.InitFailed) {
					LogManager.Instance.LogError(this, string.Format("LiveWire client {0} failed to connect", 
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
			// send updates to all clients
			if (updates.First != null) {
				foreach (var pair in connections) {
					foreach (var update in updates) {
						pair.Value.SendMessage(update);
					}
				}
			}
			// TODO implement pinging in order to detect suspended connections
			// remove dead connections
			foreach (int origin in toRemove) {
				connections.Remove(origin);
			}
		}
		private JsonString GenerateConfigMessage(int clientOrigin) {
			var result = JsonString.CreateDict();
			{
				result.AddTerminal("type", @"""config""");
				var config = JsonString.CreateDict();
				{
					config.AddTerminal("serverOrigin", StringifyTools.StringifyInt(ServerOrigin));
					config.AddTerminal("clientOrigin", StringifyTools.StringifyInt(clientOrigin));
				}
				result.AddJsonString("subject", config);
			}
			return result;
		}
	}
}

