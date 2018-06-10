using System;
using System.Collections.Generic;

namespace Mycena {
	internal abstract class BoxFactory<T> : WidgetFactory<T> where T : Gtk.Box {
		public BoxFactory() {
			
		}
		protected override bool PackWidget(T container, Gtk.Widget child, ConfigProperties properties) {
			Gtk.PackType packType;
			bool expand;
			bool fill;
			uint padding;
			try {
				packType = TextParseTools.ParsePackType(properties.GetProperty("pack_type", Gtk.PackType.Start));
				expand = TextParseTools.ParseBool(properties.GetProperty("expand", false));
				fill = TextParseTools.ParseBool(properties.GetProperty("fill", false));
				padding = TextParseTools.ParseUInt(properties.GetProperty("padding", 0));
			} catch (FormatException) {
				return false;
			}
			switch (packType) {
			case Gtk.PackType.Start:
				container.PackStart(child, expand, fill, padding);
				return true;
			case Gtk.PackType.End:
				container.PackEnd(child, expand, fill, padding);
				return true;
			default:
				return false;
			}
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

