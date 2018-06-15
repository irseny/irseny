using System;
using System.Collections.Generic;

namespace Mycena {
	internal class NotebookFactory : WidgetFactory<Gtk.Notebook> {
		public NotebookFactory() : base() {
		}
		protected override Gtk.Notebook CreateWidget(ConfigProperties properties, IInterfaceNode container) {
			return new Gtk.Notebook();
		}
		protected override bool PackWidgets(Gtk.Notebook container, IList<Tuple<Gtk.Widget, ConfigProperties>> widgets) {
			for (int i = 1; i < widgets.Count; i += 2) {
				Gtk.Widget tab = null;
				Gtk.Widget content = null;
				// tab and content widgets are expected in pairs
				string[] types = new string[] {
					widgets[i].Item2.GetAttribute("child_type", "content"),
					widgets[i - 1].Item2.GetAttribute("child_type", "content")
				};
				for (int w = 0; w < types.Length; w++) {
					if (types[w].Equals("tab")) {
						tab = widgets[i - w].Item1; // subtracting 0 or 1 as for types creation
					} else if (types[w].Equals("content")) {
						content = widgets[i - w].Item1; 
					} else {
						return false;
					}
				}
				/*if (content != null) {					
					container.AppendPage(content, tab); // tab may be null
				} else {
					return false;
				}*/
				container.AppendPage(content, tab);
			}
			return true;
		}
	}
}

