using System;

namespace Mycena {
	internal class GridFactory : WidgetFactory<Gtk.Grid> {
		public GridFactory() : base() {
		}
		protected override Gtk.Grid CreateWidget(ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) {
			return new Gtk.Grid();
		}
		protected override bool PackWidget(Gtk.Grid container, Gtk.Widget child, ConfigProperties properties, IInterfaceStock stock) {
			int left, top, width, height;
			try {
				left = TextParseTools.ParseInt(properties.GetAttribute("left_attach", 0));
				top = TextParseTools.ParseInt(properties.GetAttribute("top_attach", 0));
				width = TextParseTools.ParseInt(properties.GetAttribute("width", 1));
				height = TextParseTools.ParseInt(properties.GetAttribute("height", 1));
			} catch (FormatException) {
				return false;
			}
			container.Attach(child, left, top, width, height);
			return true;
		}
	}
}
