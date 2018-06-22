using System;
using System.Xml;
using System.Collections.Generic;

namespace Mycena {
	internal class WindowFactory : WidgetFactory<Gtk.Window> {
		public WindowFactory() : base() {
			CreationProperties.Add("default_width", SetDefaultSize);
			CreationProperties.Add("default_height", SetDefaultSize);
		}

		protected override Gtk.Window CreateWidget(ConfigProperties properties, IInterfaceNode container) {
			bool top;
			try {
				top = TextParseTools.ParseBool(properties.GetProperty("top_level", true));
			} catch (FormatException) {
				return null;
			}
			if (top) {
				return new Gtk.Window(Gtk.WindowType.Toplevel);
			} else {
				return new Gtk.Window(Gtk.WindowType.Popup);
			}
		}
		protected override bool PackWidget(Gtk.Window container, Gtk.Widget child, ConfigProperties properties) {
			if (container.Child != null) {
				return false;
			} else {
				container.Add(child);
				return true;
			}
		}
		private static bool SetDefaultSize(Gtk.Window widget, ConfigProperties properties, IInterfaceNode container) {
			int width, height;
			try {
				width = TextParseTools.ParseInt(properties.GetProperty("default_width", -1));
				height = TextParseTools.ParseInt(properties.GetProperty("default_height", -1));

			} catch (FormatException) {
				return false;
			}
			if (width < 0) {
				width = widget.DefaultWidth;
			}
			if (height < 0) {
				height = widget.DefaultHeight;
			}
			widget.SetDefaultSize(width, height);
			return true;
		}
	}
}

