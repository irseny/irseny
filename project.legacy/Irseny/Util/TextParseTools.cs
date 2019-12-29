using System;
using System.IO;
using System.Globalization;

namespace Irseny.Util {
	public static class TextParseTools {
		public readonly static NumberStyles NumberStyle = NumberStyles.Float;
		public readonly static CultureInfo FormatProvider = CultureInfo.InvariantCulture;

		public static char ParseChar(string text, char fallback) {
			if (text == null) {
				return fallback;
			}
			if (text.Length > 0) {
				return text[0];
			} else {
				return fallback;
			}
		}
		public static float ParseFloat(string text, float fallback) {
			if (text == null) {
				return fallback;
			}
			if (text.Length > 0) {
				float result;
				if (float.TryParse(text, NumberStyle, FormatProvider, out result)) {
					return result;
				}
			}
			return fallback;
		}
		public static double ParseDouble(string text, double fallback) {
			if (text == null) {
				return fallback;
			}
			if (text.Length > 0) {
				double result;
				if (double.TryParse(text, NumberStyle, FormatProvider, out result)) {
					return result;
				}
			}
			return fallback;
		}
		/// <summary>
		/// Reads a boolean value from the given string.
		/// </summary>
		/// <returns><c>true</c>, if given value evaluates to "True", <c>false</c> otherwise.</returns>
		/// <param name="text">String to read from.</param>
		public static bool ParseBool(string text, bool fallback) {
			if (text == null) {
				return fallback;
			}
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

			}
			return fallback;
		}
		/// <summary>
		/// Reads an integer from the given string.
		/// </summary>
		/// <returns>The integer read.</returns>
		/// <param name="text">String to read from.</param>
		public static int ParseInt(string text, int fallback) {
			if (text == null) {
				return fallback;
			}
			if (text.Length > 0) {
				int result;
				if (int.TryParse(text, out result)) {
					return result;
				}
			}
			return fallback;
		}

		public static uint ParseUInt(string text, uint fallback) {
			if (text == null) {
				return fallback;
			}
			if (text.Length > 0) {
				uint result;
				if (uint.TryParse(text, out result)) {
					return result;
				}
			}
			return fallback;
		}
	}
}
