using System;

namespace Mycena {
	internal static class CommonWidgetModification {
		
		public static bool SetVisibility(Gtk.Widget widget, ConfigProperties properties, IInterfaceNode container) {
			try {
				bool visible = TextParseTools.ParseBool(properties.GetProperty("visible"));
				widget.Visible = visible;
				return true;
			} catch (FormatException) {
				return false;
			}
		}

		public static bool SetFocusable(Gtk.Widget widget, ConfigProperties properties, IInterfaceNode container) {
			try {
				bool focusable = TextParseTools.ParseBool(properties.GetProperty("can_focus"));
				widget.CanFocus = focusable;
				return true;
			} catch (FormatException) {
				return false;
			}
		}
		public static bool SetReceiveDefault(Gtk.Widget widget, ConfigProperties properties, IInterfaceNode container) {
			try {
				bool receive = TextParseTools.ParseBool(properties.GetProperty("receives_default"));
				widget.ReceivesDefault = receive;
				return true;
			} catch (FormatException) {
				return false;
			}
		}
		public static bool SetSensitivity(Gtk.Widget widget, ConfigProperties properties, IInterfaceNode container) {
			try {
				bool sensitive = TextParseTools.ParseBool(properties.GetProperty("sensitive"));
				widget.Sensitive = sensitive;
				return true;
			} catch (FormatException) {
				return false;
			}
		}
	}
}

