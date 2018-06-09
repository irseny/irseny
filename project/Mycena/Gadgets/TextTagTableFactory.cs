using System;

namespace Mycena {
	internal class TextTagTableFactory : GadgetFactory<Gtk.TextTagTable> {
		public TextTagTableFactory() : base() {
		}
		protected override Gtk.TextTagTable CreateGadget(ConfigProperties properties, IInterfaceNode container) {
			return new Gtk.TextTagTable();
		}
	}
}

