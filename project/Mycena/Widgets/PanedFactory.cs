using System;
using System.Xml;
using System.Collections.Generic;

namespace Mycena {
	internal abstract class PanedFactory<T> : WidgetFactory<T> where T : Gtk.Paned {
		public PanedFactory() {
			IsContainer = true;
			PackChildren = true;
			ModProperties.Add("position", SetPosition);
			ModProperties.Add("position_set", ApplyPositionSet);
			PackProperties.Add("resize");
			PackProperties.Add("shrink");

		}
		protected override bool PackWidget(T container, Gtk.Widget child, IDictionary<string, XmlNode> properties) {
			try {
				bool resize = TextParseTools.ParseBool(properties["resize"].InnerText);
				bool shrink = TextParseTools.ParseBool(properties["shrink"].InnerText);
				if (container.Child1 == null) {
					container.Pack1(child, resize, shrink);
					return true;
				} else if (container.Child2 == null) {
					container.Pack2(child, resize, shrink);
					return true;
				} else {
					return false;
				}
			} catch (FormatException) {
				return false;
			}

		}
		private static bool SetPosition(Gtk.Paned widget, XmlNode property) {
			try {
				int position = TextParseTools.ParseInt(property.InnerText);
				widget.Position = position;
				return true;
			} catch (FormatException) {
				return false;
			}
		}
		private static bool ApplyPositionSet(Gtk.Paned widget, XmlNode property) {
			try {
				bool enabled = TextParseTools.ParseBool(property.InnerText);
				widget.PositionSet = enabled;
				return true;
			} catch (FormatException) {
				return false;
			}
		}
	}

	internal class HorizontalPanedFactory : PanedFactory<Gtk.HPaned> {
		
		public override string ClassName {
			get { return "GtkHPaned"; }
		}
		protected override Gtk.HPaned CreateWidget(System.Collections.Generic.IDictionary<string, XmlNode> properties) {
			return new Gtk.HPaned();
		}



	}

	internal class VerticalPanedFactory : PanedFactory<Gtk.VPaned> {
		public override string ClassName {
			get { return "GtkVPaned"; }
		}
		protected override Gtk.VPaned CreateWidget(IDictionary<string, XmlNode> properties) {
			return new Gtk.VPaned();
		}
	}
}

