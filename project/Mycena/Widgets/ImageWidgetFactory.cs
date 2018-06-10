using System;
using System.Xml;
using System.Collections.Generic;

namespace Mycena {
	internal class ImageWidgetFactory : WidgetFactory<Gtk.Image> {

		public ImageWidgetFactory() : base() {
			CreationProperties.Add("stock", SetStock);
			CreationProperties.Add("icon_name", SetIcon);
		}
		protected override Gtk.Image CreateWidget(ConfigProperties properties, IInterfaceNode container) {
			Gtk.Image result = new Gtk.Image();
			ApplyProperties(result, properties, container);
			return result;
		}

		public static bool SetStock(Gtk.Image widget, ConfigProperties properties, IInterfaceNode container) {
			string stock = properties.GetProperty("stock", "gtk-missing-icon");
			Gtk.IconSize size;
			try {
				size = TextParseTools.ParseIconSize(properties.GetProperty("icon-size", Gtk.IconSize.Button));
			} catch (FormatException) {
				return false;
			}
			widget.SetFromStock(stock, size);
			return true;
		}
		public static bool SetIcon(Gtk.Image widget, ConfigProperties properties, IInterfaceNode container) {
			string icon;
			if (!properties.TryGetProperty("icon_name", out icon)) {
				return false;
			}
			Gtk.IconSize size;
			try {
				size = TextParseTools.ParseIconSize(properties.GetProperty("icon-size", Gtk.IconSize.Button));
			} catch (FormatException) {
				return false;
			}
			widget.SetFromIconName(icon, size);
			return true;
		}
	}
}

