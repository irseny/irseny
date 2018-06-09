using System;
using System.Xml;
using System.Collections.Generic;

namespace Mycena {
	internal class ImageWidgetFactory : WidgetFactory<Gtk.Image> {

		public ImageWidgetFactory() : base() {
			
		}
		protected override Gtk.Image CreateWidget(ConfigProperties properties, IInterfaceNode container) {
			Gtk.Image result = CreateImage(properties);
			if (result != null) {
				ApplyProperties(result, properties, container);
			}
			return result;
		}
		public static Gtk.Image CreateImage(ConfigProperties properties) {
			try {
				string stock = properties.GetProperty("stock", null);
				Gtk.IconSize size = TextParseTools.ParseIconSize(properties.GetProperty("icon-size", 4));
				Gtk.Image result;
				if (stock == null) {
					result = new Gtk.Image();			
				} else {
					result = new Gtk.Image(stock, size);
				}
				return result;
			} catch (FormatException) {
				return null;
			}
		}
	}
}

