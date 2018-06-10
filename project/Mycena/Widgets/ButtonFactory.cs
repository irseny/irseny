using System;

namespace Mycena {
	internal class ButtonFactory {
		public ButtonFactory() {
		}

		public static bool SetUseStock<T>(T widget, ConfigProperties properties, IInterfaceNode container) where T : Gtk.Button {
			bool stock;
			try {
				stock = TextParseTools.ParseBool(properties.GetProperty("use_stock"));
			} catch (FormatException) {
				return false;
			}
			widget.UseStock = stock;
			return true;
		}
		public static bool SetLabel<T>(T widget, ConfigProperties properties, IInterfaceNode container) where T : Gtk.Button {
			string label = properties.GetProperty("label");
			widget.Label = label;
			return true;
		}
		public static bool SetImage<T>(T widget, ConfigProperties properties, IInterfaceNode container) where T : Gtk.Button {
			Gtk.Image image = container.GetGadget<Gtk.Image>(properties.GetProperty("image"), null);
			if (image == null) {
				return false;
			} else {
				widget.Image = image;
				return true;
			}
		}
		public static bool SetImagePosition<T>(T widget, ConfigProperties properties, IInterfaceNode container) where T : Gtk.Button {
			Gtk.PositionType position;
			try {
				position = TextParseTools.ParsePositionType(properties.GetProperty("image_position"));
			} catch (FormatException) {
				return false;
			}
			widget.ImagePosition = position;
			return true;
		}
		public static bool SetUnderline<T>(T widget, ConfigProperties properties, IInterfaceNode container) where T : Gtk.Button {
			bool underline;
			try {
				underline = TextParseTools.ParseBool(properties.GetProperty("use_underline"));
			} catch (FormatException) {
				return false;
			}
			widget.UseUnderline = underline;
			return true;
		}
	}
}
