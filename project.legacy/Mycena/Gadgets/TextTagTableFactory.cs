using System;

namespace Mycena {
	internal class TextTagTableFactory : GadgetFactory<Gtk.TextTagTable> {
		public TextTagTableFactory() : base() {
		}
		protected override Gtk.TextTagTable CreateGadget(ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) {
			return new Gtk.TextTagTable();
		}
		public static Gtk.TextTagTable GetTable(ConfigProperties properties, IInterfaceNode container) {
			Gtk.TextTagTable result;
			string tableName;
			if (properties.TryGetProperty("tag_table", out tableName)) {
				if (!container.TryGetGadget(tableName, out result)) {
					result = new Gtk.TextTagTable();
					container.RegisterGadget(tableName, result);
				}
			} else {
				result = new Gtk.TextTagTable();
				container.AddGadget(result);
			}
			return result;

		}
	}
}

