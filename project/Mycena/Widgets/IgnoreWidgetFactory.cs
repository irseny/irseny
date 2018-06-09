using System;
using System.Xml;
using System.Collections.Generic;

namespace Mycena {
	internal class IgnoreWidgetFactory : WidgetFactory<Gtk.Widget> {
		public IgnoreWidgetFactory() : base() {
			
		}

		protected override Gtk.Widget CreateWidget(ConfigProperties properties, IInterfaceNode container) {
			return new Gtk.Widget(IntPtr.Zero);
		}
	}
}

