using System;
using System.Collections.Generic;

namespace Mycena {
	internal class WindowFactory : WidgetFactory<Gtk.Window> {
		public WindowFactory() : base() {
			IsContainer = true;
		}
		public override string ClassName {
			get { return "GtkWindow"; }
		}
		protected override Gtk.Window CreateWidget(IDictionary<string, string> properties) {
			return new Gtk.Window(Gtk.WindowType.Toplevel);
		}
		protected override void PackWidget(Gtk.Window container, Gtk.Widget child, IDictionary<string, string> properties) {
			container.Add(child);
		}
	}
}

