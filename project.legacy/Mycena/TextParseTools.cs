﻿using System;
using System.IO;
using System.Globalization;

namespace Mycena {

	internal static class TextParseTools {
		readonly static NumberStyles numberStyle = NumberStyles.Float;
		readonly static CultureInfo formatProvider = CultureInfo.InvariantCulture;
		public static string ParsePath(string text) {
#if LINUX
			return text.Replace('\\', Path.DirectorySeparatorChar);
#elif WINDOWS
			return text.Replace('/', Path.DirectorySeparatorChar);
#else
			return text;
#endif
		}
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
				if (float.TryParse(text, numberStyle, formatProvider, out result)) {
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
				if (double.TryParse(text, numberStyle, formatProvider, out result)) {
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
		public static Gtk.ScrollablePolicy ParseScrollablePolicy(string text) {
			text = text.Trim().ToLower();
			if (text.Equals("minimum")) {
				return Gtk.ScrollablePolicy.Minimum;
			} else if (text.Equals("natural")) {
				return Gtk.ScrollablePolicy.Natural;
			} else {
				throw new FormatException(text + " not converticable to scrollable policy");
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
		public static Gtk.Orientation ParseOrientation(string text) {
			text = text.Trim().ToLower();
			if (text.Equals("horizontal")) {
				return Gtk.Orientation.Horizontal;
			} else if (text.Equals("vertical")) {
				return Gtk.Orientation.Vertical;
			} else {
				throw new FormatException(text + " can not be converted to orientation");
			}
		}
		public static Gtk.ShadowType ParseShadowType(string text) {
			text = text.Trim().ToLower();
			if (text.Equals("none")) {
				return Gtk.ShadowType.None;
			} else if (text.Equals("in")) {
				return Gtk.ShadowType.In;
			} else if (text.Equals("out")) {
				return Gtk.ShadowType.Out;
			} else if (text.Equals("etched_in")) {
				return Gtk.ShadowType.EtchedIn;
			} else if (text.Equals("etched_out")) {
				return Gtk.ShadowType.EtchedOut;
			} else {
				throw new FormatException(text + " can not be converted to shadow type");
			}
		}
		public static Gtk.ReliefStyle ParseReliefStyle(string text) {
			text = text.Trim().ToLower();
			if (text.Equals("none")) {
				return Gtk.ReliefStyle.None;
			} else if (text.Equals("half")) {
				return Gtk.ReliefStyle.Half;
			} else if (text.Equals("normal")) {
				return Gtk.ReliefStyle.Normal;
			} else {
				throw new FormatException(text + " can not be converted to relief style");
			}
		}
		public static Gtk.Align ParseAlignment(string text) {
			text = text.Trim().ToLower();
			if (text.Equals("fill")) {
				return Gtk.Align.Fill;
			} else if (text.Equals("start")) {
				return Gtk.Align.Start;
			} else if (text.Equals("center")) {
				return Gtk.Align.Center;
			} else if (text.Equals("end")) {
				return Gtk.Align.End;
			} else if (text.Equals("baseline")) {
				return Gtk.Align.Baseline;
			} else {
				throw new FormatException(text + " can not be converted to alignment");
			}
		}
	}
}
