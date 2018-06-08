using System;
using System.Collections.Generic;

namespace Mycena {
	internal abstract class BoxFactory<T> : WidgetFactory<T> where T : Gtk.Box {
		public BoxFactory() {
			
		}
		protected override bool PackWidget(T container, Gtk.Widget child, ConfigProperties properties) {
			bool start = true;
			try {
				start = TextParseTools.ParseBool(properties.GetProperty("start", "true"));
			} catch (FormatException) {
				return false;
			}
			if (start) {
				container.PackStart(child);
			} else {
				container.PackEnd(child);
			}
			return true;
		}
	}

	internal class HorizontalBoxFactory : BoxFactory<Gtk.HBox> {
		protected override Gtk.HBox CreateWidget(ConfigProperties properties, IInterfaceNode container) {
			var result = new Gtk.HBox();
			ApplyProperties(result, properties, container);
			return result;
		}

	}

	internal class VerticalBoxFactory : BoxFactory<Gtk.VBox> {
		protected override Gtk.VBox CreateWidget(ConfigProperties properties, IInterfaceNode container) {
			return new Gtk.VBox();
		}

	}
}

