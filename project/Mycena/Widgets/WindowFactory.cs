using System;
using System.Xml;
using System.Collections.Generic;

namespace Mycena {
	internal class WindowFactory : WidgetFactory<Gtk.Window> {
		public WindowFactory() : base() {
			
		}

		protected override Gtk.Window CreateWidget(ConfigProperties properties, IInterfaceNode container) {
			var result = new Gtk.Window(Gtk.WindowType.Toplevel);
			ApplyProperties(result, properties, container);
			return result;
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

