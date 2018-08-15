using System;

namespace Mycena {
	internal static class CommonWidgetMods {

		public static bool SetVisibility<T>(T widget, ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) where T : Gtk.Widget {
			try {
				bool visible = TextParseTools.ParseBool(properties.GetProperty("visible"));
				widget.Visible = visible;
				return true;
			} catch (FormatException) {
				return false;
			}
		}

		public static bool SetFocusable<T>(T widget, ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) where T : Gtk.Widget {
			try {
				bool focusable = TextParseTools.ParseBool(properties.GetProperty("can_focus"));
				widget.CanFocus = focusable;
				return true;
			} catch (FormatException) {
				return false;
			}
		}
		public static bool SetReceiveDefault<T>(T widget, ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) where T : Gtk.Widget {
			try {
				bool receive = TextParseTools.ParseBool(properties.GetProperty("receives_default"));
				widget.ReceivesDefault = receive;
				return true;
			} catch (FormatException) {
				return false;
			}
		}
		public static bool SetSensitivity<T>(T widget, ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) where T : Gtk.Widget {
			try {
				bool sensitive = TextParseTools.ParseBool(properties.GetProperty("sensitive"));
				widget.Sensitive = sensitive;
				return true;
			} catch (FormatException) {
				return false;
			}
		}
		public static bool SetScrollAdjustment<T>(T widget, ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) where T : Gtk.Widget {
			string hName = properties.GetProperty("hadjustment", "adj_Default");
			string vName = properties.GetProperty("vadjustment", "adj_Default");
			Gtk.Adjustment hAdjustment = AdjustmentFactory.GetAdjustment(hName, container);
			Gtk.Adjustment vAdjustment = AdjustmentFactory.GetAdjustment(vName, container);
			// TODO: set scroll adjustment
			//widget.SetScrollAdjustments(hAdjustment, vAdjustment);
			return true;
		}
		/*public static bool SetActive<T>(T widget, ConfigProperties properties, IInterfaceNode container, IInterfaceStock strock) where T : Gtk.Widget {
			bool active;
			try {
				active = TextParseTools.ParseBool(properties.GetProperty("active", true));
			}


		}*/
	}
}

