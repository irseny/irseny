using System;
using System.Collections.Generic;

namespace Mycena {
	internal class IgnoreWidgetFactory : WidgetFactory<Gtk.Widget> {
		public IgnoreWidgetFactory() : base() {
			AllowChildren = false;
			AllowRegister = false;
		}
		public override string ClassName {
			get { return null; }
		}
		protected override Gtk.Widget CreateWidget(IDictionary<string, string> properties) {
			return null;
		}
	}
}

