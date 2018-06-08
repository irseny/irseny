using System;
using System.Xml;
using System.Collections.Generic;

namespace Mycena {
	internal class ImageFactory : WidgetFactory<Gtk.Image> {
		const string StockPropertyName = "stock";
		const string MissingImageText = "gtk-missing-image";

		public ImageFactory() : base() {
			
		}
		protected override Gtk.Image CreateWidget(ConfigProperties properties, IInterfaceNode container) {
			string stock = properties.GetProperty(StockPropertyName, null);
			Gtk.Image result;
			if (stock == null) {
				result = new Gtk.Image();
			} else if (stock.Equals(MissingImageText)) {
				result = new Gtk.Image(stock, Gtk.IconSize.Invalid);
			} else {
				result = new Gtk.Image(stock, Gtk.IconSize.Button);
			}
			ApplyProperties(result, properties, container);
			return result;
		}
	}
}

