using System;

namespace Mycena {
	internal class TextBufferFactory : GadgetFactory<Gtk.TextBuffer> {
		public TextBufferFactory() : base() {
			CreationProperties.Add("text", SetText);
		}
		protected override Gtk.TextBuffer CreateGadget(ConfigProperties properties, IInterfaceNode container) {
			Gtk.TextTagTable table;
			string tableName;
			if (properties.TryGetProperty("tag_table", out tableName)) {
				if (!container.TryGetGadget(tableName, out table)) {
					return null;
				}
			} else {
				table = new Gtk.TextTagTable();
				container.AddGadget(table);
			}
			string text = properties.GetProperty("text", null);
			var result = new Gtk.TextBuffer(table);
			ApplyProperties(result, properties, container);
			return result;
		}
		private static bool SetText(Gtk.TextBuffer gadget, ConfigProperties properties, IInterfaceNode container) {
			string text = properties.GetProperty("text");
			gadget.Text = text;
			return true;
		}
	}
}

