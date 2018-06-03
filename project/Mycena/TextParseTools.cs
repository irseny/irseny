using System;

namespace Mycena {
	internal static class TextParseTools {
		public static bool ParseBool(string text) {
			text = text.Trim();

			if (text.Length > 0) {
				int intValue;
				if (int.TryParse(text, out intValue)) {
					return intValue != 0;
				}
				string lower = text.ToLower();
				if (lower.Equals("true")) {
					return true;
				}
				if (lower.Equals("false")) {
					return false;
				}
				if (lower.Equals("yes")) {
					return true;
				}
				if (lower.Equals("no")) {
					return false;
				}
				throw new FormatException(text + "not convertable to bool");
			} else {
				return false;
			}
		}
		public static int ParseInt(string text) {
			text = text.Trim();
			if (text.Length > 0) {
				int result;
				if (int.TryParse(text, out result)) {
					return result;
				} else {
					throw new FormatException(text + "not convertable to int");
				}
			} else {
				return 0;
			}
		}
	}
}

