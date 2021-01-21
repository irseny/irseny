﻿// This file is part of Irseny.
//
// Copyright (C) 2021  Thilo Gabel
//
// Irseny is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Irseny is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Collections.Generic;
using Irseny.Core.Util;
using Irseny.Core.Log;
using System.Threading;

namespace Irseny.Main.Webface.LiveWire {
	public class LiveWireServer {
		readonly int ServerPort = 9243;
		public static readonly int ServerOrigin = 0;

		readonly AutoResetEvent processSignal;
		readonly object connectionSync;
		readonly object listenerSync;
		TcpListener listener;
		Dictionary<int, WebSocketChannel> connections;
		Dictionary<int, List<LiveCaptureSubscription>> subscriptions;
		LinkedList<JsonString> updates;
		LinkedList<Task<JsonString>> pendingRequests;
		int nextClientOrigin;


		private int GenerateClientOrigin() {
			return nextClientOrigin++;
		}
		public LiveWireServer() {
			processSignal = new AutoResetEvent(false);
			listenerSync = new object();
			connectionSync = new object();
			listener = null;
			connections = new Dictionary<int, WebSocketChannel>(16);
			subscriptions = new Dictionary<int, List<LiveCaptureSubscription>>(16);
			pendingRequests = new LinkedList<Task<JsonString>>();
			updates = new LinkedList<JsonString>();
			nextClientOrigin = ServerOrigin + 1;
		}
		public bool Started {
			get { 
				lock (listenerSync) {
					return listener != null; 
				}
			}
		}
		public IReadOnlyList<LiveCaptureSubscription> GetSubscriptions(int clientOrigin) {
			lock (connectionSync) {
				List<LiveCaptureSubscription> result;
				if (!subscriptions.TryGetValue(clientOrigin, out result)) {
					return new List<LiveCaptureSubscription>();
				}
				return result;
			}
		}
		public bool Subscribe(int clientOrigin, LiveCaptureSubscription subscription) {
			if (subscription == null) throw new ArgumentNullException("subscription");
			if (subscription.IsCancelled) {
				return false;
			}
			lock (connectionSync) {
				List<LiveCaptureSubscription> sub;
				if (!subscriptions.TryGetValue(clientOrigin, out sub)) {
					return false;
				}
				sub.Add(subscription);
				processSignal.Set();
				return true;
			}
		}
		public bool Start() {
			lock (listenerSync) {
				if (listener != null) {
					return true;
				}
				listener = new TcpListener(IPAddress.Parse("127.0.0.1"), ServerPort);
				try {
					listener.Start();
				} catch (SocketException) {
					listener = null;
					return false;
				}
				return true;
			}
		}
		public bool Stop() {
			lock (listenerSync) {
				if (listener == null) {
					return true;
				}
			}
			lock (connectionSync) {
				foreach (var client in connections.Values) {
					client.Close();
				}
				connections.Clear();
				foreach (var list in subscriptions.Values) {
					foreach (var subscription in list) {
						subscription.Cancel();
					}
					list.Clear();
				}
				subscriptions.Clear();
				updates.Clear();
			}
			lock (listenerSync) {
				if (listener == null) {
					return true;
				}
				listener.Stop();
				listener = null;
			}
			return true;
		}
		/// <summary>
		/// Adds a single update to be sent to the clients specified in it.
		/// </summary>
		/// <param name="update">Update message.</param>
		public void PostUpdate(JsonString update) {
			if (update == null) throw new ArgumentNullException("update");
			lock (connectionSync) {
				updates.AddLast(update);
			}
		}
		/// <summary>
		/// Processes pending connection requests.
		/// Registers new clients.
		/// </summary>
		private void ProcessListener() {
			TcpClient nextClient = null;
			// accept new clients
			lock (listenerSync) {
				if (listener == null) {
					return;
				}
				if (listener.Pending()) {
					nextClient = listener.AcceptTcpClient();
				}
			}
			// add new client connection
			// if new client was accepted
			if (nextClient == null) {
				return;
			}
			lock (connectionSync) {
				var channel = new WebSocketChannel(new TcpChannel(nextClient));
				int clientOrigin = GenerateClientOrigin();
				connections.Add(clientOrigin, channel);
				subscriptions.Add(clientOrigin, new List<LiveCaptureSubscription>());
				LogManager.Instance.LogMessage(this, string.Format("New LiveWire client {0}", clientOrigin));
			}
		}
		/// <summary>
		/// Processes active connections.
		/// Handles received messages and detects closed clients.
		/// </summary>
		private void ProcessConnections() {
			lock (connectionSync) {
				LinkedList<int> toRemove = null;
				foreach (var pair in connections) {
					WebSocketChannel channel = pair.Value;
					int clientOrigin = pair.Key;
					channel.Process();
					// handle incoming requests and updates
					// request handling may involve processing delays
					// so each request is wrapped in a new task to avoid user-observable lockups
					// a downside of this implementation is that consecutive requests could be handeled out of order
					while (channel.AvailableMessageNo > 0) {
						var message = channel.EmitMessage();
						Task<JsonString> task2 = new Task<JsonString>(delegate {
							JsonString request;
							try {
								request = JsonString.Parse(message.Text);
							} catch (FormatException) {
								LogManager.Instance.LogError(this, string.Format("failed to parse JSON from client {0}: {1}",
									clientOrigin, message.Text));
								return null;
							}
							return new LiveRequestHandler(this, clientOrigin).Respond(request); 
						});

						task2.ContinueWith((Task finished, object result) => {							
							Exception e = finished.Exception;
							if (e == null) {
								LogManager.Instance.LogError(this, string.Format("Task failed but no exception was thrown while processing request\n{0}",
									message.Text));
								return;
							}
							LogManager.Instance.LogMessage(this, string.Format("{0}\noccurred while processing request\n{1}",
								e, message.Text));
						}, TaskContinuationOptions.OnlyOnFaulted);

						task2.ContinueWith((Task finished, object result) => {
							LogManager.Instance.LogWarning(this, string.Format("Cancelled task unexpectedly while processing request\n{1}",
								message.Text));
						}, TaskContinuationOptions.OnlyOnCanceled);

						pendingRequests.AddLast(task2);
						task2.Start();

					}
					// close and remove disconnected clients
					if (channel.State == WebChannelState.InitFailed || channel.State == WebChannelState.Closed) {
						if (channel.State == WebChannelState.InitFailed) {
							LogManager.Instance.LogError(this, string.Format("LiveWire client {0} failed to connect", 
								clientOrigin));
						}
						if (channel.State == WebChannelState.Closed) {
							LogManager.Instance.LogMessage(this, string.Format("LiveWire client {0} disconnected",
								clientOrigin));
						}
						channel.Close(true);
						if (toRemove == null) {
							toRemove = new LinkedList<int>();
						}
						toRemove.AddLast(clientOrigin);
					} 
					// TODO implement pinging in order to detect suspended connections
				}
				// finally remove dead connections and kill corresponding subscriptions
				if (toRemove != null) {
					foreach (int origin in toRemove) {
						connections[origin].Close(true);
						connections.Remove(origin);
						foreach (var sub in subscriptions[origin]) {
							if (!sub.IsCancelled) {
								sub.Cancel();
							}
							sub.Dispose();
						}
						subscriptions.Remove(origin);
					}
				}
			}
		}
		/// <summary>
		/// Processes pending request tasks.
		/// Handles generated messages for sending out to clients
		/// and releases completed tasks.
		/// </summary>
		private void ProcessPendingRequests() {
			LinkedList<Task<JsonString>> completed = null;
			foreach (Task<JsonString> task in pendingRequests) {
				do {
					if (!task.IsCompleted) {
						break;
					}

					if (completed == null) {
						completed = new LinkedList<Task<JsonString>>();
					}
					completed.AddLast(task);
					JsonString response = null;
					try {
					// TODO check for exception or cancellation
						 response = task.Result;
					} catch (AggregateException) {
						// exception handeled elsewhere
					}
					if (response == null) {
						// may occur when not enough information is extractable
						// in order to send an error response
						LogManager.Instance.LogError(this, "Failed to deliver response for unrecognizable request");
						break;
					}
					updates.AddLast(response);
				} while (false);
			}
			if (completed != null) {
				foreach (var task in completed) {
					pendingRequests.Remove(task);
				}
			}
		}
		/// <summary>
		/// Processes active subscriptions.
		/// Handles generated messages for sending out to clients
		/// and detects cancelled subscriptions.
		/// </summary>
		private void ProcessSubscriptions() {
			lock (connectionSync) {
				foreach (var pair in subscriptions) {
					LinkedList<LiveCaptureSubscription>  toRemove = null;
					foreach (var subscription in pair.Value) {
						while (subscription.AvailableMessageNo > 0) {
							JsonString message = subscription.EmitMessage();
							if (message != null) {
								updates.AddLast(message);
							}
						}
						if (subscription.IsCancelled) {
							if (toRemove == null) {
								toRemove = new LinkedList<LiveCaptureSubscription>();
							}
							toRemove.AddLast(subscription);
						}
					}
					if (toRemove != null) {
						foreach (var subscription in toRemove) {
							subscription.Dispose();
							pair.Value.Remove(subscription);
						}
					}
				}
			}
		}
		/// <summary>
		/// Sends subscription, update and response messages to their target clients.
		/// </summary>
		private void ProcessUpdates() {
			lock (connectionSync) {
				foreach (var update in updates) {
					bool delivered = false;
					do {
						// send to all targets
						string target = update.GetTerminal("target", string.Empty);
						if (target.Equals(string.Empty)) {
							break;
						}
						if (target.Equals("\"all\"")) {
							// special send to all target
							string text = update.ToString();
							foreach (var connection in connections.Values) {
								connection.SendMessage(new WebSocketMessage(text));
							}
							delivered = true;
							break;
						} 
						// read singular target from field
						int targetId = JsonString.ParseInt(target, -1);
						if (targetId < 0) {
							break;
						}
						{
							WebSocketChannel connection;
							if (!connections.TryGetValue(targetId, out connection)) {
								break;
							}
							string text = update.ToString();
							connection.SendMessage(new WebSocketMessage(text));
						}
						delivered = true;
						break;
					} while (false);
					if (!delivered) {
						LogManager.Instance.LogError(this, string.Format("Failed to deliver message {0}", 
							update.ToJsonString()));
					}
				}
				updates.Clear();
			}
		}
		/// <summary>
		/// Processes all steadily reoccuring tasks of this instance.
		/// Handles connections, requests, updates and subcriptions.
		/// </summary>
		public void Process() {
			if (!Started) {
				return;
			}
			ProcessListener();
			ProcessConnections();
			ProcessSubscriptions();
			ProcessPendingRequests();
			ProcessUpdates();
		}
		public delegate JsonString RequestTask();

	}
}

