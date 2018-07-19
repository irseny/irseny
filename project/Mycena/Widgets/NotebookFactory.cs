using System;
using System.Collections.Generic;

namespace Mycena {
	internal class NotebookFactory : WidgetFactory<Gtk.Notebook> {
		public NotebookFactory() : base() {
			CreationProperties.Add("homogeneous", SetHomogeneous);
			CreationProperties.Add("homogeneous_tabs", SetHomogeneousTabs);
			CreationProperties.Add("tab_pos", SetTabPosition);
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
		private static bool SetHomogeneous(Gtk.Notebook widget, ConfigProperties properties, IInterfaceNode container) {
			bool homogeneous;
			try {
				homogeneous = TextParseTools.ParseBool(properties.GetProperty("homogeneous", false));
			} catch (FormatException) {
				return false;
			}
			// TODO: set homogeneousity 
			//widget.Homogeneous = homogeneous;
			return true;
		}
		private static bool SetHomogeneousTabs(Gtk.Notebook widget, ConfigProperties properties, IInterfaceNode container) {
			bool homogeneous;
			try {
				homogeneous = TextParseTools.ParseBool(properties.GetProperty("homogeneous_tabs", false));
			} catch (FormatException) {
				return false;
			}
			//widget.HomogeneousTabs = homogeneous;

			return true;
		}
		public static bool SetTabPosition(Gtk.Notebook widget, ConfigProperties properties, IInterfaceNode container) {
			Gtk.PositionType position;
			try {
				position = TextParseTools.ParsePositionType(properties.GetProperty("tab_pos", "top"));
			} catch (FormatException) {
				return false;
			}
			widget.TabPos = position;
			return true;
		}

	}
}

