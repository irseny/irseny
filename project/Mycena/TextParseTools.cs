using System;

namespace Mycena {
	internal static class TextParseTools {
		public static char ParseChar(string text) {
			if (text.Length > 0) {
				return text[0];
			} else {
				return '\0';
			}
		}
		public static float ParseFloat(string text) {
			if (text.Length > 0) {
				float result;
				if (float.TryParse(text, out result)) {
					return result;
				} else {
					throw new FormatException(text + " not convertible to float");
				}
			} else {
				return 0;
			}
		}
		public static double ParseDouble(string text) {
			if (text.Length > 0) {
				double result;
				if (double.TryParse(text, out result)) {
					return result;
				} else {
					throw new FormatException(text + " not convertible to double");
				}
			} else {
				return 0;
			}
		}
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
				throw new FormatException(text + " not convertible to bool");
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
			if (text.Length > 0) {
				int result;
				if (int.TryParse(text, out result)) {
					return result;
				} else {
					throw new FormatException(text + " not convertible to int");
				}
			} else {
				return 0;
			}
		}

		public static uint ParseUInt(string text) {
			if (text.Length > 0) {
				uint result;
				if (uint.TryParse(text, out result)) {
					return result;
				} else {
					throw new FormatException(text + " not convertible to uint");
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
		public static Gtk.IconSize ParseIconSize(string text) {
			int value;
			if (int.TryParse(text, out value)) {
				switch (value) {
				case 0:
					return Gtk.IconSize.Invalid;
				case 1:
					return Gtk.IconSize.Menu;
				case 2:
					return Gtk.IconSize.SmallToolbar;
				case 3:
					return Gtk.IconSize.LargeToolbar;
				case 4:
					return Gtk.IconSize.Button;
				case 5:
					return Gtk.IconSize.Dnd;
				case 6:
					return Gtk.IconSize.Dialog;
				}
			} else {
				text = text.Trim().ToLower();
				if (text.Equals("invalid")) {
					return Gtk.IconSize.Invalid;
				} else if (text.Equals("button")) {
					return Gtk.IconSize.Button;
				} else if (text.Equals("dialog")) {
					return Gtk.IconSize.Dialog;
				} else if (text.Equals("dnd")) {
					return Gtk.IconSize.Dnd;
				} else if (text.Equals("menu")) {
					return Gtk.IconSize.Menu;
				} else if (text.Equals("small-toolbar")) {
					return Gtk.IconSize.SmallToolbar;
				} else if (text.Equals("large-toolbar")) {
					return Gtk.IconSize.LargeToolbar;
				}
			}
			throw new FormatException(text + "not convertible to icon size");
		}
		public static Gtk.PositionType ParsePositionType(string text) {
			text = text.Trim().ToLower();
			if (text.Equals("right")) {
				return Gtk.PositionType.Right;
			} else if (text.Equals("left")) {
				return Gtk.PositionType.Left;
			} else if (text.Equals("top")) {
				return Gtk.PositionType.Top;
			} else if (text.Equals("bottom")) {
				return Gtk.PositionType.Bottom;
			} else {
				throw new FormatException(text + " not convertible to position type");
			}
		}
		public static Gtk.PackType ParsePackType(string text) {
			text = text.Trim().ToLower();
			if (text.Equals("start")) {
				return Gtk.PackType.Start;
			} else if (text.Equals("end")) {
				return Gtk.PackType.End;
			} else {
				throw new FormatException(text + " can not be converted to pack type");
			}
		}
		public static Gtk.AttachOptions ParseAttachOptions(string text) {
			Gtk.AttachOptions result = 0;
			if (text.Length > 0) {
				string[] sComps = text.ToLower().Split('|');
				for (int i = 0; i < sComps.Length; i++) {
					string option = sComps[i].Trim();
					if (option.Equals("gtk_fill")) {
						result |= Gtk.AttachOptions.Fill;
					} else if (option.Equals("gtk_expand")) {
						result |= Gtk.AttachOptions.Expand;
					} else if (option.Equals("gtk_shrink")) {
						result |= Gtk.AttachOptions.Shrink;
					} else {
						throw new FormatException(text + " can not be converted to attach options");
					}
				}
			}
			return result;
		}
		public static Gtk.WrapMode ParseWrapMode(string text) {
			text = text.Trim().ToLower();
			if (text.Length > 0) {
				if (text.Equals("char")) {
					return Gtk.WrapMode.Char;
				} else if (text.Equals("word")) {
					return Gtk.WrapMode.Word;
				} else if (text.Equals("word_char")) {
					return Gtk.WrapMode.WordChar;
				} else {
					throw new FormatException(text + " can not be converted to wrap mode");
				}
			} else {
				return Gtk.WrapMode.None;
			}
		}
	}
}

