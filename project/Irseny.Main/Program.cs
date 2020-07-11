using System;
using System.Net;
using System.Text;
namespace Irseny.Main {
	public static class Program {
		public static void Main(string[] args) {
			Console.WriteLine("Hello, World!");
			var server = new Webface.HttpServer(5643);
			int requestNo = 0;
			server.ClientAccepted += (object sender, EventArgs e) => {
				HttpListenerContext context = (HttpListenerContext)sender;
				if (context.Request.IsWebSocketRequest) {
					Console.WriteLine("websocket request!");
				}
				string request = context.Request.Url.AbsolutePath;
				//string name = context.User.Identity.Name;
				string type = context.Request.ContentType;
				Console.WriteLine(string.Format("Request {0} of {1}: {2}", requestNo, type, request));
				requestNo += 1;
				if (request.StartsWith("/")) {
					byte[] data = Encoding.UTF8.GetBytes(string.Format(index, requestNo));
					context.Response.ContentType = "text/html";
					context.Response.ContentLength64 = data.Length;
					context.Response.OutputStream.Write(data, 0, data.Length);
					//context.Response.StatusCode = (int)HttpStatusCode.OK;
					
					context.Response.OutputStream.Flush();
					context.Response.OutputStream.Close();

				}
			};

			server.Start();
			System.Threading.Thread.Sleep(30000);
			server.Stop();

		}
		const string index = @"
<!DOCTYPE html>
<html lang='en'>
<meat charset='utf-8'/>
<head>

<title>head</title>

</head>
<body id='home'>

body of request {0}
<!--<img src='http://localhost:5643/images/image.png' alt='does not exist' title='an image'>-->
<script language='javascript'>

var ws = WebSocket('ws://localhost:5643')
ws.onopen = function(event) {{
	alert('websocket open');
}};
ws.onerror = function(event) {{
	alert('websocket error: ', event);
}}
ws.onclose = function(event) {{
	alert('websocket closing: ', event);
}}

</script>


<body>

</html>";

	}

}
