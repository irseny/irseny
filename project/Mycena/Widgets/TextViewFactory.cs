using System;
using System.Xml;
using System.Collections.Generic;

namespace Mycena {
	internal class TextViewFactory : WidgetFactory<Gtk.TextView> {
		public TextViewFactory() : base() {
		}

		protected override Gtk.TextView CreateWidget(ConfigProperties properties, IInterfaceNode container) {
			var result = new Gtk.TextView();
			ApplyProperties(result, properties, container);
			return result;
		}
	}
}

