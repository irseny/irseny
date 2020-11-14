using System;
using System.Collections.Generic;
using Irseny.Core.Util;

namespace Irseny.Main.Webface {
	public class LiveUpdateHandler {
		public LiveUpdateHandler() {
		}

		public bool TryApplyUpdate(JsonString update, out JsonString result) {
			if (update == null) throw new ArgumentNullException("update");
			result = null;
			string fallback = string.Empty;
			string type = TextParseTools.ParseString(update.GetTerminal("type", fallback), fallback);
			if (!type.Equals("update")) {
				return false;
			}

			return false;
		}
	}
}

