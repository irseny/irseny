using System;
using System.Xml;
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
		protected override Gtk.Image CreateWidget(IDictionary<string, XmlNode> properties) {
			XmlNode stock;
			if (properties.TryGetValue(StockPropertyName, out stock)) {
				if (stock.InnerText.Equals(MissingImageText)) {
					return new Gtk.Image(stock.InnerText, Gtk.IconSize.Invalid);
				} else {
					return new Gtk.Image(stock.InnerText, Gtk.IconSize.Button);
				}
			}
			return new Gtk.Image();
		}
		protected override void PackWidget(Gtk.Image container, Gtk.Widget child, IDictionary<string, XmlNode> properties) {
			throw new NotSupportedException(ClassName + " can not pack widgets");
		}
	}
}

