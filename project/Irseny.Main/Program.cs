using System;
using System.Net;
using System.Text;
using Irseny.Core.Util;
using Irseny.Core.Log;
using Irseny.Core.Sensors;
using Irseny.Core.Tracking;
using Irseny.Core.Inco.Device;
using Irseny.Core.Listing;

using Irseny.Main.Content;
using Irseny.Main.Webface;

namespace Irseny.Main {
	public static class Program {
		//public static void Main(string[] args) {
		//	Console.WriteLine("Hello, World!");
		//	var server = new Webface.HttpServer(5643);
		//	int requestNo = 0;
		//	server.ClientAccepted += (object sender, EventArgs e) => {
		//		HttpListenerContext context = (HttpListenerContext)sender;
		//		if (context.Request.IsWebSocketRequest) {
		//			Console.WriteLine("websocket request!");
		//		}
		//		string request = context.Request.Url.AbsolutePath;
		//		//string name = context.User.Identity.Name;
		//		string type = context.Request.ContentType;
		//		Console.WriteLine(string.Format("Request {0} of {1}: {2}", requestNo, type, request));
		//		requestNo += 1;
		//		if (request.StartsWith("/")) {
		//			byte[] data = Encoding.UTF8.GetBytes(string.Format(index, requestNo));
		//			context.Response.ContentType = "text/html";
		//			context.Response.ContentLength64 = data.Length;
		//			context.Response.OutputStream.Write(data, 0, data.Length);
		//			//context.Response.StatusCode = (int)HttpStatusCode.OK;
					
		//			context.Response.OutputStream.Flush();
		//			context.Response.OutputStream.Close();

		//		}
		//	};

		//	server.Start();
		//	System.Threading.Thread.Sleep(30000);
		//	server.Stop();

		//}


//		public static void Main(string[] args) {
//
//			LogManager.MakeInstance(new LogManager());
//			Console.WriteLine("Hello, World!");
//			var server = new Webface.WebfaceServer();
//			server.Start();
//			Console.ReadLine();
//			server.Stop();
//
//		}
		public static void Main(string[] args) {
			{ // start main systems
				LogManager.MakeInstance(new LogManager());
				CaptureSystem.MakeInstance(new CaptureSystem());
				TrackingSystem.MakeInstance(new TrackingSystem());
				VirtualDeviceManager.MakeInstance(new VirtualDeviceManager());
			}
			{ // prepare content managers
				ContentMaster.MakeInstance(new ContentMaster());
				var contentSettings = new ContentManagerSettings();
				string resourceRoot = ContentMaster.FindResourceRoot();
				contentSettings.SetResourcePaths(resourceRoot, resourceRoot, "(no-file)");
				string userRoot = ContentMaster.FindConfigRoot();
				contentSettings.SetConfigPaths(userRoot, userRoot, "(no-file)");
				ContentMaster.Instance.Load(contentSettings);
			}
			{ // load last configuration
				new Emgu.CV.Mat();
				var profile = ContentMaster.Instance.Profiles.LoadDefaultProfile();
				new ProfileActivator().ActivateProfile(profile).Wait();
			}
			{ // start webserver
				var server = new Webface.WebfaceServer();
				server.Start();
				Console.ReadLine();
				server.Stop();
			}
			{ // cleanup main systems
				VirtualDeviceManager.MakeInstance(null);
				TrackingSystem.MakeInstance(null);
				CaptureSystem.MakeInstance(null);
				LogManager.MakeInstance(null);
				EquipmentMaster.MakeInstance(null);
			}
		}
	}
}
