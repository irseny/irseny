using System;

namespace Irseny.Core.Util
{
	public partial class JsonString
	{
		public static string ParseString(string text, string fallback) {
			if (text == null) {
				return fallback;
			}
			if (text.Length < 2) {
				return text;
			}
			// try trim start and end quotes
			char first = text[0];
			char last = text[text.Length - 1];
			if (first == '"' && last == '"') {
				return text.Substring(1, text.Length - 2);
			}
			if (first == '\'' && last == '\'') {
				return text.Substring(1, text.Length - 2);
			}
			return text;
		}
	}
}

