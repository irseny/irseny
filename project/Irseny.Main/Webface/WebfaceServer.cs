using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Text;
using Irseny.Core.Util;

namespace Irseny.Main.Webface {
	public class WebfaceServer  {
		readonly AutoResetEvent stopSignal = new AutoResetEvent(false);
		Thread listenerThread = null;
		List<IWebChannel> channels;
		// non accepted channels with non tested junctions


		public WebfaceServer() {
			channels = new List<IWebChannel>();
		}
		public bool Start() {
			if (listenerThread != null) {
				return false;
			}
			listenerThread = new Thread(Process);
			listenerThread.Start();
			return true;
		}
		private void Process() {
			// wait for clients and process channels and changes
			var resourceListener = new TcpListener(IPAddress.Parse("127.0.0.1"), 9232);
			//var liveListener = new TcpListener(IPAddress.Parse("127.0.0.1"), 9234);
			var liveWire = new LiveWireServer();
			liveWire.Start();
			resourceListener.Start();
			//liveListener.Start();
			Console.WriteLine("Started server on port 9232");
			while (!stopSignal.WaitOne(16)) {
				liveWire.Process();
				// accept new clients
				if (resourceListener.Pending()) {
					var client = resourceListener.AcceptTcpClient();
					//client.NoDelay = true;
					//client.ReceiveTimeout = 1;
					//client.SendTimeout = 1;
					var baseChannel = new TcpChannel(client);
					var channel = new HttpChannel(baseChannel);
					//var channel = new WebSocketChannel(baseChannel);
					AddChannelPrototype(channel);

				}
//				if (liveListener.Pending()) {
//					var client = liveListener.AcceptTcpClient();
//					var baseChannel = new TcpChannel(client);
//					var channel = new WebSocketChannel(baseChannel);
//					AddChannelPrototype(channel);
//				}
				List<IWebChannel> toClose = new List<IWebChannel>();
				foreach (var c in channels) {
					c.Process();
					if (c.State == WebChannelState.InitFailed || c.State == WebChannelState.Closed) {
						toClose.Add(c);
						continue;
					}

					if (c.AvailableMessageNo > 0) {
						var wc = c as WebSocketChannel;
						if (wc != null) {
							WebSocketMessage message = wc.EmitMessage();
							string text = message.Text;
							Console.WriteLine("received: " + text);
							//string updateMsg = "{ 'type': 'request', 
							Console.WriteLine("circling that");
							//string updateMsg = "{ 'sender': 'minority', 'type': 'config',  'update', 'content': 'sent to update division' }";
							//Console.WriteLine("sending update: " + updateMsg);
							//Thread.Sleep(1000);
							JsonString str = JsonString.Parse(text);
							str.AddTerminal("text", "tentacles");
							wc.SendMessage(new WebSocketMessage(str.ToJsonString()));

							wc.SendMessage(new WebSocketMessage("{\"type\":\"update\", \"subject\":\"content\"  }"));
						}
						var hc = c as HttpChannel;
						if (hc != null) {
							HttpMessage message = hc.EmitMessage();
							//Console.WriteLine("message: ");
							//Console.WriteLine(message.Header.ToString());
							//Console.WriteLine(Encoding.UTF8.GetString(message.Content));
							string resource = message.Header.Resource;

							byte[] responseContent = new Services.ResourceService().ProvideResource(resource);
							HttpHeader responseHeader;
							if (responseContent.Length > 0) {
								responseHeader = new HttpHeader(HttpMethod.Response, resource, message.Header.Version, HttpStatusCode.OK);

								if (resource.EndsWith(".js")) {
									responseHeader.Fields.Add("Content-Type", "text/javascript; charset=UTF-8");
								} else if (resource.EndsWith(".html")) { 
									responseHeader.Fields.Add("Content-Type", "text/html; charset=UTF-8");
								} else if (resource.EndsWith(".css")) {
									responseHeader.Fields.Add("Content-Type", "text/css; charset=UTF-8");
								} else if (resource.EndsWith(".ico")) {
									responseHeader.Fields.Add("Content-Type", "image/vnd.microsoft.icon");
								} else {
									responseHeader.Fields.Add("Content-Type", "text/plain; charset=UTF-8");
								}
								responseHeader.Fields.Add("Content-Encoding", "identity");
								responseHeader.Fields.Add("Content-Length", string.Format("{0}", responseContent.Length));
								//responseHeader.Fields.Add("Transfer-Encoding", "identity");
								responseHeader.Fields.Add("Connection", "close");
							} else {
								responseHeader = new HttpHeader(HttpMethod.Response, resource, message.Header.Version, HttpStatusCode.NotFound);
								responseHeader.Fields.Add("Content-Length", "0");
								//responseHeader.Fields.Add("Transfer-Encoding", "identity");
								responseHeader.Fields.Add("Connection", "close");
							}
							responseHeader.Fields.Add("Server", "Irseny");
							responseHeader.Fields.Add("Date", DateTime.UtcNow.ToString());

							var response = new HttpMessage(responseHeader, responseContent);
							
							((HttpChannel)c).SendMessage(response);
							((HttpChannel)c).Flush();
						}

					}

				}
				foreach (IWebChannel c in toClose) {
					c.Close();
					Console.WriteLine("closed client " + channels.IndexOf(c));
					channels.Remove(c);
				}
			}
			foreach (var c in channels) { 
				c.Close();
			}
			resourceListener.Stop();
		}

		public void Stop() {
			stopSignal.Set();
		}
		public void AddChannelPrototype(IWebChannel channel) {
			channels.Add(channel);
			Console.WriteLine("new client " + (channels.Count - 1));

		}
		public void RejectChannelPrototype(IWebChannel channel) {

		}
		public void AcceptChannelPrototype(IWebChannel channel) {
			//acceptedChannels.Add(channel);
		}
//		public void AddJunction(IWebJunction junction, string[] path, int pathStart) {
//			if (path.Length < 1) throw new ArgumentException("path.Length");
//			if (pathStart < 0) throw new ArgumentException("pathStart");
//			if (pathStart >= path.Length) throw new ArgumentException("pathStart");
//			if (pathStart < path.Length - 1) {
//				string name = path[pathStart];
//			}
//		}
	}
}
