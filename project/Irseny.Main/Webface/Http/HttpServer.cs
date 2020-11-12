using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
namespace Irseny.Main.Webface {
	public class HttpServer {
		Thread listenThread = null;
		readonly AutoResetEvent stopSignal = new AutoResetEvent(false);
		public int Port { get; private set; }
		public bool Started {
			get {  return listenThread != null; }
		}
		public event EventHandler<EventArgs> ClientAccepted; 


		public HttpServer(int port) {
			Port = port;
		}
		
		public void Start() {
			if (Started) {
				return;
			}
			listenThread = new Thread(Listen);
			listenThread.Start();

		}
		public void Stop() {
			if (!Started) {
				return;
			}
			stopSignal.Set();
			listenThread.Join();
			listenThread = null;
		}
		private void Listen() {
			var listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 8006);
			listener.Start();

			while (!stopSignal.WaitOne(16)) {
				if (listener.Pending()) {
					try { 
						TcpClient client = listener.AcceptTcpClient();
						OnClientAccepted(listener, new EventArgs());
					} catch (SocketException) {
						// TODO log
					}
				}
			}
			listener.Stop();
			//var listener = new HttpListener();
			//listener.Prefixes.Add(string.Format("http://*:{0}/", Port));
			//listener.Prefixes.Add(string.Format("ws://*:{0}/", Port));
			//listener.IgnoreWriteExceptions = true;
			//listener.Start();
			//if (!listener.IsListening) {
			//	return;
			//}
			//Task<HttpListenerContext> contextProvider;
			//try {
			//	contextProvider = listener.GetContextAsync();
			//} catch (HttpListenerException e) {
			//	return;
			//}
			//while (!stopSignal.WaitOne(16)) {
			//	if (contextProvider.IsCompleted) {
			//		HttpListenerContext context = null;
			//		try { 
			//			context = contextProvider.Result;
			//		} catch (HttpListenerException e) {
			//			// TODO log
			//		} finally {
			//			if (context != null) {
			//				OnClientAccepted(context, new EventArgs());
			//			}
			//		}
			//		contextProvider = listener.GetContextAsync();
			//	}

			//}

		}
		private void OnClientAccepted(object client, EventArgs args) {
			ClientAccepted(client, args);
		}
	}
}
