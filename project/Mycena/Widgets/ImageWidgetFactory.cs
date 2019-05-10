using System;
using System.Xml;
using System.Collections.Generic;

namespace Mycena {
	internal class ImageWidgetFactory : WidgetFactory<Gtk.Image> {

		public ImageWidgetFactory() : base() {
			/*CreationProperties.Add("stock", SetStock);
			CreationProperties.Add("icon_name", SetIcon);*/
		}
		protected override Gtk.Image CreateWidget(ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) {
			return CreateImage(properties, container, stock);
		}
		public static Gtk.Image CreateImage(ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) {
			string stockName = properties.GetProperty("stock", null);
			string iconName = properties.GetProperty("icon_name", null);
			string resourceName = properties.GetProperty("resource", null);
			Gtk.IconSize size;
			string pixbufName;
			try {
				size = TextParseTools.ParseIconSize(properties.GetProperty("icon-size", (int)Gtk.IconSize.Button));
				if (properties.TryGetProperty("pixbuf", out pixbufName)) {
					pixbufName = TextParseTools.ParsePath(pixbufName);
				}
			} catch (FormatException) {
				throw new ArgumentException("Failed to parse image properties");
			}
			if (stockName != null) {
				return new Gtk.Image(stockName, size);
			} else if (iconName != null) {
				return new Gtk.Image(iconName, size);
			} else if (resourceName != null) {
				Gdk.Pixbuf pixbuf = stock.GetPixbuf(resourceName, null);
				if (pixbuf != null) {
					return new Gtk.Image(pixbuf);
				} else {
					throw new ArgumentException("Resource " + resourceName + " does not exist");
				}
			} else if (pixbufName != null) {
				Gdk.Pixbuf pixbuf = stock.GetPixbuf(pixbufName, null);
				if (pixbuf != null) {
					return new Gtk.Image(pixbuf);
				} else {
					throw new ArgumentException("Pixbuf " + pixbufName + " does not exist");
				}
			} else {
				return new Gtk.Image(); // emtpy image
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

