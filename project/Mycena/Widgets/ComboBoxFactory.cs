using System;
using System.Collections.Generic;

namespace Mycena {
	internal class ComboBoxFactory : WidgetFactory<Gtk.ComboBox> {
		public ComboBoxFactory() : base() {
		}
		protected override Gtk.ComboBox CreateWidget(ConfigProperties properties, IInterfaceNode container) {
			IList<string> sourceEntries = properties.GetItems();
			string[] entries = new string[sourceEntries.Count];
			sourceEntries.CopyTo(entries, 0);
			var result = new Gtk.ComboBox(entries);
			return result;
		}
	}
}

