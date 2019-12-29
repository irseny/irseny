using System;

namespace Mycena {
	//internal class HorizontalSeparatorFactory : WidgetFactory<Gtk.HSeparator> {
	//	public HorizontalSeparatorFactory() : base() {
	//	}
	//	protected override Gtk.HSeparator CreateWidget(ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) {
	//		return new Gtk.HSeparator();
	//	}
	//}
	//internal class VerticalSeparatorFactory : WidgetFactory<Gtk.VSeparator> {
	//	public VerticalSeparatorFactory() : base() {

	//	}
	//	protected override Gtk.VSeparator CreateWidget(ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) {
	//		return new Gtk.VSeparator();
	//		return new Gtk.Separator(Gtk.Orientation.)
	//	}
	//}
	internal class SeparatorFactory : WidgetFactory<Gtk.Separator> {
		protected override Gtk.Separator CreateWidget(ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) {
			var orientation = Gtk.Orientation.Horizontal;
			string sOrientation;
			try {
				if (properties.TryGetProperty("orientation", out sOrientation)) {
					orientation = TextParseTools.ParseOrientation(sOrientation);
				}
			} catch (FormatException) {
				throw new ArgumentException("Unable to parse orientation");
			}
			return new Gtk.Separator(orientation);
		}
	}

}
