﻿using System;
using System.Xml;
using System.Collections.Generic;

namespace Mycena {
	internal class WindowFactory : WidgetFactory<Gtk.Window> {
		public WindowFactory() : base() {
			IsContainer = true;
		}
		public override string ClassName {
			get { return "GtkWindow"; }
		}
		protected override Gtk.Window CreateWidget(IDictionary<string, XmlNode> properties) {
			return new Gtk.Window(Gtk.WindowType.Toplevel);
		}
		protected override void PackWidget(Gtk.Window container, Gtk.Widget child, IDictionary<string, XmlNode> properties) {
			container.Add(child);
		}
	}
}

