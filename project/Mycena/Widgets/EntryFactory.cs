using System;
namespace Mycena {
	public static class EntryFactory {
		public static bool SetMaxLength<T>(T widget, ConfigProperties properties, IInterfaceNode container) where T : Gtk.Entry {
			int maxLength;
			try {
				maxLength = TextParseTools.ParseInt(properties.GetProperty("max_length"));
			} catch (FormatException) {
				return false;
			}
			widget.MaxLength = maxLength;
			return true;
		}
		public static bool SetInvisibleChar<T>(T widget, ConfigProperties properties, IInterfaceNode container) where T : Gtk.Entry {
			char invisible;
			try {
				invisible = TextParseTools.ParseChar(properties.GetProperty("invisible_char"));
			} catch (FormatException) {
				return false;
			}
			widget.InvisibleChar = invisible;
			return true;
		}
		public static bool SetWidthChars<T>(T widget, ConfigProperties properties, IInterfaceNode container) where T : Gtk.Entry {
			int width;
			try {
				width = TextParseTools.ParseInt(properties.GetProperty("width_chars"));
			} catch (FormatException) {
				return false;
			}
			widget.WidthChars = width;
			return true;
		}
	}
}
