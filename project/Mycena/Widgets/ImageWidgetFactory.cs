using System;
using System.Xml;
using System.Collections.Generic;

namespace Mycena {
	internal class ImageWidgetFactory : WidgetFactory<Gtk.Image> {

		public ImageWidgetFactory() : base() {
			/*CreationProperties.Add("stock", SetStock);
			CreationProperties.Add("icon_name", SetIcon);*/
		}
		protected override Gtk.Image CreateWidget(ConfigProperties properties, IInterfaceNode container) {
			return CreateImage(properties, container);
		}
		public static Gtk.Image CreateImage(ConfigProperties properties, IInterfaceNode container) {
			string stock = properties.GetProperty("stock", null);
			string icon = properties.GetProperty("icon_name", null);
			Gtk.IconSize size;
			try {
				size = TextParseTools.ParseIconSize(properties.GetProperty("icon-size", (int)Gtk.IconSize.Button));
			} catch (FormatException) {
				return null;
			}
			if (stock != null) {
				return new Gtk.Image(stock, size);
			} else if (icon != null) {
				return new Gtk.Image(icon, size);
			} else {
				return new Gtk.Image();
			}

		}

		/*public static bool SetStock(Gtk.Image widget, ConfigProperties properties, IInterfaceNode container) {
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
		}*/
	}
}

