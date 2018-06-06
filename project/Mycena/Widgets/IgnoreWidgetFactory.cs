using System;
using System.Xml;
using System.Collections.Generic;

namespace Mycena {
	internal class IgnoreWidgetFactory : WidgetFactory<Gtk.Widget> {
		public IgnoreWidgetFactory() : base() {
			PackChildren = false;
			AllowRegister = false;
		}
		public override string ClassName {
			get { return null; }
		}
		protected override Gtk.Widget CreateWidget(IDictionary<string, XmlNode> properties) {
			return null;
		}
	}
}

