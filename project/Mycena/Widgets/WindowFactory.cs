using System;
using System.Xml;
using System.Collections.Generic;

namespace Mycena {
	internal class WindowFactory : WidgetFactory<Gtk.Window> {
		public WindowFactory() : base() {

		}

		protected override Gtk.Window CreateWidget(ConfigProperties properties, IInterfaceNode container) {
			bool top;
			try {
				top = TextParseTools.ParseBool(properties.GetProperty("top_level", false));
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
	}
}

