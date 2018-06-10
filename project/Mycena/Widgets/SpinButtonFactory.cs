using System;
using System.Collections.Generic;

namespace Mycena {
	internal class SpinButtonFactory : WidgetFactory<Gtk.SpinButton> {
		public SpinButtonFactory() : base() {
			CreationProperties.Add("max_length", EntryFactory.SetMaxLength);
			CreationProperties.Add("invisible_char", EntryFactory.SetInvisibleChar);
			CreationProperties.Add("width_chars", EntryFactory.SetWidthChars);
			CreationProperties.Add("adjustment", SetAdjustment);
			CreationProperties.Add("numeric", SetNumeric);
		}
		protected override Gtk.SpinButton CreateWidget(ConfigProperties properties, IInterfaceNode container) {
			return new Gtk.SpinButton(0, 10, 1);
		}
		private static bool SetNumeric(Gtk.SpinButton widget, ConfigProperties properties, IInterfaceNode container) {
			bool numeric;
			try {
				numeric = TextParseTools.ParseBool(properties.GetProperty("numeric"));
			} catch (FormatException) {
				return false;
			}
			widget.Numeric = numeric;

			return true;
		}
		private static bool SetAdjustment(Gtk.SpinButton widget, ConfigProperties properties, IInterfaceNode container) {
			Gtk.Adjustment adjustment = container.GetGadget<Gtk.Adjustment>(properties.GetProperty("adjustment"), null);
			if (adjustment == null) {
				return false;
			} else {
				widget.Adjustment = adjustment;
				return true;
			}
		}


	}
}
