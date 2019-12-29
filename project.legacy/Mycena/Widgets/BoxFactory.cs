using System;
using System.Collections.Generic;

namespace Mycena {
	internal class BoxFactory : WidgetFactory<Gtk.Box> {
		public BoxFactory() : base() {

		}
		protected override Gtk.Box CreateWidget(ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) {
			Gtk.Orientation orientation;
			int spacing;
			try {
				orientation = TextParseTools.ParseOrientation(properties.GetProperty("orientation", Gtk.Orientation.Horizontal));
				spacing = TextParseTools.ParseInt(properties.GetProperty("spacing", 0));
			} catch (FormatException) {
				return null;
			}
			return new Gtk.Box(orientation, spacing);
		}
		protected override bool PackWidget(Gtk.Box container, Gtk.Widget child, ConfigProperties properties, IInterfaceStock stock) {
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

	internal class HorizontalBoxFactory : BoxFactory {
		protected override Gtk.Box CreateWidget(ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) {
			return new Gtk.HBox();
		}

	}

	internal class VerticalBoxFactory : BoxFactory {
		protected override Gtk.Box CreateWidget(ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) {
			return new Gtk.VBox();
		}

	}
}

