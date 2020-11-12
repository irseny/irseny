using System;
using System.Net;
using System.Text;
using Irseny.Core.Util;
using Irseny.Core.Log;

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
		public static void Main(string[] args) {

			LogManager.MakeInstance(new LogManager());
			Console.WriteLine("Hello, World!");
			var server = new Webface.WebfaceServer();
			server.Start();
			Console.ReadLine();
			server.Stop();

			Irseny.Core.Util.JsonString.Parse(@"");

			var partition = JsonString.PartitionJson(@"{
  ""Date"": ""2019-08-01T00:00:00-07:00"",
  ""TemperatureCelsius"": 25,
  ""Summary"": ""Hot"",
  ""DatesAvailable"": [
    ""2019-08-01T00:00:00-07:00"",
    ""2019-08-02T00:00:00-07:00""
  ],
  ""TemperatureRanges"": {
    ""Cold"": {
      ""High"": 20,
      ""Low"": -10
    },
    ""Hot"": {
      ""High"": 60,
      ""Low"": 20
    }
  },
  ""SummaryWords"": [
    ""Cool"",
    ""Windy"",
    ""Humid""
  ]
}");
//			var partition = JsonString.PartitionJson(@"{
//	'jamesbond': 'gravel',
//	'harry': {
//		'calamity': 'jane',
//		'lucky':'luke',
//		'selmy':{
//			'area':1234
//		}
//	}
//}");
			Console.WriteLine("Parts:");
			foreach (string part in partition) {
				Console.WriteLine(part);
			}
			Console.WriteLine("Formatted:");
			Console.WriteLine(partition.ToJsonString());
			JsonString str = JsonString.InterpretJson(partition);
			Console.WriteLine("Interpreted:");
			Console.WriteLine(str.ToJsonString());
			return;
		}
	}
}
