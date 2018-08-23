using System;

namespace Mycena {
	internal class DrawingAreaFactory : WidgetFactory<Gtk.DrawingArea> {
		public DrawingAreaFactory() : base() {
		}
		protected override Gtk.DrawingArea CreateWidget(ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) {
			return new Gtk.DrawingArea();
		}
	}
}

