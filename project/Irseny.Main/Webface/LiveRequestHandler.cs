using System;
using System.Collections.Generic;
using Irseny.Core.Util;

namespace Irseny.Main.Webface {
	public class LiveRequestHandler {
		
		public LiveRequestHandler () {
		}
		public bool TryAnswer(JsonString request, out JsonString answer) {
			answer = null;
			if (request == null) {
				return false;
			}
			// check the signature
			string fallback = string.Empty;
			string type = TextParseTools.ParseString(request.GetTerminal("type", fallback), fallback);
			if (!type.Equals("request")) {
				return false;
			}
			JsonString subject = request.TryGetJsonString("subject");
			if (subject == null) {
				return false;
			}
			// interpret subject information

			return false;
		}
	}
}

