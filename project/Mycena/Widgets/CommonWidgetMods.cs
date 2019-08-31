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
		//public static bool SetScrollAdjustment<T>(T widget, ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) where T : Gtk.Widget {
		//	string hName = properties.GetProperty("hadjustment", "adj_Default");
		//	string vName = properties.GetProperty("vadjustment", "adj_Default");
		//	Gtk.Adjustment hAdjustment = AdjustmentFactory.GetAdjustment(hName, container);
		//	Gtk.Adjustment vAdjustment = AdjustmentFactory.GetAdjustment(vName, container);
		//	// TODO: set scroll adjustment
		//	//widget.SetScrollAdjustments(hAdjustment, vAdjustment);
		//	widget.
		//	return true;
		//}
		public static bool SetAlignment<T>(T widget, ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock)
			where T : Gtk.Widget {
			try {
				Gtk.Align hAlign = TextParseTools.ParseAlignment(properties.GetProperty("halign", Gtk.Align.Fill));
				Gtk.Align vAlign = TextParseTools.ParseAlignment(properties.GetProperty("valign", Gtk.Align.Fill));
				widget.Halign = hAlign;
				widget.Valign = vAlign;
				return true;
			} catch (FormatException) {
				return false;
			}
		}
		public static bool SetExpansion<T>(T widget, ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock)
			where T : Gtk.Widget {
			try {
				bool hExpand = TextParseTools.ParseBool(properties.GetProperty("hexpand", false));
				bool vExpand = TextParseTools.ParseBool(properties.GetProperty("vexpand", false));
				widget.Hexpand = hExpand;
				widget.Vexpand = vExpand;
				return true;
			} catch (FormatException) {
				return false;
			}
		}
		public static bool SetMargins<T>(T widget, ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock)
			where T : Gtk.Widget {
			try {
				int overall = TextParseTools.ParseInt(properties.GetProperty("margin", -1));
				int top = TextParseTools.ParseInt(properties.GetProperty("margin_top", -1));
				int bottom = TextParseTools.ParseInt(properties.GetProperty("margin_bottom", -1));
				int left = TextParseTools.ParseInt(properties.GetProperty("margin_left", -1));
				int right = TextParseTools.ParseInt(properties.GetProperty("margin_right", -1));
				int start = TextParseTools.ParseInt(properties.GetProperty("margin_start", -1));
				int end = TextParseTools.ParseInt(properties.GetProperty("margin_end", -1));
				if (overall > -1) {
					widget.Margin = overall;
				}
				if (top > -1) {
					widget.MarginTop = top;
				}
				if (bottom > -1) {
					widget.MarginBottom = bottom;
				}
				if (right > -1) {
					widget.MarginEnd = right;
				}
				if (left > -1) {
					widget.MarginStart = left;
				}
				if (start > -1) {
					widget.MarginStart = start;
				}
				if (end > -1) {
					widget.MarginEnd = end;
				}
				return true;
			} catch (FormatException) {
				return false;
			}
		}
		public static bool Noop<T>(T widget, ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) {
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

