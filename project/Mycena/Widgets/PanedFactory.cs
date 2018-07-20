using System;
using System.Xml;
using System.Collections.Generic;

namespace Mycena {
	internal abstract class PanedFactory : WidgetFactory<Gtk.Paned> {
		public PanedFactory() {
			CreationProperties.Add("position", SetPosition);
			CreationProperties.Add("position_set", ApplyPositionSet);
			PackProperties.Add("resize");
			PackProperties.Add("shrink");

		}
		protected override bool PackWidget(Gtk.Paned container, Gtk.Widget child, ConfigProperties properties, IInterfaceStock stock) {
			try {
				bool resize = TextParseTools.ParseBool(properties.GetProperty("resize", "false"));
				bool shrink = TextParseTools.ParseBool(properties.GetProperty("shrink", "false"));
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
		private static bool SetPosition(Gtk.Paned widget, ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) {
			try {
				int position = TextParseTools.ParseInt(properties.GetProperty("position"));
				widget.Position = position;
				return true;
			} catch (FormatException) {
				return false;
			}
		}
		private static bool ApplyPositionSet(Gtk.Paned widget, ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) {
			try {
				bool enabled = TextParseTools.ParseBool(properties.GetProperty("position_set"));
				widget.PositionSet = enabled;
				return true;
			} catch (FormatException) {
				return false;
			}
		}
	}

	internal class HorizontalPanedFactory : PanedFactory {


		protected override Gtk.Paned CreateWidget(ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) {
			return new Gtk.HPaned();

		}



	}

	internal class VerticalPanedFactory : PanedFactory {
		protected override Gtk.Paned CreateWidget(ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) {
			return new Gtk.VPaned();
		}
	}
}

