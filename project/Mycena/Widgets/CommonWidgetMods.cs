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
			string hName = properties.GetProperty("hadjustment", String.Empty);
			string vName = properties.GetProperty("vadjustment", String.Empty);
			var hAdjustment = container.GetGadget<Gtk.Adjustment>(hName, null);
			var vAdjustment = container.GetGadget<Gtk.Adjustment>(vName, null);
			if (hAdjustment == null || vAdjustment == null) {
				var defaultAdjustment = container.GetGadget<Gtk.Adjustment>("adj_Default", null);
				if (defaultAdjustment == null) {
					defaultAdjustment = new Gtk.Adjustment(0, 0, 100, 1, 10, 0);
					container.RegisterGadget("adj_Default", defaultAdjustment);
				}
				if (hAdjustment == null) {
					hAdjustment = defaultAdjustment;
				}
				if (vAdjustment == null) {
					vAdjustment = defaultAdjustment;
				}
			}
			widget.SetScrollAdjustments(hAdjustment, vAdjustment);
			return true;
		}
	}
}

