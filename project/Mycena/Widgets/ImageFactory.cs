using System;
using System.Collections.Generic;

namespace Mycena {
	internal class ImageFactory : WidgetFactory<Gtk.Image> {
		const string StockPropertyName = "stock";
		const string MissingImageText = "gtk-missing-image";

		public ImageFactory() : base() {
			CreationProperties.Add("stock");
		}
		public override string ClassName {
			get { return "GtkImage"; }
		}
		protected override Gtk.Image CreateWidget(IDictionary<string, string> properties) {
			string stock;
			if (properties.TryGetValue(StockPropertyName, out stock)) {
				if (stock.Equals(MissingImageText)) {
					return new Gtk.Image(stock, Gtk.IconSize.Invalid);
				} else {
					return new Gtk.Image(stock, Gtk.IconSize.Button);
				}
			}
			return new Gtk.Image();
		}
	}
}

