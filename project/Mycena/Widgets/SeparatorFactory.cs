using System;

namespace Mycena {
	internal class HorizontalSeparatorFactory : WidgetFactory<Gtk.HSeparator> {
		public HorizontalSeparatorFactory() : base() {
		}
		protected override Gtk.HSeparator CreateWidget(ConfigProperties properties, IInterfaceNode container) {
			return new Gtk.HSeparator();
		}
	}
	internal class VerticalSeparatorFactory : WidgetFactory<Gtk.VSeparator> {
		public VerticalSeparatorFactory() : base() {

		}
		protected override Gtk.VSeparator CreateWidget(ConfigProperties properties, IInterfaceNode container) {
			return new Gtk.VSeparator();
		}
	}
}
