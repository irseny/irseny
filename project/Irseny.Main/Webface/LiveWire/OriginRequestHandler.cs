using System;
using System.Net;
using Irseny.Core.Util;

namespace Irseny.Main.Webface {
	public class OriginRequestHandler {
		int serverOrigin;
		int clientOrigin;
		public OriginRequestHandler(int serverOrigin, int clientOrigin) {
			this.serverOrigin = serverOrigin;
			this.clientOrigin = clientOrigin;
		}
		public HttpStatusCode Respond(JsonString subject, JsonString response) {
			if (subject == null) throw new ArgumentNullException("subject");
			if (response == null) throw new ArgumentNullException("response");
			// send the client and server origin ids as subject
			// while the type must be 'get' which is answered with 'post'
			HttpStatusCode status = HttpStatusCode.OK;
			do {
				string type = TextParseTools.ParseString(subject.GetTerminal("type", "error"), "error");
				if (!type.Equals("get")) {
					status = HttpStatusCode.Forbidden;
					break;
				}
				response.AddTerminal("origin", StringifyTools.StringifyInt(serverOrigin), true);
				var rSubject = JsonString.CreateDict();
				{
					rSubject.AddTerminal("type", StringifyTools.StringifyString("post"));
					rSubject.AddTerminal("topic", StringifyTools.StringifyString("origin"));
					var data = JsonString.CreateDict();
					{
						data.AddTerminal("serverOrigin", StringifyTools.StringifyInt(serverOrigin));
						data.AddTerminal("clientOrigin", StringifyTools.StringifyInt(clientOrigin));
					}
					rSubject.AddJsonString("data", data);
				}
				response.AddJsonString("subject", rSubject);
			} while (false);
			return status;
		}
	}
}

