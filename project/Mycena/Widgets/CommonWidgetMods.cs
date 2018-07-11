using System;

namespace Mycena {
	internal static partial class WidgetFactory {

		public static bool SetVisibility<T>(T widget, ConfigProperties properties, IInterfaceNode container) where T : Gtk.Widget {
			try {
				bool visible = TextParseTools.ParseBool(properties.GetProperty("visible"));
				widget.Visible = visible;
				return true;
			} catch (FormatException) {
				return false;
			}
		}

		public static bool SetFocusable<T>(T widget, ConfigProperties properties, IInterfaceNode container) where T : Gtk.Widget {
			try {
				bool focusable = TextParseTools.ParseBool(properties.GetProperty("can_focus"));
				widget.CanFocus = focusable;
				return true;
			} catch (FormatException) {
				return false;
			}
		}
		public static bool SetReceiveDefault<T>(T widget, ConfigProperties properties, IInterfaceNode container) where T : Gtk.Widget {
			try {
				bool receive = TextParseTools.ParseBool(properties.GetProperty("receives_default"));
				widget.ReceivesDefault = receive;
				return true;
			} catch (FormatException) {
				return false;
			}
		}
		public static bool SetSensitivity<T>(T widget, ConfigProperties properties, IInterfaceNode container) where T : Gtk.Widget {
			try {
				bool sensitive = TextParseTools.ParseBool(properties.GetProperty("sensitive"));
				widget.Sensitive = sensitive;
				return true;
			} catch (FormatException) {
				return false;
			}
		}
		public static bool SetScrollAdjustment<T>(T widget, ConfigProperties properties, IInterfaceNode container) where T : Gtk.Widget {
			string hName = properties.GetProperty("hadjustment", "adj_Default");
			string vName = properties.GetProperty("vadjustment", "adj_Default");
			Gtk.Adjustment hAdjustment = AdjustmentFactory.GetAdjustment(hName, container);
			Gtk.Adjustment vAdjustment = AdjustmentFactory.GetAdjustment(vName, container);
			widget.SetScrollAdjustments(hAdjustment, vAdjustment);
			return true;
		}
	}
}

