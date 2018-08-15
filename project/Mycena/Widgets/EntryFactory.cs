using System;

namespace Mycena {
	internal class EntryFactory : WidgetFactory<Gtk.Entry> {
		public EntryFactory() : base() {
			CreationProperties.Add("text", SetText);
			CreationProperties.Add("editable", SetEditable);
			CreationProperties.Add("max_length", SetMaxLength);
			CreationProperties.Add("invisible_char", SetInvisibleChar);
			CreationProperties.Add("width_chars", SetWidth);
			//CreationProperties.Add("max_width_chars", SetMaxWidth);
		}
		protected override Gtk.Entry CreateWidget(ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) {
			return new Gtk.Entry();
		}
		public static bool SetText<T>(T widget, ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) where T : Gtk.Entry {
			string text = properties.GetProperty("text");
			widget.Text = text;
			return true;
		}
		public static bool SetEditable<T>(T widget, ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) where T : Gtk.Entry {
			bool editable;
			try {
				editable = TextParseTools.ParseBool(properties.GetProperty("editable"));
			} catch (FormatException) {
				return false;
			}
			widget.IsEditable = editable;
			return true;
		}
		public static bool SetMaxLength<T>(T widget, ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) where T : Gtk.Entry {
			int maxLength;
			try {
				maxLength = TextParseTools.ParseInt(properties.GetProperty("max_length"));
			} catch (FormatException) {
				return false;
			}
			widget.MaxLength = maxLength;
			return true;
		}
		public static bool SetInvisibleChar<T>(T widget, ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) where T : Gtk.Entry {
			char invisible;
			try {
				invisible = TextParseTools.ParseChar(properties.GetProperty("invisible_char"));
			} catch (FormatException) {
				return false;
			}
			widget.InvisibleChar = invisible;
			return true;
		}
		public static bool SetWidth<T>(T widget, ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) where T : Gtk.Entry {
			int width;
			try {
				width = TextParseTools.ParseInt(properties.GetProperty("width_chars"));
			} catch (FormatException) {
				return false;
			}
			widget.WidthChars = width;
			return true;
		}
		/*public static bool SetMaxWidth<T>(T widget, ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) where T : Gtk.Entry {
			int width;
			try {
				width = TextParseTools.ParseInt(properties.GetProperty("max_width_chars"));
			} catch (FormatException) {
				return false;
			}
			widget.MaxWidth = width;
			return true;
		}*/
	}
}
