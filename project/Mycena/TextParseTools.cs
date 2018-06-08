using System;

namespace Mycena {
	internal static class TextParseTools {
		/// <summary>
		/// Reads a boolean value from the given string.
		/// </summary>
		/// <returns><c>true</c>, if given value evaluates to "True", <c>false</c> otherwise.</returns>
		/// <param name="text">String to read from.</param>
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
				throw new FormatException(text + " not convertable to bool");
			} else {
				return false;
			}
		}
		/// <summary>
		/// Reads an integer from the given string.
		/// </summary>
		/// <returns>The integer read.</returns>
		/// <param name="text">String to read from.</param>
		public static int ParseInt(string text) {
			text = text.Trim();
			if (text.Length > 0) {
				int result;
				if (int.TryParse(text, out result)) {
					return result;
				} else {
					throw new FormatException(text + " not convertable to int");
				}
			} else {
				return 0;
			}
		}
		/// <summary>
		/// Reads a policy type value from the given string.
		/// </summary>
		/// <returns>The policy type value.</returns>
		/// <param name="text">String to read from.</param>
		public static Gtk.PolicyType ParsePolicyType(string text) {
			text = text.Trim();
			if (text.Equals("automatic")) {
				return Gtk.PolicyType.Automatic;
			} else if (text.Equals("always")) {
				return Gtk.PolicyType.Always;
			} else if (text.Equals("never")) {
				return Gtk.PolicyType.Never;
			} else {
				throw new FormatException(text + " not convertible to policy type");
			}
		}
	}
}

