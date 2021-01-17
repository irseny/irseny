using System;
using System.Net;
using Irseny.Core.Util;

namespace Irseny.Main.Webface.LiveWire {
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
				string type = JsonString.ParseString(subject.GetTerminal("type", "error"), "error");
				if (!type.Equals("get")) {
					status = HttpStatusCode.Forbidden;
					break;
				}
				response.AddTerminal("origin", JsonString.Stringify(serverOrigin));
				var rSubject = JsonString.CreateDict();
				{
					rSubject.AddTerminal("type", JsonString.StringifyString("post"));
					rSubject.AddTerminal("topic", JsonString.StringifyString("origin"));
					var data = JsonString.CreateDict();
					{
						data.AddTerminal("serverOrigin", JsonString.Stringify(serverOrigin));
						data.AddTerminal("clientOrigin", JsonString.Stringify(clientOrigin));
					}
					rSubject.AddJsonString("data", data);
				}
				response.AddJsonString("subject", rSubject);
			} while (false);
			return status;
		}
	}
}

