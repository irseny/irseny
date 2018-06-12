using System;
using System.Xml;
using System.Collections.Generic;

namespace Mycena {
	internal class TextViewFactory : WidgetFactory<Gtk.TextView> {
		public TextViewFactory() : base() {
		}

		protected override Gtk.TextView CreateWidget(ConfigProperties properties, IInterfaceNode container) {


			Gtk.TextBuffer buffer = TextBufferFactory.GetBuffer(properties, container);
			return new Gtk.TextView(buffer);
		}
	}
}

