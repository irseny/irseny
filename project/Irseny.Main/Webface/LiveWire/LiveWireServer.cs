using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Collections.Generic;
using Irseny.Core.Util;
using Irseny.Core.Log;

namespace Irseny.Main.Webface {
	public class LiveWireServer {
		const int ServerPort = 9234;
		const int ServerPortFallback = 9236;
		public static readonly int ServerOrigin = 0;

		TcpListener listener = null;
		Dictionary<int, WebSocketChannel> connections;
		LinkedList<Task<JsonString>> pendingAnswers;
		int nextClientOrigin = 1;
		LiveRequestHandler requestHandler;

		private int GenerateClientOrigin() {
			return nextClientOrigin++;
		}
		public LiveWireServer () {
			connections = new Dictionary<int, WebSocketChannel>(16);
			pendingAnswers = new LinkedList<Task<JsonString>>();
			requestHandler = new LiveRequestHandler();

		}
		public bool Started {
			get { return listener != null; }
		}
		public bool Start() {
			if (Started) return false;
				listener = new TcpListener(IPAddress.Parse("127.0.0.1"), ServerPort);
			try {
				listener.Start();
			} catch (SocketException) {
				listener = null;
				return false;
			}
			return true;
		}
		public bool Stop() {
			if (!Started) return false;
			foreach (var client in connections.Values) {
				client.Close();
			}
			listener.Stop();
			listener = null;
			return true;
		}
		public void Process() {
			if (!Started) return;
			// accept pending connections
			if (listener.Pending()) {
				TcpClient client = listener.AcceptTcpClient();
				var channel = new WebSocketChannel(new TcpChannel(client));
				int clientOrigin = GenerateClientOrigin();
				connections.Add(clientOrigin, channel);
				LogManager.Instance.LogMessage(this, string.Format("New LiveWire client {0}", clientOrigin));
			}
			// process connections
			var toRemove = new HashSet<int>();
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
					
					Task<JsonString> task = Task.Factory.StartNew(delegate { 
						JsonString request;
						try {
							request = JsonString.Parse(message.Text);
						} catch (FormatException) {
							LogManager.Instance.LogMessage(this, string.Format("LiveWire failed to parse JSON from client {0}: {1}",
								clientOrigin, message.Text));
							return null;
						}
						return requestHandler.Answer(request, clientOrigin ); 
					});
					pendingAnswers.AddLast(task);

				}

				if (channel.State == WebChannelState.InitFailed) {
					LogManager.Instance.LogError(this, string.Format("LiveWire client {0} failed to connect", 
						clientOrigin));
					toRemove.Add(clientOrigin);
					channel.Close(true);
				} else if (channel.State == WebChannelState.Closed) {
					LogManager.Instance.LogMessage(this, string.Format("LiveWire client {0} disconnected",
						clientOrigin));
					toRemove.Add(clientOrigin);
					channel.Close(true);
				}
			}
			// TODO implement pinging in order to detect suspended connections
			// remove dead connections
			foreach (int origin in toRemove) {
				connections.Remove(origin);
			}
			// send updates to all clients
			if (updates.First != null) {
				foreach (var pair in connections) {
					foreach (var update in updates) {
						pair.Value.SendMessage(update);
					}
				}
			}
			if (pendingAnswers.First != null) {
				var completedTasks = new HashSet<Task<JsonString>>();
				foreach (Task<JsonString> task in pendingAnswers) {
					bool delivered = false;
					do {
						if (!task.IsCompleted) {
							delivered = true;
							break;
						}
						completedTasks.Add(task);
						// TODO check for exception or cancellation
						JsonString answer = task.Result;
						if (answer == null) {
							// may happen on internal processing errors
							// when not enough information is extractable
							break;
						}
						string text = answer.ToString();
						// send to all targets
						string target = answer.GetTerminal("target", string.Empty);
						if (target.Equals(string.Empty)) {
							break;
						}
						if (target.Equals("'all'")) {
							// special send to all target
							foreach (var pair in connections) {
								pair.Value.SendMessage(new WebSocketMessage(text));
							}
							delivered = true;
							break;
						} 
						// read singular target from field
						int targetId = TextParseTools.ParseInt(target, -1);
						if (targetId < 0) {
							break;
						}
						WebSocketChannel connection;
						if (!connections.TryGetValue(targetId, out connection)) {
							break;
						}
						connection.SendMessage(new WebSocketMessage(text));
						delivered = true;
						break;
					} while (false);
					if (!delivered) {
						LogManager.Instance.LogError(this, "Cannot deliver live answer " + task.Result);
					}
				}
				foreach (var task in completedTasks) {
					pendingAnswers.Remove(task);
				}
			}


		}

	}
}

