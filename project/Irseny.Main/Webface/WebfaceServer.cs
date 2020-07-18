using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Text;

namespace Irseny.Main.Webface {
	public class WebfaceServer  {
		readonly AutoResetEvent stopSignal = new AutoResetEvent(false);
		Thread listenerThread = null;
		List<IWebChannel> channels;
		int clientNo = 0;
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
			var listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 9232);
			listener.Start();
			Console.WriteLine("listening");
			while (!stopSignal.WaitOne(16)) {
				// accept new clients
				if (listener.Pending()) {
					var client = listener.AcceptTcpClient();
					//client.NoDelay = true;
					//client.ReceiveTimeout = 1;
					//client.SendTimeout = 1;
					var baseChannel = new TcpChannel(client);
					var channel = new HttpChannel(baseChannel);
					//var channel = new WebSocketChannel(baseChannel);
					AddChannelPrototype(channel);

				}
				foreach (var c in channels) {
					c.Process();
					if (c.AvailableMessageNo > 0) {


						HttpMessage message = ((HttpChannel)c).EmitMessage();
						//Console.WriteLine("message: ");
						//Console.WriteLine(message.Header.ToString());
						//Console.WriteLine(Encoding.UTF8.GetString(message.Content));
						string resource = message.Header.Resource;

						byte[] responseContent = new Services.ResourceService().ProvideResource(resource);
						HttpHeader responseHeader;
						if (responseContent.Length > 0) {
							responseHeader = new HttpHeader(HttpMethod.Response) { 
								Resource = resource,
							    Status = HttpStatusCode.OK
							};
							if (resource.EndsWith(".js")) {
								responseHeader.Fields.Add("Content-Type", "text/javascript; charset=UTF-8");
							} else if (resource.EndsWith(".html")) { 
								responseHeader.Fields.Add("Content-Type", "text/html; charset=UTF-8");
							} else if (resource.EndsWith(".css")) {
								responseHeader.Fields.Add("Content-Type", "text/css; charset=UTF-8");
							} else if (resource.EndsWith(".ico")) {
								responseHeader.Fields.Add("Cotnent-Type", "image/vnd.microsoft.icon");
							} else {
								responseHeader.Fields.Add("Content-Type", "text/plain; charset=UTF-8");
							}
							responseHeader.Fields.Add("Content-Encoding", "identity");
							responseHeader.Fields.Add("Content-Length", string.Format("{0}", responseContent.Length));
							//responseHeader.Fields.Add("Transfer-Encoding", "identity");
							responseHeader.Fields.Add("Connection", "close");
						} else {
							responseHeader = new HttpHeader(HttpMethod.Response) {
								Resource = resource,
								Status = HttpStatusCode.NotFound
							};
							responseHeader.Fields.Add("Content-Length", "0");
							//responseHeader.Fields.Add("Transfer-Encoding", "identity");
							responseHeader.Fields.Add("Connection", "close");
						}
						responseHeader.Fields.Add("Server", "Irseny");
						responseHeader.Fields.Add("Date", DateTime.UtcNow.ToString());

						var response = new HttpMessage(responseHeader, responseContent);
						
						((HttpChannel)c).SendMessage(response);
						((HttpChannel)c).Flush();


						//Console.WriteLine("answer: ");
						//Console.WriteLine(response.Header.ToString());
						//Console.WriteLine(Encoding.UTF8.GetString(response.Content));

					}
				}
			}
			foreach (var c in channels) { 
				c.Close();
			}
			listener.Stop();
		}

		public void Stop() {
			stopSignal.Set();
		}
		public void AddChannelPrototype(IWebChannel channel) {
			channels.Add(channel);
			Console.WriteLine("new client " + clientNo++);

		}
		public void RejectChannelPrototype(IWebChannel channel) {

		}
		public void AcceptChannelPrototype(IWebChannel channel) {
			//acceptedChannels.Add(channel);
		}
		public void AddJunction(IWebJunction junction, string[] path, int pathStart) {
			if (path.Length < 1) throw new ArgumentException("path.Length");
			if (pathStart < 0) throw new ArgumentException("pathStart");
			if (pathStart >= path.Length) throw new ArgumentException("pathStart");
			if (pathStart < path.Length - 1) {
				string name = path[pathStart];
			}
		}
	}
}
