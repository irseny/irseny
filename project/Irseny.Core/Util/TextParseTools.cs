using System;
using System.IO;
using System.Globalization;

namespace Irseny.Core.Util {
	public static class TextParseTools {
		public readonly static NumberStyles NumberStyle = NumberStyles.Float;
		public readonly static CultureInfo FormatProvider = CultureInfo.InvariantCulture;

		public static bool TryParseNull(string text, out object result) {
			if (text == null) {
				result = null;
				return true;
			}
			if (text.ToLower().Equals("null")) {
				result = null;
				return true;
			}
			result = null;
			return false;
		}
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
		public static bool TryParseChar(string text, out char result) {
			if (text == null) {
				result = '\0';
				return false;
			}
			if (text.Length > 0) {
				result = text[0];
				return false;
			} else {
				result = '\0';
				return false;
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
		public static bool TryParseFloat(string text, out float result) {
			if (text == null) {
				result = 0.0f;
				return false;
			}
			if (text.Length > 0) {
				return float.TryParse(text, NumberStyle, FormatProvider, out result);
			}
			result = 0.0f;
			return false;
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
		public static bool TryParseDouble(string text, out double result) {
			if (text == null) {
				result = 0.0;
				return false;
			}
			if (text.Length > 0) {
				return double.TryParse(text, NumberStyle, FormatProvider, out result);
			}
			result = 0.0;
			return false;
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
		public static bool TryParseBool(string text, out bool result) {
			if (text == null) {
				result = false;
				return false;
			}
			text = text.Trim();
			if (text.Length > 0) {
				int intValue;
				if (int.TryParse(text, out intValue)) {
					result = (intValue != 0);
					return true;
				}
				string lower = text.ToLower();
				if (lower.Equals("true")) {
					result = true;
					return true;
				}
				if (lower.Equals("false")) {
					result = false;
					return true;
				}
				if (lower.Equals("yes")) {
					result = true;
					return true;
				}
				if (lower.Equals("no")) {
					result = false;
					return true;
				}

			}
			result = false;
			return false;
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
		public static bool TryParseInt(string text, out int result) {
			if (text == null) {
				result = 0;
				return false;
			}
			if (text.Length > 0) {
				return int.TryParse(text, out result);
			}
			result = 0;
			return false;
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
		public static bool TryParseUInt(string text, out uint result) {
			if (text == null) {
				result = 0;
				return false;
			}
			if (text.Length > 0) {
				return uint.TryParse(text, out result);
			}
			result = 0;
			return false;
		}
	}
}
