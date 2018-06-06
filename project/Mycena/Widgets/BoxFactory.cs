using System;
using System.Collections.Generic;

namespace Mycena {
	internal abstract class BoxFactory<T> : WidgetFactory<T> where T : Gtk.Box {
		public BoxFactory() {
			IsContainer = true;
			PackChildren = true;
		}
		protected override bool PackWidget(T container, Gtk.Widget child, IDictionary<string, System.Xml.XmlNode> properties) {
			bool start = true;
			if (CheckRequiredProperties(properties, "position")) {
				try {
					start = TextParseTools.ParseBool(properties["start"].InnerText);
				} catch (FormatException) {
					return false;
				}
			}
			if (start) {
				container.PackStart(child);
			} else {
				container.PackEnd(child);
			}
			return true;
		}
	}

	internal class HorizontalBoxFactory : BoxFactory<Gtk.HBox> {
		protected override Gtk.HBox CreateWidget(System.Collections.Generic.IDictionary<string, System.Xml.XmlNode> properties) {
			return new Gtk.HBox();
		}
		public override string ClassName {
			get { return "GtkHBox"; }
		}
	}

	internal class VerticalBoxFactory : BoxFactory<Gtk.VBox> {
		protected override Gtk.VBox CreateWidget(System.Collections.Generic.IDictionary<string, System.Xml.XmlNode> properties) {
			return new Gtk.VBox();
		}
		public override string ClassName {
			get { return "GtkVBox"; }
		}
	}
}

