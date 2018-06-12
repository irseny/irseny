using System;

namespace Mycena {
	internal class TextBufferFactory : GadgetFactory<Gtk.TextBuffer> {
		public TextBufferFactory() : base() {
			CreationProperties.Add("text", SetText);
		}
		protected override Gtk.TextBuffer CreateGadget(ConfigProperties properties, IInterfaceNode container) {
			Gtk.TextTagTable table = TextTagTableFactory.GetTable(properties, container);
			return new Gtk.TextBuffer(table);
		}
		private static bool SetText(Gtk.TextBuffer gadget, ConfigProperties properties, IInterfaceNode container) {
			gadget.Text = properties.GetProperty("text");
			return true;
		}
		public static Gtk.TextBuffer GetBuffer(ConfigProperties properties, IInterfaceNode container) {
			Gtk.TextBuffer result;
			string bufferName;
			if (properties.TryGetProperty("text_buffer", out bufferName)) {
				if (!container.TryGetGadget(bufferName, out result)) {
					result = new Gtk.TextBuffer(TextTagTableFactory.GetTable(properties, container));
					container.RegisterGadget(bufferName, result);
				}
			} else {
				result = new Gtk.TextBuffer(TextTagTableFactory.GetTable(properties, container));
				container.AddGadget(result);
			}
			return result;
		}
	}
}

