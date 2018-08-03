using System;
using System.Collections.Generic;

namespace Mycena {
	internal class ComboBoxFactory {
		
		public static bool SetActive<T>(T widget, ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) where T : Gtk.ComboBox {
			int active;
			try {
				active = TextParseTools.ParseInt(properties.GetProperty("active", -1));
			} catch (FormatException) {
				return false;
			}
			widget.Active = active;
			return true;
		}
		public static bool SetActiveId<T>(T widget, ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) where T : Gtk.ComboBox {
			string active;
			try {
				active = properties.GetProperty("active_id", null);
			} catch (FormatException) {
				return false;
			}
			if (active != null) {
				widget.ActiveId = active;
			}
			return true;
		}
		public static bool SetTearoffEnabled<T>(T widget, ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) where T : Gtk.ComboBox {
			bool tearoffsEnabled;
			try {
				tearoffsEnabled = TextParseTools.ParseBool(properties.GetProperty("add_tearoffs", -1));
			} catch (FormatException) {
				return false;
			}
			widget.AddTearoffs = tearoffsEnabled;
			return true;
		}
		public static bool SetTearoffTitle<T>(T widget, ConfigProperties properties, IInterfaceNode container, IInterfaceStock stock) where T : Gtk.ComboBox {
			string title;
			try {
				title = properties.GetProperty("tearoff_title", null);
			} catch (FormatException) {
				return false;
			}
			if (title != null) {
				widget.TearoffTitle = title;
			}
			return true;
		}
	}
}

