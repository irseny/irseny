using System;
using System.Collections.Generic;

namespace Mycena {
	internal class ExpanderFactory : WidgetFactory<Gtk.Expander> {
		public ExpanderFactory() : base() {
		}
		protected override Gtk.Expander CreateWidget(ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) {
			return new Gtk.Expander(string.Empty);
		}
		protected override bool PackWidgets(Gtk.Expander container, IList<Tuple<Gtk.Widget, ConfigProperties>> children, IInterfaceStock stock) {
			foreach (var pair in children) {
				string widgetType = pair.Item2.GetAttribute("child_type", string.Empty);
				if (widgetType.Equals("label")) {
					container.LabelWidget = pair.Item1;
				} else {
					container.Child = pair.Item1;
				}
			}
			return true;
		}
	}
}
