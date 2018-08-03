using System;
using System.Xml;
using System.Collections.Generic;

namespace Mycena {
	internal class PanedFactory : WidgetFactory<Gtk.Paned> {
		public PanedFactory() {
			CreationProperties.Add("position", SetPosition);
			CreationProperties.Add("position_set", ApplyPositionSet);
			CreationProperties.Add("wide_handle", SetHandleWidth);
		}
		protected override Gtk.Paned CreateWidget(ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) {
			Gtk.Orientation orientation;
			try {
				orientation = TextParseTools.ParseOrientation(properties.GetProperty("orientation", Gtk.Orientation.Horizontal));
			} catch (FormatException) {
				return null;
			}
			return new Gtk.Paned(orientation);
		}
		protected override bool PackWidget(Gtk.Paned container, Gtk.Widget child, ConfigProperties properties, IInterfaceStock stock) {
			bool shrink, resize;
			try {
				resize = TextParseTools.ParseBool(properties.GetProperty("resize", false));
				shrink = TextParseTools.ParseBool(properties.GetProperty("shrink", false));
			} catch (FormatException) {
				return false;
			}
			if (container.Child1 == null) {
				container.Pack1(child, resize, shrink);
				return true;
			} else if (container.Child2 == null) {
				container.Pack2(child, resize, shrink);
				return true;
			} else {
				return false;
			}

		}
		private static bool SetPosition(Gtk.Paned widget, ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) {
			int position;
			try {
				position = TextParseTools.ParseInt(properties.GetProperty("position"));
			} catch (FormatException) {
				return false;
			}
			widget.Position = position;
			return true;
		}
		private static bool ApplyPositionSet(Gtk.Paned widget, ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) {
			bool enabled;
			try {
				enabled = TextParseTools.ParseBool(properties.GetProperty("position_set"));

			} catch (FormatException) {
				return false;
			}
			widget.PositionSet = enabled;
			return true;
		}
		private static bool SetHandleWidth(Gtk.Paned widget, ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) {
			bool wide;
			try {
				bool enabled = TextParseTools.ParseBool(properties.GetProperty("wide_handle", false));
			} catch (FormatException) {
				return false;
			}
			// TODO: set handle width
			return true;
		}
	}


}

